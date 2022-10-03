// Program: OE_PCON_UPDATE_CONTACT_DETAILS, ID: 371885532, model: 746.
// Short name: SWE00953
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
/// A program: OE_PCON_UPDATE_CONTACT_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates Contact details.
/// It creates CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL records.
/// </para>
/// </summary>
[Serializable]
public partial class OePconUpdateContactDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_UPDATE_CONTACT_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconUpdateContactDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconUpdateContactDetails.
  /// </summary>
  public OePconUpdateContactDetails(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block updates contact details
    // PROCESSING:
    // It is passed with CSE_PERSON, CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL
    // details. The individual CONTACT_DETAIL entries may be added or updated.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R U -
    // 	CONTACT_ADDRESS		C R U -
    // 	CONTACT_DETAIL		C R U -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/26/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGREQ	DESCRIPTION
    // govind	01/26/95	Initial coding
    // *********************************************
    export.CsePerson.Number = import.CsePerson.Number;
    MoveContact1(import.Contact, export.Contact);
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

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Next();
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadContact())
    {
      MoveContact2(entities.ExistingContact, export.UpdatedStamp);
    }
    else
    {
      ExitState = "CONTACT_NF";

      return;
    }

    if (Lt(new DateTime(1, 1, 1), import.Contact.VerifiedDate) && !
      Equal(import.Contact.VerifiedDate, entities.ExistingContact.VerifiedDate))
    {
      // ---------------------------------------------
      // Changed verified date. Assume that a response has been received
      // ---------------------------------------------
      export.TeResponseReceived.Flag = "Y";
    }

    // ---------------------------------------------
    // Update CONTACT if any data is changed.
    // ---------------------------------------------
    if (!Equal(import.Contact.CompanyName, entities.ExistingContact.CompanyName) ||
      !
      Equal(import.Contact.Fax.GetValueOrDefault(), entities.ExistingContact.Fax)
      || !
      Equal(import.Contact.HomePhone.GetValueOrDefault(),
      entities.ExistingContact.HomePhone) || AsChar
      (import.Contact.MiddleInitial) != AsChar
      (entities.ExistingContact.MiddleInitial) || !
      Equal(import.Contact.NameFirst, entities.ExistingContact.NameFirst) || !
      Equal(import.Contact.NameLast, entities.ExistingContact.NameLast) || !
      Equal(import.Contact.NameTitle, entities.ExistingContact.NameTitle) || !
      Equal(import.Contact.RelationshipToCsePerson,
      entities.ExistingContact.RelationshipToCsePerson) || !
      Equal(import.Contact.WorkPhone.GetValueOrDefault(),
      entities.ExistingContact.WorkPhone) || !
      Equal(import.Contact.WorkPhoneExt, entities.ExistingContact.WorkPhoneExt) ||
      !Equal(import.Contact.FaxExt, entities.ExistingContact.FaxExt) || !
      Equal(import.Contact.HomePhoneAreaCode.GetValueOrDefault(),
      entities.ExistingContact.HomePhoneAreaCode) || !
      Equal(import.Contact.WorkPhoneAreaCode.GetValueOrDefault(),
      entities.ExistingContact.WorkPhoneAreaCode) || !
      Equal(import.Contact.FaxAreaCode.GetValueOrDefault(),
      entities.ExistingContact.FaxAreaCode) || !
      Equal(import.Contact.VerifiedDate, entities.ExistingContact.VerifiedDate) ||
      !
      Equal(import.Contact.VerifiedUserId,
      entities.ExistingContact.VerifiedUserId))
    {
      try
      {
        UpdateContact();
        export.UpdatedStamp.LastUpdatedBy = global.UserId;
        export.UpdatedStamp.LastUpdatedTimestamp = Now();
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
    }

    if (!import.Import1.IsEmpty)
    {
      foreach(var item in ReadContactDetail())
      {
        if (Lt(export.UpdatedStamp.LastUpdatedTimestamp,
          entities.ExistingContactDetail.LastUpdatedTimestamp))
        {
          export.UpdatedStamp.LastUpdatedTimestamp =
            entities.ExistingContactDetail.LastUpdatedTimestamp;
          export.UpdatedStamp.LastUpdatedBy =
            entities.ExistingContactDetail.LastUpdatedBy;
        }

        DeleteContactDetail();
      }

      local.Temp.Identifier = 0;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);

        if (IsEmpty(export.Export1.Item.Detail.Note))
        {
          export.Export1.Next();

          continue;
        }

        ++local.Temp.Identifier;

        try
        {
          CreateContactDetail();
          export.Export1.Update.Detail.Assign(entities.NewContactDetail);
          export.UpdatedStamp.LastUpdatedBy = global.UserId;
          export.UpdatedStamp.LastUpdatedTimestamp = Now();
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

    local.ContactAddressFound.Flag = "N";

    if (ReadContactAddress())
    {
      local.ContactAddressFound.Flag = "Y";

      // Only the latest address is read and processed. So escape out of READ 
      // EACH statement.
      if (Lt(export.UpdatedStamp.LastUpdatedTimestamp,
        entities.ExistingContactAddress.LastUpdatedTimestamp))
      {
        export.UpdatedStamp.LastUpdatedTimestamp =
          entities.ExistingContactAddress.LastUpdatedTimestamp;
        export.UpdatedStamp.LastUpdatedBy =
          entities.ExistingContactAddress.LastUpdatedBy;
      }
    }

    // ---------------------------------------------
    // Perform an update only if one or more attributes have been changed.
    // ---------------------------------------------
    if (IsEmpty(import.ContactAddress.City) && IsEmpty
      (import.ContactAddress.Country) && IsEmpty
      (import.ContactAddress.PostalCode) && IsEmpty
      (import.ContactAddress.Province) && IsEmpty
      (import.ContactAddress.State) && IsEmpty
      (import.ContactAddress.Street1) && IsEmpty
      (import.ContactAddress.Street2) && IsEmpty
      (import.ContactAddress.Zip3) && IsEmpty
      (import.ContactAddress.ZipCode4) && IsEmpty
      (import.ContactAddress.ZipCode5))
    {
      // ---------------------------------------------
      // Address details not supplied. It is assumed that no address change was 
      // required or address details were not be available.So escape.
      // ---------------------------------------------
      return;
    }

    if (AsChar(local.ContactAddressFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // Some address details were provided but there is no existing 
      // CONTACT_ADDRESS. So create a new one.
      // ---------------------------------------------
      try
      {
        CreateContactAddress();
        export.ContactAddress.Assign(entities.NewContactAddress);
        export.UpdatedStamp.LastUpdatedBy = global.UserId;
        export.UpdatedStamp.LastUpdatedTimestamp = Now();
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

      return;
    }

    // ---------------------------------------------
    // Some address details were provided and there is an existing 
    // CONTACT_ADDRESS record. First check if any address details is changed. If
    // not, take no action. Otherwise update existing record.
    // ---------------------------------------------
    if (!Equal(import.ContactAddress.City, entities.ExistingContactAddress.City) ||
      !
      Equal(import.ContactAddress.State, entities.ExistingContactAddress.State) ||
      !
      Equal(import.ContactAddress.Street1,
      entities.ExistingContactAddress.Street1) || !
      Equal(import.ContactAddress.Street2,
      entities.ExistingContactAddress.Street2) || !
      Equal(import.ContactAddress.Zip3, entities.ExistingContactAddress.Zip3) ||
      !
      Equal(import.ContactAddress.ZipCode4,
      entities.ExistingContactAddress.ZipCode4) || !
      Equal(import.ContactAddress.ZipCode5,
      entities.ExistingContactAddress.ZipCode5))
    {
      try
      {
        UpdateContactAddress();
        export.ContactAddress.Assign(entities.ExistingContactAddress);
        export.UpdatedStamp.LastUpdatedBy = global.UserId;
        export.UpdatedStamp.LastUpdatedTimestamp = Now();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CONTACT_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveContact1(Contact source, Contact target)
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

  private static void MoveContact2(Contact source, Contact target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void CreateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);

    var conNumber = entities.ExistingContact.ContactNumber;
    var cspNumber = entities.ExistingContact.CspNumber;
    var effectiveDate = import.ContactAddress.EffectiveDate;
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
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
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
    entities.NewContactAddress.LastUpdatedBy = createdBy;
    entities.NewContactAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.NewContactAddress.Populated = true;
  }

  private void CreateContactDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);

    var conNumber = entities.ExistingContact.ContactNumber;
    var cspNumber = entities.ExistingContact.CspNumber;
    var identifier = local.Temp.Identifier;
    var contactTime = TimeSpan.Zero;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var note = import.Import1.Item.Detail.Note ?? "";

    entities.NewContactDetail.Populated = false;
    Update("CreateContactDetail",
      (db, command) =>
      {
        db.SetInt32(command, "conNumber", conNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableTimeSpan(command, "contactTime", contactTime);
        db.SetNullableDate(command, "contactDate", null);
        db.SetNullableString(command, "contactedUserid", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "note", note);
      });

    entities.NewContactDetail.ConNumber = conNumber;
    entities.NewContactDetail.CspNumber = cspNumber;
    entities.NewContactDetail.Identifier = identifier;
    entities.NewContactDetail.ContactTime = contactTime;
    entities.NewContactDetail.ContactDate = null;
    entities.NewContactDetail.ContactedUserid = "";
    entities.NewContactDetail.CreatedBy = createdBy;
    entities.NewContactDetail.CreatedTimestamp = createdTimestamp;
    entities.NewContactDetail.LastUpdatedBy = createdBy;
    entities.NewContactDetail.LastUpdatedTimestamp = createdTimestamp;
    entities.NewContactDetail.Note = note;
    entities.NewContactDetail.Populated = true;
  }

  private void DeleteContactDetail()
  {
    Update("DeleteContactDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "conNumber", entities.ExistingContactDetail.ConNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingContactDetail.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingContactDetail.Identifier);
      });
  }

  private bool ReadContact()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.WorkPhoneExt =
          db.GetNullableString(reader, 15);
        entities.ExistingContact.FaxExt = db.GetNullableString(reader, 16);
        entities.ExistingContact.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingContact.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingContact.FaxAreaCode = db.GetNullableInt32(reader, 19);
        entities.ExistingContact.VerifiedDate = db.GetNullableDate(reader, 20);
        entities.ExistingContact.VerifiedUserId =
          db.GetNullableString(reader, 21);
        entities.ExistingContact.Populated = true;
      });
  }

  private bool ReadContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactAddress.Populated = false;

    return Read("ReadContactAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingContactAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingContactAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingContactAddress.City = db.GetNullableString(reader, 5);
        entities.ExistingContactAddress.State = db.GetNullableString(reader, 6);
        entities.ExistingContactAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingContactAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingContactAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingContactAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ExistingContactAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingContactAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingContactAddress.CreatedBy = db.GetString(reader, 14);
        entities.ExistingContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingContactAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingContactAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContactDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactDetail.Populated = false;

    return ReadEach("ReadContactDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactDetail.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactDetail.Identifier = db.GetInt32(reader, 2);
        entities.ExistingContactDetail.ContactTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.ExistingContactDetail.ContactDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingContactDetail.ContactedUserid =
          db.GetNullableString(reader, 5);
        entities.ExistingContactDetail.CreatedBy = db.GetString(reader, 6);
        entities.ExistingContactDetail.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingContactDetail.LastUpdatedBy = db.GetString(reader, 8);
        entities.ExistingContactDetail.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingContactDetail.Note = db.GetNullableString(reader, 10);
        entities.ExistingContactDetail.Populated = true;

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

  private void UpdateContact()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);

    var fax = import.Contact.Fax.GetValueOrDefault();
    var nameTitle = import.Contact.NameTitle ?? "";
    var companyName = import.Contact.CompanyName ?? "";
    var relationshipToCsePerson = import.Contact.RelationshipToCsePerson ?? "";
    var nameLast = import.Contact.NameLast ?? "";
    var nameFirst = import.Contact.NameFirst ?? "";
    var middleInitial = import.Contact.MiddleInitial ?? "";
    var homePhone = import.Contact.HomePhone.GetValueOrDefault();
    var workPhone = import.Contact.WorkPhone.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var workPhoneExt = import.Contact.WorkPhoneExt ?? "";
    var faxExt = import.Contact.FaxExt ?? "";
    var workPhoneAreaCode =
      import.Contact.WorkPhoneAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      import.Contact.HomePhoneAreaCode.GetValueOrDefault();
    var faxAreaCode = import.Contact.FaxAreaCode.GetValueOrDefault();
    var verifiedDate = import.Contact.VerifiedDate;
    var verifiedUserId = import.Contact.VerifiedUserId ?? "";

    entities.ExistingContact.Populated = false;
    Update("UpdateContact",
      (db, command) =>
      {
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
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "faxExt", faxExt);
        db.SetNullableInt32(command, "workPhoneArea", workPhoneAreaCode);
        db.SetNullableInt32(command, "homePhoneArea", homePhoneAreaCode);
        db.SetNullableInt32(command, "faxArea", faxAreaCode);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
        db.SetInt32(
          command, "contactNumber", entities.ExistingContact.ContactNumber);
      });

    entities.ExistingContact.Fax = fax;
    entities.ExistingContact.NameTitle = nameTitle;
    entities.ExistingContact.CompanyName = companyName;
    entities.ExistingContact.RelationshipToCsePerson = relationshipToCsePerson;
    entities.ExistingContact.NameLast = nameLast;
    entities.ExistingContact.NameFirst = nameFirst;
    entities.ExistingContact.MiddleInitial = middleInitial;
    entities.ExistingContact.HomePhone = homePhone;
    entities.ExistingContact.WorkPhone = workPhone;
    entities.ExistingContact.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingContact.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingContact.WorkPhoneExt = workPhoneExt;
    entities.ExistingContact.FaxExt = faxExt;
    entities.ExistingContact.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.ExistingContact.HomePhoneAreaCode = homePhoneAreaCode;
    entities.ExistingContact.FaxAreaCode = faxAreaCode;
    entities.ExistingContact.VerifiedDate = verifiedDate;
    entities.ExistingContact.VerifiedUserId = verifiedUserId;
    entities.ExistingContact.Populated = true;
  }

  private void UpdateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContactAddress.Populated);

    var street1 = import.ContactAddress.Street1 ?? "";
    var street2 = import.ContactAddress.Street2 ?? "";
    var city = import.ContactAddress.City ?? "";
    var state = import.ContactAddress.State ?? "";
    var zipCode5 = import.ContactAddress.ZipCode5 ?? "";
    var zipCode4 = import.ContactAddress.ZipCode4 ?? "";
    var zip3 = import.ContactAddress.Zip3 ?? "";
    var addressType = import.ContactAddress.AddressType ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingContactAddress.Populated = false;
    Update("UpdateContactAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "conNumber", entities.ExistingContactAddress.ConNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingContactAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingContactAddress.EffectiveDate.GetValueOrDefault());
      });

    entities.ExistingContactAddress.Street1 = street1;
    entities.ExistingContactAddress.Street2 = street2;
    entities.ExistingContactAddress.City = city;
    entities.ExistingContactAddress.State = state;
    entities.ExistingContactAddress.ZipCode5 = zipCode5;
    entities.ExistingContactAddress.ZipCode4 = zipCode4;
    entities.ExistingContactAddress.Zip3 = zip3;
    entities.ExistingContactAddress.AddressType = addressType;
    entities.ExistingContactAddress.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingContactAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingContactAddress.Populated = true;
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
    /// A value of TeResponseReceived.
    /// </summary>
    [JsonPropertyName("teResponseReceived")]
    public Common TeResponseReceived
    {
      get => teResponseReceived ??= new();
      set => teResponseReceived = value;
    }

    /// <summary>
    /// A value of UpdatedStamp.
    /// </summary>
    [JsonPropertyName("updatedStamp")]
    public Contact UpdatedStamp
    {
      get => updatedStamp ??= new();
      set => updatedStamp = value;
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

    private Common teResponseReceived;
    private Contact updatedStamp;
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
    /// A value of ContactAddressFound.
    /// </summary>
    [JsonPropertyName("contactAddressFound")]
    public Common ContactAddressFound
    {
      get => contactAddressFound ??= new();
      set => contactAddressFound = value;
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

    private Common contactAddressFound;
    private ContactDetail temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public ContactDetail ExistingLast
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
    /// A value of ExistingContact.
    /// </summary>
    [JsonPropertyName("existingContact")]
    public Contact ExistingContact
    {
      get => existingContact ??= new();
      set => existingContact = value;
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
    /// A value of ExistingContactAddress.
    /// </summary>
    [JsonPropertyName("existingContactAddress")]
    public ContactAddress ExistingContactAddress
    {
      get => existingContactAddress ??= new();
      set => existingContactAddress = value;
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

    /// <summary>
    /// A value of ExistingContactDetail.
    /// </summary>
    [JsonPropertyName("existingContactDetail")]
    public ContactDetail ExistingContactDetail
    {
      get => existingContactDetail ??= new();
      set => existingContactDetail = value;
    }

    private ContactDetail existingLast;
    private CsePerson existingCsePerson;
    private Contact existingContact;
    private ContactAddress newContactAddress;
    private ContactAddress existingContactAddress;
    private ContactDetail newContactDetail;
    private ContactDetail existingContactDetail;
  }
#endregion
}
