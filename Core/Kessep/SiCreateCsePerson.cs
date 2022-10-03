// Program: SI_CREATE_CSE_PERSON, ID: 371727792, model: 746.
// Short name: SWE01123
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD adds information about a CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCsePerson.
  /// </summary>
  public SiCreateCsePerson(IContext context, Import import, Export export):
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
    //   Date    Developer		Description
    // 02/24/95  Helen Sharland	Initial Development
    // 02/20/97  G. Lofton - MTW	Added FIPS association
    // 06/06/97  Sid			Cleanup.
    // 06/18/99  M. Lachowicz          Change property of READ (Select only)
    // 05/11/00  M.Lachowicz           Add new attributes of
    //                                 
    // CSE_Person
    // ----------------------------------------------------------
    // ---------------------------------------------
    // This PAD creates a CSE Person.
    // ---------------------------------------------
    // 05/11/00 M.L Start
    if (IsEmpty(import.CsePerson.BornOutOfWedlock))
    {
      local.CsePerson.BornOutOfWedlock = "U";
    }
    else
    {
      local.CsePerson.BornOutOfWedlock = import.CsePerson.BornOutOfWedlock ?? ""
        ;
    }

    if (IsEmpty(import.CsePerson.CseToEstblPaternity))
    {
      local.CsePerson.CseToEstblPaternity = "U";
    }
    else
    {
      local.CsePerson.CseToEstblPaternity =
        import.CsePerson.CseToEstblPaternity ?? "";
    }

    if (IsEmpty(import.CsePerson.PaternityEstablishedIndicator))
    {
      local.CsePerson.PaternityEstablishedIndicator = "N";
    }
    else
    {
      local.CsePerson.PaternityEstablishedIndicator =
        import.CsePerson.PaternityEstablishedIndicator ?? "";
    }

    switch(AsChar(import.CsePerson.Type1))
    {
      case 'C':
        try
        {
          CreateCsePerson2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_AE";

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
      case 'O':
        local.ControlTable.Identifier = "CSE PERSON ORGANIZATION";
        UseAccessControlTable();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.WorkArea.Text9 =
          NumberToString(local.ControlTable.LastUsedNumber, 9);
        local.CsePerson.Number = local.WorkArea.Text9 + "O";

        // ---------------------------------------------
        // Call the action block which creates
        // the next available number for an organization
        // ---------------------------------------------
        try
        {
          CreateCsePerson1();

          if (import.Fips.State > 0 || import.Fips.County > 0 || import
            .Fips.Location > 0)
          {
            // 06/18/99 M.L Start
            if (ReadFips())
            {
              AssociateCsePerson();
            }
            else
            {
              ExitState = "FIPS_NF";
            }

            // 06/18/99 M.L End
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CSE_PERSON_AE";

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

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void AssociateCsePerson()
  {
    var cspNumber = entities.CsePerson.Number;

    entities.Fips.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "state", entities.Fips.State);
        db.SetInt32(command, "county", entities.Fips.County);
        db.SetInt32(command, "location", entities.Fips.Location);
      });

    entities.Fips.CspNumber = cspNumber;
    entities.Fips.Populated = true;
  }

  private void CreateCsePerson1()
  {
    var number = local.CsePerson.Number;
    var type1 = import.CsePerson.Type1;
    var taxId = import.CsePerson.TaxId ?? "";
    var organizationName = import.CsePerson.OrganizationName ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var taxIdSuffix = import.CsePerson.TaxIdSuffix ?? "";

    CheckValid<CsePerson>("Type1", type1);
    entities.CsePerson.Populated = false;
    Update("CreateCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", number);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "occupation", "");
        db.SetNullableString(command, "aeCaseNumber", "");
        db.SetNullableDate(command, "dateOfDeath", null);
        db.SetNullableString(command, "illegalAlienInd", "");
        db.SetNullableString(command, "currentSpouseMi", "");
        db.SetNullableString(command, "currSpouse1StNm", "");
        db.SetNullableString(command, "birthPlaceState", "");
        db.SetNullableInt32(command, "emergencyPhone", 0);
        db.SetNullableString(command, "nameMiddle", "");
        db.SetNullableString(command, "nameMaiden", "");
        db.SetNullableInt32(command, "homePhone", 0);
        db.SetNullableInt32(command, "otherNumber", 0);
        db.SetNullableString(command, "birthPlaceCity", "");
        db.SetNullableString(command, "currMaritalSts", "");
        db.SetNullableString(command, "curSpouseLastNm", "");
        db.SetNullableString(command, "race", "");
        db.SetNullableString(command, "hairColor", "");
        db.SetNullableString(command, "eyeColor", "");
        db.SetNullableString(command, "taxId", taxId);
        db.SetNullableString(command, "organizationName", organizationName);
        db.SetNullableInt32(command, "weight", 0);
        db.SetNullableInt32(command, "heightFt", 0);
        db.SetNullableInt32(command, "heightIn", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "kscaresNumber", "");
        db.SetNullableInt32(command, "otherAreaCode", 0);
        db.SetNullableInt32(command, "emergencyAreaCd", 0);
        db.SetNullableInt32(command, "homePhoneAreaCd", 0);
        db.SetNullableInt32(command, "workPhoneAreaCd", 0);
        db.SetNullableInt32(command, "workPhone", 0);
        db.SetNullableString(command, "workPhoneExt", "");
        db.SetNullableString(command, "otherPhoneType", "");
        db.SetNullableString(command, "unemploymentInd", "");
        db.SetNullableString(command, "federalInd", "");
        db.SetNullableString(command, "taxIdSuffix", taxIdSuffix);
        db.SetNullableString(command, "otherIdInfo", "");
        db.SetNullableString(command, "familyViolInd", "");
        db.SetNullableString(command, "outOfWedlock", "");
        db.SetNullableString(command, "cseToEstPatr", "");
        db.SetNullableString(command, "patEstabInd", "");
        db.SetDate(command, "datePaternEstab", null);
        db.SetNullableString(command, "bcFatherLastNm", "");
        db.SetNullableString(command, "bcFatherFirstNm", "");
        db.SetNullableString(command, "bcFathersMi", "");
        db.SetNullableString(command, "bcSignature", "");
        db.SetNullableDate(command, "fvLtrSentDt", default(DateTime));
        db.SetNullableString(command, "birthplaceCountry", "");
        db.SetNullableString(command, "hospitalPatEst", "");
        db.SetNullableString(command, "fviUpdatedBy", "");
        db.SetNullableString(command, "tribalCode", "");
      });

    entities.CsePerson.Number = number;
    entities.CsePerson.Type1 = type1;
    entities.CsePerson.Occupation = "";
    entities.CsePerson.AeCaseNumber = "";
    entities.CsePerson.DateOfDeath = null;
    entities.CsePerson.IllegalAlienIndicator = "";
    entities.CsePerson.CurrentSpouseMi = "";
    entities.CsePerson.CurrentSpouseFirstName = "";
    entities.CsePerson.BirthPlaceState = "";
    entities.CsePerson.EmergencyPhone = 0;
    entities.CsePerson.NameMiddle = "";
    entities.CsePerson.NameMaiden = "";
    entities.CsePerson.HomePhone = 0;
    entities.CsePerson.OtherNumber = 0;
    entities.CsePerson.BirthPlaceCity = "";
    entities.CsePerson.CurrentMaritalStatus = "";
    entities.CsePerson.CurrentSpouseLastName = "";
    entities.CsePerson.Race = "";
    entities.CsePerson.HairColor = "";
    entities.CsePerson.EyeColor = "";
    entities.CsePerson.TaxId = taxId;
    entities.CsePerson.OrganizationName = organizationName;
    entities.CsePerson.Weight = 0;
    entities.CsePerson.HeightFt = 0;
    entities.CsePerson.HeightIn = 0;
    entities.CsePerson.CreatedBy = createdBy;
    entities.CsePerson.CreatedTimestamp = createdTimestamp;
    entities.CsePerson.LastUpdatedTimestamp = null;
    entities.CsePerson.LastUpdatedBy = "";
    entities.CsePerson.KscaresNumber = "";
    entities.CsePerson.OtherAreaCode = 0;
    entities.CsePerson.EmergencyAreaCode = 0;
    entities.CsePerson.HomePhoneAreaCode = 0;
    entities.CsePerson.WorkPhoneAreaCode = 0;
    entities.CsePerson.WorkPhone = 0;
    entities.CsePerson.WorkPhoneExt = "";
    entities.CsePerson.OtherPhoneType = "";
    entities.CsePerson.UnemploymentInd = "";
    entities.CsePerson.FederalInd = "";
    entities.CsePerson.TaxIdSuffix = taxIdSuffix;
    entities.CsePerson.OtherIdInfo = "";
    entities.CsePerson.FamilyViolenceIndicator = "";
    entities.CsePerson.BornOutOfWedlock = "";
    entities.CsePerson.CseToEstblPaternity = "";
    entities.CsePerson.PaternityEstablishedIndicator = "";
    entities.CsePerson.DatePaternEstab = null;
    entities.CsePerson.BirthCertFathersLastName = "";
    entities.CsePerson.BirthCertFathersFirstName = "";
    entities.CsePerson.BirthCertFathersMi = "";
    entities.CsePerson.BirthCertificateSignature = "";
    entities.CsePerson.Populated = true;
  }

  private void CreateCsePerson2()
  {
    var number = import.CsePersonsWorkSet.Number;
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
    var weight = import.CsePerson.Weight.GetValueOrDefault();
    var heightFt = import.CsePerson.HeightFt.GetValueOrDefault();
    var heightIn = import.CsePerson.HeightIn.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
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
    var federalInd = import.CsePerson.FederalInd ?? "";
    var otherIdInfo = import.CsePerson.OtherIdInfo ?? "";
    var bornOutOfWedlock = local.CsePerson.BornOutOfWedlock ?? "";
    var cseToEstblPaternity = local.CsePerson.CseToEstblPaternity ?? "";
    var paternityEstablishedIndicator =
      local.CsePerson.PaternityEstablishedIndicator ?? "";
    var datePaternEstab = local.Initial.Date;

    CheckValid<CsePerson>("Type1", type1);
    entities.CsePerson.Populated = false;
    Update("CreateCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", number);
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
        db.SetNullableString(command, "taxId", "");
        db.SetNullableString(command, "organizationName", "");
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetNullableInt32(command, "otherAreaCode", otherAreaCode);
        db.SetNullableInt32(command, "emergencyAreaCd", emergencyAreaCode);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "otherPhoneType", otherPhoneType);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetNullableString(command, "federalInd", federalInd);
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "familyViolInd", "");
        db.SetNullableString(command, "outOfWedlock", bornOutOfWedlock);
        db.SetNullableString(command, "cseToEstPatr", cseToEstblPaternity);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
        db.SetDate(command, "datePaternEstab", datePaternEstab);
        db.SetNullableString(command, "bcFatherLastNm", "");
        db.SetNullableString(command, "bcFatherFirstNm", "");
        db.SetNullableString(command, "bcFathersMi", "");
        db.SetNullableString(command, "bcSignature", "");
        db.SetNullableDate(command, "fvLtrSentDt", default(DateTime));
        db.SetNullableString(command, "birthplaceCountry", "");
        db.SetNullableString(command, "hospitalPatEst", "");
        db.SetNullableString(command, "fviUpdatedBy", "");
        db.SetNullableString(command, "tribalCode", "");
      });

    entities.CsePerson.Number = number;
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
    entities.CsePerson.TaxId = "";
    entities.CsePerson.OrganizationName = "";
    entities.CsePerson.Weight = weight;
    entities.CsePerson.HeightFt = heightFt;
    entities.CsePerson.HeightIn = heightIn;
    entities.CsePerson.CreatedBy = createdBy;
    entities.CsePerson.CreatedTimestamp = createdTimestamp;
    entities.CsePerson.LastUpdatedTimestamp = createdTimestamp;
    entities.CsePerson.LastUpdatedBy = createdBy;
    entities.CsePerson.KscaresNumber = kscaresNumber;
    entities.CsePerson.OtherAreaCode = otherAreaCode;
    entities.CsePerson.EmergencyAreaCode = emergencyAreaCode;
    entities.CsePerson.HomePhoneAreaCode = homePhoneAreaCode;
    entities.CsePerson.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.CsePerson.WorkPhone = workPhone;
    entities.CsePerson.WorkPhoneExt = workPhoneExt;
    entities.CsePerson.OtherPhoneType = otherPhoneType;
    entities.CsePerson.UnemploymentInd = unemploymentInd;
    entities.CsePerson.FederalInd = federalInd;
    entities.CsePerson.TaxIdSuffix = "";
    entities.CsePerson.OtherIdInfo = otherIdInfo;
    entities.CsePerson.FamilyViolenceIndicator = "";
    entities.CsePerson.BornOutOfWedlock = bornOutOfWedlock;
    entities.CsePerson.CseToEstblPaternity = cseToEstblPaternity;
    entities.CsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.CsePerson.DatePaternEstab = datePaternEstab;
    entities.CsePerson.BirthCertFathersLastName = "";
    entities.CsePerson.BirthCertFathersFirstName = "";
    entities.CsePerson.BirthCertFathersMi = "";
    entities.CsePerson.BirthCertificateSignature = "";
    entities.CsePerson.Populated = true;
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CspNumber = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
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

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Fips fips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public DateWorkArea Initial
    {
      get => initial ??= new();
      set => initial = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of ZdelClientDbf.
    /// </summary>
    [JsonPropertyName("zdelClientDbf")]
    public ZdelClientDbf ZdelClientDbf
    {
      get => zdelClientDbf ??= new();
      set => zdelClientDbf = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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

    private DateWorkArea initial;
    private WorkArea workArea;
    private ControlTable controlTable;
    private ZdelClientDbf zdelClientDbf;
    private Common error;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ZdelClientDbf Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    private ZdelClientDbf zdel;
    private CsePerson csePerson;
  }
#endregion
}
