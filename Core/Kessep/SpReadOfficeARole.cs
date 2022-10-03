// Program: SP_READ_OFFICE_A_ROLE, ID: 371782550, model: 746.
// Short name: SWE01416
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_READ_OFFICE_A_ROLE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// This common action block uses the Service Provider ID to read and return all
/// active  Service Provider Roles (Codes) assigned to a specific Service
/// Provider as well as the appropriate Office and Office Address details.
/// </para>
/// </summary>
[Serializable]
public partial class SpReadOfficeARole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_READ_OFFICE_A_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReadOfficeARole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReadOfficeARole.
  /// </summary>
  public SpReadOfficeARole(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************
    // ** P R O G R A M    M A I N T E N A N C E
    // **
    // ** DATE     *  DEVELOPER  *  DESCRIPTION
    // ** 04/28/95    R Grey        Create
    // ** 02/16/99    Anita Massey  Removed "and desired office
    // **              offers svcs thru desired osp" from the read
    // **              because  that was limiting losp to only listing
    // **              offices where the osp had a caseload.
    // ** 6/11/99   Anita Massey   Made read service provider a
    //                          select only in the read property
    // *******************************************
    // *********************************************
    // * This action block is designed with the
    // * assumption that when the OFFICE_SERVICE_
    // * PROVIDER Effective-Date is populated, the
    // * Discontinue-Date is set to maximum value
    // * until the 'actual' Discontinue-Date is
    // * supplied.
    // *********************************************
    export.Search.SystemGeneratedId = import.Search.SystemGeneratedId;

    if (ReadServiceProvider())
    {
      export.Search.Assign(entities.ServiceProvider);

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadOfficeServiceProviderOfficeOfficeAddress())
      {
        export.Export1.Update.OfficeServiceProvider.Assign(
          entities.OfficeServiceProvider);
        export.Export1.Update.Office.Assign(entities.Office);
        MoveOfficeAddress(entities.OfficeAddress,
          export.Export1.Update.OfficeAddress);

        // *******************************************
        // * Suppress duplicate phone numbers.
        // *******************************************
        if (export.Export1.Item.OfficeServiceProvider.WorkPhoneAreaCode == export
          .Export1.Item.Office.MainPhoneAreaCode.GetValueOrDefault() && export
          .Export1.Item.OfficeServiceProvider.WorkPhoneNumber == export
          .Export1.Item.Office.MainPhoneNumber.GetValueOrDefault())
        {
          export.Export1.Update.Office.MainPhoneAreaCode = 0;
          export.Export1.Update.Office.MainPhoneNumber = 0;
        }

        if (export.Export1.Item.OfficeServiceProvider.WorkFaxAreaCode.
          GetValueOrDefault() == export
          .Export1.Item.Office.MainFaxAreaCode.GetValueOrDefault() && export
          .Export1.Item.OfficeServiceProvider.WorkFaxNumber.
            GetValueOrDefault() == export
          .Export1.Item.Office.MainFaxPhoneNumber.GetValueOrDefault())
        {
          export.Export1.Update.Office.MainFaxAreaCode = 0;
          export.Export1.Update.Office.MainFaxPhoneNumber = 0;
        }

        export.Export1.Next();
      }
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (export.Export1.IsEmpty)
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";
    }
  }

  private static void MoveOfficeAddress(OfficeAddress source,
    OfficeAddress target)
  {
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StateProvince = source.StateProvince;
    target.Zip = source.Zip;
    target.Zip4 = source.Zip4;
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeOfficeAddress()
  {
    return ReadEach("ReadOfficeServiceProviderOfficeOfficeAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 10);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 11);
        entities.Office.TypeCode = db.GetString(reader, 12);
        entities.Office.Name = db.GetString(reader, 13);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 14);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 15);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 16);
        entities.OfficeAddress.Type1 = db.GetString(reader, 17);
        entities.OfficeAddress.Street1 = db.GetString(reader, 18);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 19);
        entities.OfficeAddress.City = db.GetString(reader, 20);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 21);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 22);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 23);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeAddress.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "servicePrvderId", import.Search.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
    }

    private ServiceProvider search;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private OfficeServiceProvider officeServiceProvider;
      private Office office;
      private OfficeAddress officeAddress;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private ServiceProvider search;
    private Array<ExportGroup> export1;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
  }
#endregion
}
