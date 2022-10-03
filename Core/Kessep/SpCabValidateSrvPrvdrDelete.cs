// Program: SP_CAB_VALIDATE_SRV_PRVDR_DELETE, ID: 371454597, model: 746.
// Short name: SWE00105
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_VALIDATE_SRV_PRVDR_DELETE.
/// </para>
/// <para>
/// RESP: SRVPLN
/// </para>
/// </summary>
[Serializable]
public partial class SpCabValidateSrvPrvdrDelete: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_VALIDATE_SRV_PRVDR_DELETE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabValidateSrvPrvdrDelete(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabValidateSrvPrvdrDelete.
  /// </summary>
  public SpCabValidateSrvPrvdrDelete(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadOfficeServiceProvider())
    {
      ExitState = "CANNOT_DEL_REL_TO_OFC_SERV_PROV";

      return;
    }

    if (ReadReceiptResearchAssignment())
    {
      ExitState = "CANNOT_DEL_REL_TO_REC_RSRCH_ASGN";

      return;
    }

    if (ReadAdministrativeActCertification())
    {
      ExitState = "CANNOT_DEL_REL_TO_ADMIN_ACT_CERT";

      return;
    }

    if (ReadServiceProviderProfile())
    {
      ExitState = "SP0000_CANT_DEL_REL_TO_SP_PROFIL";
    }
  }

  private bool ReadAdministrativeActCertification()
  {
    entities.ExistingAdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeActCertification.Type1 =
          db.GetString(reader, 2);
        entities.ExistingAdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.ExistingAdministrativeActCertification.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingAdministrativeActCertification.TanfCode =
          db.GetString(reader, 5);
        entities.ExistingAdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingAdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingAdministrativeActCertification.Type1);
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadReceiptResearchAssignment()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;

    return Read("ReadReceiptResearchAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingReceiptResearchAssignment.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderProfile()
  {
    entities.ServiceProviderProfile.Populated = false;

    return Read("ReadServiceProviderProfile",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGenId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 1);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProviderProfile.Populated = true;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of ExistingReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("existingReceiptResearchAssignment")]
    public ReceiptResearchAssignment ExistingReceiptResearchAssignment
    {
      get => existingReceiptResearchAssignment ??= new();
      set => existingReceiptResearchAssignment = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("existingAdministrativeActCertification")]
    public AdministrativeActCertification ExistingAdministrativeActCertification
    {
      get => existingAdministrativeActCertification ??= new();
      set => existingAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    private ServiceProviderProfile serviceProviderProfile;
    private ReceiptResearchAssignment existingReceiptResearchAssignment;
    private AdministrativeActCertification existingAdministrativeActCertification;
      
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
  }
#endregion
}
