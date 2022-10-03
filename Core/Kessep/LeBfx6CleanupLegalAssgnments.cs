// Program: LE_BFX6_CLEANUP_LEGAL_ASSGNMENTS, ID: 945034199, model: 746.
// Short name: SWEBFX6P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX6_CLEANUP_LEGAL_ASSGNMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx6CleanupLegalAssgnments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX6_CLEANUP_LEGAL_ASSGNMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx6CleanupLegalAssgnments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx6CleanupLegalAssgnments.
  /// </summary>
  public LeBfx6CleanupLegalAssgnments(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 10/21/10  GVandy	CQ109		Initial Development
    // 12/03/10  GVandy	CQ109		For Segment B, close open interstate case 
    // assignments on closed cases.
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);

    // -------------------------------------------------------------------------------------------
    // -- Read the PPI record.
    // -------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Determine if this is a restart.
    // --------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------------------------------
      // -- We are restarting.
      // --------------------------------------------------------------------------------------------
      // -- Extract which section we restarting in.
      if (IsEmpty(Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 2)))
      {
        local.RestartInSection.Text2 = "00";
      }
      else
      {
        local.RestartInSection.Text2 =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 2);
      }
    }
    else
    {
      local.RestartInSection.Text2 = "00";
    }

    // --------------------------------------------------------------------------------------------
    // -- Open Error Report.
    // --------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = global.UserId;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Open Control Report.
    // --------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // --------------------------------------------------------------------------------------------
    // -- PPI Info...
    // --	Positions	Description
    // --	---------	
    // ------------------------------------------
    // --	 1 to 6   	Run Mode (REPORT or UPDATE)
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- Determine the run mode.
    // --------------------------------------------------------------------------------------------
    if (Equal(local.ProgramProcessingInfo.ParameterList, 1, 6, "UPDATE"))
    {
      local.RunMode.Command = "UPDATE";
    }
    else
    {
      // -- If run mode is not specifically set to UPDATE on the PPI then 
      // default to REPORT mode.
      local.RunMode.Command = "REPORT";
    }

    // --------------------------------------------------------------------------------------------
    // -- Log the parameters to the Control Report.
    // --------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Restarting!  Checkpoint info... " + (
                local.ProgramCheckpointRestart.RestartInfo ?? "");
          }
          else
          {
            local.Common.Count = 2;

            continue;
          }

          break;
        case 3:
          local.EabReportSend.RptDetail = "Run Mode = " + Substring
            (local.RunMode.Command, Common.Command_MaxLength, 1, 6);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Close all active EST legal action assignments.
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- Close all active RSP legal action assignments if the legal action is 
    // not associated to any open case.
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- End date MONAs on closed cases.
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- End date Monitored Documents on closed cases.
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- Delete Alerts on closed cases.
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- End Date Interstate Case Assignments on closed cases.
    // --------------------------------------------------------------------------------------------
    if (!Lt("06", local.RestartInSection.Text2))
    {
      if (Equal(local.RestartInSection.Text2, "06"))
      {
        local.Restart.Number =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 4, 10);
      }
      else
      {
        local.Restart.Number = "0000000000";
      }

      if (!Equal(local.RunMode.Command, "UPDATE"))
      {
        for(local.Common.Count = 1; local.Common.Count <= 6; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 3:
              local.EabReportSend.RptDetail =
                "Interstate Case assignments that would be End Dated are below...";
                

              break;
            case 5:
              local.EabReportSend.RptDetail =
                "OFF   LAST NAME         FIRST NAME    CASE NUMB   TRAN SERIAL#  TRANS DATE  INTERST CASE ID  ACTION REASON";
                

              break;
            case 6:
              local.EabReportSend.RptDetail =
                "----  ----------------  ------------  ----------  ------------  ----------  ---------------  --------------";
                

              break;
            default:
              local.EabReportSend.RptDetail = "";

              break;
          }

          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }
      }

      foreach(var item in ReadInterstateCaseAssignmentInterstateCaseCase())
      {
        ++local.TotalIntCaseAssignments.Count;
        ++local.NumRecsUpdated.Count;

        if (Equal(local.RunMode.Command, "UPDATE"))
        {
          UpdateInterstateCaseAssignment();
        }
        else
        {
          // --------------------------------------------------------------------------------------------
          // -- Log Interstate Case info to the control report.
          // --------------------------------------------------------------------------------------------
          if (!ReadOfficeServiceProvider())
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";
            local.EabReportSend.RptDetail = UseEabExtractExitStateMessage2();
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // <office id> <sp last name>,<sp first name>  <case number> 
          // <transaction date> <interstate case id> <action reason code>
          local.EabReportSend.RptDetail =
            NumberToString(entities.Office.SystemGeneratedId, 12, 4);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 4) + "  " + entities
            .ServiceProvider.LastName;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 22) + "  " + entities
            .ServiceProvider.FirstName;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 36) + "  " + entities
            .Case1.Number;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 48) + "  " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 62) + "  " + NumberToString
            (Year(entities.InterstateCase.TransactionDate), 12, 4) + "-" + NumberToString
            (Month(entities.InterstateCase.TransactionDate), 14, 2) + "-" + NumberToString
            (Day(entities.InterstateCase.TransactionDate), 14, 2);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 74) + "  " + entities
            .InterstateCase.InterstateCaseId;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 91) + "  " + entities
            .InterstateCase.ActionReasonCode;
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }

        if (local.NumRecsUpdated.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // --  Set checkpoint to "06 <case number>"
          local.ProgramCheckpointRestart.RestartInfo = "06 " + entities
            .Case1.Number;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabReportSend.RptDetail = UseEabExtractExitStateMessage2();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  -  CHECKPOINT ERROR";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.NumRecsUpdated.Count = 0;
        }
      }

      for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
      {
        if (local.Common.Count == 3)
        {
          if (Equal(local.RunMode.Command, "UPDATE"))
          {
            local.EabReportSend.RptDetail =
              "Number of Interstate Case assignments CLOSED   = " + NumberToString
              (local.TotalIntCaseAssignments.Count, 15);
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Number of Interstate Case assignments REPORTED   = " + NumberToString
              (local.TotalIntCaseAssignments.Count, 15);
          }
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Clear the checkpoint info.
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail = UseEabExtractExitStateMessage2();
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "  -  CHECKPOINT ERROR";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Control report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage1();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Error report.
    // -------------------------------------------------------------------------------------------
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

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

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage1()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private string UseEabExtractExitStateMessage2()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCaseCase()
  {
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCaseCase",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 7);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 9);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 10);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 10);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 11);
        entities.Case1.Number = db.GetString(reader, 11);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 13);
        entities.Case1.Status = db.GetNullableString(reader, 14);
        entities.InterstateCaseAssignment.Populated = true;
        entities.InterstateCase.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId", entities.InterstateCaseAssignment.OffId);
          
        db.SetInt32(
          command, "servicePrvderId", entities.InterstateCaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = "CQ109";
    var lastUpdatedTimestamp = Now();

    entities.InterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.InterstateCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.InterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.InterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.InterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", entities.InterstateCaseAssignment.IcsNo);
      });

    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateCaseAssignment.Populated = true;
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
    /// A value of TotalIntCaseAssignments.
    /// </summary>
    [JsonPropertyName("totalIntCaseAssignments")]
    public Common TotalIntCaseAssignments
    {
      get => totalIntCaseAssignments ??= new();
      set => totalIntCaseAssignments = value;
    }

    /// <summary>
    /// A value of TotalAlerts.
    /// </summary>
    [JsonPropertyName("totalAlerts")]
    public Common TotalAlerts
    {
      get => totalAlerts ??= new();
      set => totalAlerts = value;
    }

    /// <summary>
    /// A value of TotalMonitoredDocs.
    /// </summary>
    [JsonPropertyName("totalMonitoredDocs")]
    public Common TotalMonitoredDocs
    {
      get => totalMonitoredDocs ??= new();
      set => totalMonitoredDocs = value;
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
    /// A value of TotalMonas.
    /// </summary>
    [JsonPropertyName("totalMonas")]
    public Common TotalMonas
    {
      get => totalMonas ??= new();
      set => totalMonas = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RestartInSection.
    /// </summary>
    [JsonPropertyName("restartInSection")]
    public TextWorkArea RestartInSection
    {
      get => restartInSection ??= new();
      set => restartInSection = value;
    }

    /// <summary>
    /// A value of NumRecsUpdated.
    /// </summary>
    [JsonPropertyName("numRecsUpdated")]
    public Common NumRecsUpdated
    {
      get => numRecsUpdated ??= new();
      set => numRecsUpdated = value;
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
    /// A value of RunMode.
    /// </summary>
    [JsonPropertyName("runMode")]
    public Common RunMode
    {
      get => runMode ??= new();
      set => runMode = value;
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
    /// A value of TotalRspAssignments.
    /// </summary>
    [JsonPropertyName("totalRspAssignments")]
    public Common TotalRspAssignments
    {
      get => totalRspAssignments ??= new();
      set => totalRspAssignments = value;
    }

    /// <summary>
    /// A value of TotalEstAssignments.
    /// </summary>
    [JsonPropertyName("totalEstAssignments")]
    public Common TotalEstAssignments
    {
      get => totalEstAssignments ??= new();
      set => totalEstAssignments = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    private Common totalIntCaseAssignments;
    private Common totalAlerts;
    private Common totalMonitoredDocs;
    private DateWorkArea null1;
    private Common totalMonas;
    private DateWorkArea max;
    private Case1 restart;
    private TextWorkArea restartInSection;
    private Common numRecsUpdated;
    private DateWorkArea current;
    private Common runMode;
    private Common common;
    private Common totalRspAssignments;
    private Common totalEstAssignments;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend open;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
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

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Case1 Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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

    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase interstateCase;
    private PrinterOutputDestination printerOutputDestination;
    private LegalActionDetail legalActionDetail;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private MonitoredDocument monitoredDocument;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
    private Case1 open;
    private CaseRole caseRole;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private LegalActionCaseRole legalActionCaseRole;
    private OfficeServiceProvider officeServiceProvider;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
  }
#endregion
}
