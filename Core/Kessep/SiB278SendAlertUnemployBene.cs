// Program: SI_B278_SEND_ALERT_UNEMPLOY_BENE, ID: 373313553, model: 746.
// Short name: SWE01306
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B278_SEND_ALERT_UNEMPLOY_BENE.
/// </summary>
[Serializable]
public partial class SiB278SendAlertUnemployBene: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B278_SEND_ALERT_UNEMPLOY_BENE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB278SendAlertUnemployBene(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB278SendAlertUnemployBene.
  /// </summary>
  public SiB278SendAlertUnemployBene(IContext context, Import import,
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
    export.AlertsSent.Count = import.AlertsSent.Count;
    export.AlreadyProcessed.Count = import.AlreadyProcessed.Count;
    local.Infrastructure.EventId = 10;
    local.Infrastructure.UserId = import.ProgramCheckpointRestart.ProgramName;
    local.Infrastructure.ReferenceDate = import.Process.Date;
    local.Infrastructure.BusinessObjectCd = "ICS";
    local.Infrastructure.Detail = import.Infrastructure.Detail ?? "";
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.ReasonCode = "UIFROMNDNH";

    foreach(var item in ReadInfrastructure())
    {
      if (Equal(import.Infrastructure.Detail, entities.Infrastructure.Detail))
      {
        ++export.AlreadyProcessed.Count;

        return;
      }
    }

    ExitState = "ACO_NN0000_ALL_OK";
    UseOeCabRaiseEvent();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR ENCOUNTERED DURING RAISE EVENT FOR CREATE OF UNEMPLOYMENT FOR CSE PERSON = " +
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
    else
    {
      ++export.AlertsSent.Count;
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

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "csePersonNum", import.CsePerson.Number);
        db.SetString(command, "reasonCode", local.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 1);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 3);
        entities.Infrastructure.Populated = true;

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
    /// <summary>
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Common alertsSent;
    private Common alreadyProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea process;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    private Common alertsSent;
    private Common alreadyProcessed;
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

    private Infrastructure infrastructure;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private Infrastructure infrastructure;
  }
#endregion
}
