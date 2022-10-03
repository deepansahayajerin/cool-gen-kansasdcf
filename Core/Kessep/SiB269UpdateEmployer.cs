// Program: SI_B269_UPDATE_EMPLOYER, ID: 373411353, model: 746.
// Short name: SWE02631
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B269_UPDATE_EMPLOYER.
/// </summary>
[Serializable]
public partial class SiB269UpdateEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B269_UPDATE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB269UpdateEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB269UpdateEmployer.
  /// </summary>
  public SiB269UpdateEmployer(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      UpdateEmployer();

      try
      {
        UpdateEmployerAddress();
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_ADDRESS_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_ADDRESS_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateEmployer()
  {
    var kansasId = import.NewEmployer.KansasId ?? "";
    var name = import.NewEmployer.Name ?? "";
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTstamp = Now();
    var phoneNo = import.NewEmployer.PhoneNo ?? "";
    var areaCode = import.NewEmployer.AreaCode.GetValueOrDefault();

    import.PersEmployer.Populated = false;
    Update("UpdateEmployer",
      (db, command) =>
      {
        db.SetNullableString(command, "kansasId", kansasId);
        db.SetNullableString(command, "name", name);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "phoneNo", phoneNo);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetInt32(command, "identifier", import.PersEmployer.Identifier);
      });

    import.PersEmployer.KansasId = kansasId;
    import.PersEmployer.Name = name;
    import.PersEmployer.LastUpdatedBy = lastUpdatedBy;
    import.PersEmployer.LastUpdatedTstamp = lastUpdatedTstamp;
    import.PersEmployer.PhoneNo = phoneNo;
    import.PersEmployer.AreaCode = areaCode;
    import.PersEmployer.Populated = true;
  }

  private void UpdateEmployerAddress()
  {
    System.Diagnostics.Debug.Assert(import.PersEmployerAddress.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var street1 = import.NewEmployerAddress.Street1 ?? "";
    var street2 = import.NewEmployerAddress.Street2 ?? "";
    var city = import.NewEmployerAddress.City ?? "";
    var state = import.NewEmployerAddress.State ?? "";
    var zipCode = import.NewEmployerAddress.ZipCode ?? "";
    var zip4 = import.NewEmployerAddress.Zip4 ?? "";
    var zip3 = import.NewEmployerAddress.Zip3 ?? "";
    var note = import.NewEmployerAddress.Note ?? "";

    import.PersEmployerAddress.Populated = false;
    Update("UpdateEmployerAddress",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          import.PersEmployerAddress.Identifier.GetValueOrDefault());
        db.SetInt32(command, "empId", import.PersEmployerAddress.EmpId);
      });

    import.PersEmployerAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    import.PersEmployerAddress.LastUpdatedBy = lastUpdatedBy;
    import.PersEmployerAddress.Street1 = street1;
    import.PersEmployerAddress.Street2 = street2;
    import.PersEmployerAddress.City = city;
    import.PersEmployerAddress.State = state;
    import.PersEmployerAddress.ZipCode = zipCode;
    import.PersEmployerAddress.Zip4 = zip4;
    import.PersEmployerAddress.Zip3 = zip3;
    import.PersEmployerAddress.Note = note;
    import.PersEmployerAddress.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of NewEmployer.
    /// </summary>
    [JsonPropertyName("newEmployer")]
    public Employer NewEmployer
    {
      get => newEmployer ??= new();
      set => newEmployer = value;
    }

    /// <summary>
    /// A value of NewEmployerAddress.
    /// </summary>
    [JsonPropertyName("newEmployerAddress")]
    public EmployerAddress NewEmployerAddress
    {
      get => newEmployerAddress ??= new();
      set => newEmployerAddress = value;
    }

    /// <summary>
    /// A value of PersEmployer.
    /// </summary>
    [JsonPropertyName("persEmployer")]
    public Employer PersEmployer
    {
      get => persEmployer ??= new();
      set => persEmployer = value;
    }

    /// <summary>
    /// A value of PersEmployerAddress.
    /// </summary>
    [JsonPropertyName("persEmployerAddress")]
    public EmployerAddress PersEmployerAddress
    {
      get => persEmployerAddress ??= new();
      set => persEmployerAddress = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Employer newEmployer;
    private EmployerAddress newEmployerAddress;
    private Employer persEmployer;
    private EmployerAddress persEmployerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
