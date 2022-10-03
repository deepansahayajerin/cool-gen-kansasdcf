// Program: SP_B308_ARCHIVE_FIELD_VALUES_RS, ID: 372968418, model: 746.
// Short name: SWEP308B
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
/// A program: SP_B308_ARCHIVE_FIELD_VALUES_RS.
/// </para>
/// <para>
/// Archive FIELD_VALUE records if associated OUTGOING_DOCUMENT printed 
/// successfully 30 days back.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB308ArchiveFieldValuesRs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B308_ARCHIVE_FIELD_VALUES_RS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB308ArchiveFieldValuesRs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB308ArchiveFieldValuesRs.
  /// </summary>
  public SpB308ArchiveFieldValuesRs(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // DATE		Developer	Number		Description
    // ----------------------------------------------------------------------------
    // 11/09/1999	Srini Ganji	Initial Dev	Archive FIELD_VALUE records if
    // 						associated OUTGOING_DOCUMENT printed
    // 						successfully 30 days back.
    // 12/22/1999	Srini Ganji			Added Checkpoint restart logic
    // 02/03/2000	Srini Ganji			Extract Input Parameters from PPI
    // 						Parameter List
    // 08/08/2000	M Ramirez	99884		Don't archive ' KEY' fields
    // 08/08/2000	M Ramirez			Added housekeeping and control
    // 						report CABs
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB308Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ************************************************
    // Read Outgoing_documents and check for Archival of Field values
    // ************************************************
    foreach(var item in ReadOutgoingDocumentInfrastructure())
    {
      ++local.TotalDocsRead.Count;

      // *****************************************************************
      // Check for Archive date
      // *****************************************************************
      if (!Lt(local.NullDateWorkArea.Date,
        entities.OutgoingDocument.FieldValuesArchiveDate))
      {
        // *****************************************************************
        // Outgoing Document printed successfully 30 days back  and not archived
        // till now, so Read Field values and archive into External file
        // *****************************************************************
        // mjr
        // ----------------------------------------------
        // 08/08/2000
        // PR# 99884 - Don't archive ' KEY' fields
        // Added the "... AND DESIRED field dependancy IS NOT EQUAL TO " KEY" 
        // clause
        // -----------------------------------------------------------
        foreach(var item1 in ReadFieldValueDocumentField())
        {
          ++local.TotalFieldValuesRead.Count;
          local.WsFieldValues.ArchiveDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.WsFieldValues.InfId =
            entities.Infrastructure.SystemGeneratedIdentifier;
          local.WsFieldValues.DocName = entities.Document.Name;
          local.WsFieldValues.DocEffectiveDate =
            entities.Document.EffectiveDate;
          local.WsFieldValues.FldName = entities.Field.Name;
          local.WsFieldValues.Valu = entities.FieldValue.Value ?? Spaces(245);
          local.WsFieldValues.CreatedBy = entities.FieldValue.CreatedBy;
          local.WsFieldValues.CreatedTimestamp =
            entities.FieldValue.CreatedTimestamp;
          local.WsFieldValues.LastUpdatedBy = entities.FieldValue.LastUpdatedBy;
          local.WsFieldValues.LastUpdatedTstamp =
            entities.FieldValue.LastUpdatdTstamp;

          // *****************************************************************
          // Call external to write Field values into External File
          // *****************************************************************
          local.PassArea.FileInstruction = "WRITE";
          UseSpEabArchiveFieldValuesRs();

          if (!IsEmpty(local.PassArea.TextReturnCode))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error writing External File : " + NumberToString
              (local.PassArea.NumericReturnCode, 14, 2);
            UseCabErrorReport();
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalRecsWritnOnArcFl.Count;
          local.WsFieldValues.Assign(local.NullWsFieldValues);

          // *****************************************************************
          // On successful write, delete Field value record
          // *****************************************************************
          DeleteFieldValue();
          ++local.TotalFldValsArchiveDel.Count;
        }

        // *****************************************************************
        // Update Outgoing_Document Archive Flag to 'Y' and Set
        // Archive date to Process date
        // *****************************************************************
        try
        {
          UpdateOutgoingDocument2();
          ++local.TotalDocsArchivAndUpdt.Count;
          ++local.Commit.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // ------------------------------------------------
              // Never happen
              // ------------------------------------------------
              break;
            case ErrorCode.PermittedValueViolation:
              // ------------------------------------------------
              // Never happen
              // ------------------------------------------------
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (Lt(entities.OutgoingDocument.FieldValuesArchiveDate,
        local.Archive.Date))
      {
        // *****************************************************************
        // Field values already archived and retrieved 60 days back,
        // so delete Field value information
        // *****************************************************************
        // mjr
        // ----------------------------------------------
        // 08/08/2000
        // PR# 99884 - Don't archive ' KEY' fields
        // Added FIELD to the READ EACH
        // -----------------------------------------------------------
        foreach(var item1 in ReadFieldValueField())
        {
          ++local.TotalFieldValuesRead.Count;
          DeleteFieldValue();
          ++local.TotalFldValsOnlyDel.Count;
        }

        // *****************************************************************
        // Update ONLY Outgoing_Document Archive Flag to 'Y'
        // *****************************************************************
        try
        {
          UpdateOutgoingDocument1();
          ++local.TotalDocsOnlyUpdated.Count;
          ++local.Commit.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // ------------------------------------------------
              // Never happen
              // ------------------------------------------------
              break;
            case ErrorCode.PermittedValueViolation:
              // ------------------------------------------------
              // Never happen
              // ------------------------------------------------
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // *****************************************************************
      // Check for Commit count
      // *****************************************************************
      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // Write Restart information on Checkpoint restart info field
        // Information on restart Info field,
        // 1. Process Date, 10
        // 2. Infrastructure Id, 15
        // 3. Records written on Archive File,15
        // 4. Outgoing Documents Read, 15
        // 5. Field Values Read , 15
        // 6. Outgoing Documents Archived and Updated , 15
        // 7. Outgoing Documents only Updated , 15
        // 8. Field Values Archived and Deleted , 15
        // 9. Field Values only Deleted, 15
        // *****************************************************************
        local.ProgramCheckpointRestart.RestartInfo = "";

        // *** Process date (YYYY-MM-DD), 10
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) +
          "-" + NumberToString
          (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "-";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 8) + NumberToString
          (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);

        // *** Infrastructure Id, 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10) + NumberToString
          (entities.Infrastructure.SystemGeneratedIdentifier, 15);

        // *** Records written on Archive File,15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 25) + NumberToString
          (local.TotalRecsWritnOnArcFl.Count, 15);

        // *** Outgoing Documents Read, 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 40) + NumberToString
          (local.TotalDocsRead.Count, 15);

        // *** Field Values Read , 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 55) + NumberToString
          (local.TotalFieldValuesRead.Count, 15);

        // *** Outgoing Documents Archived and Updated , 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 70) + NumberToString
          (local.TotalDocsArchivAndUpdt.Count, 15);

        // *** Outgoing Documents only Updated, 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 85) + NumberToString
          (local.TotalDocsOnlyUpdated.Count, 15);

        // *** Field Values Archived and Deleted, 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 100) + NumberToString
          (local.TotalFldValsArchiveDel.Count, 15);

        // *** Field Values only Deleted, 15
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 115) + NumberToString
          (local.TotalFldValsOnlyDel.Count, 15);
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error while updating Checkpoint restart information";
          UseCabErrorReport();
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

          return;
        }

        // *****************************************************************
        // Commit database
        // *****************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.Commit.Count = 0;
      }
    }

    // *****************************************************************
    // Before final Commit, update Checkpoint restart info to Spaces and
    // restart ind to 'N'
    // *****************************************************************
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error while updating Job end Checkpoint restart information";
      UseCabErrorReport();
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

      return;
    }

    // *****************************************************************
    // Do final Commit
    // *****************************************************************
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    UseSpB308WriteControlsAndClose();
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpB308Housekeeping()
  {
    var useImport = new SpB308Housekeeping.Import();
    var useExport = new SpB308Housekeeping.Export();

    Call(SpB308Housekeeping.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Archive, local.Archive);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.Restart.SystemGeneratedIdentifier =
      useExport.Restart.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.TotalDocsOnlyUpdated.Count = useExport.DocsOnlyUpdated.Count;
    local.TotalRecsWritnOnArcFl.Count = useExport.RecsWrittenToArchive.Count;
    local.TotalFldValsOnlyDel.Count = useExport.FieldValuesOnlyDeleted.Count;
    local.TotalFldValsArchiveDel.Count = useExport.FieldValuesArchivedDel.Count;
    local.TotalFieldValuesRead.Count = useExport.FieldValuesRead.Count;
    local.TotalDocsRead.Count = useExport.DocsRead.Count;
    local.TotalDocsArchivAndUpdt.Count = useExport.DocsArchvdAndUpdated.Count;
  }

  private void UseSpB308WriteControlsAndClose()
  {
    var useImport = new SpB308WriteControlsAndClose.Import();
    var useExport = new SpB308WriteControlsAndClose.Export();

    useImport.DocsOnlyUpdated.Count = local.TotalDocsOnlyUpdated.Count;
    useImport.RecsWrittenToArchive.Count = local.TotalRecsWritnOnArcFl.Count;
    useImport.FieldValuesOnlyDeleted.Count = local.TotalFldValsOnlyDel.Count;
    useImport.FieldValuesArchvdDeltd.Count = local.TotalFldValsArchiveDel.Count;
    useImport.FieldValuesRead.Count = local.TotalFieldValuesRead.Count;
    useImport.DocsRead.Count = local.TotalDocsRead.Count;
    useImport.DocsArchvdUpdated.Count = local.TotalDocsArchivAndUpdt.Count;

    Call(SpB308WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpEabArchiveFieldValuesRs()
  {
    var useImport = new SpEabArchiveFieldValuesRs.Import();
    var useExport = new SpEabArchiveFieldValuesRs.Export();

    useImport.WsFieldValues.Assign(local.WsFieldValues);
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(SpEabArchiveFieldValuesRs.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void DeleteFieldValue()
  {
    Update("DeleteFieldValue",
      (db, command) =>
      {
        db.SetString(command, "fldName", entities.FieldValue.FldName);
        db.SetString(command, "docName", entities.FieldValue.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          entities.FieldValue.DocEffectiveDte.GetValueOrDefault());
        db.
          SetInt32(command, "infIdentifier", entities.FieldValue.InfIdentifier);
          
      });
  }

  private IEnumerable<bool> ReadFieldValueDocumentField()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Field.Populated = false;
    entities.Document.Populated = false;
    entities.FieldValue.Populated = false;

    return ReadEach("ReadFieldValueDocumentField",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.CreatedBy = db.GetString(reader, 0);
        entities.FieldValue.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.FieldValue.LastUpdatedBy = db.GetString(reader, 2);
        entities.FieldValue.LastUpdatdTstamp = db.GetDateTime(reader, 3);
        entities.FieldValue.Value = db.GetNullableString(reader, 4);
        entities.FieldValue.FldName = db.GetString(reader, 5);
        entities.FieldValue.DocName = db.GetString(reader, 6);
        entities.Document.Name = db.GetString(reader, 6);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 7);
        entities.Document.EffectiveDate = db.GetDate(reader, 7);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 8);
        entities.Field.Name = db.GetString(reader, 9);
        entities.Field.Dependancy = db.GetString(reader, 10);
        entities.Field.Populated = true;
        entities.Document.Populated = true;
        entities.FieldValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFieldValueField()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Field.Populated = false;
    entities.FieldValue.Populated = false;

    return ReadEach("ReadFieldValueField",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.CreatedBy = db.GetString(reader, 0);
        entities.FieldValue.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.FieldValue.LastUpdatedBy = db.GetString(reader, 2);
        entities.FieldValue.LastUpdatdTstamp = db.GetDateTime(reader, 3);
        entities.FieldValue.Value = db.GetNullableString(reader, 4);
        entities.FieldValue.FldName = db.GetString(reader, 5);
        entities.FieldValue.DocName = db.GetString(reader, 6);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 7);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 8);
        entities.Field.Name = db.GetString(reader, 9);
        entities.Field.Dependancy = db.GetString(reader, 10);
        entities.Field.Populated = true;
        entities.FieldValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructure()
  {
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fieldValArchDt", local.Archive.Date.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.Archive.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "infId", local.Restart.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private void UpdateOutgoingDocument1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var fieldValuesArchiveInd = "Y";

    CheckValid<OutgoingDocument>("FieldValuesArchiveInd", fieldValuesArchiveInd);
      
    entities.OutgoingDocument.Populated = false;
    Update("UpdateOutgoingDocument1",
      (db, command) =>
      {
        db.SetNullableString(command, "fieldValArchInd", fieldValuesArchiveInd);
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      });

    entities.OutgoingDocument.FieldValuesArchiveInd = fieldValuesArchiveInd;
    entities.OutgoingDocument.Populated = true;
  }

  private void UpdateOutgoingDocument2()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var fieldValuesArchiveDate = local.ProgramProcessingInfo.ProcessDate;
    var fieldValuesArchiveInd = "Y";

    CheckValid<OutgoingDocument>("FieldValuesArchiveInd", fieldValuesArchiveInd);
      
    entities.OutgoingDocument.Populated = false;
    Update("UpdateOutgoingDocument2",
      (db, command) =>
      {
        db.SetNullableDate(command, "fieldValArchDt", fieldValuesArchiveDate);
        db.SetNullableString(command, "fieldValArchInd", fieldValuesArchiveInd);
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      });

    entities.OutgoingDocument.FieldValuesArchiveDate = fieldValuesArchiveDate;
    entities.OutgoingDocument.FieldValuesArchiveInd = fieldValuesArchiveInd;
    entities.OutgoingDocument.Populated = true;
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
    /// A value of Archive.
    /// </summary>
    [JsonPropertyName("archive")]
    public DateWorkArea Archive
    {
      get => archive ??= new();
      set => archive = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Infrastructure Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of WsFieldValues.
    /// </summary>
    [JsonPropertyName("wsFieldValues")]
    public WsFieldValues WsFieldValues
    {
      get => wsFieldValues ??= new();
      set => wsFieldValues = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of NullWsFieldValues.
    /// </summary>
    [JsonPropertyName("nullWsFieldValues")]
    public WsFieldValues NullWsFieldValues
    {
      get => nullWsFieldValues ??= new();
      set => nullWsFieldValues = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of TotalDocsOnlyUpdated.
    /// </summary>
    [JsonPropertyName("totalDocsOnlyUpdated")]
    public Common TotalDocsOnlyUpdated
    {
      get => totalDocsOnlyUpdated ??= new();
      set => totalDocsOnlyUpdated = value;
    }

    /// <summary>
    /// A value of TotalRecsWritnOnArcFl.
    /// </summary>
    [JsonPropertyName("totalRecsWritnOnArcFl")]
    public Common TotalRecsWritnOnArcFl
    {
      get => totalRecsWritnOnArcFl ??= new();
      set => totalRecsWritnOnArcFl = value;
    }

    /// <summary>
    /// A value of TotalFldValsOnlyDel.
    /// </summary>
    [JsonPropertyName("totalFldValsOnlyDel")]
    public Common TotalFldValsOnlyDel
    {
      get => totalFldValsOnlyDel ??= new();
      set => totalFldValsOnlyDel = value;
    }

    /// <summary>
    /// A value of TotalFldValsArchiveDel.
    /// </summary>
    [JsonPropertyName("totalFldValsArchiveDel")]
    public Common TotalFldValsArchiveDel
    {
      get => totalFldValsArchiveDel ??= new();
      set => totalFldValsArchiveDel = value;
    }

    /// <summary>
    /// A value of TotalFieldValuesRead.
    /// </summary>
    [JsonPropertyName("totalFieldValuesRead")]
    public Common TotalFieldValuesRead
    {
      get => totalFieldValuesRead ??= new();
      set => totalFieldValuesRead = value;
    }

    /// <summary>
    /// A value of TotalDocsRead.
    /// </summary>
    [JsonPropertyName("totalDocsRead")]
    public Common TotalDocsRead
    {
      get => totalDocsRead ??= new();
      set => totalDocsRead = value;
    }

    /// <summary>
    /// A value of TotalDocsArchivAndUpdt.
    /// </summary>
    [JsonPropertyName("totalDocsArchivAndUpdt")]
    public Common TotalDocsArchivAndUpdt
    {
      get => totalDocsArchivAndUpdt ??= new();
      set => totalDocsArchivAndUpdt = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    private DateWorkArea archive;
    private Common debugOn;
    private Infrastructure restart;
    private WsFieldValues wsFieldValues;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External passArea;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private WsFieldValues nullWsFieldValues;
    private DateWorkArea nullDateWorkArea;
    private Common totalDocsOnlyUpdated;
    private Common totalRecsWritnOnArcFl;
    private Common totalFldValsOnlyDel;
    private Common totalFldValsArchiveDel;
    private Common totalFieldValuesRead;
    private Common totalDocsRead;
    private Common totalDocsArchivAndUpdt;
    private Common commit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private Field field;
    private Infrastructure infrastructure;
    private DocumentField documentField;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private FieldValue fieldValue;
  }
#endregion
}
