// Program: SI_B288_PROCESS_VEHICLE, ID: 1625323195, model: 746.
// Short name: SWE02215
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B288_PROCESS_VEHICLE.
/// </summary>
[Serializable]
public partial class SiB288ProcessVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B288_PROCESS_VEHICLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB288ProcessVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB288ProcessVehicle.
  /// </summary>
  public SiB288ProcessVehicle(IContext context, Import import, Export export):
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

    local.Changes.Flag = "N";

    // -------------------------------------------------------------------------------------
    // -- Store the data from KDOR in the KDOR_Vehicle table.
    // -------------------------------------------------------------------------------------
    local.KdorVehicle.DateOfBirth =
      StringToDate(import.KdorVehicleRecord.DateOfBirth);
    local.KdorVehicle.FirstName = import.KdorVehicleRecord.FirstName;
    local.KdorVehicle.LastName = import.KdorVehicleRecord.LastName;
    local.KdorVehicle.LicenseNumber =
      import.KdorVehicleRecord.DriversLicenseNumber;
    local.KdorVehicle.Make = import.KdorVehicleRecord.Make;
    local.KdorVehicle.Model = import.KdorVehicleRecord.Model;
    local.KdorVehicle.PlateNumber = import.KdorVehicleRecord.PlateNumber;
    local.KdorVehicle.Ssn = import.KdorVehicleRecord.Ssn;
    local.KdorVehicle.VinNumber = import.KdorVehicleRecord.Vin;
    local.KdorVehicle.Year = import.KdorVehicleRecord.Year;

    if (ReadKdorVehicle1())
    {
      if (!Equal(local.KdorVehicle.DateOfBirth, entities.KdorVehicle.DateOfBirth)
        || !
        Equal(local.KdorVehicle.FirstName, entities.KdorVehicle.FirstName) || !
        Equal(local.KdorVehicle.LastName, entities.KdorVehicle.LastName) || !
        Equal(local.KdorVehicle.LicenseNumber,
        entities.KdorVehicle.LicenseNumber) || !
        Equal(local.KdorVehicle.Make, entities.KdorVehicle.Make) || !
        Equal(local.KdorVehicle.Model, entities.KdorVehicle.Model) || !
        Equal(local.KdorVehicle.PlateNumber, entities.KdorVehicle.PlateNumber) ||
        !Equal(local.KdorVehicle.Ssn, entities.KdorVehicle.Ssn) || !
        Equal(local.KdorVehicle.VinNumber, entities.KdorVehicle.VinNumber) || !
        Equal(local.KdorVehicle.Year, entities.KdorVehicle.Year))
      {
        local.Changes.Flag = "Y";
      }

      try
      {
        UpdateKdorVehicle();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "KDOR_VEHICLE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "KDOR_VEHICLE_PV";

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
      local.Changes.Flag = "Y";
      ReadKdorVehicle2();

      try
      {
        CreateKdorVehicle();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "KDOR_VEHICLE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "KDOR_VEHICLE_PV";

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
    // -- Store the data from KDOR in the KDOR_Vehicle_Owner table.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // --Owner 1
          local.KdorVehicleOwner.AddressLine1 =
            import.KdorVehicleRecord.Owner1MailingAddressLine1;
          local.KdorVehicleOwner.AddressLine2 =
            import.KdorVehicleRecord.Owner1MailingAddresssLine2;
          local.KdorVehicleOwner.BusinessPhone =
            import.KdorVehicleRecord.Owner1BusinessNumber;
          local.KdorVehicleOwner.City =
            import.KdorVehicleRecord.Owner1MailingCity;
          local.KdorVehicleOwner.FirstName =
            import.KdorVehicleRecord.Owner1FirstName;
          local.KdorVehicleOwner.HomePhone =
            import.KdorVehicleRecord.Owner1HomeNumber;
          local.KdorVehicleOwner.LastName =
            import.KdorVehicleRecord.Owner1LastName;
          local.KdorVehicleOwner.MiddleName =
            import.KdorVehicleRecord.Owner1MiddleName;
          local.KdorVehicleOwner.OrganizationName =
            import.KdorVehicleRecord.Owner1OrganizationName;
          local.KdorVehicleOwner.State =
            import.KdorVehicleRecord.Owner1MailingState;
          local.KdorVehicleOwner.Suffix = import.KdorVehicleRecord.Owner1Suffix;
          local.KdorVehicleOwner.VestmentType =
            import.KdorVehicleRecord.Owner1VestmentType;
          local.KdorVehicleOwner.ZipCode =
            import.KdorVehicleRecord.Owner1MailingZipCode;

          break;
        case 2:
          // --Owner 2
          local.KdorVehicleOwner.AddressLine1 =
            import.KdorVehicleRecord.Owner2MailingAddressLine1;
          local.KdorVehicleOwner.AddressLine2 =
            import.KdorVehicleRecord.Owner2MailingAddresssLine2;
          local.KdorVehicleOwner.BusinessPhone =
            import.KdorVehicleRecord.Owner2BusinessNumber;
          local.KdorVehicleOwner.City =
            import.KdorVehicleRecord.Owner2MailingCity;
          local.KdorVehicleOwner.FirstName =
            import.KdorVehicleRecord.Owner2FirstName;
          local.KdorVehicleOwner.HomePhone =
            import.KdorVehicleRecord.Owner2HomeNumber;
          local.KdorVehicleOwner.LastName =
            import.KdorVehicleRecord.Owner2LastName;
          local.KdorVehicleOwner.MiddleName =
            import.KdorVehicleRecord.Owner2MiddleName;
          local.KdorVehicleOwner.OrganizationName =
            import.KdorVehicleRecord.Owner2OrganizationName;
          local.KdorVehicleOwner.State =
            import.KdorVehicleRecord.Owner2MailingState;
          local.KdorVehicleOwner.Suffix = import.KdorVehicleRecord.Owner2Suffix;
          local.KdorVehicleOwner.VestmentType =
            import.KdorVehicleRecord.Owner2VestmentType;
          local.KdorVehicleOwner.ZipCode =
            import.KdorVehicleRecord.Owner2MailingZipCode;

          break;
        case 3:
          // --Owner 3
          local.KdorVehicleOwner.AddressLine1 =
            import.KdorVehicleRecord.Owner3MailingAddressLine1;
          local.KdorVehicleOwner.AddressLine2 =
            import.KdorVehicleRecord.Owner3MailingAddresssLine2;
          local.KdorVehicleOwner.BusinessPhone =
            import.KdorVehicleRecord.Owner3BusinessNumber;
          local.KdorVehicleOwner.City =
            import.KdorVehicleRecord.Owner3MailingCity;
          local.KdorVehicleOwner.FirstName =
            import.KdorVehicleRecord.Owner3FirstName;
          local.KdorVehicleOwner.HomePhone =
            import.KdorVehicleRecord.Owner3HomeNumber;
          local.KdorVehicleOwner.LastName =
            import.KdorVehicleRecord.Owner3LastName;
          local.KdorVehicleOwner.MiddleName =
            import.KdorVehicleRecord.Owner3MiddleName;
          local.KdorVehicleOwner.OrganizationName =
            import.KdorVehicleRecord.Owner3OrganizationName;
          local.KdorVehicleOwner.State =
            import.KdorVehicleRecord.Owner3MailingState;
          local.KdorVehicleOwner.Suffix = import.KdorVehicleRecord.Owner3Suffix;
          local.KdorVehicleOwner.VestmentType =
            import.KdorVehicleRecord.Owner3VestmentType;
          local.KdorVehicleOwner.ZipCode =
            import.KdorVehicleRecord.Owner3MailingZipCode;

          break;
        case 4:
          // --Owner 4
          local.KdorVehicleOwner.AddressLine1 =
            import.KdorVehicleRecord.Owner4MailingAddressLine1;
          local.KdorVehicleOwner.AddressLine2 =
            import.KdorVehicleRecord.Owner4MailingAddresssLine2;
          local.KdorVehicleOwner.BusinessPhone =
            import.KdorVehicleRecord.Owner4BusinessNumber;
          local.KdorVehicleOwner.City =
            import.KdorVehicleRecord.Owner4MailingCity;
          local.KdorVehicleOwner.FirstName =
            import.KdorVehicleRecord.Owner4FirstName;
          local.KdorVehicleOwner.HomePhone =
            import.KdorVehicleRecord.Owner4HomeNumber;
          local.KdorVehicleOwner.LastName =
            import.KdorVehicleRecord.Owner4LastName;
          local.KdorVehicleOwner.MiddleName =
            import.KdorVehicleRecord.Owner4MiddleName;
          local.KdorVehicleOwner.OrganizationName =
            import.KdorVehicleRecord.Owner4OrganizationName;
          local.KdorVehicleOwner.State =
            import.KdorVehicleRecord.Owner4MailingState;
          local.KdorVehicleOwner.Suffix = import.KdorVehicleRecord.Owner4Suffix;
          local.KdorVehicleOwner.VestmentType =
            import.KdorVehicleRecord.Owner4VestmentType;
          local.KdorVehicleOwner.ZipCode =
            import.KdorVehicleRecord.Owner4MailingZipCode;

          break;
        case 5:
          // --Owner 5
          local.KdorVehicleOwner.AddressLine1 =
            import.KdorVehicleRecord.Owner5MailingAddressLine1;
          local.KdorVehicleOwner.AddressLine2 =
            import.KdorVehicleRecord.Owner5MailingAddresssLine2;
          local.KdorVehicleOwner.BusinessPhone =
            import.KdorVehicleRecord.Owner5BusinessNumber;
          local.KdorVehicleOwner.City =
            import.KdorVehicleRecord.Owner5MailingCity;
          local.KdorVehicleOwner.FirstName =
            import.KdorVehicleRecord.Owner5FirstName;
          local.KdorVehicleOwner.HomePhone =
            import.KdorVehicleRecord.Owner5HomeNumber;
          local.KdorVehicleOwner.LastName =
            import.KdorVehicleRecord.Owner5LastName;
          local.KdorVehicleOwner.MiddleName =
            import.KdorVehicleRecord.Owner5MiddleName;
          local.KdorVehicleOwner.OrganizationName =
            import.KdorVehicleRecord.Owner5OrganizationName;
          local.KdorVehicleOwner.State =
            import.KdorVehicleRecord.Owner5MailingState;
          local.KdorVehicleOwner.Suffix = import.KdorVehicleRecord.Owner5Suffix;
          local.KdorVehicleOwner.VestmentType =
            import.KdorVehicleRecord.Owner5VestmentType;
          local.KdorVehicleOwner.ZipCode =
            import.KdorVehicleRecord.Owner5MailingZipCode;

          break;
        default:
          break;
      }

      if (ReadKdorVehicleOwner())
      {
        if (IsEmpty(local.KdorVehicleOwner.AddressLine1) && IsEmpty
          (local.KdorVehicleOwner.AddressLine2) && IsEmpty
          (local.KdorVehicleOwner.BusinessPhone) && IsEmpty
          (local.KdorVehicleOwner.City) && IsEmpty
          (local.KdorVehicleOwner.FirstName) && IsEmpty
          (local.KdorVehicleOwner.HomePhone) && IsEmpty
          (local.KdorVehicleOwner.LastName) && IsEmpty
          (local.KdorVehicleOwner.MiddleName) && IsEmpty
          (local.KdorVehicleOwner.OrganizationName) && IsEmpty
          (local.KdorVehicleOwner.State) && IsEmpty
          (local.KdorVehicleOwner.Suffix) && IsEmpty
          (local.KdorVehicleOwner.VestmentType) && IsEmpty
          (local.KdorVehicleOwner.ZipCode))
        {
          local.Changes.Flag = "Y";
          DeleteKdorVehicleOwner();
        }
        else if (Equal(local.KdorVehicleOwner.AddressLine1,
          entities.KdorVehicleOwner.AddressLine1) && Equal
          (local.KdorVehicleOwner.AddressLine2,
          entities.KdorVehicleOwner.AddressLine2) && Equal
          (local.KdorVehicleOwner.BusinessPhone,
          entities.KdorVehicleOwner.BusinessPhone) && Equal
          (local.KdorVehicleOwner.City, entities.KdorVehicleOwner.City) && Equal
          (local.KdorVehicleOwner.FirstName, entities.KdorVehicleOwner.FirstName)
          && Equal
          (local.KdorVehicleOwner.HomePhone, entities.KdorVehicleOwner.HomePhone)
          && Equal
          (local.KdorVehicleOwner.LastName, entities.KdorVehicleOwner.LastName) &&
          Equal
          (local.KdorVehicleOwner.MiddleName,
          entities.KdorVehicleOwner.MiddleName) && Equal
          (local.KdorVehicleOwner.OrganizationName,
          entities.KdorVehicleOwner.OrganizationName) && Equal
          (local.KdorVehicleOwner.State, entities.KdorVehicleOwner.State) && Equal
          (local.KdorVehicleOwner.Suffix, entities.KdorVehicleOwner.Suffix) && Equal
          (local.KdorVehicleOwner.VestmentType,
          entities.KdorVehicleOwner.VestmentType) && Equal
          (local.KdorVehicleOwner.ZipCode, entities.KdorVehicleOwner.ZipCode))
        {
          // --Data hasn't changed.  No update required.
        }
        else
        {
          local.Changes.Flag = "Y";

          try
          {
            UpdateKdorVehicleOwner();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "KDOR_VEHICLE_OWNER_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KDOR_VEHICLE_OWNER_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else if (IsEmpty(local.KdorVehicleOwner.AddressLine1) && IsEmpty
        (local.KdorVehicleOwner.AddressLine2) && IsEmpty
        (local.KdorVehicleOwner.BusinessPhone) && IsEmpty
        (local.KdorVehicleOwner.City) && IsEmpty
        (local.KdorVehicleOwner.FirstName) && IsEmpty
        (local.KdorVehicleOwner.HomePhone) && IsEmpty
        (local.KdorVehicleOwner.LastName) && IsEmpty
        (local.KdorVehicleOwner.MiddleName) && IsEmpty
        (local.KdorVehicleOwner.OrganizationName) && IsEmpty
        (local.KdorVehicleOwner.State) && IsEmpty
        (local.KdorVehicleOwner.Suffix) && IsEmpty
        (local.KdorVehicleOwner.VestmentType) && IsEmpty
        (local.KdorVehicleOwner.ZipCode))
      {
        // --No info provided by KDOR for this owner.  No processing required.
      }
      else
      {
        local.Changes.Flag = "Y";

        try
        {
          CreateKdorVehicleOwner();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "KDOR_VEHICLE_OWNER_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "KDOR_VEHICLE_OWNER_PV";

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
    }

    if (AsChar(local.Changes.Flag) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      // --Raise an event to create an alert to the caseworker.
      // -------------------------------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 24;
      local.Infrastructure.ReasonCode = "KDORVEHMATCH";
      local.Infrastructure.EventDetailName = "KDOR_VEHICLE_MATCH";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.EventType = "CASE";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.Function = "LOC";
      local.Infrastructure.UserId = "KDOR";
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();
      local.Infrastructure.Detail = "Review VEH info on KDOR.";

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

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void CreateKdorVehicle()
  {
    var identifier = local.Max.Identifier + 1;
    var lastName = local.KdorVehicle.LastName ?? "";
    var firstName = local.KdorVehicle.FirstName ?? "";
    var ssn = local.KdorVehicle.Ssn ?? "";
    var dateOfBirth = local.KdorVehicle.DateOfBirth;
    var licenseNumber = local.KdorVehicle.LicenseNumber ?? "";
    var vinNumber = local.KdorVehicle.VinNumber ?? "";
    var make = local.KdorVehicle.Make ?? "";
    var model = local.KdorVehicle.Model ?? "";
    var year = local.KdorVehicle.Year ?? "";
    var plateNumber = local.KdorVehicle.PlateNumber ?? "";
    var createdTstamp = Now();
    var createdBy = global.UserId;
    var fkCktCsePersnumb = entities.CsePerson.Number;

    entities.KdorVehicle.Populated = false;
    Update("CreateKdorVehicle",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetNullableString(command, "vinNumber", vinNumber);
        db.SetNullableString(command, "make", make);
        db.SetNullableString(command, "model", model);
        db.SetNullableString(command, "year", year);
        db.SetNullableString(command, "plateNumber", plateNumber);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstamp", null);
        db.SetString(command, "fkCktCsePersnumb", fkCktCsePersnumb);
      });

    entities.KdorVehicle.Identifier = identifier;
    entities.KdorVehicle.LastName = lastName;
    entities.KdorVehicle.FirstName = firstName;
    entities.KdorVehicle.Ssn = ssn;
    entities.KdorVehicle.DateOfBirth = dateOfBirth;
    entities.KdorVehicle.LicenseNumber = licenseNumber;
    entities.KdorVehicle.VinNumber = vinNumber;
    entities.KdorVehicle.Make = make;
    entities.KdorVehicle.Model = model;
    entities.KdorVehicle.Year = year;
    entities.KdorVehicle.PlateNumber = plateNumber;
    entities.KdorVehicle.CreatedTstamp = createdTstamp;
    entities.KdorVehicle.CreatedBy = createdBy;
    entities.KdorVehicle.LastUpdatedBy = "";
    entities.KdorVehicle.LastUpdatedTstamp = null;
    entities.KdorVehicle.FkCktCsePersnumb = fkCktCsePersnumb;
    entities.KdorVehicle.Populated = true;
  }

  private void CreateKdorVehicleOwner()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicle.Populated);

    var identifier = local.Common.Count;
    var organizationName = local.KdorVehicleOwner.OrganizationName ?? "";
    var firstName = local.KdorVehicleOwner.FirstName ?? "";
    var middleName = local.KdorVehicleOwner.MiddleName ?? "";
    var lastName = local.KdorVehicleOwner.LastName ?? "";
    var suffix = local.KdorVehicleOwner.Suffix ?? "";
    var addressLine1 = local.KdorVehicleOwner.AddressLine1 ?? "";
    var addressLine2 = local.KdorVehicleOwner.AddressLine2 ?? "";
    var city = local.KdorVehicleOwner.City ?? "";
    var state = local.KdorVehicleOwner.State ?? "";
    var zipCode = local.KdorVehicleOwner.ZipCode ?? "";
    var vestmentType = local.KdorVehicleOwner.VestmentType ?? "";
    var homePhone = local.KdorVehicleOwner.HomePhone ?? "";
    var businessPhone = local.KdorVehicleOwner.BusinessPhone ?? "";
    var createdTstamp = Now();
    var createdBy = global.UserId;
    var fkCktKdorVehfkCktCsePers = entities.KdorVehicle.FkCktCsePersnumb;
    var fkCktKdorVehidentifier = entities.KdorVehicle.Identifier;

    entities.KdorVehicleOwner.Populated = false;
    Update("CreateKdorVehicleOwner",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "organizationName", organizationName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "vestmentType", vestmentType);
        db.SetNullableString(command, "homePhone", homePhone);
        db.SetNullableString(command, "businessPhone", businessPhone);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstamp", null);
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers", fkCktKdorVehfkCktCsePers);
        db.SetInt32(command, "fkCktKdorVehidentifier", fkCktKdorVehidentifier);
      });

    entities.KdorVehicleOwner.Identifier = identifier;
    entities.KdorVehicleOwner.OrganizationName = organizationName;
    entities.KdorVehicleOwner.FirstName = firstName;
    entities.KdorVehicleOwner.MiddleName = middleName;
    entities.KdorVehicleOwner.LastName = lastName;
    entities.KdorVehicleOwner.Suffix = suffix;
    entities.KdorVehicleOwner.AddressLine1 = addressLine1;
    entities.KdorVehicleOwner.AddressLine2 = addressLine2;
    entities.KdorVehicleOwner.City = city;
    entities.KdorVehicleOwner.State = state;
    entities.KdorVehicleOwner.ZipCode = zipCode;
    entities.KdorVehicleOwner.VestmentType = vestmentType;
    entities.KdorVehicleOwner.HomePhone = homePhone;
    entities.KdorVehicleOwner.BusinessPhone = businessPhone;
    entities.KdorVehicleOwner.CreatedTstamp = createdTstamp;
    entities.KdorVehicleOwner.CreatedBy = createdBy;
    entities.KdorVehicleOwner.LastUpdatedBy = "";
    entities.KdorVehicleOwner.LastUpdatedTstamp = null;
    entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers =
      fkCktKdorVehfkCktCsePers;
    entities.KdorVehicleOwner.FkCktKdorVehidentifier = fkCktKdorVehidentifier;
    entities.KdorVehicleOwner.Populated = true;
  }

  private void DeleteKdorVehicleOwner()
  {
    Update("DeleteKdorVehicleOwner",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", entities.KdorVehicleOwner.Identifier);
          
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers);
        db.SetInt32(
          command, "fkCktKdorVehidentifier",
          entities.KdorVehicleOwner.FkCktKdorVehidentifier);
      });
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
        db.SetString(command, "numb", import.KdorVehicleRecord.PersonNumber);
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

  private bool ReadKdorVehicle1()
  {
    entities.KdorVehicle.Populated = false;

    return Read("ReadKdorVehicle1",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
        db.
          SetNullableString(command, "vinNumber", import.KdorVehicleRecord.Vin);
          
      },
      (db, reader) =>
      {
        entities.KdorVehicle.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicle.LastName = db.GetNullableString(reader, 1);
        entities.KdorVehicle.FirstName = db.GetNullableString(reader, 2);
        entities.KdorVehicle.Ssn = db.GetNullableString(reader, 3);
        entities.KdorVehicle.DateOfBirth = db.GetNullableDate(reader, 4);
        entities.KdorVehicle.LicenseNumber = db.GetNullableString(reader, 5);
        entities.KdorVehicle.VinNumber = db.GetNullableString(reader, 6);
        entities.KdorVehicle.Make = db.GetNullableString(reader, 7);
        entities.KdorVehicle.Model = db.GetNullableString(reader, 8);
        entities.KdorVehicle.Year = db.GetNullableString(reader, 9);
        entities.KdorVehicle.PlateNumber = db.GetNullableString(reader, 10);
        entities.KdorVehicle.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.KdorVehicle.CreatedBy = db.GetString(reader, 12);
        entities.KdorVehicle.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.KdorVehicle.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.KdorVehicle.FkCktCsePersnumb = db.GetString(reader, 15);
        entities.KdorVehicle.Populated = true;
      });
  }

  private bool ReadKdorVehicle2()
  {
    local.Max.Populated = false;

    return Read("ReadKdorVehicle2",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.Max.Identifier = db.GetInt32(reader, 0);
        local.Max.Populated = true;
      });
  }

  private bool ReadKdorVehicleOwner()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicle.Populated);
    entities.KdorVehicleOwner.Populated = false;

    return Read("ReadKdorVehicleOwner",
      (db, command) =>
      {
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicle.FkCktCsePersnumb);
        db.SetInt32(
          command, "fkCktKdorVehidentifier", entities.KdorVehicle.Identifier);
        db.SetInt32(command, "count", local.Common.Count);
      },
      (db, reader) =>
      {
        entities.KdorVehicleOwner.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicleOwner.OrganizationName =
          db.GetNullableString(reader, 1);
        entities.KdorVehicleOwner.FirstName = db.GetNullableString(reader, 2);
        entities.KdorVehicleOwner.MiddleName = db.GetNullableString(reader, 3);
        entities.KdorVehicleOwner.LastName = db.GetNullableString(reader, 4);
        entities.KdorVehicleOwner.Suffix = db.GetNullableString(reader, 5);
        entities.KdorVehicleOwner.AddressLine1 =
          db.GetNullableString(reader, 6);
        entities.KdorVehicleOwner.AddressLine2 =
          db.GetNullableString(reader, 7);
        entities.KdorVehicleOwner.City = db.GetNullableString(reader, 8);
        entities.KdorVehicleOwner.State = db.GetNullableString(reader, 9);
        entities.KdorVehicleOwner.ZipCode = db.GetNullableString(reader, 10);
        entities.KdorVehicleOwner.VestmentType =
          db.GetNullableString(reader, 11);
        entities.KdorVehicleOwner.HomePhone = db.GetNullableString(reader, 12);
        entities.KdorVehicleOwner.BusinessPhone =
          db.GetNullableString(reader, 13);
        entities.KdorVehicleOwner.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.KdorVehicleOwner.CreatedBy = db.GetString(reader, 15);
        entities.KdorVehicleOwner.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.KdorVehicleOwner.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 17);
        entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers =
          db.GetString(reader, 18);
        entities.KdorVehicleOwner.FkCktKdorVehidentifier =
          db.GetInt32(reader, 19);
        entities.KdorVehicleOwner.Populated = true;
      });
  }

  private void UpdateKdorVehicle()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicle.Populated);

    var lastName = local.KdorVehicle.LastName ?? "";
    var firstName = local.KdorVehicle.FirstName ?? "";
    var ssn = local.KdorVehicle.Ssn ?? "";
    var dateOfBirth = local.KdorVehicle.DateOfBirth;
    var licenseNumber = local.KdorVehicle.LicenseNumber ?? "";
    var vinNumber = local.KdorVehicle.VinNumber ?? "";
    var make = local.KdorVehicle.Make ?? "";
    var model = local.KdorVehicle.Model ?? "";
    var year = local.KdorVehicle.Year ?? "";
    var plateNumber = local.KdorVehicle.PlateNumber ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KdorVehicle.Populated = false;
    Update("UpdateKdorVehicle",
      (db, command) =>
      {
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetNullableString(command, "vinNumber", vinNumber);
        db.SetNullableString(command, "make", make);
        db.SetNullableString(command, "model", model);
        db.SetNullableString(command, "year", year);
        db.SetNullableString(command, "plateNumber", plateNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstamp", lastUpdatedTstamp);
        db.SetInt32(command, "identifier", entities.KdorVehicle.Identifier);
        db.SetString(
          command, "fkCktCsePersnumb", entities.KdorVehicle.FkCktCsePersnumb);
      });

    entities.KdorVehicle.LastName = lastName;
    entities.KdorVehicle.FirstName = firstName;
    entities.KdorVehicle.Ssn = ssn;
    entities.KdorVehicle.DateOfBirth = dateOfBirth;
    entities.KdorVehicle.LicenseNumber = licenseNumber;
    entities.KdorVehicle.VinNumber = vinNumber;
    entities.KdorVehicle.Make = make;
    entities.KdorVehicle.Model = model;
    entities.KdorVehicle.Year = year;
    entities.KdorVehicle.PlateNumber = plateNumber;
    entities.KdorVehicle.LastUpdatedBy = lastUpdatedBy;
    entities.KdorVehicle.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KdorVehicle.Populated = true;
  }

  private void UpdateKdorVehicleOwner()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicleOwner.Populated);

    var organizationName = local.KdorVehicleOwner.OrganizationName ?? "";
    var firstName = local.KdorVehicleOwner.FirstName ?? "";
    var middleName = local.KdorVehicleOwner.MiddleName ?? "";
    var lastName = local.KdorVehicleOwner.LastName ?? "";
    var suffix = local.KdorVehicleOwner.Suffix ?? "";
    var addressLine1 = local.KdorVehicleOwner.AddressLine1 ?? "";
    var addressLine2 = local.KdorVehicleOwner.AddressLine2 ?? "";
    var city = local.KdorVehicleOwner.City ?? "";
    var state = local.KdorVehicleOwner.State ?? "";
    var zipCode = local.KdorVehicleOwner.ZipCode ?? "";
    var vestmentType = local.KdorVehicleOwner.VestmentType ?? "";
    var homePhone = local.KdorVehicleOwner.HomePhone ?? "";
    var businessPhone = local.KdorVehicleOwner.BusinessPhone ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KdorVehicleOwner.Populated = false;
    Update("UpdateKdorVehicleOwner",
      (db, command) =>
      {
        db.SetNullableString(command, "organizationName", organizationName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "vestmentType", vestmentType);
        db.SetNullableString(command, "homePhone", homePhone);
        db.SetNullableString(command, "businessPhone", businessPhone);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstamp", lastUpdatedTstamp);
        db.
          SetInt32(command, "identifier", entities.KdorVehicleOwner.Identifier);
          
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers);
        db.SetInt32(
          command, "fkCktKdorVehidentifier",
          entities.KdorVehicleOwner.FkCktKdorVehidentifier);
      });

    entities.KdorVehicleOwner.OrganizationName = organizationName;
    entities.KdorVehicleOwner.FirstName = firstName;
    entities.KdorVehicleOwner.MiddleName = middleName;
    entities.KdorVehicleOwner.LastName = lastName;
    entities.KdorVehicleOwner.Suffix = suffix;
    entities.KdorVehicleOwner.AddressLine1 = addressLine1;
    entities.KdorVehicleOwner.AddressLine2 = addressLine2;
    entities.KdorVehicleOwner.City = city;
    entities.KdorVehicleOwner.State = state;
    entities.KdorVehicleOwner.ZipCode = zipCode;
    entities.KdorVehicleOwner.VestmentType = vestmentType;
    entities.KdorVehicleOwner.HomePhone = homePhone;
    entities.KdorVehicleOwner.BusinessPhone = businessPhone;
    entities.KdorVehicleOwner.LastUpdatedBy = lastUpdatedBy;
    entities.KdorVehicleOwner.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KdorVehicleOwner.Populated = true;
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
    /// A value of KdorVehicleRecord.
    /// </summary>
    [JsonPropertyName("kdorVehicleRecord")]
    public KdorVehicleRecord KdorVehicleRecord
    {
      get => kdorVehicleRecord ??= new();
      set => kdorVehicleRecord = value;
    }

    private KdorVehicleRecord kdorVehicleRecord;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Changes.
    /// </summary>
    [JsonPropertyName("changes")]
    public Common Changes
    {
      get => changes ??= new();
      set => changes = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public KdorVehicle Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of KdorVehicleOwner.
    /// </summary>
    [JsonPropertyName("kdorVehicleOwner")]
    public KdorVehicleOwner KdorVehicleOwner
    {
      get => kdorVehicleOwner ??= new();
      set => kdorVehicleOwner = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Common common;
    private Common changes;
    private KdorVehicle max;
    private KdorVehicleOwner kdorVehicleOwner;
    private KdorVehicle kdorVehicle;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KdorVehicleOwner.
    /// </summary>
    [JsonPropertyName("kdorVehicleOwner")]
    public KdorVehicleOwner KdorVehicleOwner
    {
      get => kdorVehicleOwner ??= new();
      set => kdorVehicleOwner = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
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

    private KdorVehicleOwner kdorVehicleOwner;
    private KdorVehicle kdorVehicle;
    private CsePerson csePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
  }
#endregion
}
