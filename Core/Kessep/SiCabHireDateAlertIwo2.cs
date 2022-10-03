// Program: SI_CAB_HIRE_DATE_ALERT_IWO2, ID: 945107636, model: 746.
// Short name: SWE03671
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_HIRE_DATE_ALERT_IWO2.
/// </summary>
[Serializable]
public partial class SiCabHireDateAlertIwo2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_HIRE_DATE_ALERT_IWO2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabHireDateAlertIwo2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabHireDateAlertIwo2.
  /// </summary>
  public SiCabHireDateAlertIwo2(IContext context, Import import, Export export):
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
    // 10/05/2012             DDupree         CQ33281
    // As part of CQ33281 this cab was copied from is cab hire date alert iwo 
    // and modified
    //  to be used with the quarterly wage program.
    // **********************************************************************************************
    export.NumberIncSrcUpdates.Count = import.NumberIncSrcUpdates.Count;
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
          local.Employment.Note =
            "Start Date has been supplied by Federal Quarterly Wage.";
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
        ++export.NumberIncSrcUpdates.Count;
        local.Infrastructure.ReasonCode = "FEDQTRWAGE";
        local.Infrastructure.Detail =
          "Start Date confirmed by Federal Quarterly Wage for employer: " + (
            import.Employer.Name ?? "");
        UseSiB274SendAlertNewIncSrce();

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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ReasonCode = source.ReasonCode;
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

  private void UseSiB274SendAlertNewIncSrce()
  {
    var useImport = new SiB274SendAlertNewIncSrce.Import();
    var useExport = new SiB274SendAlertNewIncSrce.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Employment.Identifier = import.Employment.Identifier;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Process.Date = import.Process.Date;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SiB274SendAlertNewIncSrce.Execute, useImport, useExport);
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
        entities.Employment.LastQtrIncome = db.GetNullableDecimal(reader, 1);
        entities.Employment.LastQtr = db.GetNullableString(reader, 2);
        entities.Employment.LastQtrYr = db.GetNullableInt32(reader, 3);
        entities.Employment.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 4);
        entities.Employment.Attribute2NdQtr = db.GetNullableString(reader, 5);
        entities.Employment.Attribute2NdQtrYr = db.GetNullableInt32(reader, 6);
        entities.Employment.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 7);
        entities.Employment.Attribute3RdQtr = db.GetNullableString(reader, 8);
        entities.Employment.Attribute3RdQtrYr = db.GetNullableInt32(reader, 9);
        entities.Employment.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 10);
        entities.Employment.Attribute4ThQtr = db.GetNullableString(reader, 11);
        entities.Employment.Attribute4ThQtrYr = db.GetNullableInt32(reader, 12);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 13);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 14);
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Employment.CspINumber = db.GetString(reader, 17);
        entities.Employment.WorkerId = db.GetNullableString(reader, 18);
        entities.Employment.StartDt = db.GetNullableDate(reader, 19);
        entities.Employment.Note = db.GetNullableString(reader, 20);
        entities.Employment.Populated = true;
      });
  }

  private void UpdateEmployment()
  {
    System.Diagnostics.Debug.Assert(entities.Employment.Populated);

    var lastQtrIncome = import.NewInfo.LastQtrIncome.GetValueOrDefault();
    var lastQtr = import.NewInfo.LastQtr ?? "";
    var lastQtrYr = import.NewInfo.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      import.NewInfo.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = import.NewInfo.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      import.NewInfo.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      import.NewInfo.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = import.NewInfo.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      import.NewInfo.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      import.NewInfo.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = import.NewInfo.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      import.NewInfo.Attribute4ThQtrYr.GetValueOrDefault();
    var returnDt = import.NewInfo.ReturnDt;
    var returnCd = import.NewInfo.ReturnCd ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var startDt = import.NewInfo.StartDt;
    var note = local.Employment.Note ?? "";

    entities.Employment.Populated = false;
    Update("UpdateEmployment",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "lastQtrIncome", lastQtrIncome);
        db.SetNullableString(command, "lastQtr", lastQtr);
        db.SetNullableInt32(command, "lastQtrYr", lastQtrYr);
        db.
          SetNullableDecimal(command, "secondQtrIncome", attribute2NdQtrIncome);
          
        db.SetNullableString(command, "secondQtr", attribute2NdQtr);
        db.SetNullableInt32(command, "secondQtrYr", attribute2NdQtrYr);
        db.SetNullableDecimal(command, "thirdQtrIncome", attribute3RdQtrIncome);
        db.SetNullableString(command, "thirdQtr", attribute3RdQtr);
        db.SetNullableInt32(command, "thirdQtrYr", attribute3RdQtrYr);
        db.
          SetNullableDecimal(command, "fourthQtrIncome", attribute4ThQtrIncome);
          
        db.SetNullableString(command, "fourthQtr", attribute4ThQtr);
        db.SetNullableInt32(command, "fourthQtrYr", attribute4ThQtrYr);
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

    entities.Employment.LastQtrIncome = lastQtrIncome;
    entities.Employment.LastQtr = lastQtr;
    entities.Employment.LastQtrYr = lastQtrYr;
    entities.Employment.Attribute2NdQtrIncome = attribute2NdQtrIncome;
    entities.Employment.Attribute2NdQtr = attribute2NdQtr;
    entities.Employment.Attribute2NdQtrYr = attribute2NdQtrYr;
    entities.Employment.Attribute3RdQtrIncome = attribute3RdQtrIncome;
    entities.Employment.Attribute3RdQtr = attribute3RdQtr;
    entities.Employment.Attribute3RdQtrYr = attribute3RdQtrYr;
    entities.Employment.Attribute4ThQtrIncome = attribute4ThQtrIncome;
    entities.Employment.Attribute4ThQtr = attribute4ThQtr;
    entities.Employment.Attribute4ThQtrYr = attribute4ThQtrYr;
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
    /// A value of NumberIncSrcUpdates.
    /// </summary>
    [JsonPropertyName("numberIncSrcUpdates")]
    public Common NumberIncSrcUpdates
    {
      get => numberIncSrcUpdates ??= new();
      set => numberIncSrcUpdates = value;
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

    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private Common numberIncSrcUpdates;
    private Common recordsAlreadyProcessed;
    private IncomeSource newInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dateOfHire;
    private Employer employer;
    private IncomeSource employment;
    private CsePerson csePerson;
    private DateWorkArea process;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NumberIncSrcUpdates.
    /// </summary>
    [JsonPropertyName("numberIncSrcUpdates")]
    public Common NumberIncSrcUpdates
    {
      get => numberIncSrcUpdates ??= new();
      set => numberIncSrcUpdates = value;
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

    private Common numberIncSrcUpdates;
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
