// Program: SI_GET_DATA_INTERSTATE_PART_DBS, ID: 373331279, model: 746.
// Short name: SWE02743
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_DATA_INTERSTATE_PART_DBS.
/// </summary>
[Serializable]
public partial class SiGetDataInterstatePartDbs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_DATA_INTERSTATE_PART_DBS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetDataInterstatePartDbs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetDataInterstatePartDbs.
  /// </summary>
  public SiGetDataInterstatePartDbs(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //                    M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------
    // 03/01/2002	M Ramirez			Initial Development
    // -----------------------------------------------------------------
    if (Equal(import.Current.Date, local.NullDateWorkArea.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = import.Current.Date;
    }

    export.InterstateCase.ParticipantDataInd = 0;

    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'C' && !
        Lt(local.Current.Date, entities.Case1.StatusDate))
      {
        local.Current.Date = entities.Case1.StatusDate;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // Find AR
    // ---------------------------------------------------------
    if (ReadCsePerson2())
    {
      // ---------------------------------------------------------
      // Create Participant Datablock for AR
      // ---------------------------------------------------------
      local.Ar.Number = entities.CsePerson.Number;

      if (AsChar(entities.CsePerson.Type1) == 'O')
      {
        // mjr
        // -----------------------------------------
        // 05/20/2002
        // Verify AR meets the minimum requirements
        // ------------------------------------------------------
        if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
          (import.InterstateCase.ActionCode) == 'U')
        {
          if (Equal(import.InterstateCase.FunctionalTypeCode, "PAT"))
          {
            // mjr
            // -----------------------------------------
            // 05/20/2002
            // An Organization doesn't meet the minimum requirements
            // for PAT-R transactions because a DOB is required
            // ------------------------------------------------------
            goto Read1;
          }
        }

        export.InterstateCase.ParticipantDataInd = 1;

        export.Group.Index =
          export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
        export.Group.CheckSize();

        export.Group.Update.G.NameLast = entities.CsePerson.OrganizationName;
        export.Group.Update.G.NameFirst = "ORGANIZATION";
        export.Group.Update.G.Relationship = "AR";
        export.Group.Update.G.Status = entities.Case1.Status;
      }
      else
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        local.AbendData.Assign(local.NullAbendData);

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

        // mjr
        // -----------------------------------------
        // 05/20/2002
        // Verify AR meets the minimum requirements
        // ------------------------------------------------------
        if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
          (import.InterstateCase.ActionCode) == 'U')
        {
          if (Equal(import.InterstateCase.FunctionalTypeCode, "PAT"))
          {
            if (Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
            {
              goto Read1;
            }
          }
        }

        export.InterstateCase.ParticipantDataInd = 1;

        export.Group.Index =
          export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
        export.Group.CheckSize();

        export.Group.Update.GexportParticipant.Number =
          entities.CsePerson.Number;
        export.Group.Update.G.NameLast = local.CsePersonsWorkSet.LastName;
        export.Group.Update.G.NameFirst = local.CsePersonsWorkSet.FirstName;
        export.Group.Update.G.NameMiddle =
          local.CsePersonsWorkSet.MiddleInitial;
        export.Group.Update.G.NameSuffix = "";
        export.Group.Update.G.DateOfBirth = local.CsePersonsWorkSet.Dob;

        if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
          Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
        {
          export.Group.Update.G.Ssn = local.CsePersonsWorkSet.Ssn;
        }

        export.Group.Update.G.Sex = local.CsePersonsWorkSet.Sex;
        export.Group.Update.G.Race = entities.CsePerson.Race;
        export.Group.Update.G.Relationship = "AR";
        export.Group.Update.G.Status = entities.Case1.Status;

        if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
          IsEmpty(entities.CsePerson.BirthPlaceState))
        {
          export.Group.Update.G.PlaceOfBirth =
            TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
            .CsePerson.BirthPlaceState;
        }
        else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
        {
          export.Group.Update.G.PlaceOfBirth =
            entities.CsePerson.BirthPlaceCity;
        }
        else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
        {
          export.Group.Update.G.PlaceOfBirth =
            entities.CsePerson.BirthPlaceState;
        }
        else
        {
        }

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          goto Test;
        }

        // mjr
        // --------------------------------------------
        // 05/20/2002
        // Address should not be sent for AR since the other state
        // should send all correspondence to us directly
        // ---------------------------------------------------------
        foreach(var item in ReadIncomeSourceEmployer())
        {
          switch(AsChar(entities.IncomeSource.Type1))
          {
            case 'E':
              switch(AsChar(entities.IncomeSource.ReturnCd))
              {
                case 'E':
                  if (Lt(local.NullDateWorkArea.Date,
                    entities.IncomeSource.ReturnDt))
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "Y";
                    export.Group.Update.G.EmployerVerifiedDate =
                      entities.IncomeSource.ReturnDt;
                  }
                  else
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "N";
                    export.Group.Update.G.EmployerVerifiedDate =
                      local.NullDateWorkArea.Date;
                  }

                  break;
                case 'N':
                  // -------------------------------------
                  // Never worked for this employer
                  // -------------------------------------
                  continue;
                default:
                  break;
              }

              break;
            case 'M':
              switch(AsChar(entities.IncomeSource.ReturnCd))
              {
                case 'A':
                  if (Lt(local.NullDateWorkArea.Date,
                    entities.IncomeSource.ReturnDt))
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "Y";
                    export.Group.Update.G.EmployerVerifiedDate =
                      entities.IncomeSource.ReturnDt;
                  }
                  else
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "N";
                    export.Group.Update.G.EmployerVerifiedDate =
                      local.NullDateWorkArea.Date;
                  }

                  break;
                case 'R':
                  if (Lt(local.NullDateWorkArea.Date,
                    entities.IncomeSource.ReturnDt))
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "Y";
                    export.Group.Update.G.EmployerVerifiedDate =
                      entities.IncomeSource.ReturnDt;
                  }
                  else
                  {
                    export.Group.Update.G.EmployerConfirmedInd = "N";
                    export.Group.Update.G.EmployerVerifiedDate =
                      local.NullDateWorkArea.Date;
                  }

                  break;
                default:
                  // -------------------------------------
                  // No record of military service
                  // -------------------------------------
                  continue;
              }

              break;
            default:
              continue;
          }

          export.Group.Update.G.EmployerName = entities.Employer.Name;

          if (IsEmpty(entities.Employer.Ein))
          {
            export.Group.Update.G.EmployerEin = 0;
          }
          else if (Verify(entities.Employer.Ein, "0123456789") > 0)
          {
            export.Group.Update.G.EmployerEin = 0;
          }
          else
          {
            export.Group.Update.G.EmployerEin =
              (int?)StringToNumber(entities.Employer.Ein);
          }

          if (Lt(0, entities.Employer.AreaCode) && !
            IsEmpty(entities.Employer.PhoneNo))
          {
            export.Group.Update.G.WorkAreaCode =
              NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13,
              3);
            export.Group.Update.G.WorkPhone = entities.Employer.PhoneNo;
          }

          if (ReadEmployerAddress())
          {
            export.Group.Update.G.EmployerAddressLine1 =
              entities.EmployerAddress.Street1;
            export.Group.Update.G.EmployerAddressLine2 =
              entities.EmployerAddress.Street2;
            export.Group.Update.G.EmployerCity = entities.EmployerAddress.City;
            export.Group.Update.G.EmployerState =
              entities.EmployerAddress.State;
            export.Group.Update.G.EmployerZipCode5 =
              entities.EmployerAddress.ZipCode;
            export.Group.Update.G.EmployerZipCode4 =
              entities.EmployerAddress.Zip4;
          }

          break;
        }
      }

