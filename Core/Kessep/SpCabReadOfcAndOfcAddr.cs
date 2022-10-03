// Program: SP_CAB_READ_OFC_AND_OFC_ADDR, ID: 371782950, model: 746.
// Short name: SWE00091
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_READ_OFC_AND_OFC_ADDR.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReadOfcAndOfcAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_OFC_AND_OFC_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadOfcAndOfcAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadOfcAndOfcAddr.
  /// </summary>
  public SpCabReadOfcAndOfcAddr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************
    // 6/14/99    Anita Massey     set read properties to select only
    // **************************
    // ******************************************************************************************************
    // 12/02/2004     M J Quinn   WR040802    Expanded the Garden City
    // Customer Service Center pilot.                   Modules SWE02231,
    // SWEOFFCS, SWEOFFCP, SWE01441, SWE01311, SWE00091 are included in this
    // work request.
    // ******************************************************************************************************
    if (ReadOffice3())
    {
      export.Office.Assign(entities.Office);
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    if (AsChar(import.PromptCustomerServiceC.Flag) == 'S')
    {
      if (ReadOffice2())
      {
        MoveOffice(entities.CustomerServiceCenter, export.CustomerServiceCenter);
          
      }
    }
    else if (ReadOffice1())
    {
      MoveOffice(entities.CustomerServiceCenter, export.CustomerServiceCenter);
    }

    if (ReadFips())
    {
      export.Fips.Assign(entities.Fips);
    }

    if (ReadCseOrganization())
    {
      MoveCseOrganization(entities.CseOrganization, export.CseOrganization);
    }
    else
    {
      ExitState = "COUNTY_NF";

      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadOfficeAddress())
    {
      export.Group.Update.OfficeAddress.Assign(entities.OfficeAddress);
      export.Group.Next();
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "typeCode", entities.Office.CogTypeCode ?? "");
        db.SetString(command, "organztnId", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offIdentifier", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.OffIdentifier = db.GetNullableInt32(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadOffice1()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.Office.OffOffice.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 1);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.CustomerServiceCenter.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", import.CustomerServiceCenter.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CustomerServiceCenter.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CustomerServiceCenter.Name = db.GetString(reader, 1);
        entities.CustomerServiceCenter.OffOffice =
          db.GetNullableInt32(reader, 2);
        entities.CustomerServiceCenter.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 5);
        entities.Office.CogCode = db.GetNullableString(reader, 6);
        entities.Office.EffectiveDate = db.GetDate(reader, 7);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 9);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 10);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 11);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeAddress()
  {
    return ReadEach("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.CreatedBy = db.GetString(reader, 10);
        entities.OfficeAddress.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.OfficeAddress.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.OfficeAddress.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.OfficeAddress.Populated = true;

        return true;
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
    /// A value of PromptCustomerServiceC.
    /// </summary>
    [JsonPropertyName("promptCustomerServiceC")]
    public Common PromptCustomerServiceC
    {
      get => promptCustomerServiceC ??= new();
      set => promptCustomerServiceC = value;
    }

    /// <summary>
    /// A value of PromtOffice.
    /// </summary>
    [JsonPropertyName("promtOffice")]
    public Common PromtOffice
    {
      get => promtOffice ??= new();
      set => promtOffice = value;
    }

    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
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

    private Common promptCustomerServiceC;
    private Common promtOffice;
    private Office customerServiceCenter;
    private Office office;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of AddTypeDesc.
      /// </summary>
      [JsonPropertyName("addTypeDesc")]
      public CodeValue AddTypeDesc
      {
        get => addTypeDesc ??= new();
        set => addTypeDesc = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public OfficeAddress Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private OfficeAddress officeAddress;
      private CodeValue addTypeDesc;
      private Common promptAddressType;
      private Common promptState;
      private OfficeAddress hidden;
    }

    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private Office customerServiceCenter;
    private Fips fips;
    private CseOrganization cseOrganization;
    private Office office;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CustomerServiceCenter.
    /// </summary>
    [JsonPropertyName("customerServiceCenter")]
    public Office CustomerServiceCenter
    {
      get => customerServiceCenter ??= new();
      set => customerServiceCenter = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private Office customerServiceCenter;
    private Fips fips;
    private CseOrganization cseOrganization;
    private Office office;
    private OfficeAddress officeAddress;
  }
#endregion
}
