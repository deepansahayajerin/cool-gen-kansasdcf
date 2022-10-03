// Program: SI_B273_UPDATE_ENLISTMENT_DATE, ID: 371072320, model: 746.
// Short name: SWE01291
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_UPDATE_ENLISTMENT_DATE.
/// </summary>
[Serializable]
public partial class SiB273UpdateEnlistmentDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_UPDATE_ENLISTMENT_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273UpdateEnlistmentDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273UpdateEnlistmentDate.
  /// </summary>
  public SiB273UpdateEnlistmentDate(IContext context, Import import,
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
    export.DateOfHireUpdates.Count = import.DateOfHireUpdates.Count;
    export.RecordsAlreadyProcessed.Count = import.RecordsAlreadyProcessed.Count;

    if (ReadMilitary())
    {
      if (Equal(entities.Military.ReturnDt, local.Null1.Date) && Equal
        (entities.Military.StartDt, local.Null1.Date) && !
        Equal(entities.Military.WorkerId,
        import.ProgramCheckpointRestart.ProgramName))
      {
        if (IsEmpty(entities.Military.Note))
        {
          if (IsEmpty(import.NewInfo.Note))
          {
            local.Employment.Note =
              "Start Date has been supplied by Federal New Hire.";
          }
          else
          {
            // ***********************************************************
            // Note field probably contains alternate ssn information.
            // ***********************************************************
            local.Employment.Note = import.NewInfo.Note ?? "";
          }
        }
        else
        {
          // ***********************************************************
          // Income Source already had info in the Note field.
          // ***********************************************************
          local.Employment.Note = entities.Military.Note;
        }

        try
        {
          UpdateMilitary();

          if (IsEmpty(import.NewInfo.Note))
          {
            local.Infrastructure.Detail =
              "Enlistment Date supplied for military employer: " + (
                import.Employer.Name ?? "");
          }
          else
          {
            local.Infrastructure.Detail = import.NewInfo.Note ?? "";
          }

          local.Employment.Identifier = import.Military.Identifier;
          UseSiB273SendAlertNewIncSrce();
          ++export.DateOfHireUpdates.Count;
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
        local.NewHire.StartDt = import.DateOfHire.Date;

        if (AsChar(import.AlreadyProcessed.Flag) == 'Y')
        {
          UseSiB273AlreadyProcessedEmpl();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }
        }

        ++export.RecordsAlreadyProcessed.Count;
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private void UseSiB273AlreadyProcessedEmpl()
  {
    var useImport = new SiB273AlreadyProcessedEmpl.Import();
    var useExport = new SiB273AlreadyProcessedEmpl.Export();

    useImport.Cse.Assign(entities.Military);
    MoveEmployer(import.Employer, useImport.Kaecses);
    MoveEmployer(import.Employer, useImport.NewHireEmployer);
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.NewHireIncomeSource.Assign(local.NewHire);

    Call(SiB273AlreadyProcessedEmpl.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273SendAlertNewIncSrce()
  {
    var useImport = new SiB273SendAlertNewIncSrce.Import();
    var useExport = new SiB273SendAlertNewIncSrce.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Process.Date = import.Process.Date;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;

    Call(SiB273SendAlertNewIncSrce.Execute, useImport, useExport);
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
        entities.Military.Type1 = db.GetString(reader, 1);
        entities.Military.SentDt = db.GetNullableDate(reader, 2);
        entities.Military.ReturnDt = db.GetNullableDate(reader, 3);
        entities.Military.ReturnCd = db.GetNullableString(reader, 4);
        entities.Military.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Military.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Military.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Military.CreatedBy = db.GetString(reader, 8);
        entities.Military.CspINumber = db.GetString(reader, 9);
        entities.Military.MilitaryCode = db.GetString(reader, 10);
        entities.Military.SendTo = db.GetNullableString(reader, 11);
        entities.Military.WorkerId = db.GetNullableString(reader, 12);
        entities.Military.StartDt = db.GetNullableDate(reader, 13);
        entities.Military.EndDt = db.GetNullableDate(reader, 14);
        entities.Military.Note = db.GetNullableString(reader, 15);
        entities.Military.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Military.Type1);
        CheckValid<IncomeSource>("SendTo", entities.Military.SendTo);
      });
  }

  private void UpdateMilitary()
  {
    System.Diagnostics.Debug.Assert(entities.Military.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var startDt = import.DateOfHire.Date;
    var note = local.Employment.Note ?? "";

    entities.Military.Populated = false;
    Update("UpdateMilitary",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          entities.Military.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.Military.CspINumber);
      });

    entities.Military.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Military.LastUpdatedBy = lastUpdatedBy;
    entities.Military.WorkerId = lastUpdatedBy;
    entities.Military.StartDt = startDt;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
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
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    private Common dateOfHireUpdates;
    private Common recordsAlreadyProcessed;
    private IncomeSource newInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dateOfHire;
    private Employer employer;
    private IncomeSource military;
    private CsePerson csePerson;
    private DateWorkArea process;
    private Common alreadyProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private EabFileHandling eabFileHandling;
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
    /// A value of NewHire.
    /// </summary>
    [JsonPropertyName("newHire")]
    public IncomeSource NewHire
    {
      get => newHire ??= new();
      set => newHire = value;
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

    private IncomeSource newHire;
    private IncomeSource employment;
    private Infrastructure infrastructure;
    private DateWorkArea null1;
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
