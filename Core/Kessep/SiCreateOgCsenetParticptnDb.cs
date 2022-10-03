// Program: SI_CREATE_OG_CSENET_PARTICPTN_DB, ID: 372382222, model: 746.
// Short name: SWE01614
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
/// A program: SI_CREATE_OG_CSENET_PARTICPTN_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the entity type that contains Interstate (CSENet) 
/// information about the participants in the Case.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetParticptnDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_PARTICPTN_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetParticptnDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetParticptnDb.
  /// </summary>
  public SiCreateOgCsenetParticptnDb(IContext context, Import import,
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
    // ***************************************************************
    // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
    // 3/20/2000    D. Jean	 IDCR#160 - Change paternity attribute
    // used from Child to CSE Person
    // 04/07/2000   C. Scroggins Added check for individual tagged with family 
    // violence.
    // 05/09/2000   C. Scroggins Re-made changes that had been undone for 
    // Paternity.
    // **************************************************************
    local.CurrentDate.Date = Now().Date;

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // **************************************************************
    // ***   Create the Participation_DB for the AP in the Case   ***
    // **************************************************************
    if (ReadAbsentParentCsePerson())
    {
      local.InterstateParticipant.Relationship =
        entities.ParticipantAbsentParent.Type1;

      if (!Lt(local.CurrentDate.Date, entities.ParticipantAbsentParent.StartDate)
        && Lt
        (local.CurrentDate.Date, entities.ParticipantAbsentParent.EndDate))
      {
        local.InterstateParticipant.Status = "O";
      }
      else
      {
        local.InterstateParticipant.Status = "C";
      }
    }
    else
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    local.ParticipationCsePersonsWorkSet.Number =
      entities.ParticipantCsePerson.Number;
    UseSiReadCsePerson();
    local.InterstateParticipant.NameLast =
      local.ParticipationCsePersonsWorkSet.LastName;
    local.InterstateParticipant.NameFirst =
      local.ParticipationCsePersonsWorkSet.FirstName;

    if (IsEmpty(local.ParticipationCsePerson.NameMiddle))
    {
      local.InterstateParticipant.NameMiddle =
        local.ParticipationCsePersonsWorkSet.MiddleInitial;
    }
    else
    {
      local.InterstateParticipant.NameMiddle =
        local.ParticipationCsePerson.NameMaiden ?? "";
    }

    local.InterstateParticipant.NameSuffix = "";
    local.InterstateParticipant.DateOfBirth =
      local.ParticipationCsePersonsWorkSet.Dob;
    local.InterstateParticipant.Ssn = local.ParticipationCsePersonsWorkSet.Ssn;
    local.InterstateParticipant.Sex = local.ParticipationCsePersonsWorkSet.Sex;
    local.InterstateParticipant.Race = local.ParticipationCsePerson.Race ?? "";
    local.InterstateParticipant.DependentRelationCp = "";

    // ***************************************************************
    // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
    // **************************************************************
    if (ReadCsePersonAddress())
    {
      local.InterstateParticipant.AddressLine1 =
        entities.CsePersonAddress.Street1;
      local.InterstateParticipant.AddressLine2 =
        entities.CsePersonAddress.Street2;
      local.InterstateParticipant.City = entities.CsePersonAddress.City;
      local.InterstateParticipant.State = entities.CsePersonAddress.State;
      local.InterstateParticipant.ZipCode5 = entities.CsePersonAddress.ZipCode;
      local.InterstateParticipant.ZipCode4 = entities.CsePersonAddress.Zip4;
      local.InterstateParticipant.AddressVerifiedDate =
        entities.CsePersonAddress.VerifiedDate;

      if (Lt(Now().Date.AddYears(-1), entities.CsePersonAddress.VerifiedDate))
      {
        local.InterstateParticipant.AddressConfirmedInd = "Y";
      }
      else
      {
        local.InterstateParticipant.AddressConfirmedInd = "N";
      }

      // ---------------------------------------------------------------------------------------
      // If family violence indicator is set, set address/phone number 
      // information to spaces. CLS. 04/10/00
      // ---------------------------------------------------------------------------------------
      if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
      {
        local.InterstateParticipant.AddressLine1 = "";
        local.InterstateParticipant.AddressLine2 = "";
        local.InterstateParticipant.City = "";
        local.InterstateParticipant.State = "";
        local.InterstateParticipant.ZipCode5 = "";
        local.InterstateParticipant.ZipCode4 = "";
        local.InterstateParticipant.AddressVerifiedDate =
          entities.CsePersonAddress.VerifiedDate;
      }
    }

    local.InterstateParticipant.PlaceOfBirth =
      entities.ParticipantCsePerson.BirthPlaceCity;
    local.InterstateParticipant.WorkAreaCode =
      NumberToString(entities.ParticipantCsePerson.WorkPhoneAreaCode.
        GetValueOrDefault(), 3);
    local.InterstateParticipant.WorkPhone =
      NumberToString(entities.ParticipantCsePerson.WorkPhone.
        GetValueOrDefault(), 7);

    // ---------------------------------------------------------------------------------------
    // If family violence indicator is set, set address/phone number information
    // to spaces. CLS. 04/10/00
    // ---------------------------------------------------------------------------------------
    if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
    {
      local.InterstateParticipant.WorkAreaCode = "";
      local.InterstateParticipant.WorkPhone = "";
    }

    if (ReadEmployerEmployerAddressIncomeSource())
    {
      if (IsEmpty(entities.Employer.Ein))
      {
        local.InterstateParticipant.EmployerEin = 0;
      }
      else if (Verify(entities.Employer.Ein, "1234567890") > 0)
      {
        local.InterstateParticipant.EmployerEin = 0;
      }
      else
      {
        local.InterstateParticipant.EmployerEin =
          (int?)StringToNumber(entities.Employer.Ein);
      }

      local.InterstateParticipant.EmployerName = entities.Employer.Name;
      local.InterstateParticipant.EmployerAddressLine1 =
        entities.EmployerAddress.Street1;
      local.InterstateParticipant.EmployerAddressLine2 =
        entities.EmployerAddress.Street2;
      local.InterstateParticipant.EmployerCity = entities.EmployerAddress.City;
      local.InterstateParticipant.EmployerState =
        entities.EmployerAddress.State;
      local.InterstateParticipant.EmployerZipCode5 =
        entities.EmployerAddress.ZipCode;
      local.InterstateParticipant.EmployerZipCode4 =
        entities.EmployerAddress.Zip4;
      local.InterstateParticipant.EmployerVerifiedDate =
        entities.IncomeSource.ReturnDt;

      // ---------------------------------------------------------------------------------------
      // If family violence indicator is set, set employer name, address, and 
      // phone number information to spaces. CLS. 04/10/00
      // ---------------------------------------------------------------------------------------
      if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
      {
        local.InterstateParticipant.EmployerName = "";
        local.InterstateParticipant.EmployerAddressLine1 = "";
        local.InterstateParticipant.EmployerAddressLine2 = "";
        local.InterstateParticipant.EmployerCity = "";
        local.InterstateParticipant.EmployerState = "";
        local.InterstateParticipant.EmployerZipCode5 = "";
        local.InterstateParticipant.EmployerZipCode4 = "";
        local.InterstateParticipant.EmployerEin = 0;
      }

      if (Lt(Now().Date.AddYears(-1), entities.IncomeSource.ReturnDt))
      {
        local.InterstateParticipant.EmployerConfirmedInd = "Y";
      }
      else
      {
        local.InterstateParticipant.EmployerConfirmedInd = "N";
      }
    }

    UseSiCreateInterstateParticipant();

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      export.InterstateCase.ParticipantDataInd =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;
    }
    else
    {
      return;
    }

    MoveInterstateParticipant(local.Refresh, local.InterstateParticipant);

    // **************************************************************
    // ***   Create the Participation_DB for the AR in the Case   ***
    // **************************************************************
    if (ReadApplicantRecipientCsePerson())
    {
      local.InterstateParticipant.Relationship =
        entities.ParticipantApplicantRecipient.Type1;

      if (!Lt(Now().Date, entities.ParticipantApplicantRecipient.StartDate) && Lt
        (Now().Date, entities.ParticipantApplicantRecipient.EndDate))
      {
        local.InterstateParticipant.Status = "O";
      }
      else
      {
        local.InterstateParticipant.Status = "C";
      }
    }
    else
    {
      ExitState = "AR_DB_ERROR_NF";

      return;
    }

    local.ParticipationCsePersonsWorkSet.Number =
      entities.ParticipantCsePerson.Number;
    UseSiReadCsePerson();
    local.InterstateParticipant.NameLast =
      local.ParticipationCsePersonsWorkSet.LastName;
    local.InterstateParticipant.NameFirst =
      local.ParticipationCsePersonsWorkSet.FirstName;

    if (IsEmpty(local.ParticipationCsePerson.NameMiddle))
    {
      local.InterstateParticipant.NameMiddle =
        local.ParticipationCsePersonsWorkSet.MiddleInitial;
    }
    else
    {
      local.InterstateParticipant.NameMiddle =
        local.ParticipationCsePerson.NameMaiden ?? "";
    }

    local.InterstateParticipant.NameSuffix = "";
    local.InterstateParticipant.DateOfBirth =
      local.ParticipationCsePersonsWorkSet.Dob;
    local.InterstateParticipant.Ssn = local.ParticipationCsePersonsWorkSet.Ssn;
    local.InterstateParticipant.Sex = local.ParticipationCsePersonsWorkSet.Sex;
    local.InterstateParticipant.Race = local.ParticipationCsePerson.Race ?? "";
    local.InterstateParticipant.DependentRelationCp = "";

    // ***************************************************************
    // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
    // **************************************************************
    if (ReadCsePersonAddress())
    {
      local.InterstateParticipant.AddressLine1 =
        entities.CsePersonAddress.Street1;
      local.InterstateParticipant.AddressLine2 =
        entities.CsePersonAddress.Street2;
      local.InterstateParticipant.City = entities.CsePersonAddress.City;
      local.InterstateParticipant.State = entities.CsePersonAddress.State;
      local.InterstateParticipant.ZipCode5 = entities.CsePersonAddress.ZipCode;
      local.InterstateParticipant.ZipCode4 = entities.CsePersonAddress.Zip4;
      local.InterstateParticipant.AddressVerifiedDate =
        entities.CsePersonAddress.VerifiedDate;

      // ---------------------------------------------------------------------------------------
      // If family violence indicator is set, set address/phone number 
      // information to spaces. CLS. 04/10/00
      // ---------------------------------------------------------------------------------------
      if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
      {
        local.InterstateParticipant.AddressLine1 = "";
        local.InterstateParticipant.AddressLine2 = "";
        local.InterstateParticipant.City = "";
        local.InterstateParticipant.State = "";
        local.InterstateParticipant.ZipCode5 = "";
        local.InterstateParticipant.ZipCode4 = "";
      }
    }

    local.InterstateParticipant.PlaceOfBirth =
      entities.ParticipantCsePerson.BirthPlaceCity;
    local.InterstateParticipant.WorkAreaCode =
      NumberToString(entities.ParticipantCsePerson.WorkPhoneAreaCode.
        GetValueOrDefault(), 3);
    local.InterstateParticipant.WorkPhone =
      NumberToString(entities.ParticipantCsePerson.WorkPhone.
        GetValueOrDefault(), 7);

    // ---------------------------------------------------------------------------------------
    // If family violence indicator is set, set address/phone number information
    // to spaces. CLS. 04/10/00
    // ---------------------------------------------------------------------------------------
    if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
    {
      local.InterstateParticipant.WorkAreaCode = "";
      local.InterstateParticipant.WorkPhone = "";
    }

    if (ReadEmployerEmployerAddressIncomeSource())
    {
      if (IsEmpty(entities.Employer.Ein))
      {
        local.InterstateParticipant.EmployerEin = 0;
      }
      else if (Verify(entities.Employer.Ein, "1234567890") > 0)
      {
        local.InterstateParticipant.EmployerEin = 0;
      }
      else
      {
        local.InterstateParticipant.EmployerEin =
          (int?)StringToNumber(entities.Employer.Ein);
      }

      local.InterstateParticipant.EmployerName = entities.Employer.Name;
      local.InterstateParticipant.EmployerAddressLine1 =
        entities.EmployerAddress.Street1;
      local.InterstateParticipant.EmployerAddressLine2 =
        entities.EmployerAddress.Street2;
      local.InterstateParticipant.EmployerCity = entities.EmployerAddress.City;
      local.InterstateParticipant.EmployerState =
        entities.EmployerAddress.State;
      local.InterstateParticipant.EmployerZipCode5 =
        entities.EmployerAddress.ZipCode;
      local.InterstateParticipant.EmployerZipCode4 =
        entities.EmployerAddress.Zip4;
      local.InterstateParticipant.EmployerVerifiedDate =
        entities.IncomeSource.ReturnDt;

      // ---------------------------------------------------------------------------------------
      // If family violence indicator is set, set employer name, address, and 
      // phone number information to spaces. CLS. 04/10/00
      // ---------------------------------------------------------------------------------------
      if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
      {
        local.InterstateParticipant.EmployerName = "";
        local.InterstateParticipant.EmployerAddressLine1 = "";
        local.InterstateParticipant.EmployerAddressLine2 = "";
        local.InterstateParticipant.EmployerCity = "";
        local.InterstateParticipant.EmployerState = "";
        local.InterstateParticipant.EmployerZipCode5 = "";
        local.InterstateParticipant.EmployerZipCode4 = "";
        local.InterstateParticipant.EmployerEin = 0;
      }
    }

    UseSiCreateInterstateParticipant();

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      export.InterstateCase.ParticipantDataInd =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;
    }
    else
    {
      return;
    }

    // *******************************************************************************
    // ***   Create the Participation_DB for the Children referred with the Case
    // ***
    // *******************************************************************************
    for(import.Participant.Index = 0; import.Participant.Index < import
      .Participant.Count; ++import.Participant.Index)
    {
      MoveInterstateParticipant(local.Refresh, local.InterstateParticipant);

      if (AsChar(import.Participant.Item.Sel.SelectChar) == 'S')
      {
        if (ReadChildCsePerson())
        {
          local.InterstateParticipant.Relationship =
            entities.ParticipantChild.Type1;
          local.InterstateParticipant.DependentRelationCp =
            entities.ParticipantChild.RelToAr;

          if (!Lt(Now().Date, entities.ParticipantChild.StartDate) && Lt
            (Now().Date, entities.ParticipantChild.EndDate))
          {
            local.InterstateParticipant.Status = "O";
          }
          else
          {
            local.InterstateParticipant.Status = "C";
          }
        }
        else
        {
          ExitState = "CASE_ROLE_CHILD_NF";

          return;
        }

        local.ParticipationCsePersonsWorkSet.Number =
          entities.ParticipantCsePerson.Number;
        UseSiReadCsePerson();
        local.InterstateParticipant.NameLast =
          local.ParticipationCsePersonsWorkSet.LastName;
        local.InterstateParticipant.NameFirst =
          local.ParticipationCsePersonsWorkSet.FirstName;

        if (IsEmpty(local.ParticipationCsePerson.NameMiddle))
        {
          local.InterstateParticipant.NameMiddle =
            local.ParticipationCsePersonsWorkSet.MiddleInitial;
        }
        else
        {
          local.InterstateParticipant.NameMiddle =
            local.ParticipationCsePerson.NameMaiden ?? "";
        }

        local.InterstateParticipant.NameSuffix = "";
        local.InterstateParticipant.DateOfBirth =
          local.ParticipationCsePersonsWorkSet.Dob;
        local.InterstateParticipant.Ssn =
          local.ParticipationCsePersonsWorkSet.Ssn;
        local.InterstateParticipant.Sex =
          local.ParticipationCsePersonsWorkSet.Sex;
        local.InterstateParticipant.Race =
          local.ParticipationCsePerson.Race ?? "";

        // ***************************************************************
        // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
        // **************************************************************
        if (ReadCsePersonAddress())
        {
          local.InterstateParticipant.AddressLine1 =
            entities.CsePersonAddress.Street1;
          local.InterstateParticipant.AddressLine2 =
            entities.CsePersonAddress.Street2;
          local.InterstateParticipant.City = entities.CsePersonAddress.City;
          local.InterstateParticipant.State = entities.CsePersonAddress.State;
          local.InterstateParticipant.ZipCode5 =
            entities.CsePersonAddress.ZipCode;
          local.InterstateParticipant.ZipCode4 = entities.CsePersonAddress.Zip4;
          local.InterstateParticipant.AddressVerifiedDate =
            entities.CsePersonAddress.VerifiedDate;

          // ---------------------------------------------------------------------------------------
          // If family violence indicator is set, set address/phone number 
          // information to spaces. CLS. 04/10/00
          // ---------------------------------------------------------------------------------------
          if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
          {
            local.InterstateParticipant.AddressLine1 = "";
            local.InterstateParticipant.AddressLine2 = "";
            local.InterstateParticipant.City = "";
            local.InterstateParticipant.State = "";
            local.InterstateParticipant.ZipCode5 = "";
            local.InterstateParticipant.ZipCode4 = "";
          }
        }

        local.InterstateParticipant.PlaceOfBirth =
          entities.ParticipantCsePerson.BirthPlaceCity;
        local.InterstateParticipant.WorkAreaCode =
          NumberToString(entities.ParticipantCsePerson.WorkPhoneAreaCode.
            GetValueOrDefault(), 3);
        local.InterstateParticipant.WorkPhone =
          NumberToString(entities.ParticipantCsePerson.WorkPhone.
            GetValueOrDefault(), 7);
        local.InterstateParticipant.ChildStateOfResidence =
          entities.CsePersonAddress.State;
        local.InterstateParticipant.ChildPaternityStatus =
          entities.ParticipantCsePerson.PaternityEstablishedIndicator;

        // ---------------------------------------------------------------------------------------
        // If family violence indicator is set, set address/phone number 
        // information to spaces. CLS. 04/10/00
        // ---------------------------------------------------------------------------------------
        if (!IsEmpty(entities.ParticipantCsePerson.FamilyViolenceIndicator))
        {
          local.InterstateParticipant.WorkAreaCode = "";
          local.InterstateParticipant.WorkPhone = "";
        }

        UseSiCreateInterstateParticipant();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.InterstateCase.ParticipantDataInd =
            export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;
        }
        else
        {
          return;
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
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
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateParticipant(InterstateParticipant source,
    InterstateParticipant target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.NameSuffix = source.NameSuffix;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.Sex = source.Sex;
    target.Race = source.Race;
    target.Relationship = source.Relationship;
    target.Status = source.Status;
    target.DependentRelationCp = source.DependentRelationCp;
    target.AddressLine1 = source.AddressLine1;
    target.AddressLine2 = source.AddressLine2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.EmployerAddressLine1 = source.EmployerAddressLine1;
    target.EmployerAddressLine2 = source.EmployerAddressLine2;
    target.EmployerCity = source.EmployerCity;
    target.EmployerState = source.EmployerState;
    target.EmployerZipCode5 = source.EmployerZipCode5;
    target.EmployerZipCode4 = source.EmployerZipCode4;
    target.EmployerName = source.EmployerName;
    target.EmployerEin = source.EmployerEin;
    target.AddressVerifiedDate = source.AddressVerifiedDate;
    target.EmployerVerifiedDate = source.EmployerVerifiedDate;
    target.WorkPhone = source.WorkPhone;
    target.WorkAreaCode = source.WorkAreaCode;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.ChildStateOfResidence = source.ChildStateOfResidence;
    target.ChildPaternityStatus = source.ChildPaternityStatus;
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    useImport.InterstateParticipant.Assign(local.InterstateParticipant);
    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);

    local.InterstateParticipant.Assign(useExport.InterstateParticipant);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      local.ParticipationCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ParticipationCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, local.ParticipationCsePerson);
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.ParticipantCsePerson.Populated = false;
    entities.ParticipantAbsentParent.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Referred.Number);
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ParticipantAbsentParent.CasNumber = db.GetString(reader, 0);
        entities.ParticipantAbsentParent.CspNumber = db.GetString(reader, 1);
        entities.ParticipantCsePerson.Number = db.GetString(reader, 1);
        entities.ParticipantAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ParticipantAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ParticipantAbsentParent.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ParticipantAbsentParent.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ParticipantCsePerson.Type1 = db.GetString(reader, 6);
        entities.ParticipantCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 7);
        entities.ParticipantCsePerson.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.ParticipantCsePerson.WorkPhone =
          db.GetNullableInt32(reader, 9);
        entities.ParticipantCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 10);
        entities.ParticipantCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 11);
        entities.ParticipantCsePerson.Populated = true;
        entities.ParticipantAbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParticipantAbsentParent.Type1);
        CheckValid<CsePerson>("Type1", entities.ParticipantCsePerson.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson()
  {
    entities.ParticipantCsePerson.Populated = false;
    entities.ParticipantApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipientCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Referred.Number);
      },
      (db, reader) =>
      {
        entities.ParticipantApplicantRecipient.CasNumber =
          db.GetString(reader, 0);
        entities.ParticipantApplicantRecipient.CspNumber =
          db.GetString(reader, 1);
        entities.ParticipantCsePerson.Number = db.GetString(reader, 1);
        entities.ParticipantApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ParticipantApplicantRecipient.Identifier =
          db.GetInt32(reader, 3);
        entities.ParticipantApplicantRecipient.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ParticipantApplicantRecipient.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ParticipantCsePerson.Type1 = db.GetString(reader, 6);
        entities.ParticipantCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 7);
        entities.ParticipantCsePerson.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 8);
        entities.ParticipantCsePerson.WorkPhone =
          db.GetNullableInt32(reader, 9);
        entities.ParticipantCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 10);
        entities.ParticipantCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 11);
        entities.ParticipantCsePerson.Populated = true;
        entities.ParticipantApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1",
          entities.ParticipantApplicantRecipient.Type1);
        CheckValid<CsePerson>("Type1", entities.ParticipantCsePerson.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Referred.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Referred.Number = db.GetString(reader, 0);
        entities.Referred.Populated = true;
      });
  }

  private bool ReadChildCsePerson()
  {
    entities.ParticipantCsePerson.Populated = false;
    entities.ParticipantChild.Populated = false;

    return Read("ReadChildCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Referred.Number);
        db.SetString(command, "numb", import.Participant.Item.Person.Number);
      },
      (db, reader) =>
      {
        entities.ParticipantChild.CasNumber = db.GetString(reader, 0);
        entities.ParticipantChild.CspNumber = db.GetString(reader, 1);
        entities.ParticipantCsePerson.Number = db.GetString(reader, 1);
        entities.ParticipantChild.Type1 = db.GetString(reader, 2);
        entities.ParticipantChild.Identifier = db.GetInt32(reader, 3);
        entities.ParticipantChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ParticipantChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ParticipantChild.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 6);
        entities.ParticipantChild.RelToAr = db.GetNullableString(reader, 7);
        entities.ParticipantCsePerson.Type1 = db.GetString(reader, 8);
        entities.ParticipantCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 9);
        entities.ParticipantCsePerson.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.ParticipantCsePerson.WorkPhone =
          db.GetNullableInt32(reader, 11);
        entities.ParticipantCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 12);
        entities.ParticipantCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 13);
        entities.ParticipantCsePerson.Populated = true;
        entities.ParticipantChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ParticipantChild.Type1);
        CheckValid<CsePerson>("Type1", entities.ParticipantCsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.ParticipantCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 10);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadEmployerEmployerAddressIncomeSource()
  {
    entities.IncomeSource.Populated = false;
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddressIncomeSource",
      (db, command) =>
      {
        db.
          SetString(command, "cspINumber", entities.ParticipantCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.EmployerAddress.LocationType = db.GetString(reader, 3);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 4);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.City = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 7);
        entities.EmployerAddress.State = db.GetNullableString(reader, 8);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 9);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 10);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 11);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 12);
        entities.IncomeSource.CspINumber = db.GetString(reader, 13);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 14);
        entities.IncomeSource.Populated = true;
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
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
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Person.
      /// </summary>
      [JsonPropertyName("person")]
      public CsePersonsWorkSet Person
      {
        get => person ??= new();
        set => person = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common sel;
      private CsePersonsWorkSet person;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

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
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    private Case1 case1;
    private CsePerson ap;
    private InterstateCase interstateCase;
    private Array<ParticipantGroup> participant;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Refresh.
    /// </summary>
    [JsonPropertyName("refresh")]
    public InterstateParticipant Refresh
    {
      get => refresh ??= new();
      set => refresh = value;
    }

    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    /// <summary>
    /// A value of ParticipationCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("participationCsePersonsWorkSet")]
    public CsePersonsWorkSet ParticipationCsePersonsWorkSet
    {
      get => participationCsePersonsWorkSet ??= new();
      set => participationCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ParticipationCsePerson.
    /// </summary>
    [JsonPropertyName("participationCsePerson")]
    public CsePerson ParticipationCsePerson
    {
      get => participationCsePerson ??= new();
      set => participationCsePerson = value;
    }

    private DateWorkArea currentDate;
    private InterstateParticipant refresh;
    private InterstateParticipant interstateParticipant;
    private CsePersonsWorkSet participationCsePersonsWorkSet;
    private CsePerson participationCsePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Referred.
    /// </summary>
    [JsonPropertyName("referred")]
    public Case1 Referred
    {
      get => referred ??= new();
      set => referred = value;
    }

    /// <summary>
    /// A value of ParticipantCsePerson.
    /// </summary>
    [JsonPropertyName("participantCsePerson")]
    public CsePerson ParticipantCsePerson
    {
      get => participantCsePerson ??= new();
      set => participantCsePerson = value;
    }

    /// <summary>
    /// A value of ParticipantAbsentParent.
    /// </summary>
    [JsonPropertyName("participantAbsentParent")]
    public CaseRole ParticipantAbsentParent
    {
      get => participantAbsentParent ??= new();
      set => participantAbsentParent = value;
    }

    /// <summary>
    /// A value of ParticipantApplicantRecipient.
    /// </summary>
    [JsonPropertyName("participantApplicantRecipient")]
    public CaseRole ParticipantApplicantRecipient
    {
      get => participantApplicantRecipient ??= new();
      set => participantApplicantRecipient = value;
    }

    /// <summary>
    /// A value of ParticipantChild.
    /// </summary>
    [JsonPropertyName("participantChild")]
    public CaseRole ParticipantChild
    {
      get => participantChild ??= new();
      set => participantChild = value;
    }

    private IncomeSource incomeSource;
    private EmployerAddress employerAddress;
    private Employer employer;
    private CsePersonAddress csePersonAddress;
    private Case1 referred;
    private CsePerson participantCsePerson;
    private CaseRole participantAbsentParent;
    private CaseRole participantApplicantRecipient;
    private CaseRole participantChild;
  }
#endregion
}
