// Program: SI_B273_SEND_ALERT_NEW_INC_SRCE, ID: 371060341, model: 746.
// Short name: SWE01274
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_SEND_ALERT_NEW_INC_SRCE.
/// </summary>
[Serializable]
public partial class SiB273SendAlertNewIncSrce: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_SEND_ALERT_NEW_INC_SRCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273SendAlertNewIncSrce(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273SendAlertNewIncSrce.
  /// </summary>
  public SiB273SendAlertNewIncSrce(IContext context, Import import,
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
    // **********************************************************************************************
    // 12/21/2005             DDupree         WR258947
    // As part of WR258947 added a code to select different reason codes based 
    // on
    // which program is using this cab.
    // **********************************************************************************************
    local.Infrastructure.EventId = 10;
    local.Infrastructure.UserId = import.ProgramCheckpointRestart.ProgramName;
    local.Infrastructure.ReferenceDate = import.Process.Date;
    local.Infrastructure.BusinessObjectCd = "ICS";
    local.Infrastructure.Detail = import.Infrastructure.Detail ?? "";
    local.Infrastructure.DenormTimestamp = import.Employment.Identifier;
    local.Infrastructure.SituationNumber = 0;

    // **********************************************************************************************
    // 12/21/2005             DDupree         WR258947
    // As part of WR258947 added the following reason code to reflect new
    // employment from nsa instead of from new hire
    // **********************************************************************************************
    if (Equal(import.FplsLocateResponse.AgencyCode, "A03"))
    {
      local.Infrastructure.ReasonCode = "NSANEWEMPL";
    }
    else
    {
      local.Infrastructure.ReasonCode = "NDNHEMPL";
    }

    ExitState = "ACO_NN0000_ALL_OK";
    UseOeCabRaiseEvent();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR ENCOUNTERED DURING RAISE EVENT FOR CREATE OF INCOME SOURCE FOR CSE PERSON = " +
        import.CsePerson.Number;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure infrastructure;
    private IncomeSource employment;
    private DateWorkArea process;
    private CsePerson csePerson;
    private FplsLocateResponse fplsLocateResponse;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of NewHireIndicator.
    /// </summary>
    [JsonPropertyName("newHireIndicator")]
    public Common NewHireIndicator
    {
      get => newHireIndicator ??= new();
      set => newHireIndicator = value;
    }

    /// <summary>
    /// A value of DateOfHireUpdates.
    /// </summary>
    [JsonPropertyName("dateOfHireUpdates")]
    public Common DateOfHireUpdates
    {
      get => dateOfHireUpdates ??= new();
      set => dateOfHireUpdates = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    public DateWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    private Infrastructure infrastructure;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common personsUpdated;
    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private Common newHireIndicator;
    private Common dateOfHireUpdates;
    private Employer employer;
    private DateWorkArea current;
    private DateWorkArea dateOfHire;
    private IncomeSource employment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    private CsePerson csePerson;
    private IncomeSource employment;
  }
#endregion
}
