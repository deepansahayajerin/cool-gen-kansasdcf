// Program: OE_CREATE_INSURANCE_COVER_HOLDER, ID: 371853938, model: 746.
// Short name: SWE00889
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_INSURANCE_COVER_HOLDER.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateInsuranceCoverHolder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_INSURANCE_COVER_HOLDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateInsuranceCoverHolder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateInsuranceCoverHolder.
  /// </summary>
  public OeCreateInsuranceCoverHolder(IContext context, Import import,
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
    // ***********************************************************************
    // ******               M A I N T E N A N C E   L O G                 ****
    // ***********************************************************************
    // Date		Author	WR/PR		Description
    // ***********************************************************************
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/10/95  Sid             Rework  and
    //                                   
    // Completion.
    // 09/06/2003	E.Shirk	WR20311	Added logic to end date HI coverage when HI 
    // company was ended.
    // ***********************************************************************
    export.HealthInsuranceCoverage.Assign(import.HealthInsuranceCoverage);
    export.Contact.Assign(import.Contact);
    export.CsePerson.Number = import.CsePerson.Number;
    export.HealthInsuranceCompany.Assign(import.HealthInsuranceCompany);

    // ---------------------------------------------
    // Read the CSE Person .
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

    // ---------------------------------------------
    // Read the health insurance company .
    // ---------------------------------------------
    if (ReadHealthInsuranceCompany())
    {
      export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COMPANY_NF_RB";

      return;
    }

    // ---------------------------------------------
    //       Read the CSE Person Contact .
    // ---------------------------------------------
    if (import.Contact.ContactNumber != 0)
    {
      if (ReadContact())
      {
        export.HealthInsuranceCoverage.PolicyPaidByCsePersonInd = "N";
        export.Contact.Assign(entities.Contact);
      }
    }
    else
    {
      export.HealthInsuranceCoverage.PolicyPaidByCsePersonInd = "Y";
    }

    // ****************************************************************
    // Check to ensure that is not a duplicate record for combination of 
    // CSE_PERSON,CARRIER_CODE,POLICY #, AND GROUP ID.  These fields are not
    // IDENTIFIERS in the database.
    // ****************************************************************
    if (ReadHealthInsuranceCoverage1())
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_AE_RB";

      return;
    }

    // ---------------------------------------------
    // Create the health insurance coverage record
    // which identifies the holder of the insurance
    // policy.
    // ---------------------------------------------
    ReadHealthInsuranceCoverage2();

    if (Equal(export.HealthInsuranceCoverage.VerifiedDate, null))
    {
      export.HealthInsuranceCoverage.VerifiedUserId = "";
    }
    else
    {
      export.HealthInsuranceCoverage.VerifiedUserId = global.UserId;
    }

    // ***********************************************************************
    // **       Set expiration date on HI coverage if HI company is ended.
    // ***********************************************************************
    if (Lt(entities.HealthInsuranceCompany.EndDate, import.Max.Date))
    {
      export.HealthInsuranceCoverage.PolicyExpirationDate =
        entities.HealthInsuranceCompany.EndDate;
    }

    try
    {
      CreateHealthInsuranceCoverage();
      export.HealthInsuranceCoverage.Assign(entities.HealthInsuranceCoverage);
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

      // ---------------------------------------------
      // Associate the health insurance coverage with
      // the Contact holding the policy.
      // ---------------------------------------------
      if (entities.Contact.Populated)
      {
        AssociateHealthInsuranceCoverage1();
      }

      if (!Equal(import.IncomeSource.Identifier, local.Blank.Identifier))
      {
        if (ReadIncomeSource())
        {
          AssociateHealthInsuranceCoverage2();
        }
        else
        {
          ExitState = "INCOME_SOURCE_NF";
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "HEALTH_INSURANCE_COVERAGE_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "HEALTH_INSURANCE_COVERAGE_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void AssociateHealthInsuranceCoverage1()
  {
    System.Diagnostics.Debug.Assert(entities.Contact.Populated);

    var cspHNumber = entities.Contact.CspNumber;
    var conHNumber = entities.Contact.ContactNumber;

    entities.HealthInsuranceCoverage.Populated = false;
    Update("AssociateHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspHNumber", cspHNumber);
        db.SetNullableInt32(command, "conHNumber", conHNumber);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CspHNumber = cspHNumber;
    entities.HealthInsuranceCoverage.ConHNumber = conHNumber;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void AssociateHealthInsuranceCoverage2()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var cseNumber = entities.IncomeSource.CspINumber;
    var isrIdentifier = entities.IncomeSource.Identifier;

    entities.HealthInsuranceCoverage.Populated = false;
    Update("AssociateHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(command, "cseNumber", cseNumber);
        db.SetNullableDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CseNumber = cseNumber;
    entities.HealthInsuranceCoverage.IsrIdentifier = isrIdentifier;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void CreateHealthInsuranceCoverage()
  {
    var identifier = local.Work.Count;
    var policyPaidByCsePersonInd =
      export.HealthInsuranceCoverage.PolicyPaidByCsePersonInd;
    var insuranceGroupNumber =
      export.HealthInsuranceCoverage.InsuranceGroupNumber ?? "";
    var verifiedDate = export.HealthInsuranceCoverage.VerifiedDate;
    var verifiedUserId = export.HealthInsuranceCoverage.VerifiedUserId ?? "";
    var insurancePolicyNumber =
      export.HealthInsuranceCoverage.InsurancePolicyNumber ?? "";
    var policyExpirationDate =
      export.HealthInsuranceCoverage.PolicyExpirationDate;
    var coverageCode1 = export.HealthInsuranceCoverage.CoverageCode1 ?? "";
    var coverageCode2 = export.HealthInsuranceCoverage.CoverageCode2 ?? "";
    var coverageCode3 = export.HealthInsuranceCoverage.CoverageCode3 ?? "";
    var coverageCode4 = export.HealthInsuranceCoverage.CoverageCode4 ?? "";
    var coverageCode5 = export.HealthInsuranceCoverage.CoverageCode5 ?? "";
    var coverageCode6 = export.HealthInsuranceCoverage.CoverageCode6 ?? "";
    var coverageCode7 = export.HealthInsuranceCoverage.CoverageCode7 ?? "";
    var policyEffectiveDate =
      export.HealthInsuranceCoverage.PolicyEffectiveDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.CsePerson.Number;
    var hicIdentifier = entities.HealthInsuranceCompany.Identifier;
    var otherCoveredPersons =
      export.HealthInsuranceCoverage.OtherCoveredPersons ?? "";

    entities.HealthInsuranceCoverage.Populated = false;
    Update("CreateHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "paidByCsepInd", policyPaidByCsePersonInd);
        db.SetNullableString(command, "groupNumber", insuranceGroupNumber);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableString(command, "policyNumber", insurancePolicyNumber);
        db.SetNullableDate(command, "policyExpDate", policyExpirationDate);
        db.SetNullableString(command, "coverageCode1", coverageCode1);
        db.SetNullableString(command, "coverageCode2", coverageCode2);
        db.SetNullableString(command, "coverageCode3", coverageCode3);
        db.SetNullableString(command, "coverageCode4", coverageCode4);
        db.SetNullableString(command, "coverageCode5", coverageCode5);
        db.SetNullableString(command, "coverageCode6", coverageCode6);
        db.SetNullableString(command, "coverageCode7", coverageCode7);
        db.SetNullableDate(command, "policyEffDate", policyEffectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableInt32(command, "hicIdentifier", hicIdentifier);
        db.SetNullableString(command, "othCovPersons", otherCoveredPersons);
      });

    entities.HealthInsuranceCoverage.Identifier = identifier;
    entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
      policyPaidByCsePersonInd;
    entities.HealthInsuranceCoverage.InsuranceGroupNumber =
      insuranceGroupNumber;
    entities.HealthInsuranceCoverage.VerifiedDate = verifiedDate;
    entities.HealthInsuranceCoverage.VerifiedUserId = verifiedUserId;
    entities.HealthInsuranceCoverage.InsurancePolicyNumber =
      insurancePolicyNumber;
    entities.HealthInsuranceCoverage.PolicyExpirationDate =
      policyExpirationDate;
    entities.HealthInsuranceCoverage.CoverageCode1 = coverageCode1;
    entities.HealthInsuranceCoverage.CoverageCode2 = coverageCode2;
    entities.HealthInsuranceCoverage.CoverageCode3 = coverageCode3;
    entities.HealthInsuranceCoverage.CoverageCode4 = coverageCode4;
    entities.HealthInsuranceCoverage.CoverageCode5 = coverageCode5;
    entities.HealthInsuranceCoverage.CoverageCode6 = coverageCode6;
    entities.HealthInsuranceCoverage.CoverageCode7 = coverageCode7;
    entities.HealthInsuranceCoverage.PolicyEffectiveDate = policyEffectiveDate;
    entities.HealthInsuranceCoverage.CreatedBy = createdBy;
    entities.HealthInsuranceCoverage.CreatedTimestamp = createdTimestamp;
    entities.HealthInsuranceCoverage.LastUpdatedBy = createdBy;
    entities.HealthInsuranceCoverage.LastUpdatedTimestamp = createdTimestamp;
    entities.HealthInsuranceCoverage.CspHNumber = null;
    entities.HealthInsuranceCoverage.ConHNumber = null;
    entities.HealthInsuranceCoverage.CspNumber = cspNumber;
    entities.HealthInsuranceCoverage.HicIdentifier = hicIdentifier;
    entities.HealthInsuranceCoverage.OtherCoveredPersons = otherCoveredPersons;
    entities.HealthInsuranceCoverage.CseNumber = null;
    entities.HealthInsuranceCoverage.IsrIdentifier = null;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 5);
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

  private bool ReadHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.HealthInsuranceCompany.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 3);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.CreatedBy = db.GetString(reader, 5);
        entities.HealthInsuranceCompany.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 7);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 13);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage1()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "groupNumber",
          import.HealthInsuranceCoverage.InsuranceGroupNumber ?? "");
        db.SetNullableString(
          command, "policyNumber",
          import.HealthInsuranceCoverage.InsurancePolicyNumber ?? "");
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableInt32(
          command, "hicIdentifier", entities.HealthInsuranceCompany.Identifier);
          
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
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 23);
        entities.HealthInsuranceCoverage.CseNumber =
          db.GetNullableString(reader, 24);
        entities.HealthInsuranceCoverage.IsrIdentifier =
          db.GetNullableDateTime(reader, 25);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage2()
  {
    return Read("ReadHealthInsuranceCoverage2",
      null,
      (db, reader) =>
      {
        local.Work.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    private DateWorkArea max;
    private IncomeSource incomeSource;
    private Contact contact;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Contact contact;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public IncomeSource Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    private IncomeSource blank;
    private Common work;
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private IncomeSource incomeSource;
    private Contact contact;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
