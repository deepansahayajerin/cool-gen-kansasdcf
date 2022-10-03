// Program: OE_HAIV_AVAILABLE_INS_UPDATE, ID: 371857738, model: 746.
// Short name: SWE02298
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HAIV_AVAILABLE_INS_UPDATE.
/// </summary>
[Serializable]
public partial class OeHaivAvailableInsUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HAIV_AVAILABLE_INS_UPDATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHaivAvailableInsUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHaivAvailableInsUpdate.
  /// </summary>
  public OeHaivAvailableInsUpdate(IContext context, Import import, Export export)
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // C. Chhun	01/05/99	Initial Code
    // ******** END MAINTENANCE LOG ****************
    export.HealthInsuranceAvailability.
      Assign(import.HealthInsuranceAvailability);

    if (!ReadHealthInsuranceAvailability())
    {
      ExitState = "OE0189_AVAILABLE_HEALTH_INS_NF";

      return;
    }

    try
    {
      UpdateHealthInsuranceAvailability();
      export.HealthInsuranceAvailability.Assign(
        import.HealthInsuranceAvailability);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OE0189_AVAILABLE_HEALTH_INS_NF";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OE0191_AVAILABLE_HEALTH_INS_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadHealthInsuranceAvailability()
  {
    entities.HealthInsuranceAvailability.Populated = false;

    return Read("ReadHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetInt32(
          command, "insuranceId",
          import.HealthInsuranceAvailability.InsuranceId);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceAvailability.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceAvailability.InsuranceName =
          db.GetString(reader, 3);
        entities.HealthInsuranceAvailability.Street1 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceAvailability.Street2 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceAvailability.City =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceAvailability.State =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceAvailability.Zip5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceAvailability.Zip4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceAvailability.Cost =
          db.GetNullableDecimal(reader, 10);
        entities.HealthInsuranceAvailability.CostFrequency =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceAvailability.VerifiedDate =
          db.GetNullableDate(reader, 12);
        entities.HealthInsuranceAvailability.EndDate =
          db.GetNullableDate(reader, 13);
        entities.HealthInsuranceAvailability.EmployerName =
          db.GetString(reader, 14);
        entities.HealthInsuranceAvailability.EmpStreet1 =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceAvailability.EmpStreet2 =
          db.GetNullableString(reader, 16);
        entities.HealthInsuranceAvailability.EmpCity =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceAvailability.EmpState =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceAvailability.EmpZip5 =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceAvailability.EmpZip4 =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceAvailability.EmpPhoneAreaCode =
          db.GetInt32(reader, 21);
        entities.HealthInsuranceAvailability.EmpPhoneNo =
          db.GetNullableInt32(reader, 22);
        entities.HealthInsuranceAvailability.LastUpdatedBy =
          db.GetString(reader, 23);
        entities.HealthInsuranceAvailability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 25);
        entities.HealthInsuranceAvailability.Populated = true;
      });
  }

  private void UpdateHealthInsuranceAvailability()
  {
    System.Diagnostics.Debug.Assert(
      entities.HealthInsuranceAvailability.Populated);

    var insurancePolicyNumber =
      import.HealthInsuranceAvailability.InsurancePolicyNumber ?? "";
    var insuranceGroupNumber =
      import.HealthInsuranceAvailability.InsuranceGroupNumber ?? "";
    var insuranceName = import.HealthInsuranceAvailability.InsuranceName;
    var street1 = import.HealthInsuranceAvailability.Street1 ?? "";
    var street2 = import.HealthInsuranceAvailability.Street2 ?? "";
    var city = import.HealthInsuranceAvailability.City ?? "";
    var state = import.HealthInsuranceAvailability.State ?? "";
    var zip5 = import.HealthInsuranceAvailability.Zip5 ?? "";
    var zip4 = import.HealthInsuranceAvailability.Zip4 ?? "";
    var cost = import.HealthInsuranceAvailability.Cost.GetValueOrDefault();
    var costFrequency = import.HealthInsuranceAvailability.CostFrequency ?? "";
    var verifiedDate = import.HealthInsuranceAvailability.VerifiedDate;
    var endDate = import.HealthInsuranceAvailability.EndDate;
    var employerName = import.HealthInsuranceAvailability.EmployerName;
    var empStreet1 = import.HealthInsuranceAvailability.EmpStreet1 ?? "";
    var empStreet2 = import.HealthInsuranceAvailability.EmpStreet2 ?? "";
    var empCity = import.HealthInsuranceAvailability.EmpCity ?? "";
    var empState = import.HealthInsuranceAvailability.EmpState ?? "";
    var empZip5 = import.HealthInsuranceAvailability.EmpZip5 ?? "";
    var empZip4 = import.HealthInsuranceAvailability.EmpZip4 ?? "";
    var empPhoneAreaCode = import.HealthInsuranceAvailability.EmpPhoneAreaCode;
    var empPhoneNo =
      import.HealthInsuranceAvailability.EmpPhoneNo.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.HealthInsuranceAvailability.Populated = false;
    Update("UpdateHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetNullableString(command, "policyNumber", insurancePolicyNumber);
        db.SetNullableString(command, "groupNumber", insuranceGroupNumber);
        db.SetString(command, "insuranceName", insuranceName);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zip5", zip5);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableDecimal(command, "cost", cost);
        db.SetNullableString(command, "costFrequency", costFrequency);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "employerName", employerName);
        db.SetNullableString(command, "empStreet1", empStreet1);
        db.SetNullableString(command, "empStreet2", empStreet2);
        db.SetNullableString(command, "empCity", empCity);
        db.SetNullableString(command, "empState", empState);
        db.SetNullableString(command, "empZip5", empZip5);
        db.SetNullableString(command, "empZip4", empZip4);
        db.SetInt32(command, "empAreaCode", empPhoneAreaCode);
        db.SetNullableInt32(command, "empPhoneNo", empPhoneNo);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdTstamp", lastUpdatedTimestamp);
        db.SetInt32(
          command, "insuranceId",
          entities.HealthInsuranceAvailability.InsuranceId);
        db.SetString(
          command, "cspNumber", entities.HealthInsuranceAvailability.CspNumber);
          
      });

    entities.HealthInsuranceAvailability.InsurancePolicyNumber =
      insurancePolicyNumber;
    entities.HealthInsuranceAvailability.InsuranceGroupNumber =
      insuranceGroupNumber;
    entities.HealthInsuranceAvailability.InsuranceName = insuranceName;
    entities.HealthInsuranceAvailability.Street1 = street1;
    entities.HealthInsuranceAvailability.Street2 = street2;
    entities.HealthInsuranceAvailability.City = city;
    entities.HealthInsuranceAvailability.State = state;
    entities.HealthInsuranceAvailability.Zip5 = zip5;
    entities.HealthInsuranceAvailability.Zip4 = zip4;
    entities.HealthInsuranceAvailability.Cost = cost;
    entities.HealthInsuranceAvailability.CostFrequency = costFrequency;
    entities.HealthInsuranceAvailability.VerifiedDate = verifiedDate;
    entities.HealthInsuranceAvailability.EndDate = endDate;
    entities.HealthInsuranceAvailability.EmployerName = employerName;
    entities.HealthInsuranceAvailability.EmpStreet1 = empStreet1;
    entities.HealthInsuranceAvailability.EmpStreet2 = empStreet2;
    entities.HealthInsuranceAvailability.EmpCity = empCity;
    entities.HealthInsuranceAvailability.EmpState = empState;
    entities.HealthInsuranceAvailability.EmpZip5 = empZip5;
    entities.HealthInsuranceAvailability.EmpZip4 = empZip4;
    entities.HealthInsuranceAvailability.EmpPhoneAreaCode = empPhoneAreaCode;
    entities.HealthInsuranceAvailability.EmpPhoneNo = empPhoneNo;
    entities.HealthInsuranceAvailability.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceAvailability.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.HealthInsuranceAvailability.Populated = true;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
    }

    private HealthInsuranceAvailability healthInsuranceAvailability;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private CsePerson csePerson;
  }
#endregion
}
