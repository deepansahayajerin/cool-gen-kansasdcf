// Program: OE_HICP_UPDATE_PERSONAL_HINS, ID: 371846365, model: 746.
// Short name: SWE00972
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_HICP_UPDATE_PERSONAL_HINS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeHicpUpdatePersonalHins: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_UPDATE_PERSONAL_HINS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpUpdatePersonalHins(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpUpdatePersonalHins.
  /// </summary>
  public OeHicpUpdatePersonalHins(IContext context, Import import, Export export)
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
    // 10/12/99  David Lowry 	  PRH75900
    // 08/08/00  P Phinney       H00102466 - fix -811 caused by multiple rows
    //                                        
    // returned on holder Cse_Person.
    // 01/31/03  E Lyman         WR20311   - fix -811 caused by multiple rows
    //                                       
    // returned on covered Cse_Person.
    // ---------------------------------------------
    export.Covered.Number = import.Covered.Number;
    export.PersonalHealthInsurance.Assign(import.PersonalHealthInsurance);

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // *** October 12, 1999   David Lowry
    // The case role may be ar or ap.  PRH75900
    // 08/08/00  P Phinney       H00102466 - fix -811 caused by multiple return 
    // on Cse_Person.
    // * * *     Qualify Case_Role on End-Date - Active should have  Only one 
    // Case_Role with MAX end_date / per Pam
    local.MaxDate.EndDate = new DateTime(2099, 12, 31);

    if (!ReadCsePerson2())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadHealthInsuranceCoverage())
    {
      export.HealthInsuranceCoverage.Assign(entities.HealthInsuranceCoverage);
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";

      return;
    }

    // *** The coverage date must be between the policy dates.  PRH75900.
    if (Lt(import.PersonalHealthInsurance.CoverageBeginDate,
      entities.HealthInsuranceCoverage.PolicyEffectiveDate))
    {
      export.Common.Count = 1;
    }

    if (Lt(entities.HealthInsuranceCoverage.PolicyExpirationDate,
      import.PersonalHealthInsurance.CoverageEndDate))
    {
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

    if (!ReadCsePerson1())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Read the personal health insurance coverage
    // for a CSE Person and Update the record .
    // ---------------------------------------------
    if (!ReadPersonalHealthInsurance())
    {
      ExitState = "PERSONAL_HEALTH_INSURANCE_NF";

      return;
    }

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
      UpdatePersonalHealthInsurance();
      ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      export.PersonalHealthInsurance.Assign(entities.PersonalHealthInsurance);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PERSONAL_HEALTH_INSURANCE_NU";

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

  private bool ReadCsePerson1()
  {
    entities.Covered.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Covered.Number);
      },
      (db, reader) =>
      {
        entities.Covered.Number = db.GetString(reader, 0);
        entities.Covered.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Holder.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Holder.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Holder.Number = db.GetString(reader, 0);
        entities.Holder.Populated = true;
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
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Covered.Number);
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.VerifiedUserId =
          db.GetNullableString(reader, 2);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.AlertFlagInsuranceExistsInd =
          db.GetNullableString(reader, 4);
        entities.PersonalHealthInsurance.CoverageCostAmount =
          db.GetNullableDecimal(reader, 5);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 6);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 7);
        entities.PersonalHealthInsurance.CreatedBy = db.GetString(reader, 8);
        entities.PersonalHealthInsurance.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.PersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.PersonalHealthInsurance.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.PersonalHealthInsurance.PremiumVerifiedDate =
          db.GetNullableDate(reader, 12);
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private void UpdatePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);

    var verifiedUserId = local.PersonalHealthInsurance.VerifiedUserId ?? "";
    var coverageVerifiedDate =
      import.PersonalHealthInsurance.CoverageVerifiedDate;
    var alertFlagInsuranceExistsInd =
      import.PersonalHealthInsurance.AlertFlagInsuranceExistsInd ?? "";
    var coverageCostAmount =
      import.PersonalHealthInsurance.CoverageCostAmount.GetValueOrDefault();
    var coverageBeginDate = import.PersonalHealthInsurance.CoverageBeginDate;
    var coverageEndDate = import.PersonalHealthInsurance.CoverageEndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var premiumVerifiedDate =
      import.PersonalHealthInsurance.PremiumVerifiedDate;

    entities.PersonalHealthInsurance.Populated = false;
    Update("UpdatePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableDate(command, "covVerifiedDate", coverageVerifiedDate);
        db.SetNullableString(
          command, "insExistsInd", alertFlagInsuranceExistsInd);
        db.SetNullableDecimal(command, "coverCostAmt", coverageCostAmount);
        db.SetNullableDate(command, "coverBeginDate", coverageBeginDate);
        db.SetNullableDate(command, "coverEndDate", coverageEndDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableDate(command, "premVerifiedDate", premiumVerifiedDate);
        db.SetInt64(command, "hcvId", entities.PersonalHealthInsurance.HcvId);
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
      });

    entities.PersonalHealthInsurance.VerifiedUserId = verifiedUserId;
    entities.PersonalHealthInsurance.CoverageVerifiedDate =
      coverageVerifiedDate;
    entities.PersonalHealthInsurance.AlertFlagInsuranceExistsInd =
      alertFlagInsuranceExistsInd;
    entities.PersonalHealthInsurance.CoverageCostAmount = coverageCostAmount;
    entities.PersonalHealthInsurance.CoverageBeginDate = coverageBeginDate;
    entities.PersonalHealthInsurance.CoverageEndDate = coverageEndDate;
    entities.PersonalHealthInsurance.LastUpdatedBy = lastUpdatedBy;
    entities.PersonalHealthInsurance.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.PersonalHealthInsurance.PremiumVerifiedDate = premiumVerifiedDate;
    entities.PersonalHealthInsurance.Populated = true;
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
    public CaseRole MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private PersonalHealthInsurance personalHealthInsurance;
    private CaseRole maxDate;
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
    /// A value of Holder.
    /// </summary>
    [JsonPropertyName("holder")]
    public CsePerson Holder
    {
      get => holder ??= new();
      set => holder = value;
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
    private CsePerson holder;
    private CaseRole coveredPerson;
    private Case1 case1;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson covered;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
