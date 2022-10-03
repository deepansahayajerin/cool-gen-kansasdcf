// Program: OE_PCON_CREATE_CONTACT_DETAILS, ID: 371853941, model: 746.
// Short name: SWE00950
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
/// A program: OE_PCON_CREATE_CONTACT_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates Contact details.
/// It creates CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL records.
/// </para>
/// </summary>
[Serializable]
public partial class OePconCreateContactDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_CREATE_CONTACT_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconCreateContactDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconCreateContactDetails.
  /// </summary>
  public OePconCreateContactDetails(IContext context, Import import,
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
    // ---------------------------------------------
    // Statements to set User ID and Time stamps for audit purposes to be fixed.
    // ---------------------------------------------
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block creates Contact details
    // PROCESSING:
    // This action block is passed with details of CSE_PERSON, CONTACT, 
    // repeating group of CONTACT_DETAIL and CONTACT_ADDRESS. It creates
    // CONTACT, CONTACT_DETAIL and CONTACT_ADDRESS records.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			C R - -
    // 	CONTACT_ADDRESS		C - - -
    // 	CONTACT_DETAIL		C R - -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/26/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGREQ#	DESCRIPTION
    // govind	01/26/95	Initial coding
    // Lofton  02/22/96	Add check for like
    // 			contact details.
    // *********************************************
    export.CsePerson.Number = import.CsePerson.Number;
    MoveContact(import.Contact, export.Contact);
    export.ContactCreated.Flag = "N";
    export.ContactAddress.Assign(import.ContactAddress);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailAction.ActionEntry =
        import.Import1.Item.DetailAction.ActionEntry;
      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Next();
    }

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadContact2())
    {
      if (!IsEmpty(export.Contact.CompanyName))
      {
        if (Equal(export.Contact.CompanyName,
          entities.ExistingContact.CompanyName))
        {
          export.Contact.ContactNumber = entities.ExistingContact.ContactNumber;
          ExitState = "OE0140_CANNOT_ADD_CONTACT_AE";

          return;
        }
      }
      else if (Equal(export.Contact.NameLast, entities.ExistingContact.NameLast) &&
        Equal(export.Contact.NameFirst, entities.ExistingContact.NameFirst) && AsChar
        (export.Contact.MiddleInitial) == AsChar
        (entities.ExistingContact.MiddleInitial))
      {
        export.Contact.ContactNumber = entities.ExistingContact.ContactNumber;
        ExitState = "OE0140_CANNOT_ADD_CONTACT_AE";

        return;
      }
    }

    local.Last.ContactNumber = 0;

    if (ReadContact1())
    {
      local.Last.ContactNumber = entities.ExistingLast.ContactNumber;
    }

    try
    {
      CreateContact();
      export.Contact.Assign(entities.NewContact);
      export.LastUpdatedStamps.LastUpdatedBy = global.UserId;
      export.LastUpdatedStamps.LastUpdatedTimestamp =
        entities.NewContact.LastUpdatedTimestamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CONTACT_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ---------------------------------------------
    // Create CONTACT_ADDRESS only if one or more address field is supplied.
    // ---------------------------------------------
    if (!IsEmpty(import.ContactAddress.City) || !
      IsEmpty(import.ContactAddress.Country) || !
      IsEmpty(import.ContactAddress.PostalCode) || !
      IsEmpty(import.ContactAddress.Province) || !
      IsEmpty(import.ContactAddress.State) || !
      IsEmpty(import.ContactAddress.Street1) || !
      IsEmpty(import.ContactAddress.Street2) || !
      IsEmpty(import.ContactAddress.Zip3) || !
      IsEmpty(import.ContactAddress.ZipCode4) || !
      IsEmpty(import.ContactAddress.ZipCode5))
    {
      try
      {
        CreateContactAddress();
        export.ContactAddress.Assign(entities.NewContactAddress);
        export.LastUpdatedStamps.LastUpdatedBy = global.UserId;
        export.LastUpdatedStamps.LastUpdatedTimestamp = Now();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CONTACT_ADDRESS_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Temp.Identifier = 0;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      if (IsEmpty(import.Import1.Item.Detail.Note))
      {
        export.Export1.Next();

        continue;
      }

      ++local.Temp.Identifier;

      try
      {
        CreateContactDetail();
        export.Export1.Update.Detail.Assign(entities.NewContactDetail);
        export.LastUpdatedStamps.LastUpdatedBy = global.UserId;
        export.LastUpdatedStamps.LastUpdatedTimestamp = Now();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CONTACT_DETAIL_AE";
            export.Export1.Next();

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      export.Export1.Next();
    }
  }

  private static void MoveContact(Contact source, Contact target)
  {
    target.VerifiedDate = source.VerifiedDate;
    target.VerifiedUserId = source.VerifiedUserId;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.FaxAreaCode = source.FaxAreaCode;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.FaxExt = source.FaxExt;
    target.Fax = source.Fax;
    target.ContactNumber = source.ContactNumber;
    target.NameTitle = source.NameTitle;
    target.CompanyName = source.CompanyName;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
    target.HomePhone = source.HomePhone;
    target.WorkPhone = source.WorkPhone;
  }

  private void CreateContact()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var contactNumber = local.Last.ContactNumber + 1;
    var fax = import.Contact.Fax.GetValueOrDefault();
    var nameTitle = import.Contact.NameTitle ?? "";
    var companyName = import.Contact.CompanyName ?? "";
    var relationshipToCsePerson = import.Contact.RelationshipToCsePerson ?? "";
    var nameLast = import.Contact.NameLast ?? "";
    var nameFirst = import.Contact.NameFirst ?? "";
    var middleInitial = import.Contact.MiddleInitial ?? "";
    var homePhone = import.Contact.HomePhone.GetValueOrDefault();
    var workPhone = import.Contact.WorkPhone.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var workPhoneExt = import.Contact.WorkPhoneExt ?? "";
    var faxExt = import.Contact.FaxExt ?? "";
    var workPhoneAreaCode =
      import.Contact.WorkPhoneAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      import.Contact.HomePhoneAreaCode.GetValueOrDefault();
    var faxAreaCode = import.Contact.FaxAreaCode.GetValueOrDefault();
    var verifiedDate = import.Contact.VerifiedDate;
    var verifiedUserId = import.Contact.VerifiedUserId ?? "";

    entities.NewContact.Populated = false;
    Update("CreateContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "contactNumber", contactNumber);
        db.SetNullableInt32(command, "fax", fax);
        db.SetNullableString(command, "nameTitle", nameTitle);
        db.SetNullableString(command, "companyName", companyName);
        db.
          SetNullableString(command, "relToCsePerson", relationshipToCsePerson);
          
        db.SetNullableString(command, "lastName", nameLast);
        db.SetNullableString(command, "firstName", nameFirst);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "faxExt", faxExt);
        db.SetNullableInt32(command, "workPhoneArea", workPhoneAreaCode);
        db.SetNullableInt32(command, "homePhoneArea", homePhoneAreaCode);
        db.SetNullableInt32(command, "faxArea", faxAreaCode);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
      });

    entities.NewContact.CspNumber = cspNumber;
    entities.NewContact.ContactNumber = contactNumber;
    entities.NewContact.Fax = fax;
    entities.NewContact.NameTitle = nameTitle;
    entities.NewContact.CompanyName = companyName;
    entities.NewContact.RelationshipToCsePerson = relationshipToCsePerson;
    entities.NewContact.NameLast = nameLast;
    entities.NewContact.NameFirst = nameFirst;
    entities.NewContact.MiddleInitial = middleInitial;
    entities.NewContact.HomePhone = homePhone;
    entities.NewContact.WorkPhone = workPhone;
    entities.NewContact.CreatedBy = createdBy;
    entities.NewContact.CreatedTimestamp = createdTimestamp;
    entities.NewContact.LastUpdatedBy = "";
    entities.NewContact.LastUpdatedTimestamp = createdTimestamp;
    entities.NewContact.WorkPhoneExt = workPhoneExt;
    entities.NewContact.FaxExt = faxExt;
    entities.NewContact.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.NewContact.HomePhoneAreaCode = homePhoneAreaCode;
    entities.NewContact.FaxAreaCode = faxAreaCode;
    entities.NewContact.VerifiedDate = verifiedDate;
    entities.NewContact.VerifiedUserId = verifiedUserId;
    entities.NewContact.Populated = true;
  }

  private void CreateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.NewContact.Populated);

    var conNumber = entities.NewContact.ContactNumber;
    var cspNumber = entities.NewContact.CspNumber;
    var effectiveDate = Now().Date;
    var street1 = import.ContactAddress.Street1 ?? "";
    var street2 = import.ContactAddress.Street2 ?? "";
    var city = import.ContactAddress.City ?? "";
    var state = import.ContactAddress.State ?? "";
    var zipCode5 = import.ContactAddress.ZipCode5 ?? "";
    var zipCode4 = import.ContactAddress.ZipCode4 ?? "";
    var zip3 = import.ContactAddress.Zip3 ?? "";
    var addressType = import.ContactAddress.AddressType ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.ZeroInitialised.LastUpdatedTimestamp;

    entities.NewContactAddress.Populated = false;
    Update("CreateContactAddress",
      (db, command) =>
      {
        db.SetInt32(command, "conNumber", conNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", "");
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
      });

    entities.NewContactAddress.ConNumber = conNumber;
    entities.NewContactAddress.CspNumber = cspNumber;
    entities.NewContactAddress.EffectiveDate = effectiveDate;
    entities.NewContactAddress.Street1 = street1;
    entities.NewContactAddress.Street2 = street2;
    entities.NewContactAddress.City = city;
    entities.NewContactAddress.State = state;
    entities.NewContactAddress.Province = "";
    entities.NewContactAddress.PostalCode = "";
    entities.NewContactAddress.ZipCode5 = zipCode5;
    entities.NewContactAddress.ZipCode4 = zipCode4;
    entities.NewContactAddress.Zip3 = zip3;
    entities.NewContactAddress.Country = "";
    entities.NewContactAddress.AddressType = addressType;
    entities.NewContactAddress.CreatedBy = createdBy;
    entities.NewContactAddress.CreatedTimestamp = createdTimestamp;
    entities.NewContactAddress.LastUpdatedBy = "";
    entities.NewContactAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewContactAddress.Populated = true;
  }

  private void CreateContactDetail()
  {
    System.Diagnostics.Debug.Assert(entities.NewContact.Populated);

    var conNumber = entities.NewContact.ContactNumber;
    var cspNumber = entities.NewContact.CspNumber;
    var identifier = local.Temp.Identifier;
    var contactTime =
      import.Import1.Item.Detail.ContactTime.GetValueOrDefault();
    var contactDate = import.Import1.Item.Detail.ContactDate;
    var contactedUserid = import.Import1.Item.Detail.ContactedUserid ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.ZeroInitialised.LastUpdatedTimestamp;
    var note = import.Import1.Item.Detail.Note ?? "";

    entities.NewContactDetail.Populated = false;
    Update("CreateContactDetail",
      (db, command) =>
      {
        db.SetInt32(command, "conNumber", conNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableTimeSpan(command, "contactTime", contactTime);
        db.SetNullableDate(command, "contactDate", contactDate);
        db.SetNullableString(command, "contactedUserid", contactedUserid);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "note", note);
      });

    entities.NewContactDetail.ConNumber = conNumber;
    entities.NewContactDetail.CspNumber = cspNumber;
    entities.NewContactDetail.Identifier = identifier;
    entities.NewContactDetail.ContactTime = contactTime;
    entities.NewContactDetail.ContactDate = contactDate;
    entities.NewContactDetail.ContactedUserid = contactedUserid;
    entities.NewContactDetail.CreatedBy = createdBy;
    entities.NewContactDetail.CreatedTimestamp = createdTimestamp;
    entities.NewContactDetail.LastUpdatedBy = "";
    entities.NewContactDetail.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewContactDetail.Note = note;
    entities.NewContactDetail.Populated = true;
  }

  private bool ReadContact1()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspNumber = db.GetString(reader, 0);
        entities.ExistingLast.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContact2()
  {
    entities.ExistingContact.Populated = false;

    return ReadEach("ReadContact2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 2);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 3);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 4);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ContactDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailAction;
      private ContactDetail detail;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private CsePerson csePerson;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ImportGroup> import1;
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
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ContactDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailAction;
      private ContactDetail detail;
    }

    /// <summary>
    /// A value of ContactCreated.
    /// </summary>
    [JsonPropertyName("contactCreated")]
    public Common ContactCreated
    {
      get => contactCreated ??= new();
      set => contactCreated = value;
    }

    /// <summary>
    /// A value of LastUpdatedStamps.
    /// </summary>
    [JsonPropertyName("lastUpdatedStamps")]
    public Contact LastUpdatedStamps
    {
      get => lastUpdatedStamps ??= new();
      set => lastUpdatedStamps = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
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

    private Common contactCreated;
    private Contact lastUpdatedStamps;
    private CsePerson csePerson;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroInitialised.
    /// </summary>
    [JsonPropertyName("zeroInitialised")]
    public Contact ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Contact Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public ContactDetail Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private Contact zeroInitialised;
    private Contact last;
    private ContactDetail temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingContact.
    /// </summary>
    [JsonPropertyName("existingContact")]
    public Contact ExistingContact
    {
      get => existingContact ??= new();
      set => existingContact = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Contact ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of NewContact.
    /// </summary>
    [JsonPropertyName("newContact")]
    public Contact NewContact
    {
      get => newContact ??= new();
      set => newContact = value;
    }

    /// <summary>
    /// A value of NewContactAddress.
    /// </summary>
    [JsonPropertyName("newContactAddress")]
    public ContactAddress NewContactAddress
    {
      get => newContactAddress ??= new();
      set => newContactAddress = value;
    }

    /// <summary>
    /// A value of NewContactDetail.
    /// </summary>
    [JsonPropertyName("newContactDetail")]
    public ContactDetail NewContactDetail
    {
      get => newContactDetail ??= new();
      set => newContactDetail = value;
    }

    private Contact existingContact;
    private Contact existingLast;
    private CsePerson existingCsePerson;
    private Contact newContact;
    private ContactAddress newContactAddress;
    private ContactDetail newContactDetail;
  }
#endregion
}
