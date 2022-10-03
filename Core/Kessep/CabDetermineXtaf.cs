// Program: CAB_DETERMINE_XTAF, ID: 372817552, model: 746.
// Short name: SWEFE740
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
/// A program: CAB_DETERMINE_XTAF.
/// </para>
/// <para>
/// AF or AFI case with arrears only, NA case with AF arrears or NAI case with 
/// AFI arrears
/// </para>
/// </summary>
[Serializable]
public partial class CabDetermineXtaf: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_DETERMINE_XTAF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabDetermineXtaf(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabDetermineXtaf.
  /// </summary>
  public CabDetermineXtaf(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // Input should be a CSE PERSON number, this number MUST be for a 'CH' or '
    // AR' Case Role.
    // Business Rule(s):
    // XTAF (formerly XAFDC) is defined as follows:
    // 1) a open Case with arrears, NO current Program, but previously had an "
    // AF" or "AFI" Program
    // 2) a open Case with arrears, a current "NA" or "NAI" Program and 
    // previously had an "AF" or "AFI' Program.
    // To make this determination, the Program for all Case (household)
    // participants ("CH" and "AR") must be checked.
    // EXAMPLE(s):
    // 1) A collection for arrears was received for a child, there is no current
    // Program, the child previously had an "AF" program ......accumulate as
    // XTAF
    // 2) A collection for arrears was received for a child, there is a current
    // "NA" Program, the child previously had an "AF" program ......accumulate
    // as XTAF
    // *****************************************************************
    export.XtafFound.Flag = "N";
    export.ErrorFound.Flag = "N";
    local.Max.Date = new DateTime(2099, 12, 31);

    // ***
    // *** get each Case Role for CSE Person, where
    // *** CSE Person number = imported CSE Person number
    // ***
    foreach(var item in ReadCaseRole1())
    {
      // ***
      // *** get Case for the current Case Role
      // ***
      if (ReadCase())
      {
        break;
      }
      else
      {
        export.ErrorFound.Flag = "Y";
        export.NeededToWrite.RptDetail =
          "Case not found for Case Role with Identifier " + NumberToString
          (entities.CaseRole.Identifier, 15) + " Type " + entities
          .CaseRole.Type1;

        return;
      }
    }

    // ***
    // *** get each Case Role ("CH" or" AR") for current Case,
    // *** all Case participants must be checked
    // ***
    foreach(var item in ReadCaseRole2())
    {
      if (!Equal(entities.CaseRole.Type1, "AR") && !
        Equal(entities.CaseRole.Type1, "CH"))
      {
        continue;
      }

      // ***
      // *** get CSE Person for current Case Role
      // ***
      if (!ReadCsePerson())
      {
        export.ErrorFound.Flag = "Y";
        export.NeededToWrite.RptDetail =
          "CSE Person not found for Case Role with Identifier " + NumberToString
          (entities.CaseRole.Identifier, 15) + " Type " + entities
          .CaseRole.Type1;

        return;
      }

      // *** Determine if the Case (household) has a current Program
      // *** (Discontinue Date = 2099-12-31)
      // ***
      // *** get each Person Program/Program combination for
      // *** current CSE Person
      // ***
      if (ReadPersonProgramProgram1())
      {
        if (Equal(entities.Program.Code, "NA") || Equal
          (entities.Program.Code, "NAI"))
        {
          goto Read;
        }

        export.XtafFound.Flag = "N";

        return;
      }

Read:

      // *** Determine if the Case (household) had a previous
      // *** "AF" or "AFI' Program
      // *** (Discontinue Date not = 2099-12-31 and => Case open date)
      // ***
      // *** get each Person Program/Program combination for
      // *** current CSE Person
      // ***
      foreach(var item1 in ReadPersonProgramProgram2())
      {
        if (Equal(entities.Program.Code, "AF") || Equal
          (entities.Program.Code, "AFI"))
        {
          export.XtafFound.Flag = "Y";

          break;
        }
      }
    }
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Program.Code = db.GetString(reader, 4);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram2()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate1", local.Max.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate2",
          entities.Case1.CseOpenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Program.Code = db.GetString(reader, 4);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private CsePerson csePerson;
    private Collection collection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of XtafFound.
    /// </summary>
    [JsonPropertyName("xtafFound")]
    public Common XtafFound
    {
      get => xtafFound ??= new();
      set => xtafFound = value;
    }

    private Common errorFound;
    private EabReportSend neededToWrite;
    private Common xtafFound;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CaseRole caseRole;
    private Case1 case1;
    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
  }
#endregion
}
