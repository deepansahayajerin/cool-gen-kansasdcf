// Program: SI_CHANGE_PGM_IN_PERSON_PGM, ID: 373297431, model: 746.
// Short name: SWE00544
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHANGE_PGM_IN_PERSON_PGM.
/// </summary>
[Serializable]
public partial class SiChangePgmInPersonPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHANGE_PGM_IN_PERSON_PGM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChangePgmInPersonPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChangePgmInPersonPgm.
  /// </summary>
  public SiChangePgmInPersonPgm(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************
    // 09/06/00   M.Lachowicz    WR # 00188.
    //                          This CAB deletes existing
    //                          Person Programs and create new
    //                          one with new person program code.
    // *************************************************************
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadProgram2())
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    if (ReadPersonProgram())
    {
      local.Old.Assign(entities.OldPersonProgram);
      DeletePersonProgram();
    }
    else
    {
      ExitState = "PERSON_PROGRAM_NF";

      return;
    }

    if (!ReadProgram1())
    {
      ExitState = "1099_LOCATE_REQUEST_NF";

      return;
    }

    try
    {
      CreatePersonProgram();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreatePersonProgram()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = local.Old.EffectiveDate;
    var status = local.Old.Status ?? "";
    var closureReason = local.Old.ClosureReason ?? "";
    var assignedDate = local.Old.AssignedDate;
    var discontinueDate = local.Old.DiscontinueDate;
    var createdBy = import.NewPersonProgram.CreatedBy;
    var createdTimestamp = import.NewPersonProgram.CreatedTimestamp;
    var changedInd = local.Old.ChangedInd ?? "";
    var changeDate = local.Old.ChangeDate;
    var prgGeneratedId = entities.NewProgram.SystemGeneratedIdentifier;
    var medTypeDiscontinueDate = local.Old.MedTypeDiscontinueDate;
    var medType = local.Old.MedType ?? "";

    entities.NewPersonProgram.Populated = false;
    Update("CreatePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "closureReason", closureReason);
        db.SetNullableDate(command, "assignedDate", assignedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "changedInd", changedInd);
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetNullableString(command, "medType", medType);
      });

    entities.NewPersonProgram.CspNumber = cspNumber;
    entities.NewPersonProgram.EffectiveDate = effectiveDate;
    entities.NewPersonProgram.Status = status;
    entities.NewPersonProgram.ClosureReason = closureReason;
    entities.NewPersonProgram.AssignedDate = assignedDate;
    entities.NewPersonProgram.DiscontinueDate = discontinueDate;
    entities.NewPersonProgram.CreatedBy = createdBy;
    entities.NewPersonProgram.CreatedTimestamp = createdTimestamp;
    entities.NewPersonProgram.LastUpdatedBy = "";
    entities.NewPersonProgram.LastUpdatdTstamp = null;
    entities.NewPersonProgram.ChangedInd = changedInd;
    entities.NewPersonProgram.ChangeDate = changeDate;
    entities.NewPersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.NewPersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.NewPersonProgram.MedType = medType;
    entities.NewPersonProgram.Populated = true;
  }

  private void DeletePersonProgram()
  {
    Update("DeletePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.OldPersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OldPersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.OldPersonProgram.PrgGeneratedId);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadPersonProgram()
  {
    entities.OldPersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.OldProgram.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.OldPersonProgram.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OldPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.OldPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.OldPersonProgram.Status = db.GetNullableString(reader, 2);
        entities.OldPersonProgram.ClosureReason =
          db.GetNullableString(reader, 3);
        entities.OldPersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.OldPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OldPersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.OldPersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.OldPersonProgram.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.OldPersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.OldPersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.OldPersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.OldPersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.OldPersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.OldPersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.OldPersonProgram.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.NewProgram.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "code", import.NewProgram.Code);
      },
      (db, reader) =>
      {
        entities.NewProgram.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.NewProgram.Code = db.GetString(reader, 1);
        entities.NewProgram.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.OldProgram.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetString(command, "code", import.OldProgram.Code);
      },
      (db, reader) =>
      {
        entities.OldProgram.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.OldProgram.Code = db.GetString(reader, 1);
        entities.OldProgram.Populated = true;
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
    /// A value of OldPersonProgram.
    /// </summary>
    [JsonPropertyName("oldPersonProgram")]
    public PersonProgram OldPersonProgram
    {
      get => oldPersonProgram ??= new();
      set => oldPersonProgram = value;
    }

    /// <summary>
    /// A value of NewPersonProgram.
    /// </summary>
    [JsonPropertyName("newPersonProgram")]
    public PersonProgram NewPersonProgram
    {
      get => newPersonProgram ??= new();
      set => newPersonProgram = value;
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
    /// A value of NewProgram.
    /// </summary>
    [JsonPropertyName("newProgram")]
    public Program NewProgram
    {
      get => newProgram ??= new();
      set => newProgram = value;
    }

    /// <summary>
    /// A value of OldProgram.
    /// </summary>
    [JsonPropertyName("oldProgram")]
    public Program OldProgram
    {
      get => oldProgram ??= new();
      set => oldProgram = value;
    }

    private PersonProgram oldPersonProgram;
    private PersonProgram newPersonProgram;
    private CsePerson csePerson;
    private Program newProgram;
    private Program oldProgram;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public PersonProgram Old
    {
      get => old ??= new();
      set => old = value;
    }

    private PersonProgram old;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewPersonProgram.
    /// </summary>
    [JsonPropertyName("newPersonProgram")]
    public PersonProgram NewPersonProgram
    {
      get => newPersonProgram ??= new();
      set => newPersonProgram = value;
    }

    /// <summary>
    /// A value of OldPersonProgram.
    /// </summary>
    [JsonPropertyName("oldPersonProgram")]
    public PersonProgram OldPersonProgram
    {
      get => oldPersonProgram ??= new();
      set => oldPersonProgram = value;
    }

    /// <summary>
    /// A value of NewProgram.
    /// </summary>
    [JsonPropertyName("newProgram")]
    public Program NewProgram
    {
      get => newProgram ??= new();
      set => newProgram = value;
    }

    /// <summary>
    /// A value of OldProgram.
    /// </summary>
    [JsonPropertyName("oldProgram")]
    public Program OldProgram
    {
      get => oldProgram ??= new();
      set => oldProgram = value;
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

    private PersonProgram newPersonProgram;
    private PersonProgram oldPersonProgram;
    private Program newProgram;
    private Program oldProgram;
    private CsePerson csePerson;
  }
#endregion
}
