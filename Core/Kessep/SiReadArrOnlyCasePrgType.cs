// Program: SI_READ_ARR_ONLY_CASE_PRG_TYPE, ID: 373548293, model: 746.
// Short name: SWE02026
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_ARR_ONLY_CASE_PRG_TYPE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure provides information or details on the arrears Program for a 
/// Case when there are no open programs.  It retrieves the closed programs for
/// every child belonging to IMPORTED case (number).  It determines the
/// overriding Program based on priorities assigned each Program. The assigned
/// program hierarchial level is as follows:
///           AF            - 1 /highest priority
///           FC            - 2
///           NF            - 3
///           CC, CI, FS,   - 4
///           MA, MP, MS,
///           NA, NC, SI
///           AFI, FCI,     - 5
///           MAI, NAI
/// </para>
/// </summary>
[Serializable]
public partial class SiReadArrOnlyCasePrgType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_ARR_ONLY_CASE_PRG_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadArrOnlyCasePrgType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadArrOnlyCasePrgType.
  /// </summary>
  public SiReadArrOnlyCasePrgType(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // Date		Developer	Request#	Description
    // 03/14/00        C. Ott          PR # 85011      Retrieves a program type 
    // for an arrears only case - no programs currently open.  This action block
    // is used to derive an arrears program type code to send on outgoing
    // interstate transactions.  Only AF and FC programs are considered for
    // arrears only case type.
    // ------------------------------------------------------------------
    export.Program.Code = "";

    if (import.Current.Date != null)
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    foreach(var item in ReadPersonProgramProgram())
    {
      switch(TrimEnd(entities.Program.Code))
      {
        case "AF":
          local.Af.Flag = "Y";

          goto ReadEach;
        case "FC":
          local.Fc.Flag = "Y";

          goto ReadEach;
        case "NF":
          local.Fc.Flag = "Y";

          goto ReadEach;
        default:
          break;
      }
    }

ReadEach:

    if (AsChar(local.Af.Flag) == 'Y')
    {
      export.Program.Code = "R";
    }
    else if (AsChar(local.Fc.Flag) == 'Y')
    {
      export.Program.Code = "C";
    }
    else
    {
      export.Program.Code = "NA";
    }
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
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

    private Program program;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Common Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public Common Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Case1 case1;
    private Program program;
    private Common af;
    private Common fc;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
