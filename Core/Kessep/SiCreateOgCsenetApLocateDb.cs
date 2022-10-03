// Program: SI_CREATE_OG_CSENET_AP_LOCATE_DB, ID: 372382226, model: 746.
// Short name: SWE01140
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
/// A program: SI_CREATE_OG_CSENET_AP_LOCATE_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the entity type that contains Interstate (CSENet) 
/// information about a AP's current and previous employers and addresses.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetApLocateDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_AP_LOCATE_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetApLocateDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetApLocateDb.
  /// </summary>
  public SiCreateOgCsenetApLocateDb(IContext context, Import import,
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
    // **************************************************************
    // 4/29/97		SHERAZ MALIK	CHANGE CURRENT_DATE
    // 2/16/99         Carl Ott        IDCR # 501, add new attributes
    // **************************************************************
    // **************************************************************
    // 8/5/99  C. Ott   Validation that either employer or address
    // information is provided for a "successful" action reason
    // code.
    // 01/06/99	PMcElderry
    // PR # 84397 - Changes to READ EACH for
    // Employment-Employer-Domestic_Employer_Address;
    // Interstate_ap_locate employer effective_date settings.
    // For the hardcoded date, many max date values in KESSEP
    // have max values is defaulted to 12-31-2099; end_date for
    // EMPLOYMENT is one such example.  If the date is not
    // that value, a change has been made and the record has
    // been end dated.
    // If applicable employer infomation is not found, the DB will
    // be created but all employer info will be null.
    // 01/28/00	PMcElderry
    // PR # 84721 - automated transactions created from ADDR not
    // using correct address as specified by USER.
    // Various - changed READs to READ EACHes when looking for
    // last known addresses.
    // 02/22/00	PMcElderry
    // PR # 84721 - removed 1 year parameter when setting
    // verified date indicator as no business rule supported this
    // date.  After speaking to the state csenet contact, Jolene
    // Bickel this was decided upon - the date is considered valid
    // until ended.
    // 03/30/00	PMcElderry
    // PR # 92066 - Obtain the most current verified,
    // non-end-dated residential and mailing addresses for all
    // csenet action codes
    // 04/12/00	CScroggins
    // WR # 00162 - Added code to not send address and
    // phone number information if the individual is marked
    // for family violence.
    // 08/14/00	PMcElderry
    // PR # 100337
    // Regardless of transaction, only the domestic address is being
    // retrieved.  If a change is being made to a foreign address,
    // the foreign address needs to show up on the transaction.
    // NOTE - the standard for this has not been decided upon but
    // the necessary coding, IDCR withstanding for the addition of
    // the 3 needed attributes of "Province", "Postal_Code", and
    // "County", is complete.
    // **************************************************************
    // **********************************************************************
    // 04/17/02 MAshworth
    // PR # 143017 - CSENet outgoing error report showed error message invalid 
    // Employer2 Phone Number.  There were 000 in the area code and the rest of
    // the number was blank.  Only set area code to 000 if phone number is
    // populated.
    // **********************************************************************
    local.Current.Date = Now().Date;
    local.ScreenIdentification.Command = import.ScreenIndentification.Command;
    UseOeCabSetMnemonics();

    if (ReadInterstateCaseInterstateApIdentification())
    {
      if (ReadCsePerson())
      {
        local.ApCsePersonsWorkSet.Number = entities.Ap.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    if (Equal(local.ScreenIdentification.Command, "FADS"))
    {
      // ---------------
      // Beg PR # 100337
      // ---------------
      // -------------------------------------------------------
      // Get the most current non end-dated non-domestic
      // RESIDENTIAL ADDRESS
      // -------------------------------------------------------
      if (ReadCsePersonAddress8())
      {
        local.InterstateApLocate.ResidentialAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.ResidentialAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.ResidentialCity =
          entities.CsePersonAddress.City;
        local.InterstateApLocate.ResidentialState =
          entities.CsePersonAddress.State;
        local.InterstateApLocate.ResidentialZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.ResidentialZipCode4 =
          entities.CsePersonAddress.Zip4;

        if (Lt(new DateTime(1, 1, 1), entities.CsePersonAddress.VerifiedDate))
        {
          local.InterstateApLocate.ResidentialAddressEffectvDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          local.InterstateApLocate.ResidentialAddressEffectvDate =
            Date(entities.CsePersonAddress.Identifier);
        }

        if (!Equal(entities.CsePersonAddress.EndDate,
          local.MaxDate.ExpirationDate))
        {
          local.InterstateApLocate.ResidentialAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        if (entities.CsePersonAddress.VerifiedDate != null)
        {
          local.InterstateApLocate.ResidentialAddressConfirmInd = "Y";
        }
        else
        {
          local.InterstateApLocate.ResidentialAddressConfirmInd = "N";
        }
      }

      // --------------------------------------------------
      // Get the most current non end-dated MAILING ADDRESS
      // --------------------------------------------------
      if (ReadCsePersonAddress6())
      {
        local.InterstateApLocate.MailingAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.MailingAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.MailingCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.MailingState = entities.CsePersonAddress.State;
        local.InterstateApLocate.MailingZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.MailingZipCode4 =
          entities.CsePersonAddress.Zip4;

        if (Lt(new DateTime(1, 1, 1), entities.CsePersonAddress.VerifiedDate))
        {
          local.InterstateApLocate.MailingAddressEffectiveDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          local.InterstateApLocate.MailingAddressEffectiveDate =
            Date(entities.CsePersonAddress.Identifier);
        }

        if (!Equal(entities.CsePersonAddress.EndDate,
          local.MaxDate.ExpirationDate))
        {
          local.InterstateApLocate.MailingAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        if (entities.CsePersonAddress.VerifiedDate != null)
        {
          local.InterstateApLocate.MailingAddressConfirmedInd = "Y";
        }
        else
        {
          local.InterstateApLocate.MailingAddressConfirmedInd = "N";
        }
      }

      // ---------------
      // End PR # 100337
      // ---------------
    }
    else
    {
      // ---------------------
      // Beg PR's 84721, 92066
      // ---------------------
      // -------------------------------------------------------
      // Get the most current non end-dated domestic RESIDENTIAL
      // ADDRESS
      // -------------------------------------------------------
      if (ReadCsePersonAddress7())
      {
        local.InterstateApLocate.ResidentialAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.ResidentialAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.ResidentialCity =
          entities.CsePersonAddress.City;
        local.InterstateApLocate.ResidentialState =
          entities.CsePersonAddress.State;
        local.InterstateApLocate.ResidentialZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.ResidentialZipCode4 =
          entities.CsePersonAddress.Zip4;

        if (Lt(new DateTime(1, 1, 1), entities.CsePersonAddress.VerifiedDate))
        {
          local.InterstateApLocate.ResidentialAddressEffectvDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          local.InterstateApLocate.ResidentialAddressEffectvDate =
            Date(entities.CsePersonAddress.Identifier);
        }

        if (!Equal(entities.CsePersonAddress.EndDate,
          local.MaxDate.ExpirationDate))
        {
          local.InterstateApLocate.ResidentialAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        if (entities.CsePersonAddress.VerifiedDate != null)
        {
          local.InterstateApLocate.ResidentialAddressConfirmInd = "Y";
        }
        else
        {
          local.InterstateApLocate.ResidentialAddressConfirmInd = "N";
        }
      }

      // --------------------------------------------------
      // Get the most current non end-dated MAILING ADDRESS
      // --------------------------------------------------
      if (ReadCsePersonAddress5())
      {
        local.InterstateApLocate.MailingAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.MailingAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.MailingCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.MailingState = entities.CsePersonAddress.State;
        local.InterstateApLocate.MailingZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.MailingZipCode4 =
          entities.CsePersonAddress.Zip4;

        if (Lt(new DateTime(1, 1, 1), entities.CsePersonAddress.VerifiedDate))
        {
          local.InterstateApLocate.MailingAddressEffectiveDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          local.InterstateApLocate.MailingAddressEffectiveDate =
            Date(entities.CsePersonAddress.Identifier);
        }

        if (!Equal(entities.CsePersonAddress.EndDate,
          local.MaxDate.ExpirationDate))
        {
          local.InterstateApLocate.MailingAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        if (entities.CsePersonAddress.VerifiedDate != null)
        {
          local.InterstateApLocate.MailingAddressConfirmedInd = "Y";
        }
        else
        {
          local.InterstateApLocate.MailingAddressConfirmedInd = "N";
        }
      }

      // ---------------------
      // End PR's 84721, 92066
      // ---------------------
    }

    local.InterstateApLocate.HomeAreaCode = entities.Ap.HomePhoneAreaCode;
    local.InterstateApLocate.HomePhoneNumber = entities.Ap.HomePhone;
    local.InterstateApLocate.WorkAreaCode = entities.Ap.WorkPhoneAreaCode;
    local.InterstateApLocate.WorkPhoneNumber = entities.Ap.WorkPhone;

    foreach(var item in ReadCsePersonLicense())
    {
      if (Equal(entities.CsePersonLicense.Type1, "D") && IsEmpty
        (local.InterstateApLocate.DriversLicState))
      {
        local.InterstateApLocate.DriversLicState =
          entities.CsePersonLicense.IssuingState;
        local.InterstateApLocate.DriversLicenseNum =
          entities.CsePersonLicense.Number;
      }

      if (Equal(entities.CsePersonLicense.Type1, "P") && IsEmpty
        (local.InterstateApLocate.ProfessionalLicenses))
      {
        local.InterstateApLocate.ProfessionalLicenses =
          entities.CsePersonLicense.Description;
      }
    }

    // ***   Call CAB to get the AP's alias SSNs   ***
    UseSiAltsBuildAliasAndSsn();

    if (!local.ApAliasName.IsEmpty)
    {
      for(local.ApAliasName.Index = 0; local.ApAliasName.Index < local
        .ApAliasName.Count; ++local.ApAliasName.Index)
      {
        if (!local.ApAliasName.CheckSize())
        {
          break;
        }

        switch(local.ApAliasName.Index + 1)
        {
          case 1:
            local.InterstateApLocate.Alias1FirstName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.FirstName;
            local.InterstateApLocate.Alias1LastName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.LastName;
            local.InterstateApLocate.Alias1MiddleName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.MiddleInitial;

            break;
          case 2:
            local.InterstateApLocate.Alias2FirstName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.FirstName;
            local.InterstateApLocate.Alias2LastName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.LastName;
            local.InterstateApLocate.Alias2MiddleName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.MiddleInitial;

            break;
          case 3:
            local.InterstateApLocate.Alias3FirstName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.FirstName;
            local.InterstateApLocate.Alias3LastName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.LastName;
            local.InterstateApLocate.Alias3MiddleName =
              local.ApAliasName.Item.GapCsePersonsWorkSet.MiddleInitial;

            break;
          default:
            goto Test;
        }
      }

      local.ApAliasName.CheckIndex();
    }

Test:

    local.Null1.Date = null;
    local.InterstateApLocate.CurrentSpouseLastName =
      entities.Ap.CurrentSpouseLastName;
    local.InterstateApLocate.CurrentSpouseFirstName =
      entities.Ap.CurrentSpouseFirstName;
    local.InterstateApLocate.CurrentSpouseMiddleName =
      entities.Ap.CurrentSpouseMi;
    local.InterstateApLocate.CurrentSpouseSuffix = "";
    local.InterstateApLocate.Occupation = entities.Ap.Occupation;
    local.Max.Date = new DateTime(2099, 12, 31);

    // ----------------
    // Beg of PR #84397
    // ----------------
    foreach(var item in ReadEmploymentEmployerDomesticEmployerAddress())
    {
      if (Lt(local.Current.Date, entities.Employment.EndDt))
      {
        switch(AsChar(entities.Employment.Type1))
        {
          case 'E':
            if (AsChar(entities.Employment.ReturnCd) == 'E' || AsChar
              (entities.Employment.ReturnCd) == 'W')
            {
              if (Lt(local.Current.Date, entities.Employment.ReturnDt) || Equal
                (entities.Employment.ReturnDt, null))
              {
                local.InterstateApLocate.EmployerConfirmedInd = "N";
              }
              else
              {
                local.InterstateApLocate.EmployerConfirmedInd = "Y";
              }
            }
            else
            {
              continue;
            }

            break;
          case 'M':
            if (AsChar(entities.Employment.ReturnCd) == 'A' || AsChar
              (entities.Employment.ReturnCd) == 'R')
            {
              if (Lt(local.Current.Date, entities.Employment.ReturnDt) || Equal
                (entities.Employment.ReturnDt, null))
              {
                local.InterstateApLocate.EmployerConfirmedInd = "N";
              }
              else
              {
                local.InterstateApLocate.EmployerConfirmedInd = "Y";
              }

              // ----------------
              // End of PR #84397
              // ----------------
            }
            else
            {
              local.InterstateApLocate.EmployerConfirmedInd = "N";
            }

            if (AsChar(entities.Employment.ReturnCd) == 'N' || AsChar
              (entities.Employment.ReturnCd) == 'U')
            {
              continue;
            }

            break;
          default:
            continue;
        }

        ++local.CurrentEmployer.Count;

        switch(local.CurrentEmployer.Count)
        {
          case 1:
            if (IsEmpty(entities.Employer.Ein))
            {
              local.InterstateApLocate.EmployerEin = 0;
            }
            else if (Verify(entities.Employer.Ein, "1234567890") > 0)
            {
              local.InterstateApLocate.EmployerEin = 0;
            }
            else
            {
              local.InterstateApLocate.EmployerEin =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            local.InterstateApLocate.EmployerName = entities.Employer.Name;
            local.InterstateApLocate.EmployerAreaCode =
              entities.Employer.AreaCode;
            local.InterstateApLocate.EmployerPhoneNum =
              (int?)StringToNumber(entities.Employer.PhoneNo);

            if (entities.Employment.StartDt != null)
            {
              local.InterstateApLocate.EmployerEffectiveDate =
                entities.Employment.StartDt;
            }
            else
            {
              if (entities.Employment.ReturnDt != null)
              {
                local.InterstateApLocate.EmployerEffectiveDate =
                  entities.Employment.ReturnDt;
              }
              else
              {
                local.InterstateApLocate.EmployerEffectiveDate =
                  Date(entities.Employment.CreatedTimestamp);
              }

              // ----------------
              // End of PR #84397
              // ----------------
            }

            if (!Equal(entities.Employment.EndDt, local.MaxDate.ExpirationDate))
            {
              local.InterstateApLocate.EmployerEndDate =
                entities.Employment.EndDt;
            }

            if (Lt(0, entities.Employment.LastQtrIncome))
            {
              local.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.Employment.LastQtr);
              local.InterstateApLocate.WageYear = entities.Employment.LastQtrYr;
              local.InterstateApLocate.WageAmount =
                entities.Employment.LastQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute2NdQtrIncome))
            {
              local.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.Employment.Attribute2NdQtr);
              local.InterstateApLocate.WageYear =
                entities.Employment.Attribute2NdQtrYr;
              local.InterstateApLocate.WageAmount =
                entities.Employment.Attribute2NdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute3RdQtrIncome))
            {
              local.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.Employment.Attribute3RdQtr);
              local.InterstateApLocate.WageYear =
                entities.Employment.Attribute3RdQtrYr;
              local.InterstateApLocate.WageAmount =
                entities.Employment.Attribute3RdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute4ThQtrIncome))
            {
              local.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.Employment.Attribute4ThQtr);
              local.InterstateApLocate.WageYear =
                entities.Employment.Attribute4ThQtrYr;
              local.InterstateApLocate.WageAmount =
                entities.Employment.Attribute4ThQtrIncome;
            }

            local.InterstateApLocate.EmployerAddressLine1 =
              entities.DomesticEmployerAddress.Street1;
            local.InterstateApLocate.EmployerAddressLine2 =
              entities.DomesticEmployerAddress.Street2;
            local.InterstateApLocate.EmployerCity =
              entities.DomesticEmployerAddress.City;
            local.InterstateApLocate.EmployerState =
              entities.DomesticEmployerAddress.State;
            local.InterstateApLocate.EmployerZipCode5 =
              entities.DomesticEmployerAddress.ZipCode;
            local.InterstateApLocate.EmployerZipCode4 =
              entities.DomesticEmployerAddress.Zip4;

            break;
          case 2:
            if (IsEmpty(entities.Employer.Ein))
            {
              local.InterstateApLocate.Employer2Ein = 0;
            }
            else if (Verify(entities.Employer.Ein, "1234567890") > 0)
            {
              local.InterstateApLocate.Employer2Ein = 0;
            }
            else
            {
              local.InterstateApLocate.Employer2Ein =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            local.InterstateApLocate.Employer2Name = entities.Employer.Name;
            local.InterstateApLocate.Employer2PhoneNumber =
              entities.Employer.PhoneNo;

            // ---------------------------------------------------------------------
            // PR #143017 MCA 4-16-02 Invalid phone # from host.  Phone number 
            // was spaces and area code had 000 in it.
            // ---------------------------------------------------------------------
            if (!IsEmpty(local.InterstateApLocate.Employer2PhoneNumber))
            {
              local.InterstateApLocate.Employer2AreaCode =
                NumberToString(entities.Employer.AreaCode.GetValueOrDefault(),
                13, 3);
            }

            // ----------------
            // Beg of PR #84397
            // ----------------
            if (Lt(new DateTime(1, 1, 1), entities.Employment.StartDt))
            {
              local.InterstateApLocate.Employer2EffectiveDate =
                entities.Employment.StartDt;
            }
            else
            {
              if (Lt(new DateTime(1, 1, 1), entities.Employment.ReturnDt))
              {
                local.InterstateApLocate.Employer2EffectiveDate =
                  entities.Employment.ReturnDt;
              }
              else
              {
                local.InterstateApLocate.Employer2EffectiveDate =
                  Date(entities.Employment.CreatedTimestamp);
              }

              // ----------------
              // End of PR #84397
              // ----------------
            }

            local.InterstateApLocate.Employer2ConfirmedIndicator =
              local.InterstateApLocate.EmployerConfirmedInd ?? "";

            if (!Equal(entities.Employment.EndDt, local.MaxDate.ExpirationDate))
            {
              local.InterstateApLocate.Employer2EndDate =
                entities.Employment.EndDt;
            }

            if (Lt(0, entities.Employment.LastQtrIncome))
            {
              local.InterstateApLocate.Employer2WageQuarter =
                (int?)StringToNumber(entities.Employment.LastQtr);
              local.InterstateApLocate.Employer2WageYear =
                entities.Employment.LastQtrYr;
              local.InterstateApLocate.Employer2WageAmount =
                (long?)entities.Employment.LastQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute2NdQtrIncome))
            {
              local.InterstateApLocate.Employer2WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute2NdQtr);
              local.InterstateApLocate.Employer2WageYear =
                entities.Employment.Attribute2NdQtrYr;
              local.InterstateApLocate.Employer2WageAmount =
                (long?)entities.Employment.Attribute2NdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute3RdQtrIncome))
            {
              local.InterstateApLocate.Employer2WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute3RdQtr);
              local.InterstateApLocate.Employer2WageYear =
                entities.Employment.Attribute3RdQtrYr;
              local.InterstateApLocate.Employer2WageAmount =
                (long?)entities.Employment.Attribute3RdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute4ThQtrIncome))
            {
              local.InterstateApLocate.Employer2WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute4ThQtr);
              local.InterstateApLocate.Employer2WageYear =
                entities.Employment.Attribute4ThQtrYr;
              local.InterstateApLocate.Employer2WageAmount =
                (long?)entities.Employment.Attribute4ThQtrIncome;
            }

            local.InterstateApLocate.Employer2AddressLine1 =
              entities.DomesticEmployerAddress.Street1;
            local.InterstateApLocate.Employer2AddressLine2 =
              entities.DomesticEmployerAddress.Street2;
            local.InterstateApLocate.Employer2City =
              entities.DomesticEmployerAddress.City;
            local.InterstateApLocate.Employer2State =
              entities.DomesticEmployerAddress.State;
            local.InterstateApLocate.Employer2ZipCode5 =
              (int?)StringToNumber(entities.DomesticEmployerAddress.ZipCode);
            local.InterstateApLocate.Employer2ZipCode4 =
              (int?)StringToNumber(entities.DomesticEmployerAddress.Zip4);

            break;
          case 3:
            if (IsEmpty(entities.Employer.Ein))
            {
              local.InterstateApLocate.Employer3Ein = 0;
            }
            else if (Verify(entities.Employer.Ein, "1234567890") > 0)
            {
              local.InterstateApLocate.Employer3Ein = 0;
            }
            else
            {
              local.InterstateApLocate.Employer3Ein =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            local.InterstateApLocate.Employer3Name = entities.Employer.Name;
            local.InterstateApLocate.Employer3PhoneNumber =
              entities.Employer.PhoneNo;

            // ---------------------------------------------------------------------
            // PR #143017 MCA 4-16-02 Invalid phone # from host.  Phone number 
            // was spaces and area code had 000 in it. Therefore, only set area
            // code if phone is populated.
            // ---------------------------------------------------------------------
            if (!IsEmpty(local.InterstateApLocate.Employer3PhoneNumber))
            {
              local.InterstateApLocate.Employer3AreaCode =
                NumberToString(entities.Employer.AreaCode.GetValueOrDefault(),
                13, 3);
            }

            // ----------------
            // Beg of PR #84397
            // ----------------
            if (Lt(new DateTime(1, 1, 1), entities.Employment.StartDt))
            {
              local.InterstateApLocate.Employer3EffectiveDate =
                entities.Employment.StartDt;
            }
            else
            {
              if (Lt(new DateTime(1, 1, 1), entities.Employment.ReturnDt))
              {
                local.InterstateApLocate.Employer3EffectiveDate =
                  entities.Employment.ReturnDt;
              }
              else
              {
                local.InterstateApLocate.Employer3EffectiveDate =
                  Date(entities.Employment.CreatedTimestamp);
              }

              // ----------------
              // End of PR #84397
              // ----------------
            }

            local.InterstateApLocate.Employer3ConfirmedIndicator =
              local.InterstateApLocate.EmployerConfirmedInd ?? "";

            if (!Equal(entities.Employment.EndDt, local.MaxDate.ExpirationDate))
            {
              local.InterstateApLocate.Employer3EndDate =
                entities.Employment.EndDt;
            }

            if (Lt(0, entities.Employment.LastQtrIncome))
            {
              local.InterstateApLocate.Employer3WageQuarter =
                (int?)StringToNumber(entities.Employment.LastQtr);
              local.InterstateApLocate.Employer3WageYear =
                entities.Employment.LastQtrYr;
              local.InterstateApLocate.Employer3WageAmount =
                (long?)entities.Employment.LastQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute2NdQtrIncome))
            {
              local.InterstateApLocate.Employer3WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute2NdQtr);
              local.InterstateApLocate.Employer3WageYear =
                entities.Employment.Attribute2NdQtrYr;
              local.InterstateApLocate.Employer3WageAmount =
                (long?)entities.Employment.Attribute2NdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute3RdQtrIncome))
            {
              local.InterstateApLocate.Employer3WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute3RdQtr);
              local.InterstateApLocate.Employer3WageYear =
                entities.Employment.Attribute3RdQtrYr;
              local.InterstateApLocate.Employer3WageAmount =
                (long?)entities.Employment.Attribute3RdQtrIncome;
            }
            else if (Lt(0, entities.Employment.Attribute4ThQtrIncome))
            {
              local.InterstateApLocate.Employer3WageQuarter =
                (int?)StringToNumber(entities.Employment.Attribute4ThQtr);
              local.InterstateApLocate.Employer3WageYear =
                entities.Employment.Attribute4ThQtrYr;
              local.InterstateApLocate.Employer3WageAmount =
                (long?)entities.Employment.Attribute4ThQtrIncome;
            }

            local.InterstateApLocate.Employer3AddressLine1 =
              entities.DomesticEmployerAddress.Street1;
            local.InterstateApLocate.Employer3AddressLine2 =
              entities.DomesticEmployerAddress.Street2;
            local.InterstateApLocate.Employer3City =
              entities.DomesticEmployerAddress.City;
            local.InterstateApLocate.Employer3State =
              entities.DomesticEmployerAddress.State;
            local.InterstateApLocate.Employer3ZipCode5 =
              (int?)StringToNumber(entities.DomesticEmployerAddress.ZipCode);
            local.InterstateApLocate.Employer3ZipCode4 =
              (int?)StringToNumber(entities.DomesticEmployerAddress.Zip4);

            break;
          default:
            // -------------------------------------------------------
            // In the event more than three EMPLOYERS are found, exit
            // this loop - the three most current will be sent.  If no
            // EMPLOYERS are found, a blank DB will be sent.
            // -------------------------------------------------------
            break;
        }
      }
      else
      {
        local.InterstateApLocate.LastEmployerName = entities.Employer.Name;
        local.InterstateApLocate.LastEmployerEndDate =
          entities.Employment.EndDt;
        local.InterstateApLocate.LastEmployerDate = entities.Employment.StartDt;
        local.InterstateApLocate.LastEmployerAddressLine1 =
          entities.DomesticEmployerAddress.Street1;
        local.InterstateApLocate.LastEmployerAddressLine2 =
          entities.DomesticEmployerAddress.Street2;
        local.InterstateApLocate.LastEmployerCity =
          entities.DomesticEmployerAddress.City;
        local.InterstateApLocate.LastEmployerState =
          entities.DomesticEmployerAddress.State;
        local.InterstateApLocate.LastEmployerZipCode5 =
          entities.DomesticEmployerAddress.ZipCode;
        local.InterstateApLocate.LastEmployerZipCode4 =
          entities.DomesticEmployerAddress.Zip4;
      }
    }

    if (ReadHealthInsuranceCoverage())
    {
      local.InterstateApLocate.InsurancePolicyNum =
        entities.HealthInsuranceCoverage.InsurancePolicyNumber;

      if (ReadHealthInsuranceCompany())
      {
        local.InterstateApLocate.InsuranceCarrierName =
          entities.HealthInsuranceCompany.InsurancePolicyCarrier;
      }
      else
      {
        // --------------------------------------------
        // Not a terminating error; continue processing
        // 
        // --------------------------------------------
      }
    }
    else
    {
      // --------------------------------------------
      // Not a terminating error; continue processing
      // 
      // --------------------------------------------
    }

    if (Equal(local.ScreenIdentification.Command, "FADS"))
    {
      // ---------------
      // Beg PR # 100337
      // ---------------
      // -----------------
      // Foreign addresses
      // -----------------
      if (ReadCsePersonAddress4())
      {
        local.InterstateApLocate.LastResAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.LastResAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.LastResCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.LastResState = entities.CsePersonAddress.State;
        local.InterstateApLocate.LastResZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.LastResZipCode4 =
          entities.CsePersonAddress.Zip4;
        local.InterstateApLocate.LastResAddressDate =
          entities.CsePersonAddress.EndDate;
      }

      if (ReadCsePersonAddress2())
      {
        local.InterstateApLocate.LastMailAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.LastMailAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.LastMailCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.LastMailState =
          entities.CsePersonAddress.State;
        local.InterstateApLocate.LastMailZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.LastMailZipCode4 =
          entities.CsePersonAddress.Zip4;
        local.InterstateApLocate.LastMailAddressDate =
          entities.CsePersonAddress.EndDate;
      }
    }
    else
    {
      // ------------------
      // Domestic addresses
      // ------------------
      if (ReadCsePersonAddress3())
      {
        local.InterstateApLocate.LastResAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.LastResAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.LastResCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.LastResState = entities.CsePersonAddress.State;
        local.InterstateApLocate.LastResZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.LastResZipCode4 =
          entities.CsePersonAddress.Zip4;
        local.InterstateApLocate.LastResAddressDate =
          entities.CsePersonAddress.EndDate;
      }

      if (ReadCsePersonAddress1())
      {
        local.InterstateApLocate.LastMailAddressLine1 =
          entities.CsePersonAddress.Street1;
        local.InterstateApLocate.LastMailAddressLine2 =
          entities.CsePersonAddress.Street2;
        local.InterstateApLocate.LastMailCity = entities.CsePersonAddress.City;
        local.InterstateApLocate.LastMailState =
          entities.CsePersonAddress.State;
        local.InterstateApLocate.LastMailZipCode5 =
          entities.CsePersonAddress.ZipCode;
        local.InterstateApLocate.LastMailZipCode4 =
          entities.CsePersonAddress.Zip4;
        local.InterstateApLocate.LastMailAddressDate =
          entities.CsePersonAddress.EndDate;
      }

      // ---------------
      // End PR # 100337
      // ---------------
    }

    // ***************************************************************
    // 8/5/99  C. Ott   Validation that either employer or address
    // information is provided for a "successful" action reason
    // code.
    // ****************************************************************
    if (CharAt(import.InterstateCase.ActionReasonCode, 2) == 'S')
    {
      if (IsEmpty(local.InterstateApLocate.EmployerName) && IsEmpty
        (local.InterstateApLocate.MailingAddressLine1) && IsEmpty
        (local.InterstateApLocate.ResidentialAddressLine1))
      {
        return;
      }
    }

    // ----------------
    // CLS 04/12/00
    // Beg of WR# 00162
    // ----------------
    if (!IsEmpty(entities.Ap.FamilyViolenceIndicator))
    {
      local.InterstateApLocate.ResidentialAddressLine1 = "";
      local.InterstateApLocate.ResidentialAddressLine2 = "";
      local.InterstateApLocate.ResidentialCity = "";
      local.InterstateApLocate.ResidentialState = "";
      local.InterstateApLocate.ResidentialZipCode5 = "";
      local.InterstateApLocate.ResidentialZipCode4 = "";
      local.InterstateApLocate.MailingAddressLine1 = "";
      local.InterstateApLocate.MailingAddressLine2 = "";
      local.InterstateApLocate.MailingCity = "";
      local.InterstateApLocate.MailingState = "";
      local.InterstateApLocate.MailingZipCode5 = "";
      local.InterstateApLocate.MailingZipCode4 = "";
      local.InterstateApLocate.HomeAreaCode = 0;
      local.InterstateApLocate.HomePhoneNumber = 0;
      local.InterstateApLocate.WorkAreaCode = 0;
      local.InterstateApLocate.WorkPhoneNumber = 0;
      local.InterstateApLocate.EmployerName = "";
      local.InterstateApLocate.EmployerAddressLine1 = "";
      local.InterstateApLocate.EmployerAddressLine2 = "";
      local.InterstateApLocate.EmployerCity = "";
      local.InterstateApLocate.EmployerState = "";
      local.InterstateApLocate.EmployerZipCode5 = "";
      local.InterstateApLocate.EmployerZipCode4 = "";
      local.InterstateApLocate.EmployerAreaCode = 0;
      local.InterstateApLocate.EmployerPhoneNum = 0;
      local.InterstateApLocate.EmployerEin = 0;
      local.InterstateApLocate.LastResAddressLine1 = "";
      local.InterstateApLocate.LastResAddressLine2 = "";
      local.InterstateApLocate.LastResCity = "";
      local.InterstateApLocate.LastResState = "";
      local.InterstateApLocate.LastResZipCode5 = "";
      local.InterstateApLocate.LastResZipCode4 = "";
      local.InterstateApLocate.LastMailAddressLine1 = "";
      local.InterstateApLocate.LastMailAddressLine2 = "";
      local.InterstateApLocate.LastMailCity = "";
      local.InterstateApLocate.LastMailState = "";
      local.InterstateApLocate.LastMailZipCode5 = "";
      local.InterstateApLocate.LastMailZipCode4 = "";
      local.InterstateApLocate.LastEmployerName = "";
      local.InterstateApLocate.LastEmployerAddressLine1 = "";
      local.InterstateApLocate.LastEmployerAddressLine2 = "";
      local.InterstateApLocate.LastEmployerCity = "";
      local.InterstateApLocate.LastEmployerState = "";
      local.InterstateApLocate.LastEmployerZipCode5 = "";
      local.InterstateApLocate.LastEmployerZipCode4 = "";
      local.InterstateApLocate.Employer2Name = "";
      local.InterstateApLocate.Employer2AddressLine1 = "";
      local.InterstateApLocate.Employer2AddressLine2 = "";
      local.InterstateApLocate.Employer2City = "";
      local.InterstateApLocate.Employer2State = "";
      local.InterstateApLocate.Employer2ZipCode5 = 0;
      local.InterstateApLocate.Employer2ZipCode4 = 0;
      local.InterstateApLocate.Employer2PhoneNumber = "";
      local.InterstateApLocate.Employer2AreaCode = "";
      local.InterstateApLocate.Employer2Ein = 0;
      local.InterstateApLocate.Employer3Name = "";
      local.InterstateApLocate.Employer3AddressLine1 = "";
      local.InterstateApLocate.Employer3AddressLine2 = "";
      local.InterstateApLocate.Employer3City = "";
      local.InterstateApLocate.Employer3State = "";
      local.InterstateApLocate.Employer3ZipCode5 = 0;
      local.InterstateApLocate.Employer3ZipCode4 = 0;
      local.InterstateApLocate.Employer3PhoneNumber = "";
      local.InterstateApLocate.Employer3AreaCode = "";
      local.InterstateApLocate.Employer3Ein = 0;
    }

    // ----------------
    // CLS 04/12/00
    // End of WR# 00162
    // ----------------
    try
    {
      CreateInterstateApLocate();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      export.InterstateApLocate.Assign(entities.InterstateApLocate);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTATE_AP_LOCATE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTATE_AP_LOCATE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveAp(SiAltsBuildAliasAndSsn.Export.ApGroup source,
    Local.ApAliasNameGroup target)
  {
    target.GapCommon.SelectChar = source.GapCommon.SelectChar;
    target.GapCsePersonsWorkSet.Assign(source.GapCsePersonsWorkSet);
    target.GapSsn3.Text3 = source.GapSsn3.Text3;
    target.GapSsn2.Text3 = source.GapSsn2.Text3;
    target.GapSsn4.Text4 = source.GapSsn4.Text4;
    target.Temp1.Flag = source.GapKscares.Flag;
    target.Temp2.Flag = source.GapKanpay.Flag;
    target.Temp3.Flag = source.GapCse.Flag;
    target.Temp4.Flag = source.GapAe.Flag;
    target.Temp5.Flag = source.GapDbOccurrence.Flag;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    useImport.Ap1.Number = local.ApCsePersonsWorkSet.Number;

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    useExport.Ap.CopyTo(local.ApAliasName, MoveAp);
  }

  private void CreateInterstateApLocate()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateApIdentification.Populated);

    var cncTransactionDt = entities.InterstateApIdentification.CcaTransactionDt;
    var cncTransSerlNbr = entities.InterstateApIdentification.CcaTransSerNum;
    var employerEin = local.InterstateApLocate.EmployerEin.GetValueOrDefault();
    var employerName = local.InterstateApLocate.EmployerName ?? "";
    var employerPhoneNum =
      local.InterstateApLocate.EmployerPhoneNum.GetValueOrDefault();
    var employerEffectiveDate = local.InterstateApLocate.EmployerEffectiveDate;
    var employerEndDate = local.InterstateApLocate.EmployerEndDate;
    var employerConfirmedInd =
      local.InterstateApLocate.EmployerConfirmedInd ?? "";
    var residentialAddressLine1 =
      local.InterstateApLocate.ResidentialAddressLine1 ?? "";
    var residentialAddressLine2 =
      local.InterstateApLocate.ResidentialAddressLine2 ?? "";
    var residentialCity = local.InterstateApLocate.ResidentialCity ?? "";
    var residentialState = local.InterstateApLocate.ResidentialState ?? "";
    var residentialZipCode5 = local.InterstateApLocate.ResidentialZipCode5 ?? ""
      ;
    var residentialZipCode4 = local.InterstateApLocate.ResidentialZipCode4 ?? ""
      ;
    var mailingAddressLine1 = local.InterstateApLocate.MailingAddressLine1 ?? ""
      ;
    var mailingAddressLine2 = local.InterstateApLocate.MailingAddressLine2 ?? ""
      ;
    var mailingCity = local.InterstateApLocate.MailingCity ?? "";
    var mailingState = local.InterstateApLocate.MailingState ?? "";
    var mailingZipCode5 = local.InterstateApLocate.MailingZipCode5 ?? "";
    var mailingZipCode4 = local.InterstateApLocate.MailingZipCode4 ?? "";
    var residentialAddressEffectvDate =
      local.InterstateApLocate.ResidentialAddressEffectvDate;
    var residentialAddressEndDate =
      local.InterstateApLocate.ResidentialAddressEndDate;
    var residentialAddressConfirmInd =
      local.InterstateApLocate.ResidentialAddressConfirmInd ?? "";
    var mailingAddressEffectiveDate =
      local.InterstateApLocate.MailingAddressEffectiveDate;
    var mailingAddressEndDate = local.InterstateApLocate.MailingAddressEndDate;
    var mailingAddressConfirmedInd =
      local.InterstateApLocate.MailingAddressConfirmedInd ?? "";
    var homePhoneNumber =
      local.InterstateApLocate.HomePhoneNumber.GetValueOrDefault();
    var workPhoneNumber =
      local.InterstateApLocate.WorkPhoneNumber.GetValueOrDefault();
    var driversLicState = local.InterstateApLocate.DriversLicState ?? "";
    var driversLicenseNum = local.InterstateApLocate.DriversLicenseNum ?? "";
    var alias1FirstName = local.InterstateApLocate.Alias1FirstName ?? "";
    var alias1MiddleName = local.InterstateApLocate.Alias1MiddleName ?? "";
    var alias1LastName = local.InterstateApLocate.Alias1LastName ?? "";
    var alias1Suffix = local.InterstateApLocate.Alias1Suffix ?? "";
    var alias2FirstName = local.InterstateApLocate.Alias2FirstName ?? "";
    var alias2MiddleName = local.InterstateApLocate.Alias2MiddleName ?? "";
    var alias2LastName = local.InterstateApLocate.Alias2LastName ?? "";
    var alias2Suffix = local.InterstateApLocate.Alias2Suffix ?? "";
    var alias3FirstName = local.InterstateApLocate.Alias3FirstName ?? "";
    var alias3MiddleName = local.InterstateApLocate.Alias3MiddleName ?? "";
    var alias3LastName = local.InterstateApLocate.Alias3LastName ?? "";
    var alias3Suffix = local.InterstateApLocate.Alias3Suffix ?? "";
    var currentSpouseFirstName =
      local.InterstateApLocate.CurrentSpouseFirstName ?? "";
    var currentSpouseMiddleName =
      local.InterstateApLocate.CurrentSpouseMiddleName ?? "";
    var currentSpouseLastName =
      local.InterstateApLocate.CurrentSpouseLastName ?? "";
    var currentSpouseSuffix = local.InterstateApLocate.CurrentSpouseSuffix ?? ""
      ;
    var occupation = local.InterstateApLocate.Occupation ?? "";
    var employerAddressLine1 =
      local.InterstateApLocate.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      local.InterstateApLocate.EmployerAddressLine2 ?? "";
    var employerCity = local.InterstateApLocate.EmployerCity ?? "";
    var employerState = local.InterstateApLocate.EmployerState ?? "";
    var employerZipCode5 = local.InterstateApLocate.EmployerZipCode5 ?? "";
    var employerZipCode4 = local.InterstateApLocate.EmployerZipCode4 ?? "";
    var wageQtr = local.InterstateApLocate.WageQtr.GetValueOrDefault();
    var wageYear = local.InterstateApLocate.WageYear.GetValueOrDefault();
    var wageAmount = local.InterstateApLocate.WageAmount.GetValueOrDefault();
    var insuranceCarrierName =
      local.InterstateApLocate.InsuranceCarrierName ?? "";
    var insurancePolicyNum = local.InterstateApLocate.InsurancePolicyNum ?? "";
    var lastResAddressLine1 = local.InterstateApLocate.LastResAddressLine1 ?? ""
      ;
    var lastResAddressLine2 = local.InterstateApLocate.LastResAddressLine2 ?? ""
      ;
    var lastResCity = local.InterstateApLocate.LastResCity ?? "";
    var lastResState = local.InterstateApLocate.LastResState ?? "";
    var lastResZipCode5 = local.InterstateApLocate.LastResZipCode5 ?? "";
    var lastResZipCode4 = local.InterstateApLocate.LastResZipCode4 ?? "";
    var lastResAddressDate = local.InterstateApLocate.LastResAddressDate;
    var lastMailAddressLine1 =
      local.InterstateApLocate.LastMailAddressLine1 ?? "";
    var lastMailAddressLine2 =
      local.InterstateApLocate.LastMailAddressLine2 ?? "";
    var lastMailCity = local.InterstateApLocate.LastMailCity ?? "";
    var lastMailState = local.InterstateApLocate.LastMailState ?? "";
    var lastMailZipCode5 = local.InterstateApLocate.LastMailZipCode5 ?? "";
    var lastMailZipCode4 = local.InterstateApLocate.LastMailZipCode4 ?? "";
    var lastMailAddressDate = local.InterstateApLocate.LastMailAddressDate;
    var lastEmployerName = local.InterstateApLocate.LastEmployerName ?? "";
    var lastEmployerDate = local.InterstateApLocate.LastEmployerDate;
    var lastEmployerAddressLine1 =
      local.InterstateApLocate.LastEmployerAddressLine1 ?? "";
    var lastEmployerAddressLine2 =
      local.InterstateApLocate.LastEmployerAddressLine2 ?? "";
    var lastEmployerCity = local.InterstateApLocate.LastEmployerCity ?? "";
    var lastEmployerState = local.InterstateApLocate.LastEmployerState ?? "";
    var lastEmployerZipCode5 =
      local.InterstateApLocate.LastEmployerZipCode5 ?? "";
    var lastEmployerZipCode4 =
      local.InterstateApLocate.LastEmployerZipCode4 ?? "";
    var professionalLicenses =
      local.InterstateApLocate.ProfessionalLicenses ?? "";
    var workAreaCode =
      local.InterstateApLocate.WorkAreaCode.GetValueOrDefault();
    var homeAreaCode =
      local.InterstateApLocate.HomeAreaCode.GetValueOrDefault();
    var lastEmployerEndDate = local.InterstateApLocate.LastEmployerEndDate;
    var employer2Name = local.InterstateApLocate.Employer2Name ?? "";
    var employer2Ein =
      local.InterstateApLocate.Employer2Ein.GetValueOrDefault();
    var employer2PhoneNumber =
      local.InterstateApLocate.Employer2PhoneNumber ?? "";
    var employer2AreaCode = local.InterstateApLocate.Employer2AreaCode ?? "";
    var employer2AddressLine1 =
      local.InterstateApLocate.Employer2AddressLine1 ?? "";
    var employer2AddressLine2 =
      local.InterstateApLocate.Employer2AddressLine2 ?? "";
    var employer2City = local.InterstateApLocate.Employer2City ?? "";
    var employer2State = local.InterstateApLocate.Employer2State ?? "";
    var employer2ZipCode5 =
      local.InterstateApLocate.Employer2ZipCode5.GetValueOrDefault();
    var employer2ZipCode4 =
      local.InterstateApLocate.Employer2ZipCode4.GetValueOrDefault();
    var employer2ConfirmedIndicator =
      local.InterstateApLocate.Employer2ConfirmedIndicator ?? "";
    var employer2EffectiveDate =
      local.InterstateApLocate.Employer2EffectiveDate;
    var employer2EndDate = local.InterstateApLocate.Employer2EndDate;
    var employer2WageAmount =
      local.InterstateApLocate.Employer2WageAmount.GetValueOrDefault();
    var employer2WageQuarter =
      local.InterstateApLocate.Employer2WageQuarter.GetValueOrDefault();
    var employer2WageYear =
      local.InterstateApLocate.Employer2WageYear.GetValueOrDefault();
    var employer3Name = local.InterstateApLocate.Employer3Name ?? "";
    var employer3Ein =
      local.InterstateApLocate.Employer3Ein.GetValueOrDefault();
    var employer3PhoneNumber =
      local.InterstateApLocate.Employer3PhoneNumber ?? "";
    var employer3AreaCode = local.InterstateApLocate.Employer3AreaCode ?? "";
    var employer3AddressLine1 =
      local.InterstateApLocate.Employer3AddressLine1 ?? "";
    var employer3AddressLine2 =
      local.InterstateApLocate.Employer3AddressLine2 ?? "";
    var employer3City = local.InterstateApLocate.Employer3City ?? "";
    var employer3State = local.InterstateApLocate.Employer3State ?? "";
    var employer3ZipCode5 =
      local.InterstateApLocate.Employer3ZipCode5.GetValueOrDefault();
    var employer3ZipCode4 =
      local.InterstateApLocate.Employer3ZipCode4.GetValueOrDefault();
    var employer3ConfirmedIndicator =
      local.InterstateApLocate.Employer3ConfirmedIndicator ?? "";
    var employer3EffectiveDate =
      local.InterstateApLocate.Employer3EffectiveDate;
    var employer3EndDate = local.InterstateApLocate.Employer3EndDate;
    var employer3WageAmount =
      local.InterstateApLocate.Employer3WageAmount.GetValueOrDefault();
    var employer3WageQuarter =
      local.InterstateApLocate.Employer3WageQuarter.GetValueOrDefault();
    var employer3WageYear =
      local.InterstateApLocate.Employer3WageYear.GetValueOrDefault();

    entities.InterstateApLocate.Populated = false;
    Update("CreateInterstateApLocate",
      (db, command) =>
      {
        db.SetDate(command, "cncTransactionDt", cncTransactionDt);
        db.SetInt64(command, "cncTransSerlNbr", cncTransSerlNbr);
        db.SetNullableInt32(command, "employerEin", employerEin);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableInt32(command, "employerPhoneNum", employerPhoneNum);
        db.SetNullableDate(command, "employerEffDate", employerEffectiveDate);
        db.SetNullableDate(command, "employerEndDate", employerEndDate);
        db.SetNullableString(command, "employerCfmdInd", employerConfirmedInd);
        db.SetNullableString(command, "resAddrLine1", residentialAddressLine1);
        db.SetNullableString(command, "resAddrLine2", residentialAddressLine2);
        db.SetNullableString(command, "residentialCity", residentialCity);
        db.SetNullableString(command, "residentialState", residentialState);
        db.SetNullableString(command, "residentialZip5", residentialZipCode5);
        db.SetNullableString(command, "residentialZip4", residentialZipCode4);
        db.SetNullableString(command, "mailingAddrLine1", mailingAddressLine1);
        db.SetNullableString(command, "mailingAddrLine2", mailingAddressLine2);
        db.SetNullableString(command, "mailingCity", mailingCity);
        db.SetNullableString(command, "mailingState", mailingState);
        db.SetNullableString(command, "mailingZip5", mailingZipCode5);
        db.SetNullableString(command, "mailingZip4", mailingZipCode4);
        db.SetNullableDate(
          command, "resAddrEffDate", residentialAddressEffectvDate);
        db.
          SetNullableDate(command, "resAddrEndDate", residentialAddressEndDate);
          
        db.SetNullableString(
          command, "resAddrConInd", residentialAddressConfirmInd);
        db.SetNullableDate(
          command, "mailAddrEffDte", mailingAddressEffectiveDate);
        db.SetNullableDate(command, "mailAddrEndDte", mailingAddressEndDate);
        db.SetNullableString(
          command, "mailAddrConfInd", mailingAddressConfirmedInd);
        db.SetNullableInt32(command, "homePhoneNumber", homePhoneNumber);
        db.SetNullableInt32(command, "workPhoneNumber", workPhoneNumber);
        db.SetNullableString(command, "driversLicState", driversLicState);
        db.SetNullableString(command, "driverLicenseNbr", driversLicenseNum);
        db.SetNullableString(command, "alias1FirstName", alias1FirstName);
        db.SetNullableString(command, "alias1MiddleNam", alias1MiddleName);
        db.SetNullableString(command, "alias1LastName", alias1LastName);
        db.SetNullableString(command, "alias1Suffix", alias1Suffix);
        db.SetNullableString(command, "alias2FirstName", alias2FirstName);
        db.SetNullableString(command, "alias2MiddleNam", alias2MiddleName);
        db.SetNullableString(command, "alias2LastName", alias2LastName);
        db.SetNullableString(command, "alias2Suffix", alias2Suffix);
        db.SetNullableString(command, "alias3FirstName", alias3FirstName);
        db.SetNullableString(command, "alias3MiddleNam", alias3MiddleName);
        db.SetNullableString(command, "alias3LastName", alias3LastName);
        db.SetNullableString(command, "alias3Suffix", alias3Suffix);
        db.
          SetNullableString(command, "currentSpouseFir", currentSpouseFirstName);
          
        db.SetNullableString(
          command, "currentSpouseMid", currentSpouseMiddleName);
        db.
          SetNullableString(command, "currentSpouseLas", currentSpouseLastName);
          
        db.SetNullableString(command, "currentSpouseSuf", currentSpouseSuffix);
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "emplAddrLine1", employerAddressLine1);
        db.SetNullableString(command, "emplAddrLine2", employerAddressLine2);
        db.SetNullableString(command, "employerCity", employerCity);
        db.SetNullableString(command, "employerState", employerState);
        db.SetNullableString(command, "emplZipCode5", employerZipCode5);
        db.SetNullableString(command, "emplZipCode4", employerZipCode4);
        db.SetNullableInt32(command, "wageQtr", wageQtr);
        db.SetNullableInt32(command, "wageYear", wageYear);
        db.SetNullableDecimal(command, "wageAmount", wageAmount);
        db.SetNullableString(command, "insCarrierName", insuranceCarrierName);
        db.SetNullableString(command, "insPolicyNbr", insurancePolicyNum);
        db.SetNullableString(command, "lstResAddrLine1", lastResAddressLine1);
        db.SetNullableString(command, "lstResAddrLine2", lastResAddressLine2);
        db.SetNullableString(command, "lastResCity", lastResCity);
        db.SetNullableString(command, "lastResState", lastResState);
        db.SetNullableString(command, "lstResZipCode5", lastResZipCode5);
        db.SetNullableString(command, "lstResZipCode4", lastResZipCode4);
        db.SetNullableDate(command, "lastResAddrDte", lastResAddressDate);
        db.SetNullableString(command, "lstMailAddrLin1", lastMailAddressLine1);
        db.SetNullableString(command, "lstMailAddrLin2", lastMailAddressLine2);
        db.SetNullableString(command, "lastMailCity", lastMailCity);
        db.SetNullableString(command, "lastMailState", lastMailState);
        db.SetNullableString(command, "lstMailZipCode5", lastMailZipCode5);
        db.SetNullableString(command, "lstMailZipCode4", lastMailZipCode4);
        db.SetNullableDate(command, "lastMailAddrDte", lastMailAddressDate);
        db.SetNullableString(command, "lastEmployerName", lastEmployerName);
        db.SetNullableDate(command, "lastEmployerDate", lastEmployerDate);
        db.SetNullableString(
          command, "lstEmplAddrLin1", lastEmployerAddressLine1);
        db.SetNullableString(
          command, "lstEmplAddrLin2", lastEmployerAddressLine2);
        db.SetNullableString(command, "lastEmployerCity", lastEmployerCity);
        db.SetNullableString(command, "lastEmployerStat", lastEmployerState);
        db.SetNullableString(command, "lstEmplZipCode5", lastEmployerZipCode5);
        db.SetNullableString(command, "lstEmplZipCode4", lastEmployerZipCode4);
        db.SetNullableString(command, "professionalLics", professionalLicenses);
        db.SetNullableInt32(command, "workAreaCode", workAreaCode);
        db.SetNullableInt32(command, "homeAreaCode", homeAreaCode);
        db.SetNullableDate(command, "lastEmpEndDate", lastEmployerEndDate);
        db.SetNullableInt32(command, "employerAreaCode", 0);
        db.SetNullableString(command, "employer2Name", employer2Name);
        db.SetNullableInt32(command, "employer2Ein", employer2Ein);
        db.SetNullableString(command, "emp2PhoneNumber", employer2PhoneNumber);
        db.SetNullableString(command, "empl2AreaCode", employer2AreaCode);
        db.SetNullableString(command, "emp2AddrLine1", employer2AddressLine1);
        db.SetNullableString(command, "emp2AddrLine2", employer2AddressLine2);
        db.SetNullableString(command, "employer2City", employer2City);
        db.SetNullableString(command, "employer2State", employer2State);
        db.SetNullableInt32(command, "emp2ZipCode5", employer2ZipCode5);
        db.SetNullableInt32(command, "emp2ZipCode4", employer2ZipCode4);
        db.SetNullableString(
          command, "emp2ConfirmedInd", employer2ConfirmedIndicator);
        db.SetNullableDate(command, "emp2EffectiveDt", employer2EffectiveDate);
        db.SetNullableDate(command, "employer2EndDate", employer2EndDate);
        db.SetNullableInt64(command, "emp2WageAmount", employer2WageAmount);
        db.SetNullableInt32(command, "emp2WageQuarter", employer2WageQuarter);
        db.SetNullableInt32(command, "emp2WageYear", employer2WageYear);
        db.SetNullableString(command, "employer3Name", employer3Name);
        db.SetNullableInt32(command, "employer3Ein", employer3Ein);
        db.SetNullableString(command, "emp3PhoneNumber", employer3PhoneNumber);
        db.SetNullableString(command, "emp3AreaCode", employer3AreaCode);
        db.SetNullableString(command, "emp3AddrLine1", employer3AddressLine1);
        db.SetNullableString(command, "emp3AddrLine2", employer3AddressLine2);
        db.SetNullableString(command, "employer3City", employer3City);
        db.SetNullableString(command, "employer3State", employer3State);
        db.SetNullableInt32(command, "emp3ZipCode5", employer3ZipCode5);
        db.SetNullableInt32(command, "emp3ZipCode4", employer3ZipCode4);
        db.SetNullableString(
          command, "emp3ConfirmedInd", employer3ConfirmedIndicator);
        db.SetNullableDate(command, "emp3EffectiveDt", employer3EffectiveDate);
        db.SetNullableDate(command, "employer3EndDate", employer3EndDate);
        db.SetNullableInt64(command, "emp3WageAmount", employer3WageAmount);
        db.SetNullableInt32(command, "emp3WageQuarter", employer3WageQuarter);
        db.SetNullableInt32(command, "emp3WageYear", employer3WageYear);
      });

    entities.InterstateApLocate.CncTransactionDt = cncTransactionDt;
    entities.InterstateApLocate.CncTransSerlNbr = cncTransSerlNbr;
    entities.InterstateApLocate.EmployerEin = employerEin;
    entities.InterstateApLocate.EmployerName = employerName;
    entities.InterstateApLocate.EmployerPhoneNum = employerPhoneNum;
    entities.InterstateApLocate.EmployerEffectiveDate = employerEffectiveDate;
    entities.InterstateApLocate.EmployerEndDate = employerEndDate;
    entities.InterstateApLocate.EmployerConfirmedInd = employerConfirmedInd;
    entities.InterstateApLocate.ResidentialAddressLine1 =
      residentialAddressLine1;
    entities.InterstateApLocate.ResidentialAddressLine2 =
      residentialAddressLine2;
    entities.InterstateApLocate.ResidentialCity = residentialCity;
    entities.InterstateApLocate.ResidentialState = residentialState;
    entities.InterstateApLocate.ResidentialZipCode5 = residentialZipCode5;
    entities.InterstateApLocate.ResidentialZipCode4 = residentialZipCode4;
    entities.InterstateApLocate.MailingAddressLine1 = mailingAddressLine1;
    entities.InterstateApLocate.MailingAddressLine2 = mailingAddressLine2;
    entities.InterstateApLocate.MailingCity = mailingCity;
    entities.InterstateApLocate.MailingState = mailingState;
    entities.InterstateApLocate.MailingZipCode5 = mailingZipCode5;
    entities.InterstateApLocate.MailingZipCode4 = mailingZipCode4;
    entities.InterstateApLocate.ResidentialAddressEffectvDate =
      residentialAddressEffectvDate;
    entities.InterstateApLocate.ResidentialAddressEndDate =
      residentialAddressEndDate;
    entities.InterstateApLocate.ResidentialAddressConfirmInd =
      residentialAddressConfirmInd;
    entities.InterstateApLocate.MailingAddressEffectiveDate =
      mailingAddressEffectiveDate;
    entities.InterstateApLocate.MailingAddressEndDate = mailingAddressEndDate;
    entities.InterstateApLocate.MailingAddressConfirmedInd =
      mailingAddressConfirmedInd;
    entities.InterstateApLocate.HomePhoneNumber = homePhoneNumber;
    entities.InterstateApLocate.WorkPhoneNumber = workPhoneNumber;
    entities.InterstateApLocate.DriversLicState = driversLicState;
    entities.InterstateApLocate.DriversLicenseNum = driversLicenseNum;
    entities.InterstateApLocate.Alias1FirstName = alias1FirstName;
    entities.InterstateApLocate.Alias1MiddleName = alias1MiddleName;
    entities.InterstateApLocate.Alias1LastName = alias1LastName;
    entities.InterstateApLocate.Alias1Suffix = alias1Suffix;
    entities.InterstateApLocate.Alias2FirstName = alias2FirstName;
    entities.InterstateApLocate.Alias2MiddleName = alias2MiddleName;
    entities.InterstateApLocate.Alias2LastName = alias2LastName;
    entities.InterstateApLocate.Alias2Suffix = alias2Suffix;
    entities.InterstateApLocate.Alias3FirstName = alias3FirstName;
    entities.InterstateApLocate.Alias3MiddleName = alias3MiddleName;
    entities.InterstateApLocate.Alias3LastName = alias3LastName;
    entities.InterstateApLocate.Alias3Suffix = alias3Suffix;
    entities.InterstateApLocate.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.InterstateApLocate.CurrentSpouseMiddleName =
      currentSpouseMiddleName;
    entities.InterstateApLocate.CurrentSpouseLastName = currentSpouseLastName;
    entities.InterstateApLocate.CurrentSpouseSuffix = currentSpouseSuffix;
    entities.InterstateApLocate.Occupation = occupation;
    entities.InterstateApLocate.EmployerAddressLine1 = employerAddressLine1;
    entities.InterstateApLocate.EmployerAddressLine2 = employerAddressLine2;
    entities.InterstateApLocate.EmployerCity = employerCity;
    entities.InterstateApLocate.EmployerState = employerState;
    entities.InterstateApLocate.EmployerZipCode5 = employerZipCode5;
    entities.InterstateApLocate.EmployerZipCode4 = employerZipCode4;
    entities.InterstateApLocate.WageQtr = wageQtr;
    entities.InterstateApLocate.WageYear = wageYear;
    entities.InterstateApLocate.WageAmount = wageAmount;
    entities.InterstateApLocate.InsuranceCarrierName = insuranceCarrierName;
    entities.InterstateApLocate.InsurancePolicyNum = insurancePolicyNum;
    entities.InterstateApLocate.LastResAddressLine1 = lastResAddressLine1;
    entities.InterstateApLocate.LastResAddressLine2 = lastResAddressLine2;
    entities.InterstateApLocate.LastResCity = lastResCity;
    entities.InterstateApLocate.LastResState = lastResState;
    entities.InterstateApLocate.LastResZipCode5 = lastResZipCode5;
    entities.InterstateApLocate.LastResZipCode4 = lastResZipCode4;
    entities.InterstateApLocate.LastResAddressDate = lastResAddressDate;
    entities.InterstateApLocate.LastMailAddressLine1 = lastMailAddressLine1;
    entities.InterstateApLocate.LastMailAddressLine2 = lastMailAddressLine2;
    entities.InterstateApLocate.LastMailCity = lastMailCity;
    entities.InterstateApLocate.LastMailState = lastMailState;
    entities.InterstateApLocate.LastMailZipCode5 = lastMailZipCode5;
    entities.InterstateApLocate.LastMailZipCode4 = lastMailZipCode4;
    entities.InterstateApLocate.LastMailAddressDate = lastMailAddressDate;
    entities.InterstateApLocate.LastEmployerName = lastEmployerName;
    entities.InterstateApLocate.LastEmployerDate = lastEmployerDate;
    entities.InterstateApLocate.LastEmployerAddressLine1 =
      lastEmployerAddressLine1;
    entities.InterstateApLocate.LastEmployerAddressLine2 =
      lastEmployerAddressLine2;
    entities.InterstateApLocate.LastEmployerCity = lastEmployerCity;
    entities.InterstateApLocate.LastEmployerState = lastEmployerState;
    entities.InterstateApLocate.LastEmployerZipCode5 = lastEmployerZipCode5;
    entities.InterstateApLocate.LastEmployerZipCode4 = lastEmployerZipCode4;
    entities.InterstateApLocate.ProfessionalLicenses = professionalLicenses;
    entities.InterstateApLocate.WorkAreaCode = workAreaCode;
    entities.InterstateApLocate.HomeAreaCode = homeAreaCode;
    entities.InterstateApLocate.LastEmployerEndDate = lastEmployerEndDate;
    entities.InterstateApLocate.EmployerAreaCode = 0;
    entities.InterstateApLocate.Employer2Name = employer2Name;
    entities.InterstateApLocate.Employer2Ein = employer2Ein;
    entities.InterstateApLocate.Employer2PhoneNumber = employer2PhoneNumber;
    entities.InterstateApLocate.Employer2AreaCode = employer2AreaCode;
    entities.InterstateApLocate.Employer2AddressLine1 = employer2AddressLine1;
    entities.InterstateApLocate.Employer2AddressLine2 = employer2AddressLine2;
    entities.InterstateApLocate.Employer2City = employer2City;
    entities.InterstateApLocate.Employer2State = employer2State;
    entities.InterstateApLocate.Employer2ZipCode5 = employer2ZipCode5;
    entities.InterstateApLocate.Employer2ZipCode4 = employer2ZipCode4;
    entities.InterstateApLocate.Employer2ConfirmedIndicator =
      employer2ConfirmedIndicator;
    entities.InterstateApLocate.Employer2EffectiveDate = employer2EffectiveDate;
    entities.InterstateApLocate.Employer2EndDate = employer2EndDate;
    entities.InterstateApLocate.Employer2WageAmount = employer2WageAmount;
    entities.InterstateApLocate.Employer2WageQuarter = employer2WageQuarter;
    entities.InterstateApLocate.Employer2WageYear = employer2WageYear;
    entities.InterstateApLocate.Employer3Name = employer3Name;
    entities.InterstateApLocate.Employer3Ein = employer3Ein;
    entities.InterstateApLocate.Employer3PhoneNumber = employer3PhoneNumber;
    entities.InterstateApLocate.Employer3AreaCode = employer3AreaCode;
    entities.InterstateApLocate.Employer3AddressLine1 = employer3AddressLine1;
    entities.InterstateApLocate.Employer3AddressLine2 = employer3AddressLine2;
    entities.InterstateApLocate.Employer3City = employer3City;
    entities.InterstateApLocate.Employer3State = employer3State;
    entities.InterstateApLocate.Employer3ZipCode5 = employer3ZipCode5;
    entities.InterstateApLocate.Employer3ZipCode4 = employer3ZipCode4;
    entities.InterstateApLocate.Employer3ConfirmedIndicator =
      employer3ConfirmedIndicator;
    entities.InterstateApLocate.Employer3EffectiveDate = employer3EffectiveDate;
    entities.InterstateApLocate.Employer3EndDate = employer3EndDate;
    entities.InterstateApLocate.Employer3WageAmount = employer3WageAmount;
    entities.InterstateApLocate.Employer3WageQuarter = employer3WageQuarter;
    entities.InterstateApLocate.Employer3WageYear = employer3WageYear;
    entities.InterstateApLocate.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Type1 = db.GetString(reader, 1);
        entities.Ap.Occupation = db.GetNullableString(reader, 2);
        entities.Ap.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.Ap.CurrentSpouseFirstName = db.GetNullableString(reader, 4);
        entities.Ap.HomePhone = db.GetNullableInt32(reader, 5);
        entities.Ap.CurrentSpouseLastName = db.GetNullableString(reader, 6);
        entities.Ap.HomePhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.Ap.WorkPhoneAreaCode = db.GetNullableInt32(reader, 8);
        entities.Ap.WorkPhone = db.GetNullableInt32(reader, 9);
        entities.Ap.FamilyViolenceIndicator = db.GetNullableString(reader, 10);
        entities.Ap.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ap.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress3()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress4()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress5()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress6()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress7()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress7",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress8()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress8",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 10);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return ReadEach("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 3);
        entities.CsePersonLicense.ExpirationDt = db.GetNullableDate(reader, 4);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonLicense.Description = db.GetNullableString(reader, 6);
        entities.CsePersonLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadEmploymentEmployerDomesticEmployerAddress()
  {
    entities.DomesticEmployerAddress.Populated = false;
    entities.Employer.Populated = false;
    entities.Employment.Populated = false;

    return ReadEach("ReadEmploymentEmployerDomesticEmployerAddress",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.Ap.Number);
        db.
          SetNullableDate(command, "endDt", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Employment.Identifier = db.GetDateTime(reader, 0);
        entities.Employment.Type1 = db.GetString(reader, 1);
        entities.Employment.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.Employment.LastQtr = db.GetNullableString(reader, 3);
        entities.Employment.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.Employment.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.Employment.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.Employment.Attribute2NdQtrYr = db.GetNullableInt32(reader, 7);
        entities.Employment.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.Employment.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.Employment.Attribute3RdQtrYr = db.GetNullableInt32(reader, 10);
        entities.Employment.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.Employment.Attribute4ThQtr = db.GetNullableString(reader, 12);
        entities.Employment.Attribute4ThQtrYr = db.GetNullableInt32(reader, 13);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 14);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 15);
        entities.Employment.Name = db.GetNullableString(reader, 16);
        entities.Employment.CreatedTimestamp = db.GetDateTime(reader, 17);
        entities.Employment.CspINumber = db.GetString(reader, 18);
        entities.Employment.EmpId = db.GetNullableInt32(reader, 19);
        entities.Employer.Identifier = db.GetInt32(reader, 19);
        entities.DomesticEmployerAddress.EmpId = db.GetInt32(reader, 19);
        entities.Employment.StartDt = db.GetNullableDate(reader, 20);
        entities.Employment.EndDt = db.GetNullableDate(reader, 21);
        entities.Employer.Ein = db.GetNullableString(reader, 22);
        entities.Employer.Name = db.GetNullableString(reader, 23);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 24);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 25);
        entities.DomesticEmployerAddress.LocationType =
          db.GetString(reader, 26);
        entities.DomesticEmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 27);
        entities.DomesticEmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.DomesticEmployerAddress.CreatedTimestamp =
          db.GetDateTime(reader, 29);
        entities.DomesticEmployerAddress.CreatedBy = db.GetString(reader, 30);
        entities.DomesticEmployerAddress.Street1 =
          db.GetNullableString(reader, 31);
        entities.DomesticEmployerAddress.Street2 =
          db.GetNullableString(reader, 32);
        entities.DomesticEmployerAddress.City =
          db.GetNullableString(reader, 33);
        entities.DomesticEmployerAddress.Identifier =
          db.GetDateTime(reader, 34);
        entities.DomesticEmployerAddress.State =
          db.GetNullableString(reader, 35);
        entities.DomesticEmployerAddress.ZipCode =
          db.GetNullableString(reader, 36);
        entities.DomesticEmployerAddress.Zip4 =
          db.GetNullableString(reader, 37);
        entities.DomesticEmployerAddress.Zip3 =
          db.GetNullableString(reader, 38);
        entities.DomesticEmployerAddress.County =
          db.GetNullableString(reader, 39);
        entities.DomesticEmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        entities.Employment.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Employment.Type1);
        CheckValid<EmployerAddress>("LocationType",
          entities.DomesticEmployerAddress.LocationType);

        return true;
      });
  }

  private bool ReadHealthInsuranceCompany()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.HealthInsuranceCoverage.HicIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.Ap.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadInterstateCaseInterstateApIdentification()
  {
    entities.InterstateCase.Populated = false;
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateCaseInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateCase.Populated = true;
        entities.InterstateApIdentification.Populated = true;
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
    /// A value of ScreenIndentification.
    /// </summary>
    [JsonPropertyName("screenIndentification")]
    public Common ScreenIndentification
    {
      get => screenIndentification ??= new();
      set => screenIndentification = value;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// A value of Addr.
    /// </summary>
    [JsonPropertyName("addr")]
    public DateWorkArea Addr
    {
      get => addr ??= new();
      set => addr = value;
    }

    private Common screenIndentification;
    private CsePerson ap;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private DateWorkArea addr;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    private InterstateApLocate interstateApLocate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ApAliasNameGroup group.</summary>
    [Serializable]
    public class ApAliasNameGroup
    {
      /// <summary>
      /// A value of GapCommon.
      /// </summary>
      [JsonPropertyName("gapCommon")]
      public Common GapCommon
      {
        get => gapCommon ??= new();
        set => gapCommon = value;
      }

      /// <summary>
      /// A value of GapCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gapCsePersonsWorkSet")]
      public CsePersonsWorkSet GapCsePersonsWorkSet
      {
        get => gapCsePersonsWorkSet ??= new();
        set => gapCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GapSsn3.
      /// </summary>
      [JsonPropertyName("gapSsn3")]
      public WorkArea GapSsn3
      {
        get => gapSsn3 ??= new();
        set => gapSsn3 = value;
      }

      /// <summary>
      /// A value of GapSsn2.
      /// </summary>
      [JsonPropertyName("gapSsn2")]
      public WorkArea GapSsn2
      {
        get => gapSsn2 ??= new();
        set => gapSsn2 = value;
      }

      /// <summary>
      /// A value of GapSsn4.
      /// </summary>
      [JsonPropertyName("gapSsn4")]
      public WorkArea GapSsn4
      {
        get => gapSsn4 ??= new();
        set => gapSsn4 = value;
      }

      /// <summary>
      /// A value of Temp1.
      /// </summary>
      [JsonPropertyName("temp1")]
      public Common Temp1
      {
        get => temp1 ??= new();
        set => temp1 = value;
      }

      /// <summary>
      /// A value of Temp2.
      /// </summary>
      [JsonPropertyName("temp2")]
      public Common Temp2
      {
        get => temp2 ??= new();
        set => temp2 = value;
      }

      /// <summary>
      /// A value of Temp3.
      /// </summary>
      [JsonPropertyName("temp3")]
      public Common Temp3
      {
        get => temp3 ??= new();
        set => temp3 = value;
      }

      /// <summary>
      /// A value of Temp4.
      /// </summary>
      [JsonPropertyName("temp4")]
      public Common Temp4
      {
        get => temp4 ??= new();
        set => temp4 = value;
      }

      /// <summary>
      /// A value of Temp5.
      /// </summary>
      [JsonPropertyName("temp5")]
      public Common Temp5
      {
        get => temp5 ??= new();
        set => temp5 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common gapCommon;
      private CsePersonsWorkSet gapCsePersonsWorkSet;
      private WorkArea gapSsn3;
      private WorkArea gapSsn2;
      private WorkArea gapSsn4;
      private Common temp1;
      private Common temp2;
      private Common temp3;
      private Common temp4;
      private Common temp5;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    /// <summary>
    /// A value of CurrentEmployer.
    /// </summary>
    [JsonPropertyName("currentEmployer")]
    public Common CurrentEmployer
    {
      get => currentEmployer ??= new();
      set => currentEmployer = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of ApAliasName.
    /// </summary>
    [JsonIgnore]
    public Array<ApAliasNameGroup> ApAliasName => apAliasName ??= new(
      ApAliasNameGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApAliasName for json serialization.
    /// </summary>
    [JsonPropertyName("apAliasName")]
    [Computed]
    public IList<ApAliasNameGroup> ApAliasName_Json
    {
      get => apAliasName;
      set => ApAliasName.Assign(value);
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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

    /// <summary>
    /// A value of Addr.
    /// </summary>
    [JsonPropertyName("addr")]
    public DateWorkArea Addr
    {
      get => addr ??= new();
      set => addr = value;
    }

    private Common screenIdentification;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common currentEmployer;
    private DateWorkArea current;
    private Code maxDate;
    private InterstateApLocate interstateApLocate;
    private Array<ApAliasNameGroup> apAliasName;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private DateWorkArea addr;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DomesticEmployerAddress.
    /// </summary>
    [JsonPropertyName("domesticEmployerAddress")]
    public EmployerAddress DomesticEmployerAddress
    {
      get => domesticEmployerAddress ??= new();
      set => domesticEmployerAddress = value;
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
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
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

    private EmployerAddress domesticEmployerAddress;
    private Employer employer;
    private IncomeSource employment;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson ap;
    private CsePersonAddress csePersonAddress;
    private CsePersonLicense csePersonLicense;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private CsePerson csePerson;
  }
#endregion
}
