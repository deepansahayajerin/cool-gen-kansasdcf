// Program: SI_REGI_CREATE_IC_REQ_CONTACT, ID: 373468014, model: 746.
// Short name: SWE02105
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_REGI_CREATE_IC_REQ_CONTACT.
/// </summary>
[Serializable]
public partial class SiRegiCreateIcReqContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CREATE_IC_REQ_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCreateIcReqContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCreateIcReqContact.
  /// </summary>
  public SiRegiCreateIcReqContact(IContext context, Import import, Export export)
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
    // ************************************************************
    // 03/23/98	Siraj Konkader		ZDEL cleanup
    // ************************************************************
    // -----------------------------------------
    // 03/19/99 W.Campbell     Added set statements
    //                         for mandatory attributes in the
    //                         create statement.  Also added
    //                         a local timestamp view and
    //                         modified statements to use it.
    //                         Also added PV and AE exit states.
    // -----------------------------------------
    // 06/23/99  M. Lachowicz  Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 09/02/99  C. Scroggins  Added code to set the contact's area
    //                         code on create.
    // ------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseOeCabSetMnemonics();

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadInterstateRequest())
    {
      if (ReadInterstateContact())
      {
        DeleteInterstateContact();
      }

      try
      {
        CreateInterstateContact();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        export.InterstateContact.Assign(entities.InterstateContact);
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

      UseSiCreateIcIsReqContactAddr();
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransactionDate = source.TransactionDate;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
  }

  private static void MoveInterstateContact(InterstateContact source,
    InterstateContact target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseSiCreateIcIsReqContactAddr()
  {
    var useImport = new SiCreateIcIsReqContactAddr.Import();
    var useExport = new SiCreateIcIsReqContactAddr.Export();

    useImport.Persistent.Assign(entities.InterstateRequest);
    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);
    MoveInterstateContact(export.InterstateContact, useImport.InterstateContact);
      

    Call(SiCreateIcIsReqContactAddr.Execute, useImport, useExport);

    export.InterstateContactAddress.Assign(useExport.InterstateContactAddress);
  }

  private void CreateInterstateContact()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var startDate = import.InterstateCase.TransactionDate;
    var contactPhoneNum =
      import.InterstateCase.ContactPhoneNum.GetValueOrDefault();
    var endDate = local.MaxDate.ExpirationDate;
    var createdBy = global.UserId;
    var createdTstamp = local.Current.Timestamp;
    var nameLast = import.InterstateCase.ContactNameLast ?? "";
    var nameFirst = import.InterstateCase.ContactNameFirst ?? "";
    var nameMiddle = import.InterstateCase.ContactNameMiddle ?? "";
    var contactNameSuffix = import.InterstateCase.ContactNameSuffix ?? "";
    var areaCode = import.InterstateCase.ContactAreaCode.GetValueOrDefault();
    var contactPhoneExtension = import.InterstateCase.ContactPhoneExtension ?? ""
      ;
    var contactFaxNumber =
      import.InterstateCase.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      import.InterstateCase.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      import.InterstateCase.ContactInternetAddress ?? "";

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
          
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTimes", createdTstamp);
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
    entities.InterstateContact.LastUpdatedBy = createdBy;
    entities.InterstateContact.LastUpdatedTimestamp = createdTstamp;
    entities.InterstateContact.Populated = true;
  }

  private void DeleteInterstateContact()
  {
    Update("DeleteInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    private InterstateCase interstateCase;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private InterstateContact interstateContact;
    private InterstateContactAddress interstateContactAddress;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea current;
    private Code maxDate;
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

    private InterstateContactAddress interstateContactAddress;
    private InterstateRequest interstateRequest;
    private InterstateContact interstateContact;
  }
#endregion
}
