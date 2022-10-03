// Program: SP_DELETE_PROGRAM, ID: 371745837, model: 746.
// Short name: SWE01337
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_PROGRAM.
/// </summary>
[Serializable]
public partial class SpDeleteProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteProgram.
  /// </summary>
  public SpDeleteProgram(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************
    // *
    // *  4/30/97	Rod Grey	Change Current Date.
    // ***********************************************
    local.SetDiscontinueDate.Flag = "N";
    local.Current.Date = Now().Date;
    MoveProgram1(import.Program, export.Program);

    if (ReadPersonProgram())
    {
      if (!Lt(local.Current.Date, entities.PersonProgram.EffectiveDate) && Lt
        (local.Current.Date, entities.PersonProgram.DiscontinueDate))
      {
        // *****************************************
        // ASSOCIATION TO PROGRAM IS ACTIVE
        // *****************************************
        ExitState = "PROGRAM_ASSOCIATED_TO_PERSON";

        return;
      }
      else
      {
        local.SetDiscontinueDate.Flag = "Y";
      }
    }

    if (ReadCountyService())
    {
      if (!Lt(local.Current.Date, entities.CountyService.EffectiveDate) && Lt
        (local.Current.Date, entities.CountyService.DiscontinueDate))
      {
        // *****************************************
        // ASSOCIATION TO PROGRAM IS ACTIVE
        // *****************************************
        ExitState = "PROGRAM_ASSOCIATED_TO_COUNTY_SER";

        return;
      }
      else
      {
        local.SetDiscontinueDate.Flag = "Y";
      }
    }

    if (ReadOfficeCaseloadAssignment())
    {
      if (Lt(entities.OfficeCaseloadAssignment.EffectiveDate, Now().Date) && Lt
        (local.Current.Date, entities.OfficeCaseloadAssignment.DiscontinueDate))
      {
        // *****************************************
        // ASSOCIATION TO PROGRAM IS ACTIVE
        // *****************************************
        ExitState = "PROGRAM_ASSOCIATED_TO_CASELOAD";

        return;
      }
      else
      {
        local.SetDiscontinueDate.Flag = "Y";
      }
    }

    if (ReadProgram())
    {
      if (AsChar(local.SetDiscontinueDate.Flag) == 'Y')
      {
        // ******************************************
        // CHECK THE DISCOUNTINUE DATE IS NOT ALREADY SET.
        // *****************************************
        if (Lt(local.Current.Date, entities.Program.DiscontinueDate))
        {
          if (ReadProgramIndicator())
          {
            try
            {
              UpdateProgram();
              MoveProgram2(entities.Program, export.Program);

              try
              {
                UpdateProgramIndicator();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
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
            ExitState = "PROGRAM_INDICATOR_NF";
          }
        }
      }
      else
      {
        DeleteProgram();
      }

      ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_3";
    }
    else
    {
      ExitState = "PROGRAM_NF";
    }
  }

  private static void MoveProgram1(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Title = source.Title;
    target.DistributionProgramType = source.DistributionProgramType;
    target.InterstateIndicator = source.InterstateIndicator;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveProgram2(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatdTstamp = source.LastUpdatdTstamp;
  }

  private void DeleteProgram()
  {
    bool exists;

    exists = Read("DeleteProgram#1",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_PERSON_PROGRAM\".",
        "50001");
    }

    Update("DeleteProgram#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCountyService()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prgGeneratedId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 1);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 2);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 3);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadOfficeCaseloadAssignment()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prgGeneratedId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeCaseloadAssignment.Populated = true;
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.EffectiveDate = db.GetDate(reader, 1);
        entities.Program.DiscontinueDate = db.GetDate(reader, 2);
        entities.Program.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Program.LastUpdatdTstamp = db.GetNullableDateTime(reader, 4);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgramIndicator()
  {
    entities.ProgramIndicator.Populated = false;

    return Read("ReadProgramIndicator",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramIndicator.EffectiveDate = db.GetDate(reader, 0);
        entities.ProgramIndicator.DiscontinueDate = db.GetDate(reader, 1);
        entities.ProgramIndicator.PrgGeneratedId = db.GetInt32(reader, 2);
        entities.ProgramIndicator.Populated = true;
      });
  }

  private void UpdateProgram()
  {
    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.Program.Populated = false;
    Update("UpdateProgram",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetInt32(
          command, "programId", entities.Program.SystemGeneratedIdentifier);
      });

    entities.Program.DiscontinueDate = discontinueDate;
    entities.Program.LastUpdatedBy = lastUpdatedBy;
    entities.Program.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Program.Populated = true;
  }

  private void UpdateProgramIndicator()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramIndicator.Populated);

    var discontinueDate = local.Current.Date;

    entities.ProgramIndicator.Populated = false;
    Update("UpdateProgramIndicator",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });

    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.Populated = true;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private Program program;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private Program program;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of SetDiscontinueDate.
    /// </summary>
    [JsonPropertyName("setDiscontinueDate")]
    public Common SetDiscontinueDate
    {
      get => setDiscontinueDate ??= new();
      set => setDiscontinueDate = value;
    }

    private DateWorkArea current;
    private Common setDiscontinueDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    private ProgramIndicator programIndicator;
    private CountyService countyService;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private CsePerson csePerson;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
