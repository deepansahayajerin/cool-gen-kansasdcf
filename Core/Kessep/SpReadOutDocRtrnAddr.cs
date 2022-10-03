// Program: SP_READ_OUT_DOC_RTRN_ADDR, ID: 371915676, model: 746.
// Short name: SWE01643
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_READ_OUT_DOC_RTRN_ADDR.
/// </para>
/// <para>
/// This common action block retrieves the current outgoing document return 
/// address information for the user that is logged on.
/// </para>
/// </summary>
[Serializable]
public partial class SpReadOutDocRtrnAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_READ_OUT_DOC_RTRN_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReadOutDocRtrnAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReadOutDocRtrnAddr.
  /// </summary>
  public SpReadOutDocRtrnAddr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // Date		Developer	Request
    // ---------------------------------------------------------------
    // 04/14/1997	Jack Rookard
    // Initial Development
    // 03/25/2002	M Ramirez	PR142174
    // Reflect current office information.  Changed CAB to just perform READ
    // ---------------------------------------------------------------
    if (ReadOutDocRtrnAddr())
    {
      export.OutDocRtrnAddr.Assign(entities.OutDocRtrnAddr);

      return;
    }

    ExitState = "SP0000_OUT_DOC_RTRN_ADDR_NF";
  }

  private bool ReadOutDocRtrnAddr()
  {
    entities.OutDocRtrnAddr.Populated = false;

    return Read("ReadOutDocRtrnAddr",
      (db, command) =>
      {
        db.SetString(command, "createdBy", global.UserId);
      },
      (db, reader) =>
      {
        entities.OutDocRtrnAddr.OspWorkPhoneNumber = db.GetInt32(reader, 0);
        entities.OutDocRtrnAddr.OspWorkPhoneAreaCode = db.GetInt32(reader, 1);
        entities.OutDocRtrnAddr.CreatedBy = db.GetString(reader, 2);
        entities.OutDocRtrnAddr.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.OutDocRtrnAddr.OspWorkPhoneExtension =
          db.GetNullableString(reader, 4);
        entities.OutDocRtrnAddr.OspCertificationNumber =
          db.GetNullableString(reader, 5);
        entities.OutDocRtrnAddr.OspLocalContactCode =
          db.GetNullableInt32(reader, 6);
        entities.OutDocRtrnAddr.OspRoleCode = db.GetString(reader, 7);
        entities.OutDocRtrnAddr.OspEffectiveDate = db.GetDate(reader, 8);
        entities.OutDocRtrnAddr.OfficeSysGenId = db.GetInt32(reader, 9);
        entities.OutDocRtrnAddr.OfficeName = db.GetString(reader, 10);
        entities.OutDocRtrnAddr.OffcAddrStreet1 =
          db.GetNullableString(reader, 11);
        entities.OutDocRtrnAddr.OffcAddrStreet2 =
          db.GetNullableString(reader, 12);
        entities.OutDocRtrnAddr.OffcAddrCity = db.GetNullableString(reader, 13);
        entities.OutDocRtrnAddr.OffcAddrState =
          db.GetNullableString(reader, 14);
        entities.OutDocRtrnAddr.OffcAddrPostalCode =
          db.GetNullableString(reader, 15);
        entities.OutDocRtrnAddr.OffcAddrZip = db.GetNullableString(reader, 16);
        entities.OutDocRtrnAddr.OffcAddrZip4 = db.GetNullableString(reader, 17);
        entities.OutDocRtrnAddr.OffcAddrCountry =
          db.GetNullableString(reader, 18);
        entities.OutDocRtrnAddr.ServProvSysGenId = db.GetInt32(reader, 19);
        entities.OutDocRtrnAddr.ServProvUserId = db.GetString(reader, 20);
        entities.OutDocRtrnAddr.ServProvLastName = db.GetString(reader, 21);
        entities.OutDocRtrnAddr.ServProvFirstName = db.GetString(reader, 22);
        entities.OutDocRtrnAddr.ServProvMi = db.GetNullableString(reader, 23);
        entities.OutDocRtrnAddr.OffcAddrZip3 = db.GetNullableString(reader, 24);
        entities.OutDocRtrnAddr.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    private OutDocRtrnAddr outDocRtrnAddr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    private DateWorkArea zdelLocalCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelOfficeAddress.
    /// </summary>
    [JsonPropertyName("zdelOfficeAddress")]
    public OfficeAddress ZdelOfficeAddress
    {
      get => zdelOfficeAddress ??= new();
      set => zdelOfficeAddress = value;
    }

    /// <summary>
    /// A value of ZdelServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("zdelServiceProviderAddress")]
    public ServiceProviderAddress ZdelServiceProviderAddress
    {
      get => zdelServiceProviderAddress ??= new();
      set => zdelServiceProviderAddress = value;
    }

    /// <summary>
    /// A value of ZdelOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelOfficeServiceProvider")]
    public OfficeServiceProvider ZdelOfficeServiceProvider
    {
      get => zdelOfficeServiceProvider ??= new();
      set => zdelOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelServiceProvider")]
    public ServiceProvider ZdelServiceProvider
    {
      get => zdelServiceProvider ??= new();
      set => zdelServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelOffice.
    /// </summary>
    [JsonPropertyName("zdelOffice")]
    public Office ZdelOffice
    {
      get => zdelOffice ??= new();
      set => zdelOffice = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    private OfficeAddress zdelOfficeAddress;
    private ServiceProviderAddress zdelServiceProviderAddress;
    private OfficeServiceProvider zdelOfficeServiceProvider;
    private ServiceProvider zdelServiceProvider;
    private Office zdelOffice;
    private OutDocRtrnAddr outDocRtrnAddr;
  }
#endregion
}
