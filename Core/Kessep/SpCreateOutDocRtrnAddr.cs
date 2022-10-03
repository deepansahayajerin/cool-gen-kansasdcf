// Program: SP_CREATE_OUT_DOC_RTRN_ADDR, ID: 371915675, model: 746.
// Short name: SWE01641
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CREATE_OUT_DOC_RTRN_ADDR.
/// </para>
/// <para>
/// This action block creates a new occurrence of Out_Doc_Rtrn_Addr for the 
/// currently signed on user_id.
/// </para>
/// </summary>
[Serializable]
public partial class SpCreateOutDocRtrnAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OUT_DOC_RTRN_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOutDocRtrnAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOutDocRtrnAddr.
  /// </summary>
  public SpCreateOutDocRtrnAddr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date		Developer	Request
    // -------------------------------------------------------------------
    // 04/01/2002	M Ramirez	PR142174
    // Reflect current office information.
    // -------------------------------------------------------------------
    if (Lt(local.Current.Date, import.Current.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadOffice())
    {
      if (Lt(entities.Office.DiscontinueDate, local.Current.Date))
      {
        ExitState = "SP0000_SEL_OFC_IS_DISCONTINUED";

        return;
      }
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (ReadOfficeServiceProvider())
    {
      if (Lt(entities.OfficeServiceProvider.DiscontinueDate, local.Current.Date))
        
      {
        ExitState = "SP0000_SEL_OSP_IS_DISCONTINUED";

        return;
      }
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    if (!ReadServiceProviderAddress())
    {
      if (!ReadOfficeAddress())
      {
        ExitState = "OFFICE_ADDRESS_NF";

        return;
      }
    }

    if (ReadOutDocRtrnAddr())
    {
      if (entities.OutDocRtrnAddr.ServProvSysGenId == import
        .ServiceProvider.SystemGeneratedId && entities
        .OutDocRtrnAddr.OfficeSysGenId == import.Office.SystemGeneratedId && Equal
        (entities.OutDocRtrnAddr.OspRoleCode,
        import.OfficeServiceProvider.RoleCode) && Equal
        (entities.OutDocRtrnAddr.OspEffectiveDate,
        import.OfficeServiceProvider.EffectiveDate))
      {
      }
      else
      {
        try
        {
          UpdateOutDocRtrnAddr();
          MoveOutDocRtrnAddr(entities.OutDocRtrnAddr, export.OutDocRtrnAddr);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_OUT_DOC_RTRN_ADDR_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      return;
    }

    try
    {
      CreateOutDocRtrnAddr();
      MoveOutDocRtrnAddr(entities.OutDocRtrnAddr, export.OutDocRtrnAddr);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_OUT_DOC_RTRN_ADDR_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveOutDocRtrnAddr(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.OspRoleCode = source.OspRoleCode;
    target.OspEffectiveDate = source.OspEffectiveDate;
    target.OfficeSysGenId = source.OfficeSysGenId;
    target.ServProvSysGenId = source.ServProvSysGenId;
  }

  private void CreateOutDocRtrnAddr()
  {
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var ospRoleCode = import.OfficeServiceProvider.RoleCode;
    var ospEffectiveDate = import.OfficeServiceProvider.EffectiveDate;
    var officeSysGenId = import.Office.SystemGeneratedId;
    var servProvSysGenId = import.ServiceProvider.SystemGeneratedId;

    entities.OutDocRtrnAddr.Populated = false;
    Update("CreateOutDocRtrnAddr",
      (db, command) =>
      {
        db.SetInt32(command, "ospWkPhoneNum", 0);
        db.SetInt32(command, "ospWkPhAreaCd", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "ospWkPhoneExt", "");
        db.SetNullableString(command, "ospCertNum", "");
        db.SetString(command, "ospRoleCode", ospRoleCode);
        db.SetDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetInt32(command, "officeSysGenId", officeSysGenId);
        db.SetString(command, "officeName", "");
        db.SetNullableString(command, "offcAddrStreet1", "");
        db.SetNullableString(command, "offcAddrCity", "");
        db.SetNullableString(command, "offcAddrStProv", "");
        db.SetNullableString(command, "offcAddrZip4", "");
        db.SetInt32(command, "srvPrvSysGenId", servProvSysGenId);
        db.SetString(command, "servProvUserId", "");
        db.SetString(command, "srvProvLastName", "");
        db.SetString(command, "srvPrvFirstName", "");
        db.SetNullableString(command, "servProvMi", "");
        db.SetNullableString(command, "offcAddrZip3", "");
      });

    entities.OutDocRtrnAddr.CreatedBy = createdBy;
    entities.OutDocRtrnAddr.CreatedTimestamp = createdTimestamp;
    entities.OutDocRtrnAddr.OspRoleCode = ospRoleCode;
    entities.OutDocRtrnAddr.OspEffectiveDate = ospEffectiveDate;
    entities.OutDocRtrnAddr.OfficeSysGenId = officeSysGenId;
    entities.OutDocRtrnAddr.ServProvSysGenId = servProvSysGenId;
    entities.OutDocRtrnAddr.Populated = true;
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOutDocRtrnAddr()
  {
    entities.OutDocRtrnAddr.Populated = false;

    return Read("ReadOutDocRtrnAddr",
      (db, command) =>
      {
        db.SetString(command, "createdBy", local.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.OutDocRtrnAddr.CreatedBy = db.GetString(reader, 0);
        entities.OutDocRtrnAddr.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.OutDocRtrnAddr.OspRoleCode = db.GetString(reader, 2);
        entities.OutDocRtrnAddr.OspEffectiveDate = db.GetDate(reader, 3);
        entities.OutDocRtrnAddr.OfficeSysGenId = db.GetInt32(reader, 4);
        entities.OutDocRtrnAddr.ServProvSysGenId = db.GetInt32(reader, 5);
        entities.OutDocRtrnAddr.Populated = true;
      });
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
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private void UpdateOutDocRtrnAddr()
  {
    var ospRoleCode = import.OfficeServiceProvider.RoleCode;
    var ospEffectiveDate = import.OfficeServiceProvider.EffectiveDate;
    var officeSysGenId = import.Office.SystemGeneratedId;
    var servProvSysGenId = import.ServiceProvider.SystemGeneratedId;

    entities.OutDocRtrnAddr.Populated = false;
    Update("UpdateOutDocRtrnAddr",
      (db, command) =>
      {
        db.SetString(command, "ospRoleCode", ospRoleCode);
        db.SetDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetInt32(command, "officeSysGenId", officeSysGenId);
        db.SetInt32(command, "srvPrvSysGenId", servProvSysGenId);
        db.SetString(command, "createdBy", entities.OutDocRtrnAddr.CreatedBy);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OutDocRtrnAddr.CreatedTimestamp.GetValueOrDefault());
      });

    entities.OutDocRtrnAddr.OspRoleCode = ospRoleCode;
    entities.OutDocRtrnAddr.OspEffectiveDate = ospEffectiveDate;
    entities.OutDocRtrnAddr.OfficeSysGenId = officeSysGenId;
    entities.OutDocRtrnAddr.ServProvSysGenId = servProvSysGenId;
    entities.OutDocRtrnAddr.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private DateWorkArea current;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ZdelLocalSpAddrFound.
    /// </summary>
    [JsonPropertyName("zdelLocalSpAddrFound")]
    public Common ZdelLocalSpAddrFound
    {
      get => zdelLocalSpAddrFound ??= new();
      set => zdelLocalSpAddrFound = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ServiceProviderAddress Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private DateWorkArea current;
    private ServiceProvider serviceProvider;
    private Common zdelLocalSpAddrFound;
    private ServiceProviderAddress zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ZdelOutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("zdelOutDocRtrnAddr")]
    public OutDocRtrnAddr ZdelOutDocRtrnAddr
    {
      get => zdelOutDocRtrnAddr ??= new();
      set => zdelOutDocRtrnAddr = value;
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
    /// A value of ZdelOfficeAddress.
    /// </summary>
    [JsonPropertyName("zdelOfficeAddress")]
    public OfficeAddress ZdelOfficeAddress
    {
      get => zdelOfficeAddress ??= new();
      set => zdelOfficeAddress = value;
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

    private OutDocRtrnAddr outDocRtrnAddr;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeAddress officeAddress;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private OutDocRtrnAddr zdelOutDocRtrnAddr;
    private ServiceProviderAddress zdelServiceProviderAddress;
    private OfficeAddress zdelOfficeAddress;
    private OfficeServiceProvider zdelOfficeServiceProvider;
    private ServiceProvider zdelServiceProvider;
    private Office zdelOffice;
  }
#endregion
}
