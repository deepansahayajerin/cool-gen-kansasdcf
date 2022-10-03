// Program: OE_B429_PROCESS_FIDM_PURGE, ID: 371024506, model: 746.
// Short name: SWEE429B
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
/// A program: OE_B429_PROCESS_FIDM_PURGE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB429ProcessFidmPurge: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B429_PROCESS_FIDM_PURGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB429ProcessFidmPurge(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB429ProcessFidmPurge.
  /// </summary>
  public OeB429ProcessFidmPurge(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  ---------------------
    // 12/13/00  C. Scroggins/S. Newman  WR# 000223 New FIDM Purge Batch
    // ******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ErrorFound.Flag = "N";
    local.HardcodeClose.FileInstruction = "CLOSE";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeWrite.FileInstruction = "WRITE";

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // ***** Call external to open the output file.
    }
    else
    {
      // * * * * * * * * * *
      // SAVE the Exit_State message
      // * * * * * * * * * *
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ErrorFound.Flag = "Y";
    }

    // ******************************************************************
    // If NO Error found -- Process the Records
    // ******************************************************************
    if (AsChar(local.ErrorFound.Flag) != 'Y')
    {
      // ***** Process the selected records in groups based upon the commit
      // frequencies.  Do a DB2 commit at the end of each group.
      local.TotalNumRecordsDeleted.Count = 0;
      local.DateRangeBeginning.CreatedTimestamp = Now().AddMonths(-3);

      foreach(var item in ReadFinancialInstitutionDataMatch())
      {
        DeleteFinancialInstitutionDataMatch();
        ++local.TotalNumRecordsDeleted.Count;
      }
    }

    // ---------------------------------------------
    // After all processing has completed
    // Print the control total Report which
    // will reflect the total creates.
    // ---------------------------------------------
    // * * * * * * * * * *
    // 01/02/99  SWSRPDP Changes to meet DIR Batch Report Standards.
    // * * * * * * * * * *
    // OPEN the CONTROL REPORT
    // * * * * * * * * * *
    local.ReportHandling.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandling.ProgramName = "SWEEB429";
    local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // WRITE the CONTROL REPORT -- Total Count of Records Deleted
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of Records Deleted................." + NumberToString
      (local.TotalNumRecordsDeleted.Count, 15);
    local.ReportHandling.ProgramName = "SWEEB429";
    local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // CLOSE the CONTROL REPORT
    // * * * * * * * * * *
    local.ReportHandling.ProgramName = "SWEEB429";
    local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    // Process ERROR Report
    if (AsChar(local.ErrorFound.Flag) == 'Y')
    {
      // * * * * * * * * * *
      // OPEN the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportHandling.ProgramName = "SWEEB429";
      local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = " " + local.ExitStateWorkArea.Message;
      local.ReportHandling.ProgramName = "SWEEB429";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // CLOSE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProgramName = "SWEEB429";
      local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
        // A Program ERROR Occured to get HERE
        // -- ALL Errors have been written to Reports and files closed
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void DeleteFinancialInstitutionDataMatch()
  {
    Update("DeleteFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetString(
          command, "cseNumber",
          entities.FinancialInstitutionDataMatch.CsePersonNumber);
        db.SetString(
          command, "institutionTin",
          entities.FinancialInstitutionDataMatch.InstitutionTin);
        db.SetString(
          command, "matchPayAcctNum",
          entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber);
        db.SetString(
          command, "matchRunDate",
          entities.FinancialInstitutionDataMatch.MatchRunDate);
        db.SetString(
          command, "accountType",
          entities.FinancialInstitutionDataMatch.AccountType);
      });
  }

  private IEnumerable<bool> ReadFinancialInstitutionDataMatch()
  {
    entities.FinancialInstitutionDataMatch.Populated = false;

    return ReadEach("ReadFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "createdTimestamp",
          local.DateRangeBeginning.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 4);
        entities.FinancialInstitutionDataMatch.CreatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.FinancialInstitutionDataMatch.Populated = true;

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
    /// A value of DateRangeBeginning.
    /// </summary>
    [JsonPropertyName("dateRangeBeginning")]
    public FinancialInstitutionDataMatch DateRangeBeginning
    {
      get => dateRangeBeginning ??= new();
      set => dateRangeBeginning = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportProcessing.
    /// </summary>
    [JsonPropertyName("reportProcessing")]
    public EabFileHandling ReportProcessing
    {
      get => reportProcessing ??= new();
      set => reportProcessing = value;
    }

    /// <summary>
    /// A value of ReportHandling.
    /// </summary>
    [JsonPropertyName("reportHandling")]
    public EabReportSend ReportHandling
    {
      get => reportHandling ??= new();
      set => reportHandling = value;
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
    /// A value of TotalNumRecordsDeleted.
    /// </summary>
    [JsonPropertyName("totalNumRecordsDeleted")]
    public Common TotalNumRecordsDeleted
    {
      get => totalNumRecordsDeleted ??= new();
      set => totalNumRecordsDeleted = value;
    }

    /// <summary>
    /// A value of HardcodeOpen.
    /// </summary>
    [JsonPropertyName("hardcodeOpen")]
    public External HardcodeOpen
    {
      get => hardcodeOpen ??= new();
      set => hardcodeOpen = value;
    }

    /// <summary>
    /// A value of HardcodeClose.
    /// </summary>
    [JsonPropertyName("hardcodeClose")]
    public External HardcodeClose
    {
      get => hardcodeClose ??= new();
      set => hardcodeClose = value;
    }

    /// <summary>
    /// A value of HardcodeWrite.
    /// </summary>
    [JsonPropertyName("hardcodeWrite")]
    public External HardcodeWrite
    {
      get => hardcodeWrite ??= new();
      set => hardcodeWrite = value;
    }

    /// <summary>
    /// A value of ErrorFileOpen.
    /// </summary>
    [JsonPropertyName("errorFileOpen")]
    public Common ErrorFileOpen
    {
      get => errorFileOpen ??= new();
      set => errorFileOpen = value;
    }

    private FinancialInstitutionDataMatch dateRangeBeginning;
    private Common errorFound;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling reportProcessing;
    private EabReportSend reportHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private Common totalNumRecordsDeleted;
    private External hardcodeOpen;
    private External hardcodeClose;
    private External hardcodeWrite;
    private Common errorFileOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }
#endregion
}
