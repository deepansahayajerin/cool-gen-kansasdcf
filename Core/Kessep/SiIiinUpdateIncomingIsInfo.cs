// Program: SI_IIIN_UPDATE_INCOMING_IS_INFO, ID: 372500274, model: 746.
// Short name: SWE02450
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IIIN_UPDATE_INCOMING_IS_INFO.
/// </summary>
[Serializable]
public partial class SiIiinUpdateIncomingIsInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIIN_UPDATE_INCOMING_IS_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIiinUpdateIncomingIsInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIiinUpdateIncomingIsInfo.
  /// </summary>
  public SiIiinUpdateIncomingIsInfo(IContext context, Import import,
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
    // -----------------------------------------------------------
    // 03/08/99    Scroggins/Deghand         Initial Development
    // -----------------------------------------------------------
    local.Current.Date = Now().Date;
    export.InterstateContact.Assign(import.InterstateContact);
    export.InterstateRequest.Assign(import.InterstateRequest);

    if (ReadInterstateContact())
    {
      try
      {
        UpdateInterstateContact();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTAT_CONTACT_ADD_ERR";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTAT_CONTACT_ADD_ERR";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "SI0000_INTERSTAT_CONTACT_ADD_ERR";
    }
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
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
        entities.InterstateContact.Populated = true;
      });
  }

  private void UpdateInterstateContact()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var contactPhoneNum =
      export.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = export.InterstateContact.EndDate;
    var createdBy = export.InterstateContact.CreatedBy;
    var createdTstamp = export.InterstateContact.CreatedTstamp;
    var nameLast = export.InterstateContact.NameLast ?? "";
    var nameFirst = export.InterstateContact.NameFirst ?? "";
    var nameMiddle = export.InterstateContact.NameMiddle ?? "";
    var contactNameSuffix = export.InterstateContact.ContactNameSuffix ?? "";
    var areaCode = export.InterstateContact.AreaCode.GetValueOrDefault();
    var contactPhoneExtension =
      export.InterstateContact.ContactPhoneExtension ?? "";
    var contactFaxNumber =
      export.InterstateContact.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      export.InterstateContact.ContactInternetAddress ?? "";

    entities.InterstateContact.Populated = false;
    Update("UpdateInterstateContact",
      (db, command) =>
      {
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
          
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });

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

    private DateWorkArea current;
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
