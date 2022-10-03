// Program: FN_157_GET_ASSISTANCE_FOR_PERSON, ID: 371095494, model: 746.
// Short name: SWE02932
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_157_GET_ASSISTANCE_FOR_PERSON.
/// </summary>
[Serializable]
public partial class Fn157GetAssistanceForPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_157_GET_ASSISTANCE_FOR_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Fn157GetAssistanceForPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Fn157GetAssistanceForPerson.
  /// </summary>
  public Fn157GetAssistanceForPerson(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // 06/2001 - K Doshi - WR10367 - Initial code.
    // Values Exported
    // C - current assistance
    // F - former assistance
    // Any other value should be treated as never assistance in calling program.
    // -------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // 08/04/2001
    // Replace persistent import cse_person view with transient view.
    // -------------------------------------------------------------------------
    // -------------------------------------------------------
    // For Current Assistance, read active PEPR as of FY end.
    // -------------------------------------------------------
    if (ReadPersonProgramProgram1())
    {
      export.AssistanceProgram.Flag = "C";
      export.Program.Code = entities.Program.Code;

      return;
    }

    // -------------------------------------------------------
    // For Former Assistance, read all case roles before FY end.
    // -------------------------------------------------------
    if (ReadPersonProgramProgram2())
    {
      export.AssistanceProgram.Flag = "F";
      export.Program.Code = entities.Program.Code;
    }
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private DateWorkArea reportEndDate;
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
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    private Program program;
    private Common assistanceProgram;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
