// Program: OE_ATTY_UPDATE_PRIV_ATTORNEY, ID: 372179496, model: 746.
// Short name: SWE00859
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_ATTY_UPDATE_PRIV_ATTORNEY.
/// </para>
/// <para>
/// This action block updates PERSON_PRIVATE_ATTORNEY and 
/// PRIVATE_ATTORNEY_ADDRESS records.
/// </para>
/// </summary>
[Serializable]
public partial class OeAttyUpdatePrivAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_UPDATE_PRIV_ATTORNEY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyUpdatePrivAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyUpdatePrivAttorney.
  /// </summary>
  public OeAttyUpdatePrivAttorney(IContext context, Import import, Export export)
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block updates Person Private Attorney and Private Attorney 
    // Address records.
    // PROCESSING:
    // This action block updates PERSON PRIVATE ATTORNEY record if any attribute
    // of PERSON PRIVATE ATTORNEY is changed.
    // If any of the attributes of PRIVATE ATTORNEY ADDRESS is changed: it 
    // updates the record if effective date is not changed; otherwise it creates
    // a new private attorney address record.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  PERSON_PRIVATE_ATTORNEY	- R U -
    //  PRIVATE_ATTORNEY_ADDRESS	C R U -
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	02/22/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	02/22/95			Initial coding
    // JHuss	07/27/10	CQ# 467		Added check to set dismissed date to 2099-12-31 
    // if it's null before updating
    // *********************************************
    // 	
    // JHarden  3/21/2017  CQ53818   Add email address to ATTY screen
    // JHarden    5/26/2017  Add fields consent, note, and bar #.
    MovePersonPrivateAttorney1(import.PersonPrivateAttorney,
      export.PersonPrivateAttorney);

    if (ReadPersonPrivateAttorney())
    {
      MovePersonPrivateAttorney2(entities.ExistingPersonPrivateAttorney,
        export.PersonPrivateAttorney);
      export.UpdateStamp.LastUpdatedBy =
        entities.ExistingPersonPrivateAttorney.CreatedBy;
      export.UpdateStamp.LastUpdatedTimestamp =
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp;

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedBy =
          entities.ExistingPersonPrivateAttorney.LastUpdatedBy;
        export.UpdateStamp.LastUpdatedTimestamp =
          entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp;
      }
    }
    else
    {
      ExitState = "PERSONS_ATTORNEY_NF";

      return;
    }

    // CQ57453 added fields consent, note, and Bar #.
    if (!Equal(entities.ExistingPersonPrivateAttorney.BarNumber,
      import.PersonPrivateAttorney.BarNumber) || AsChar
      (entities.ExistingPersonPrivateAttorney.ConsentIndicator) != AsChar
      (import.PersonPrivateAttorney.ConsentIndicator) || !
      Equal(entities.ExistingPersonPrivateAttorney.Note,
      import.PersonPrivateAttorney.Note) || !
      Equal(entities.ExistingPersonPrivateAttorney.EmailAddress,
      import.PersonPrivateAttorney.EmailAddress) || !
      Equal(entities.ExistingPersonPrivateAttorney.DateDismissed,
      import.PersonPrivateAttorney.DateDismissed) || !
      Equal(entities.ExistingPersonPrivateAttorney.DateRetained,
      import.PersonPrivateAttorney.DateRetained) || !
      Equal(entities.ExistingPersonPrivateAttorney.FaxNumber,
      entities.ExistingPersonPrivateAttorney.FaxNumber) || !
      Equal(entities.ExistingPersonPrivateAttorney.FaxExt,
      import.PersonPrivateAttorney.FaxExt) || !
      Equal(entities.ExistingPersonPrivateAttorney.FirmName,
      import.PersonPrivateAttorney.FirmName) || !
      Equal(entities.ExistingPersonPrivateAttorney.FirstName,
      import.PersonPrivateAttorney.FirstName) || !
      Equal(entities.ExistingPersonPrivateAttorney.LastName,
      import.PersonPrivateAttorney.LastName) || AsChar
      (entities.ExistingPersonPrivateAttorney.MiddleInitial) != AsChar
      (import.PersonPrivateAttorney.MiddleInitial) || !
      Equal(entities.ExistingPersonPrivateAttorney.Phone,
      import.PersonPrivateAttorney.Phone.GetValueOrDefault()) || !
      Equal(entities.ExistingPersonPrivateAttorney.PhoneExt,
      import.PersonPrivateAttorney.PhoneExt) || !
      Equal(entities.ExistingPersonPrivateAttorney.PhoneAreaCode,
      import.PersonPrivateAttorney.PhoneAreaCode.GetValueOrDefault()) || !
      Equal(entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode,
      import.PersonPrivateAttorney.FaxNumberAreaCode.GetValueOrDefault()) || !
      Equal(entities.ExistingPersonPrivateAttorney.CourtCaseNumber,
      import.PersonPrivateAttorney.CourtCaseNumber) || !
      Equal(entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation,
      import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
      Equal(entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation,
      import.PersonPrivateAttorney.FipsStateAbbreviation) || !
      Equal(entities.ExistingPersonPrivateAttorney.TribCountry,
      import.PersonPrivateAttorney.TribCountry))
    {
      // ---------------------------------------------
      // One or more attributes have been changed. So update the record.
      // ---------------------------------------------
      if (Equal(import.PersonPrivateAttorney.DateDismissed, null))
      {
        local.PersonPrivateAttorney.DateDismissed = new DateTime(2099, 12, 31);
      }
      else
      {
        local.PersonPrivateAttorney.DateDismissed =
          import.PersonPrivateAttorney.DateDismissed;
      }

      try
      {
        UpdatePersonPrivateAttorney();
        MovePersonPrivateAttorney2(entities.ExistingPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney3(entities.ExistingPersonPrivateAttorney,
          export.UpdateStamp);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PERSONS_ATTORNEY_AE";

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

    local.AttorneyAddressFound.Flag = "N";

    if (ReadPrivateAttorneyAddress())
    {
      local.Current.Assign(entities.ExistingPrivateAttorneyAddress);
      local.AttorneyAddressFound.Flag = "Y";

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        entities.ExistingPrivateAttorneyAddress.CreatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedBy =
          entities.ExistingPrivateAttorneyAddress.CreatedBy;
        export.UpdateStamp.LastUpdatedTimestamp =
          entities.ExistingPrivateAttorneyAddress.CreatedTimestamp;
      }

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedBy =
          entities.ExistingPrivateAttorneyAddress.LastUpdatedBy;
        export.UpdateStamp.LastUpdatedTimestamp =
          entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp;
      }
    }

    if (AsChar(local.AttorneyAddressFound.Flag) == 'N')
    {
      if (!IsEmpty(import.PrivateAttorneyAddress.City) || !
        IsEmpty(import.PrivateAttorneyAddress.Country) || !
        IsEmpty(import.PrivateAttorneyAddress.PostalCode) || !
        IsEmpty(import.PrivateAttorneyAddress.Province) || !
        IsEmpty(import.PrivateAttorneyAddress.State) || !
        IsEmpty(import.PrivateAttorneyAddress.Street1) || !
        IsEmpty(import.PrivateAttorneyAddress.Street2) || !
        IsEmpty(import.PrivateAttorneyAddress.Zip3) || !
        IsEmpty(import.PrivateAttorneyAddress.ZipCode4) || !
        IsEmpty(import.PrivateAttorneyAddress.ZipCode5))
      {
        try
        {
          CreatePrivateAttorneyAddress2();
          export.UpdateStamp.LastUpdatedBy = global.UserId;
          export.UpdateStamp.LastUpdatedTimestamp = Now();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PRIVATE_ATTORNEY_ADDRESS_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PRIVATE_ATTORNEY_ADDRESS_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else if (!Equal(import.PrivateAttorneyAddress.City,
      entities.ExistingPrivateAttorneyAddress.City) || !
      Equal(import.PrivateAttorneyAddress.Country,
      entities.ExistingPrivateAttorneyAddress.Country) || !
      Equal(import.PrivateAttorneyAddress.PostalCode,
      entities.ExistingPrivateAttorneyAddress.PostalCode) || !
      Equal(import.PrivateAttorneyAddress.Province,
      entities.ExistingPrivateAttorneyAddress.Province) || !
      Equal(import.PrivateAttorneyAddress.State,
      entities.ExistingPrivateAttorneyAddress.State) || !
      Equal(import.PrivateAttorneyAddress.Street1,
      entities.ExistingPrivateAttorneyAddress.Street1) || !
      Equal(import.PrivateAttorneyAddress.Street2,
      entities.ExistingPrivateAttorneyAddress.Street2) || !
      Equal(import.PrivateAttorneyAddress.Zip3,
      entities.ExistingPrivateAttorneyAddress.Zip3) || !
      Equal(import.PrivateAttorneyAddress.ZipCode4,
      entities.ExistingPrivateAttorneyAddress.ZipCode4) || !
      Equal(import.PrivateAttorneyAddress.ZipCode5,
      entities.ExistingPrivateAttorneyAddress.ZipCode5))
    {
      // ---------------------------------------------
      // One or more attributes have been changed. So update the address record.
      // ---------------------------------------------
      DeletePrivateAttorneyAddress();

      try
      {
        CreatePrivateAttorneyAddress1();
        export.UpdateStamp.LastUpdatedBy = global.UserId;
        export.UpdateStamp.LastUpdatedTimestamp = Now();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PRIVATE_ATTORNEY_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PRIVATE_ATTORNEY_ADDRESS_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MovePersonPrivateAttorney1(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
  }

  private static void MovePersonPrivateAttorney2(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MovePersonPrivateAttorney3(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void CreatePrivateAttorneyAddress1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);

    var ppaIdentifier = entities.ExistingPersonPrivateAttorney.Identifier;
    var cspNumber = entities.ExistingPersonPrivateAttorney.CspNumber;
    var effectiveDate = Now().Date;
    var street1 = import.PrivateAttorneyAddress.Street1 ?? "";
    var street2 = import.PrivateAttorneyAddress.Street2 ?? "";
    var city = import.PrivateAttorneyAddress.City ?? "";
    var state = import.PrivateAttorneyAddress.State ?? "";
    var province = import.PrivateAttorneyAddress.Province ?? "";
    var postalCode = import.PrivateAttorneyAddress.PostalCode ?? "";
    var zipCode5 = import.PrivateAttorneyAddress.ZipCode5 ?? "";
    var zipCode4 = import.PrivateAttorneyAddress.ZipCode4 ?? "";
    var zip3 = import.PrivateAttorneyAddress.Zip3 ?? "";
    var country = import.PrivateAttorneyAddress.Country ?? "";
    var createdBy = local.Current.CreatedBy;
    var createdTimestamp = local.Current.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.NewPrivateAttorneyAddress.Populated = false;
    Update("CreatePrivateAttorneyAddress1",
      (db, command) =>
      {
        db.SetInt32(command, "ppaIdentifier", ppaIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "addressType", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
      });

    entities.NewPrivateAttorneyAddress.PpaIdentifier = ppaIdentifier;
    entities.NewPrivateAttorneyAddress.CspNumber = cspNumber;
    entities.NewPrivateAttorneyAddress.EffectiveDate = effectiveDate;
    entities.NewPrivateAttorneyAddress.Street1 = street1;
    entities.NewPrivateAttorneyAddress.Street2 = street2;
    entities.NewPrivateAttorneyAddress.City = city;
    entities.NewPrivateAttorneyAddress.State = state;
    entities.NewPrivateAttorneyAddress.Province = province;
    entities.NewPrivateAttorneyAddress.PostalCode = postalCode;
    entities.NewPrivateAttorneyAddress.ZipCode5 = zipCode5;
    entities.NewPrivateAttorneyAddress.ZipCode4 = zipCode4;
    entities.NewPrivateAttorneyAddress.Zip3 = zip3;
    entities.NewPrivateAttorneyAddress.Country = country;
    entities.NewPrivateAttorneyAddress.AddressType = "";
    entities.NewPrivateAttorneyAddress.CreatedBy = createdBy;
    entities.NewPrivateAttorneyAddress.CreatedTimestamp = createdTimestamp;
    entities.NewPrivateAttorneyAddress.LastUpdatedBy = lastUpdatedBy;
    entities.NewPrivateAttorneyAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NewPrivateAttorneyAddress.Populated = true;
  }

  private void CreatePrivateAttorneyAddress2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);

    var ppaIdentifier = entities.ExistingPersonPrivateAttorney.Identifier;
    var cspNumber = entities.ExistingPersonPrivateAttorney.CspNumber;
    var effectiveDate = Now().Date;
    var street1 = import.PrivateAttorneyAddress.Street1 ?? "";
    var street2 = import.PrivateAttorneyAddress.Street2 ?? "";
    var city = import.PrivateAttorneyAddress.City ?? "";
    var state = import.PrivateAttorneyAddress.State ?? "";
    var province = import.PrivateAttorneyAddress.Province ?? "";
    var postalCode = import.PrivateAttorneyAddress.PostalCode ?? "";
    var zipCode5 = import.PrivateAttorneyAddress.ZipCode5 ?? "";
    var zipCode4 = import.PrivateAttorneyAddress.ZipCode4 ?? "";
    var zip3 = import.PrivateAttorneyAddress.Zip3 ?? "";
    var country = import.PrivateAttorneyAddress.Country ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.InitialisedToZeros.LastUpdatedTimestamp;

    entities.NewPrivateAttorneyAddress.Populated = false;
    Update("CreatePrivateAttorneyAddress2",
      (db, command) =>
      {
        db.SetInt32(command, "ppaIdentifier", ppaIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "addressType", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
      });

    entities.NewPrivateAttorneyAddress.PpaIdentifier = ppaIdentifier;
    entities.NewPrivateAttorneyAddress.CspNumber = cspNumber;
    entities.NewPrivateAttorneyAddress.EffectiveDate = effectiveDate;
    entities.NewPrivateAttorneyAddress.Street1 = street1;
    entities.NewPrivateAttorneyAddress.Street2 = street2;
    entities.NewPrivateAttorneyAddress.City = city;
    entities.NewPrivateAttorneyAddress.State = state;
    entities.NewPrivateAttorneyAddress.Province = province;
    entities.NewPrivateAttorneyAddress.PostalCode = postalCode;
    entities.NewPrivateAttorneyAddress.ZipCode5 = zipCode5;
    entities.NewPrivateAttorneyAddress.ZipCode4 = zipCode4;
    entities.NewPrivateAttorneyAddress.Zip3 = zip3;
    entities.NewPrivateAttorneyAddress.Country = country;
    entities.NewPrivateAttorneyAddress.AddressType = "";
    entities.NewPrivateAttorneyAddress.CreatedBy = createdBy;
    entities.NewPrivateAttorneyAddress.CreatedTimestamp = createdTimestamp;
    entities.NewPrivateAttorneyAddress.LastUpdatedBy = "";
    entities.NewPrivateAttorneyAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NewPrivateAttorneyAddress.Populated = true;
  }

  private void DeletePrivateAttorneyAddress()
  {
    Update("DeletePrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier",
          entities.ExistingPrivateAttorneyAddress.PpaIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPrivateAttorneyAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingPrivateAttorneyAddress.EffectiveDate.
            GetValueOrDefault());
      });
  }

  private bool ReadPersonPrivateAttorney()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(command, "casNumber", import.Case1.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);
    entities.ExistingPrivateAttorneyAddress.Populated = false;

    return Read("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier",
          entities.ExistingPersonPrivateAttorney.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonPrivateAttorney.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrivateAttorneyAddress.PpaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrivateAttorneyAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrivateAttorneyAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingPrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingPrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingPrivateAttorneyAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingPrivateAttorneyAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingPrivateAttorneyAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingPrivateAttorneyAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingPrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingPrivateAttorneyAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingPrivateAttorneyAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingPrivateAttorneyAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingPrivateAttorneyAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingPrivateAttorneyAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPrivateAttorneyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPrivateAttorneyAddress.Populated = true;
      });
  }

  private void UpdatePersonPrivateAttorney()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);

    var dateRetained = import.PersonPrivateAttorney.DateRetained;
    var dateDismissed = local.PersonPrivateAttorney.DateDismissed;
    var lastName = import.PersonPrivateAttorney.LastName ?? "";
    var firstName = import.PersonPrivateAttorney.FirstName ?? "";
    var middleInitial = import.PersonPrivateAttorney.MiddleInitial ?? "";
    var firmName = import.PersonPrivateAttorney.FirmName ?? "";
    var phone = import.PersonPrivateAttorney.Phone.GetValueOrDefault();
    var faxNumber = import.PersonPrivateAttorney.FaxNumber.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var faxNumberAreaCode =
      import.PersonPrivateAttorney.FaxNumberAreaCode.GetValueOrDefault();
    var faxExt = import.PersonPrivateAttorney.FaxExt ?? "";
    var phoneAreaCode =
      import.PersonPrivateAttorney.PhoneAreaCode.GetValueOrDefault();
    var phoneExt = import.PersonPrivateAttorney.PhoneExt ?? "";
    var courtCaseNumber = import.PersonPrivateAttorney.CourtCaseNumber ?? "";
    var fipsStateAbbreviation =
      import.PersonPrivateAttorney.FipsStateAbbreviation ?? "";
    var fipsCountyAbbreviation =
      import.PersonPrivateAttorney.FipsCountyAbbreviation ?? "";
    var tribCountry = import.PersonPrivateAttorney.TribCountry ?? "";
    var emailAddress = import.PersonPrivateAttorney.EmailAddress ?? "";
    var barNumber = import.PersonPrivateAttorney.BarNumber ?? "";
    var consentIndicator = import.PersonPrivateAttorney.ConsentIndicator ?? "";
    var note = import.PersonPrivateAttorney.Note ?? "";

    entities.ExistingPersonPrivateAttorney.Populated = false;
    Update("UpdatePersonPrivateAttorney",
      (db, command) =>
      {
        db.SetDate(command, "dateRetained", dateRetained);
        db.SetDate(command, "dateDismissed", dateDismissed);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "firmName", firmName);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableInt32(command, "faxArea", faxNumberAreaCode);
        db.SetNullableString(command, "faxExt", faxExt);
        db.SetNullableInt32(command, "phoneArea", phoneAreaCode);
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableString(command, "courtCaseNumber", courtCaseNumber);
        db.SetNullableString(command, "fipsStateAbbrev", fipsStateAbbreviation);
        db.
          SetNullableString(command, "fipsCountyAbbrev", fipsCountyAbbreviation);
          
        db.SetNullableString(command, "tribCountry", tribCountry);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableString(command, "barNumber", barNumber);
        db.SetNullableString(command, "consentInd", consentIndicator);
        db.SetNullableString(command, "note", note);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonPrivateAttorney.CspNumber);
        db.SetInt32(
          command, "identifier",
          entities.ExistingPersonPrivateAttorney.Identifier);
      });

    entities.ExistingPersonPrivateAttorney.DateRetained = dateRetained;
    entities.ExistingPersonPrivateAttorney.DateDismissed = dateDismissed;
    entities.ExistingPersonPrivateAttorney.LastName = lastName;
    entities.ExistingPersonPrivateAttorney.FirstName = firstName;
    entities.ExistingPersonPrivateAttorney.MiddleInitial = middleInitial;
    entities.ExistingPersonPrivateAttorney.FirmName = firmName;
    entities.ExistingPersonPrivateAttorney.Phone = phone;
    entities.ExistingPersonPrivateAttorney.FaxNumber = faxNumber;
    entities.ExistingPersonPrivateAttorney.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
      faxNumberAreaCode;
    entities.ExistingPersonPrivateAttorney.FaxExt = faxExt;
    entities.ExistingPersonPrivateAttorney.PhoneAreaCode = phoneAreaCode;
    entities.ExistingPersonPrivateAttorney.PhoneExt = phoneExt;
    entities.ExistingPersonPrivateAttorney.CourtCaseNumber = courtCaseNumber;
    entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
      fipsStateAbbreviation;
    entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
      fipsCountyAbbreviation;
    entities.ExistingPersonPrivateAttorney.TribCountry = tribCountry;
    entities.ExistingPersonPrivateAttorney.EmailAddress = emailAddress;
    entities.ExistingPersonPrivateAttorney.BarNumber = barNumber;
    entities.ExistingPersonPrivateAttorney.ConsentIndicator = consentIndicator;
    entities.ExistingPersonPrivateAttorney.Note = note;
    entities.ExistingPersonPrivateAttorney.Populated = true;
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
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private PrivateAttorneyAddress privateAttorneyAddress;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePerson csePerson;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public PersonPrivateAttorney UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    private PersonPrivateAttorney updateStamp;
    private PersonPrivateAttorney personPrivateAttorney;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AttorneyAddressFound.
    /// </summary>
    [JsonPropertyName("attorneyAddressFound")]
    public Common AttorneyAddressFound
    {
      get => attorneyAddressFound ??= new();
      set => attorneyAddressFound = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public PrivateAttorneyAddress Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public PersonPrivateAttorney InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    private Common attorneyAddressFound;
    private PrivateAttorneyAddress current;
    private PersonPrivateAttorney initialisedToZeros;
    private PersonPrivateAttorney personPrivateAttorney;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("existingPrivateAttorneyAddress")]
    public PrivateAttorneyAddress ExistingPrivateAttorneyAddress
    {
      get => existingPrivateAttorneyAddress ??= new();
      set => existingPrivateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of ExistingPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("existingPersonPrivateAttorney")]
    public PersonPrivateAttorney ExistingPersonPrivateAttorney
    {
      get => existingPersonPrivateAttorney ??= new();
      set => existingPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public PersonPrivateAttorney ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of NewPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("newPrivateAttorneyAddress")]
    public PrivateAttorneyAddress NewPrivateAttorneyAddress
    {
      get => newPrivateAttorneyAddress ??= new();
      set => newPrivateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of NewPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("newPersonPrivateAttorney")]
    public PersonPrivateAttorney NewPersonPrivateAttorney
    {
      get => newPersonPrivateAttorney ??= new();
      set => newPersonPrivateAttorney = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private PrivateAttorneyAddress existingPrivateAttorneyAddress;
    private PersonPrivateAttorney existingPersonPrivateAttorney;
    private PersonPrivateAttorney existingLast;
    private PrivateAttorneyAddress newPrivateAttorneyAddress;
    private PersonPrivateAttorney newPersonPrivateAttorney;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
  }
#endregion
}
