// Program: SI_CAB_CREATE_IS_CONTACT, ID: 373465508, model: 746.
// Short name: SWE02792
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_CREATE_IS_CONTACT.
/// </summary>
[Serializable]
public partial class SiCabCreateIsContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_CREATE_IS_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabCreateIsContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabCreateIsContact.
  /// </summary>
  public SiCabCreateIsContact(IContext context, Import import, Export export):
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
    MoveInterstateContact(import.InterstateContact, local.InterstateContact);

    if (Lt(local.Null1.Timestamp, import.InterstateContact.CreatedTstamp))
    {
      local.InterstateContact.CreatedTstamp =
        import.InterstateContact.CreatedTstamp;
    }
    else
    {
      local.InterstateContact.CreatedTstamp = Now();
    }

    if (!IsEmpty(import.InterstateContact.CreatedBy))
    {
      local.InterstateContact.CreatedBy = import.InterstateContact.CreatedBy;
    }
    else
    {
      local.InterstateContact.CreatedBy = global.UserId;
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

    local.InterstateContact.LastUpdatedTimestamp =
      local.InterstateContact.CreatedTstamp;
    local.InterstateContact.LastUpdatedBy = local.InterstateContact.CreatedBy;

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
        UseSiCabUpdateIsContact();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.InterstateContact.StartDate = local.InterstateContact.StartDate;
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
      if (Lt(local.Null1.Date, import.InterstateContact.StartDate))
      {
        local.InterstateContact.StartDate = import.InterstateContact.StartDate;
      }
      else
      {
        local.InterstateContact.StartDate = Now().Date;
      }

      if (!ReadInterstateRequest())
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }

      try
      {
        CreateInterstateContact();
        export.InterstateContact.StartDate = local.InterstateContact.StartDate;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_CONTACT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_CONTACT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveInterstateContact(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTstamp = source.CreatedTstamp;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
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

  private void UseSiCabUpdateIsContact()
  {
    var useImport = new SiCabUpdateIsContact.Import();
    var useExport = new SiCabUpdateIsContact.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;
    useImport.InterstateContact.Assign(local.InterstateContact);

    Call(SiCabUpdateIsContact.Execute, useImport, useExport);
  }

  private void CreateInterstateContact()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var startDate = local.InterstateContact.StartDate;
    var contactPhoneNum =
      local.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = local.InterstateContact.EndDate;
    var createdBy = local.InterstateContact.CreatedBy;
    var createdTstamp = local.InterstateContact.CreatedTstamp;
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
    Update("CreateInterstateContact",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", startDate);
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
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
          
      });

    entities.InterstateContact.IntGeneratedId = intGeneratedId;
    entities.InterstateContact.StartDate = startDate;
    entities.InterstateContact.ContactPhoneNum = contactPhoneNum;
    entities.InterstateContact.EndDate = endDate;
    entities.InterstateContact.CreatedBy = createdBy;
    entities.InterstateContact.CreatedTstamp = createdTstamp;
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
        entities.InterstateContact.CreatedBy = db.GetString(reader, 4);
        entities.InterstateContact.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 6);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 7);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 12);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 14);
        entities.InterstateContact.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.InterstateContact.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
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
