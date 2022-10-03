// Program: SI_READ_AP_CASE_ROLE_DETAILS, ID: 371755709, model: 746.
// Short name: SWE01197
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
/// A program: SI_READ_AP_CASE_ROLE_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This process will retrieve details about an
/// AP on a specified case and the role that person plays within the case.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadApCaseRoleDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_AP_CASE_ROLE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadApCaseRoleDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadApCaseRoleDetails.
  /// </summary>
  public SiReadApCaseRoleDetails(IContext context, Import import, Export export):
    
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
    // CSE Person and the role that they play on a
    // specified case.
    // ---------------------------------------------
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03/10/95  Helen Sharland	Initial Development
    // 09/11/96  G. Lofton		Unemployment ind moved from
    // 				income source to cse person
    // 04/29/97  A.Kinney		Changed Current_Date
    // 09/29/98  C Deghand             Added check for other children
    //                                 
    // and added views
    // 10/01/98  C Deghand             Added code to check for other CS orders
    //                                 
    // and added views
    // ------------------------------------------------------------
    // 06/21/99 W.Campbell        Modified the properties
    //                            of 3 READ statements to
    //                            Select Only.
    // ---------------------------------------------------------
    // 06/24/99 W.Campbell        IF stmt with
    //                            escape from READ EACH
    //                            loop added in 2 READ EACH
    //                            statements for
    //                            performance improvement.
    //                            One READ EACH for FA role
    //                            and one READ EACH for
    //                            AR or AP role.
    // -----------------------------------------------------------
    // 06/24/99 W.Campbell        Qualification changed in 2
    //                            READ EACH case_role, case
    //                            statements,
    //                            from end date > current date
    //                            to end date >= current date.
    //                            One READ EACH for FA role
    //                            and one READ EACH for
    //                            AR or AP role.
    // -----------------------------------------------------------
    // 12/29/2000  Madhu Kumar   Changed the logic for display
    // PR # 109005               of the bankruptcy indicator .
    //                           Now the bankruptcy indicator shows as Y
    //                           only if there is a  bankruptcy with
    //                           discharge date greater than the
    //                           current date .
    // -----------------------------------------------------------
    // WR# 010339    Vithal Madhira        Added new attribute '
    // Birthplace_Country'. (11/13/2001)
    // --------------------------------------------------------------
    // 8/29/19  JHarden  CQ66290 Add an indicator for Threats made by CP or NCP 
    // on staff
    // 12/23/2020  GVandy  CQ68785  Add customer service code.
    local.Current.Date = Now().Date;
    local.Zero.Date = null;
    local.ErrOnAdabasUnavailable.Flag = "Y";
    local.SuppressZeroDob.Flag = "Y";
    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCsePerson1())
    {
      MoveCsePerson2(entities.Ap, export.ApCsePerson);

      if (AsChar(entities.Ap.UnemploymentInd) == 'Y')
      {
        export.Uci.Flag = "Y";
      }
      else
      {
        export.Uci.Flag = "N";
      }

      // ---------------------------------------------
      // Read any associated Federal Benefits
      // ---------------------------------------------
      if (AsChar(entities.Ap.FederalInd) == 'Y')
      {
        export.FedBens.Flag = "Y";
      }
      else
      {
        export.FedBens.Flag = "N";
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!IsEmpty(export.ApCsePerson.AeCaseNumber))
    {
      // ---------------------------------------------------------
      // 06/21/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCsePerson2())
      {
        if (Equal(entities.Ar.AeCaseNumber, export.ApCsePerson.AeCaseNumber))
        {
          export.ApCaseRole.LivingWithArIndicator = "Y";
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }

    // ------------------------------------------------------------
    // Read the case seperately to determine whether
    // the Case Role is missing or whether the Case is not found.
    // ------------------------------------------------------------
    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase1())
    {
      ExitState = "CASE_NF";
    }

    if (ReadCaseRole())
    {
      export.ApCaseRole.Assign(entities.CaseRole);
    }
    else
    {
      ExitState = "CASE_ROLE_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Read any associated Military
    // ---------------------------------------------
    if (ReadIncomeSource())
    {
      export.Military.Flag = "Y";
    }
    else
    {
      export.Military.Flag = "N";
    }

    // ---------------------------------------------
    // Read any associated Incarcerations
    // ---------------------------------------------
    if (ReadIncarceration())
    {
      export.Incarceration.Flag = "Y";
    }
    else
    {
      export.Incarceration.Flag = "N";
    }

    // ---------------------------------------------
    // Read any associated bankruptcies
    // ---------------------------------------------
    export.Bankruptcy.Flag = "N";

    if (ReadBankruptcy())
    {
      export.Bankruptcy.Flag = "Y";
    }

    // -----------------------------------------------------------
    // Check to see if the AP has other children (added 09/29/98)
    // -----------------------------------------------------------
    local.Common.Count = 0;

    // -----------------------------------------------------------
    // 06/24/99 W.Campbell - Qualification changed
    // in the following READ EACH stmt,
    // from end date > current date
    // to end date >= current date.
    // -----------------------------------------------------------
    foreach(var item in ReadCaseRoleCase1())
    {
      ++local.Common.Count;

      // -----------------------------------------------------------
      // 06/24/99 W.Campbell - Following IF stmt with
      // escape from READ EACH loop added for
      // performance improvement.
      // -----------------------------------------------------------
      if (local.Common.Count > 1)
      {
        break;
      }
    }

    if (local.Common.Count > 1)
    {
      export.OtherChildren.Flag = "Y";
    }
    else
    {
      export.OtherChildren.Flag = "N";
    }

    // ---------------------------------------------
    // Read driver's license information
    // ---------------------------------------------
    if (ReadCsePersonLicense())
    {
      export.CsePersonLicense.Assign(entities.CsePersonLicense);
    }
    else
    {
      // --------------------------------------------
      // This is okay.  Driver's license not captured
      // yet
      // --------------------------------------------
    }

    // ---------------------------------------------
    // Check if the AP is on multiple cases.
    // --------------------------------------------
    local.Common.Count = 0;

    // -----------------------------------------------------------
    // 06/24/99 W.Campbell - Qualification changed
    // in the following READ EACH stmt,
    // from end date > current date
    // to end date >= current date.
    // -----------------------------------------------------------
    foreach(var item in ReadCaseRoleCase2())
    {
      ++local.Common.Count;

      // -----------------------------------------------------------
      // 06/24/99 W.Campbell - Following IF stmt with
      // escape from READ EACH loop added for
      // performance improvement.
      // -----------------------------------------------------------
      if (local.Common.Count > 1)
      {
        break;
      }
    }

    if (local.Common.Count > 1)
    {
      export.MultipleCases.Flag = "Y";
    }
    else
    {
      export.MultipleCases.Flag = "N";
    }

    // ----------------------------------------------------------
    // Check to see if the AP has other CS orders (added 10/01/98)
    // ----------------------------------------------------------
    if (AsChar(export.MultipleCases.Flag) == 'Y')
    {
      local.Common.Count = 0;

      foreach(var item in ReadLegalAction())
      {
        foreach(var item1 in ReadCase2())
        {
          if (Equal(entities.Case1.Number, import.Case1.Number))
          {
            goto ReadEach;
          }
          else
          {
            ++local.Common.Count;
          }
        }

ReadEach:
        ;
      }

      if (local.Common.Count >= 1)
      {
        export.OtherCsOrders.Flag = "Y";
      }
      else
      {
        export.OtherCsOrders.Flag = "N";
      }
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
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TribalCode = source.TribalCode;
    target.ThreatOnStaff = source.ThreatOnStaff;
    target.CustomerServiceCode = source.CustomerServiceCode;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Ap.Number;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson1(useExport.CsePerson, export.ApCsePerson);
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "dischargeDate1", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "dischargeDate2", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 2);
        entities.Bankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 3);
        entities.Bankruptcy.Populated = true;
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
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

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApCsePerson.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 20);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 22);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 23);
        entities.CaseRole.CreatedBy = db.GetString(reader, 24);
        entities.CaseRole.Note = db.GetNullableString(reader, 25);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase1()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 20);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 22);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 23);
        entities.CaseRole.CreatedBy = db.GetString(reader, 24);
        entities.CaseRole.Note = db.GetNullableString(reader, 25);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase2()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 20);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 22);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 23);
        entities.CaseRole.CreatedBy = db.GetString(reader, 24);
        entities.CaseRole.Note = db.GetNullableString(reader, 25);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Type1 = db.GetString(reader, 1);
        entities.Ap.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.Ap.UnemploymentInd = db.GetNullableString(reader, 3);
        entities.Ap.FederalInd = db.GetNullableString(reader, 4);
        entities.Ap.BirthplaceCountry = db.GetNullableString(reader, 5);
        entities.Ap.TribalCode = db.GetNullableString(reader, 6);
        entities.Ap.ThreatOnStaff = db.GetNullableString(reader, 7);
        entities.Ap.CustomerServiceCode = db.GetNullableString(reader, 8);
        entities.Ap.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ap.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.Ar.TribalCode = db.GetNullableString(reader, 3);
        entities.Ar.ThreatOnStaff = db.GetNullableString(reader, 4);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "expirationDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.IssuingAgencyName =
          db.GetNullableString(reader, 3);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 4);
        entities.CsePersonLicense.ExpirationDt = db.GetNullableDate(reader, 5);
        entities.CsePersonLicense.StartDt = db.GetNullableDate(reader, 6);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonLicense.Description = db.GetNullableString(reader, 8);
        entities.CsePersonLicense.Note = db.GetNullableString(reader, 9);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(command, "endDate", date);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.Ap.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OtherCsOrders.
    /// </summary>
    [JsonPropertyName("otherCsOrders")]
    public Common OtherCsOrders
    {
      get => otherCsOrders ??= new();
      set => otherCsOrders = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
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
    /// A value of MultipleCases.
    /// </summary>
    [JsonPropertyName("multipleCases")]
    public Common MultipleCases
    {
      get => multipleCases ??= new();
      set => multipleCases = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of FedBens.
    /// </summary>
    [JsonPropertyName("fedBens")]
    public Common FedBens
    {
      get => fedBens ??= new();
      set => fedBens = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    private Common otherCsOrders;
    private Common otherChildren;
    private Common ae;
    private AbendData abendData;
    private Common multipleCases;
    private CsePersonLicense csePersonLicense;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common bankruptcy;
    private Common fedBens;
    private Common incarceration;
    private Common military;
    private Common uci;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private DateWorkArea zero;
    private Common suppressZeroDob;
    private DateWorkArea current;
    private Common errOnAdabasUnavailable;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private LegalActionCaseRole legalActionCaseRole;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private CsePersonLicense csePersonLicense;
    private IncomeSource incomeSource;
    private Bankruptcy bankruptcy;
    private Incarceration incarceration;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ap;
    private CsePerson ar;
  }
#endregion
}
