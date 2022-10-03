// Program: LE_B515_SDSO_ENTITY_PURGE, ID: 374361871, model: 746.
// Short name: SWEL515B
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
/// A program: LE_B515_SDSO_ENTITY_PURGE.
/// </para>
/// <para>
/// This batch procedure will be run on an as-needed basis.  Its purpose is to 
/// eliminate the following two entities created during RUN 3 (creation of SDSO
/// records):
/// admin_action_cert_obligation
/// adm_act_cert_debt_detail
/// This is being performed to due DB2 build-up of these entities.
/// IMPORTANT PROCESSING NOTE:
/// For the associated date on program_processing_information, CRITICAL 
/// ATTENTION will be needed to change as needed.  Every night in the cycle, a
/// program updates ALL program dates to the current date.  Unless that date is
/// moved to sometime in the past, an unwanted deletion will occur causing major
/// undesired downstream effects.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB515SdsoEntityPurge: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B515_SDSO_ENTITY_PURGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB515SdsoEntityPurge(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB515SdsoEntityPurge.
  /// </summary>
  public LeB515SdsoEntityPurge(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    // CHANGE LOG:
    // 03-31-00	PMcElderry
    // Original coding
    // -----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------
    // Jobrun parameters
    // -----------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ReportEabReportSend.ProcessDate = Now().Date;
        local.ReportEabReportSend.ProgramName = "SWELB515";
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

          goto Test;
        }

        local.ReportEabReportSend.ProcessDate = Now().Date;
        local.ReportEabReportSend.ProgramName = "SWELB515";
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

          goto Test;
        }

        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
        }
        else
        {
          local.ProgramCheckpointRestart.RestartInfo = "";
        }

        if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
        {
          foreach(var item in ReadCsePersonObligor())
          {
            local.DeletesForMember.Count = 0;
            local.FirstRecord.Count = 0;

            foreach(var item1 in ReadStateDebtSetoff3())
            {
              if (local.FirstRecord.Count == 0)
              {
                local.FirstRecord.Count = 1;

                continue;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              foreach(var item2 in ReadAdminActionCertObligation())
              {
                DeleteAdminActionCertObligation();
                local.DeletesForMember.Count = 1 + local.DeletesForMember.Count;
                local.CheckpointNumOfDeletes.Count = 1 + local
                  .CheckpointNumOfDeletes.Count;
              }

              foreach(var item2 in ReadAdmActCertDebtDetail())
              {
                DeleteAdmActCertDebtDetail();
                local.DeletesForMember.Count = 1 + local.DeletesForMember.Count;
                local.CheckpointNumOfDeletes.Count = 1 + local
                  .CheckpointNumOfDeletes.Count;
              }
            }

            if (local.DeletesForMember.Count > 0)
            {
              if (Equal(entities.CsePerson.Number,
                local.ProgramCheckpointRestart.RestartInfo))
              {
              }
              else
              {
                ++export.TotalMembers.Count;
              }

              local.ProgramCheckpointRestart.RestartInfo =
                entities.CsePerson.Number;
              local.ReportCsePerson.Number = entities.CsePerson.Number;
              local.ReportEabReportSend.RptDetail =
                "RECORDS DELETED FOR MEMBER " + entities.CsePerson.Number + " IS " +
                NumberToString(local.DeletesForMember.Count, 15);
              local.ReportEabReportSend.ProgramName = "SWELB515";
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

                goto Test;
              }

              local.ReportEabReportSend.RptDetail = "";
              local.ReportEabReportSend.ProgramName = "SWELB515";
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

                goto Test;
              }

              export.TotalNbrOfDeletes.Count += local.DeletesForMember.Count;

              // ----------------------
              // Check the commit count
              // ----------------------
              if (local.CheckpointNumOfDeletes.Count > local
                .ProgramCheckpointRestart.UpdateFrequencyCount.
                  GetValueOrDefault())
              {
                export.TotalNbrOfDeletes.Count += local.CheckpointNumOfDeletes.
                  Count;
                local.ProgramCheckpointRestart.RestartInfo =
                  entities.CsePerson.Number;
                local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
                local.ProgramCheckpointRestart.RestartInd = "Y";
                UseUpdatePgmCheckpointRestart();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseExtToDoACommit();

                  if (local.PassArea.NumericReturnCode != 0)
                  {
                    ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                    goto Test;
                  }
                  else
                  {
                    // -------------------
                    // continue processing
                    // -------------------
                    local.CheckpointNumOfDeletes.Count = 0;

                    continue;
                  }
                }
                else
                {
                  // --------------------
                  // terminate processing
                  // --------------------
                  goto Test;
                }
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }
            }
            else
            {
              // -------------------------------------------------------------
              // After the 1st time the job is run, a member's past SDSO
              // records will not indicate the entities that were previously
              // DELETEed.
              // As a result, this condition check must be performed.
              // -------------------------------------------------------------
            }
          }
        }
        else
        {
          foreach(var item in ReadCsePersonObligor())
          {
            local.DeletesForMember.Count = 0;

            foreach(var item1 in ReadStateDebtSetoff1())
            {
              local.NullCheck.TakenDate =
                entities.TestForAllowableDelete.TakenDate;

              if (Lt(entities.TestForAllowableDelete.TakenDate,
                AddDays(local.ProgramProcessingInfo.ProcessDate, -14)))
              {
                local.ReportEabReportSend.RptDetail =
                  "DELETE CANNOT TAKE PLACE.  THERE ARE NO SDSO RECORDS CREATED AFTER PROCESSING DATE.  MEMBER NUMBER IS " +
                  entities.CsePerson.Number;
                local.ReportEabReportSend.ProgramName = "SWELB515";
                local.ReportEabFileHandling.Action = "WRITE";
                UseCabErrorReport1();

                if (Equal(local.ReportEabFileHandling.Status, "OK"))
                {
                  ++export.TotalNbrErrorsWritten.Count;
                }
                else
                {
                  // --------------------
                  // terminate processing
                  // --------------------
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  goto Test;
                }

                local.ReportEabReportSend.RptDetail = "";
                local.ReportEabReportSend.ProgramName = "SWELB515";
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
                  // --------------------
                  // terminate processing
                  // --------------------
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  goto Test;
                }

                ExitState = "ACO_NN0000_ALL_OK";
                local.ProgramCheckpointRestart.RestartInfo =
                  entities.CsePerson.Number;

                goto ReadEach;
              }
              else
              {
                break;

                // -------------------
                // continue processing
                // -------------------
              }
            }

            // --------------------------------------------------------
            // Discontinues logic trail for obligors w/out SDSO records
            // --------------------------------------------------------
            if (Equal(local.NullCheck.TakenDate, null))
            {
              continue;
            }
            else
            {
              // -------------------
              // continue processing
              // -------------------
            }

            local.DeletesForMember.Count = 0;
            local.NullCheck.TakenDate = null;

            // -----------------------------------------------------------
            // per state request (04/24/2000), keep the last 2 weeks worth
            // of records untouched
            // -----------------------------------------------------------
            foreach(var item1 in ReadStateDebtSetoff2())
            {
              foreach(var item2 in ReadAdminActionCertObligation())
              {
                DeleteAdminActionCertObligation();
                local.DeletesForMember.Count = 1 + local.DeletesForMember.Count;
                local.CheckpointNumOfDeletes.Count = 1 + local
                  .CheckpointNumOfDeletes.Count;
              }

              foreach(var item2 in ReadAdmActCertDebtDetail())
              {
                DeleteAdmActCertDebtDetail();
                local.DeletesForMember.Count = 1 + local.DeletesForMember.Count;
                local.CheckpointNumOfDeletes.Count = 1 + local
                  .CheckpointNumOfDeletes.Count;
              }
            }

            if (local.DeletesForMember.Count > 0)
            {
              if (Equal(entities.CsePerson.Number,
                local.ProgramCheckpointRestart.RestartInfo))
              {
              }
              else
              {
                ++export.TotalMembers.Count;
              }

              local.ProgramCheckpointRestart.RestartInfo =
                entities.CsePerson.Number;
              local.ReportCsePerson.Number = entities.CsePerson.Number;
              local.ReportEabReportSend.RptDetail =
                "RECORDS DELETED FOR MEMBER " + entities.CsePerson.Number + " IS " +
                NumberToString(local.DeletesForMember.Count, 15);
              local.ReportEabReportSend.ProgramName = "SWELB515";
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

                goto Test;
              }

              local.ReportEabReportSend.RptDetail = "";
              local.ReportEabReportSend.ProgramName = "SWELB515";
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

                goto Test;
              }

              export.TotalNbrOfDeletes.Count += local.DeletesForMember.Count;

              // ----------------------
              // Check the commit count
              // ----------------------
              if (local.CheckpointNumOfDeletes.Count > local
                .ProgramCheckpointRestart.UpdateFrequencyCount.
                  GetValueOrDefault())
              {
                local.ProgramCheckpointRestart.RestartInfo =
                  entities.CsePerson.Number;
                local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
                local.ProgramCheckpointRestart.RestartInd = "Y";
                UseUpdatePgmCheckpointRestart();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseExtToDoACommit();

                  if (local.PassArea.NumericReturnCode != 0)
                  {
                    ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                    goto Test;
                  }
                  else
                  {
                    // -------------------
                    // continue processing
                    // -------------------
                    local.CheckpointNumOfDeletes.Count = 0;

                    continue;
                  }
                }
                else
                {
                  // --------------------
                  // terminate processing
                  // --------------------
                  goto Test;
                }
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              local.DeletesForMember.Count = 0;
            }
            else
            {
            }

