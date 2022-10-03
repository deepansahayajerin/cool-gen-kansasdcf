// Program: OE_B417_HOUSEKEEPING, ID: 374565565, model: 746.
// Short name: SWE00115
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B417_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB417Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B417_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB417Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB417Housekeeping.
  /// </summary>
  public OeB417Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    //  Get the run parameters for this program.
    // *****************************************************************
    export.ProgramProcessingInfo.Name = "SWEEB417";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProcessingDate.Date = export.ProgramProcessingInfo.ProcessDate;
    export.CurrentDateLessFive.Date =
      AddDays(export.ProgramProcessingInfo.ProcessDate, -5);

    // *****************************************************************
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98 .
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB417";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Eab.FileInstruction = "OPEN";

    // **************************************************************************************
    // The below mentioned External Action Block (EAB) will read the input 
    // records from the
    // FCR Master Dataset to load the data to FCR DB2 master tables(Case & Case 
    // Member Table).
    // **************************************************************************************
    UseOeEabReadFcrMasterRecord();

    if (!IsEmpty(local.Eab.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error in opening the FCR_MASTER file";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **************************************************************************************
    // Read the program checkpoint restart records for the execution program and
    // return the
    // values to the Pstep to handle the restart point.
    // **************************************************************************************
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
    target.TextLine8 = source.TextLine8;
  }

  private static void MoveFcrMasterCaseRecord(FcrMasterCaseRecord source,
    FcrMasterCaseRecord target)
  {
    target.CaseId = source.CaseId;
    target.OrderIndicator = source.OrderIndicator;
    target.ActionTypeCode = source.ActionTypeCode;
    target.BatchNumber = source.BatchNumber;
    target.FipsCountyCode = source.FipsCountyCode;
    target.PreviousCaseId = source.PreviousCaseId;
    target.CaseSentDateToFcr = source.CaseSentDateToFcr;
    target.FcrCaseComments = source.FcrCaseComments;
    target.FcrCaseResponseDate = source.FcrCaseResponseDate;
    target.AcknowlegementCode = source.AcknowlegementCode;
    target.ErrorCode1 = source.ErrorCode1;
    target.ErrorCode2 = source.ErrorCode2;
    target.ErrorCode3 = source.ErrorCode3;
    target.ErrorCode4 = source.ErrorCode4;
    target.ErrorCode5 = source.ErrorCode5;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.RecordIdentifier = source.RecordIdentifier;
  }

  private static void MoveFcrMasterMemberRecord(FcrMasterMemberRecord source,
    FcrMasterMemberRecord target)
  {
    target.CaseId = source.CaseId;
    target.MemberId = source.MemberId;
    target.ActionTypeCode = source.ActionTypeCode;
    target.LocateRequestType = source.LocateRequestType;
    target.RecordIdentifier = source.RecordIdentifier;
    target.ParticipantType = source.ParticipantType;
    target.SexCode = source.SexCode;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.FipsCountyCode = source.FipsCountyCode;
    target.FamilyViolence = source.FamilyViolence;
    target.PreviousSsn = source.PreviousSsn;
    target.CityOfBirth = source.CityOfBirth;
    target.StateOrCountryOfBirth = source.StateOrCountryOfBirth;
    target.FathersFirstName = source.FathersFirstName;
    target.FathersMiddleInitial = source.FathersMiddleInitial;
    target.FathersLastName = source.FathersLastName;
    target.MothersFirstName = source.MothersFirstName;
    target.MothersMiddleInitial = source.MothersMiddleInitial;
    target.MothersMaidenNm = source.MothersMaidenNm;
    target.IrsUSsn = source.IrsUSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.NewMemberId = source.NewMemberId;
    target.Irs1099 = source.Irs1099;
    target.LocateSource1 = source.LocateSource1;
    target.LocateSource2 = source.LocateSource2;
    target.LocateSource3 = source.LocateSource3;
    target.LocateSource4 = source.LocateSource4;
    target.LocateSource5 = source.LocateSource5;
    target.LocateSource6 = source.LocateSource6;
    target.LocateSource7 = source.LocateSource7;
    target.LocateSource8 = source.LocateSource8;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.MultipleSsn1 = source.MultipleSsn1;
    target.MultipleSsn2 = source.MultipleSsn2;
    target.MultipleSsn3 = source.MultipleSsn3;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.BatchNumber = source.BatchNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.FcrPrimarySsn = source.FcrPrimarySsn;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
    target.AcknowledgementCode = source.AcknowledgementCode;
    target.ErrorCode1 = source.ErrorCode1;
    target.ErrorCode2 = source.ErrorCode2;
    target.ErrorCode3 = source.ErrorCode3;
    target.ErrorCode4 = source.ErrorCode4;
    target.ErrorCode5 = source.ErrorCode5;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
    target.BundleFplsLocateResults = source.BundleFplsLocateResults;
    target.SsaCityOfLastResidence = source.SsaCityOfLastResidence;
    target.SsaStateOfLastResidence = source.SsaStateOfLastResidence;
    target.SsaCityOfLumpSumPayment = source.SsaCityOfLumpSumPayment;
    target.SsaStateOfLumpSumPayment = source.SsaStateOfLumpSumPayment;
    target.LastSentDtToFcr = source.LastSentDtToFcr;
    target.LastReceivedDtFromFcr = source.LastReceivedDtFromFcr;
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

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabReadFcrMasterRecord()
  {
    var useImport = new OeEabReadFcrMasterRecord.Import();
    var useExport = new OeEabReadFcrMasterRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);
    MoveFcrMasterMemberRecord(local.FcrMasterMemberRecord,
      useExport.FcrMasterMemberRecord);
    MoveFcrMasterCaseRecord(local.FcrMasterCaseRecord,
      useExport.FcrMasterCaseRecord);

    Call(OeEabReadFcrMasterRecord.Execute, useImport, useExport);

    local.Eab.Assign(useExport.ExternalFileStatus);
    local.FcrMasterMemberRecord.Assign(useExport.FcrMasterMemberRecord);
    local.FcrMasterCaseRecord.Assign(useExport.FcrMasterCaseRecord);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    /// <summary>
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
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

    private DateWorkArea processingDate;
    private DateWorkArea currentDateLessFive;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of FcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterMemberRecord")]
    public FcrMasterMemberRecord FcrMasterMemberRecord
    {
      get => fcrMasterMemberRecord ??= new();
      set => fcrMasterMemberRecord = value;
    }

    /// <summary>
    /// A value of FcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterCaseRecord")]
    public FcrMasterCaseRecord FcrMasterCaseRecord
    {
      get => fcrMasterCaseRecord ??= new();
      set => fcrMasterCaseRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External eab;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
    private FcrMasterCaseRecord fcrMasterCaseRecord;
  }
#endregion
}
