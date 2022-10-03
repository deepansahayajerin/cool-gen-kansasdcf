// Program: SI_DISPLAY_DEDE_EMPLOYER, ID: 371076367, model: 746.
// Short name: SWE01938
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DISPLAY_DEDE_EMPLOYER.
/// </summary>
[Serializable]
public partial class SiDisplayDedeEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DISPLAY_DEDE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDisplayDedeEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDisplayDedeEmployer.
  /// </summary>
  public SiDisplayDedeEmployer(IContext context, Import import, Export export):
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
    export.Employer.Identifier = import.Employer.Identifier;
    local.CurrentDate.Date = Now().Date;

    if (ReadEmployerEmployerAddress())
    {
      export.Employer.Assign(entities.Employer);
      export.EmployerAddress.Assign(entities.EmployerAddress);
    }
    else
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    // -- Determine if the employer is a headquarters for another employer.
    if (ReadEmployerRelation1())
    {
      export.Headquarters.Flag = "Y";
    }
    else
    {
      export.Headquarters.Flag = "N";
    }

    // -- Determine if the employer is a worksite for another employer.
    if (ReadEmployerRelation2())
    {
      export.Worksite.Flag = "Y";
    }
    else
    {
      export.Worksite.Flag = "N";
    }

    // -- Determine if the employer has a registered agent.
    if (ReadEmployerRegisteredAgent())
    {
      export.RegisteredAgent.Flag = "Y";
    }
    else
    {
      export.RegisteredAgent.Flag = "N";
    }

    // -- Determine if the employer has income source records (ties to 
    // employment).
    ReadIncomeSource();

    if (local.Common.Count == 0)
    {
      export.EmploymentTies.Flag = "N";
    }
    else
    {
      export.EmploymentTies.Flag = "Y";
    }
  }

  private bool ReadEmployerEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 4);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 5);
        entities.EmployerAddress.LocationType = db.GetString(reader, 6);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 8);
        entities.EmployerAddress.City = db.GetNullableString(reader, 9);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 10);
        entities.EmployerAddress.State = db.GetNullableString(reader, 11);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 13);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 14);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerRegisteredAgent()
  {
    entities.EmployerRegisteredAgent.Populated = false;

    return Read("ReadEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.EmployerRegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.EmployerRegisteredAgent.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.EmployerRegisteredAgent.EndDate =
          db.GetNullableDate(reader, 2);
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 3);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 4);
        entities.EmployerRegisteredAgent.Populated = true;
      });
  }

  private bool ReadEmployerRelation1()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation1",
      (db, command) =>
      {
        db.SetInt32(command, "empHqId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
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
        db.SetNullableDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
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
    /// <summary>
    /// A value of EmploymentTies.
    /// </summary>
    [JsonPropertyName("employmentTies")]
    public Common EmploymentTies
    {
      get => employmentTies ??= new();
      set => employmentTies = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public Common RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of Worksite.
    /// </summary>
    [JsonPropertyName("worksite")]
    public Common Worksite
    {
      get => worksite ??= new();
      set => worksite = value;
    }

    /// <summary>
    /// A value of Headquarters.
    /// </summary>
    [JsonPropertyName("headquarters")]
    public Common Headquarters
    {
      get => headquarters ??= new();
      set => headquarters = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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

    private Common employmentTies;
    private Common registeredAgent;
    private Common worksite;
    private Common headquarters;
    private EmployerAddress employerAddress;
    private Employer employer;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private DateWorkArea currentDate;
    private Common common;
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
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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

    private IncomeSource incomeSource;
    private EmployerRelation employerRelation;
    private EmployerRegisteredAgent employerRegisteredAgent;
    private EmployerAddress employerAddress;
    private Employer employer;
  }
#endregion
}