ReadEach:
            ;
          }
        }
      }
      else
      {
        // --------------------
        // terminate processing
        // --------------------
      }
    }
    else
    {
      // --------------------
      // terminate processing
      // --------------------
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ReportEabReportSend.RptDetail = "";
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER OF ERRORS:   " + TrimEnd
        (NumberToString(export.TotalNbrErrorsWritten.Count, 15));
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail =
        "TOTAL NUMBER OF MEMBER RECORDS DELETED:  " + NumberToString
        (export.TotalMembers.Count, 15);
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail =
        "TOTAL NUMBER OF DELETIONS FOR ALL MEMBERS:  " + NumberToString
        (export.TotalNbrOfDeletes.Count, 15);
      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.ProgramName = "SWELB515";
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
        // --------------------
        // terminate processing
        // --------------------
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.ProgramName = "SWELB515";
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
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
    }
    else
    {
      // -----------------
      // end of processing
      // -----------------
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart2(useExport.ProgramCheckpointRestart,
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void DeleteAdmActCertDebtDetail()
  {
    Update("DeleteAdmActCertDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "obgId", entities.AdmActCertDebtDetail.ObgId);
        db.SetString(
          command, "cspNumber", entities.AdmActCertDebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.AdmActCertDebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.AdmActCertDebtDetail.OtrId);
        db.SetInt32(command, "otyId", entities.AdmActCertDebtDetail.OtyId);
        db.SetString(command, "otrType", entities.AdmActCertDebtDetail.OtrType);
        db.SetString(
          command, "cpaTypeDebt", entities.AdmActCertDebtDetail.CpaTypeDebt);
        db.SetString(
          command, "cspNumberDebt",
          entities.AdmActCertDebtDetail.CspNumberDebt);
        db.SetString(command, "aacType", entities.AdmActCertDebtDetail.AacType);
        db.SetDate(
          command, "aacTakenDate",
          entities.AdmActCertDebtDetail.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.AdmActCertDebtDetail.AacTanfCode);
      });
  }

  private void DeleteAdminActionCertObligation()
  {
    Update("DeleteAdminActionCertObligation",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.AdminActionCertObligation.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacType", entities.AdminActionCertObligation.AacType);
        db.SetString(
          command, "cspNumber", entities.AdminActionCertObligation.CspNumber);
        db.SetString(
          command, "cpaType", entities.AdminActionCertObligation.CpaType);
        db.SetDateTime(
          command, "tstamp",
          entities.AdminActionCertObligation.Timestamp.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode",
          entities.AdminActionCertObligation.AacTanfCode);
      });
  }

  private IEnumerable<bool> ReadAdmActCertDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.StateDebtSetoff.Populated);
    entities.AdmActCertDebtDetail.Populated = false;

    return ReadEach("ReadAdmActCertDebtDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.StateDebtSetoff.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.StateDebtSetoff.Type1);
        db.SetString(command, "aacTanfCode", entities.StateDebtSetoff.TanfCode);
        db.SetString(
          command, "cspNumberDebt", entities.StateDebtSetoff.CspNumber);
        db.SetString(command, "cpaTypeDebt", entities.StateDebtSetoff.CpaType);
      },
      (db, reader) =>
      {
        entities.AdmActCertDebtDetail.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.AdmActCertDebtDetail.ObgId = db.GetInt32(reader, 1);
        entities.AdmActCertDebtDetail.CspNumber = db.GetString(reader, 2);
        entities.AdmActCertDebtDetail.CpaType = db.GetString(reader, 3);
        entities.AdmActCertDebtDetail.OtrId = db.GetInt32(reader, 4);
        entities.AdmActCertDebtDetail.OtyId = db.GetInt32(reader, 5);
        entities.AdmActCertDebtDetail.OtrType = db.GetString(reader, 6);
        entities.AdmActCertDebtDetail.CpaTypeDebt = db.GetString(reader, 7);
        entities.AdmActCertDebtDetail.CspNumberDebt = db.GetString(reader, 8);
        entities.AdmActCertDebtDetail.AacType = db.GetString(reader, 9);
        entities.AdmActCertDebtDetail.AacTakenDate = db.GetDate(reader, 10);
        entities.AdmActCertDebtDetail.AacTanfCode = db.GetString(reader, 11);
        entities.AdmActCertDebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadAdminActionCertObligation()
  {
    System.Diagnostics.Debug.Assert(entities.StateDebtSetoff.Populated);
    entities.AdminActionCertObligation.Populated = false;

    return ReadEach("ReadAdminActionCertObligation",
      (db, command) =>
      {
        db.SetString(command, "aacTanfCode", entities.StateDebtSetoff.TanfCode);
        db.SetDate(
          command, "aacTakenDate",
          entities.StateDebtSetoff.TakenDate.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.StateDebtSetoff.CpaType);
        db.SetString(command, "aacType", entities.StateDebtSetoff.Type1);
        db.SetString(command, "cspNumber", entities.StateDebtSetoff.CspNumber);
      },
      (db, reader) =>
      {
        entities.AdminActionCertObligation.AacTakenDate = db.GetDate(reader, 0);
        entities.AdminActionCertObligation.AacType = db.GetString(reader, 1);
        entities.AdminActionCertObligation.CspNumber = db.GetString(reader, 2);
        entities.AdminActionCertObligation.CpaType = db.GetString(reader, 3);
        entities.AdminActionCertObligation.Timestamp =
          db.GetDateTime(reader, 4);
        entities.AdminActionCertObligation.AacTanfCode =
          db.GetString(reader, 5);
        entities.AdminActionCertObligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetNullableString(
          command, "restartInfo",
          local.ProgramCheckpointRestart.RestartInfo ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStateDebtSetoff1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.TestForAllowableDelete.Populated = false;

    return ReadEach("ReadStateDebtSetoff1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.TestForAllowableDelete.CpaType = db.GetString(reader, 0);
        entities.TestForAllowableDelete.CspNumber = db.GetString(reader, 1);
        entities.TestForAllowableDelete.Type1 = db.GetString(reader, 2);
        entities.TestForAllowableDelete.TakenDate = db.GetDate(reader, 3);
        entities.TestForAllowableDelete.TanfCode = db.GetString(reader, 4);
        entities.TestForAllowableDelete.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStateDebtSetoff2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.StateDebtSetoff.Populated = false;

    return ReadEach("ReadStateDebtSetoff2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableDate(
          command, "processDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.StateDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.StateDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.StateDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.StateDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.StateDebtSetoff.TanfCode = db.GetString(reader, 4);
        entities.StateDebtSetoff.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadStateDebtSetoff3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.StateDebtSetoff.Populated = false;

    return ReadEach("ReadStateDebtSetoff3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.StateDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.StateDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.StateDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.StateDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.StateDebtSetoff.TanfCode = db.GetString(reader, 4);
        entities.StateDebtSetoff.Populated = true;

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
    /// A value of ReportFilesOpened.
    /// </summary>
    [JsonPropertyName("reportFilesOpened")]
    public Common ReportFilesOpened
    {
      get => reportFilesOpened ??= new();
      set => reportFilesOpened = value;
    }

    /// <summary>
    /// A value of TotalNbrOfDeletes.
    /// </summary>
    [JsonPropertyName("totalNbrOfDeletes")]
    public Common TotalNbrOfDeletes
    {
      get => totalNbrOfDeletes ??= new();
      set => totalNbrOfDeletes = value;
    }

    /// <summary>
    /// A value of TotalNbrErrorsWritten.
    /// </summary>
    [JsonPropertyName("totalNbrErrorsWritten")]
    public Common TotalNbrErrorsWritten
    {
      get => totalNbrErrorsWritten ??= new();
      set => totalNbrErrorsWritten = value;
    }

    /// <summary>
    /// A value of TotalMembers.
    /// </summary>
    [JsonPropertyName("totalMembers")]
    public Common TotalMembers
    {
      get => totalMembers ??= new();
      set => totalMembers = value;
    }

    private Common reportFilesOpened;
    private Common totalNbrOfDeletes;
    private Common totalNbrErrorsWritten;
    private Common totalMembers;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullCheck.
    /// </summary>
    [JsonPropertyName("nullCheck")]
    public AdministrativeActCertification NullCheck
    {
      get => nullCheck ??= new();
      set => nullCheck = value;
    }

    /// <summary>
    /// A value of FirstRecord.
    /// </summary>
    [JsonPropertyName("firstRecord")]
    public Common FirstRecord
    {
      get => firstRecord ??= new();
      set => firstRecord = value;
    }

    /// <summary>
    /// A value of CheckpointNumOfDeletes.
    /// </summary>
    [JsonPropertyName("checkpointNumOfDeletes")]
    public Common CheckpointNumOfDeletes
    {
      get => checkpointNumOfDeletes ??= new();
      set => checkpointNumOfDeletes = value;
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
    /// A value of ReportCsePerson.
    /// </summary>
    [JsonPropertyName("reportCsePerson")]
    public CsePerson ReportCsePerson
    {
      get => reportCsePerson ??= new();
      set => reportCsePerson = value;
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
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of TotalMembers.
    /// </summary>
    [JsonPropertyName("totalMembers")]
    public Common TotalMembers
    {
      get => totalMembers ??= new();
      set => totalMembers = value;
    }

    /// <summary>
    /// A value of DeletesForMember.
    /// </summary>
    [JsonPropertyName("deletesForMember")]
    public Common DeletesForMember
    {
      get => deletesForMember ??= new();
      set => deletesForMember = value;
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

    private AdministrativeActCertification nullCheck;
    private Common firstRecord;
    private Common checkpointNumOfDeletes;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson reportCsePerson;
    private EabReportSend reportEabReportSend;
    private EabFileHandling reportEabFileHandling;
    private Common totalMembers;
    private Common deletesForMember;
    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of StateDebtSetoff.
    /// </summary>
    [JsonPropertyName("stateDebtSetoff")]
    public AdministrativeActCertification StateDebtSetoff
    {
      get => stateDebtSetoff ??= new();
      set => stateDebtSetoff = value;
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

    /// <summary>
    /// A value of AdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("adminActionCertObligation")]
    public AdminActionCertObligation AdminActionCertObligation
    {
      get => adminActionCertObligation ??= new();
      set => adminActionCertObligation = value;
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
    /// A value of AdmActCertDebtDetail.
    /// </summary>
    [JsonPropertyName("admActCertDebtDetail")]
    public AdmActCertDebtDetail AdmActCertDebtDetail
    {
      get => admActCertDebtDetail ??= new();
      set => admActCertDebtDetail = value;
    }

    /// <summary>
    /// A value of TestForAllowableDelete.
    /// </summary>
    [JsonPropertyName("testForAllowableDelete")]
    public AdministrativeActCertification TestForAllowableDelete
    {
      get => testForAllowableDelete ??= new();
      set => testForAllowableDelete = value;
    }

    private Obligation obligation;
    private AdministrativeActCertification stateDebtSetoff;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private AdminActionCertObligation adminActionCertObligation;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private AdmActCertDebtDetail admActCertDebtDetail;
    private AdministrativeActCertification testForAllowableDelete;
  }
#endregion
}
