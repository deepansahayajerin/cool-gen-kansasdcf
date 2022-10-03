// Program: FN_CHECK_FOR_ACTIVE_AF_FC_PGM, ID: 371113368, model: 746.
// Short name: SWE02969
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CHECK_FOR_ACTIVE_AF_FC_PGM.
/// </summary>
[Serializable]
public partial class FnCheckForActiveAfFcPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_ACTIVE_AF_FC_PGM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForActiveAfFcPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForActiveAfFcPgm.
  /// </summary>
  public FnCheckForActiveAfFcPgm(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------
    // CAB returns Y if all CH/AR on case have AF, AFI, FC, FCI
    // active as of import date.
    // -------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      foreach(var item1 in ReadPersonProgramProgram())
      {
        export.AssistanceProgram.Flag = "Y";

        goto ReadEach;
      }

      export.AssistanceProgram.Flag = "N";

      return;

ReadEach:
      ;
    }
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.AsOfDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", import.AsOfDate.Date.GetValueOrDefault());
          
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
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
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
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
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

    private DateWorkArea asOfDate;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    private Common assistanceProgram;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Program program;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private CaseRole caseRole;
  }
#endregion
}
