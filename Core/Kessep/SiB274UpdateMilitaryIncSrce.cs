// Program: SI_B274_UPDATE_MILITARY_INC_SRCE, ID: 371072746, model: 746.
// Short name: SWE01293
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B274_UPDATE_MILITARY_INC_SRCE.
/// </summary>
[Serializable]
public partial class SiB274UpdateMilitaryIncSrce: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B274_UPDATE_MILITARY_INC_SRCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB274UpdateMilitaryIncSrce(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB274UpdateMilitaryIncSrce.
  /// </summary>
  public SiB274UpdateMilitaryIncSrce(IContext context, Import import,
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
    // ******************************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ******************************************************************************
    // 
    // *    DATE       NAME      REQUEST      DESCRIPTION
    // *
    // 
    // * ----------  ---------  ---------
    // ----------------------------------------*
    // * 08/07/2003  Bonnie Lee
    // PR185104   New value of 'P' for military status and*
    // *
    // new value of 'Pension/Retired' for
    // note.*
    // 
    // *
    // 
    // *
    // ******************************************************************************
    export.EabFileHandling.Status = "OK";
    export.TenPercentIncrease.Flag = "N";
    export.NumberOfIncSrcUpdates.Count = import.NumberOfIncSrcUpdates.Count;
    export.RecordsAlreadyProcessed.Count = import.RecordsAlreadyProcessed.Count;

    if (ReadMilitary())
    {
      switch(AsChar(import.Quarter.Text1))
      {
        case '1':
          if (!Lt(entities.Military.LastQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Military.LastQtrIncome) || Lt
              (import.Year.Year, entities.Military.LastQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedMilitaryWages();
            }

            return;
          }
          else
          {
            local.Military.Assign(entities.Military);
            local.Military.LastQtr = import.Quarter.Text1;
            local.Military.LastQtrYr = import.Year.Year;
            local.Military.LastQtrIncome = import.Wage.AverageCurrency;

            if (Lt(2600, entities.Military.Attribute4ThQtrIncome) && Lt
              (2600, entities.Military.Attribute3RdQtrIncome) && Equal
              (entities.Military.Attribute4ThQtrYr, import.Year.Year - 1) && Equal
              (entities.Military.Attribute3RdQtrYr, import.Year.Year - 1))
            {
              if (import.Wage.AverageCurrency >= entities
                .Military.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Military.Attribute3RdQtrIncome.GetValueOrDefault() * 1.1M && Lt
                (entities.Military.SentDt, AddMonths(import.Process.Date, -6)))
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '2':
          if (!Lt(entities.Military.Attribute2NdQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Military.Attribute2NdQtrIncome) || Lt
              (import.Year.Year, entities.Military.Attribute2NdQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedMilitaryWages();
            }

            return;
          }
          else
          {
            local.Military.Assign(entities.Military);
            local.Military.Attribute2NdQtr = import.Quarter.Text1;
            local.Military.Attribute2NdQtrYr = import.Year.Year;
            local.Military.Attribute2NdQtrIncome = import.Wage.AverageCurrency;

            if (Lt(2600, entities.Military.LastQtrIncome) && Lt
              (2600, entities.Military.Attribute4ThQtrIncome) && Equal
              (entities.Military.LastQtrYr, import.Year.Year) && Equal
              (entities.Military.Attribute4ThQtrYr, import.Year.Year - 1))
            {
              if (import.Wage.AverageCurrency >= entities
                .Military.LastQtrIncome.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Military.Attribute4ThQtrIncome.GetValueOrDefault() * 1.1M && Lt
                (entities.Military.SentDt, AddMonths(import.Process.Date, -6)))
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '3':
          if (!Lt(entities.Military.Attribute3RdQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Military.Attribute3RdQtrIncome) || Lt
              (import.Year.Year, entities.Military.Attribute3RdQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedMilitaryWages();
            }

            return;
          }
          else
          {
            local.Military.Assign(entities.Military);
            local.Military.Attribute3RdQtr = import.Quarter.Text1;
            local.Military.Attribute3RdQtrYr = import.Year.Year;
            local.Military.Attribute3RdQtrIncome = import.Wage.AverageCurrency;

            if (Lt(2600, entities.Military.Attribute2NdQtrIncome) && Lt
              (2600, entities.Military.LastQtrIncome) && Equal
              (entities.Military.Attribute2NdQtrYr, import.Year.Year) && Equal
              (entities.Military.LastQtrYr, import.Year.Year))
            {
              if (import.Wage.AverageCurrency >= entities
                .Military.Attribute2NdQtrIncome.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Military.LastQtrIncome.GetValueOrDefault() * 1.1M && Lt
                (entities.Military.SentDt, AddMonths(import.Process.Date, -6)))
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        case '4':
          if (!Lt(entities.Military.Attribute4ThQtrYr, import.Year.Year))
          {
            ++export.RecordsAlreadyProcessed.Count;

            if (Equal(import.Wage.AverageCurrency,
              entities.Military.Attribute4ThQtrIncome) || Lt
              (import.Year.Year, entities.Military.Attribute4ThQtrYr))
            {
            }
            else
            {
              UseSiB274RevisedMilitaryWages();
            }

            return;
          }
          else
          {
            local.Military.Assign(entities.Military);
            local.Military.Attribute4ThQtr = import.Quarter.Text1;
            local.Military.Attribute4ThQtrYr = import.Year.Year;
            local.Military.Attribute4ThQtrIncome = import.Wage.AverageCurrency;

            if (Lt(2600, entities.Military.Attribute3RdQtrIncome) && Lt
              (2600, entities.Military.Attribute2NdQtrIncome) && Equal
              (entities.Military.Attribute3RdQtrYr, import.Year.Year) && Equal
              (entities.Military.Attribute2NdQtrYr, import.Year.Year))
            {
              if (import.Wage.AverageCurrency >= entities
                .Military.Attribute3RdQtrIncome.GetValueOrDefault() * 1.1M && import
                .Wage.AverageCurrency >= entities
                .Military.Attribute2NdQtrIncome.GetValueOrDefault() * 1.1M && Lt
                (entities.Military.SentDt, AddMonths(import.Process.Date, -6)))
              {
                export.TenPercentIncrease.Flag = "Y";
              }
            }
          }

          break;
        default:
          break;
      }

      // ***************************************************************************************
      // 08/07/2003   B. Lee
      // 
      // Added check for new vaule of 'Pension/Retired' in note and set note to
      // 'Pension/Retired'                                          for
      // military status of 'P' and set military code to new value of 'P'.
      // 
      // ***************************************************************************************
      if (Equal(entities.Military.Note, "Active Service") || Equal
        (entities.Military.Note, "Reserves") || Equal
        (entities.Military.Note, "Pension/Retired") || IsEmpty
        (entities.Military.Note))
      {
        if (AsChar(import.MilitaryStatus.Text1) == 'A')
        {
          local.Military.Note = "Active Service";
          local.Military.MilitaryCode = "A";
        }
        else if (AsChar(import.MilitaryStatus.Text1) == 'R')
        {
          local.Military.Note = "Reserves";
          local.Military.MilitaryCode = "R";
        }
        else
        {
          local.Military.Note = "Pension/Retired";
          local.Military.MilitaryCode = "P";
        }
      }
      else
      {
        local.Military.Note = entities.Military.Note;
        local.Military.MilitaryCode = entities.Military.MilitaryCode;
      }

      try
      {
        UpdateMilitary();
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

  private void UseSiB274RevisedMilitaryWages()
  {
    var useImport = new SiB274RevisedMilitaryWages.Import();
    var useExport = new SiB274RevisedMilitaryWages.Export();

    useImport.Existing.Assign(entities.Military);
    useImport.Year.Year = import.Year.Year;
    useImport.Quarter.Text1 = import.Quarter.Text1;
    useImport.Wage.AverageCurrency = import.Wage.AverageCurrency;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Employer.Name = import.Employer.Name;

    Call(SiB274RevisedMilitaryWages.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadMilitary()
  {
    entities.Military.Populated = false;

    return Read("ReadMilitary",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.Military.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Military.Identifier = db.GetDateTime(reader, 0);
        entities.Military.LastQtrIncome = db.GetNullableDecimal(reader, 1);
        entities.Military.LastQtr = db.GetNullableString(reader, 2);
        entities.Military.LastQtrYr = db.GetNullableInt32(reader, 3);
        entities.Military.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 4);
        entities.Military.Attribute2NdQtr = db.GetNullableString(reader, 5);
        entities.Military.Attribute2NdQtrYr = db.GetNullableInt32(reader, 6);
        entities.Military.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 7);
        entities.Military.Attribute3RdQtr = db.GetNullableString(reader, 8);
        entities.Military.Attribute3RdQtrYr = db.GetNullableInt32(reader, 9);
        entities.Military.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 10);
        entities.Military.Attribute4ThQtr = db.GetNullableString(reader, 11);
        entities.Military.Attribute4ThQtrYr = db.GetNullableInt32(reader, 12);
        entities.Military.SentDt = db.GetNullableDate(reader, 13);
        entities.Military.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.Military.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Military.CspINumber = db.GetString(reader, 16);
        entities.Military.MilitaryCode = db.GetString(reader, 17);
        entities.Military.WorkerId = db.GetNullableString(reader, 18);
        entities.Military.Note = db.GetNullableString(reader, 19);
        entities.Military.Populated = true;
      });
  }

  private void UpdateMilitary()
  {
    System.Diagnostics.Debug.Assert(entities.Military.Populated);

    var lastQtrIncome = local.Military.LastQtrIncome.GetValueOrDefault();
    var lastQtr = local.Military.LastQtr ?? "";
    var lastQtrYr = local.Military.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      local.Military.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = local.Military.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      local.Military.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      local.Military.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = local.Military.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      local.Military.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      local.Military.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = local.Military.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      local.Military.Attribute4ThQtrYr.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var militaryCode = local.Military.MilitaryCode;
    var note = local.Military.Note ?? "";

    entities.Military.Populated = false;
    Update("UpdateMilitary",
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
        db.SetString(command, "militaryCode", militaryCode);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          entities.Military.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.Military.CspINumber);
      });

    entities.Military.LastQtrIncome = lastQtrIncome;
    entities.Military.LastQtr = lastQtr;
    entities.Military.LastQtrYr = lastQtrYr;
    entities.Military.Attribute2NdQtrIncome = attribute2NdQtrIncome;
    entities.Military.Attribute2NdQtr = attribute2NdQtr;
    entities.Military.Attribute2NdQtrYr = attribute2NdQtrYr;
    entities.Military.Attribute3RdQtrIncome = attribute3RdQtrIncome;
    entities.Military.Attribute3RdQtr = attribute3RdQtr;
    entities.Military.Attribute3RdQtrYr = attribute3RdQtrYr;
    entities.Military.Attribute4ThQtrIncome = attribute4ThQtrIncome;
    entities.Military.Attribute4ThQtr = attribute4ThQtr;
    entities.Military.Attribute4ThQtrYr = attribute4ThQtrYr;
    entities.Military.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Military.LastUpdatedBy = lastUpdatedBy;
    entities.Military.MilitaryCode = militaryCode;
    entities.Military.WorkerId = lastUpdatedBy;
    entities.Military.Note = note;
    entities.Military.Populated = true;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
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
    /// A value of Wage.
    /// </summary>
    [JsonPropertyName("wage")]
    public Common Wage
    {
      get => wage ??= new();
      set => wage = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of MilitaryStatus.
    /// </summary>
    [JsonPropertyName("militaryStatus")]
    public TextWorkArea MilitaryStatus
    {
      get => militaryStatus ??= new();
      set => militaryStatus = value;
    }

    private DateWorkArea process;
    private IncomeSource military;
    private Common numberOfIncSrcUpdates;
    private Common recordsAlreadyProcessed;
    private Common wage;
    private CsePerson csePerson;
    private Employer employer;
    private TextWorkArea quarter;
    private DateWorkArea year;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea militaryStatus;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    private IncomeSource military;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    private IncomeSource military;
  }
#endregion
}
