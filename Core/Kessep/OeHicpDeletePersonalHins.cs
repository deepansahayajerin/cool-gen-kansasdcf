// Program: OE_HICP_DELETE_PERSONAL_HINS, ID: 371846364, model: 746.
// Short name: SWE00904
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_HICP_DELETE_PERSONAL_HINS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeHicpDeletePersonalHins: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_DELETE_PERSONAL_HINS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpDeletePersonalHins(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpDeletePersonalHins.
  /// </summary>
  public OeHicpDeletePersonalHins(IContext context, Import import, Export export)
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
    // 02/15/95  Sid             Initial Creation.
    // ---------------------------------------------
    if (ReadHealthInsuranceCoverage())
    {
      if (ReadCsePerson())
      {
        if (ReadPersonalHealthInsurance())
        {
          DeletePersonalHealthInsurance();
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
          ExitState = "PERSONAL_HEALTH_INSURANCE_NF";
        }
      }
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
    }
  }

  private void DeletePersonalHealthInsurance()
  {
    Update("DeletePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(command, "hcvId", entities.PersonalHealthInsurance.HcvId);
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
      });
  }

  private bool ReadCsePerson()
  {
    entities.Insured.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Insured.Number);
      },
      (db, reader) =>
      {
        entities.Insured.Number = db.GetString(reader, 0);
        entities.Insured.Populated = true;
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
        db.SetNullableString(command, "cspNumber", import.Holder.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Insured.Number);
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
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
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
    /// A value of Holder.
    /// </summary>
    [JsonPropertyName("holder")]
    public CsePerson Holder
    {
      get => holder ??= new();
      set => holder = value;
    }

    private CsePerson insured;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson holder;
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
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
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

    private CsePerson insured;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson holder;
    private PersonalHealthInsurance personalHealthInsurance;
  }
#endregion
}
