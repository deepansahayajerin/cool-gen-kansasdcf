// Program: SI_B274_UPDATE_EMPLOYMNT_INC_SRC, ID: 371072738, model: 746.
// Short name: SWE01285
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B274_UPDATE_EMPLOYMNT_INC_SRC.
/// </summary>
[Serializable]
public partial class SiB274UpdateEmploymntIncSrc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B274_UPDATE_EMPLOYMNT_INC_SRC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB274UpdateEmploymntIncSrc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB274UpdateEmploymntIncSrc.
  /// </summary>
  public SiB274UpdateEmploymntIncSrc(IContext context, Import import,
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
    export.EabFileHandling.Status = "OK";
    export.TenPercentIncrease.Flag = "N";
    export.NumberOfIncSrcUpdates.Count = import.NumberOfIncSrcUpdates.Count;
    export.RecordsAlreadyProcessed.Count = import.RecordsAlreadyProcessed.Count;

    if (ReadEmployment())
    {
      switch(AsChar(import.Quarter.Text1))
      {
        case '1':
          if (!Lt(entities.Employment.LastQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Employment.LastQtrIncome) || Lt
              (import.Year.Year, entities.Employment.LastQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedWageReport();
            }

            return;
          }
          else
          {
            local.Employment.Assign(entities.Employment);
            local.Employment.LastQtr = import.Quarter.Text1;
            local.Employment.LastQtrYr = import.Year.Year;
            local.Employment.LastQtrIncome = import.Wage.AverageCurrency;

            if (Lt(2600, entities.Employment.Attribute4ThQtrIncome) && Lt
              (2600, entities.Employment.Attribute3RdQtrIncome) && Equal
              (entities.Employment.Attribute4ThQtrYr, import.Year.Year - 1) && Equal
              (entities.Employment.Attribute3RdQtrYr, import.Year.Year - 1))
            {
              if (import.Wage.AverageCurrency >= entities
                .Employment.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M
                && import.Wage.AverageCurrency >= entities
                .Employment.Attribute3RdQtrIncome.GetValueOrDefault() * 1.1M
                && Lt
                (entities.Employment.SentDt, AddMonths(import.Process.Date, -6)))
                
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '2':
          if (!Lt(entities.Employment.Attribute2NdQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Employment.Attribute2NdQtrIncome) || Lt
              (import.Year.Year, entities.Employment.Attribute2NdQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedWageReport();
            }

            return;
          }
          else
          {
            local.Employment.Assign(entities.Employment);
            local.Employment.Attribute2NdQtr = import.Quarter.Text1;
            local.Employment.Attribute2NdQtrYr = import.Year.Year;
            local.Employment.Attribute2NdQtrIncome =
              import.Wage.AverageCurrency;

            if (Lt(2600, entities.Employment.LastQtrIncome) && Lt
              (2600, entities.Employment.Attribute4ThQtrIncome) && Equal
              (entities.Employment.LastQtrYr, import.Year.Year) && Equal
              (entities.Employment.Attribute4ThQtrYr, import.Year.Year - 1))
            {
              if (import.Wage.AverageCurrency >= entities
                .Employment.LastQtrIncome.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Employment.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M
                && Lt
                (entities.Employment.SentDt, AddMonths(import.Process.Date, -6)))
                
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '3':
          if (!Lt(entities.Employment.Attribute3RdQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Employment.Attribute3RdQtrIncome) || Lt
              (import.Year.Year, entities.Employment.Attribute3RdQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedWageReport();
            }

            return;
          }
          else
          {
            local.Employment.Assign(entities.Employment);
            local.Employment.Attribute3RdQtr = import.Quarter.Text1;
            local.Employment.Attribute3RdQtrYr = import.Year.Year;
            local.Employment.Attribute3RdQtrIncome =
              import.Wage.AverageCurrency;

            if (Lt(2600, entities.Employment.Attribute2NdQtrIncome) && Lt
              (2600, entities.Employment.LastQtrIncome) && Equal
              (entities.Employment.Attribute2NdQtrYr, import.Year.Year) && Equal
              (entities.Employment.LastQtrYr, import.Year.Year))
            {
              if (import.Wage.AverageCurrency >= entities
                .Employment.Attribute2NdQtrIncome.GetValueOrDefault() * 1.1M
                && import.Wage.AverageCurrency >= entities
                .Employment.LastQtrIncome.GetValueOrDefault() * 1.1M && Lt
                (entities.Employment.SentDt, AddMonths(import.Process.Date, -6)))
                
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '4':
          if (!Lt(entities.Employment.Attribute4ThQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Employment.Attribute4ThQtrIncome) || Lt
              (import.Year.Year, entities.Employment.Attribute4ThQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedWageReport();
            }

            return;
          }
          else
          {
            local.Employment.Assign(entities.Employment);
            local.Employment.Attribute4ThQtr = import.Quarter.Text1;
            local.Employment.Attribute4ThQtrYr = import.Year.Year;
            local.Employment.Attribute4ThQtrIncome =
              import.Wage.AverageCurrency;

            if (Lt(2600, entities.Employment.Attribute3RdQtrIncome) && Lt
              (2600, entities.Employment.Attribute2NdQtrIncome) && Equal
              (entities.Employment.Attribute3RdQtrYr, import.Year.Year) && Equal
              (entities.Employment.Attribute2NdQtrYr, import.Year.Year))
            {
              if (import.Wage.AverageCurrency >= entities
                .Employment.Attribute3RdQtrYr.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Employment.Attribute2NdQtrYr.GetValueOrDefault() * 1.1M && Lt
                (entities.Employment.SentDt, AddMonths(import.Process.Date, -6)))
                
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        default:
          break;
      }

      try
      {
        UpdateEmployment();
        ++export.NumberOfIncSrcUpdates.Count;
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

  private void UseSiB274RevisedWageReport()
  {
    var useImport = new SiB274RevisedWageReport.Import();
    var useExport = new SiB274RevisedWageReport.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Employer.Name = import.Employer.Name;
    useImport.Existing.Assign(entities.Employment);
    useImport.Quarter.Text1 = import.Quarter.Text1;
    useImport.Year.Year = import.Year.Year;
    useImport.Wage.AverageCurrency = import.Wage.AverageCurrency;

    Call(SiB274RevisedWageReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
        entities.Employment.SentDt = db.GetNullableDate(reader, 13);
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Employment.CspINumber = db.GetString(reader, 16);
        entities.Employment.WorkerId = db.GetNullableString(reader, 17);
        entities.Employment.Populated = true;
      });
  }

  private void UpdateEmployment()
  {
    System.Diagnostics.Debug.Assert(entities.Employment.Populated);

    var lastQtrIncome = local.Employment.LastQtrIncome.GetValueOrDefault();
    var lastQtr = local.Employment.LastQtr ?? "";
    var lastQtrYr = local.Employment.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      local.Employment.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = local.Employment.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      local.Employment.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      local.Employment.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = local.Employment.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      local.Employment.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      local.Employment.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = local.Employment.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      local.Employment.Attribute4ThQtrYr.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;

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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
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
    entities.Employment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Employment.LastUpdatedBy = lastUpdatedBy;
    entities.Employment.WorkerId = lastUpdatedBy;
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
    /// A value of Wage.
    /// </summary>
    [JsonPropertyName("wage")]
    public Common Wage
    {
      get => wage ??= new();
      set => wage = value;
    }

    /// <summary>
    /// A value of Quarter.
    /// </summary>
    [JsonPropertyName("quarter")]
    public TextWorkArea Quarter
    {
      get => quarter ??= new();
      set => quarter = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of NumberOfIncSrcUpdates.
    /// </summary>
    [JsonPropertyName("numberOfIncSrcUpdates")]
    public Common NumberOfIncSrcUpdates
    {
      get => numberOfIncSrcUpdates ??= new();
      set => numberOfIncSrcUpdates = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private Common wage;
    private TextWorkArea quarter;
    private DateWorkArea year;
    private Common numberOfIncSrcUpdates;
    private Common recordsAlreadyProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of TenPercentIncrease.
    /// </summary>
    [JsonPropertyName("tenPercentIncrease")]
    public Common TenPercentIncrease
    {
      get => tenPercentIncrease ??= new();
      set => tenPercentIncrease = value;
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
    /// A value of NumberOfIncSrcUpdates.
    /// </summary>
    [JsonPropertyName("numberOfIncSrcUpdates")]
    public Common NumberOfIncSrcUpdates
    {
      get => numberOfIncSrcUpdates ??= new();
      set => numberOfIncSrcUpdates = value;
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

    private Common tenPercentIncrease;
    private EabFileHandling eabFileHandling;
    private Common numberOfIncSrcUpdates;
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

    private IncomeSource employment;
    private Infrastructure infrastructure;
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
