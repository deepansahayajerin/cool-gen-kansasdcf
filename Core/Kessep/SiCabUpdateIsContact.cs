// Program: SI_CAB_UPDATE_IS_CONTACT, ID: 373465713, model: 746.
// Short name: SWE02797
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_UPDATE_IS_CONTACT.
/// </summary>
[Serializable]
public partial class SiCabUpdateIsContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_UPDATE_IS_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabUpdateIsContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabUpdateIsContact.
  /// </summary>
  public SiCabUpdateIsContact(IContext context, Import import, Export export):
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
    local.InterstateContact.Assign(import.InterstateContact);

    if (Lt(local.Null1.Timestamp, import.InterstateContact.LastUpdatedTimestamp))
      
    {
      local.InterstateContact.LastUpdatedTimestamp =
        import.InterstateContact.LastUpdatedTimestamp;
    }
    else
    {
      local.InterstateContact.LastUpdatedTimestamp = Now();
    }

    if (!IsEmpty(import.InterstateContact.LastUpdatedBy))
    {
      local.InterstateContact.LastUpdatedBy =
        import.InterstateContact.LastUpdatedBy ?? "";
    }
    else
    {
      local.InterstateContact.LastUpdatedBy = global.UserId;
    }

    if (Lt(local.Null1.Date, import.InterstateContact.EndDate) && Lt
      (import.InterstateContact.EndDate, local.Max.Date))
    {
      local.InterstateContact.EndDate = import.InterstateContact.EndDate;
    }
    else
    {
      local.InterstateContact.EndDate = local.Max.Date;
    }

    if (import.InterstateContact.AreaCode.GetValueOrDefault() == 0 && import
      .InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
    {
      if (!IsEmpty(import.InterstateContact.ContactPhoneExtension))
      {
        ExitState = "INTERSTATE_CONTACT_PV";

        return;
      }
    }
    else
    {
      if (import.InterstateContact.AreaCode.GetValueOrDefault() == 0)
      {
        ExitState = "INTERSTATE_CONTACT_PV";

        return;
      }

      if (import.InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
      {
        ExitState = "INTERSTATE_CONTACT_PV";

        return;
      }
    }

    if (import.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == 0
      && import.InterstateContact.ContactFaxNumber.GetValueOrDefault() == 0)
    {
    }
    else
    {
      if (import.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == 0)
      {
        ExitState = "INTERSTATE_CONTACT_PV";

        return;
      }

      if (import.InterstateContact.ContactFaxNumber.GetValueOrDefault() == 0)
      {
        ExitState = "INTERSTATE_CONTACT_PV";

        return;
      }
    }

    if (IsEmpty(import.InterstateContact.NameFirst) && IsEmpty
      (import.InterstateContact.NameLast))
    {
      local.InterstateContact.NameMiddle = "";
      local.InterstateContact.ContactNameSuffix = "";
    }

    local.InterstateContact.StartDate = local.Null1.Date;

    foreach(var item in ReadInterstateContact())
    {
      if (Equal(local.InterstateContact.StartDate, local.Null1.Date))
      {
        local.InterstateContact.StartDate =
          entities.InterstateContact.StartDate;

        try
        {
          UpdateInterstateContact();
          export.InterstateContact.StartDate =
            local.InterstateContact.StartDate;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INTERSTATE_CONTACT_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_CONTACT_PV";

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
        UseSiCabDeleteIsContact();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    if (Equal(local.InterstateContact.StartDate, local.Null1.Date))
    {
      if (ReadInterstateRequest())
      {
        ExitState = "INTERSTATE_CONTACT_NF";
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

  private void UseSiCabDeleteIsContact()
  {
    var useImport = new SiCabDeleteIsContact.Import();
    var useExport = new SiCabDeleteIsContact.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;

    Call(SiCabDeleteIsContact.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return ReadEach("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 4);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 5);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 6);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 7);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 8);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 11);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 12);
        entities.InterstateContact.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.InterstateContact.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.InterstateContact.Populated = true;

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

  private void UpdateInterstateContact()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var contactPhoneNum =
      local.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = local.InterstateContact.EndDate;
    var nameLast = local.InterstateContact.NameLast ?? "";
    var nameFirst = local.InterstateContact.NameFirst ?? "";
    var nameMiddle = local.InterstateContact.NameMiddle ?? "";
    var contactNameSuffix = local.InterstateContact.ContactNameSuffix ?? "";
    var areaCode = local.InterstateContact.AreaCode.GetValueOrDefault();
    var contactPhoneExtension =
      local.InterstateContact.ContactPhoneExtension ?? "";
    var contactFaxNumber =
      local.InterstateContact.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      local.InterstateContact.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      local.InterstateContact.ContactInternetAddress ?? "";
    var lastUpdatedBy = local.InterstateContact.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.InterstateContact.LastUpdatedTimestamp;

    entities.InterstateContact.Populated = false;
    Update("UpdateInterstateContact",
      (db, command) =>
      {
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "contFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "contInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });

    entities.InterstateContact.ContactPhoneNum = contactPhoneNum;
    entities.InterstateContact.EndDate = endDate;
    entities.InterstateContact.NameLast = nameLast;
    entities.InterstateContact.NameFirst = nameFirst;
    entities.InterstateContact.NameMiddle = nameMiddle;
    entities.InterstateContact.ContactNameSuffix = contactNameSuffix;
    entities.InterstateContact.AreaCode = areaCode;
    entities.InterstateContact.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateContact.ContactFaxNumber = contactFaxNumber;
    entities.InterstateContact.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateContact.ContactInternetAddress = contactInternetAddress;
    entities.InterstateContact.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateContact.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateContact.Populated = true;
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

    private InterstateRequest interstateRequest;
    private InterstateContact interstateContact;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    private InterstateContact interstateContact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private DateWorkArea null1;
    private DateWorkArea max;
    private InterstateContact interstateContact;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
  }
#endregion
}
