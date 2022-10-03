// Program: OE_HICP_CREATE_PERSONAL_HINS, ID: 371846369, model: 746.
// Short name: SWE00892
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_HICP_CREATE_PERSONAL_HINS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeHicpCreatePersonalHins: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_CREATE_PERSONAL_HINS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpCreatePersonalHins(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpCreatePersonalHins.
  /// </summary>
  public OeHicpCreatePersonalHins(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/16/95  Sid             Completion.
    // 08/12/99  D.Lowry         PR719.  Extended the read for covered person to
    // 		 	  include case role
    // 10/08/1999 D.Lowry	  PRH75900.  The case role for the insurance holder
    // can be the AR or the AP.  The dates must fall			 	  between the
    // begin and end dates of the policy.
    // 02/24/2000   Vithal Madhira   PR# 87086     Added code to fix  the abend.
    // ---------------------------------------------
    export.Covered.Number = import.Covered.Number;
    export.PersonalHealthInsurance.Assign(import.PersonalHealthInsurance);

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // *** October 07,1999  David Lowry
    // The case role may be AR or AP.  PRH75900.
    // -------------------------------------------------------------------
    // PR# 87086 Included the 'End Date' qualifier in the Read. Otherwise 
    // sometimes it is returning more than one row and causing the abend.
    //                                                  
    // ---  Vithal Madhira(02/24/2000)
    // --------------------------------------------------------------------
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadHealthInsuranceCoverage())
    {
      MoveHealthInsuranceCoverage(entities.HealthInsuranceCoverage,
        export.HealthInsuranceCoverage);
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";

      return;
    }

    // *** October 07,1999  David Lowry
    // The dates must fall between the policy dates.  PRH75900.
    if (Lt(import.PersonalHealthInsurance.CoverageBeginDate,
      entities.HealthInsuranceCoverage.PolicyEffectiveDate))
    {
      export.Common.Count = 1;
    }

    if (Lt(entities.HealthInsuranceCoverage.PolicyExpirationDate,
      import.PersonalHealthInsurance.CoverageEndDate))
    {
      // *** The coverage date must be between the policy dates.  PRH75900.
      switch(export.Common.Count)
      {
        case 0:
          export.Common.Count = 2;

          break;
        case 1:
          export.Common.Count = 3;

          break;
        default:
          break;
      }
    }

    if (export.Common.Count == 0)
    {
    }
    else
    {
      return;
    }

    // *** Aug 12, 1999  David Lowry
    // To resolve PR719  the read was expanded to include case role so the type 
    // could be exported to the screen.
    // *** November 1, 1999 David Lowry
    // PR78706,  There may be more than one row for CH.  A child may be end 
    // dated and then reinstated.  Get the row with an end date GT or EQ the
    // coverage end date.
    if (ReadCsePersonCaseRole())
    {
      export.CaseRole.Type1 = entities.CoveredPerson.Type1;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Create the personal health insurance coverage
    // for a CSE Person and associate that record to
    // the covered CSE Person, Health Insurance
    // Coverage, and Case records.
    // ---------------------------------------------
    if (Equal(import.PersonalHealthInsurance.CoverageVerifiedDate, null))
    {
      local.PersonalHealthInsurance.VerifiedUserId = "";
    }
    else
    {
      local.PersonalHealthInsurance.VerifiedUserId = global.UserId;
    }

    try
    {
      CreatePersonalHealthInsurance();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      export.PersonalHealthInsurance.Assign(entities.PersonalHealthInsurance);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PERSONAL_HEALTH_INSURANCE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PERSONAL_HEALTH_INSURANCE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
  }

  private void CreatePersonalHealthInsurance()
  {
    var hcvId = entities.HealthInsuranceCoverage.Identifier;
    var cspNumber = entities.Covered.Number;
    var verifiedUserId = local.PersonalHealthInsurance.VerifiedUserId ?? "";
    var coverageVerifiedDate =
      import.PersonalHealthInsurance.CoverageVerifiedDate;
    var alertFlagInsuranceExistsInd =
      import.PersonalHealthInsurance.AlertFlagInsuranceExistsInd ?? "";
    var coverageCostAmount =
      import.PersonalHealthInsurance.CoverageCostAmount.GetValueOrDefault();
    var coverageBeginDate = import.PersonalHealthInsurance.CoverageBeginDate;
    var coverageEndDate = import.PersonalHealthInsurance.CoverageEndDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var premiumVerifiedDate =
      import.PersonalHealthInsurance.PremiumVerifiedDate;

    entities.PersonalHealthInsurance.Populated = false;
    Update("CreatePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(command, "hcvId", hcvId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableDate(command, "covVerifiedDate", coverageVerifiedDate);
        db.SetNullableString(
          command, "insExistsInd", alertFlagInsuranceExistsInd);
        db.SetNullableDecimal(command, "coverCostAmt", coverageCostAmount);
        db.SetNullableDate(command, "coverBeginDate", coverageBeginDate);
        db.SetNullableDate(command, "coverEndDate", coverageEndDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDate(command, "premVerifiedDate", premiumVerifiedDate);
      });

    entities.PersonalHealthInsurance.HcvId = hcvId;
    entities.PersonalHealthInsurance.CspNumber = cspNumber;
    entities.PersonalHealthInsurance.VerifiedUserId = verifiedUserId;
    entities.PersonalHealthInsurance.CoverageVerifiedDate =
      coverageVerifiedDate;
    entities.PersonalHealthInsurance.AlertFlagInsuranceExistsInd =
      alertFlagInsuranceExistsInd;
    entities.PersonalHealthInsurance.CoverageCostAmount = coverageCostAmount;
    entities.PersonalHealthInsurance.CoverageBeginDate = coverageBeginDate;
    entities.PersonalHealthInsurance.CoverageEndDate = coverageEndDate;
    entities.PersonalHealthInsurance.CreatedBy = createdBy;
    entities.PersonalHealthInsurance.CreatedTimestamp = createdTimestamp;
    entities.PersonalHealthInsurance.LastUpdatedBy = createdBy;
    entities.PersonalHealthInsurance.LastUpdatedTimestamp = createdTimestamp;
    entities.PersonalHealthInsurance.PremiumVerifiedDate = premiumVerifiedDate;
    entities.PersonalHealthInsurance.Populated = true;
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

  private bool ReadCsePerson()
  {
    entities.Holder.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Holder.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Holder.Number = db.GetString(reader, 0);
        entities.Holder.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.CoveredPerson.Populated = false;
    entities.Covered.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Covered.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate",
          import.PersonalHealthInsurance.CoverageEndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Covered.Number = db.GetString(reader, 0);
        entities.CoveredPerson.CspNumber = db.GetString(reader, 0);
        entities.CoveredPerson.CasNumber = db.GetString(reader, 1);
        entities.CoveredPerson.Type1 = db.GetString(reader, 2);
        entities.CoveredPerson.Identifier = db.GetInt32(reader, 3);
        entities.CoveredPerson.StartDate = db.GetNullableDate(reader, 4);
        entities.CoveredPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.CoveredPerson.Populated = true;
        entities.Covered.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CoveredPerson.Type1);
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", import.HealthInsuranceCoverage.Identifier);
        db.SetNullableString(command, "cspNumber", entities.Holder.Number);
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
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceCoverage.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Holder.
    /// </summary>
    [JsonPropertyName("holder")]
    public CsePerson Holder
    {
      get => holder ??= new();
      set => holder = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    public CsePerson Covered
    {
      get => covered ??= new();
      set => covered = value;
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

    private Case1 case1;
    private CsePerson holder;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson covered;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    public CsePerson Covered
    {
      get => covered ??= new();
      set => covered = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private CaseRole caseRole;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson covered;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Common common;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private PersonalHealthInsurance personalHealthInsurance;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    public CaseRole PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    /// <summary>
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CaseRole CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    /// <summary>
    /// A value of Holder.
    /// </summary>
    [JsonPropertyName("holder")]
    public CsePerson Holder
    {
      get => holder ??= new();
      set => holder = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of Covered.
    /// </summary>
    [JsonPropertyName("covered")]
    public CsePerson Covered
    {
      get => covered ??= new();
      set => covered = value;
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

    private CaseRole policyHolder;
    private CaseRole coveredPerson;
    private CsePerson holder;
    private Case1 case1;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson covered;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
