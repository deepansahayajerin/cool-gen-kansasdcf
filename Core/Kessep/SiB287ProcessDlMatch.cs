// Program: SI_B287_PROCESS_DL_MATCH, ID: 1625320903, model: 746.
// Short name: SWE02214
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B287_PROCESS_DL_MATCH.
/// </summary>
[Serializable]
public partial class SiB287ProcessDlMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B287_PROCESS_DL_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB287ProcessDlMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB287ProcessDlMatch.
  /// </summary>
  public SiB287ProcessDlMatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 11/27/18  GVandy	CQ61419		Initial Development.
    // --------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Store the data from KDOR in the KDOR_Drivers_License table.
    // -------------------------------------------------------------------------------------
    local.KdorDriversLicense.DateOfBirth =
      StringToDate(import.KdorDlMatchRecord.DateOfBirth);
    local.KdorDriversLicense.FirstName = import.KdorDlMatchRecord.FirstName;
    local.KdorDriversLicense.LastName = import.KdorDlMatchRecord.LastName;
    local.KdorDriversLicense.LicenseNumber =
      import.KdorDlMatchRecord.DriversLicenseNumber;
    local.KdorDriversLicense.Ssn = import.KdorDlMatchRecord.Ssn;
    local.KdorDriversLicense.DlClassInd = import.KdorDlMatchRecord.DlClass;
    local.KdorDriversLicense.MotorcycleClassInd =
      import.KdorDlMatchRecord.MotocycleClass;
    local.KdorDriversLicense.CdlClassInd = import.KdorDlMatchRecord.CdlClass;
    local.KdorDriversLicense.ExpirationDt =
      StringToDate(import.KdorDlMatchRecord.ExpirationDate);
    local.KdorDriversLicense.GenderCode = import.KdorDlMatchRecord.Gender;
    local.KdorDriversLicense.AddressLine1 =
      import.KdorDlMatchRecord.AddressLine1;
    local.KdorDriversLicense.AddressLine2 =
      import.KdorDlMatchRecord.AddressLine2;
    local.KdorDriversLicense.City = import.KdorDlMatchRecord.City;
    local.KdorDriversLicense.State = import.KdorDlMatchRecord.State;
    local.KdorDriversLicense.ZipCode = import.KdorDlMatchRecord.ZipCode;
    local.KdorDriversLicense.HeightFeet = import.KdorDlMatchRecord.HeightFt;
    local.KdorDriversLicense.HeightInches = import.KdorDlMatchRecord.HeightIn;
    local.KdorDriversLicense.Weight = import.KdorDlMatchRecord.Weight;
    local.KdorDriversLicense.EyeColor = import.KdorDlMatchRecord.EyeColor;
    local.KdorDriversLicense.Type1 = "M";

    if (ReadKdorDriversLicense())
    {
      local.BeforeUpdate.Assign(entities.KdorDriversLicense);

      // --Note that the attributes in the "kdl_error" subtype are set to 
      // initialized values.
      // --This is done because the last time we received info from KDOR might 
      // have been a
      // --error.  We need to overwrite that data and store the info we received
      // this
      // --week in the "kdl_match" subtype attributes.
      try
      {
        UpdateKdorDriversLicense();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "KDOR_DRIVERS_LICENSE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "KDOR_DRIVERS_LICENSE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      try
      {
        CreateKdorDriversLicense();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "KDOR_DRIVERS_LICENSE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "KDOR_DRIVERS_LICENSE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --Determine if alerts or HIST records should be raised.
    // --
    // --Note that an alert is only generated the first time that new/changed 
    // data from KDOR
    // --is received.  If the worker decides the data in KAECSES is more 
    // accurate and they
    // --make no updates in KAECSES then they are not given a duplicate alert 
    // next week.
    // -------------------------------------------------------------------------------------
    local.RaiseAlert.Flag = "N";
    local.CreateHistRecord.Flag = "N";
    local.CsePersonsWorkSet.Number = import.KdorDlMatchRecord.PersonNumber;
    UseSiReadCsePerson();

    for(local.Common.Count = 1; local.Common.Count <= 11; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // --If SSN does not match then raise alert.
          if (!Equal(local.KdorDriversLicense.Ssn, local.CsePersonsWorkSet.Ssn) &&
            !Equal(local.KdorDriversLicense.Ssn, local.BeforeUpdate.Ssn))
          {
            local.RaiseAlert.Flag = "Y";
          }

          break;
        case 2:
          // --If Date of Birth does not match then raise alert.
          if (!Equal(local.KdorDriversLicense.DateOfBirth,
            local.CsePersonsWorkSet.Dob) && !
            Equal(local.KdorDriversLicense.DateOfBirth,
            local.BeforeUpdate.DateOfBirth))
          {
            local.RaiseAlert.Flag = "Y";
          }

          break;
        case 3:
          // --Check License Number.
          if (ReadCsePersonLicense())
          {
            if (!IsEmpty(entities.CsePersonLicense.IssuingState) || !
              IsEmpty(entities.CsePersonLicense.Number))
            {
              // --If License number does not match then raise alert.
              if ((!Equal(entities.CsePersonLicense.IssuingState, "KS") || !
                Equal(TrimEnd(entities.CsePersonLicense.Number),
                local.KdorDriversLicense.LicenseNumber)) && !
                Equal(local.KdorDriversLicense.LicenseNumber,
                local.BeforeUpdate.LicenseNumber))
              {
                local.RaiseAlert.Flag = "Y";
              }

              // --If Expiration Date does not match then create a HIST record.
              if (!Equal(entities.CsePersonLicense.ExpirationDt,
                local.KdorDriversLicense.ExpirationDt) && !
                Equal(local.KdorDriversLicense.ExpirationDt,
                local.BeforeUpdate.ExpirationDt))
              {
                local.CreateHistRecord.Flag = "Y";
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + "/ExpirationDate";
              }
            }
            else
            {
              // --If KAECSES doesn't have a license number then add the KDOR 
              // license number to APDS and create a HIST record.
              try
              {
                UpdateCsePersonLicense();
                local.CreateHistRecord.Flag = "Y";
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + "/DL#";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CSE_PERSON_LICENSE_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CSE_PERSON_LICENSE_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
          }
          else
          {
            // --If KAECSES doesn't have a license number then add the KDOR 
            // license number to APDS and create a HIST record.
            try
            {
              CreateCsePersonLicense();
              local.CreateHistRecord.Flag = "Y";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + "/DL#";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_LICENSE_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_LICENSE_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          break;
        case 4:
          // --If DLClass does not match then create a HIST record.
          if (AsChar(local.KdorDriversLicense.DlClassInd) != AsChar
            (local.BeforeUpdate.DlClassInd))
          {
            local.CreateHistRecord.Flag = "Y";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + "/DLClass";
          }

          break;
        case 5:
          // --If MotorcycleClass does not match then create a HIST record.
          if (AsChar(local.KdorDriversLicense.MotorcycleClassInd) != AsChar
            (local.BeforeUpdate.MotorcycleClassInd))
          {
            local.CreateHistRecord.Flag = "Y";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + "/MotorcycleClass";
          }

          break;
        case 6:
          // --If CDLClass does not match then create a HIST record.
          if (AsChar(local.KdorDriversLicense.CdlClassInd) != AsChar
            (local.BeforeUpdate.CdlClassInd))
          {
            local.CreateHistRecord.Flag = "Y";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + "/CDLClass";
          }

          break;
        case 7:
          // --If Gender does not match then raise alert.
          if (AsChar(local.KdorDriversLicense.GenderCode) != AsChar
            (local.CsePersonsWorkSet.Sex) && AsChar
            (local.KdorDriversLicense.GenderCode) != AsChar
            (local.BeforeUpdate.GenderCode))
          {
            local.RaiseAlert.Flag = "Y";
          }

          break;
        case 8:
          // --If Address does not match then raise alert.
          if (!Equal(local.KdorDriversLicense.AddressLine1,
            local.BeforeUpdate.AddressLine1) || !
            Equal(local.KdorDriversLicense.AddressLine2,
            local.BeforeUpdate.AddressLine2) || !
            Equal(local.KdorDriversLicense.City, local.BeforeUpdate.City) || !
            Equal(local.KdorDriversLicense.State, local.BeforeUpdate.State) || !
            Equal(local.KdorDriversLicense.ZipCode, local.BeforeUpdate.ZipCode))
          {
            local.Kdor.Street1 = local.KdorDriversLicense.AddressLine1 ?? "";
            local.Kdor.Street2 = local.KdorDriversLicense.AddressLine2 ?? "";
            local.Kdor.City = local.KdorDriversLicense.City ?? "";
            local.Kdor.State = local.KdorDriversLicense.State ?? "";
            local.Kdor.ZipCode =
              Substring(local.KdorDriversLicense.ZipCode, 1, 5);
            local.Kdor.Zip4 = Substring(local.KdorDriversLicense.ZipCode, 6, 4);

            // --If KAECSES doesn't have the KDOR address then raise alert.
            ReadCsePersonAddress();

            if (!entities.CsePersonAddress.Populated)
            {
              local.RaiseAlert.Flag = "Y";
            }
          }

          break;
        case 9:
          // --Check Height.
          local.ForCompare.HeightFt =
            (int?)StringToNumber(local.KdorDriversLicense.HeightFeet);
          local.ForCompare.HeightIn =
            (int?)StringToNumber(local.KdorDriversLicense.HeightInches);

          if (entities.CsePerson.HeightFt.GetValueOrDefault() + entities
            .CsePerson.HeightIn.GetValueOrDefault() == 0)
          {
            if (local.ForCompare.HeightFt.GetValueOrDefault() + local
              .ForCompare.HeightIn.GetValueOrDefault() == 0)
            {
              break;
            }

            // --If KAECSES doesn't have a height then add the KDOR height to 
            // APDS.
            try
            {
              UpdateCsePerson2();
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

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // --If height does not match then raise alert.
            if (entities.CsePerson.HeightFt.GetValueOrDefault() - local
              .ForCompare.HeightFt.GetValueOrDefault() != 0 && AsChar
              (local.KdorDriversLicense.HeightFeet) != AsChar
              (local.BeforeUpdate.HeightFeet) || entities
              .CsePerson.HeightIn.GetValueOrDefault() - local
              .ForCompare.HeightIn.GetValueOrDefault() != 0 && !
              Equal(local.KdorDriversLicense.HeightInches,
              local.BeforeUpdate.HeightInches))
            {
              local.RaiseAlert.Flag = "Y";
            }
          }

          break;
        case 10:
          // --Check Weight.
          local.ForCompare.Weight =
            (int?)StringToNumber(local.KdorDriversLicense.Weight);

          if (Equal(entities.CsePerson.Weight, 0))
          {
            if (local.ForCompare.Weight.GetValueOrDefault() == 0)
            {
              break;
            }

            // --If KAECSES doesn't have a Weight then add the KDOR Weight to 
            // APDS.
            try
            {
              UpdateCsePerson3();
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

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // --If Weight does not match then raise alert.
            if (!Equal(entities.CsePerson.Weight,
              local.ForCompare.Weight.GetValueOrDefault()) && !
              Equal(local.KdorDriversLicense.Weight, local.BeforeUpdate.Weight))
            {
              local.RaiseAlert.Flag = "Y";
            }
          }

          break;
        case 11:
          // --Check Eye Color.
          switch(TrimEnd(local.KdorDriversLicense.EyeColor ?? ""))
          {
            case "":
              break;
            case "BLK":
              local.ForCompare.EyeColor = "BK";

              break;
            case "BLU":
              local.ForCompare.EyeColor = "BU";

              break;
            case "BRO":
              local.ForCompare.EyeColor = "BN";

              break;
            case "GRY":
              local.ForCompare.EyeColor = "GY";

              break;
            case "GRN":
              local.ForCompare.EyeColor = "GN";

              break;
            case "HAZ":
              local.ForCompare.EyeColor = "HZ";

              break;
            case "MAR":
              local.ForCompare.EyeColor = "OT";

              break;
            case "PNK":
              local.ForCompare.EyeColor = "OT";

              break;
            case "DIC":
              local.ForCompare.EyeColor = "DC";

              break;
            case "UNK":
              local.ForCompare.EyeColor = "UN";

              break;
            default:
              local.ForCompare.EyeColor = "OT";

              break;
          }

          if (IsEmpty(entities.CsePerson.EyeColor))
          {
            if (IsEmpty(local.ForCompare.EyeColor))
            {
              break;
            }

            // --If KAECSES doesn't have Eye Color then add the KDOR Eye Color 
            // to APDS.
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

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // --If Eye Color does not match then raise alert.
            if (!Equal(entities.CsePerson.EyeColor, local.ForCompare.EyeColor) &&
              !
              Equal(local.KdorDriversLicense.EyeColor,
              local.BeforeUpdate.EyeColor))
            {
              local.RaiseAlert.Flag = "Y";
            }
          }

          break;
        default:
          break;
      }
    }

    if (AsChar(local.CreateHistRecord.Flag) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      // --Raise an event to create an HIST record.
      // -------------------------------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 24;
      local.Infrastructure.ReasonCode = "KDORDLUPDATE";
      local.Infrastructure.EventDetailName =
        "KDOR_DRIVERS_LICENSE_INFO_UPDATED";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.EventType = "CASE";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.Function = "LOC";
      local.Infrastructure.UserId = "KDOR";
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();
      local.Infrastructure.Detail = "New/Updated Data: " + TrimEnd
        (local.Infrastructure.Detail);

      // --Raise event for each case on which the person is an active AP.
      foreach(var item in ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    if (AsChar(local.RaiseAlert.Flag) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      // --Raise an event to create an alert to the caseworker.
      // -------------------------------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 24;
      local.Infrastructure.ReasonCode = "KDORDLMATCH";
      local.Infrastructure.EventDetailName = "KDOR_DRIVERS_LICENSE_MATCH";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.EventType = "CASE";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.Function = "LOC";
      local.Infrastructure.UserId = "KDOR";
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();
      local.Infrastructure.Detail =
        "KDOR response received. Review/verify highlighted KDL info on KDOR screen.";
        

      // --Raise event for each case on which the person is an active AP.
      foreach(var item in ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void CreateCsePersonLicense()
  {
    var identifier = 1;
    var cspNumber = entities.CsePerson.Number;
    var issuingState = "KS";
    var number = local.KdorDriversLicense.LicenseNumber ?? "";
    var expirationDt = local.KdorDriversLicense.ExpirationDt;
    var startDt = Now().Date;
    var type1 = "D";
    var createdTimestamp = Now();
    var createdBy = global.UserId;

    entities.CsePersonLicense.Populated = false;
    Update("CreateCsePersonLicense",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "issuingState", issuingState);
        db.SetNullableString(command, "issuingAgencyNm", "");
        db.SetNullableString(command, "numb", number);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableString(command, "type", type1);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", "");
        db.SetNullableString(command, "note", "");
      });

    entities.CsePersonLicense.Identifier = identifier;
    entities.CsePersonLicense.CspNumber = cspNumber;
    entities.CsePersonLicense.IssuingState = issuingState;
    entities.CsePersonLicense.IssuingAgencyName = "";
    entities.CsePersonLicense.Number = number;
    entities.CsePersonLicense.ExpirationDt = expirationDt;
    entities.CsePersonLicense.StartDt = startDt;
    entities.CsePersonLicense.Type1 = type1;
    entities.CsePersonLicense.LastUpdatedTimestamp = null;
    entities.CsePersonLicense.LastUpdatedBy = "";
    entities.CsePersonLicense.CreatedTimestamp = createdTimestamp;
    entities.CsePersonLicense.CreatedBy = createdBy;
    entities.CsePersonLicense.Description = "";
    entities.CsePersonLicense.Note = "";
    entities.CsePersonLicense.Populated = true;
  }

  private void CreateKdorDriversLicense()
  {
    var type1 = local.KdorDriversLicense.Type1;
    var lastName = local.KdorDriversLicense.LastName ?? "";
    var firstName = local.KdorDriversLicense.FirstName ?? "";
    var ssn = local.KdorDriversLicense.Ssn ?? "";
    var dateOfBirth = local.KdorDriversLicense.DateOfBirth;
    var licenseNumber = local.KdorDriversLicense.LicenseNumber ?? "";
    var createdTstamp = Now();
    var createdBy = global.UserId;
    var dlClassInd = local.KdorDriversLicense.DlClassInd ?? "";
    var motorcycleClassInd = local.KdorDriversLicense.MotorcycleClassInd ?? "";
    var cdlClassInd = local.KdorDriversLicense.CdlClassInd ?? "";
    var expirationDt = local.KdorDriversLicense.ExpirationDt;
    var genderCode = local.KdorDriversLicense.GenderCode ?? "";
    var addressLine1 = local.KdorDriversLicense.AddressLine1 ?? "";
    var addressLine2 = local.KdorDriversLicense.AddressLine2 ?? "";
    var city = local.KdorDriversLicense.City ?? "";
    var state = local.KdorDriversLicense.State ?? "";
    var zipCode = local.KdorDriversLicense.ZipCode ?? "";
    var heightFeet = local.KdorDriversLicense.HeightFeet ?? "";
    var heightInches = local.KdorDriversLicense.HeightInches ?? "";
    var weight = local.KdorDriversLicense.Weight ?? "";
    var eyeColor = local.KdorDriversLicense.EyeColor ?? "";
    var fkCktCsePersnumb = entities.CsePerson.Number;

    CheckValid<KdorDriversLicense>("Type1", type1);
    entities.KdorDriversLicense.Populated = false;
    Update("CreateKdorDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstamp", null);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "errorReason", "");
        db.SetNullableString(command, "dlClassInd", dlClassInd);
        db.SetNullableString(command, "motorcycleClassInd", motorcycleClassInd);
        db.SetNullableString(command, "cdlClassInd", cdlClassInd);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableString(command, "genderCode", genderCode);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "heightFeet", heightFeet);
        db.SetNullableString(command, "heightInches", heightInches);
        db.SetNullableString(command, "weight", weight);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetString(command, "fkCktCsePersnumb", fkCktCsePersnumb);
      });

    entities.KdorDriversLicense.Type1 = type1;
    entities.KdorDriversLicense.LastName = lastName;
    entities.KdorDriversLicense.FirstName = firstName;
    entities.KdorDriversLicense.Ssn = ssn;
    entities.KdorDriversLicense.DateOfBirth = dateOfBirth;
    entities.KdorDriversLicense.LicenseNumber = licenseNumber;
    entities.KdorDriversLicense.CreatedTstamp = createdTstamp;
    entities.KdorDriversLicense.CreatedBy = createdBy;
    entities.KdorDriversLicense.LastUpdatedBy = "";
    entities.KdorDriversLicense.LastUpdatedTstamp = null;
    entities.KdorDriversLicense.Status = "";
    entities.KdorDriversLicense.ErrorReason = "";
    entities.KdorDriversLicense.DlClassInd = dlClassInd;
    entities.KdorDriversLicense.MotorcycleClassInd = motorcycleClassInd;
    entities.KdorDriversLicense.CdlClassInd = cdlClassInd;
    entities.KdorDriversLicense.ExpirationDt = expirationDt;
    entities.KdorDriversLicense.GenderCode = genderCode;
    entities.KdorDriversLicense.AddressLine1 = addressLine1;
    entities.KdorDriversLicense.AddressLine2 = addressLine2;
    entities.KdorDriversLicense.City = city;
    entities.KdorDriversLicense.State = state;
    entities.KdorDriversLicense.ZipCode = zipCode;
    entities.KdorDriversLicense.HeightFeet = heightFeet;
    entities.KdorDriversLicense.HeightInches = heightInches;
    entities.KdorDriversLicense.Weight = weight;
    entities.KdorDriversLicense.EyeColor = eyeColor;
    entities.KdorDriversLicense.FkCktCsePersnumb = fkCktCsePersnumb;
    entities.KdorDriversLicense.Populated = true;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.KdorDlMatchRecord.PersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 2);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 3);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 4);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 5);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
        db.SetNullableString(command, "street1", local.Kdor.Street1 ?? "");
        db.SetNullableString(command, "street2", local.Kdor.Street2 ?? "");
        db.SetNullableString(command, "city", local.Kdor.City ?? "");
        db.SetNullableString(command, "state", local.Kdor.State ?? "");
        db.SetNullableString(command, "zipCode", local.Kdor.ZipCode ?? "");
        db.SetNullableString(command, "zip4", local.Kdor.Zip4 ?? "");
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 9);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
        entities.CsePersonLicense.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonLicense.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.CsePersonLicense.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.CsePersonLicense.CreatedBy = db.GetString(reader, 11);
        entities.CsePersonLicense.Description =
          db.GetNullableString(reader, 12);
        entities.CsePersonLicense.Note = db.GetNullableString(reader, 13);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadKdorDriversLicense()
  {
    entities.KdorDriversLicense.Populated = false;

    return Read("ReadKdorDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KdorDriversLicense.Type1 = db.GetString(reader, 0);
        entities.KdorDriversLicense.LastName = db.GetNullableString(reader, 1);
        entities.KdorDriversLicense.FirstName = db.GetNullableString(reader, 2);
        entities.KdorDriversLicense.Ssn = db.GetNullableString(reader, 3);
        entities.KdorDriversLicense.DateOfBirth = db.GetNullableDate(reader, 4);
        entities.KdorDriversLicense.LicenseNumber =
          db.GetNullableString(reader, 5);
        entities.KdorDriversLicense.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.KdorDriversLicense.CreatedBy = db.GetString(reader, 7);
        entities.KdorDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.KdorDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.KdorDriversLicense.Status = db.GetNullableString(reader, 10);
        entities.KdorDriversLicense.ErrorReason =
          db.GetNullableString(reader, 11);
        entities.KdorDriversLicense.DlClassInd =
          db.GetNullableString(reader, 12);
        entities.KdorDriversLicense.MotorcycleClassInd =
          db.GetNullableString(reader, 13);
        entities.KdorDriversLicense.CdlClassInd =
          db.GetNullableString(reader, 14);
        entities.KdorDriversLicense.ExpirationDt =
          db.GetNullableDate(reader, 15);
        entities.KdorDriversLicense.GenderCode =
          db.GetNullableString(reader, 16);
        entities.KdorDriversLicense.AddressLine1 =
          db.GetNullableString(reader, 17);
        entities.KdorDriversLicense.AddressLine2 =
          db.GetNullableString(reader, 18);
        entities.KdorDriversLicense.City = db.GetNullableString(reader, 19);
        entities.KdorDriversLicense.State = db.GetNullableString(reader, 20);
        entities.KdorDriversLicense.ZipCode = db.GetNullableString(reader, 21);
        entities.KdorDriversLicense.HeightFeet =
          db.GetNullableString(reader, 22);
        entities.KdorDriversLicense.HeightInches =
          db.GetNullableString(reader, 23);
        entities.KdorDriversLicense.Weight = db.GetNullableString(reader, 24);
        entities.KdorDriversLicense.EyeColor = db.GetNullableString(reader, 25);
        entities.KdorDriversLicense.FkCktCsePersnumb = db.GetString(reader, 26);
        entities.KdorDriversLicense.Populated = true;
        CheckValid<KdorDriversLicense>("Type1",
          entities.KdorDriversLicense.Type1);
      });
  }

  private void UpdateCsePerson1()
  {
    var eyeColor = local.ForCompare.EyeColor ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.EyeColor = eyeColor;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var heightFt = local.ForCompare.HeightFt.GetValueOrDefault();
    var heightIn = local.ForCompare.HeightIn.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.HeightFt = heightFt;
    entities.CsePerson.HeightIn = heightIn;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var weight = local.ForCompare.Weight.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "weight", weight);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.Weight = weight;
    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePersonLicense()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonLicense.Populated);

    var issuingState = "KS";
    var number = local.KdorDriversLicense.LicenseNumber ?? "";
    var expirationDt = local.KdorDriversLicense.ExpirationDt;
    var startDt = Now().Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CsePersonLicense.Populated = false;
    Update("UpdateCsePersonLicense",
      (db, command) =>
      {
        db.SetNullableString(command, "issuingState", issuingState);
        db.SetNullableString(command, "numb", number);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableDate(command, "startDt", startDt);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetInt32(command, "identifier", entities.CsePersonLicense.Identifier);
          
        db.SetString(command, "cspNumber", entities.CsePersonLicense.CspNumber);
      });

    entities.CsePersonLicense.IssuingState = issuingState;
    entities.CsePersonLicense.Number = number;
    entities.CsePersonLicense.ExpirationDt = expirationDt;
    entities.CsePersonLicense.StartDt = startDt;
    entities.CsePersonLicense.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonLicense.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonLicense.Populated = true;
  }

  private void UpdateKdorDriversLicense()
  {
    System.Diagnostics.Debug.Assert(entities.KdorDriversLicense.Populated);

    var type1 = local.KdorDriversLicense.Type1;
    var lastName = local.KdorDriversLicense.LastName ?? "";
    var firstName = local.KdorDriversLicense.FirstName ?? "";
    var ssn = local.KdorDriversLicense.Ssn ?? "";
    var dateOfBirth = local.KdorDriversLicense.DateOfBirth;
    var licenseNumber = local.KdorDriversLicense.LicenseNumber ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var status = local.KdorDriversLicense.Status ?? "";
    var errorReason = local.KdorDriversLicense.ErrorReason ?? "";
    var dlClassInd = local.KdorDriversLicense.DlClassInd ?? "";
    var motorcycleClassInd = local.KdorDriversLicense.MotorcycleClassInd ?? "";
    var cdlClassInd = local.KdorDriversLicense.CdlClassInd ?? "";
    var expirationDt = local.KdorDriversLicense.ExpirationDt;
    var genderCode = local.KdorDriversLicense.GenderCode ?? "";
    var addressLine1 = local.KdorDriversLicense.AddressLine1 ?? "";
    var addressLine2 = local.KdorDriversLicense.AddressLine2 ?? "";
    var city = local.KdorDriversLicense.City ?? "";
    var state = local.KdorDriversLicense.State ?? "";
    var zipCode = local.KdorDriversLicense.ZipCode ?? "";
    var heightFeet = local.KdorDriversLicense.HeightFeet ?? "";
    var heightInches = local.KdorDriversLicense.HeightInches ?? "";
    var weight = local.KdorDriversLicense.Weight ?? "";
    var eyeColor = local.KdorDriversLicense.EyeColor ?? "";

    CheckValid<KdorDriversLicense>("Type1", type1);
    entities.KdorDriversLicense.Populated = false;
    Update("UpdateKdorDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "errorReason", errorReason);
        db.SetNullableString(command, "dlClassInd", dlClassInd);
        db.SetNullableString(command, "motorcycleClassInd", motorcycleClassInd);
        db.SetNullableString(command, "cdlClassInd", cdlClassInd);
        db.SetNullableDate(command, "expirationDt", expirationDt);
        db.SetNullableString(command, "genderCode", genderCode);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "heightFeet", heightFeet);
        db.SetNullableString(command, "heightInches", heightInches);
        db.SetNullableString(command, "weight", weight);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetString(
          command, "fkCktCsePersnumb",
          entities.KdorDriversLicense.FkCktCsePersnumb);
      });

    entities.KdorDriversLicense.Type1 = type1;
    entities.KdorDriversLicense.LastName = lastName;
    entities.KdorDriversLicense.FirstName = firstName;
    entities.KdorDriversLicense.Ssn = ssn;
    entities.KdorDriversLicense.DateOfBirth = dateOfBirth;
    entities.KdorDriversLicense.LicenseNumber = licenseNumber;
    entities.KdorDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KdorDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KdorDriversLicense.Status = status;
    entities.KdorDriversLicense.ErrorReason = errorReason;
    entities.KdorDriversLicense.DlClassInd = dlClassInd;
    entities.KdorDriversLicense.MotorcycleClassInd = motorcycleClassInd;
    entities.KdorDriversLicense.CdlClassInd = cdlClassInd;
    entities.KdorDriversLicense.ExpirationDt = expirationDt;
    entities.KdorDriversLicense.GenderCode = genderCode;
    entities.KdorDriversLicense.AddressLine1 = addressLine1;
    entities.KdorDriversLicense.AddressLine2 = addressLine2;
    entities.KdorDriversLicense.City = city;
    entities.KdorDriversLicense.State = state;
    entities.KdorDriversLicense.ZipCode = zipCode;
    entities.KdorDriversLicense.HeightFeet = heightFeet;
    entities.KdorDriversLicense.HeightInches = heightInches;
    entities.KdorDriversLicense.Weight = weight;
    entities.KdorDriversLicense.EyeColor = eyeColor;
    entities.KdorDriversLicense.Populated = true;
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
    /// A value of KdorDlMatchRecord.
    /// </summary>
    [JsonPropertyName("kdorDlMatchRecord")]
    public KdorDlMatchRecord KdorDlMatchRecord
    {
      get => kdorDlMatchRecord ??= new();
      set => kdorDlMatchRecord = value;
    }

    private KdorDlMatchRecord kdorDlMatchRecord;
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
    /// A value of Kdor.
    /// </summary>
    [JsonPropertyName("kdor")]
    public CsePersonAddress Kdor
    {
      get => kdor ??= new();
      set => kdor = value;
    }

    /// <summary>
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public CsePerson ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

    /// <summary>
    /// A value of BeforeUpdate.
    /// </summary>
    [JsonPropertyName("beforeUpdate")]
    public KdorDriversLicense BeforeUpdate
    {
      get => beforeUpdate ??= new();
      set => beforeUpdate = value;
    }

    /// <summary>
    /// A value of CreateHistRecord.
    /// </summary>
    [JsonPropertyName("createHistRecord")]
    public Common CreateHistRecord
    {
      get => createHistRecord ??= new();
      set => createHistRecord = value;
    }

    /// <summary>
    /// A value of RaiseAlert.
    /// </summary>
    [JsonPropertyName("raiseAlert")]
    public Common RaiseAlert
    {
      get => raiseAlert ??= new();
      set => raiseAlert = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of KdorDriversLicense.
    /// </summary>
    [JsonPropertyName("kdorDriversLicense")]
    public KdorDriversLicense KdorDriversLicense
    {
      get => kdorDriversLicense ??= new();
      set => kdorDriversLicense = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePersonAddress kdor;
    private CsePerson forCompare;
    private KdorDriversLicense beforeUpdate;
    private Common createHistRecord;
    private Common raiseAlert;
    private Common common;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private TextWorkArea date;
    private KdorDriversLicense kdorDriversLicense;
    private Infrastructure infrastructure;
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
    /// A value of KdorDriversLicense.
    /// </summary>
    [JsonPropertyName("kdorDriversLicense")]
    public KdorDriversLicense KdorDriversLicense
    {
      get => kdorDriversLicense ??= new();
      set => kdorDriversLicense = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private KdorDriversLicense kdorDriversLicense;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
    private CsePersonLicense csePersonLicense;
  }
#endregion
}
