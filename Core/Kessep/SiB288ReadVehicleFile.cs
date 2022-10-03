// Program: SI_B288_READ_VEHICLE_FILE, ID: 1625323183, model: 746.
// Short name: SWEXIE06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B288_READ_VEHICLE_FILE.
/// </summary>
[Serializable]
public partial class SiB288ReadVehicleFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B288_READ_VEHICLE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB288ReadVehicleFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB288ReadVehicleFile.
  /// </summary>
  public SiB288ReadVehicleFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXIE06", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of KdorVehicleRecord.
    /// </summary>
    [JsonPropertyName("kdorVehicleRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "Ssn",
      "DateOfBirth",
      "PersonNumber",
      "DriversLicenseNumber",
      "Vin",
      "Make",
      "Model",
      "Year",
      "PlateNumber",
      "Owner1OrganizationName",
      "Owner1FirstName",
      "Owner1MiddleName",
      "Owner1LastName",
      "Owner1Suffix",
      "Owner1MailingAddressLine1",
      "Owner1MailingAddresssLine2",
      "Owner1MailingCity",
      "Owner1MailingState",
      "Owner1MailingZipCode",
      "Owner1VestmentType",
      "Owner1HomeNumber",
      "Owner1BusinessNumber",
      "Owner2OrganizationName",
      "Owner2FirstName",
      "Owner2MiddleName",
      "Owner2LastName",
      "Owner2Suffix",
      "Owner2MailingAddressLine1",
      "Owner2MailingAddresssLine2",
      "Owner2MailingCity",
      "Owner2MailingState",
      "Owner2MailingZipCode",
      "Owner2VestmentType",
      "Owner2HomeNumber",
      "Owner2BusinessNumber",
      "Owner3OrganizationName",
      "Owner3FirstName",
      "Owner3MiddleName",
      "Owner3LastName",
      "Owner3Suffix",
      "Owner3MailingAddressLine1",
      "Owner3MailingAddresssLine2",
      "Owner3MailingCity",
      "Owner3MailingState",
      "Owner3MailingZipCode",
      "Owner3VestmentType",
      "Owner3HomeNumber",
      "Owner3BusinessNumber",
      "Owner4OrganizationName",
      "Owner4FirstName",
      "Owner4MiddleName",
      "Owner4LastName",
      "Owner4Suffix",
      "Owner4MailingAddressLine1",
      "Owner4MailingAddresssLine2",
      "Owner4MailingCity",
      "Owner4MailingState",
      "Owner4MailingZipCode",
      "Owner4VestmentType",
      "Owner4HomeNumber",
      "Owner4BusinessNumber",
      "Owner5OrganizationName",
      "Owner5FirstName",
      "Owner5MiddleName",
      "Owner5LastName",
      "Owner5Suffix",
      "Owner5MailingAddressLine1",
      "Owner5MailingAddresssLine2",
      "Owner5MailingCity",
      "Owner5MailingState",
      "Owner5MailingZipCode",
      "Owner5VestmentType",
      "Owner5HomeNumber",
      "Owner5BusinessNumber"
    })]
    public KdorVehicleRecord KdorVehicleRecord
    {
      get => kdorVehicleRecord ??= new();
      set => kdorVehicleRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private KdorVehicleRecord kdorVehicleRecord;
  }
#endregion
}