Test:

      export.ArFound.Flag = "Y";
    }

Read1:

    // ---------------------------------------------------------
    // Find MO (if different than AR)
    // ---------------------------------------------------------
    if (ReadCsePerson1())
    {
      // ---------------------------------------------------------
      // Create Participant Datablock for MO
      // ---------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.AbendData.Assign(local.NullAbendData);

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

      // mjr
      // -----------------------------------------
      // 05/20/2002
      // Verify MO meets the minimum requirements
      // ------------------------------------------------------
      if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
        (import.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(import.InterstateCase.FunctionalTypeCode, "PAT"))
        {
          if (Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
          {
            goto Read2;
          }
        }
      }

      export.InterstateCase.ParticipantDataInd =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;

      export.Group.Index =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
      export.Group.CheckSize();

      export.Group.Update.GexportParticipant.Number = entities.CsePerson.Number;
      export.Group.Update.G.NameLast = local.CsePersonsWorkSet.LastName;
      export.Group.Update.G.NameFirst = local.CsePersonsWorkSet.FirstName;
      export.Group.Update.G.NameMiddle = local.CsePersonsWorkSet.MiddleInitial;
      export.Group.Update.G.NameSuffix = "";
      export.Group.Update.G.DateOfBirth = local.CsePersonsWorkSet.Dob;

      if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
        Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
      {
        export.Group.Update.G.Ssn = local.CsePersonsWorkSet.Ssn;
      }

      export.Group.Update.G.Sex = local.CsePersonsWorkSet.Sex;
      export.Group.Update.G.Race = entities.CsePerson.Race;

      // ------------------------------------------------------
      // Since the AR and the Mother are different people, the
      // mother is a Non-custodial Parent (A)
      // ------------------------------------------------------
      export.Group.Update.G.Relationship = "A";
      export.Group.Update.G.Status = entities.Case1.Status;

      if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
        IsEmpty(entities.CsePerson.BirthPlaceState))
      {
        export.Group.Update.G.PlaceOfBirth =
          TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
          .CsePerson.BirthPlaceState;
      }
      else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
      {
        export.Group.Update.G.PlaceOfBirth = entities.CsePerson.BirthPlaceCity;
      }
      else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
      {
        export.Group.Update.G.PlaceOfBirth = entities.CsePerson.BirthPlaceState;
      }
      else
      {
      }

      if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
      {
        goto Read2;
      }

      if (ReadCsePersonAddress())
      {
        export.Group.Update.G.AddressLine1 = entities.CsePersonAddress.Street1;
        export.Group.Update.G.AddressLine2 = entities.CsePersonAddress.Street2;
        export.Group.Update.G.City = entities.CsePersonAddress.City;
        export.Group.Update.G.State = entities.CsePersonAddress.State;
        export.Group.Update.G.ZipCode5 = entities.CsePersonAddress.ZipCode;
        export.Group.Update.G.ZipCode4 = entities.CsePersonAddress.Zip4;

        if (Lt(local.NullDateWorkArea.Date,
          entities.CsePersonAddress.VerifiedDate))
        {
          export.Group.Update.G.AddressConfirmedInd = "Y";
          export.Group.Update.G.AddressVerifiedDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          export.Group.Update.G.AddressConfirmedInd = "N";
          export.Group.Update.G.AddressVerifiedDate =
            local.NullDateWorkArea.Date;
        }
      }

      foreach(var item in ReadIncomeSourceEmployer())
      {
        switch(AsChar(entities.IncomeSource.Type1))
        {
          case 'E':
            switch(AsChar(entities.IncomeSource.ReturnCd))
            {
              case 'E':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              case 'N':
                // -------------------------------------
                // Never worked for this employer
                // -------------------------------------
                continue;
              default:
                break;
            }

            break;
          case 'M':
            switch(AsChar(entities.IncomeSource.ReturnCd))
            {
              case 'A':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              case 'R':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              default:
                // -------------------------------------
                // No record of military service
                // -------------------------------------
                continue;
            }

            break;
          default:
            continue;
        }

        export.Group.Update.G.EmployerName = entities.Employer.Name;

        if (IsEmpty(entities.Employer.Ein))
        {
          export.Group.Update.G.EmployerEin = 0;
        }
        else if (Verify(entities.Employer.Ein, "0123456789") > 0)
        {
          export.Group.Update.G.EmployerEin = 0;
        }
        else
        {
          export.Group.Update.G.EmployerEin =
            (int?)StringToNumber(entities.Employer.Ein);
        }

        if (Lt(0, entities.Employer.AreaCode) && !
          IsEmpty(entities.Employer.PhoneNo))
        {
          export.Group.Update.G.WorkAreaCode =
            NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13, 3);
            
          export.Group.Update.G.WorkPhone = entities.Employer.PhoneNo;
        }

        if (ReadEmployerAddress())
        {
          export.Group.Update.G.EmployerAddressLine1 =
            entities.EmployerAddress.Street1;
          export.Group.Update.G.EmployerAddressLine2 =
            entities.EmployerAddress.Street2;
          export.Group.Update.G.EmployerCity = entities.EmployerAddress.City;
          export.Group.Update.G.EmployerState = entities.EmployerAddress.State;
          export.Group.Update.G.EmployerZipCode5 =
            entities.EmployerAddress.ZipCode;
          export.Group.Update.G.EmployerZipCode4 =
            entities.EmployerAddress.Zip4;
        }

        break;
      }
    }

