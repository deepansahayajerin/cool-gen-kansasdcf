// Program: SI_GET_DATA_INTERSTATE_AP_LOC_DB, ID: 373008346, model: 746.
// Short name: SWE02737
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_DATA_INTERSTATE_AP_LOC_DB.
/// </summary>
[Serializable]
public partial class SiGetDataInterstateApLocDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_DATA_INTERSTATE_AP_LOC_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetDataInterstateApLocDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetDataInterstateApLocDb.
  /// </summary>
  public SiGetDataInterstateApLocDb(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ----------------------------------------------------------------------------
    // 06/26/2001	M Ramirez			Initial development
    // ----------------------------------------------------------------------------
    export.InterstateCase.ApLocateDataInd = 0;

    if (IsEmpty(import.Ap.Number))
    {
      ExitState = "SI0000_CSENET_AP_LOC_ERROR_RB";

      return;
    }

    if (Equal(import.Current.Date, local.Null1.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    local.Max.Date = new DateTime(2099, 12, 31);

    if (ReadCsePerson())
    {
      if (AsChar(import.FviBypass.Flag) != 'Y')
      {
        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          export.FamilyViolence.Flag =
            entities.CsePerson.FamilyViolenceIndicator ?? Spaces(1);

          return;
        }
      }
    }
    else
    {
      return;
    }

    local.Found.Flag = "";

    foreach(var item in ReadCsePersonAddress2())
    {
      if (!Lt(entities.CsePersonAddress.EndDate, local.Current.Date) && IsEmpty
        (local.Found.Flag))
      {
        local.Found.Flag = "Y";
        export.InterstateCase.ApLocateDataInd = 1;

        if (Lt(import.VerifiedSince.Date, entities.CsePersonAddress.VerifiedDate))
          
        {
          export.InterstateApLocate.ResidentialAddressConfirmInd = "Y";
        }
        else
        {
          export.InterstateApLocate.ResidentialAddressConfirmInd = "N";
        }

        if (Lt(local.Null1.Date, entities.CsePersonAddress.VerifiedDate))
        {
          export.InterstateApLocate.ResidentialAddressEffectvDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          export.InterstateApLocate.ResidentialAddressEffectvDate =
            Date(entities.CsePersonAddress.CreatedTimestamp);
        }

        if (Lt(entities.CsePersonAddress.EndDate, local.Max.Date))
        {
          export.InterstateApLocate.ResidentialAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        export.InterstateApLocate.ResidentialAddressLine1 =
          entities.CsePersonAddress.Street1;
        export.InterstateApLocate.ResidentialAddressLine2 =
          entities.CsePersonAddress.Street2;
        export.InterstateApLocate.ResidentialCity =
          entities.CsePersonAddress.City;
        export.InterstateApLocate.ResidentialState =
          entities.CsePersonAddress.State;
        export.InterstateApLocate.ResidentialZipCode5 =
          entities.CsePersonAddress.ZipCode;
        export.InterstateApLocate.ResidentialZipCode4 =
          entities.CsePersonAddress.Zip4;
      }
      else
      {
        export.InterstateApLocate.LastResAddressDate =
          entities.CsePersonAddress.EndDate;
        export.InterstateApLocate.LastResAddressLine1 =
          entities.CsePersonAddress.Street1;
        export.InterstateApLocate.LastResAddressLine2 =
          entities.CsePersonAddress.Street2;
        export.InterstateApLocate.LastResCity = entities.CsePersonAddress.City;
        export.InterstateApLocate.LastResState =
          entities.CsePersonAddress.State;
        export.InterstateApLocate.LastResZipCode5 =
          entities.CsePersonAddress.ZipCode;
        export.InterstateApLocate.LastResZipCode4 =
          entities.CsePersonAddress.Zip4;

        break;
      }
    }

    local.Found.Flag = "";

    foreach(var item in ReadCsePersonAddress1())
    {
      if (!Lt(entities.CsePersonAddress.EndDate, local.Current.Date) && IsEmpty
        (local.Found.Flag))
      {
        local.Found.Flag = "Y";
        export.InterstateCase.ApLocateDataInd = 1;

        if (Lt(import.VerifiedSince.Date, entities.CsePersonAddress.VerifiedDate))
          
        {
          export.InterstateApLocate.MailingAddressConfirmedInd = "Y";
        }
        else
        {
          export.InterstateApLocate.MailingAddressConfirmedInd = "N";
        }

        if (Lt(local.Null1.Date, entities.CsePersonAddress.VerifiedDate))
        {
          export.InterstateApLocate.MailingAddressEffectiveDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          export.InterstateApLocate.MailingAddressEffectiveDate =
            Date(entities.CsePersonAddress.CreatedTimestamp);
        }

        if (Lt(entities.CsePersonAddress.EndDate, local.Max.Date))
        {
          export.InterstateApLocate.MailingAddressEndDate =
            entities.CsePersonAddress.EndDate;
        }

        export.InterstateApLocate.MailingAddressLine1 =
          entities.CsePersonAddress.Street1;
        export.InterstateApLocate.MailingAddressLine2 =
          entities.CsePersonAddress.Street2;
        export.InterstateApLocate.MailingCity = entities.CsePersonAddress.City;
        export.InterstateApLocate.MailingState =
          entities.CsePersonAddress.State;
        export.InterstateApLocate.MailingZipCode5 =
          entities.CsePersonAddress.ZipCode;
        export.InterstateApLocate.MailingZipCode4 =
          entities.CsePersonAddress.Zip4;
      }
      else
      {
        export.InterstateApLocate.LastMailAddressDate =
          entities.CsePersonAddress.EndDate;
        export.InterstateApLocate.LastMailAddressLine1 =
          entities.CsePersonAddress.Street1;
        export.InterstateApLocate.LastMailAddressLine2 =
          entities.CsePersonAddress.Street2;
        export.InterstateApLocate.LastMailCity = entities.CsePersonAddress.City;
        export.InterstateApLocate.LastMailState =
          entities.CsePersonAddress.State;
        export.InterstateApLocate.LastMailZipCode5 =
          entities.CsePersonAddress.ZipCode;
        export.InterstateApLocate.LastMailZipCode4 =
          entities.CsePersonAddress.Zip4;

        break;
      }
    }

    export.InterstateApLocate.HomeAreaCode =
      entities.CsePerson.HomePhoneAreaCode;
    export.InterstateApLocate.HomePhoneNumber = entities.CsePerson.HomePhone;
    export.InterstateApLocate.WorkAreaCode =
      entities.CsePerson.WorkPhoneAreaCode;
    export.InterstateApLocate.WorkPhoneNumber = entities.CsePerson.WorkPhone;

    foreach(var item in ReadCsePersonLicense())
    {
      switch(TrimEnd(entities.CsePersonLicense.Type1))
      {
        case "D":
          if (IsEmpty(export.InterstateApLocate.DriversLicState))
          {
            export.InterstateApLocate.DriversLicState =
              entities.CsePersonLicense.IssuingState;
            export.InterstateApLocate.DriversLicenseNum =
              entities.CsePersonLicense.Number;
          }

          break;
        case "P":
          if (IsEmpty(export.InterstateApLocate.ProfessionalLicenses))
          {
            export.InterstateApLocate.ProfessionalLicenses =
              entities.CsePersonLicense.Description;
          }
          else
          {
            export.InterstateApLocate.ProfessionalLicenses =
              TrimEnd(export.InterstateApLocate.ProfessionalLicenses) + "," + entities
              .CsePersonLicense.Description;
          }

          break;
        default:
          break;
      }
    }

    if (IsEmpty(import.Ap.FirstName) && IsEmpty(import.Ap.MiddleInitial) && IsEmpty
      (import.Ap.LastName))
    {
      if (AsChar(import.Batch.Flag) == 'Y')
      {
        UseEabReadCsePersonBatch();
      }
      else
      {
        UseEabReadCsePerson();
      }

      if (!IsEmpty(local.AbendData.Type1))
      {
        switch(AsChar(local.AbendData.Type1))
        {
          case 'A':
            switch(TrimEnd(local.AbendData.AdabasResponseCd))
            {
              case "0113":
                ExitState = "ACO_ADABAS_PERSON_NF_113";

                break;
              case "0148":
                ExitState = "ACO_ADABAS_UNAVAILABLE";

                break;
              default:
                ExitState = "ADABAS_READ_UNSUCCESSFUL";

                break;
            }

            break;
          case 'C':
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

            break;
          default:
            ExitState = "ADABAS_INVALID_RETURN_CODE";

            break;
        }

        return;
      }
    }
    else
    {
      MoveCsePersonsWorkSet2(import.Ap, local.CsePersonsWorkSet);
    }

    UseCabRetrieveAliasesAndAltSsn();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    for(local.Alias.Index = 0; local.Alias.Index < local.Alias.Count; ++
      local.Alias.Index)
    {
      if (!local.Alias.CheckSize())
      {
        break;
      }

      switch(local.Alias.Index + 1)
      {
        case 1:
          export.InterstateApLocate.Alias1FirstName =
            local.Alias.Item.GlocalAlias.FirstName;
          export.InterstateApLocate.Alias1MiddleName =
            local.Alias.Item.GlocalAlias.MiddleInitial;
          export.InterstateApLocate.Alias1LastName =
            local.Alias.Item.GlocalAlias.LastName;

          break;
        case 2:
          export.InterstateApLocate.Alias2FirstName =
            local.Alias.Item.GlocalAlias.FirstName;
          export.InterstateApLocate.Alias2MiddleName =
            local.Alias.Item.GlocalAlias.MiddleInitial;
          export.InterstateApLocate.Alias2LastName =
            local.Alias.Item.GlocalAlias.LastName;

          break;
        case 3:
          export.InterstateApLocate.Alias3FirstName =
            local.Alias.Item.GlocalAlias.FirstName;
          export.InterstateApLocate.Alias3MiddleName =
            local.Alias.Item.GlocalAlias.MiddleInitial;
          export.InterstateApLocate.Alias3LastName =
            local.Alias.Item.GlocalAlias.LastName;

          break;
        default:
          goto AfterCycle;
      }
    }

AfterCycle:

    local.Alias.CheckIndex();
    export.InterstateApLocate.CurrentSpouseLastName =
      entities.CsePerson.CurrentSpouseLastName;
    export.InterstateApLocate.CurrentSpouseFirstName =
      entities.CsePerson.CurrentSpouseFirstName;
    export.InterstateApLocate.CurrentSpouseMiddleName =
      entities.CsePerson.CurrentSpouseMi;
    export.InterstateApLocate.Occupation = entities.CsePerson.Occupation;

    if (Lt(local.Null1.Date, import.VerifiedSince.Date))
    {
      // ------------------------------------------------------------
      // Set year to the oldest year that a quarterly wage amount can
      // still be considered verified.
      // EX:	Verified since date is 07/02/2000.
      // Oldest year that is still considered verified for the first
      // quarter (1Q) is 2001, because a quarterly wage received in
      // 1Q of 2000 is over a year old.
      // Oldest year that is still considered verified for 2Q is 2001,
      // because a quarterly wage received in 2Q of 2000 is over
      // a year old.
      // Oldest year that is still considered verified for 3Q is 2001,
      // because a quarterly wage received in 3Q of 2000 MAY be over
      // a year old.
      // Oldest year that is still considered verified for 4Q is 2000,
      // because a quarterly wage received in 4Q of 2000 is less than
      // a year old.
      // ------------------------------------------------------------
      local.Temp.Year = Year(import.VerifiedSince.Date);
      local.Temp.Month = Month(import.VerifiedSince.Date);

      if (local.Temp.Month < 4)
      {
        local.OldestQtrYear.LastQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute2NdQtrYr = local.Temp.Year;
        local.OldestQtrYear.Attribute3RdQtrYr = local.Temp.Year;
        local.OldestQtrYear.Attribute4ThQtrYr = local.Temp.Year;
      }
      else if (local.Temp.Month < 7)
      {
        local.OldestQtrYear.LastQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute2NdQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute3RdQtrYr = local.Temp.Year;
        local.OldestQtrYear.Attribute4ThQtrYr = local.Temp.Year;
      }
      else if (local.Temp.Month < 10)
      {
        local.OldestQtrYear.LastQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute2NdQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute3RdQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute4ThQtrYr = local.Temp.Year;
      }
      else
      {
        local.OldestQtrYear.LastQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute2NdQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute3RdQtrYr = local.Temp.Year + 1;
        local.OldestQtrYear.Attribute4ThQtrYr = local.Temp.Year + 1;
      }
    }

    local.Count.Count = 0;

    foreach(var item in ReadIncomeSourceEmployer())
    {
      // ------------------------------------------------------------------
      // Only list Employment and Military, but don't list those that have
      // a return code indicating the record is wrong
      // ------------------------------------------------------------------
      switch(AsChar(entities.IncomeSource.Type1))
      {
        case 'E':
          if (AsChar(entities.IncomeSource.ReturnCd) == 'N')
          {
            // -------------------------------------
            // Never worked for this employer
            // -------------------------------------
            continue;
          }

          break;
        case 'M':
          if (AsChar(entities.IncomeSource.ReturnCd) == 'N' || AsChar
            (entities.IncomeSource.ReturnCd) == 'U')
          {
            // -------------------------------------
            // No record of military service
            // -------------------------------------
            continue;
          }

          break;
        default:
          continue;
      }

      // ------------------------------------------------------------------
      // If the Employment or Military record is ended, or the return code
      // indicates the record has ended, this record is a Previous income
      // source.
      // We can only display one Previous income source
      // ------------------------------------------------------------------
      if (Lt(entities.IncomeSource.EndDt, local.Current.Date) || AsChar
        (entities.IncomeSource.Type1) == 'E' && AsChar
        (entities.IncomeSource.ReturnCd) == 'F' || AsChar
        (entities.IncomeSource.Type1) == 'E' && AsChar
        (entities.IncomeSource.ReturnCd) == 'Q' || AsChar
        (entities.IncomeSource.Type1) == 'M' && AsChar
        (entities.IncomeSource.ReturnCd) == 'R')
      {
        if (IsEmpty(export.InterstateApLocate.LastEmployerName))
        {
          export.InterstateApLocate.LastEmployerName = entities.Employer.Name;

          if (ReadEmployerAddress())
          {
            export.InterstateApLocate.LastEmployerAddressLine1 =
              entities.EmployerAddress.Street1;
            export.InterstateApLocate.LastEmployerAddressLine2 =
              entities.EmployerAddress.Street2;
            export.InterstateApLocate.LastEmployerCity =
              entities.EmployerAddress.City;
            export.InterstateApLocate.LastEmployerState =
              entities.EmployerAddress.State;
            export.InterstateApLocate.LastEmployerZipCode5 =
              entities.EmployerAddress.ZipCode;
            export.InterstateApLocate.LastEmployerZipCode4 =
              entities.EmployerAddress.Zip4;
          }

          if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt) && Lt
            (entities.IncomeSource.ReturnDt, entities.IncomeSource.EndDt))
          {
            export.InterstateApLocate.LastEmployerDate =
              entities.IncomeSource.ReturnDt;
          }
          else if (Lt(local.Null1.Date, entities.IncomeSource.StartDt))
          {
            export.InterstateApLocate.LastEmployerDate =
              entities.IncomeSource.StartDt;
          }
          else
          {
            export.InterstateApLocate.LastEmployerDate =
              Date(entities.IncomeSource.CreatedTimestamp);
          }

          if (Lt(entities.IncomeSource.EndDt, local.Max.Date))
          {
            export.InterstateApLocate.LastEmployerEndDate =
              entities.IncomeSource.EndDt;
          }
        }
      }
      else
      {
        // ------------------------------------------------------------------
        // This is a current income source.
        // We can only display three current income sources
        // ------------------------------------------------------------------
        ++local.Count.Count;

        switch(local.Count.Count)
        {
          case 1:
            export.InterstateCase.ApLocateDataInd = 1;

            if (IsEmpty(entities.Employer.Ein))
            {
              export.InterstateApLocate.EmployerEin = 0;
            }
            else if (Verify(entities.Employer.Ein, "0123456789") > 0)
            {
              export.InterstateApLocate.EmployerEin = 0;
            }
            else
            {
              export.InterstateApLocate.EmployerEin =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            export.InterstateApLocate.EmployerName = entities.Employer.Name;

            if (ReadEmployerAddress())
            {
              export.InterstateApLocate.EmployerAddressLine1 =
                entities.EmployerAddress.Street1;
              export.InterstateApLocate.EmployerAddressLine2 =
                entities.EmployerAddress.Street2;
              export.InterstateApLocate.EmployerCity =
                entities.EmployerAddress.City;
              export.InterstateApLocate.EmployerState =
                entities.EmployerAddress.State;
              export.InterstateApLocate.EmployerZipCode5 =
                entities.EmployerAddress.ZipCode;
              export.InterstateApLocate.EmployerZipCode4 =
                entities.EmployerAddress.Zip4;
            }

            if (Lt(0, entities.Employer.AreaCode) && !
              IsEmpty(entities.Employer.PhoneNo))
            {
              export.InterstateApLocate.EmployerAreaCode =
                entities.Employer.AreaCode;
              export.InterstateApLocate.EmployerPhoneNum =
                (int?)StringToNumber(entities.Employer.PhoneNo);
            }

            // -----------------------------------------------------------
            // Set Employer Confirmed Indicator
            // Confirmed Indicator = N unless we have received a quarterly
            // wage update within a given timeframe, or the income source
            // verified date has been updated within a given timeframe.
            // If no timeframe is given, the quarterly wage updates are
            // ignored, and the income source verified date simply must be
            // populated
            // -----------------------------------------------------------
            export.InterstateApLocate.EmployerConfirmedInd = "N";

            if (Equal(import.VerifiedSince.Date, local.Null1.Date))
            {
              if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt))
              {
                if (AsChar(entities.IncomeSource.Type1) == 'E')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                  {
                    export.InterstateApLocate.EmployerConfirmedInd = "Y";
                  }
                }
                else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
                {
                  export.InterstateApLocate.EmployerConfirmedInd = "Y";
                }
              }
            }
            else if (!Lt(entities.IncomeSource.LastQtrYr,
              local.OldestQtrYear.LastQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.EmployerConfirmedInd = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute2NdQtrYr,
              local.OldestQtrYear.Attribute2NdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.EmployerConfirmedInd = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute3RdQtrYr,
              local.OldestQtrYear.Attribute3RdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.EmployerConfirmedInd = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute4ThQtrYr,
              local.OldestQtrYear.Attribute4ThQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.EmployerConfirmedInd = "Y";
            }
            else if (!Lt(entities.IncomeSource.ReturnDt,
              import.VerifiedSince.Date))
            {
              if (AsChar(entities.IncomeSource.Type1) == 'E')
              {
                if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                {
                  export.InterstateApLocate.EmployerConfirmedInd = "Y";
                }
              }
              else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
              {
                export.InterstateApLocate.EmployerConfirmedInd = "Y";
              }
            }

            if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt) && Lt
              (entities.IncomeSource.ReturnDt, entities.IncomeSource.EndDt))
            {
              export.InterstateApLocate.EmployerEffectiveDate =
                entities.IncomeSource.ReturnDt;
            }
            else if (Lt(local.Null1.Date, entities.IncomeSource.StartDt))
            {
              export.InterstateApLocate.EmployerEffectiveDate =
                entities.IncomeSource.StartDt;
            }
            else
            {
              export.InterstateApLocate.EmployerEffectiveDate =
                Date(entities.IncomeSource.CreatedTimestamp);
            }

            if (Lt(entities.IncomeSource.EndDt, local.Max.Date))
            {
              export.InterstateApLocate.EmployerEndDate =
                entities.IncomeSource.EndDt;
            }

            // ---------------------------------------------------------------
            // Set Wage Quarter, Wage Year and Wage Amount
            // Should be the latest information.  For example:  4Q is the
            // latest month in the year, but may be from a prior year.  So get
            // the latest year AND latest quarter.
            // Local temp year holds the highest year, preference given to
            // the highest quarter
            // ---------------------------------------------------------------
            local.Temp.Year = 0;

            if (!IsEmpty(entities.IncomeSource.Attribute4ThQtr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrYr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrIncome))
            {
              switch(AsChar(entities.IncomeSource.Attribute4ThQtr))
              {
                case '1':
                  break;
                case '2':
                  break;
                case '3':
                  break;
                case '4':
                  break;
                default:
                  goto Test1;
              }

              if (Lt(entities.IncomeSource.Attribute4ThQtrYr, 1900) || Lt
                (2099, entities.IncomeSource.Attribute4ThQtrYr))
              {
                goto Test1;
              }

              local.Temp.Year =
                entities.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault();
              export.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.IncomeSource.Attribute4ThQtr);
              export.InterstateApLocate.WageYear =
                entities.IncomeSource.Attribute4ThQtrYr;
              export.InterstateApLocate.WageAmount =
                entities.IncomeSource.Attribute4ThQtrIncome;
            }

