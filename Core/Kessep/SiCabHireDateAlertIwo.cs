// Program: SI_CAB_HIRE_DATE_ALERT_IWO, ID: 373399364, model: 746.
// Short name: SWE01298
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_HIRE_DATE_ALERT_IWO.
/// </summary>
[Serializable]
public partial class SiCabHireDateAlertIwo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_HIRE_DATE_ALERT_IWO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabHireDateAlertIwo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabHireDateAlertIwo.
  /// </summary>
  public SiCabHireDateAlertIwo(IContext context, Import import, Export export):
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
    // As part of WR258947 added a import view (fpls_locate_response) that will 
    // be
    // past the some called cabs.
    // **********************************************************************************************
    export.DateOfHireUpdates.Count = import.DateOfHireUpdates.Count;
    export.RecordsAlreadyProcessed.Count = import.RecordsAlreadyProcessed.Count;

    if (ReadEmployment())
    {
      // *******************************************************************
      // Although employment note is usually spaces, sometimes the worker
      // enters a note. We wish to preserve whatever the worker has entered.
      // *******************************************************************
      if (IsEmpty(entities.Employment.Note))
      {
        if (IsEmpty(import.NewInfo.Note))
        {
          if (AsChar(import.FederalNewhireIndicator.Flag) == 'Y')
          {
            local.Employment.Note =
              "Start Date has been supplied by Federal New Hire.";
          }
          else
          {
            local.Employment.Note =
              "Start Date has been supplied by State New Hire.";
          }
        }
        else
        {
          // **************************************************************
          // Import note field probably contains alternate ssn information.
          // **************************************************************
          local.Employment.Note = import.NewInfo.Note ?? "";
        }
      }
      else
      {
        // ***********************************************************
        // Income Source already had info in the Note field.
        // ***********************************************************
        local.Employment.Note = entities.Employment.Note;
      }

      try
      {
        UpdateEmployment();
        ++export.DateOfHireUpdates.Count;

        if (AsChar(import.FederalNewhireIndicator.Flag) == 'Y')
        {
          if (Equal(import.FplsLocateResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.Detail =
              "Start Date confirmed by National Security Agency";
          }
          else
          {
            local.Infrastructure.Detail =
              "Start Date confirmed by Federal New Hire for employer: " + (
                import.Employer.Name ?? "");
          }

          UseSiB273SendAlertNewIncSrce();
        }
        else
        {
          if (Equal(import.FplsLocateResponse.AgencyCode, "A03"))
          {
            local.Infrastructure.Detail =
              "Start Date confirmed by National Security Agency";
          }
          else
          {
            local.Infrastructure.Detail =
              "Start Date confirmed by State New Hire for employer: " + (
                import.Employer.Name ?? "");
          }

          UseSiB276SendAlertNewIncSrce();
        }

        if (AsChar(import.AddressSuitableForIwo.Flag) == 'Y' && AsChar
          (import.AutomaticGenerateIwo.Flag) == 'Y')
        {
          UseLeAutomaticIwoGeneration();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail =
              "UNABLE TO CREATE IWO FOR PERSON:  " + local.CsePerson.Number + " ERR: " +
              local.ExitStateWorkArea.Message;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
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

  private void UseLeAutomaticIwoGeneration()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    MoveIncomeSource(entities.Employment, useImport.IncomeSource);
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);
  }

  private void UseSiB273SendAlertNewIncSrce()
  {
    var useImport = new SiB273SendAlertNewIncSrce.Import();
    var useExport = new SiB273SendAlertNewIncSrce.Export();

    useImport.FplsLocateResponse.AgencyCode =
      import.FplsLocateResponse.AgencyCode;
    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Employment.Identifier = import.Employment.Identifier;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Process.Date = import.Process.Date;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;

    Call(SiB273SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiB276SendAlertNewIncSrce()
  {
    var useImport = new SiB276SendAlertNewIncSrce.Import();
    var useExport = new SiB276SendAlertNewIncSrce.Export();

    useImport.FplsLocateResponse.AgencyCode =
      import.FplsLocateResponse.AgencyCode;
    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = import.Process.Date;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;
    useImport.Employment.Identifier = import.Employment.Identifier;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(SiB276SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private bool ReadEmployment()
  {
    entities.Employment.Populated = false;

    return Read("ReadEmployment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.Employment.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employment.Identifier = db.GetDateTime(reader, 0);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 1);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 2);
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Employment.CspINumber = db.GetString(reader, 5);
        entities.Employment.WorkerId = db.GetNullableString(reader, 6);
        entities.Employment.StartDt = db.GetNullableDate(reader, 7);
        entities.Employment.Note = db.GetNullableString(reader, 8);
        entities.Employment.Populated = true;
      });
  }

  private void UpdateEmployment()
  {
    System.Diagnostics.Debug.Assert(entities.Employment.Populated);

    var returnDt = import.NewInfo.ReturnDt;
    var returnCd = import.NewInfo.ReturnCd ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var startDt = import.DateOfHire.Date;
    var note = local.Employment.Note ?? "";

    entities.Employment.Populated = false;
    Update("UpdateEmployment",
      (db, command) =>
      {
        db.SetNullableDate(command, "returnDt", returnDt);
        db.SetNullableString(command, "returnCd", returnCd);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          entities.Employment.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.Employment.CspINumber);
      });

    entities.Employment.ReturnDt = returnDt;
    entities.Employment.ReturnCd = returnCd;
    entities.Employment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Employment.LastUpdatedBy = lastUpdatedBy;
    entities.Employment.WorkerId = lastUpdatedBy;
    entities.Employment.StartDt = startDt;
    entities.Employment.Note = note;
    entities.Employment.Populated = true;
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
    /// A value of FederalNewhireIndicator.
    /// </summary>
    [JsonPropertyName("federalNewhireIndicator")]
    public Common FederalNewhireIndicator
    {
      get => federalNewhireIndicator ??= new();
      set => federalNewhireIndicator = value;
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
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of NewInfo.
    /// </summary>
    [JsonPropertyName("newInfo")]
    public IncomeSource NewInfo
    {
      get => newInfo ??= new();
      set => newInfo = value;
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
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    public DateWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
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
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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

    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private Common federalNewhireIndicator;
    private Common dateOfHireUpdates;
    private Common recordsAlreadyProcessed;
    private IncomeSource newInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dateOfHire;
    private Employer employer;
    private IncomeSource employment;
    private CsePerson csePerson;
    private DateWorkArea process;
    private FplsLocateResponse fplsLocateResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    private Common dateOfHireUpdates;
    private Common recordsAlreadyProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private IncomeSource employment;
    private Infrastructure infrastructure;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    private IncomeSource employment;
  }
#endregion
}
