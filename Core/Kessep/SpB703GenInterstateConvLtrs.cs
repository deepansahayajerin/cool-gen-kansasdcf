// Program: SP_B703_GEN_INTERSTATE_CONV_LTRS, ID: 372705173, model: 746.
// Short name: P8683506
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B703_GEN_INTERSTATE_CONV_LTRS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB703GenInterstateConvLtrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B703_GEN_INTERSTATE_CONV_LTRS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB703GenInterstateConvLtrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB703GenInterstateConvLtrs.
  /// </summary>
  public SpB703GenInterstateConvLtrs(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    //                    M A I N T E N A N C E   L O G
    // Date		Developer	Description
    // 04/24/1999	Carl Ott	Initial Dev.
    // 04/28/1999	M Ramirez	Re-work to create document trigger
    // 				instead of actual document.
    // 05/05/1999	M Ramirez	Rename PStep to SP
    // 05/12/1999	M Ramirez	No document will be generated for office 21
    // 07/01/1999	M Ramirez	Rework to use housekeeping and cleanup cabs
    // 07/01/1999	M Raimrez	Changed READ EACH to use one timestamp
    // ----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB703Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabConvertNumeric.SendNonSuppressPos = 1;

    // mjr
    // -----------------------------------------------
    // 07/01/1999
    // Modified READ EACH to use only one timestamp
    // ------------------------------------------------------------
    foreach(var item in ReadInterstateRequest())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.WriteError.Flag = "";
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      ++local.CheckpointReads.Count;
      ++local.LcontrolTotalReads.Count;

      if (ReadOffice())
      {
        if (entities.Office.SystemGeneratedId == 21)
        {
          // mjr
          // ---------------------------------------------------
          // 05/12/1999
          // No document will be generated for this office
          // ----------------------------------------------------------------
          continue;
        }
        else
        {
          local.Document.Name = "CASECHNG";
        }

        local.SpDocKey.KeyInterstateRequest =
          entities.InterstateRequest.IntHGeneratedId;
        UseSpCreateDocumentInfrastruct();
      }
      else
      {
        ExitState = "OFFICE_NF";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.LcontrolTotalErrors.Count;
        UseEabExtractExitStateMessage();

        if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
        {
          // mjr
          // -------------------------------------------------------
          // Errors that occur before an infrastructure record is
          // created cause an ABEND.
          // ----------------------------------------------------------
          // ****************************************************************
          // Write to Error Report
          // ****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();

        // ****************************************************************
        // Write to Error Report
        // ****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Infrastructure ID = " + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        try
        {
          CreateInterstateRequestHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.LcontrolTotalErrors.Count;
              local.ExitStateWorkArea.Message =
                "INTERSTATE REQUEST HISTORY already exists.";
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Infrastructure.SystemGeneratedIdentifier,
                15);
              UseEabConvertNumeric1();

              // ****************************************************************
              // Write to Error Report
              // ****************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail = "Infrastructure ID = " + TrimEnd
                (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ++local.LcontrolTotalErrors.Count;
              local.ExitStateWorkArea.Message =
                "INTERSTATE REQUEST HISTORY permitted value violation.";
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Infrastructure.SystemGeneratedIdentifier,
                15);
              UseEabConvertNumeric1();

              // ****************************************************************
              // Write to Error Report
              // ****************************************************************
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail = "Infrastructure ID = " + TrimEnd
                (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.LcontrolTotalLettersSent.Count;
      }

      if (local.CheckpointReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CheckpointUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.BatchTimestampWorkArea.IefTimestamp =
          entities.InterstateRequest.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          local.BatchTimestampWorkArea.TextTimestamp;
        ExitState = "ACO_NN0000_ALL_OK";
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ***************************************************
          // *Write a line to the ERROR RPT.
          // ***************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered updating the Program Checkpoint Restart information.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered performing a Commit.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.CheckpointUpdates.Count = 0;
        local.CheckpointReads.Count = 0;
      }
    }

    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.ProgramProcessingInfo.ParameterList =
      local.BatchTimestampWorkArea.TextTimestamp;
    ExitState = "ACO_NN0000_ALL_OK";
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered updating the Program Processing Information.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered updating the Program Checkpoint Restart information.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    UseSpB703WriteControlsAndClose();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
  }

  private void UseSpB703Housekeeping()
  {
    var useImport = new SpB703Housekeeping.Import();
    var useExport = new SpB703Housekeeping.Export();

    Call(SpB703Housekeeping.Execute, useImport, useExport);

    local.Current.Timestamp = useExport.Current.Timestamp;
    local.LastCommitted.CreatedTimestamp =
      useExport.LastCommitted.CreatedTimestamp;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseSpB703WriteControlsAndClose()
  {
    var useImport = new SpB703WriteControlsAndClose.Import();
    var useExport = new SpB703WriteControlsAndClose.Export();

    useImport.Processed.Count = local.LcontrolTotalLettersSent.Count;
    useImport.ErredData.Count = local.LcontrolTotalErrors.Count;
    useImport.Read.Count = local.LcontrolTotalReads.Count;

    Call(SpB703WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    useImport.SpDocKey.KeyInterstateRequest =
      local.SpDocKey.KeyInterstateRequest;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);
  }

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = local.ProgramProcessingInfo.Name;
    var actionCode = "P";
    var functionalTypeCode = "MSC";
    var actionReasonCode = "GSCAS";
    var actionResolutionDate = local.ProgramProcessingInfo.ProcessDate;

    entities.InterstateRequestHistory.Populated = false;
    Update("CreateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetString(command, "transactionDirect", "");
        db.SetInt32(command, "transactionSerial", 0);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", null);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResDte", actionResolutionDate);
        db.SetNullableString(command, "attachmentIndicat", "");
        db.SetNullableString(command, "note", "");
      });

    entities.InterstateRequestHistory.IntGeneratedId = intGeneratedId;
    entities.InterstateRequestHistory.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.TransactionDirectionInd = "";
    entities.InterstateRequestHistory.TransactionSerialNum = 0;
    entities.InterstateRequestHistory.ActionCode = actionCode;
    entities.InterstateRequestHistory.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateRequestHistory.TransactionDate = null;
    entities.InterstateRequestHistory.ActionReasonCode = actionReasonCode;
    entities.InterstateRequestHistory.ActionResolutionDate =
      actionResolutionDate;
    entities.InterstateRequestHistory.AttachmentIndicator = "";
    entities.InterstateRequestHistory.Note = "";
    entities.InterstateRequestHistory.Populated = true;
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.LastCommitted.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 11);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateRequest.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.InterstateRequest.CasINumber ?? "");
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
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
    /// <summary>A ZdelLocalGroupParticipantsGroup group.</summary>
    [Serializable]
    public class ZdelLocalGroupParticipantsGroup
    {
      /// <summary>
      /// A value of ZdelCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("zdelCsePersonsWorkSet")]
      public CsePersonsWorkSet ZdelCsePersonsWorkSet
      {
        get => zdelCsePersonsWorkSet ??= new();
        set => zdelCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ZdelCaseRole.
      /// </summary>
      [JsonPropertyName("zdelCaseRole")]
      public CaseRole ZdelCaseRole
      {
        get => zdelCaseRole ??= new();
        set => zdelCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet zdelCsePersonsWorkSet;
      private CaseRole zdelCaseRole;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ZdelLocalBlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("zdelLocalBlankCsePersonsWorkSet")]
    public CsePersonsWorkSet ZdelLocalBlankCsePersonsWorkSet
    {
      get => zdelLocalBlankCsePersonsWorkSet ??= new();
      set => zdelLocalBlankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ZdelCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("zdelCsePersonsWorkSet")]
    public CsePersonsWorkSet ZdelCsePersonsWorkSet
    {
      get => zdelCsePersonsWorkSet ??= new();
      set => zdelCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ZdelLocalBlankCaseRole.
    /// </summary>
    [JsonPropertyName("zdelLocalBlankCaseRole")]
    public CaseRole ZdelLocalBlankCaseRole
    {
      get => zdelLocalBlankCaseRole ??= new();
      set => zdelLocalBlankCaseRole = value;
    }

    /// <summary>
    /// A value of ZdelOffice.
    /// </summary>
    [JsonPropertyName("zdelOffice")]
    public Office ZdelOffice
    {
      get => zdelOffice ??= new();
      set => zdelOffice = value;
    }

    /// <summary>
    /// Gets a value of ZdelLocalGroupParticipants.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelLocalGroupParticipantsGroup> ZdelLocalGroupParticipants =>
      zdelLocalGroupParticipants ??= new(ZdelLocalGroupParticipantsGroup.
        Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelLocalGroupParticipants for json serialization.
    /// </summary>
    [JsonPropertyName("zdelLocalGroupParticipants")]
    [Computed]
    public IList<ZdelLocalGroupParticipantsGroup>
      ZdelLocalGroupParticipants_Json
    {
      get => zdelLocalGroupParticipants;
      set => ZdelLocalGroupParticipants.Assign(value);
    }

    /// <summary>
    /// A value of ZdelFips.
    /// </summary>
    [JsonPropertyName("zdelFips")]
    public Fips ZdelFips
    {
      get => zdelFips ??= new();
      set => zdelFips = value;
    }

    /// <summary>
    /// A value of ZdelLocalConverted.
    /// </summary>
    [JsonPropertyName("zdelLocalConverted")]
    public Case1 ZdelLocalConverted
    {
      get => zdelLocalConverted ??= new();
      set => zdelLocalConverted = value;
    }

    /// <summary>
    /// A value of ZdelInterstateRequest.
    /// </summary>
    [JsonPropertyName("zdelInterstateRequest")]
    public InterstateRequest ZdelInterstateRequest
    {
      get => zdelInterstateRequest ??= new();
      set => zdelInterstateRequest = value;
    }

    /// <summary>
    /// A value of ZdelLocalBlankInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("zdelLocalBlankInterstateContactAddress")]
    public InterstateContactAddress ZdelLocalBlankInterstateContactAddress
    {
      get => zdelLocalBlankInterstateContactAddress ??= new();
      set => zdelLocalBlankInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("zdelInterstateContactAddress")]
    public InterstateContactAddress ZdelInterstateContactAddress
    {
      get => zdelInterstateContactAddress ??= new();
      set => zdelInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContact.
    /// </summary>
    [JsonPropertyName("zdelInterstateContact")]
    public InterstateContact ZdelInterstateContact
    {
      get => zdelInterstateContact ??= new();
      set => zdelInterstateContact = value;
    }

    /// <summary>
    /// A value of ZdelLocalBlankInterstateContact.
    /// </summary>
    [JsonPropertyName("zdelLocalBlankInterstateContact")]
    public InterstateContact ZdelLocalBlankInterstateContact
    {
      get => zdelLocalBlankInterstateContact ??= new();
      set => zdelLocalBlankInterstateContact = value;
    }

    /// <summary>
    /// A value of ProcessParameter.
    /// </summary>
    [JsonPropertyName("processParameter")]
    public BatchTimestampWorkArea ProcessParameter
    {
      get => processParameter ??= new();
      set => processParameter = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of LastCommitted.
    /// </summary>
    [JsonPropertyName("lastCommitted")]
    public InterstateRequest LastCommitted
    {
      get => lastCommitted ??= new();
      set => lastCommitted = value;
    }

    /// <summary>
    /// A value of LcontrolTotalLettersSent.
    /// </summary>
    [JsonPropertyName("lcontrolTotalLettersSent")]
    public Common LcontrolTotalLettersSent
    {
      get => lcontrolTotalLettersSent ??= new();
      set => lcontrolTotalLettersSent = value;
    }

    /// <summary>
    /// A value of LcontrolTotalErrors.
    /// </summary>
    [JsonPropertyName("lcontrolTotalErrors")]
    public Common LcontrolTotalErrors
    {
      get => lcontrolTotalErrors ??= new();
      set => lcontrolTotalErrors = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of LcontrolTotalReads.
    /// </summary>
    [JsonPropertyName("lcontrolTotalReads")]
    public Common LcontrolTotalReads
    {
      get => lcontrolTotalReads ??= new();
      set => lcontrolTotalReads = value;
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
    /// A value of CheckpointUpdates.
    /// </summary>
    [JsonPropertyName("checkpointUpdates")]
    public Common CheckpointUpdates
    {
      get => checkpointUpdates ??= new();
      set => checkpointUpdates = value;
    }

    /// <summary>
    /// A value of CheckpointReads.
    /// </summary>
    [JsonPropertyName("checkpointReads")]
    public Common CheckpointReads
    {
      get => checkpointReads ??= new();
      set => checkpointReads = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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

    private EabConvertNumeric2 eabConvertNumeric;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private Document document;
    private SpDocKey spDocKey;
    private WorkArea workArea;
    private CsePersonsWorkSet zdelLocalBlankCsePersonsWorkSet;
    private AbendData abendData;
    private CsePersonsWorkSet zdelCsePersonsWorkSet;
    private CaseRole zdelLocalBlankCaseRole;
    private Office zdelOffice;
    private Array<ZdelLocalGroupParticipantsGroup> zdelLocalGroupParticipants;
    private Fips zdelFips;
    private Case1 zdelLocalConverted;
    private InterstateRequest zdelInterstateRequest;
    private InterstateContactAddress zdelLocalBlankInterstateContactAddress;
    private InterstateContactAddress zdelInterstateContactAddress;
    private InterstateContact zdelInterstateContact;
    private InterstateContact zdelLocalBlankInterstateContact;
    private BatchTimestampWorkArea processParameter;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private InterstateRequest lastCommitted;
    private Common lcontrolTotalLettersSent;
    private Common lcontrolTotalErrors;
    private Common writeError;
    private Common lcontrolTotalReads;
    private External external;
    private Common checkpointUpdates;
    private Common checkpointReads;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of ZdelCsePerson.
    /// </summary>
    [JsonPropertyName("zdelCsePerson")]
    public CsePerson ZdelCsePerson
    {
      get => zdelCsePerson ??= new();
      set => zdelCsePerson = value;
    }

    /// <summary>
    /// A value of ZdelCaseRole.
    /// </summary>
    [JsonPropertyName("zdelCaseRole")]
    public CaseRole ZdelCaseRole
    {
      get => zdelCaseRole ??= new();
      set => zdelCaseRole = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("zdelInterstateContactAddress")]
    public InterstateContactAddress ZdelInterstateContactAddress
    {
      get => zdelInterstateContactAddress ??= new();
      set => zdelInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of ZdelInterstateContact.
    /// </summary>
    [JsonPropertyName("zdelInterstateContact")]
    public InterstateContact ZdelInterstateContact
    {
      get => zdelInterstateContact ??= new();
      set => zdelInterstateContact = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private Office office;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CsePerson zdelCsePerson;
    private CaseRole zdelCaseRole;
    private InterstateContactAddress zdelInterstateContactAddress;
    private InterstateContact zdelInterstateContact;
  }
#endregion
}
