// Program: SI_READ_CHILD_DETAILS, ID: 371757640, model: 746.
// Short name: SWE01207
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
/// A program: SI_READ_CHILD_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This process will retrieve details about a child on a specified case and the
/// role that person plays within the case.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadChildDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CHILD_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadChildDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadChildDetails.
  /// </summary>
  public SiReadChildDetails(IContext context, Import import, Export export):
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
    // This process retrieves the details about a
    // Child and the case specific details.
    // ---------------------------------------------
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 03/14/95  Helen Sharland    Initial Development
    // 03/31/97  Sid Chowdhary	    Derive the "Health Insurance" and
    // 			    "Medical Support" indicators.
    // 04/29/97  A.Kinne           Changed Current_Date
    // 04/30/97  Sid               Suppress zero DOB
    // 10/07/98  C Deghand         Added a statement to
    //                             check for an obligation
    //                             type of HIC per Pam V (SME) request
    // ----------------------------------------------------------
    // 02/05/99 W.Campbell         Removed qualifier
    //                             from a READ which used
    //                             ZDEL_START_DATE from the
    //                             CSE_PERSON_ADDRESS entity type.
    //                             This was done on IDCR454.
    //                             The READ may need to be changed
    //                             to get the 'best' address.
    // ---------------------------------------------
    // 06/22/99 W.Campbell         Modified the properties
    //                             of a READ statement to
    //                             Select Only.
    // ---------------------------------------------------------
    // 03/03/00  C. Ott            Modified for PRWORA Paternity redesign.
    //                             Added Paternity attributes to CSE
    //                             Person.
    // ---------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 12/26/2006	Raj S		PR 296538 	Modified to fix the problem of Medical 
    // Support
    //                                                 
    // Indicator setting.
    // ------------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    UseSiReadCsePerson();

    if (!IsEmpty(export.AbendData.Type1))
    {
      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadCaseRoleCsePerson())
    {
      export.ChildCaseRole.Assign(entities.ChildCaseRole);
      MoveCsePerson1(entities.ChildCsePerson, export.ChildCsePerson);

      if (!Lt(local.Current.Date, entities.ChildCaseRole.StartDate) && !
        Lt(entities.ChildCaseRole.EndDate, local.Current.Date))
      {
        export.ActiveChild.Flag = "Y";
      }
      else
      {
        export.ActiveChild.Flag = "N";
      }
    }

    if (!entities.ChildCaseRole.Populated)
    {
      // ---------------------------------------------
      // Read the case separately to determine whether
      // the Case Role is missing or whether the Case is not found.
      // ---------------------------------------------
      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCase())
      {
        ExitState = "CASE_ROLE_NF";
      }
      else
      {
        ExitState = "CASE_NF";
      }

      return;
    }

    export.Group.Index = -1;

    foreach(var item in ReadProgram())
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Detail.Code = entities.ChildProgram.Code;

      if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
      {
        ExitState = "MORE_PROGRAMS_AVAILABLE";

        break;
      }
    }

    // ---------------------------------------------
    // Retrieve the Placement Address of the Foster
    // Care Child
    // ---------------------------------------------
    // ---------------------------------------------
    // 02/05/99 W.Campbell - Removed qualifier
    // from the following READ which used
    // ZDEL_START_DATE from the
    // CSE_PERSON_ADDRESS entity type.
    // This was done on IDCR454.
    // The READ may need to be changed
    // to get the 'best' address.
    // ---------------------------------------------
    if (ReadCsePersonAddress())
    {
      export.ChildFc.Assign(entities.ChildCsePersonAddress);
    }
    else
    {
      // ---------------------------------------------
      // This is okay
      // ---------------------------------------------
    }

    // ---------------------------------------------
    // Derive the "Health Insurance" exists indicator for the Child.
    // ----------------------------------------------
    if (ReadPersonalHealthInsurance())
    {
      export.ChildCaseRole.HealthInsuranceIndicator = "Y";
    }
    else
    {
      export.ChildCaseRole.HealthInsuranceIndicator = "N";
    }

    // ---------------------------------------------
    // Derive the "Medical Support" ordered indicator for the Child.
    // ----------------------------------------------
    // ---------------------------------------------------------------
    // Added check for HIC obligation type C Deghand 10/07/98
    // ---------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // Date:12/14/2006  Who: Raj S    Ref: PR#296538
    // Change Description: Added parenthesis to distingsuish OR condition for 
    // financial
    //                     obligation type.
    // --------------------------------------------------------------------------------
    if (ReadLegalActionPerson())
    {
      export.ChildCaseRole.MedicalSupportIndicator = "Y";
    }
    else
    {
      export.ChildCaseRole.MedicalSupportIndicator = "N";
    }
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.BornOutOfWedlock = source.BornOutOfWedlock;
    target.CseToEstblPaternity = source.CseToEstblPaternity;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
    target.DatePaternEstab = source.DatePaternEstab;
    target.BirthCertFathersLastName = source.BirthCertFathersLastName;
    target.BirthCertFathersFirstName = source.BirthCertFathersFirstName;
    target.BirthCertFathersMi = source.BirthCertFathersMi;
    target.BirthCertificateSignature = source.BirthCertificateSignature;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.BornOutOfWedlock = source.BornOutOfWedlock;
    target.CseToEstblPaternity = source.CseToEstblPaternity;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
    target.DatePaternEstab = source.DatePaternEstab;
    target.BirthCertFathersLastName = source.BirthCertFathersLastName;
    target.BirthCertFathersFirstName = source.BirthCertFathersFirstName;
    target.BirthCertFathersMi = source.BirthCertFathersMi;
    target.BirthCertificateSignature = source.BirthCertificateSignature;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Child.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.ChildCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson2(useExport.CsePerson, export.ChildCsePerson);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ChildCaseRole.Populated = false;
    entities.ChildCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChildCsePerson.Number = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChildCaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.ChildCaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ChildCaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ChildCaseRole.AbsenceReasonCode =
          db.GetNullableString(reader, 9);
        entities.ChildCaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 10);
        entities.ChildCaseRole.ArWaivedInsurance =
          db.GetNullableString(reader, 11);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 12);
        entities.ChildCaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 13);
        entities.ChildCaseRole.FcApNotified = db.GetNullableString(reader, 14);
        entities.ChildCaseRole.FcCincInd = db.GetNullableString(reader, 15);
        entities.ChildCaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 16);
        entities.ChildCaseRole.FcCostOfCareFreq =
          db.GetNullableString(reader, 17);
        entities.ChildCaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 18);
        entities.ChildCaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 19);
        entities.ChildCaseRole.FcInHomeServiceInd =
          db.GetNullableString(reader, 20);
        entities.ChildCaseRole.FcIvECaseNumber =
          db.GetNullableString(reader, 21);
        entities.ChildCaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 22);
        entities.ChildCaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 23);
        entities.ChildCaseRole.FcLevelOfCare = db.GetNullableString(reader, 24);
        entities.ChildCaseRole.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 25);
        entities.ChildCaseRole.FcOrderEstBy = db.GetNullableString(reader, 26);
        entities.ChildCaseRole.FcOtherBenefitInd =
          db.GetNullableString(reader, 27);
        entities.ChildCaseRole.FcParentalRights =
          db.GetNullableString(reader, 28);
        entities.ChildCaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 29);
        entities.ChildCaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 30);
        entities.ChildCaseRole.FcPlacementDate = db.GetNullableDate(reader, 31);
        entities.ChildCaseRole.FcPlacementName =
          db.GetNullableString(reader, 32);
        entities.ChildCaseRole.FcPlacementReason =
          db.GetNullableString(reader, 33);
        entities.ChildCaseRole.FcPreviousPa = db.GetNullableString(reader, 34);
        entities.ChildCaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 35);
        entities.ChildCaseRole.FcSourceOfFunding =
          db.GetNullableString(reader, 36);
        entities.ChildCaseRole.FcSrsPayee = db.GetNullableString(reader, 37);
        entities.ChildCaseRole.FcSsa = db.GetNullableString(reader, 38);
        entities.ChildCaseRole.FcSsi = db.GetNullableString(reader, 39);
        entities.ChildCaseRole.FcVaInd = db.GetNullableString(reader, 40);
        entities.ChildCaseRole.FcWardsAccount =
          db.GetNullableString(reader, 41);
        entities.ChildCaseRole.FcZebInd = db.GetNullableString(reader, 42);
        entities.ChildCaseRole.Over18AndInSchool =
          db.GetNullableString(reader, 43);
        entities.ChildCaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 44);
        entities.ChildCaseRole.SpecialtyArea = db.GetNullableString(reader, 45);
        entities.ChildCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 46);
        entities.ChildCaseRole.LastUpdatedBy = db.GetNullableString(reader, 47);
        entities.ChildCaseRole.CreatedTimestamp = db.GetDateTime(reader, 48);
        entities.ChildCaseRole.CreatedBy = db.GetString(reader, 49);
        entities.ChildCaseRole.RelToAr = db.GetNullableString(reader, 50);
        entities.ChildCaseRole.Note = db.GetNullableString(reader, 51);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 52);
        entities.ChildCsePerson.Occupation = db.GetNullableString(reader, 53);
        entities.ChildCsePerson.AeCaseNumber = db.GetNullableString(reader, 54);
        entities.ChildCsePerson.DateOfDeath = db.GetNullableDate(reader, 55);
        entities.ChildCsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 56);
        entities.ChildCsePerson.CurrentSpouseMi =
          db.GetNullableString(reader, 57);
        entities.ChildCsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 58);
        entities.ChildCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 59);
        entities.ChildCsePerson.EmergencyPhone =
          db.GetNullableInt32(reader, 60);
        entities.ChildCsePerson.NameMiddle = db.GetNullableString(reader, 61);
        entities.ChildCsePerson.NameMaiden = db.GetNullableString(reader, 62);
        entities.ChildCsePerson.HomePhone = db.GetNullableInt32(reader, 63);
        entities.ChildCsePerson.OtherNumber = db.GetNullableInt32(reader, 64);
        entities.ChildCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 65);
        entities.ChildCsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 66);
        entities.ChildCsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 67);
        entities.ChildCsePerson.Race = db.GetNullableString(reader, 68);
        entities.ChildCsePerson.HairColor = db.GetNullableString(reader, 69);
        entities.ChildCsePerson.EyeColor = db.GetNullableString(reader, 70);
        entities.ChildCsePerson.Weight = db.GetNullableInt32(reader, 71);
        entities.ChildCsePerson.HeightFt = db.GetNullableInt32(reader, 72);
        entities.ChildCsePerson.HeightIn = db.GetNullableInt32(reader, 73);
        entities.ChildCsePerson.OtherIdInfo = db.GetNullableString(reader, 74);
        entities.ChildCsePerson.BornOutOfWedlock =
          db.GetNullableString(reader, 75);
        entities.ChildCsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 76);
        entities.ChildCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 77);
        entities.ChildCsePerson.DatePaternEstab = db.GetDate(reader, 78);
        entities.ChildCsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 79);
        entities.ChildCsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 80);
        entities.ChildCsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 81);
        entities.ChildCsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 82);
        entities.ChildCsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 83);
        entities.ChildCsePerson.TribalCode = db.GetNullableString(reader, 84);
        entities.ChildCaseRole.Populated = true;
        entities.ChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ChildCaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea",
          entities.ChildCaseRole.SpecialtyArea);
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.ChildCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Child.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ChildCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ChildCsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.ChildCsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.ChildCsePersonAddress.Street1 =
          db.GetNullableString(reader, 4);
        entities.ChildCsePersonAddress.Street2 =
          db.GetNullableString(reader, 5);
        entities.ChildCsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.ChildCsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.ChildCsePersonAddress.WorkerId =
          db.GetNullableString(reader, 8);
        entities.ChildCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 9);
        entities.ChildCsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.ChildCsePersonAddress.EndCode =
          db.GetNullableString(reader, 11);
        entities.ChildCsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.ChildCsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.ChildCsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.ChildCsePersonAddress.CreatedBy =
          db.GetNullableString(reader, 15);
        entities.ChildCsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.ChildCsePersonAddress.ZipCode =
          db.GetNullableString(reader, 17);
        entities.ChildCsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.ChildCsePersonAddress.Zip3 = db.GetNullableString(reader, 19);
        entities.ChildCsePersonAddress.LocationType = db.GetString(reader, 20);
        entities.ChildCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.ChildCsePersonAddress.LocationType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.ChildLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ChildCsePerson.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ChildLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ChildLegalActionPerson.Role = db.GetString(reader, 2);
        entities.ChildLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.ChildLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 4);
        entities.ChildLegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.ChildLegalActionPerson.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
        db.SetNullableDate(
          command, "coverBeginDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    entities.ChildProgram.Populated = false;

    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ChildProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ChildProgram.Code = db.GetString(reader, 1);
        entities.ChildProgram.Populated = true;

        return true;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
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

    private CsePersonsWorkSet child;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Program detail;
    }

    /// <summary>
    /// A value of ChildFc.
    /// </summary>
    [JsonPropertyName("childFc")]
    public CsePersonAddress ChildFc
    {
      get => childFc ??= new();
      set => childFc = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    private CsePersonAddress childFc;
    private Array<GroupGroup> group;
    private Common ae;
    private AbendData abendData;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private CaseRole childCaseRole;
    private CsePerson childCsePerson;
    private Common activeChild;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SuppressZeroDob.
    /// </summary>
    [JsonPropertyName("suppressZeroDob")]
    public Common SuppressZeroDob
    {
      get => suppressZeroDob ??= new();
      set => suppressZeroDob = value;
    }

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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common suppressZeroDob;
    private DateWorkArea current;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildLegalActionPerson.
    /// </summary>
    [JsonPropertyName("childLegalActionPerson")]
    public LegalActionPerson ChildLegalActionPerson
    {
      get => childLegalActionPerson ??= new();
      set => childLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ChildObligationType.
    /// </summary>
    [JsonPropertyName("childObligationType")]
    public ObligationType ChildObligationType
    {
      get => childObligationType ??= new();
      set => childObligationType = value;
    }

    /// <summary>
    /// A value of ChildLegalActionDetail.
    /// </summary>
    [JsonPropertyName("childLegalActionDetail")]
    public LegalActionDetail ChildLegalActionDetail
    {
      get => childLegalActionDetail ??= new();
      set => childLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ChildLegalAction.
    /// </summary>
    [JsonPropertyName("childLegalAction")]
    public LegalAction ChildLegalAction
    {
      get => childLegalAction ??= new();
      set => childLegalAction = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of ChildCsePersonAddress.
    /// </summary>
    [JsonPropertyName("childCsePersonAddress")]
    public CsePersonAddress ChildCsePersonAddress
    {
      get => childCsePersonAddress ??= new();
      set => childCsePersonAddress = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of ChildProgram.
    /// </summary>
    [JsonPropertyName("childProgram")]
    public Program ChildProgram
    {
      get => childProgram ??= new();
      set => childProgram = value;
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

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    private LegalActionPerson childLegalActionPerson;
    private ObligationType childObligationType;
    private LegalActionDetail childLegalActionDetail;
    private LegalAction childLegalAction;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePersonAddress childCsePersonAddress;
    private PersonProgram personProgram;
    private Program childProgram;
    private Case1 case1;
    private CaseRole childCaseRole;
    private CsePerson childCsePerson;
  }
#endregion
}
