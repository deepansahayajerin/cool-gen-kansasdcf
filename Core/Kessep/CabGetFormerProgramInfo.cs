// Program: CAB_GET_FORMER_PROGRAM_INFO, ID: 372927320, model: 746.
// Short name: SWEFC715
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_GET_FORMER_PROGRAM_INFO.
/// </summary>
[Serializable]
public partial class CabGetFormerProgramInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GET_FORMER_PROGRAM_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGetFormerProgramInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGetFormerProgramInfo.
  /// </summary>
  public CabGetFormerProgramInfo(IContext context, Import import, Export export):
    
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
    // *                  M A I N T E N A N C E    L O G
    // *
    // *******************************************************************
    // *
    // *   Date   Programmer     Reason/Modification
    // * -------- ----------     -------------------
    // * 10/27/99 C. Fairley     New action block
    // *
    // *******************************************************************
    export.CaseVerificationExtract.Assign(import.CaseVerificationExtract);

    if (ReadCaseProgramCaseRoleCsePersonPersonProgram())
    {
      export.CaseVerificationExtract.CrIdentifier =
        entities.CaseRole.Identifier;
      export.CaseVerificationExtract.CrType = entities.CaseRole.Type1;
      export.CaseVerificationExtract.CrStartDate = entities.CaseRole.StartDate;
      export.CaseVerificationExtract.CrEndDate = entities.CaseRole.EndDate;
      export.CaseVerificationExtract.CpNumber = entities.CsePerson.Number;
      export.CaseVerificationExtract.CpType = entities.CsePerson.Type1;
      export.CaseVerificationExtract.PpCreatedTimestamp =
        entities.PersonProgram.CreatedTimestamp;
      export.CaseVerificationExtract.PpEffDate =
        entities.PersonProgram.EffectiveDate;
      export.CaseVerificationExtract.PpDiscDate =
        entities.PersonProgram.DiscontinueDate;
      export.CaseVerificationExtract.Pcode = entities.Program.Code;
      export.CaseVerificationExtract.PeffDate = entities.Program.EffectiveDate;
      export.CaseVerificationExtract.PdiscDate =
        entities.Program.DiscontinueDate;
    }
  }

  private bool ReadCaseProgramCaseRoleCsePersonPersonProgram()
  {
    entities.Current.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadCaseProgramCaseRoleCsePersonPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CaseVerificationExtract.Cnumber);
        db.SetNullableDate(
          command, "endDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Current.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Current.Status = db.GetNullableString(reader, 1);
        entities.Current.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.Program.Code = db.GetString(reader, 4);
        entities.Program.EffectiveDate = db.GetDate(reader, 5);
        entities.Program.DiscontinueDate = db.GetDate(reader, 6);
        entities.CaseRole.CspNumber = db.GetString(reader, 7);
        entities.CsePerson.Number = db.GetString(reader, 7);
        entities.PersonProgram.CspNumber = db.GetString(reader, 7);
        entities.CaseRole.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Identifier = db.GetInt32(reader, 9);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePerson.Type1 = db.GetString(reader, 12);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 13);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 14);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.Current.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of CaseVerificationExtract.
    /// </summary>
    [JsonPropertyName("caseVerificationExtract")]
    public CaseVerificationExtract CaseVerificationExtract
    {
      get => caseVerificationExtract ??= new();
      set => caseVerificationExtract = value;
    }

    private DateWorkArea max;
    private CaseVerificationExtract caseVerificationExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseVerificationExtract.
    /// </summary>
    [JsonPropertyName("caseVerificationExtract")]
    public CaseVerificationExtract CaseVerificationExtract
    {
      get => caseVerificationExtract ??= new();
      set => caseVerificationExtract = value;
    }

    private CaseVerificationExtract caseVerificationExtract;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Case1 Current
    {
      get => current ??= new();
      set => current = value;
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

    private Case1 current;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
