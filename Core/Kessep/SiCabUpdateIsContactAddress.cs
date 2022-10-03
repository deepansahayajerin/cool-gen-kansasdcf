// Program: SI_CAB_UPDATE_IS_CONTACT_ADDRESS, ID: 373465738, model: 746.
// Short name: SWE02798
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_UPDATE_IS_CONTACT_ADDRESS.
/// </summary>
[Serializable]
public partial class SiCabUpdateIsContactAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_UPDATE_IS_CONTACT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabUpdateIsContactAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabUpdateIsContactAddress.
  /// </summary>
  public SiCabUpdateIsContactAddress(IContext context, Import import,
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
    local.InterstateContactAddress.Assign(import.InterstateContactAddress);

    if (Lt(local.Null1.Timestamp,
      import.InterstateContactAddress.LastUpdatedTimestamp))
    {
      local.InterstateContactAddress.LastUpdatedTimestamp =
        import.InterstateContactAddress.LastUpdatedTimestamp;
    }
    else
    {
      local.InterstateContactAddress.LastUpdatedTimestamp = Now();
    }

    if (!IsEmpty(import.InterstateContactAddress.LastUpdatedBy))
    {
      local.InterstateContactAddress.LastUpdatedBy =
        import.InterstateContactAddress.LastUpdatedBy;
    }
    else
    {
      local.InterstateContactAddress.LastUpdatedBy = global.UserId;
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

        try
        {
          UpdateInterstateContactAddress();
          export.InterstateContactAddress.StartDate =
            local.InterstateContactAddress.StartDate;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_CONTACT_ADDRESS_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
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
      if (ReadInterstateRequest())
      {
        if (ReadInterstateContact())
        {
          ExitState = "INTERSTATE_CONTACT_NF";
        }
        else
        {
          ExitState = "INTERSTATE_CONTACT_ADDRESS_NF";
        }
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";
      }
    }
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
    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;
    useImport.InterstateContact.StartDate = import.InterstateContact.StartDate;

    Call(SiCabDeleteIsContactAddress.Execute, useImport, useExport);
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "startDate",
          import.InterstateContact.StartDate.GetValueOrDefault());
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
        entities.InterstateContactAddress.LastUpdatedBy =
          db.GetString(reader, 3);
        entities.InterstateContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 5);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 6);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateContactAddress.County =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 11);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.Zip3 =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 16);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 17);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 18);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 19);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 20);
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

  private void UpdateInterstateContactAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateContactAddress.Populated);

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
    Update("UpdateInterstateContactAddress",
      (db, command) =>
      {
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
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContactAddress.IcoContStartDt.GetValueOrDefault());
          
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateContactAddress.IntGeneratedId);
        db.SetDate(
          command, "startDate",
          entities.InterstateContactAddress.StartDate.GetValueOrDefault());
      });

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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private InterstateRequest interstateRequest;
    private InterstateContact interstateContact;
    private InterstateContactAddress interstateContactAddress;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

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

    private InterstateContactAddress interstateContactAddress;
    private DateWorkArea max;
    private DateWorkArea null1;
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
