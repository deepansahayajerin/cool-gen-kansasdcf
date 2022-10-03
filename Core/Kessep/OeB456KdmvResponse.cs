// Program: OE_B456_KDMV_RESPONSE, ID: 371367230, model: 746.
// Short name: SWEE456B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B456_KDMV_RESPONSE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB456KdmvResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B456_KDMV_RESPONSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB456KdmvResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB456KdmvResponse.
  /// </summary>
  public OeB456KdmvResponse(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 07/24/2006      DDupree   	Initial Creation
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB456Housekeeping();
    local.TotalProcessed.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 15);
    local.TotalCseLicense.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 15);
    local.TotalKsDriversLicense.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 31, 15);
    local.TotalErrors.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 46, 15);
    local.CsePersonLicenseUpdated.Count =
      (int)StringToNumber(local.TotalCseLicense.Text15);
    local.KdmvDriverLicenseUpdate.Count =
      (int)StringToNumber(local.TotalKsDriversLicense.Text15);
    local.TotalErrorRecords.Count =
      (int)StringToNumber(local.TotalErrors.Text15);
    local.LastUpdatedCsePerLic.Count = local.CsePersonLicenseUpdated.Count;
    local.LastUpdatedKdmvDrvLic.Count = local.KdmvDriverLicenseUpdate.Count;
    local.LastUpdatedTotalError.Count = local.TotalErrorRecords.Count;
    local.LastUpdatedTotalProcess.Count =
      (int)StringToNumber(local.TotalProcessed.Text15);

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // since we did not account for the last error because of the restart we 
      // will added it in now.
      ++local.TotalErrorRecords.Count;
      local.LastUpdatedTotalError.Count = local.TotalErrorRecords.Count;
    }

    local.BatchTimestampWorkArea.IefTimestamp = new DateTime(1, 1, 1);
    ReadKsDriversLicense();
    local.RestartCount.Count = 0;
    local.TotalRecordsProcessed.Count = local.LastUpdatedTotalProcess.Count;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    do
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // ************************************************
      // *Call external to READ the driver file.        *
      // ************************************************
      local.PassArea.FileInstruction = "READ";
      UseOeEabKdmvResponse1();

      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        break;
      }

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      if (local.RestartCount.Count < local.TotalRecordsProcessed.Count && local
        .TotalRecordsProcessed.Count > 0)
      {
        ++local.RestartCount.Count;

        continue;
      }

      UseOeB456ProcessKdmvResponse();
      ++local.TotalRecordsProcessed.Count;
      local.RestartCount.Count = local.TotalRecordsProcessed.Count;
      ++local.ProgramRestart.Count;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ************************************************
        // *Check the number of reads, and updates that   *
        // *have occurred since the last checkpoint.      *
        // ************************************************
        if (local.ProgramRestart.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.ProgramName =
            local.ProgramProcessingInfo.Name;

          // we have to take off one from the restart point since it will add 
          // one in the update
          // cab, if we did not take one off then it would let the miss 
          // processing some records
          local.ProgramCheckpointRestart.CheckpointCount =
            local.TotalRecordsProcessed.Count - 1;
          local.TotalProcessed.Text15 =
            NumberToString(local.TotalRecordsProcessed.Count, 15);
          local.TotalCseLicense.Text15 =
            NumberToString(local.CsePersonLicenseUpdated.Count, 15);
          local.TotalKsDriversLicense.Text15 =
            NumberToString(local.KdmvDriverLicenseUpdate.Count, 15);
          local.TotalErrors.Text15 =
            NumberToString(local.TotalErrorRecords.Count, 15);
          local.ProgramCheckpointRestart.RestartInfo =
            local.TotalProcessed.Text15 + local.TotalCseLicense.Text15 + local
            .TotalKsDriversLicense.Text15 + local.TotalErrors.Text15;
          UseUpdatePgmCheckpointRestart();
          local.LastUpdatedCsePerLic.Count =
            local.CsePersonLicenseUpdated.Count;
          local.LastUpdatedKdmvDrvLic.Count =
            local.KdmvDriverLicenseUpdate.Count;
          local.LastUpdatedTotalError.Count = local.TotalErrorRecords.Count;
          local.LastUpdatedTotalProcess.Count =
            local.TotalRecordsProcessed.Count;
          local.ProgramRestart.Count = 0;

          // ************************************************
          // *Call an external that does a DB2 commit using *
          // *a Cobol program.
          // 
          // *
          // ************************************************
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Commit Taken: " + NumberToString
            (TimeToInt(Time(Now())), 15);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }
      else
      {
        break;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.NonMatchRecsFromKdmv.Count = 0;

      do
      {
        local.PassArea.Assign(local.Clear);
        local.PassArea.FileInstruction = "READ";
        UseOeEabKdmvResponseNonMatch1();

        if (Equal(local.PassArea.TextReturnCode, "EF"))
        {
          break;
        }

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          break;
        }

        local.Name.Text33 = local.Missmatch.LastName + "   " + local
          .Missmatch.FirstName;
        local.SsnDob.Text23 = "   " + local.Missmatch.Ssn + "   " + local
          .Missmatch.Dob;
        local.PersonNumbDrLicense.Text25 = "   " + local
          .Missmatch.CsePersonNumber + "   " + local
          .Missmatch.DriverLicenseNumber;
        local.Problem.Text40 = "   " + local.Missmatch.RequestStatus + "   " + local
          .Missmatch.DmvProblemText;
        local.MismatchReportSend.RptDetail = local.Name.Text33 + local
          .SsnDob.Text23 + local.PersonNumbDrLicense.Text25 + local
          .Problem.Text40;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          break;
        }

        ++local.NonMatchRecsFromKdmv.Count;
      }
      while(!Equal(local.PassArea.TextReturnCode, "EF"));
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB456Close2();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabKdmvResponse2();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabKdmvResponseNonMatch2();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabBusinessReport01();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB456Close1();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabKdmvResponse2();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabKdmvResponseNonMatch2();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabBusinessReport01();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveKdmvFile(KdmvFile source, KdmvFile target)
  {
    target.FileType = source.FileType;
    target.CsePersonNumber = source.CsePersonNumber;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.Ssn = source.Ssn;
    target.Dob = source.Dob;
    target.DriverLicenseNumber = source.DriverLicenseNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.MismatchReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseOeB456Close1()
  {
    var useImport = new OeB456Close.Import();
    var useExport = new OeB456Close.Export();

    useImport.KdmvDriverLicenseUpdat.Count = local.LastUpdatedKdmvDrvLic.Count;
    useImport.TotalErrorRecords.Count = local.LastUpdatedTotalError.Count;
    useImport.NonMatchRecsFromKdmv.Count = local.NonMatchRecsFromKdmv.Count;
    useImport.CsePersonLicenseUpdate.Count = local.LastUpdatedCsePerLic.Count;
    useImport.TotalRecordsProcessed.Count = local.LastUpdatedTotalProcess.Count;

    Call(OeB456Close.Execute, useImport, useExport);
  }

  private void UseOeB456Close2()
  {
    var useImport = new OeB456Close.Import();
    var useExport = new OeB456Close.Export();

    useImport.KdmvDriverLicenseUpdat.Count =
      local.KdmvDriverLicenseUpdate.Count;
    useImport.TotalErrorRecords.Count = local.TotalErrorRecords.Count;
    useImport.NonMatchRecsFromKdmv.Count = local.NonMatchRecsFromKdmv.Count;
    useImport.CsePersonLicenseUpdate.Count =
      local.CsePersonLicenseUpdated.Count;
    useImport.TotalRecordsProcessed.Count = local.TotalRecordsProcessed.Count;

    Call(OeB456Close.Execute, useImport, useExport);
  }

  private void UseOeB456Housekeeping()
  {
    var useImport = new OeB456Housekeeping.Import();
    var useExport = new OeB456Housekeeping.Export();

    Call(OeB456Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeB456ProcessKdmvResponse()
  {
    var useImport = new OeB456ProcessKdmvResponse.Import();
    var useExport = new OeB456ProcessKdmvResponse.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.KdmvFile.Assign(local.Matched);
    useImport.KdmvDriverLicenseUpdat.Count =
      local.KdmvDriverLicenseUpdate.Count;
    useImport.TotalErrorRecords.Count = local.TotalErrorRecords.Count;
    useImport.CsePersonLicenseUpdate.Count =
      local.CsePersonLicenseUpdated.Count;
    useImport.TotalNumberOfRecords.Count = local.TotalNumberOfRecords.Count;

    Call(OeB456ProcessKdmvResponse.Execute, useImport, useExport);

    local.CsePersonLicenseUpdated.Count =
      useExport.CsePersonLicenseUpdate.Count;
    local.TotalErrorRecords.Count = useExport.OuputTotalErrorRecords.Count;
    local.KdmvDriverLicenseUpdate.Count =
      useExport.KdmvDriverLicenseUpdat.Count;
    local.TotalNumberOfRecords.Count = useExport.TotalNumberOfRecords.Count;
  }

  private void UseOeEabKdmvResponse1()
  {
    var useImport = new OeEabKdmvResponse.Import();
    var useExport = new OeEabKdmvResponse.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveKdmvFile(local.Matched, useExport.KdmvFile);
    useExport.External.Assign(local.PassArea);

    Call(OeEabKdmvResponse.Execute, useImport, useExport);

    local.Matched.Assign(useExport.KdmvFile);
    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabKdmvResponse2()
  {
    var useImport = new OeEabKdmvResponse.Import();
    var useExport = new OeEabKdmvResponse.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabKdmvResponse.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabKdmvResponseNonMatch1()
  {
    var useImport = new OeEabKdmvResponseNonMatch.Import();
    var useExport = new OeEabKdmvResponseNonMatch.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.KdmvFile.Assign(local.Missmatch);
    useExport.External.Assign(local.PassArea);

    Call(OeEabKdmvResponseNonMatch.Execute, useImport, useExport);

    local.Missmatch.Assign(useExport.KdmvFile);
    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabKdmvResponseNonMatch2()
  {
    var useImport = new OeEabKdmvResponseNonMatch.Import();
    var useExport = new OeEabKdmvResponseNonMatch.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabKdmvResponseNonMatch.Execute, useImport, useExport);

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

  private bool ReadKsDriversLicense()
  {
    return Read("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          local.BatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.TotalNumberOfRecords.Count = db.GetInt32(reader, 0);
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
    /// <summary>
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
    }

    private ExternalFplsResponse externalFplsResponse;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of TotalNumberOfRecords.
    /// </summary>
    [JsonPropertyName("totalNumberOfRecords")]
    public Common TotalNumberOfRecords
    {
      get => totalNumberOfRecords ??= new();
      set => totalNumberOfRecords = value;
    }

    /// <summary>
    /// A value of LastUpdatedTotalProcess.
    /// </summary>
    [JsonPropertyName("lastUpdatedTotalProcess")]
    public Common LastUpdatedTotalProcess
    {
      get => lastUpdatedTotalProcess ??= new();
      set => lastUpdatedTotalProcess = value;
    }

    /// <summary>
    /// A value of LastUpdatedTotalError.
    /// </summary>
    [JsonPropertyName("lastUpdatedTotalError")]
    public Common LastUpdatedTotalError
    {
      get => lastUpdatedTotalError ??= new();
      set => lastUpdatedTotalError = value;
    }

    /// <summary>
    /// A value of LastUpdatedKdmvDrvLic.
    /// </summary>
    [JsonPropertyName("lastUpdatedKdmvDrvLic")]
    public Common LastUpdatedKdmvDrvLic
    {
      get => lastUpdatedKdmvDrvLic ??= new();
      set => lastUpdatedKdmvDrvLic = value;
    }

    /// <summary>
    /// A value of LastUpdatedCsePerLic.
    /// </summary>
    [JsonPropertyName("lastUpdatedCsePerLic")]
    public Common LastUpdatedCsePerLic
    {
      get => lastUpdatedCsePerLic ??= new();
      set => lastUpdatedCsePerLic = value;
    }

    /// <summary>
    /// A value of ProgramRestart.
    /// </summary>
    [JsonPropertyName("programRestart")]
    public Common ProgramRestart
    {
      get => programRestart ??= new();
      set => programRestart = value;
    }

    /// <summary>
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public WorkArea TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public WorkArea TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of TotalKsDriversLicense.
    /// </summary>
    [JsonPropertyName("totalKsDriversLicense")]
    public WorkArea TotalKsDriversLicense
    {
      get => totalKsDriversLicense ??= new();
      set => totalKsDriversLicense = value;
    }

    /// <summary>
    /// A value of TotalCseLicense.
    /// </summary>
    [JsonPropertyName("totalCseLicense")]
    public WorkArea TotalCseLicense
    {
      get => totalCseLicense ??= new();
      set => totalCseLicense = value;
    }

    /// <summary>
    /// A value of Problem.
    /// </summary>
    [JsonPropertyName("problem")]
    public WorkArea Problem
    {
      get => problem ??= new();
      set => problem = value;
    }

    /// <summary>
    /// A value of PersonNumbDrLicense.
    /// </summary>
    [JsonPropertyName("personNumbDrLicense")]
    public WorkArea PersonNumbDrLicense
    {
      get => personNumbDrLicense ??= new();
      set => personNumbDrLicense = value;
    }

    /// <summary>
    /// A value of SsnDob.
    /// </summary>
    [JsonPropertyName("ssnDob")]
    public WorkArea SsnDob
    {
      get => ssnDob ??= new();
      set => ssnDob = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Missmatch.
    /// </summary>
    [JsonPropertyName("missmatch")]
    public KdmvFile Missmatch
    {
      get => missmatch ??= new();
      set => missmatch = value;
    }

    /// <summary>
    /// A value of MismatchReportSend.
    /// </summary>
    [JsonPropertyName("mismatchReportSend")]
    public EabReportSend MismatchReportSend
    {
      get => mismatchReportSend ??= new();
      set => mismatchReportSend = value;
    }

    /// <summary>
    /// A value of Matched.
    /// </summary>
    [JsonPropertyName("matched")]
    public KdmvFile Matched
    {
      get => matched ??= new();
      set => matched = value;
    }

    /// <summary>
    /// A value of NonMatchRecsFromKdmv.
    /// </summary>
    [JsonPropertyName("nonMatchRecsFromKdmv")]
    public Common NonMatchRecsFromKdmv
    {
      get => nonMatchRecsFromKdmv ??= new();
      set => nonMatchRecsFromKdmv = value;
    }

    /// <summary>
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
    }

    /// <summary>
    /// A value of KdmvDriverLicenseUpdate.
    /// </summary>
    [JsonPropertyName("kdmvDriverLicenseUpdate")]
    public Common KdmvDriverLicenseUpdate
    {
      get => kdmvDriverLicenseUpdate ??= new();
      set => kdmvDriverLicenseUpdate = value;
    }

    /// <summary>
    /// A value of CsePersonLicenseUpdated.
    /// </summary>
    [JsonPropertyName("csePersonLicenseUpdated")]
    public Common CsePersonLicenseUpdated
    {
      get => csePersonLicenseUpdated ??= new();
      set => csePersonLicenseUpdated = value;
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
    /// A value of RestartCount.
    /// </summary>
    [JsonPropertyName("restartCount")]
    public Common RestartCount
    {
      get => restartCount ??= new();
      set => restartCount = value;
    }

    /// <summary>
    /// A value of TotalRecordsProcessed.
    /// </summary>
    [JsonPropertyName("totalRecordsProcessed")]
    public Common TotalRecordsProcessed
    {
      get => totalRecordsProcessed ??= new();
      set => totalRecordsProcessed = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public External Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common totalNumberOfRecords;
    private Common lastUpdatedTotalProcess;
    private Common lastUpdatedTotalError;
    private Common lastUpdatedKdmvDrvLic;
    private Common lastUpdatedCsePerLic;
    private Common programRestart;
    private WorkArea totalProcessed;
    private WorkArea totalErrors;
    private WorkArea totalKsDriversLicense;
    private WorkArea totalCseLicense;
    private WorkArea problem;
    private WorkArea personNumbDrLicense;
    private WorkArea ssnDob;
    private WorkArea name;
    private KdmvFile missmatch;
    private EabReportSend mismatchReportSend;
    private KdmvFile matched;
    private Common nonMatchRecsFromKdmv;
    private Common totalErrorRecords;
    private Common kdmvDriverLicenseUpdate;
    private Common csePersonLicenseUpdated;
    private ExitStateWorkArea exitStateWorkArea;
    private Common restartCount;
    private Common totalRecordsProcessed;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External clear;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    private KsDriversLicense ksDriversLicense;
  }
#endregion
}
