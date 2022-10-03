// Program: SI_READ_CASE_PROGRAM_TYPE, ID: 371732397, model: 746.
// Short name: SWE01203
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
/// A program: SI_READ_CASE_PROGRAM_TYPE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure provide information or details on the current Program for a 
/// Case. It retrieve the programs for every child belogings to IMPORTED case (
/// number). It determine the overiding Program base on priorities assigned each
/// Program. The assigned program hierichial level is as follows:
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
public partial class SiReadCaseProgramType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CASE_PROGRAM_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCaseProgramType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCaseProgramType.
  /// </summary>
  public SiReadCaseProgramType(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E    L O G
    //   Date   Developer  Request#   Description
    // ---------------------------------------------------------------------------------------
    // 01/26/96 P. Elie               Initial development of action block.
    // 05/02/96 G. Lofton             Rework
    // 10/18/99 D Lowry    H00077894  Performance issues
    // 05/21/01 C Fairley  WR 010357  Case level program for "NC" should be "NC"
    // - not "NA".
    //                                
    // (Business rule change per Ralph
    // Malott)
    //                                
    // Removed OLD commented out code.
    // 09/22/03 A Doty     WR 040134  Modified the derivation of programs for
    //                                
    // case to match the business
    // rules.
    // ---------------------------------------------------------------------------------------
    export.Program.Code = "";
    export.MedProgExists.Flag = "N";

    if (import.Current.Date != null)
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    // *** Changed on 10/25/1999 by Carl Galka / David Lowry for performance. 
    // The result will
    // *** be one READ EACH, instead of nesting them. Also, we escape if the 
    // EXPORT MEDICAL
    // *** Flag is Y and we have set the local AF flag to Y.
    foreach(var item in ReadPersonProgramProgram())
    {
      switch(TrimEnd(entities.Program.Code))
      {
        case "AF":
          local.Af.Flag = "Y";

          break;
        case "CC":
          local.Na.Flag = "Y";

          break;
        case "CI":
          local.Na.Flag = "Y";

          break;
        case "FC":
          local.Fc.Flag = "Y";

          break;
        case "FS":
          local.Na.Flag = "Y";

          break;
        case "MA":
          local.Na.Flag = "Y";
          export.MedProgExists.Flag = "Y";

          break;
        case "MK":
          local.Na.Flag = "Y";
          export.MedProgExists.Flag = "Y";

          break;
        case "MP":
          local.Na.Flag = "Y";
          export.MedProgExists.Flag = "Y";

          break;
        case "MS":
          local.Na.Flag = "Y";
          export.MedProgExists.Flag = "Y";

          break;
        case "NA":
          local.Na.Flag = "Y";

          break;
        case "NC":
          local.Nc.Flag = "Y";

          break;
        case "NF":
          local.Nf.Flag = "Y";

          break;
        case "SI":
          local.Na.Flag = "Y";

          break;
        case "AFI":
          local.Afi.Flag = "Y";

          break;
        case "FCI":
          local.Fci.Flag = "Y";

          break;
        case "MAI":
          local.Mai.Flag = "Y";

          break;
        case "NAI":
          local.Nai.Flag = "Y";

          break;
        default:
          break;
      }
    }

    if (AsChar(local.Fc.Flag) == 'Y')
    {
      export.Program.Code = "FC";
    }
    else if (AsChar(local.Af.Flag) == 'Y')
    {
      export.Program.Code = "AF";
    }
    else if (AsChar(local.Nf.Flag) == 'Y')
    {
      export.Program.Code = "NF";
    }
    else if (AsChar(local.Nc.Flag) == 'Y')
    {
      export.Program.Code = "NC";
    }
    else if (AsChar(local.Na.Flag) == 'Y')
    {
      export.Program.Code = "NA";
    }
    else if (AsChar(local.Fci.Flag) == 'Y')
    {
      export.Program.Code = "FCI";
    }
    else if (AsChar(local.Afi.Flag) == 'Y')
    {
      export.Program.Code = "AFI";
    }
    else if (AsChar(local.Mai.Flag) == 'Y')
    {
      export.Program.Code = "MAI";
    }
    else if (AsChar(local.Nai.Flag) == 'Y')
    {
      export.Program.Code = "NAI";
    }
    else
    {
      ExitState = "SI0000_PERSON_PROGRAM_CASE_NF";
    }
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
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

    /// <summary>
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
    }

    private Program program;
    private Common medProgExists;
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
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Common Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Nc.
    /// </summary>
    [JsonPropertyName("nc")]
    public Common Nc
    {
      get => nc ??= new();
      set => nc = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public Common Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of Afi.
    /// </summary>
    [JsonPropertyName("afi")]
    public Common Afi
    {
      get => afi ??= new();
      set => afi = value;
    }

    /// <summary>
    /// A value of Fci.
    /// </summary>
    [JsonPropertyName("fci")]
    public Common Fci
    {
      get => fci ??= new();
      set => fci = value;
    }

    /// <summary>
    /// A value of Mai.
    /// </summary>
    [JsonPropertyName("mai")]
    public Common Mai
    {
      get => mai ??= new();
      set => mai = value;
    }

    /// <summary>
    /// A value of Nai.
    /// </summary>
    [JsonPropertyName("nai")]
    public Common Nai
    {
      get => nai ??= new();
      set => nai = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    private Common na;
    private Common nc;
    private Common nf;
    private Common afi;
    private Common fci;
    private Common mai;
    private Common nai;
    private DateWorkArea zero;
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