Test1:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute3RdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute3RdQtr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute3RdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test2;
                }

                if (Lt(entities.IncomeSource.Attribute3RdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute3RdQtrYr))
                {
                  goto Test2;
                }

                local.Temp.Year =
                  entities.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault();
                export.InterstateApLocate.WageQtr =
                  (int?)StringToNumber(entities.IncomeSource.Attribute3RdQtr);
                export.InterstateApLocate.WageYear =
                  entities.IncomeSource.Attribute3RdQtrYr;
                export.InterstateApLocate.WageAmount =
                  entities.IncomeSource.Attribute3RdQtrIncome;
              }
            }

Test2:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute2NdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute2NdQtr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute2NdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test3;
                }

                if (Lt(entities.IncomeSource.Attribute2NdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute2NdQtrYr))
                {
                }
              }

Test3:

              local.Temp.Year =
                entities.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault();
              export.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.IncomeSource.Attribute2NdQtr);
              export.InterstateApLocate.WageYear =
                entities.IncomeSource.Attribute2NdQtrYr;
              export.InterstateApLocate.WageAmount =
                entities.IncomeSource.Attribute2NdQtrIncome;
            }

            if (Lt(local.Temp.Year, entities.IncomeSource.LastQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.LastQtr) && Lt
                (0, entities.IncomeSource.LastQtrYr) && Lt
                (0, entities.IncomeSource.LastQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.LastQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test4;
                }

                if (Lt(entities.IncomeSource.LastQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.LastQtrYr))
                {
                }
              }

