// Program: SI_ADD_INCOME_SOURCE_ADDRESS, ID: 371762200, model: 746.
// Short name: SWE01095
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ADD_INCOME_SOURCE_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiAddIncomeSourceAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADD_INCOME_SOURCE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddIncomeSourceAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddIncomeSourceAddress.
  /// </summary>
  public SiAddIncomeSourceAddress(IContext context, Import import, Export export)
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
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 09-24-95 K Evans     Initial Development
    // 10-26/99 D Lowry     Export the identifier to resolve a PR
    // ---------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    if (ReadEmployer())
    {
      try
      {
        CreateEmployerAddress();
        export.EmployerAddress.Identifier = entities.EmployerAddress.Identifier;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_ADDRESS_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_ADDRESS_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "EMPLOYER_NF_RB";
    }
  }

  private void CreateEmployerAddress()
  {
    var locationType =
      Substring(ToUpper(import.EmployerAddress.LocationType), 1, 1);
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var street1 = Substring(ToUpper(import.EmployerAddress.Street1), 1, 25);
    var street2 = Substring(ToUpper(import.EmployerAddress.Street2), 1, 25);
    var city = Substring(ToUpper(import.EmployerAddress.City), 1, 15);
    var street3 = Substring(ToUpper(import.EmployerAddress.Street3), 1, 25);
    var street4 = Substring(ToUpper(import.EmployerAddress.Street4), 1, 25);
    var province = Substring(ToUpper(import.EmployerAddress.Province), 1, 5);
    var country = Substring(ToUpper(import.EmployerAddress.Country), 1, 2);
    var postalCode = import.EmployerAddress.PostalCode ?? "";
    var state = Substring(ToUpper(import.EmployerAddress.State), 1, 2);
    var zipCode = import.EmployerAddress.ZipCode ?? "";
    var zip4 = import.EmployerAddress.Zip4 ?? "";
    var zip3 = import.EmployerAddress.Zip3 ?? "";
    var empId = entities.Employer.Identifier;
    var note = import.EmployerAddress.Note ?? "";

    CheckValid<EmployerAddress>("LocationType", locationType);
    entities.EmployerAddress.Populated = false;
    Update("CreateEmployerAddress",
      (db, command) =>
      {
        db.SetString(command, "locationType", locationType);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetDateTime(command, "identifier", createdTimestamp);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetInt32(command, "empId", empId);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "note", note);
      });

    entities.EmployerAddress.LocationType = locationType;
    entities.EmployerAddress.CreatedTimestamp = createdTimestamp;
    entities.EmployerAddress.CreatedBy = createdBy;
    entities.EmployerAddress.Street1 = street1;
    entities.EmployerAddress.Street2 = street2;
    entities.EmployerAddress.City = city;
    entities.EmployerAddress.Identifier = createdTimestamp;
    entities.EmployerAddress.Street3 = street3;
    entities.EmployerAddress.Street4 = street4;
    entities.EmployerAddress.Province = province;
    entities.EmployerAddress.Country = country;
    entities.EmployerAddress.PostalCode = postalCode;
    entities.EmployerAddress.State = state;
    entities.EmployerAddress.ZipCode = zipCode;
    entities.EmployerAddress.Zip4 = zip4;
    entities.EmployerAddress.Zip3 = zip3;
    entities.EmployerAddress.EmpId = empId;
    entities.EmployerAddress.Note = note;
    entities.EmployerAddress.Populated = true;
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

    private EmployerAddress employerAddress;
    private Employer employer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private EmployerAddress employerAddress;
    private Employer employer;
  }
#endregion
}
