// Program: SI_B286_PROCESS_DL_ERROR, ID: 1625319360, model: 746.
// Short name: SWE02213
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B286_PROCESS_DL_ERROR.
/// </summary>
[Serializable]
public partial class SiB286ProcessDlError: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B286_PROCESS_DL_ERROR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB286ProcessDlError(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB286ProcessDlError.
  /// </summary>
  public SiB286ProcessDlError(IContext context, Import import, Export export):
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
    // 03/21/19  GVandy	CQ65607		If status and reason code are the same as the
    // 					previous week then don't create new alerts.
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
    local.Dob.Text10 =
      Substring(import.KdorDlErrorRecord.DateOfBirth,
      KdorDlErrorRecord.DateOfBirth_MaxLength, 1, 4) + "-" + Substring
      (import.KdorDlErrorRecord.DateOfBirth,
      KdorDlErrorRecord.DateOfBirth_MaxLength, 5, 2) + "-" + Substring
      (import.KdorDlErrorRecord.DateOfBirth,
      KdorDlErrorRecord.DateOfBirth_MaxLength, 7, 2);
    local.KdorDriversLicense.DateOfBirth = StringToDate(local.Dob.Text10);
    local.KdorDriversLicense.ErrorReason = import.KdorDlErrorRecord.ErrorReason;
    local.KdorDriversLicense.FirstName = import.KdorDlErrorRecord.FirstName;
    local.KdorDriversLicense.LastName = import.KdorDlErrorRecord.LastName;
    local.KdorDriversLicense.LicenseNumber =
      import.KdorDlErrorRecord.DriversLicenseNumber;
    local.KdorDriversLicense.Ssn = import.KdorDlErrorRecord.Ssn;
    local.KdorDriversLicense.Status = import.KdorDlErrorRecord.Status;
    local.KdorDriversLicense.Type1 = "E";

    if (ReadKdorDriversLicense())
    {
      if (Equal(entities.KdorDriversLicense.Status,
        local.KdorDriversLicense.Status) && Equal
        (entities.KdorDriversLicense.ErrorReason,
        local.KdorDriversLicense.ErrorReason))
      {
        local.ChangeToStatusReason.Flag = "N";
      }
      else
      {
        local.ChangeToStatusReason.Flag = "Y";
      }

      // --Note that the attributes in the "kdl_match" subtype are set to 
      // initialized values.
      // --This is done because the last time we received info from KDOR might 
      // have been a
      // --match.  We need to overwrite that data and store the info we received
      // this
      // --week in the "kdl_error" subtype attributes.
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
      local.ChangeToStatusReason.Flag = "Y";

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

    // 03/21/19 GVandy  CQ65607  If status and reason code are the same as the 
    // previous week then don't create new alerts.
    if (AsChar(local.ChangeToStatusReason.Flag) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      // --Raise an event to create an alert to the caseworker.
      // -------------------------------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 24;
      local.Infrastructure.ReasonCode = "KDORDLMATCHERR";
      local.Infrastructure.EventDetailName = "KDOR_DRIVERS_LICENSE_MATCH_ERROR";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.EventType = "CASE";
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.Function = "LOC";
      local.Infrastructure.UserId = "KDOR";
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();

      // --Convert status code to a description.
      // --
      // --  Status  	Description
      // --  ------  	-------------------
      // --  CAN		Cancelled
      // --  DED		Deceased
      // --  DIS		Disqualified
      // --  EXP		Expired
      // --  M/R		Moped Revoked
      // --  M/S		Moped Suspended
      // --  OTH		Other
      // --  RES		Restricted
      // --  REV		Revoked
      // --  SUR		Surrendered
      // --  SUS		Suspended
      // --  VAL		Valid
      switch(TrimEnd(import.KdorDlErrorRecord.Status))
      {
        case "CAN":
          local.Status.Text30 = "Cancelled";

          break;
        case "DED":
          local.Status.Text30 = "Deceased";

          break;
        case "DIS":
          local.Status.Text30 = "Disqualified";

          break;
        case "EXP":
          local.Status.Text30 = "Expired";

          break;
        case "M/R":
          local.Status.Text30 = "Moped Revoked";

          break;
        case "M/S":
          local.Status.Text30 = "Moped Suspended";

          break;
        case "OTH":
          local.Status.Text30 = "Other";

          break;
        case "RES":
          local.Status.Text30 = "Restricted";

          break;
        case "REV":
          local.Status.Text30 = "Revoked";

          break;
        case "SUR":
          local.Status.Text30 = "Surrendered";

          break;
        case "SUS":
          local.Status.Text30 = "Suspended";

          break;
        case "VAL":
          local.Status.Text30 = "Valid";

          break;
        default:
          local.Status.Text30 = "";

          break;
      }

      local.Infrastructure.Detail = "Check KDL# " + import
        .KdorDlErrorRecord.DriversLicenseNumber + ", " + TrimEnd
        (local.Status.Text30) + "/" + import.KdorDlErrorRecord.ErrorReason;

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

    // -------------------------------------------------------------------------------------
    // --Remove Kansas Drivers License Number from APDS for certain KDOR status 
    // values only.
    // --
    // --  Status  	Description		Remove KDL from APDS?
    // --  ------  	-------------------	---------------------
    // --  CAN		Cancelled			Y
    // --  DED		Deceased			Y
    // --  DIS		Disqualified			Y
    // --  EXP		Expired				Y
    // --  M/R		Moped Revoked			N
    // --  M/S		Moped Suspended			N
    // --  OTH		Other				Y
    // --  RES		Restricted			N
    // --  REV		Revoked				Y
    // --  SUR		Surrendered			Y
    // --  SUS		Suspended			Y
    // --  VAL		Valid				N
    // --
    // -------------------------------------------------------------------------------------
    switch(TrimEnd(import.KdorDlErrorRecord.Status))
    {
      case "CAN":
        break;
      case "DED":
        break;
      case "DIS":
        break;
      case "EXP":
        break;
      case "OTH":
        break;
      case "REV":
        break;
      case "SUR":
        break;
      case "SUS":
        break;
      default:
        return;
    }

    foreach(var item in ReadCsePersonLicense())
    {
      local.EabReportSend.RptDetail = "NCP " + entities.CsePerson.Number + " - KDL " +
        TrimEnd(entities.CsePersonLicense.Number) + " removed.";
      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(11) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      DeleteCsePersonLicense();
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
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
    var status = local.KdorDriversLicense.Status ?? "";
    var errorReason = local.KdorDriversLicense.ErrorReason ?? "";
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
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "errorReason", errorReason);
        db.SetNullableString(command, "dlClassInd", "");
        db.SetNullableString(command, "motorcycleClassInd", "");
        db.SetNullableString(command, "cdlClassInd", "");
        db.SetNullableDate(command, "expirationDt", null);
        db.SetNullableString(command, "genderCode", "");
        db.SetNullableString(command, "addressLine1", "");
        db.SetNullableString(command, "addressLine2", "");
        db.SetNullableString(command, "city", "");
        db.SetNullableString(command, "state", "");
        db.SetNullableString(command, "zipCode", "");
        db.SetNullableString(command, "heightFeet", "");
        db.SetNullableString(command, "heightInches", "");
        db.SetNullableString(command, "weight", "");
        db.SetNullableString(command, "eyeColor", "");
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
    entities.KdorDriversLicense.Status = status;
    entities.KdorDriversLicense.ErrorReason = errorReason;
    entities.KdorDriversLicense.DlClassInd = "";
    entities.KdorDriversLicense.MotorcycleClassInd = "";
    entities.KdorDriversLicense.CdlClassInd = "";
    entities.KdorDriversLicense.ExpirationDt = null;
    entities.KdorDriversLicense.GenderCode = "";
    entities.KdorDriversLicense.AddressLine1 = "";
    entities.KdorDriversLicense.AddressLine2 = "";
    entities.KdorDriversLicense.City = "";
    entities.KdorDriversLicense.State = "";
    entities.KdorDriversLicense.ZipCode = "";
    entities.KdorDriversLicense.HeightFeet = "";
    entities.KdorDriversLicense.HeightInches = "";
    entities.KdorDriversLicense.Weight = "";
    entities.KdorDriversLicense.EyeColor = "";
    entities.KdorDriversLicense.FkCktCsePersnumb = fkCktCsePersnumb;
    entities.KdorDriversLicense.Populated = true;
  }

  private void DeleteCsePersonLicense()
  {
    Update("DeleteCsePersonLicense",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", entities.CsePersonLicense.Identifier);
          
        db.SetString(command, "cspNumber", entities.CsePersonLicense.CspNumber);
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
        db.SetString(command, "numb", import.KdorDlErrorRecord.PersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return ReadEach("ReadCsePersonLicense",
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
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 3);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 4);
        entities.CsePersonLicense.Populated = true;

        return true;
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
    /// A value of KdorDlErrorRecord.
    /// </summary>
    [JsonPropertyName("kdorDlErrorRecord")]
    public KdorDlErrorRecord KdorDlErrorRecord
    {
      get => kdorDlErrorRecord ??= new();
      set => kdorDlErrorRecord = value;
    }

    private KdorDlErrorRecord kdorDlErrorRecord;
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
    /// A value of ChangeToStatusReason.
    /// </summary>
    [JsonPropertyName("changeToStatusReason")]
    public Common ChangeToStatusReason
    {
      get => changeToStatusReason ??= new();
      set => changeToStatusReason = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public TextWorkArea Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public TextWorkArea Dob
    {
      get => dob ??= new();
      set => dob = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private Common changeToStatusReason;
    private TextWorkArea status;
    private TextWorkArea dob;
    private KdorDriversLicense kdorDriversLicense;
    private Infrastructure infrastructure;
    private DateWorkArea current;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private KdorDriversLicense kdorDriversLicense;
    private CsePersonLicense csePersonLicense;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}