Test4:

              local.Temp.Year =
                entities.IncomeSource.LastQtrYr.GetValueOrDefault();
              export.InterstateApLocate.WageQtr =
                (int?)StringToNumber(entities.IncomeSource.LastQtr);
              export.InterstateApLocate.WageYear =
                entities.IncomeSource.LastQtrYr;
              export.InterstateApLocate.WageAmount =
                entities.IncomeSource.LastQtrIncome;
            }

            break;
          case 2:
            if (IsEmpty(entities.Employer.Ein))
            {
              export.InterstateApLocate.Employer2Ein = 0;
            }
            else if (Verify(entities.Employer.Ein, "0123456789") > 0)
            {
              export.InterstateApLocate.Employer2Ein = 0;
            }
            else
            {
              export.InterstateApLocate.Employer2Ein =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            export.InterstateApLocate.Employer2Name = entities.Employer.Name;

            if (ReadEmployerAddress())
            {
              export.InterstateApLocate.Employer2AddressLine1 =
                entities.EmployerAddress.Street1;
              export.InterstateApLocate.Employer2AddressLine2 =
                entities.EmployerAddress.Street2;
              export.InterstateApLocate.Employer2City =
                entities.EmployerAddress.City;
              export.InterstateApLocate.Employer2State =
                entities.EmployerAddress.State;
              export.InterstateApLocate.Employer2ZipCode5 =
                (int?)StringToNumber(entities.EmployerAddress.ZipCode);
              export.InterstateApLocate.Employer2ZipCode4 =
                (int?)StringToNumber(entities.EmployerAddress.Zip4);
            }

            if (Lt(0, entities.Employer.AreaCode) && !
              IsEmpty(entities.Employer.PhoneNo))
            {
              export.InterstateApLocate.Employer2AreaCode =
                NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 3);
                
              export.InterstateApLocate.Employer2PhoneNumber =
                entities.Employer.PhoneNo;
            }

            // -----------------------------------------------------------
            // Set Employer Confirmed Indicator
            // Confirmed Indicator = N unless we have received a quarterly
            // wage update within a given timeframe, or the income source
            // verified date has been updated within a given timeframe.
            // If no timeframe is given, the quarterly wage updates are
            // ignored, and the income source verified date simply must be
            // populated
            // -----------------------------------------------------------
            export.InterstateApLocate.Employer2ConfirmedIndicator = "N";

            if (Equal(import.VerifiedSince.Date, local.Null1.Date))
            {
              if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt))
              {
                if (AsChar(entities.IncomeSource.Type1) == 'E')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                  {
                    export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
                  }
                }
                else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
                {
                  export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
                }
              }
            }
            else if (!Lt(entities.IncomeSource.LastQtrYr,
              local.OldestQtrYear.LastQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute2NdQtrYr,
              local.OldestQtrYear.Attribute2NdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute3RdQtrYr,
              local.OldestQtrYear.Attribute3RdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute4ThQtrYr,
              local.OldestQtrYear.Attribute4ThQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.ReturnDt,
              import.VerifiedSince.Date))
            {
              if (AsChar(entities.IncomeSource.Type1) == 'E')
              {
                if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                {
                  export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
                }
              }
              else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
              {
                export.InterstateApLocate.Employer2ConfirmedIndicator = "Y";
              }
            }

            if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt) && Lt
              (entities.IncomeSource.ReturnDt, entities.IncomeSource.EndDt))
            {
              export.InterstateApLocate.Employer2EffectiveDate =
                entities.IncomeSource.ReturnDt;
            }
            else if (Lt(local.Null1.Date, entities.IncomeSource.StartDt))
            {
              export.InterstateApLocate.Employer2EffectiveDate =
                entities.IncomeSource.StartDt;
            }
            else
            {
              export.InterstateApLocate.Employer2EffectiveDate =
                Date(entities.IncomeSource.CreatedTimestamp);
            }

            if (Lt(entities.IncomeSource.EndDt, local.Max.Date))
            {
              export.InterstateApLocate.Employer2EndDate =
                entities.IncomeSource.EndDt;
            }

            // ---------------------------------------------------------------
            // Set Wage Quarter, Wage Year and Wage Amount
            // Should be the latest information.  For example:  4Q is the
            // latest month in the year, but may be from a prior year.  So get
            // the latest year AND latest quarter.
            // Local temp year holds the highest year, preference given to
            // the highest quarter
            // ---------------------------------------------------------------
            local.Temp.Year = 0;

            if (!IsEmpty(entities.IncomeSource.Attribute4ThQtr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrYr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrIncome))
            {
              switch(AsChar(entities.IncomeSource.Attribute4ThQtr))
              {
                case '1':
                  break;
                case '2':
                  break;
                case '3':
                  break;
                case '4':
                  break;
                default:
                  goto Test5;
              }

              if (Lt(entities.IncomeSource.Attribute4ThQtrYr, 1900) || Lt
                (2099, entities.IncomeSource.Attribute4ThQtrYr))
              {
                goto Test5;
              }

              local.Temp.Year =
                entities.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault();
              export.InterstateApLocate.Employer2WageQuarter =
                (int?)StringToNumber(entities.IncomeSource.Attribute4ThQtr);
              export.InterstateApLocate.Employer2WageYear =
                entities.IncomeSource.Attribute4ThQtrYr;
              export.InterstateApLocate.Employer2WageAmount =
                (long?)entities.IncomeSource.Attribute4ThQtrIncome;
            }

