// Program: FN_157_GET_ASSISTANCE_FOR_CASE, ID: 371095109, model: 746.
// Short name: SWE02933
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_157_GET_ASSISTANCE_FOR_CASE.
/// </summary>
[Serializable]
public partial class Fn157GetAssistanceForCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_157_GET_ASSISTANCE_FOR_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Fn157GetAssistanceForCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Fn157GetAssistanceForCase.
  /// </summary>
  public Fn157GetAssistanceForCase(IContext context, Import import,
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
    // ---------------------------------------------
    // 05/2001 - K Doshi - WR10367 - Initial code.
    // Values Exported
    // C - current assistance
    // F - former assistance
    // Any other value should be treated as never assistance in calling program.
    // When import_get_medicaid flag is set,
    // M - medicaid only program never on assistance.
    // ---------------------------------------------
    // -------------------------------------------------------
    // For Current Assistance, read case roles as of FY end.
    // -------------------------------------------------------
    if (ReadPersonProgramCsePersonProgram1())
    {
      export.Program.Code = entities.Program.Code;
      export.CsePerson.Number = entities.CsePerson.Number;
      export.AssistanceProgram.Flag = "C";

      return;
    }

    // -------------------------------------------------------
    // For Former Assistance, read all case roles before FY end.
    // Only look at person programs whilst person is on the case.
    // -------------------------------------------------------
    if (ReadPersonProgramCsePersonProgram3())
    {
      export.Program.Code = entities.Program.Code;
      export.CsePerson.Number = entities.CsePerson.Number;
      export.AssistanceProgram.Flag = "F";

      return;
    }

    if (AsChar(import.GetMedicaidOnlyProgram.Flag) != 'Y')
    {
      return;
    }

    if (ReadPersonProgramCsePersonProgram2())
    {
      export.Program.Code = entities.Program.Code;
      export.CsePerson.Number = entities.CsePerson.Number;
      export.AssistanceProgram.Flag = "M";
    }
  }

  private bool ReadPersonProgramCsePersonProgram1()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadPersonProgramCsePersonProgram1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadPersonProgramCsePersonProgram2()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadPersonProgramCsePersonProgram2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadPersonProgramCsePersonProgram3()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadPersonProgramCsePersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
        entities.CsePerson.Populated = true;
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
    /// A value of GetMedicaidOnlyProgram.
    /// </summary>
    [JsonPropertyName("getMedicaidOnlyProgram")]
    public Common GetMedicaidOnlyProgram
    {
      get => getMedicaidOnlyProgram ??= new();
      set => getMedicaidOnlyProgram = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Common getMedicaidOnlyProgram;
    private DateWorkArea reportEndDate;
    private Case1 case1;
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
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    private Program program;
    private CsePerson csePerson;
    private Common assistanceProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
