// Program: DETERMINE_TYPE_OF_ASSISTANCE, ID: 372780317, model: 746.
// Short name: SWEFC710
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DETERMINE_TYPE_OF_ASSISTANCE.
/// </para>
/// <para>
/// Determine whether the Child has 'Current Assistance', 'Former Assistance' or
/// 'Never Assistance'.
/// </para>
/// </summary>
[Serializable]
public partial class DetermineTypeOfAssistance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DETERMINE_TYPE_OF_ASSISTANCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DetermineTypeOfAssistance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DetermineTypeOfAssistance.
  /// </summary>
  public DetermineTypeOfAssistance(IContext context, Import import,
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
    // *******************************************************************
    // *
    // * C H A N G E   L O G
    // *
    // *******************************************************************
    // *
    // *   Date   Programmer     Reason/Modification
    // * -------- ----------     -------
    // * 08/27/99 C. Fairley     Program abended due to excessive DB2
    // *                         locks/changed the READ statements to
    // *                         SUMMARIZE statements
    // *
    // *******************************************************************
    export.Current.Flag = "N";
    export.Former.Flag = "N";
    export.Never.Flag = "N";
    ReadProgram2();

    if (local.Program.SystemGeneratedIdentifier != 0)
    {
      export.Current.Flag = "Y";

      return;
    }

    ReadProgram1();

    if (local.Program.SystemGeneratedIdentifier != 0)
    {
      export.Former.Flag = "Y";

      return;
    }

    export.Never.Flag = "Y";
  }

  private bool ReadProgram1()
  {
    local.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        local.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        local.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    local.Program.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        local.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        local.Program.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private Case1 case1;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Former.
    /// </summary>
    [JsonPropertyName("former")]
    public Common Former
    {
      get => former ??= new();
      set => former = value;
    }

    /// <summary>
    /// A value of Never.
    /// </summary>
    [JsonPropertyName("never")]
    public Common Never
    {
      get => never ??= new();
      set => never = value;
    }

    private EabReportSend neededToWrite;
    private Common errorFound;
    private Common current;
    private Common former;
    private Common never;
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

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 one;
  }
#endregion
}
