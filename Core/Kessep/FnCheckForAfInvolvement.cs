// Program: FN_CHECK_FOR_AF_INVOLVEMENT, ID: 372544237, model: 746.
// Short name: SWE02410
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHECK_FOR_AF_INVOLVEMENT.
/// </para>
/// <para>
/// Check for AF involvement within the imported timeframe. Either the AR or CH 
/// person number will be imported. If AR, check for AF involment. If CH first
/// check on the CH, and if AF involvement isn't found there then check their AR
/// as well.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForAfInvolvement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_AF_INVOLVEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForAfInvolvement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForAfInvolvement.
  /// </summary>
  public FnCheckForAfInvolvement(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // Created 12/7/98. RK
    // Changed 9/15/99  Mike Fangman - Changed code to remove use of case & 
    // duplicate read of AR person program and to pass back an AF involvement
    // indicator.
    // ****************************************************************
    export.AfInvolvementInd.Flag = "N";
    local.DateMinus90Days.Date =
      AddDays(import.ProgramProcessingInfo.ProcessDate, -90);

    if (ReadPersonProgram1())
    {
      export.AfInvolvementInd.Flag = "Y";
    }
    else if (!IsEmpty(import.Child.Number))
    {
      if (ReadPersonProgram2())
      {
        export.AfInvolvementInd.Flag = "Y";
      }
      else
      {
        // Continue
      }
    }
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.DateMinus90Days.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.DateMinus90Days.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.PersonProgram.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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

    private CsePerson ar;
    private CsePerson child;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AfInvolvementInd.
    /// </summary>
    [JsonPropertyName("afInvolvementInd")]
    public Common AfInvolvementInd
    {
      get => afInvolvementInd ??= new();
      set => afInvolvementInd = value;
    }

    private Common afInvolvementInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateMinus90Days.
    /// </summary>
    [JsonPropertyName("dateMinus90Days")]
    public DateWorkArea DateMinus90Days
    {
      get => dateMinus90Days ??= new();
      set => dateMinus90Days = value;
    }

    private DateWorkArea dateMinus90Days;
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

    private CsePerson csePerson;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