Read2:

    // ---------------------------------------------------------
    // Find Children
    // ---------------------------------------------------------
    if (import.Children.Count > 0)
    {
      import.Children.Index = 0;

      for(var limit = import.Children.Count; import.Children.Index < limit; ++
        import.Children.Index)
      {
        if (!import.Children.CheckSize())
        {
          break;
        }

        if (ReadCsePersonCaseRole())
        {
          // ---------------------------------------------------------
          // Create Participant Datablock for Child
          // ---------------------------------------------------------
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          local.AbendData.Assign(local.NullAbendData);

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

          // mjr
          // -----------------------------------------
          // 03/01/2002
          // Verify CH meets the minimum requirements
          // ------------------------------------------------------
          if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
            (import.InterstateCase.ActionCode) == 'U')
          {
            if (Equal(import.InterstateCase.FunctionalTypeCode, "EST") || Equal
              (import.InterstateCase.FunctionalTypeCode, "ENF") || Equal
              (import.InterstateCase.FunctionalTypeCode, "PAT"))
            {
              if (Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
                
              {
                continue;
              }
            }
          }

          if (export.InterstateCase.ParticipantDataInd.GetValueOrDefault() >= 9)
          {
            return;
          }

          export.InterstateCase.ParticipantDataInd =
            export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;

          export.Group.Index =
            export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
          export.Group.CheckSize();

          export.Group.Update.GexportParticipant.Number =
            entities.CsePerson.Number;
          export.Group.Update.G.NameLast = local.CsePersonsWorkSet.LastName;
          export.Group.Update.G.NameFirst = local.CsePersonsWorkSet.FirstName;
          export.Group.Update.G.NameMiddle =
            local.CsePersonsWorkSet.MiddleInitial;
          export.Group.Update.G.NameSuffix = "";
          export.Group.Update.G.DateOfBirth = local.CsePersonsWorkSet.Dob;

          if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
            Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
          {
            export.Group.Update.G.Ssn = local.CsePersonsWorkSet.Ssn;
          }

          export.Group.Update.G.Sex = local.CsePersonsWorkSet.Sex;
          export.Group.Update.G.Race = entities.CsePerson.Race;
          export.Group.Update.G.Relationship = "CH";
          export.Group.Update.G.Status = entities.Case1.Status;
          export.Group.Update.G.DependentRelationCp = entities.CaseRole.RelToAr;

          if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
            IsEmpty(entities.CsePerson.BirthPlaceState))
          {
            export.Group.Update.G.PlaceOfBirth =
              TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
              .CsePerson.BirthPlaceState;
          }
          else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
          {
            export.Group.Update.G.PlaceOfBirth =
              entities.CsePerson.BirthPlaceCity;
          }
          else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
          {
            export.Group.Update.G.PlaceOfBirth =
              entities.CsePerson.BirthPlaceState;
          }
          else
          {
          }

          export.Group.Update.G.ChildPaternityStatus =
            entities.CsePerson.PaternityEstablishedIndicator;
          export.ChFound.Flag = "Y";
        }
      }

      import.Children.CheckIndex();
    }
    else
    {
      foreach(var item in ReadCsePerson4())
      {
        // ---------------------------------------------------------
        // Create Participant Datablock for Child
        // ---------------------------------------------------------
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        local.AbendData.Assign(local.NullAbendData);

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

        // mjr
        // -----------------------------------------
        // 03/01/2002
        // Verify CH meets the minimum requirements
        // ------------------------------------------------------
        if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
          (import.InterstateCase.ActionCode) == 'U')
        {
          if (Equal(import.InterstateCase.FunctionalTypeCode, "EST") || Equal
            (import.InterstateCase.FunctionalTypeCode, "ENF") || Equal
            (import.InterstateCase.FunctionalTypeCode, "PAT"))
          {
            if (Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
            {
              continue;
            }
          }
        }

        if (export.InterstateCase.ParticipantDataInd.GetValueOrDefault() >= 9)
        {
          return;
        }

        export.InterstateCase.ParticipantDataInd =
          export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;

        export.Group.Index =
          export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
        export.Group.CheckSize();

        export.Group.Update.GexportParticipant.Number =
          entities.CsePerson.Number;
        export.Group.Update.G.NameLast = local.CsePersonsWorkSet.LastName;
        export.Group.Update.G.NameFirst = local.CsePersonsWorkSet.FirstName;
        export.Group.Update.G.NameMiddle =
          local.CsePersonsWorkSet.MiddleInitial;
        export.Group.Update.G.NameSuffix = "";
        export.Group.Update.G.DateOfBirth = local.CsePersonsWorkSet.Dob;

        if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
          Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
        {
          export.Group.Update.G.Ssn = local.CsePersonsWorkSet.Ssn;
        }

        export.Group.Update.G.Sex = local.CsePersonsWorkSet.Sex;
        export.Group.Update.G.Race = entities.CsePerson.Race;
        export.Group.Update.G.Relationship = "CH";
        export.Group.Update.G.Status = entities.Case1.Status;

        if (ReadCaseRole())
        {
          export.Group.Update.G.DependentRelationCp = entities.CaseRole.RelToAr;
        }

        if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
          IsEmpty(entities.CsePerson.BirthPlaceState))
        {
          export.Group.Update.G.PlaceOfBirth =
            TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
            .CsePerson.BirthPlaceState;
        }
        else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
        {
          export.Group.Update.G.PlaceOfBirth =
            entities.CsePerson.BirthPlaceCity;
        }
        else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
        {
          export.Group.Update.G.PlaceOfBirth =
            entities.CsePerson.BirthPlaceState;
        }
        else
        {
        }

        export.Group.Update.G.ChildPaternityStatus =
          entities.CsePerson.PaternityEstablishedIndicator;
        export.ChFound.Flag = "Y";
      }
    }

    // ---------------------------------------------------------
    // Get APs, excluding the one listed in the AP ID Datablock
    // ---------------------------------------------------------
    foreach(var item in ReadCsePerson3())
    {
      if (Equal(entities.CsePerson.Number, import.PrimaryAp.Number))
      {
        continue;
      }

      // ---------------------------------------------------------
      // Create Participant Datablock for AP
      // ---------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.AbendData.Assign(local.NullAbendData);

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

      // mjr
      // -----------------------------------------
      // 05/20/2002
      // Verify AP meets the minimum requirements
      // ------------------------------------------------------
      if (AsChar(import.InterstateCase.ActionCode) == 'R' || AsChar
        (import.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(import.InterstateCase.FunctionalTypeCode, "PAT"))
        {
          if (Equal(local.CsePersonsWorkSet.Dob, local.NullDateWorkArea.Date))
          {
            continue;
          }
        }
      }

      if (export.InterstateCase.ParticipantDataInd.GetValueOrDefault() >= 9)
      {
        return;
      }

      export.InterstateCase.ParticipantDataInd =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() + 1;

      export.Group.Index =
        export.InterstateCase.ParticipantDataInd.GetValueOrDefault() - 1;
      export.Group.CheckSize();

      export.Group.Update.GexportParticipant.Number = entities.CsePerson.Number;
      export.Group.Update.G.NameLast = local.CsePersonsWorkSet.LastName;
      export.Group.Update.G.NameFirst = local.CsePersonsWorkSet.FirstName;
      export.Group.Update.G.NameMiddle = local.CsePersonsWorkSet.MiddleInitial;
      export.Group.Update.G.NameSuffix = "";
      export.Group.Update.G.DateOfBirth = local.CsePersonsWorkSet.Dob;

      if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && !
        Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
      {
        export.Group.Update.G.Ssn = local.CsePersonsWorkSet.Ssn;
      }

      export.Group.Update.G.Sex = local.CsePersonsWorkSet.Sex;
      export.Group.Update.G.Race = entities.CsePerson.Race;

      // ------------------------------------------------------
      // All APs included in Participant DBs are Putative (P)
      // ------------------------------------------------------
      export.Group.Update.G.Relationship = "P";
      export.Group.Update.G.Status = entities.Case1.Status;

      if (!IsEmpty(entities.CsePerson.BirthPlaceCity) && !
        IsEmpty(entities.CsePerson.BirthPlaceState))
      {
        export.Group.Update.G.PlaceOfBirth =
          TrimEnd(entities.CsePerson.BirthPlaceCity) + ", " + entities
          .CsePerson.BirthPlaceState;
      }
      else if (!IsEmpty(entities.CsePerson.BirthPlaceCity))
      {
        export.Group.Update.G.PlaceOfBirth = entities.CsePerson.BirthPlaceCity;
      }
      else if (!IsEmpty(entities.CsePerson.BirthPlaceState))
      {
        export.Group.Update.G.PlaceOfBirth = entities.CsePerson.BirthPlaceState;
      }
      else
      {
      }

      if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
      {
        continue;
      }

      if (ReadCsePersonAddress())
      {
        export.Group.Update.G.AddressLine1 = entities.CsePersonAddress.Street1;
        export.Group.Update.G.AddressLine2 = entities.CsePersonAddress.Street2;
        export.Group.Update.G.City = entities.CsePersonAddress.City;
        export.Group.Update.G.State = entities.CsePersonAddress.State;
        export.Group.Update.G.ZipCode5 = entities.CsePersonAddress.ZipCode;
        export.Group.Update.G.ZipCode4 = entities.CsePersonAddress.Zip4;

        if (Lt(local.NullDateWorkArea.Date,
          entities.CsePersonAddress.VerifiedDate))
        {
          export.Group.Update.G.AddressConfirmedInd = "Y";
          export.Group.Update.G.AddressVerifiedDate =
            entities.CsePersonAddress.VerifiedDate;
        }
        else
        {
          export.Group.Update.G.AddressConfirmedInd = "N";
          export.Group.Update.G.AddressVerifiedDate =
            local.NullDateWorkArea.Date;
        }
      }

      foreach(var item1 in ReadIncomeSourceEmployer())
      {
        switch(AsChar(entities.IncomeSource.Type1))
        {
          case 'E':
            switch(AsChar(entities.IncomeSource.ReturnCd))
            {
              case 'E':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              case 'N':
                // -------------------------------------
                // Never worked for this employer
                // -------------------------------------
                continue;
              default:
                break;
            }

            break;
          case 'M':
            switch(AsChar(entities.IncomeSource.ReturnCd))
            {
              case 'A':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              case 'R':
                if (Lt(local.NullDateWorkArea.Date,
                  entities.IncomeSource.ReturnDt))
                {
                  export.Group.Update.G.EmployerConfirmedInd = "Y";
                  export.Group.Update.G.EmployerVerifiedDate =
                    entities.IncomeSource.ReturnDt;
                }
                else
                {
                  export.Group.Update.G.EmployerConfirmedInd = "N";
                  export.Group.Update.G.EmployerVerifiedDate =
                    local.NullDateWorkArea.Date;
                }

                break;
              default:
                // -------------------------------------
                // No record of military service
                // -------------------------------------
                continue;
            }

            break;
          default:
            continue;
        }

        export.Group.Update.G.EmployerName = entities.Employer.Name;

        if (IsEmpty(entities.Employer.Ein))
        {
          export.Group.Update.G.EmployerEin = 0;
        }
        else if (Verify(entities.Employer.Ein, "0123456789") > 0)
        {
          export.Group.Update.G.EmployerEin = 0;
        }
        else
        {
          export.Group.Update.G.EmployerEin =
            (int?)StringToNumber(entities.Employer.Ein);
        }

        if (Lt(0, entities.Employer.AreaCode) && !
          IsEmpty(entities.Employer.PhoneNo))
        {
          export.Group.Update.G.WorkAreaCode =
            NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13, 3);
            
          export.Group.Update.G.WorkPhone = entities.Employer.PhoneNo;
        }

        if (ReadEmployerAddress())
        {
          export.Group.Update.G.EmployerAddressLine1 =
            entities.EmployerAddress.Street1;
          export.Group.Update.G.EmployerAddressLine2 =
            entities.EmployerAddress.Street2;
          export.Group.Update.G.EmployerCity = entities.EmployerAddress.City;
          export.Group.Update.G.EmployerState = entities.EmployerAddress.State;
          export.Group.Update.G.EmployerZipCode5 =
            entities.EmployerAddress.ZipCode;
          export.Group.Update.G.EmployerZipCode4 =
            entities.EmployerAddress.Zip4;
        }

        break;
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
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

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
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
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.Ar.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 14);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 17);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 20);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 22);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 14);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 17);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 20);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 22);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 14);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 17);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 20);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 22);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 14);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 17);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 20);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 22);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
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
      });
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "numb", import.Children.Item.GimportChild.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 14);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 17);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 19);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 20);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 22);
        entities.CaseRole.CasNumber = db.GetString(reader, 23);
        entities.CaseRole.Type1 = db.GetString(reader, 24);
        entities.CaseRole.Identifier = db.GetInt32(reader, 25);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 26);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 27);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 28);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
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
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 5);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 6);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 8);
        entities.Employer.Identifier = db.GetInt32(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.Employer.Ein = db.GetNullableString(reader, 11);
        entities.Employer.Name = db.GetNullableString(reader, 12);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 13);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 14);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;

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
    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GimportChild.
      /// </summary>
      [JsonPropertyName("gimportChild")]
      public CsePerson GimportChild
      {
        get => gimportChild ??= new();
        set => gimportChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePerson gimportChild;
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
    /// A value of PrimaryAp.
    /// </summary>
    [JsonPropertyName("primaryAp")]
    public CsePersonsWorkSet PrimaryAp
    {
      get => primaryAp ??= new();
      set => primaryAp = value;
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
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
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

    private Case1 case1;
    private CsePersonsWorkSet primaryAp;
    private Common batch;
    private DateWorkArea current;
    private Array<ChildrenGroup> children;
    private InterstateCase interstateCase;
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
      /// A value of GexportParticipant.
      /// </summary>
      [JsonPropertyName("gexportParticipant")]
      public CsePerson GexportParticipant
      {
        get => gexportParticipant ??= new();
        set => gexportParticipant = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson gexportParticipant;
      private InterstateParticipant g;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of ChFound.
    /// </summary>
    [JsonPropertyName("chFound")]
    public Common ChFound
    {
      get => chFound ??= new();
      set => chFound = value;
    }

    /// <summary>
    /// A value of ArFound.
    /// </summary>
    [JsonPropertyName("arFound")]
    public Common ArFound
    {
      get => arFound ??= new();
      set => arFound = value;
    }

    private Array<GroupGroup> group;
    private InterstateCase interstateCase;
    private Common chFound;
    private Common arFound;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    /// <summary>
    /// A value of NullAbendData.
    /// </summary>
    [JsonPropertyName("nullAbendData")]
    public AbendData NullAbendData
    {
      get => nullAbendData ??= new();
      set => nullAbendData = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson ar;
    private AbendData nullAbendData;
    private AbendData abendData;
    private DateWorkArea nullDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePersonAddress csePersonAddress;
    private IncomeSource incomeSource;
    private Employer employer;
    private EmployerAddress employerAddress;
  }
#endregion
}
