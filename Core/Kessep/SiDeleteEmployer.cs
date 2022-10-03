// Program: SI_DELETE_EMPLOYER, ID: 371762199, model: 746.
// Short name: SWE01153
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_DELETE_EMPLOYER.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiDeleteEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteEmployer.
  /// </summary>
  public SiDeleteEmployer(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 02-29-96 K. Evans    Initial Development
    // 01-11-99 M.Lachowicz Delete entity view
    //                      zdel zdel_cse_person_employer.
    //                      PR #85022.
    // 02/01/2017 D Dupree  Added check on employer relationship
    // --------------------------------------------
    if (ReadEmployer())
    {
      if (ReadIncomeSource())
      {
        ExitState = "SI0000_CANT_DEL_INC_SOURCE_REL";

        return;
      }

      if (ReadEmployerRelation1())
      {
        ExitState = "CANNOT_DELETE_EMPL_RELATION_EXST";

        return;
      }

      if (ReadEmployerRelation2())
      {
        ExitState = "CANNOT_DELETE_EMPL_RELATION_EXST";

        return;
      }

      DeleteEmployer();
    }
    else
    {
      ExitState = "EMPLOYER_NF";
    }
  }

  private void DeleteEmployer()
  {
    Update("DeleteEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Employer.Identifier);
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerRelation1()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation1",
      (db, command) =>
      {
        db.SetInt32(command, "empHqId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 1);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 2);
        entities.EmployerRelation.Type1 = db.GetString(reader, 3);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelation2()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation2",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 1);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 2);
        entities.EmployerRelation.Type1 = db.GetString(reader, 3);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Employer employer;
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
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private EmployerRelation employerRelation;
    private Employer employer;
    private IncomeSource incomeSource;
  }
#endregion
}
