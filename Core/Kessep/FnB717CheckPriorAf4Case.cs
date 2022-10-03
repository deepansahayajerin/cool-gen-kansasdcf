// Program: FN_B717_CHECK_PRIOR_AF_4_CASE, ID: 373348776, model: 746.
// Short name: SWE02998
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_CHECK_PRIOR_AF_4_CASE.
/// </summary>
[Serializable]
public partial class FnB717CheckPriorAf4Case: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_CHECK_PRIOR_AF_4_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717CheckPriorAf4Case(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717CheckPriorAf4Case.
  /// </summary>
  public FnB717CheckPriorAf4Case(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.PriorAf.Flag = "N";

    // -------------------------------------------------------
    // Case level check for prior AF
    // Read programs for active and inactive CH/AR
    // Only read programs for timeframe when CH/AR is 'on' case
    // -------------------------------------------------------
    if (ReadPersonProgramCsePersonProgram())
    {
      export.PriorAf.Flag = "Y";
    }
  }

  private bool ReadPersonProgramCsePersonProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadPersonProgramCsePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.Program.Code = db.GetString(reader, 6);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
        entities.CsePerson.Populated = true;
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
    /// A value of PriorAf.
    /// </summary>
    [JsonPropertyName("priorAf")]
    public Common PriorAf
    {
      get => priorAf ??= new();
      set => priorAf = value;
    }

    private Common priorAf;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private PersonProgram personProgram;
    private Program program;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
