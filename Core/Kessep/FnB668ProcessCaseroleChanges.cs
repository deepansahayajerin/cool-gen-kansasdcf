// Program: FN_B668_PROCESS_CASEROLE_CHANGES, ID: 372275390, model: 746.
// Short name: SWEF668B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B668_PROCESS_CASEROLE_CHANGES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB668ProcessCaseroleChanges: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B668_PROCESS_CASEROLE_CHANGES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB668ProcessCaseroleChanges(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB668ProcessCaseroleChanges.
  /// </summary>
  public FnB668ProcessCaseroleChanges(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ----------  ---------  ---------  
    // -------------------------------------
    // ***********************************************************************
    // ---------------------------------------------
    // This batch procedure reverses all the (auto and manual) collections of 
    // the children in the affected case posted after the AR case role (instead
    // of just marking the collection as adjusted)
    // FOR EACH unprocessed expired/invalidated AR Case Role Change
    //    READ EACH subsequent unadjusted COLLECTION
    //         -   of the children active in that case on the change effective 
    // date
    //         -   posted on/after that change effective date
    // 	-   Auto or Manual dist
    //       Reverse that unadjusted collection
    // This eliminates Collection Adjustment Procedure 2 (SWEFB633).
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramStart.Timestamp = Now();
    local.ReportNeeded.Flag = "Y";
    UseFnB668Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE") || IsExitState
        ("PROGRAM_PROCESSING_INFO_NF"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        // **********************************************************
        // WRITE TO ERROR REPORT 99
        // **********************************************************
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport();
      }
    }
    else
    {
      local.Collection.CollectionAdjustmentReasonTxt =
        local.CaseRoleChange.Description ?? "";
      local.EabFileHandling.Action = "WRITE";

      // **** Since this AR is no longer the current AR in the above case, we 
      // need to see if this AR was a supported person. If so, Back off all
      // collections from the close_dt of the case role. If the AR_INVALID_IND =
      // 'Y' for the above AR, all collections need to be backed off ****
      // ---------------------------------------------
      // Person Program treats the end date as included in the period. So when 
      // AR is changed, reverse all the collections AFTER that date. When an AR
      // is invalid, reverse all the collections ON OR AFTER that date.
      // This batch procedure expects the AR role that has been expired to have 
      // the Case Role Change Processed Indicator (and not the new AR -  because
      // the new AR would not have any disbursements paid to her. We want to
      // reverse any disbursements paid already to the discontinued AR).
      // ---------------------------------------------
      // **** read the cases for which the AR has changed *****
      if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
      {
        foreach(var item in ReadCaseCaseRoleCsePerson1())
        {
          if (AsChar(entities.CaseRole.ArInvalidInd) == 'Y')
          {
            local.Collection.CollectionAdjustmentDt =
              entities.CaseRole.StartDate;
          }
          else
          {
            local.Collection.CollectionAdjustmentDt =
              AddDays(entities.CaseRole.EndDate, 1);
          }

          if (AsChar(local.ReportNeeded.Flag) == 'Y')
          {
            local.FormatDate.Text10 =
              NumberToString(Month(local.Collection.CollectionAdjustmentDt), 14,
              2) + "-" + NumberToString
              (Day(local.Collection.CollectionAdjustmentDt), 14, 2) + "-" + NumberToString
              (Year(local.Collection.CollectionAdjustmentDt), 12, 4);

            // **********************************************************
            // WRITE TO BUSINESS REPORT 01
            // **********************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "";
            UseCabBusinessReport01();
            local.EabReportSend.RptDetail = "CASE ROLE " + "  " + entities
              .Obligee.Number + "   " + local.FormatDate.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();
          }

          // * Read all the supported persons in the case for which the AR has 
          // changed
          foreach(var item1 in ReadCsePerson())
          {
            // --- If a manually distributed collection is not yet disbursed, 
            // don't reverse it. Leave it as it is.
            // If the collection was received in advance, then select the 
            // distributed debts with due date later than the case role changed
            // date.
            UseFnCabReplicateCollections();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              local.EabReportSend.RptDetail =
                TrimEnd(local.ExitStateWorkArea.Message) + "   Obligee number = " +
                entities.Obligee.Number;
              UseCabErrorReport();

              goto Test;
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // **** All processing for that AR is complete. Update the
            // case role ar_chg_processed_date = business process date ****
            ++local.CaseRoleChanges.Count;
            UseFnMarkArAsProcessed();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              goto Test;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            goto Test;
          }
          else if (local.NoOfIncrementalUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            // ***** Call an external that does a DB2 commit using a Cobol 
            // program.
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to commit a unit of work.  Obligor number = " +
                entities.Obligee.Number;
              UseCabErrorReport();
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              goto Test;
            }
          }
        }
      }
      else
      {
        // **** add the multi thread processing here *****
        foreach(var item in ReadCaseCaseRoleCsePerson2())
        {
          if (AsChar(entities.CaseRole.ArInvalidInd) == 'Y')
          {
            local.Collection.CollectionAdjustmentDt =
              entities.CaseRole.StartDate;
          }
          else
          {
            local.Collection.CollectionAdjustmentDt =
              AddDays(entities.CaseRole.EndDate, 1);
          }

          if (AsChar(local.ReportNeeded.Flag) == 'Y')
          {
            local.FormatDate.Text10 =
              NumberToString(Month(local.Collection.CollectionAdjustmentDt), 14,
              2) + "-" + NumberToString
              (Day(local.Collection.CollectionAdjustmentDt), 14, 2) + "-" + NumberToString
              (Year(local.Collection.CollectionAdjustmentDt), 12, 4);

            // **********************************************************
            // WRITE TO BUSINESS REPORT 01
            // **********************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "";
            UseCabBusinessReport01();
            local.EabReportSend.RptDetail = "CASE ROLE " + "  " + entities
              .Obligee.Number + "   " + local.FormatDate.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();
          }

          // * Read all the supported persons in the case for which the AR has 
          // changed
          foreach(var item1 in ReadCsePerson())
          {
            // --- If a manually distributed collection is not yet disbursed, 
            // don't reverse it. Leave it as it is.
            // If the collection was received in advance, then select the 
            // distributed debts with due date later than the case role changed
            // date.
            UseFnCabReplicateCollections();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              local.EabReportSend.RptDetail =
                TrimEnd(local.ExitStateWorkArea.Message) + "   Obligee number = " +
                entities.Obligee.Number;
              UseCabErrorReport();

              goto Test;
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // **** All processing for that AR is complete. Update the
            // case role ar_chg_processed_date = business process date ****
            ++local.CaseRoleChanges.Count;
            UseFnMarkArAsProcessed();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              goto Test;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            goto Test;
          }
          else if (local.NoOfIncrementalUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            // ***** Call an external that does a DB2 commit using a Cobol 
            // program.
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to commit a unit of work.  Obligor number = " +
                entities.Obligee.Number;
              UseCabErrorReport();
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              goto Test;
            }
          }
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB668Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB668Close();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.ArChgProcReqInd = source.ArChgProcReqInd;
    target.ArChgProcessedDate = source.ArChgProcessedDate;
    target.ArInvalidInd = source.ArInvalidInd;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB668Close()
  {
    var useImport = new FnB668Close.Import();
    var useExport = new FnB668Close.Export();

    useImport.CaseRoleChanges.Count = local.CaseRoleChanges.Count;
    useImport.NoCollectionsReplicated.Count =
      local.NoCollectionsReplicated.Count;

    Call(FnB668Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB668Housekeeping()
  {
    var useImport = new FnB668Housekeeping.Import();
    var useExport = new FnB668Housekeeping.Export();

    Call(FnB668Housekeeping.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.Max.Date;
    MoveProgramProcessingInfo2(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.CaseRoleChange.Assign(useExport.CaseRoleChgAdjustment);
  }

  private void UseFnCabReplicateCollections()
  {
    var useImport = new FnCabReplicateCollections.Import();
    var useExport = new FnCabReplicateCollections.Export();

    useImport.Obligee.Number = entities.Obligee.Number;
    useImport.Supported.Assign(entities.Supported);
    useImport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useImport.NoCollectionsReplicated.Count =
      local.NoCollectionsReplicated.Count;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.CaseRoleChange.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.Max.Date = local.MaxDate.Date;
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useExport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useExport.NoCollectionsReplicated.Count =
      local.NoCollectionsReplicated.Count;

    Call(FnCabReplicateCollections.Execute, useImport, useExport);

    local.NoOfIncrementalUpdates.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCollectionsReplicated.Count =
      useImport.NoCollectionsReplicated.Count;
    local.NoOfIncrementalUpdates.Count = useExport.NoOfIncrementalUpdates.Count;
    local.NoCollectionsReplicated.Count =
      useExport.NoCollectionsReplicated.Count;
  }

  private void UseFnMarkArAsProcessed()
  {
    var useImport = new FnMarkArAsProcessed.Import();
    var useExport = new FnMarkArAsProcessed.Export();

    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Persistent.Assign(entities.CaseRole);

    Call(FnMarkArAsProcessed.Execute, useImport, useExport);

    MoveCaseRole(useImport.Persistent, entities.CaseRole);
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson1()
  {
    entities.Obligee.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.PpiParameter.ObligorPersonNumber ?? "");
        db.SetNullableDate(
          command, "arChgProcDt", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.Obligee.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 8);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 10);
        entities.Obligee.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson2()
  {
    entities.Obligee.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "arChgProcDt", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.Obligee.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 8);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 10);
        entities.Obligee.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          local.Collection.CollectionAdjustmentDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.Supported.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of CaseRoleChanges.
    /// </summary>
    [JsonPropertyName("caseRoleChanges")]
    public Common CaseRoleChanges
    {
      get => caseRoleChanges ??= new();
      set => caseRoleChanges = value;
    }

    /// <summary>
    /// A value of NoCollectionsReplicated.
    /// </summary>
    [JsonPropertyName("noCollectionsReplicated")]
    public Common NoCollectionsReplicated
    {
      get => noCollectionsReplicated ??= new();
      set => noCollectionsReplicated = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CaseRoleChange.
    /// </summary>
    [JsonPropertyName("caseRoleChange")]
    public CollectionAdjustmentReason CaseRoleChange
    {
      get => caseRoleChange ??= new();
      set => caseRoleChange = value;
    }

    /// <summary>
    /// A value of UpdatesSinceLastCommit.
    /// </summary>
    [JsonPropertyName("updatesSinceLastCommit")]
    public Common UpdatesSinceLastCommit
    {
      get => updatesSinceLastCommit ??= new();
      set => updatesSinceLastCommit = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      caseRoleChanges = null;
      noCollectionsReplicated = null;
      noOfIncrementalUpdates = null;
      ppiParameter = null;
      eabFileHandling = null;
      maxDate = null;
      zero = null;
      programProcessingInfo = null;
      programCheckpointRestart = null;
      collection = null;
      caseRoleChange = null;
      updatesSinceLastCommit = null;
      passArea = null;
      exitStateWorkArea = null;
      reportNeeded = null;
      formatDate = null;
      programStart = null;
    }

    private Common caseRoleChanges;
    private Common noCollectionsReplicated;
    private Common noOfIncrementalUpdates;
    private CashReceiptDetail ppiParameter;
    private EabFileHandling eabFileHandling;
    private DateWorkArea maxDate;
    private DateWorkArea zero;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Collection collection;
    private CollectionAdjustmentReason caseRoleChange;
    private Common updatesSinceLastCommit;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private Common reportNeeded;
    private EabReportSend eabReportSend;
    private TextWorkArea formatDate;
    private DateWorkArea programStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

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

    private CsePerson obligee;
    private CsePerson supported;
    private CsePersonAccount csePersonAccount;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
