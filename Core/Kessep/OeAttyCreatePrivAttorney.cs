// Program: OE_ATTY_CREATE_PRIV_ATTORNEY, ID: 372179494, model: 746.
// Short name: SWE00856
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_ATTY_CREATE_PRIV_ATTORNEY.
/// </para>
/// <para>
/// This action block facilitates creation of PERSON_PRIVATE_ATTORNEY and 
/// PRIVATE_ATTORNEY_ADDRESS records.
/// </para>
/// </summary>
[Serializable]
public partial class OeAttyCreatePrivAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_CREATE_PRIV_ATTORNEY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyCreatePrivAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyCreatePrivAttorney.
  /// </summary>
  public OeAttyCreatePrivAttorney(IContext context, Import import, Export export)
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
    // This action block CREATEs PERSON_PRIVATE_ATTORNEY and 
    // PRIVATE_ATTORNEY_ADDRESS records.
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  PERSON_PRIVATE_ATTORNEY	C R - -
    //  PRIVATE_ATTORNEY_ADDRESS	C - - -
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	02/22/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	02/22/95			Initial coding
    // *********************************************
    // 	
    // JHarden    3/17/17    CQ53818  Add email address to ATTY screen
    // JHarden    5/26/17   CQ57453  add fields consent, note, and bar #.
    MovePersonPrivateAttorney1(import.PersonPrivateAttorney,
      export.PersonPrivateAttorney);

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------
    // Get the next available IDENTIFIER for the PERSON_PRIVATE_ATTORNEY
    // ---------------------------------------------
    local.Last.Identifier = 0;

    if (ReadPersonPrivateAttorney())
    {
      local.Last.Identifier = entities.ExistingLast.Identifier;
    }

    if (Equal(import.PersonPrivateAttorney.DateRetained, null))
    {
      local.DateRetained.Date = Now().Date;
    }
    else
    {
      local.DateRetained.Date = import.PersonPrivateAttorney.DateRetained;
    }

    if (Equal(import.PersonPrivateAttorney.DateDismissed, null))
    {
      local.DateDismissed.Date = new DateTime(2099, 12, 31);
    }
    else
    {
      local.DateDismissed.Date = import.PersonPrivateAttorney.DateDismissed;
    }

    try
    {
      CreatePersonPrivateAttorney();
      MovePersonPrivateAttorney2(entities.NewPersonPrivateAttorney,
        export.PersonPrivateAttorney);
      export.UpdateStamp.LastUpdatedBy = global.UserId;
      export.UpdateStamp.LastUpdatedTimestamp = Now();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_CONTENTION_IN_GENKEY";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      CreatePrivateAttorneyAddress();
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

  private void CreatePersonPrivateAttorney()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = local.Last.Identifier + 1;
    var casNumber = entities.ExistingCase.Number;
    var dateRetained = local.DateRetained.Date;
    var dateDismissed = local.DateDismissed.Date;
    var lastName = import.PersonPrivateAttorney.LastName ?? "";
    var firstName = import.PersonPrivateAttorney.FirstName ?? "";
    var middleInitial = import.PersonPrivateAttorney.MiddleInitial ?? "";
    var firmName = import.PersonPrivateAttorney.FirmName ?? "";
    var phone = import.PersonPrivateAttorney.Phone.GetValueOrDefault();
    var faxNumber = import.PersonPrivateAttorney.FaxNumber.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.InitialisedToZeros.LastUpdatedTimestamp;
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

    entities.NewPersonPrivateAttorney.Populated = false;
    Update("CreatePersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetDate(command, "dateRetained", dateRetained);
        db.SetDate(command, "dateDismissed", dateDismissed);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "firmName", firmName);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
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
      });

    entities.NewPersonPrivateAttorney.CspNumber = cspNumber;
    entities.NewPersonPrivateAttorney.Identifier = identifier;
    entities.NewPersonPrivateAttorney.CasNumber = casNumber;
    entities.NewPersonPrivateAttorney.DateRetained = dateRetained;
    entities.NewPersonPrivateAttorney.DateDismissed = dateDismissed;
    entities.NewPersonPrivateAttorney.LastName = lastName;
    entities.NewPersonPrivateAttorney.FirstName = firstName;
    entities.NewPersonPrivateAttorney.MiddleInitial = middleInitial;
    entities.NewPersonPrivateAttorney.FirmName = firmName;
    entities.NewPersonPrivateAttorney.Phone = phone;
    entities.NewPersonPrivateAttorney.FaxNumber = faxNumber;
    entities.NewPersonPrivateAttorney.CreatedBy = createdBy;
    entities.NewPersonPrivateAttorney.CreatedTimestamp = createdTimestamp;
    entities.NewPersonPrivateAttorney.LastUpdatedBy = "";
    entities.NewPersonPrivateAttorney.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NewPersonPrivateAttorney.FaxNumberAreaCode = faxNumberAreaCode;
    entities.NewPersonPrivateAttorney.FaxExt = faxExt;
    entities.NewPersonPrivateAttorney.PhoneAreaCode = phoneAreaCode;
    entities.NewPersonPrivateAttorney.PhoneExt = phoneExt;
    entities.NewPersonPrivateAttorney.CourtCaseNumber = courtCaseNumber;
    entities.NewPersonPrivateAttorney.FipsStateAbbreviation =
      fipsStateAbbreviation;
    entities.NewPersonPrivateAttorney.FipsCountyAbbreviation =
      fipsCountyAbbreviation;
    entities.NewPersonPrivateAttorney.TribCountry = tribCountry;
    entities.NewPersonPrivateAttorney.EmailAddress = emailAddress;
    entities.NewPersonPrivateAttorney.BarNumber = barNumber;
    entities.NewPersonPrivateAttorney.ConsentIndicator = consentIndicator;
    entities.NewPersonPrivateAttorney.Note = note;
    entities.NewPersonPrivateAttorney.Populated = true;
  }

  private void CreatePrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewPersonPrivateAttorney.Populated);

    var ppaIdentifier = entities.NewPersonPrivateAttorney.Identifier;
    var cspNumber = entities.NewPersonPrivateAttorney.CspNumber;
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
    Update("CreatePrivateAttorneyAddress",
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

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
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

  private bool ReadPersonPrivateAttorney()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
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
    /// A value of DateDismissed.
    /// </summary>
    [JsonPropertyName("dateDismissed")]
    public DateWorkArea DateDismissed
    {
      get => dateDismissed ??= new();
      set => dateDismissed = value;
    }

    /// <summary>
    /// A value of DateRetained.
    /// </summary>
    [JsonPropertyName("dateRetained")]
    public DateWorkArea DateRetained
    {
      get => dateRetained ??= new();
      set => dateRetained = value;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public PersonPrivateAttorney Last
    {
      get => last ??= new();
      set => last = value;
    }

    private DateWorkArea dateDismissed;
    private DateWorkArea dateRetained;
    private PersonPrivateAttorney initialisedToZeros;
    private PersonPrivateAttorney last;
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

    private PersonPrivateAttorney existingLast;
    private PrivateAttorneyAddress newPrivateAttorneyAddress;
    private PersonPrivateAttorney newPersonPrivateAttorney;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
  }
#endregion
}
