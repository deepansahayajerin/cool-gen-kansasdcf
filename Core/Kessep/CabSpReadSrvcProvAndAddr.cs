// Program: CAB_SP_READ_SRVC_PROV_AND_ADDR, ID: 371454598, model: 746.
// Short name: SWE00093
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_SP_READ_SRVC_PROV_AND_ADDR.
/// </para>
/// <para>
/// RESP: SRVPLN
/// </para>
/// </summary>
[Serializable]
public partial class CabSpReadSrvcProvAndAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SP_READ_SRVC_PROV_AND_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSpReadSrvcProvAndAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSpReadSrvcProvAndAddr.
  /// </summary>
  public CabSpReadSrvcProvAndAddr(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ------------------------------------------------------------------
    // 02/12/2001	M Ramirez	WR# 187		Add email address.
    // -------------------------------------------------------------------
    // 02/15/2001      Madhu Kumar                  WR#286 A
    // Add certification number.
    // ------------------------------------------------------------------
    if (ReadServiceProvider())
    {
      export.ServiceProvider.Assign(entities.ExistingServiceProvider);
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    local.Code.CodeName = "ADDRESS TYPE";

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadServiceProviderAddress())
    {
      export.Group.Update.ServiceProviderAddress.Assign(
        entities.ExistingServiceProviderAddress);
      export.Group.Update.Hidden.Type1 =
        entities.ExistingServiceProviderAddress.Type1;
      export.Group.Update.AddressTypeDesc.Cdvalue =
        entities.ExistingServiceProviderAddress.Type1;
      UseCabGetCodeValueDescription();
      export.Group.Next();
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = export.Group.Item.AddressTypeDesc.Cdvalue;
    MoveCode(local.Code, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, export.Group.Update.AddressTypeDesc);
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
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.EmailAddress =
          db.GetNullableString(reader, 5);
        entities.ExistingServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingServiceProvider.RoleCode =
          db.GetNullableString(reader, 7);
        entities.ExistingServiceProvider.EffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingServiceProvider.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ExistingServiceProvider.PhoneNumber =
          db.GetNullableInt32(reader, 11);
        entities.ExistingServiceProvider.PhoneExtension =
          db.GetNullableInt32(reader, 12);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderAddress()
  {
    return ReadEach("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingServiceProviderAddress.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ExistingServiceProviderAddress.Street1 =
          db.GetString(reader, 2);
        entities.ExistingServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ExistingServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ExistingServiceProviderAddress.StateProvince =
          db.GetString(reader, 5);
        entities.ExistingServiceProviderAddress.PostalCode =
          db.GetNullableString(reader, 6);
        entities.ExistingServiceProviderAddress.Zip =
          db.GetNullableString(reader, 7);
        entities.ExistingServiceProviderAddress.Zip4 =
          db.GetNullableString(reader, 8);
        entities.ExistingServiceProviderAddress.Country =
          db.GetString(reader, 9);
        entities.ExistingServiceProviderAddress.Populated = true;

        return true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of AddressTypeDesc.
      /// </summary>
      [JsonPropertyName("addressTypeDesc")]
      public CodeValue AddressTypeDesc
      {
        get => addressTypeDesc ??= new();
        set => addressTypeDesc = value;
      }

      /// <summary>
      /// A value of PromptAddressType.
      /// </summary>
      [JsonPropertyName("promptAddressType")]
      public Common PromptAddressType
      {
        get => promptAddressType ??= new();
        set => promptAddressType = value;
      }

      /// <summary>
      /// A value of PromptState.
      /// </summary>
      [JsonPropertyName("promptState")]
      public Common PromptState
      {
        get => promptState ??= new();
        set => promptState = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ServiceProviderAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CodeValue addressTypeDesc;
      private Common promptAddressType;
      private Common promptState;
      private Common common;
      private ServiceProviderAddress hidden;
      private ServiceProviderAddress serviceProviderAddress;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private ServiceProvider serviceProvider;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of SelectedCount.
    /// </summary>
    [JsonPropertyName("selectedCount")]
    public Common SelectedCount
    {
      get => selectedCount ??= new();
      set => selectedCount = value;
    }

    private Code code;
    private Common totalCount;
    private Common selectedCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("existingServiceProviderAddress")]
    public ServiceProviderAddress ExistingServiceProviderAddress
    {
      get => existingServiceProviderAddress ??= new();
      set => existingServiceProviderAddress = value;
    }

    private ServiceProvider existingServiceProvider;
    private ServiceProviderAddress existingServiceProviderAddress;
  }
#endregion
}
