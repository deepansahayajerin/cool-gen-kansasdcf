// Program: OE_CAB_CHECK_INSURANCE_COVERAGE, ID: 371846340, model: 746.
// Short name: SWE00882
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_CHECK_INSURANCE_COVERAGE.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeCabCheckInsuranceCoverage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_CHECK_INSURANCE_COVERAGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabCheckInsuranceCoverage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabCheckInsuranceCoverage.
  /// </summary>
  public OeCabCheckInsuranceCoverage(IContext context, Import import,
    Export export):
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
    // This CAb checks is Insurance Coverage exists
    // for a CSE Person and if more than one
    // coverage exists for that Person.
    // ---------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    export.HealthInsuranceCoverage.Identifier =
      import.HealthInsuranceCoverage.Identifier;
    export.Contact.Assign(import.Contact);
    export.Case1.Number = import.Case1.Number;

    // ---------------------------------------------
    // Verify the CSE Person number entered.
    // ---------------------------------------------
    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadCase())
    {
      export.Case1.Number = entities.Case1.Number;
    }

    if (import.Contact.ContactNumber != 0)
    {
      if (ReadContact())
      {
        export.Contact.Assign(import.Contact);
      }
    }

    // ---------------------------------------------
    // Read the health insurance coverage record
    // related to the CSE Person and case number
    // entered.  If there were more than one health
    // insurance coverage record and a specific
    // record has not been selected for display,
    // send an error message to the user to go to a
    // list screen to select the record desired.
    // ---------------------------------------------
    local.WorkInsCoveCount.Count = 0;

    if (import.HealthInsuranceCoverage.Identifier == 0)
    {
      if (import.Contact.ContactNumber == 0)
      {
        foreach(var item in ReadHealthInsuranceCoverage4())
        {
          ++local.WorkInsCoveCount.Count;
          local.HealthInsuranceCoverage.Identifier =
            entities.HealthInsuranceCoverage.Identifier;

          if (local.WorkInsCoveCount.Count > 1)
          {
            ExitState = "MORE_THAN_ONE_INSURANCE_COVERAGE";

            return;
          }
        }
      }
      else
      {
        foreach(var item in ReadHealthInsuranceCoverage3())
        {
          local.HealthInsuranceCoverage.Identifier =
            entities.HealthInsuranceCoverage.Identifier;
          ++local.WorkInsCoveCount.Count;

          if (local.WorkInsCoveCount.Count > 1)
          {
            ExitState = "MORE_THAN_ONE_INSURANCE_COVERAGE";

            return;
          }
        }
      }
    }
    else if (ReadHealthInsuranceCoverage1())
    {
      local.HealthInsuranceCoverage.Identifier =
        entities.HealthInsuranceCoverage.Identifier;
      local.WorkInsCoveCount.Count = 1;
    }

    if (local.WorkInsCoveCount.Count == 0)
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
    }
    else if (ReadHealthInsuranceCoverage2())
    {
      MoveHealthInsuranceCoverage(entities.HealthInsuranceCoverage,
        export.HealthInsuranceCoverage);
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
    }
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.VerifiedDate = source.VerifiedDate;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
    target.PolicyEffectiveDate = source.PolicyEffectiveDate;
    target.OtherCoveredPersons = source.OtherCoveredPersons;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.Fax = db.GetNullableInt32(reader, 2);
        entities.Contact.NameTitle = db.GetNullableString(reader, 3);
        entities.Contact.CompanyName = db.GetNullableString(reader, 4);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.Contact.NameLast = db.GetNullableString(reader, 6);
        entities.Contact.NameFirst = db.GetNullableString(reader, 7);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 8);
        entities.Contact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.Contact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.Contact.CreatedBy = db.GetString(reader, 11);
        entities.Contact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.Contact.LastUpdatedBy = db.GetString(reader, 13);
        entities.Contact.LastUpdatedTimestamp = db.GetDateTime(reader, 14);
        entities.Contact.Populated = true;
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

  private bool ReadHealthInsuranceCoverage1()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", import.HealthInsuranceCoverage.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.VerifiedUserId =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CreatedBy = db.GetString(reader, 15);
        entities.HealthInsuranceCoverage.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 22);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage2()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", local.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.VerifiedUserId =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CreatedBy = db.GetString(reader, 15);
        entities.HealthInsuranceCoverage.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 22);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverage3()
  {
    System.Diagnostics.Debug.Assert(entities.Contact.Populated);
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage3",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableInt32(
          command, "conHNumber", entities.Contact.ContactNumber);
        db.SetNullableString(command, "cspHNumber", entities.Contact.CspNumber);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.VerifiedUserId =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CreatedBy = db.GetString(reader, 15);
        entities.HealthInsuranceCoverage.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 22);
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverage4()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage4",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.VerifiedUserId =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CreatedBy = db.GetString(reader, 15);
        entities.HealthInsuranceCoverage.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 22);
        entities.HealthInsuranceCoverage.Populated = true;

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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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

    private Contact contact;
    private Case1 case1;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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

    private Contact contact;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
    private Case1 case1;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of WorkInsCoveCount.
    /// </summary>
    [JsonPropertyName("workInsCoveCount")]
    public Common WorkInsCoveCount
    {
      get => workInsCoveCount ??= new();
      set => workInsCoveCount = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Common workInsCoveCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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

    private Contact contact;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private Case1 case1;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson csePerson;
  }
#endregion
}
