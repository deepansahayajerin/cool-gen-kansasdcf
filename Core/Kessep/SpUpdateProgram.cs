// Program: SP_UPDATE_PROGRAM, ID: 371745835, model: 746.
// Short name: SWE01447
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_PROGRAM.
/// </summary>
[Serializable]
public partial class SpUpdateProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateProgram.
  /// </summary>
  public SpUpdateProgram(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Program.Assign(import.Program);
    local.Current.Date = Now().Date;

    // ************************************************
    // * 04/30/97	R. Grey		Change Current Date
    // ************************************************
    if (ReadProgram())
    {
      try
      {
        UpdateProgram();
        export.Program.Assign(entities.Program);
        ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_3";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_PV";

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
      ExitState = "PROGRAM_NF";
    }

    // **** Check to see if Program_Indicator needs to be changed ****
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
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Title = db.GetString(reader, 2);
        entities.Program.InterstateIndicator = db.GetString(reader, 3);
        entities.Program.EffectiveDate = db.GetDate(reader, 4);
        entities.Program.DiscontinueDate = db.GetDate(reader, 5);
        entities.Program.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Program.LastUpdatdTstamp = db.GetNullableDateTime(reader, 7);
        entities.Program.DistributionProgramType = db.GetString(reader, 8);
        entities.Program.Populated = true;
      });
  }

  private void UpdateProgram()
  {
    var title = import.Program.Title;
    var interstateIndicator = import.Program.InterstateIndicator;
    var effectiveDate = import.Program.EffectiveDate;
    var discontinueDate = import.Program.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var distributionProgramType = import.Program.DistributionProgramType;

    entities.Program.Populated = false;
    Update("UpdateProgram",
      (db, command) =>
      {
        db.SetString(command, "title", title);
        db.SetString(command, "interstateInd", interstateIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "distrbtnPrgmType", distributionProgramType);
        db.SetInt32(
          command, "programId", entities.Program.SystemGeneratedIdentifier);
      });

    entities.Program.Title = title;
    entities.Program.InterstateIndicator = interstateIndicator;
    entities.Program.EffectiveDate = effectiveDate;
    entities.Program.DiscontinueDate = discontinueDate;
    entities.Program.LastUpdatedBy = lastUpdatedBy;
    entities.Program.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Program.DistributionProgramType = distributionProgramType;
    entities.Program.Populated = true;
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
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
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

    private ProgramIndicator programIndicator;
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
    /// A value of CurrentEffIndFound.
    /// </summary>
    [JsonPropertyName("currentEffIndFound")]
    public Common CurrentEffIndFound
    {
      get => currentEffIndFound ??= new();
      set => currentEffIndFound = value;
    }

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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of NextEffIndFound.
    /// </summary>
    [JsonPropertyName("nextEffIndFound")]
    public Common NextEffIndFound
    {
      get => nextEffIndFound ??= new();
      set => nextEffIndFound = value;
    }

    private DateWorkArea current;
    private Common currentEffIndFound;
    private ProgramIndicator programIndicator;
    private NullDate nullDate;
    private Common nextEffIndFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ProgramIndicator New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Future.
    /// </summary>
    [JsonPropertyName("future")]
    public ProgramIndicator Future
    {
      get => future ??= new();
      set => future = value;
    }

    /// <summary>
    /// A value of FutureOrActive.
    /// </summary>
    [JsonPropertyName("futureOrActive")]
    public ProgramIndicator FutureOrActive
    {
      get => futureOrActive ??= new();
      set => futureOrActive = value;
    }

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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public ProgramIndicator Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NextEffective.
    /// </summary>
    [JsonPropertyName("nextEffective")]
    public ProgramIndicator NextEffective
    {
      get => nextEffective ??= new();
      set => nextEffective = value;
    }

    private ProgramIndicator new1;
    private ProgramIndicator future;
    private ProgramIndicator futureOrActive;
    private ProgramIndicator programIndicator;
    private Program program;
    private ProgramIndicator current;
    private ProgramIndicator nextEffective;
  }
#endregion
}