Test5:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute3RdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute3RdQtr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute3RdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test6;
                }

                if (Lt(entities.IncomeSource.Attribute3RdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute3RdQtrYr))
                {
                  goto Test6;
                }

                local.Temp.Year =
                  entities.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer2WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.Attribute3RdQtr);
                export.InterstateApLocate.Employer2WageYear =
                  entities.IncomeSource.Attribute3RdQtrYr;
                export.InterstateApLocate.Employer2WageAmount =
                  (long?)entities.IncomeSource.Attribute3RdQtrIncome;
              }
            }

Test6:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute2NdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute2NdQtr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute2NdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test7;
                }

                if (Lt(entities.IncomeSource.Attribute2NdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute2NdQtrYr))
                {
                  goto Test7;
                }

                local.Temp.Year =
                  entities.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer2WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.Attribute2NdQtr);
                export.InterstateApLocate.Employer2WageYear =
                  entities.IncomeSource.Attribute2NdQtrYr;
                export.InterstateApLocate.Employer2WageAmount =
                  (long?)entities.IncomeSource.Attribute2NdQtrIncome;
              }
            }

Test7:

            if (Lt(local.Temp.Year, entities.IncomeSource.LastQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.LastQtr) && Lt
                (0, entities.IncomeSource.LastQtrYr) && Lt
                (0, entities.IncomeSource.LastQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.LastQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test11;
                }

                if (Lt(entities.IncomeSource.LastQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.LastQtrYr))
                {
                  goto Test11;
                }

                local.Temp.Year =
                  entities.IncomeSource.LastQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer2WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.LastQtr);
                export.InterstateApLocate.Employer2WageYear =
                  entities.IncomeSource.LastQtrYr;
                export.InterstateApLocate.Employer2WageAmount =
                  (long?)entities.IncomeSource.LastQtrIncome;
              }
            }

            break;
          case 3:
            if (IsEmpty(entities.Employer.Ein))
            {
              export.InterstateApLocate.Employer3Ein = 0;
            }
            else if (Verify(entities.Employer.Ein, "0123456789") > 0)
            {
              export.InterstateApLocate.Employer3Ein = 0;
            }
            else
            {
              export.InterstateApLocate.Employer3Ein =
                (int?)StringToNumber(entities.Employer.Ein);
            }

            export.InterstateApLocate.Employer3Name = entities.Employer.Name;

            if (ReadEmployerAddress())
            {
              export.InterstateApLocate.Employer3AddressLine1 =
                entities.EmployerAddress.Street1;
              export.InterstateApLocate.Employer3AddressLine2 =
                entities.EmployerAddress.Street2;
              export.InterstateApLocate.Employer3City =
                entities.EmployerAddress.City;
              export.InterstateApLocate.Employer3State =
                entities.EmployerAddress.State;
              export.InterstateApLocate.Employer3ZipCode5 =
                (int?)StringToNumber(entities.EmployerAddress.ZipCode);
              export.InterstateApLocate.Employer3ZipCode4 =
                (int?)StringToNumber(entities.EmployerAddress.Zip4);
            }

            if (Lt(0, entities.Employer.AreaCode) && !
              IsEmpty(entities.Employer.PhoneNo))
            {
              export.InterstateApLocate.Employer3AreaCode =
                NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 3);
                
              export.InterstateApLocate.Employer3PhoneNumber =
                entities.Employer.PhoneNo;
            }

            // -----------------------------------------------------------
            // Set Employer Confirmed Indicator
            // Confirmed Indicator = N unless we have received a quarterly
            // wage update within a given timeframe, or the income source
            // verified date has been updated within a given timeframe.
            // If no timeframe is given, the quarterly wage updates are
            // ignored, and the income source verified date simply must be
            // populated
            // -----------------------------------------------------------
            export.InterstateApLocate.Employer3ConfirmedIndicator = "N";

            if (Equal(import.VerifiedSince.Date, local.Null1.Date))
            {
              if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt))
              {
                if (AsChar(entities.IncomeSource.Type1) == 'E')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                  {
                    export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
                  }
                }
                else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
                {
                  export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
                }
              }
            }
            else if (!Lt(entities.IncomeSource.LastQtrYr,
              local.OldestQtrYear.LastQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute2NdQtrYr,
              local.OldestQtrYear.Attribute2NdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute3RdQtrYr,
              local.OldestQtrYear.Attribute3RdQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.Attribute4ThQtrYr,
              local.OldestQtrYear.Attribute4ThQtrYr.GetValueOrDefault()))
            {
              export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
            }
            else if (!Lt(entities.IncomeSource.ReturnDt,
              import.VerifiedSince.Date))
            {
              if (AsChar(entities.IncomeSource.Type1) == 'E')
              {
                if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
                {
                  export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
                }
              }
              else if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
              {
                export.InterstateApLocate.Employer3ConfirmedIndicator = "Y";
              }
            }

            if (Lt(local.Null1.Date, entities.IncomeSource.ReturnDt) && Lt
              (entities.IncomeSource.ReturnDt, entities.IncomeSource.EndDt))
            {
              export.InterstateApLocate.Employer3EffectiveDate =
                entities.IncomeSource.ReturnDt;
            }
            else if (Lt(local.Null1.Date, entities.IncomeSource.StartDt))
            {
              export.InterstateApLocate.Employer3EffectiveDate =
                entities.IncomeSource.StartDt;
            }
            else
            {
              export.InterstateApLocate.Employer3EffectiveDate =
                Date(entities.IncomeSource.CreatedTimestamp);
            }

            if (Lt(entities.IncomeSource.EndDt, local.Max.Date))
            {
              export.InterstateApLocate.Employer3EndDate =
                entities.IncomeSource.EndDt;
            }

            // ---------------------------------------------------------------
            // Set Wage Quarter, Wage Year and Wage Amount
            // Should be the latest information.  For example:  4Q is the
            // latest month in the year, but may be from a prior year.  So get
            // the latest year AND latest quarter.
            // Local temp year holds the highest year, preference given to
            // the highest quarter
            // ---------------------------------------------------------------
            local.Temp.Year = 0;

            if (!IsEmpty(entities.IncomeSource.Attribute4ThQtr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrYr) && Lt
              (0, entities.IncomeSource.Attribute4ThQtrIncome))
            {
              switch(AsChar(entities.IncomeSource.Attribute4ThQtr))
              {
                case '1':
                  break;
                case '2':
                  break;
                case '3':
                  break;
                case '4':
                  break;
                default:
                  goto Test8;
              }

              if (Lt(entities.IncomeSource.Attribute4ThQtrYr, 1900) || Lt
                (2099, entities.IncomeSource.Attribute4ThQtrYr))
              {
                goto Test8;
              }

              local.Temp.Year =
                entities.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault();
              export.InterstateApLocate.Employer3WageQuarter =
                (int?)StringToNumber(entities.IncomeSource.Attribute4ThQtr);
              export.InterstateApLocate.Employer3WageYear =
                entities.IncomeSource.Attribute4ThQtrYr;
              export.InterstateApLocate.Employer3WageAmount =
                (long?)entities.IncomeSource.Attribute4ThQtrIncome;
            }

