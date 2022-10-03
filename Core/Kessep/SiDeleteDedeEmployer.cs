// Program: SI_DELETE_DEDE_EMPLOYER, ID: 371076297, model: 746.
// Short name: SWE01652
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DELETE_DEDE_EMPLOYER.
/// </summary>
[Serializable]
public partial class SiDeleteDedeEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_DEDE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteDedeEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteDedeEmployer.
  /// </summary>
  public SiDeleteDedeEmployer(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer	Request	Description
    // 01-23-01  GVandy	WR267	Initial Development
    // ------------------------------------------------------------
    if (!ReadEmployer2())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    if (!ReadEmployer1())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    // -- Transfer income source records from the duplicate employer to the 
    // correct employer.
    foreach(var item in ReadIncomeSource())
    {
      TransferIncomeSource();
    }

    // -- Delete the duplicate employer.  The cascase deletion rules will delete
    // the associated employer_address, employer_relation, and
    // employer_registered_agent rows.
    DeleteEmployer();
  }

  private void DeleteEmployer()
  {
    Update("DeleteEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Duplicate.Identifier);
      });
  }

  private bool ReadEmployer1()
  {
    entities.Correct.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Correct.Identifier);
      },
      (db, reader) =>
      {
        entities.Correct.Identifier = db.GetInt32(reader, 0);
        entities.Correct.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Duplicate.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Duplicate.Identifier);
      },
      (db, reader) =>
      {
        entities.Duplicate.Identifier = db.GetInt32(reader, 0);
        entities.Duplicate.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Duplicate.Identifier);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.Populated = true;

        return true;
      });
  }

  private void TransferIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var empId = entities.Correct.Identifier;

    entities.IncomeSource.Populated = false;
    Update("TransferIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", empId);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.EmpId = empId;
    entities.IncomeSource.Populated = true;
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
    /// A value of Correct.
    /// </summary>
    [JsonPropertyName("correct")]
    public Employer Correct
    {
      get => correct ??= new();
      set => correct = value;
    }

    /// <summary>
    /// A value of Duplicate.
    /// </summary>
    [JsonPropertyName("duplicate")]
    public Employer Duplicate
    {
      get => duplicate ??= new();
      set => duplicate = value;
    }

    private Employer correct;
    private Employer duplicate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Correct.
    /// </summary>
    [JsonPropertyName("correct")]
    public Employer Correct
    {
      get => correct ??= new();
      set => correct = value;
    }

    /// <summary>
    /// A value of Duplicate.
    /// </summary>
    [JsonPropertyName("duplicate")]
    public Employer Duplicate
    {
      get => duplicate ??= new();
      set => duplicate = value;
    }

    private IncomeSource incomeSource;
    private Employer correct;
    private Employer duplicate;
  }
#endregion
}
