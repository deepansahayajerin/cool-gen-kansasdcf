// Program: OE_DELETE_HEALTH_INS_POLICY, ID: 371180149, model: 746.
// Short name: SWE00589
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_DELETE_HEALTH_INS_POLICY.
/// </summary>
[Serializable]
public partial class OeDeleteHealthInsPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DELETE_HEALTH_INS_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDeleteHealthInsPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDeleteHealthInsPolicy.
  /// </summary>
  public OeDeleteHealthInsPolicy(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadHealthInsuranceCoverage())
    {
      if (ReadPersonalHealthInsurance())
      {
        ExitState = "OE0202_DISALLOW_DELETE";

        return;
      }

      DeleteHealthInsuranceCoverage();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";
    }
  }

  private void DeleteHealthInsuranceCoverage()
  {
    Update("DeleteHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "carrierCode", import.HealthInsuranceCompany.CarrierCode ?? ""
          );
        db.SetInt64(
          command, "identifier", import.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.VerifiedUserId =
          db.GetNullableString(reader, 2);
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

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson csePerson;
  }
#endregion
}