Test8:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute3RdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute3RdQtr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute3RdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute3RdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test9;
                }

                if (Lt(entities.IncomeSource.Attribute3RdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute3RdQtrYr))
                {
                  goto Test9;
                }

                local.Temp.Year =
                  entities.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer3WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.Attribute3RdQtr);
                export.InterstateApLocate.Employer3WageYear =
                  entities.IncomeSource.Attribute3RdQtrYr;
                export.InterstateApLocate.Employer3WageAmount =
                  (long?)entities.IncomeSource.Attribute3RdQtrIncome;
              }
            }

Test9:

            if (Lt(local.Temp.Year, entities.IncomeSource.Attribute2NdQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.Attribute2NdQtr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrYr) && Lt
                (0, entities.IncomeSource.Attribute2NdQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.Attribute2NdQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test10;
                }

                if (Lt(entities.IncomeSource.Attribute2NdQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.Attribute2NdQtrYr))
                {
                  goto Test10;
                }

                local.Temp.Year =
                  entities.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer3WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.Attribute2NdQtr);
                export.InterstateApLocate.Employer3WageYear =
                  entities.IncomeSource.Attribute2NdQtrYr;
                export.InterstateApLocate.Employer3WageAmount =
                  (long?)entities.IncomeSource.Attribute2NdQtrIncome;
              }
            }

