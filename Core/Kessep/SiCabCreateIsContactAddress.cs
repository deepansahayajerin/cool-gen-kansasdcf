// Program: SI_CAB_CREATE_IS_CONTACT_ADDRESS, ID: 373465509, model: 746.
// Short name: SWE02793
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_CREATE_IS_CONTACT_ADDRESS.
/// </summary>
[Serializable]
public partial class SiCabCreateIsContactAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_CREATE_IS_CONTACT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabCreateIsContactAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabCreateIsContactAddress.
  /// </summary>
  public SiCabCreateIsContactAddress(IContext context, Import import,
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
    UseCabSetMaximumDiscontinueDate();
    MoveInterstateContactAddress(import.InterstateContactAddress,
      local.InterstateContactAddress);

    if (Lt(local.Null1.Timestamp,
      import.InterstateContactAddress.CreatedTimestamp))
    {
      local.InterstateContactAddress.CreatedTimestamp =
        import.InterstateContactAddress.CreatedTimestamp;
    }
    else
    {
      local.InterstateContactAddress.CreatedTimestamp = Now();
    }

    if (!IsEmpty(import.InterstateContactAddress.CreatedBy))
    {
      local.InterstateContactAddress.CreatedBy =
        import.InterstateContactAddress.CreatedBy;
    }
    else
    {
      local.InterstateContactAddress.CreatedBy = global.UserId;
    }

    if (Lt(local.Null1.Date, import.InterstateContactAddress.EndDate) && Lt
      (import.InterstateContactAddress.EndDate, local.Max.Date))
    {
      local.InterstateContactAddress.EndDate =
        import.InterstateContactAddress.EndDate;
    }
    else
    {
      local.InterstateContactAddress.EndDate = local.Max.Date;
    }

    local.InterstateContactAddress.LastUpdatedTimestamp =
      local.InterstateContactAddress.CreatedTimestamp;
    local.InterstateContactAddress.LastUpdatedBy =
      local.InterstateContactAddress.CreatedBy;

    switch(AsChar(import.InterstateContactAddress.LocationType))
    {
      case 'D':
        local.InterstateContactAddress.LocationType = "D";

        break;
      case 'F':
        local.InterstateContactAddress.LocationType = "F";

        break;
      case ' ':
        if (!IsEmpty(import.InterstateContactAddress.Country))
        {
          local.InterstateContactAddress.LocationType = "F";
        }
        else
        {
          local.InterstateContactAddress.LocationType = "D";
        }

        break;
      default:
        ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

        return;
    }

    if (IsEmpty(import.InterstateContactAddress.Street1) || IsEmpty
      (import.InterstateContactAddress.City))
    {
      ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

      return;
    }

    if (AsChar(local.InterstateContactAddress.LocationType) == 'D')
    {
      if (IsEmpty(import.InterstateContactAddress.ZipCode))
      {
        ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

        return;
      }

      if (Length(TrimEnd(import.InterstateContactAddress.ZipCode)) < 5)
      {
        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Verify(import.InterstateContactAddress.ZipCode, "0123456789") > 0)
      {
        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (!IsEmpty(import.InterstateContactAddress.Zip4))
      {
        if (Length(TrimEnd(import.InterstateContactAddress.Zip4)) < 4)
        {
          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }

        if (Verify(import.InterstateContactAddress.Zip4, "0123456789") > 0)
        {
          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }
    }
    else if (IsEmpty(import.InterstateContactAddress.PostalCode))
    {
      ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

      return;
    }

    if (IsEmpty(import.InterstateContactAddress.Type1))
    {
      local.InterstateContactAddress.Type1 = "CT";
    }

    local.InterstateContactAddress.StartDate = local.Null1.Date;

    foreach(var item in ReadInterstateContactAddress())
    {
      if (Equal(local.InterstateContactAddress.StartDate, local.Null1.Date))
      {
        local.InterstateContactAddress.StartDate =
          entities.InterstateContactAddress.StartDate;
        UseSiCabUpdateIsContactAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.InterstateContactAddress.StartDate =
          local.InterstateContactAddress.StartDate;
      }
      else
      {
        UseSiCabDeleteIsContactAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    if (Equal(local.InterstateContactAddress.StartDate, local.Null1.Date))
    {
      if (Lt(local.Null1.Date, import.InterstateContactAddress.StartDate))
      {
        local.InterstateContactAddress.StartDate =
          import.InterstateContactAddress.StartDate;
      }
      else
      {
        local.InterstateContactAddress.StartDate = Now().Date;
      }

      if (!ReadInterstateContact())
      {
        if (ReadInterstateRequest())
        {
          ExitState = "INTERSTATE_CONTACT_NF";
        }
        else
        {
          ExitState = "INTERSTATE_REQUEST_NF";
        }

        return;
      }

      try
      {
        CreateInterstateContactAddress();
        export.InterstateContactAddress.StartDate =
          local.InterstateContactAddress.StartDate;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_CONTACT_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveInterstateContactAddress(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiCabDeleteIsContactAddress()
  {
    var useImport = new SiCabDeleteIsContactAddress.Import();
    var useExport = new SiCabDeleteIsContactAddress.Export();

    useImport.InterstateContactAddress.StartDate =
      entities.InterstateContactAddress.StartDate;
    useImport.InterstateContact.StartDate = import.InterstateContact.StartDate;
    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;

    Call(SiCabDeleteIsContactAddress.Execute, useImport, useExport);
  }

  private void UseSiCabUpdateIsContactAddress()
  {
    var useImport = new SiCabUpdateIsContactAddress.Import();
    var useExport = new SiCabUpdateIsContactAddress.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;
    useImport.InterstateContact.StartDate = import.InterstateContact.StartDate;
    useImport.InterstateContactAddress.Assign(local.InterstateContactAddress);

    Call(SiCabUpdateIsContactAddress.Execute, useImport, useExport);
  }

  private void CreateInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var icoContStartDt = entities.InterstateContact.StartDate;
    var intGeneratedId = entities.InterstateContact.IntGeneratedId;
    var startDate = local.InterstateContactAddress.StartDate;
    var createdBy = local.InterstateContactAddress.CreatedBy;
    var createdTimestamp = local.InterstateContactAddress.CreatedTimestamp;
    var lastUpdatedBy = local.InterstateContactAddress.LastUpdatedBy;
    var lastUpdatedTimestamp =
      local.InterstateContactAddress.LastUpdatedTimestamp;
    var type1 = local.InterstateContactAddress.Type1 ?? "";
    var street1 = local.InterstateContactAddress.Street1 ?? "";
    var street2 = local.InterstateContactAddress.Street2 ?? "";
    var city = local.InterstateContactAddress.City ?? "";
    var endDate = local.InterstateContactAddress.EndDate;
    var county = local.InterstateContactAddress.County ?? "";
    var state = local.InterstateContactAddress.State ?? "";
    var zipCode = local.InterstateContactAddress.ZipCode ?? "";
    var zip4 = local.InterstateContactAddress.Zip4 ?? "";
    var zip3 = local.InterstateContactAddress.Zip3 ?? "";
    var street3 = local.InterstateContactAddress.Street3 ?? "";
    var street4 = local.InterstateContactAddress.Street4 ?? "";
    var province = local.InterstateContactAddress.Province ?? "";
    var postalCode = local.InterstateContactAddress.PostalCode ?? "";
    var country = local.InterstateContactAddress.Country ?? "";
    var locationType = local.InterstateContactAddress.LocationType;

    CheckValid<InterstateContactAddress>("LocationType", locationType);
    entities.InterstateContactAddress.Populated = false;
    Update("CreateInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(command, "icoContStartDt", icoContStartDt);
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", startDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
      });

    entities.InterstateContactAddress.IcoContStartDt = icoContStartDt;
    entities.InterstateContactAddress.IntGeneratedId = intGeneratedId;
    entities.InterstateContactAddress.StartDate = startDate;
    entities.InterstateContactAddress.CreatedBy = createdBy;
    entities.InterstateContactAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateContactAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateContactAddress.Type1 = type1;
    entities.InterstateContactAddress.Street1 = street1;
    entities.InterstateContactAddress.Street2 = street2;
    entities.InterstateContactAddress.City = city;
    entities.InterstateContactAddress.EndDate = endDate;
    entities.InterstateContactAddress.County = county;
    entities.InterstateContactAddress.State = state;
    entities.InterstateContactAddress.ZipCode = zipCode;
    entities.InterstateContactAddress.Zip4 = zip4;
    entities.InterstateContactAddress.Zip3 = zip3;
    entities.InterstateContactAddress.Street3 = street3;
    entities.InterstateContactAddress.Street4 = street4;
    entities.InterstateContactAddress.Province = province;
    entities.InterstateContactAddress.PostalCode = postalCode;
    entities.InterstateContactAddress.Country = country;
    entities.InterstateContactAddress.LocationType = locationType;
    entities.InterstateContactAddress.Populated = true;
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetDate(
          command, "startDate",
          import.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateContactAddress()
  {
    entities.InterstateContactAddress.Populated = false;

    return ReadEach("ReadInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          import.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.CreatedBy = db.GetString(reader, 3);
        entities.InterstateContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateContactAddress.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.InterstateContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 7);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateContactAddress.County =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 17);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 18);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 19);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 20);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 21);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 22);
        entities.InterstateContactAddress.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private InterstateContactAddress interstateContactAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private DateWorkArea max;
    private DateWorkArea null1;
    private InterstateContactAddress interstateContactAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
  }
#endregion
}
