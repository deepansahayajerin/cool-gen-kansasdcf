// Program: SI_UPDATE_CSE_PERSON, ID: 371755706, model: 746.
// Short name: SWE01246
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This pad takes any changes about a persons' details and updates the CSE 
/// PERSON entity
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateCsePerson.
  /// </summary>
  public SiUpdateCsePerson(IContext context, Import import, Export export):
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
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 02/23/95  Helen Sharland                   Initial Development
    // 12/18/96  Govindaraj	    IDCR 252	 Associate organization with FIPS
    // 05/03/97  Govindaraj	    IDCR 252 (contd)	Added attrib tax id suffix
    // ----------------------------------------------------------
    // 07/02/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // ---------------------------------------------
    // This PAD updates a CSE Person.
    // ---------------------------------------------
    // 07/02/99 M.L         Change property of READ to generate
    //                      Select Only
    // 11/13/2001  Vithal   Added new attribute  'Birthplace_Country'
    // ------------------------------------------------------------
    // 018/11/2019  JHarden  CQ66290  Add an indicator for threats made by CP or
    // NCP on staff
    // 12/23/2020  GVandy  CQ68785  Add customer service code.
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    switch(AsChar(import.CsePerson.Type1))
    {
      case 'O':
        try
        {
          UpdateCsePerson2();

          // 07/02/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadFips1())
          {
            DisassociateCsePerson();
          }

          if (import.Fips.State > 0 || import.Fips.County > 0 || import
            .Fips.Location > 0)
          {
            // 07/02/99 M.L         Change property of READ to generate
            //                      Select Only
            // ------------------------------------------------------------
            if (ReadFips2())
            {
              AssociateCsePerson();
            }
            else
            {
              ExitState = "FIPS_NF";
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case 'C':
        try
        {
          UpdateCsePerson1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      default:
        break;
    }
  }

  private void AssociateCsePerson()
  {
    var cspNumber = entities.CsePerson.Number;

    entities.New1.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "state", entities.New1.State);
        db.SetInt32(command, "county", entities.New1.County);
        db.SetInt32(command, "location", entities.New1.Location);
      });

    entities.New1.CspNumber = cspNumber;
    entities.New1.Populated = true;
  }

  private void DisassociateCsePerson()
  {
    entities.Existing.Populated = false;
    Update("DisassociateCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Existing.State);
        db.SetInt32(command, "county", entities.Existing.County);
        db.SetInt32(command, "location", entities.Existing.Location);
      });

    entities.Existing.CspNumber = null;
    entities.Existing.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 20);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 21);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 22);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 23);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 24);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 26);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 27);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 28);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 29);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 30);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 31);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 32);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 33);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 34);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 35);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 36);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 37);
        entities.CsePerson.BirthplaceCountry = db.GetNullableString(reader, 38);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 39);
        entities.CsePerson.TribalCode = db.GetNullableString(reader, 40);
        entities.CsePerson.ThreatOnStaff = db.GetNullableString(reader, 41);
        entities.CsePerson.CustomerServiceCode =
          db.GetNullableString(reader, 42);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFips1()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.CspNumber = db.GetNullableString(reader, 3);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.New1.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.New1.State = db.GetInt32(reader, 0);
        entities.New1.County = db.GetInt32(reader, 1);
        entities.New1.Location = db.GetInt32(reader, 2);
        entities.New1.CspNumber = db.GetNullableString(reader, 3);
        entities.New1.Populated = true;
      });
  }

  private void UpdateCsePerson1()
  {
    var type1 = import.CsePerson.Type1;
    var occupation = import.CsePerson.Occupation ?? "";
    var aeCaseNumber = import.CsePerson.AeCaseNumber ?? "";
    var dateOfDeath = import.CsePerson.DateOfDeath;
    var illegalAlienIndicator = import.CsePerson.IllegalAlienIndicator ?? "";
    var currentSpouseMi = import.CsePerson.CurrentSpouseMi ?? "";
    var currentSpouseFirstName = import.CsePerson.CurrentSpouseFirstName ?? "";
    var birthPlaceState = import.CsePerson.BirthPlaceState ?? "";
    var emergencyPhone = import.CsePerson.EmergencyPhone.GetValueOrDefault();
    var nameMiddle = import.CsePerson.NameMiddle ?? "";
    var nameMaiden = import.CsePerson.NameMaiden ?? "";
    var homePhone = import.CsePerson.HomePhone.GetValueOrDefault();
    var otherNumber = import.CsePerson.OtherNumber.GetValueOrDefault();
    var birthPlaceCity = import.CsePerson.BirthPlaceCity ?? "";
    var currentMaritalStatus = import.CsePerson.CurrentMaritalStatus ?? "";
    var currentSpouseLastName = import.CsePerson.CurrentSpouseLastName ?? "";
    var race = import.CsePerson.Race ?? "";
    var hairColor = import.CsePerson.HairColor ?? "";
    var eyeColor = import.CsePerson.EyeColor ?? "";
    var taxId = import.CsePerson.TaxId ?? "";
    var organizationName = import.CsePerson.OrganizationName ?? "";
    var weight = import.CsePerson.Weight.GetValueOrDefault();
    var heightFt = import.CsePerson.HeightFt.GetValueOrDefault();
    var heightIn = import.CsePerson.HeightIn.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var kscaresNumber = import.CsePerson.KscaresNumber ?? "";
    var otherAreaCode = import.CsePerson.OtherAreaCode.GetValueOrDefault();
    var emergencyAreaCode =
      import.CsePerson.EmergencyAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      import.CsePerson.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode =
      import.CsePerson.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = import.CsePerson.WorkPhone.GetValueOrDefault();
    var workPhoneExt = import.CsePerson.WorkPhoneExt ?? "";
    var otherPhoneType = import.CsePerson.OtherPhoneType ?? "";
    var unemploymentInd = import.CsePerson.UnemploymentInd ?? "";
    var otherIdInfo = import.CsePerson.OtherIdInfo ?? "";
    var birthplaceCountry = import.CsePerson.BirthplaceCountry ?? "";
    var textMessageIndicator = import.CsePerson.TextMessageIndicator ?? "";
    var tribalCode = import.CsePerson.TribalCode ?? "";
    var threatOnStaff = import.CsePerson.ThreatOnStaff ?? "";
    var customerServiceCode = import.CsePerson.CustomerServiceCode ?? "";

    CheckValid<CsePerson>("Type1", type1);
    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(command, "illegalAlienInd", illegalAlienIndicator);
        db.SetNullableString(command, "currentSpouseMi", currentSpouseMi);
        db.
          SetNullableString(command, "currSpouse1StNm", currentSpouseFirstName);
          
        db.SetNullableString(command, "birthPlaceState", birthPlaceState);
        db.SetNullableInt32(command, "emergencyPhone", emergencyPhone);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameMaiden", nameMaiden);
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "otherNumber", otherNumber);
        db.SetNullableString(command, "birthPlaceCity", birthPlaceCity);
        db.SetNullableString(command, "currMaritalSts", currentMaritalStatus);
        db.SetNullableString(command, "curSpouseLastNm", currentSpouseLastName);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableString(command, "taxId", taxId);
        db.SetNullableString(command, "organizationName", organizationName);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetNullableInt32(command, "otherAreaCode", otherAreaCode);
        db.SetNullableInt32(command, "emergencyAreaCd", emergencyAreaCode);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "otherPhoneType", otherPhoneType);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "birthplaceCountry", birthplaceCountry);
        db.SetNullableString(command, "textMessageInd", textMessageIndicator);
        db.SetNullableString(command, "tribalCode", tribalCode);
        db.SetNullableString(command, "threatOnStaff", threatOnStaff);
        db.SetNullableString(command, "custServiceCd", customerServiceCode);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.Type1 = type1;
    entities.CsePerson.Occupation = occupation;
    entities.CsePerson.AeCaseNumber = aeCaseNumber;
    entities.CsePerson.DateOfDeath = dateOfDeath;
    entities.CsePerson.IllegalAlienIndicator = illegalAlienIndicator;
    entities.CsePerson.CurrentSpouseMi = currentSpouseMi;
    entities.CsePerson.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.CsePerson.BirthPlaceState = birthPlaceState;
    entities.CsePerson.EmergencyPhone = emergencyPhone;
    entities.CsePerson.NameMiddle = nameMiddle;
    entities.CsePerson.NameMaiden = nameMaiden;
    entities.CsePerson.HomePhone = homePhone;
    entities.CsePerson.OtherNumber = otherNumber;
    entities.CsePerson.BirthPlaceCity = birthPlaceCity;
    entities.CsePerson.CurrentMaritalStatus = currentMaritalStatus;
    entities.CsePerson.CurrentSpouseLastName = currentSpouseLastName;
    entities.CsePerson.Race = race;
    entities.CsePerson.HairColor = hairColor;
    entities.CsePerson.EyeColor = eyeColor;
    entities.CsePerson.TaxId = taxId;
    entities.CsePerson.OrganizationName = organizationName;
    entities.CsePerson.Weight = weight;
    entities.CsePerson.HeightFt = heightFt;
    entities.CsePerson.HeightIn = heightIn;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.KscaresNumber = kscaresNumber;
    entities.CsePerson.OtherAreaCode = otherAreaCode;
    entities.CsePerson.EmergencyAreaCode = emergencyAreaCode;
    entities.CsePerson.HomePhoneAreaCode = homePhoneAreaCode;
    entities.CsePerson.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.CsePerson.WorkPhone = workPhone;
    entities.CsePerson.WorkPhoneExt = workPhoneExt;
    entities.CsePerson.OtherPhoneType = otherPhoneType;
    entities.CsePerson.UnemploymentInd = unemploymentInd;
    entities.CsePerson.OtherIdInfo = otherIdInfo;
    entities.CsePerson.BirthplaceCountry = birthplaceCountry;
    entities.CsePerson.TextMessageIndicator = textMessageIndicator;
    entities.CsePerson.TribalCode = tribalCode;
    entities.CsePerson.ThreatOnStaff = threatOnStaff;
    entities.CsePerson.CustomerServiceCode = customerServiceCode;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var taxId = import.CsePerson.TaxId ?? "";
    var organizationName = import.CsePerson.OrganizationName ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var taxIdSuffix = import.CsePerson.TaxIdSuffix ?? "";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "taxId", taxId);
        db.SetNullableString(command, "organizationName", organizationName);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "taxIdSuffix", taxIdSuffix);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.TaxId = taxId;
    entities.CsePerson.OrganizationName = organizationName;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.TaxIdSuffix = taxIdSuffix;
    entities.CsePerson.Populated = true;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private Fips fips;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of UpdateOk.
    /// </summary>
    [JsonPropertyName("updateOk")]
    public Common UpdateOk
    {
      get => updateOk ??= new();
      set => updateOk = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common updateOk;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Fips New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private Fips existing;
    private Fips new1;
    private CsePerson csePerson;
  }
#endregion
}