Test10:

            if (Lt(local.Temp.Year, entities.IncomeSource.LastQtrYr))
            {
              if (!IsEmpty(entities.IncomeSource.LastQtr) && Lt
                (0, entities.IncomeSource.LastQtrYr) && Lt
                (0, entities.IncomeSource.LastQtrIncome))
              {
                switch(AsChar(entities.IncomeSource.LastQtr))
                {
                  case '1':
                    break;
                  case '2':
                    break;
                  case '3':
                    break;
                  case '4':
                    break;
                  default:
                    goto Test11;
                }

                if (Lt(entities.IncomeSource.LastQtrYr, 1900) || Lt
                  (2099, entities.IncomeSource.LastQtrYr))
                {
                  goto Test11;
                }

                local.Temp.Year =
                  entities.IncomeSource.LastQtrYr.GetValueOrDefault();
                export.InterstateApLocate.Employer3WageQuarter =
                  (int?)StringToNumber(entities.IncomeSource.LastQtr);
                export.InterstateApLocate.Employer3WageYear =
                  entities.IncomeSource.LastQtrYr;
                export.InterstateApLocate.Employer3WageAmount =
                  (long?)entities.IncomeSource.LastQtrIncome;
              }
            }

            break;
          default:
            break;
        }
      }

Test11:
      ;
    }

    foreach(var item in ReadHealthInsuranceCoverage())
    {
      if (ReadHealthInsuranceCompany())
      {
        export.InterstateApLocate.InsuranceCarrierName =
          entities.HealthInsuranceCompany.InsurancePolicyCarrier;
        export.InterstateApLocate.InsurancePolicyNum =
          entities.HealthInsuranceCoverage.InsurancePolicyNumber;

        return;
      }
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveNamesToAlias(CabRetrieveAliasesAndAltSsn.Export.
    NamesGroup source, Local.AliasGroup target)
  {
    target.GlocalAlias.Assign(source.Gnames);
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.Names.CopyTo(local.Alias, MoveNamesToAlias);
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Ap.Number;
    useImport.Current.Date = local.Current.Date;
    useExport.AbendData.Assign(local.AbendData);
    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = import.Ap.Number;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 5);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 6);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 8);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 13);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 13);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return ReadEach("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "expirationDt", local.Current.Date.GetValueOrDefault());
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

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.State = db.GetNullableString(reader, 5);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 8);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
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

  private IEnumerable<bool> ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "policyExpDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceEmployer()
  {
    entities.IncomeSource.Populated = false;
    entities.Employer.Populated = false;

    return ReadEach("ReadIncomeSourceEmployer",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 7);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.IncomeSource.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 10);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.IncomeSource.Attribute4ThQtr =
          db.GetNullableString(reader, 12);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 13);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 14);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 15);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.IncomeSource.CspINumber = db.GetString(reader, 17);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 18);
        entities.Employer.Identifier = db.GetInt32(reader, 18);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 19);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 20);
        entities.Employer.Ein = db.GetNullableString(reader, 21);
        entities.Employer.Name = db.GetNullableString(reader, 22);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 23);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 24);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

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
    /// A value of FviBypass.
    /// </summary>
    [JsonPropertyName("fviBypass")]
    public Common FviBypass
    {
      get => fviBypass ??= new();
      set => fviBypass = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of VerifiedSince.
    /// </summary>
    [JsonPropertyName("verifiedSince")]
    public DateWorkArea VerifiedSince
    {
      get => verifiedSince ??= new();
      set => verifiedSince = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    private Common fviBypass;
    private CsePersonsWorkSet ap;
    private DateWorkArea current;
    private DateWorkArea verifiedSince;
    private Common batch;
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
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    public Common FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    private InterstateCase interstateCase;
    private InterstateApLocate interstateApLocate;
    private Common familyViolence;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AliasGroup group.</summary>
    [Serializable]
    public class AliasGroup
    {
      /// <summary>
      /// A value of GlocalAlias.
      /// </summary>
      [JsonPropertyName("glocalAlias")]
      public CsePersonsWorkSet GlocalAlias
      {
        get => glocalAlias ??= new();
        set => glocalAlias = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet glocalAlias;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Alias.
    /// </summary>
    [JsonIgnore]
    public Array<AliasGroup> Alias => alias ??= new(AliasGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alias for json serialization.
    /// </summary>
    [JsonPropertyName("alias")]
    [Computed]
    public IList<AliasGroup> Alias_Json
    {
      get => alias;
      set => Alias.Assign(value);
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
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
    /// A value of OldestQtrYear.
    /// </summary>
    [JsonPropertyName("oldestQtrYear")]
    public IncomeSource OldestQtrYear
    {
      get => oldestQtrYear ??= new();
      set => oldestQtrYear = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<AliasGroup> alias;
    private Common found;
    private Common count;
    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private IncomeSource oldestQtrYear;
    private DateWorkArea temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
    private IncomeSource incomeSource;
    private Employer employer;
    private EmployerAddress employerAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
