// Program: SP_UPDATE_SERVICE_PROVIDER, ID: 371454604, model: 746.
// Short name: SWE01448
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_SERVICE_PROVIDER.
/// </summary>
[Serializable]
public partial class SpUpdateServiceProvider: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_SERVICE_PROVIDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateServiceProvider(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateServiceProvider.
  /// </summary>
  public SpUpdateServiceProvider(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ------------------------------------------------------------------
    // 02/12/2001	M Ramirez	WR# 187		Add email address
    // 02/14/2001      Madhu Kumar     WR# 286 A  Add certification number.
    // ------------------------------------------------------------------
    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    try
    {
      UpdateServiceProvider();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SERVICE_PROVIDER_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SERVICE_PROVIDER_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.LastName = db.GetString(reader, 4);
        entities.ServiceProvider.FirstName = db.GetString(reader, 5);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 6);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 7);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 8);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 9);
        entities.ServiceProvider.EffectiveDate = db.GetNullableDate(reader, 10);
        entities.ServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.ServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.ServiceProvider.PhoneNumber = db.GetNullableInt32(reader, 13);
        entities.ServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 14);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateServiceProvider()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var userId = import.ServiceProvider.UserId;
    var lastName = import.ServiceProvider.LastName;
    var firstName = import.ServiceProvider.FirstName;
    var middleInitial = import.ServiceProvider.MiddleInitial;
    var emailAddress = import.ServiceProvider.EmailAddress ?? "";
    var certificationNumber = import.ServiceProvider.CertificationNumber ?? "";
    var roleCode = import.ServiceProvider.RoleCode ?? "";
    var effectiveDate = import.ServiceProvider.EffectiveDate;
    var discontinueDate = import.ServiceProvider.DiscontinueDate;
    var phoneAreaCode =
      import.ServiceProvider.PhoneAreaCode.GetValueOrDefault();
    var phoneNumber = import.ServiceProvider.PhoneNumber.GetValueOrDefault();
    var phoneExtension =
      import.ServiceProvider.PhoneExtension.GetValueOrDefault();

    entities.ServiceProvider.Populated = false;
    Update("UpdateServiceProvider",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "userId", userId);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableString(command, "certificationNo", certificationNumber);
        db.SetNullableString(command, "roleCode", roleCode);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "phoneExtension", phoneExtension);
        db.SetInt32(
          command, "servicePrvderId",
          entities.ServiceProvider.SystemGeneratedId);
      });

    entities.ServiceProvider.LastUpdatedBy = lastUpdatedBy;
    entities.ServiceProvider.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.ServiceProvider.UserId = userId;
    entities.ServiceProvider.LastName = lastName;
    entities.ServiceProvider.FirstName = firstName;
    entities.ServiceProvider.MiddleInitial = middleInitial;
    entities.ServiceProvider.EmailAddress = emailAddress;
    entities.ServiceProvider.CertificationNumber = certificationNumber;
    entities.ServiceProvider.RoleCode = roleCode;
    entities.ServiceProvider.EffectiveDate = effectiveDate;
    entities.ServiceProvider.DiscontinueDate = discontinueDate;
    entities.ServiceProvider.PhoneAreaCode = phoneAreaCode;
    entities.ServiceProvider.PhoneNumber = phoneNumber;
    entities.ServiceProvider.PhoneExtension = phoneExtension;
    entities.ServiceProvider.Populated = true;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
  }
#endregion
}
