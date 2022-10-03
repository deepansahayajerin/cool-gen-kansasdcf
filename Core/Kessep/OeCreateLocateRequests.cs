// Program: OE_CREATE_LOCATE_REQUESTS, ID: 374417871, model: 746.
// Short name: SWE02615
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_LOCATE_REQUESTS.
/// </para>
/// <para>
/// Create Locate Request.
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_LOCATE_REQUESTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateLocateRequests.
  /// </summary>
  public OeCreateLocateRequests(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ----------------------------------------
    // 07/??/2000  SWSCBRS	?????		Initial Coding
    // 03/??/2001  SWSRPRM	WR # 291 	Add License Suspension
    // 10/11/2005  GVandy	?????		Performance Improvements.
    // 03/05/2007  GVandy	PR261671	Re-written to improve performance.
    // -----------------------------------------------------------------------------------------------
    local.LocateRequest.AgencyNumber = "0" + Substring
      (import.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 7, 4);

    if (ReadLocateRequest())
    {
      if (Equal(entities.LocateRequest.ResponseDate, local.NullDate.Date))
      {
        if (Equal(entities.LocateRequest.RequestDate, local.NullDate.Date))
        {
          return;
        }

        if (Lt(Now().Date, AddDays(entities.LocateRequest.RequestDate,
          (int)StringToNumber(Substring(
            import.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 1, 3)))))
        {
          return;
        }
      }
      else if (Lt(Now().Date, AddDays(entities.LocateRequest.RequestDate,
        (int)StringToNumber(Substring(
          import.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 4, 3)))))
      {
        return;
      }

      if (AsChar(import.LicSuspQualification.Flag) == 'Y')
      {
        if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y')
        {
          local.LocateRequest.LicenseSuspensionInd = "Y";
        }
        else
        {
          local.LocateRequest.LicenseSuspensionInd = "W";
        }
      }
      else
      {
        local.LocateRequest.LicenseSuspensionInd = "N";
      }

      try
      {
        UpdateLocateRequest();
        ++export.ImportExportNoLocReqUpdated.Count;
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
    }
    else
    {
      local.CsePersonsWorkSet.Number = import.CsePerson.Number;
      UseCabReadAdabasPersonBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if ((IsEmpty(local.CsePersonsWorkSet.Ssn) || StringToNumber
        (local.CsePersonsWorkSet.Ssn) == 0) && Equal
        (local.CsePersonsWorkSet.Dob, local.NullDate.Date))
      {
        return;
      }

      if (AsChar(import.LicSuspQualification.Flag) == 'Y')
      {
        local.LocateRequest.LicenseSuspensionInd = "W";
      }
      else
      {
        local.LocateRequest.LicenseSuspensionInd = "N";
      }

      try
      {
        CreateLocateRequest();
        ++export.ImportExportNoLocReqCreated.Count;
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
    }
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateLocateRequest()
  {
    var socialSecurityNumber = local.CsePersonsWorkSet.Ssn;
    var dateOfBirth = local.CsePersonsWorkSet.Dob;
    var csePersonNumber = import.CsePerson.Number;
    var agencyNumber = local.LocateRequest.AgencyNumber;
    var sequenceNumber = 1;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var licenseSuspensionInd = local.LocateRequest.LicenseSuspensionInd ?? "";

    entities.LocateRequest.Populated = false;
    Update("CreateLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "ssnNumber", socialSecurityNumber);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableDate(command, "requestDate", null);
        db.SetNullableDate(command, "responseDate", null);
        db.SetNullableDate(command, "licenseIssuedDt", null);
        db.SetNullableDate(command, "licenseExpDate", null);
        db.SetNullableDate(command, "licenseSuspDate", null);
        db.SetNullableString(command, "licenseNumber", "");
        db.SetString(command, "agencyNumber", agencyNumber);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "licSourceName", "");
        db.SetNullableString(command, "street1", "");
        db.SetNullableString(command, "addressType", "");
        db.SetNullableString(command, "street2", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "street4", "");
        db.SetNullableString(command, "city", "");
        db.SetNullableString(command, "state", "");
        db.SetNullableString(command, "zipCode5", "");
        db.SetNullableString(command, "zipCode4", "");
        db.SetNullableString(command, "zipCode3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "licSuspensionInd", licenseSuspensionInd);
      });

    entities.LocateRequest.SocialSecurityNumber = socialSecurityNumber;
    entities.LocateRequest.DateOfBirth = dateOfBirth;
    entities.LocateRequest.CsePersonNumber = csePersonNumber;
    entities.LocateRequest.RequestDate = null;
    entities.LocateRequest.ResponseDate = null;
    entities.LocateRequest.LicenseIssuedDate = null;
    entities.LocateRequest.LicenseExpirationDate = null;
    entities.LocateRequest.LicenseSuspendedDate = null;
    entities.LocateRequest.LicenseNumber = "";
    entities.LocateRequest.AgencyNumber = agencyNumber;
    entities.LocateRequest.SequenceNumber = sequenceNumber;
    entities.LocateRequest.LicenseSourceName = "";
    entities.LocateRequest.Street1 = "";
    entities.LocateRequest.AddressType = "";
    entities.LocateRequest.Street2 = "";
    entities.LocateRequest.Street3 = "";
    entities.LocateRequest.Street4 = "";
    entities.LocateRequest.City = "";
    entities.LocateRequest.State = "";
    entities.LocateRequest.ZipCode5 = "";
    entities.LocateRequest.ZipCode4 = "";
    entities.LocateRequest.ZipCode3 = "";
    entities.LocateRequest.Province = "";
    entities.LocateRequest.PostalCode = "";
    entities.LocateRequest.Country = "";
    entities.LocateRequest.CreatedTimestamp = createdTimestamp;
    entities.LocateRequest.CreatedBy = createdBy;
    entities.LocateRequest.LastUpdatedTimestamp = null;
    entities.LocateRequest.LastUpdatedBy = "";
    entities.LocateRequest.LicenseSuspensionInd = licenseSuspensionInd;
    entities.LocateRequest.Populated = true;
  }

  private bool ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", import.CsePerson.Number);
        db.SetString(command, "agencyNumber", local.LocateRequest.AgencyNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.ResponseDate = db.GetNullableDate(reader, 4);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 5);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 7);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 8);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 9);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 10);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 11);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 12);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 13);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 14);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 15);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.City = db.GetNullableString(reader, 17);
        entities.LocateRequest.State = db.GetNullableString(reader, 18);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 19);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 20);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 21);
        entities.LocateRequest.Province = db.GetNullableString(reader, 22);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 23);
        entities.LocateRequest.Country = db.GetNullableString(reader, 24);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 26);
        entities.LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 28);
        entities.LocateRequest.LicenseSuspensionInd =
          db.GetNullableString(reader, 29);
        entities.LocateRequest.Populated = true;
      });
  }

  private void UpdateLocateRequest()
  {
    var requestDate = local.NullDate.Date;
    var sequenceNumber = 1;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var licenseSuspensionInd = local.LocateRequest.LicenseSuspensionInd ?? "";

    entities.LocateRequest.Populated = false;
    Update("UpdateLocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(command, "requestDate", requestDate);
        db.SetNullableDate(command, "responseDate", requestDate);
        db.SetNullableDate(command, "licenseIssuedDt", requestDate);
        db.SetNullableDate(command, "licenseExpDate", requestDate);
        db.SetNullableDate(command, "licenseSuspDate", requestDate);
        db.SetNullableString(command, "licenseNumber", "");
        db.SetInt32(command, "sequenceNumber1", sequenceNumber);
        db.SetNullableString(command, "licSourceName", "");
        db.SetNullableString(command, "street1", "");
        db.SetNullableString(command, "addressType", "");
        db.SetNullableString(command, "street2", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "street4", "");
        db.SetNullableString(command, "city", "");
        db.SetNullableString(command, "state", "");
        db.SetNullableString(command, "zipCode5", "");
        db.SetNullableString(command, "zipCode4", "");
        db.SetNullableString(command, "zipCode3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetNullableString(command, "licSuspensionInd", licenseSuspensionInd);
        db.SetString(
          command, "csePersonNumber", entities.LocateRequest.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", entities.LocateRequest.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber2", entities.LocateRequest.SequenceNumber);
      });

    entities.LocateRequest.RequestDate = requestDate;
    entities.LocateRequest.ResponseDate = requestDate;
    entities.LocateRequest.LicenseIssuedDate = requestDate;
    entities.LocateRequest.LicenseExpirationDate = requestDate;
    entities.LocateRequest.LicenseSuspendedDate = requestDate;
    entities.LocateRequest.LicenseNumber = "";
    entities.LocateRequest.SequenceNumber = sequenceNumber;
    entities.LocateRequest.LicenseSourceName = "";
    entities.LocateRequest.Street1 = "";
    entities.LocateRequest.AddressType = "";
    entities.LocateRequest.Street2 = "";
    entities.LocateRequest.Street3 = "";
    entities.LocateRequest.Street4 = "";
    entities.LocateRequest.City = "";
    entities.LocateRequest.State = "";
    entities.LocateRequest.ZipCode5 = "";
    entities.LocateRequest.ZipCode4 = "";
    entities.LocateRequest.ZipCode3 = "";
    entities.LocateRequest.Province = "";
    entities.LocateRequest.PostalCode = "";
    entities.LocateRequest.Country = "";
    entities.LocateRequest.CreatedTimestamp = createdTimestamp;
    entities.LocateRequest.CreatedBy = createdBy;
    entities.LocateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.LocateRequest.LastUpdatedBy = createdBy;
    entities.LocateRequest.LicenseSuspensionInd = licenseSuspensionInd;
    entities.LocateRequest.Populated = true;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of LicSuspQualification.
    /// </summary>
    [JsonPropertyName("licSuspQualification")]
    public Common LicSuspQualification
    {
      get => licSuspQualification ??= new();
      set => licSuspQualification = value;
    }

    private CodeValue codeValue;
    private CsePerson csePerson;
    private Common licSuspQualification;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportExportNoLocReqCreated.
    /// </summary>
    [JsonPropertyName("importExportNoLocReqCreated")]
    public Common ImportExportNoLocReqCreated
    {
      get => importExportNoLocReqCreated ??= new();
      set => importExportNoLocReqCreated = value;
    }

    /// <summary>
    /// A value of ImportExportNoLocReqUpdated.
    /// </summary>
    [JsonPropertyName("importExportNoLocReqUpdated")]
    public Common ImportExportNoLocReqUpdated
    {
      get => importExportNoLocReqUpdated ??= new();
      set => importExportNoLocReqUpdated = value;
    }

    private Common importExportNoLocReqCreated;
    private Common importExportNoLocReqUpdated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private LocateRequest locateRequest;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    private LocateRequest locateRequest;
  }
#endregion
}
