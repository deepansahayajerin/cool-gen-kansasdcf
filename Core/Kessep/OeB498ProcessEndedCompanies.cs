// Program: OE_B498_PROCESS_ENDED_COMPANIES, ID: 371182952, model: 746.
// Short name: SWEE498B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B498_PROCESS_ENDED_COMPANIES.
/// </para>
/// <para>
/// This program writes the CSE Person numbers of those who are APs or ARs, to a
/// sequential file, using external SWEXEE27.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB498ProcessEndedCompanies: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B498_PROCESS_ENDED_COMPANIES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB498ProcessEndedCompanies(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB498ProcessEndedCompanies.
  /// </summary>
  public OeB498ProcessEndedCompanies(IContext context, Import import,
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
    // ***********************************************************************
    // *          Process Ended Health Insurance Company                     *
    // *  Read company from PPI record.
    // 
    // *
    // ***********************************************************************
    // *              M A I N T E N A N C E   L O G                          *
    // ***********************************************************************
    // *Date		Name      Work Req  		Description           *
    // ***********************************************************************
    // * 02/25/2003  	Ed Lyman  WR020311  	Initial Coding
    // * 08/25/2003	E.Shirk	  WR20311	Pre-production rewrite.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB498Housekeeping();

    // ****************************************************************************
    // **         Validate health insurance carrier code..
    // ****************************************************************************
    if (ReadHealthInsuranceCompany())
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Number + " " + (
        local.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (entities.HealthInsuranceCompany.InsurancePolicyCarrier,
        HealthInsuranceCompany.InsurancePolicyCarrier_MaxLength, 1, 16) + " " +
        local.Termination.TextDate + " " + "ACCEPTED:" + " Carrier found.";
      UseCabBusinessReport01();
      local.NeededToWrite.RptDetail = "";
      UseCabBusinessReport01();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Number + " " + (
        local.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (entities.HealthInsuranceCompany.InsurancePolicyCarrier,
        HealthInsuranceCompany.InsurancePolicyCarrier_MaxLength, 1, 16) + " " +
        local.Termination.TextDate + " " + "ERROR:" + " Carrier not found.";
      UseCabBusinessReport01();
      UseCabErrorReport();

      return;
    }

    UseOeB498EndCompany();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Abend: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseOeB498Close();
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeB498Close()
  {
    var useImport = new OeB498Close.Import();
    var useExport = new OeB498Close.Export();

    useImport.TotHiCoveragesExpired.Count = local.TotHiCoveragesExpired.Count;
    useImport.TotPersonalHiExpired.Count = local.TotPersonalHiExpired.Count;

    Call(OeB498Close.Execute, useImport, useExport);
  }

  private void UseOeB498EndCompany()
  {
    var useImport = new OeB498EndCompany.Import();
    var useExport = new OeB498EndCompany.Export();

    useImport.HealthInsuranceCompany.Identifier =
      entities.HealthInsuranceCompany.Identifier;
    useImport.Max.Date = local.Max.Date;
    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB498EndCompany.Execute, useImport, useExport);

    local.TotHiCoveragesExpired.Count = useExport.TotHiCoveragesExpired.Count;
    local.TotPersonalHiExpired.Count = useExport.TotPersHiExpired.Count;
  }

  private void UseOeB498Housekeeping()
  {
    var useImport = new OeB498Housekeeping.Import();
    var useExport = new OeB498Housekeeping.Export();

    Call(OeB498Housekeeping.Execute, useImport, useExport);

    local.HealthInsuranceCompany.CarrierCode =
      useExport.HealthInsuranceCompany.CarrierCode;
    local.Max.Date = useExport.Max.Date;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private bool ReadHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(
          command, "carrierCode", local.HealthInsuranceCompany.CarrierCode ?? ""
          );
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.Populated = true;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of TotHiCoveragesExpired.
    /// </summary>
    [JsonPropertyName("totHiCoveragesExpired")]
    public Common TotHiCoveragesExpired
    {
      get => totHiCoveragesExpired ??= new();
      set => totHiCoveragesExpired = value;
    }

    /// <summary>
    /// A value of TotPersonalHiExpired.
    /// </summary>
    [JsonPropertyName("totPersonalHiExpired")]
    public Common TotPersonalHiExpired
    {
      get => totPersonalHiExpired ??= new();
      set => totPersonalHiExpired = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of Termination.
    /// </summary>
    [JsonPropertyName("termination")]
    public DateWorkArea Termination
    {
      get => termination ??= new();
      set => termination = value;
    }

    /// <summary>
    /// A value of PolicyExpiration.
    /// </summary>
    [JsonPropertyName("policyExpiration")]
    public DateWorkArea PolicyExpiration
    {
      get => policyExpiration ??= new();
      set => policyExpiration = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea max;
    private Common commitCount;
    private Common totHiCoveragesExpired;
    private Common totPersonalHiExpired;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend neededToWrite;
    private DateWorkArea termination;
    private DateWorkArea policyExpiration;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson csePerson;
  }
#endregion
}
