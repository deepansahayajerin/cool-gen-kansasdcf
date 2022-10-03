// Program: OE_B445_PROCESS_SVES_RESPONSES, ID: 374518235, model: 746.
// Short name: SWEE445B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B445_PROCESS_SVES_RESPONSES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB445ProcessSvesResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B445_PROCESS_SVES_RESPONSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB445ProcessSvesResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB445ProcessSvesResponses.
  /// </summary>
  public OeB445ProcessSvesResponses(IContext context, Import import,
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
    // ******************************************************************************************
    // * This batch process reads the SVES responses from FCR this includes 
    // Title-II Pending,   *
    // * Title-II, Title-XVI, Prisoner and
    // Not Found records.
    // 
    // *
    // *
    // 
    // *
    // * During this first phase implementation, the process will process only 
    // Title-II pending *
    // * SVES records and generate required worker alerts & information required
    // for Narrative  *
    // * Detail screen.
    // 
    // *
    // *
    // 
    // *
    // * This process will be extended in the future to process other SVES type 
    // records.        *
    // ******************************************************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 11/12/2009  Raj S              CQ13575     Initial Coding.
    // *
    // *
    // 
    // *
    // * 03/14/2011  Raj S              CQ24511     Modified to adde code to 
    // generate worker *
    // *
    // 
    // alerts for Title-II & Title-XVI          *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB445Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      local.FcrSvesTitleIiPendingClaim.Assign(local.Null1);
      UseEabReadFcrSvesResponseFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING FCR SVES RESPONSE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;

      // ******************************************************************************************
      // * For Phase-I of this services request, select only Title-II pending 
      // claim, Title-II and *
      // * Title-16 SVES response records for processing, this can be identified
      // by checking the  *
      // * locate source response agency code value to 'E04', 'E05 or 'E06'.
      // *
      // *
      // 
      // *
      // * If the read record is NOT Title-II pending claim or Title-II or Title
      // -16 then skip the *
      // * input record from Processing.
      // 
      // *
      // ******************************************************************************************
      if (Equal(local.FcrSvesTitleIiPendingClaim.LocSrcResponseAgencyCode, "E04")
        || Equal
        (local.FcrSvesTitleIiPendingClaim.LocSrcResponseAgencyCode, "E05") || Equal
        (local.FcrSvesTitleIiPendingClaim.LocSrcResponseAgencyCode, "E06"))
      {
        // ******************************************************************************************
        // * Title-II Pending Claim, Title-II & Title-XVI record found Continue 
        // processing,         *
        // ******************************************************************************************
      }
      else
      {
        ++local.RecSkippedNotT2Pending.Count;

        continue;
      }

      local.Kaecses.Number = local.FcrSvesTitleIiPendingClaim.MemberIdentifier;
      ++local.T2endingClaimRecCount.Count;

      if (ReadCsePerson())
      {
        // ******************************************************************************************
        // * Check the FCR SVES Response SSN is in CSE Bad SSN list, if so, skip
        // from processing    *
        // *
        // 
        // *
        // * Check whether SVES response record has Corrected SSN,if so, use 
        // that SSN for           *
        // * Processing.
        // 
        // *
        // ******************************************************************************************
        if (!IsEmpty(local.FcrSvesTitleIiPendingClaim.CorrAdditlMultipleSsn))
        {
          local.ConvertSsn.SsnNum9 =
            (int)StringToNumber(local.FcrSvesTitleIiPendingClaim.
              CorrAdditlMultipleSsn);
        }
        else
        {
          local.ConvertSsn.SsnNum9 =
            (int)StringToNumber(local.FcrSvesTitleIiPendingClaim.Ssn);
        }

        if (ReadInvalidSsn())
        {
          if (!IsEmpty(local.FcrSvesTitleIiPendingClaim.CorrAdditlMultipleSsn))
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrSvesTitleIiPendingClaim.CorrAdditlMultipleSsn,
              1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrSvesTitleIiPendingClaim.CorrAdditlMultipleSsn,
              4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrSvesTitleIiPendingClaim.CorrAdditlMultipleSsn,
              6, 4);
          }
          else
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrSvesTitleIiPendingClaim.Ssn, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrSvesTitleIiPendingClaim.Ssn, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrSvesTitleIiPendingClaim.Ssn, 6, 4);
          }

          local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
            .ConvertMessage.SsnTextPart2 + "-" + local
            .ConvertMessage.SsnTextPart3;
          local.Message1.Text8 = "Bad SSN";
          local.Message1.Text6 = ", Per";
          local.Message1.Text16 = ": Rec not used -";
          local.Message1.Text2 = ",";
          local.Message1.Text1 = "";
          local.Message1.Text80 =
            TrimEnd(local.FcrSvesTitleIiPendingClaim.LocSrcResponseAgencyCode) +
            local.Message1.Text2 + TrimEnd
            (local.FcrSvesTitleIiPendingClaim.DoMailingAddressLine1) + local
            .Message1.Text2 + TrimEnd
            (local.FcrSvesTitleIiPendingClaim.DoMailingAddressCity) + local
            .Message1.Text2 + TrimEnd
            (local.FcrSvesTitleIiPendingClaim.DoMailingAddressState) + local
            .Message1.Text1 + TrimEnd
            (local.FcrSvesTitleIiPendingClaim.DoMainlingAddressZip) + local
            .Message1.Text2 + "Title II Pending Claim Record  ";
          local.NeededToWrite.RptDetail = local.Message1.Text8 + local
            .Message2.Text11 + local.Message1.Text6 + local
            .FcrSvesTitleIiPendingClaim.MemberIdentifier + local
            .Message1.Text16 + local.Message1.Text80;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.TotalBadSsnRecords.Count;
          local.Message1.Text8 = "";
          local.Message1.Text6 = "";
          local.Message1.Text16 = "";
          local.Message1.Text2 = "";
          local.Message1.Text1 = "";
          local.Message1.Text80 = "";
          local.Message2.Text11 = "";
          local.NeededToWrite.RptDetail = "";

          continue;
        }
        else
        {
          // this is fine, there is not invalid ssn record for this combination 
          // of cse person number and ssn number
        }

        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }
      else
      {
        ++local.RecordsPersonNotFound.Count;
        local.NeededToWrite.RptDetail = "Person Not Found - AP # " + local
          .FcrSvesTitleIiPendingClaim.MemberIdentifier + "  " + TrimEnd
          (local.FcrSvesTitleIiPendingClaim.LastName) + ", " + TrimEnd
          (local.FcrSvesTitleIiPendingClaim.FirstName) + " " + "" + local
          .FcrSvesTitleIiPendingClaim.MiddleName;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      // ***************************************************************************
      // Write to report of ssn's don't match.  Skip to next person.
      // ***************************************************************************
      // **************************************************************************************
      // Generate worker alert for mismatched SSNs by using the Action Block
      // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114 Changes Start
      // **************************************************************************************
      local.Infrastructure.EventId = 10;
      local.Infrastructure.ReasonCode = "FCRSVEST2PEND";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.ReferenceDate = local.Process.Date;
      local.Infrastructure.UserId = local.ProgramProcessingInfo.Name;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.BusinessObjectCd = "FCR";
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.LastUpdatedBy = "";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.Detail = "SSN:" + TrimEnd
        (local.FcrSvesTitleIiPendingClaim.Ssn) + " Claim Type: " + local
        .FcrSvesTitleIiPendingClaim.ClaimTypeCode + ", PF16 to view SSA Info (expires in 1 Yr)";
        
      ExitState = "ACO_NN0000_ALL_OK";
      local.NarrativeDetail.NarrativeText =
        "**Title II Pending Claim from SVES thru SWEIB445 Batch Process**";
      UseOeSvesAlertNNarrDtlGen();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // **************************************************************************************
      // Generate worker alert for mismatched SSNs by using the Action Block
      // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114 Changes End
      // **************************************************************************************
      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.Time.Text8 = NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.RecordsRead.Count, 15) + "  Date: " + local.Date.Text10 + "  Time: " +
          local.Time.Text8;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB445Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB445Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabReadFcrSvesResponseFile()
  {
    var useImport = new EabReadFcrSvesResponseFile.Import();
    var useExport = new EabReadFcrSvesResponseFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.SvesTitleIiPending.Assign(local.FcrSvesTitleIiPendingClaim);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFcrSvesResponseFile.Execute, useImport, useExport);

    local.FcrSvesTitleIiPendingClaim.Assign(useExport.SvesTitleIiPending);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB445Close()
  {
    var useImport = new OeB445Close.Import();
    var useExport = new OeB445Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.T2endingClaimRecs.Count = local.T2endingClaimRecCount.Count;
    useImport.TotalHistoryCount.Count = local.TotalHistoryCount.Count;
    useImport.TotalAlertCount.Count = local.TotalAlertCount.Count;
    useImport.DuplicateAlertCount.Count = local.DuplicateAlertCount.Count;
    useImport.NonT2PendingClaimRecs.Count = local.RecSkippedNotT2Pending.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.TotalT2PendSkippedCr.Count = local.TotalT2SkippedCr.Count;
    useImport.TotalBadSsnRecords.Count = local.TotalBadSsnRecords.Count;
    useImport.TotalSvesRecsForAlert.Count = local.TotT2PendForAlrtNHist.Count;

    Call(OeB445Close.Execute, useImport, useExport);
  }

  private void UseOeB445Housekeeping()
  {
    var useImport = new OeB445Housekeeping.Import();
    var useExport = new OeB445Housekeeping.Export();

    Call(OeB445Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeSvesAlertNNarrDtlGen()
  {
    var useImport = new OeSvesAlertNNarrDtlGen.Import();
    var useExport = new OeSvesAlertNNarrDtlGen.Export();

    useImport.TotT2PenForAlrtNHist.Count = local.TotT2PendForAlrtNHist.Count;
    useImport.TotalT2SkippedCr.Count = local.TotalT2SkippedCr.Count;
    useImport.DuplicateAlertCount.Count = local.DuplicateAlertCount.Count;
    useImport.TotalHistoryCount.Count = local.TotalHistoryCount.Count;
    useImport.TotalAlertCount.Count = local.TotalAlertCount.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.SvesTitleIiPendClaim.Assign(local.FcrSvesTitleIiPendingClaim);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.EmployerSourceTxt.NarrativeText =
      local.NarrativeDetail.NarrativeText;
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeSvesAlertNNarrDtlGen.Execute, useImport, useExport);

    local.TotT2PendForAlrtNHist.Count = useExport.TotT2PenForAlrtNHist.Count;
    local.TotalT2SkippedCr.Count = useExport.TotalT2SkippedCr.Count;
    local.DuplicateAlertCount.Count = useExport.DuplicateAlertCount.Count;
    local.TotalHistoryCount.Count = useExport.TotalHistoryCount.Count;
    local.TotalAlertCount.Count = useExport.TotalAlertCount.Count;
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.Kaecses.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Obligor, local.Kaecses);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", local.FcrSvesTitleIiPendingClaim.MemberIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "ssn", local.ConvertSsn.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
    /// A value of TotalHistoryCount.
    /// </summary>
    [JsonPropertyName("totalHistoryCount")]
    public Common TotalHistoryCount
    {
      get => totalHistoryCount ??= new();
      set => totalHistoryCount = value;
    }

    /// <summary>
    /// A value of TotT2PendForAlrtNHist.
    /// </summary>
    [JsonPropertyName("totT2PendForAlrtNHist")]
    public Common TotT2PendForAlrtNHist
    {
      get => totT2PendForAlrtNHist ??= new();
      set => totT2PendForAlrtNHist = value;
    }

    /// <summary>
    /// A value of TotalBadSsnRecords.
    /// </summary>
    [JsonPropertyName("totalBadSsnRecords")]
    public Common TotalBadSsnRecords
    {
      get => totalBadSsnRecords ??= new();
      set => totalBadSsnRecords = value;
    }

    /// <summary>
    /// A value of TotalT2SkippedCr.
    /// </summary>
    [JsonPropertyName("totalT2SkippedCr")]
    public Common TotalT2SkippedCr
    {
      get => totalT2SkippedCr ??= new();
      set => totalT2SkippedCr = value;
    }

    /// <summary>
    /// A value of TotalAlertCount.
    /// </summary>
    [JsonPropertyName("totalAlertCount")]
    public Common TotalAlertCount
    {
      get => totalAlertCount ??= new();
      set => totalAlertCount = value;
    }

    /// <summary>
    /// A value of DuplicateAlertCount.
    /// </summary>
    [JsonPropertyName("duplicateAlertCount")]
    public Common DuplicateAlertCount
    {
      get => duplicateAlertCount ??= new();
      set => duplicateAlertCount = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public FcrSvesTitleIiPendingClaim Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPendingClaim.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPendingClaim")]
    public FcrSvesTitleIiPendingClaim FcrSvesTitleIiPendingClaim
    {
      get => fcrSvesTitleIiPendingClaim ??= new();
      set => fcrSvesTitleIiPendingClaim = value;
    }

    /// <summary>
    /// A value of ConvertSsn.
    /// </summary>
    [JsonPropertyName("convertSsn")]
    public SsnWorkArea ConvertSsn
    {
      get => convertSsn ??= new();
      set => convertSsn = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecSkippedNotT2Pending.
    /// </summary>
    [JsonPropertyName("recSkippedNotT2Pending")]
    public Common RecSkippedNotT2Pending
    {
      get => recSkippedNotT2Pending ??= new();
      set => recSkippedNotT2Pending = value;
    }

    /// <summary>
    /// A value of T2endingClaimRecCount.
    /// </summary>
    [JsonPropertyName("t2endingClaimRecCount")]
    public Common T2endingClaimRecCount
    {
      get => t2endingClaimRecCount ??= new();
      set => t2endingClaimRecCount = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of NarrativeDetailLineCnt.
    /// </summary>
    [JsonPropertyName("narrativeDetailLineCnt")]
    public Common NarrativeDetailLineCnt
    {
      get => narrativeDetailLineCnt ??= new();
      set => narrativeDetailLineCnt = value;
    }

    /// <summary>
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    private Common totalHistoryCount;
    private Common totT2PendForAlrtNHist;
    private Common totalBadSsnRecords;
    private Common totalT2SkippedCr;
    private Common totalAlertCount;
    private Common duplicateAlertCount;
    private FcrSvesTitleIiPendingClaim null1;
    private FcrSvesTitleIiPendingClaim fcrSvesTitleIiPendingClaim;
    private SsnWorkArea convertSsn;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common recordsRead;
    private Common recSkippedNotT2Pending;
    private Common t2endingClaimRecCount;
    private Common recordsPersonNotFound;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private EabReportSend neededToWrite;
    private CsePersonsWorkSet kaecses;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private NarrativeDetail narrativeDetail;
    private Common narrativeDetailLineCnt;
    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
  }
#endregion
}
