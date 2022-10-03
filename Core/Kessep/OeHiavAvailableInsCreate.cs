// Program: OE_HIAV_AVAILABLE_INS_CREATE, ID: 371857737, model: 746.
// Short name: SWE02299
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HIAV_AVAILABLE_INS_CREATE.
/// </summary>
[Serializable]
public partial class OeHiavAvailableInsCreate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIAV_AVAILABLE_INS_CREATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiavAvailableInsCreate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiavAvailableInsCreate.
  /// </summary>
  public OeHiavAvailableInsCreate(IContext context, Import import, Export export)
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
    MoveHealthInsuranceAvailability(import.HealthInsuranceAvailability,
      export.HealthInsuranceAvailability);

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadHealthInsuranceAvailability())
    {
      local.HealthInsuranceAvailability.InsuranceId =
        entities.KeyOnly.InsuranceId;
    }

    ++local.HealthInsuranceAvailability.InsuranceId;

    try
    {
      CreateHealthInsuranceAvailability();
      export.HealthInsuranceAvailability.Assign(
        entities.HealthInsuranceAvailability);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OE0190_AVAILABLE_HEALTH_INS_AE";

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

  private static void MoveHealthInsuranceAvailability(
    HealthInsuranceAvailability source, HealthInsuranceAvailability target)
  {
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsuranceName = source.InsuranceName;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Zip5 = source.Zip5;
    target.Zip4 = source.Zip4;
    target.Cost = source.Cost;
    target.CostFrequency = source.CostFrequency;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EmployerName = source.EmployerName;
    target.EmpStreet1 = source.EmpStreet1;
    target.EmpStreet2 = source.EmpStreet2;
    target.EmpCity = source.EmpCity;
    target.EmpState = source.EmpState;
    target.EmpZip5 = source.EmpZip5;
    target.EmpZip4 = source.EmpZip4;
    target.EmpPhoneAreaCode = source.EmpPhoneAreaCode;
    target.EmpPhoneNo = source.EmpPhoneNo;
  }

  private void CreateHealthInsuranceAvailability()
  {
    var insuranceId = local.HealthInsuranceAvailability.InsuranceId;
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
    var cspNumber = entities.CsePerson.Number;

    entities.HealthInsuranceAvailability.Populated = false;
    Update("CreateHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetInt32(command, "insuranceId", insuranceId);
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
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.HealthInsuranceAvailability.InsuranceId = insuranceId;
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
    entities.HealthInsuranceAvailability.CspNumber = cspNumber;
    entities.HealthInsuranceAvailability.Populated = true;
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

  private bool ReadHealthInsuranceAvailability()
  {
    entities.KeyOnly.Populated = false;

    return Read("ReadHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KeyOnly.InsuranceId = db.GetInt32(reader, 0);
        entities.KeyOnly.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public HealthInsuranceAvailability KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

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

    private HealthInsuranceAvailability keyOnly;
    private HealthInsuranceAvailability healthInsuranceAvailability;
    private CsePerson csePerson;
  }
#endregion
}
