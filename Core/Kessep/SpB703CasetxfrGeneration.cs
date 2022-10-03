// Program: SP_B703_CASETXFR_GENERATION, ID: 945086135, model: 746.
// Short name: SWEP703B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B703_CASETXFR_GENERATION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB703CasetxfrGeneration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B703_CASETXFR_GENERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB703CasetxfrGeneration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB703CasetxfrGeneration.
  /// </summary>
  public SpB703CasetxfrGeneration(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 11/17/11  GVandy            CQ30161    Initial Development
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // --------------------------------------------------------------------------------
    // --  Read PPI record.
    // --------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------
    // -- Open error report
    // --------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // -- Open control report
    // --------------------------------------------------------------------------------
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

    // --------------------------------------------------------------------------------
    // --  Get commit frequency.
    // --------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------
    // -- Extract restart info.
    // --
    // --  Positions 1 -  4  Office Number
    // --  Positions 6 - 15  AR Person Number
    // --------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.Restart.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4));
      local.RestartAr.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 6, 10);

      // --------------------------------------------------------------------------------
      // -- Write restart info to the control report.
      // --------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail = "RESTARTING AT OFFICE " + Substring
            (local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4) + " PERSON " +
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 6, 10);
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }
    }

    // --------------------------------------------------------------------------------
    // -- Retrieve last run date from the PPI record.  This will be the starting
    // -- point for retrieving new case assignments.
    // --------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 8)))
    {
      local.LastRunDate.Date =
        AddDays(local.ProgramProcessingInfo.ProcessDate, -1);
    }
    else
    {
      local.LastRunDate.Date =
        StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10));
        
    }

    // --------------------------------------------------------------------------------
    // -- Log last run date to the control report.
    // --------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail = "Last Run Date " + Substring
          (local.ProgramProcessingInfo.ParameterList, 1, 10);
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    local.Null1.Date = new DateTime(1, 1, 1);

    // --------------------------------------------------------------------------------
    // -- Find each new case assignment that became effective since the last run
    // date.
    // --------------------------------------------------------------------------------
    foreach(var item in ReadOfficeCsePersonCase())
    {
      if (Lt(local.Null1.Date, entities.Ar.DateOfDeath))
      {
        continue;
      }

      if (Equal(entities.Ar.Number, local.PreviousAr.Number))
      {
        if (entities.Office.SystemGeneratedId == local
          .LastDocumentTriggered.SystemGeneratedId && Equal
          (entities.Ar.Number, local.LastDocumentTriggeredAr.Number))
        {
          // -- We already triggered a document for this Office/AR combination.
          continue;
        }
      }
      else
      {
        if (local.ArsSinceLastCheckpoint.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // --------------------------------------------------------------------------------
          // -- Checkpoint saving the office id and cse person number.
          // --------------------------------------------------------------------------------
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            NumberToString(entities.Office.SystemGeneratedId, 12, 4) + " " + local
            .PreviousAr.Number;
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -- Log the exit state message to the error report and abend.
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = UseEabExtractExitStateMessage();
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ArsSinceLastCheckpoint.Count = 0;
        }

        local.PreviousAr.Number = entities.Ar.Number;
        ++local.ArsSinceLastCheckpoint.Count;
      }

      // --------------------------------------------------------------------------------
      // -- Skip the case if it is assigned to the same office as on the last 
      // run date.
      // --------------------------------------------------------------------------------
      if (ReadOffice())
      {
        if (entities.Old.SystemGeneratedId == entities.Office.SystemGeneratedId)
        {
          continue;
        }
      }
      else
      {
        // If there was no caseworker assignment on the last run date then skip 
        // the case.
        continue;
      }

      // --------------------------------------------------------------------------------
      // -- Skip the case if it is incoming interstate.
      // --------------------------------------------------------------------------------
      foreach(var item1 in ReadInterstateRequest())
      {
        goto ReadEach;
      }

      // --------------------------------------------------------------------------------
      // -- AR meets criteria for receiving a CASETXFR document. Trigger the 
      // document.
      // --------------------------------------------------------------------------------
      local.Document.Name = "CASETXFR";
      local.SpDocKey.KeyOffice = entities.Office.SystemGeneratedId;
      local.SpDocKey.KeyAr = entities.Ar.Number;
      local.SpDocKey.KeyXferFromDate = local.LastRunDate.Date;
      local.SpDocKey.KeyXferToDate = local.ProgramProcessingInfo.ProcessDate;
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.ReferenceDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseSpCreateDocumentInfrastruct();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Write error to error report and abend.
        for(local.Common.Count = 1; local.Common.Count <= 2; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail =
                "Error creating CASETXFR document trigger for office " + NumberToString
                (entities.Office.SystemGeneratedId, 12, 4) + " AR number " + entities
                .Ar.Number;

              break;
            case 2:
              local.EabReportSend.RptDetail = UseEabExtractExitStateMessage();

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.LastDocumentTriggered.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
      local.LastDocumentTriggeredAr.Number = entities.Ar.Number;
      ++local.NumberOfDocsTriggered.Count;

ReadEach:
      ;
    }

    // --------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // --------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Log the exit state message to the error report and abend.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = UseEabExtractExitStateMessage();
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // -- Update the last run date on the PPI record to the current processing 
    // date.
    // --------------------------------------------------------------------------------
    local.DateWorkArea.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabConvertDate2String();
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 1, 2) +
      "/" + Substring
      (local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 3, 2) + "/" + Substring
      (local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 5, 4);
    UseUpdateProgramProcessingInfo();

    // --------------------------------------------------------------------------------
    // -- Log document totals to the control report.
    // --------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail =
          "Total number of CASETXFR documents triggered: " + NumberToString
          (local.NumberOfDocsTriggered.Count, 6, 10);
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // --------------------------------------------------------------------------------
    // -- Close the control report.
    // --------------------------------------------------------------------------------
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

    // --------------------------------------------------------------------------------
    // -- Close the error report.
    // --------------------------------------------------------------------------------
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DenormDate = source.DenormDate;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAr = source.KeyAr;
    target.KeyOffice = source.KeyOffice;
    target.KeyXferFromDate = source.KeyXferFromDate;
    target.KeyXferToDate = source.KeyXferToDate;
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

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
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

  private string UseEabExtractExitStateMessage()
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

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Old.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.LastRunDate.Date.GetValueOrDefault());
          
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Old.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Old.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCsePersonCase()
  {
    entities.Office.Populated = false;
    entities.Case1.Populated = false;
    entities.Ar.Populated = false;

    return ReadEach("ReadOfficeCsePersonCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.LastRunDate.Date.GetValueOrDefault());
          
        db.SetInt32(command, "officeId", local.Restart.SystemGeneratedId);
        db.SetString(command, "numb", local.RestartAr.Number);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.Ar.Type1 = db.GetString(reader, 3);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Office.Populated = true;
        entities.Case1.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);

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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of LastDocumentTriggeredAr.
    /// </summary>
    [JsonPropertyName("lastDocumentTriggeredAr")]
    public CsePerson LastDocumentTriggeredAr
    {
      get => lastDocumentTriggeredAr ??= new();
      set => lastDocumentTriggeredAr = value;
    }

    /// <summary>
    /// A value of LastDocumentTriggered.
    /// </summary>
    [JsonPropertyName("lastDocumentTriggered")]
    public Office LastDocumentTriggered
    {
      get => lastDocumentTriggered ??= new();
      set => lastDocumentTriggered = value;
    }

    /// <summary>
    /// A value of ArsSinceLastCheckpoint.
    /// </summary>
    [JsonPropertyName("arsSinceLastCheckpoint")]
    public Common ArsSinceLastCheckpoint
    {
      get => arsSinceLastCheckpoint ??= new();
      set => arsSinceLastCheckpoint = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Office Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RestartAr.
    /// </summary>
    [JsonPropertyName("restartAr")]
    public CsePerson RestartAr
    {
      get => restartAr ??= new();
      set => restartAr = value;
    }

    /// <summary>
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
    }

    /// <summary>
    /// A value of LastRunDate.
    /// </summary>
    [JsonPropertyName("lastRunDate")]
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
    }

    /// <summary>
    /// A value of NumberOfDocsTriggered.
    /// </summary>
    [JsonPropertyName("numberOfDocsTriggered")]
    public Common NumberOfDocsTriggered
    {
      get => numberOfDocsTriggered ??= new();
      set => numberOfDocsTriggered = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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

    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private Common common;
    private CsePerson lastDocumentTriggeredAr;
    private Office lastDocumentTriggered;
    private Common arsSinceLastCheckpoint;
    private Office restart;
    private CsePerson restartAr;
    private CsePerson previousAr;
    private DateWorkArea lastRunDate;
    private Common numberOfDocsTriggered;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Document document;
    private SpDocKey spDocKey;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Office Old
    {
      get => old ??= new();
      set => old = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private InterstateRequest interstateRequest;
    private Office old;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ar;
  }
#endregion
}
