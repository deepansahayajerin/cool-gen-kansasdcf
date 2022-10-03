// Program: OE_B447_SVES_GENERAL_INFO_PRS, ID: 945066136, model: 746.
// Short name: SWE04471
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
/// A program: OE_B447_SVES_GENERAL_INFO_PRS.
/// </para>
/// <para>
/// This Action block maintains SVES General information (i.e. common 
/// information for all SVES response types).
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesGeneralInfoPrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_GENERAL_INFO_PRS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesGeneralInfoPrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesGeneralInfoPrs.
  /// </summary>
  public OeB447SvesGeneralInfoPrs(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************************************
    // * This Action block creates/updates the SVES generation information 
    // entity for the given *
    // * Member Id and Agency Response code.  If there is no record exists for 
    // the given key    *
    // * value, action block will create a new record otherwise updates the 
    // existing record with*
    // * the new information.
    // 
    // *
    // *
    // 
    // *
    // ******************************************************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 06/02/2011  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ****************************************************************************************
    // ** Find out recent FPLS request for that person, this will populated form
    // FPLS Request**
    // ** table, this is only for worker's 
    // information.
    // 
    // **
    // ****************************************************************************************
    local.CsePerson.Number = Substring(import.FcrSvesGenInfo.MemberId, 6, 10);
    local.FcrSvesGenInfo.RequestDate = import.FcrSvesGenInfo.RequestDate;

    if (ReadFplsLocateRequest())
    {
      local.FcrSvesGenInfo.RequestDate =
        entities.ExistingFplsLocateRequest.RequestSentDate;
    }

    if (ReadFcrSvesGenInfo())
    {
      if (Lt(local.Null1.Date, entities.ExistingFcrSvesGenInfo.RequestDate))
      {
        local.FcrSvesGenInfo.RequestDate =
          entities.ExistingFcrSvesGenInfo.RequestDate;
      }

      try
      {
        UpdateFcrSvesGenInfo();
        ++import.TotSvesGeninfoUpdated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FCR_SVES_GEN_INFO_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FCR_SVES_GEN_INFO_PV";

            return;
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
        CreateFcrSvesGenInfo();
        ++import.TotSvesGeninfoCreated.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FCR_SVES_GEN_INFO_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FCR_SVES_GEN_INFO_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ****************************************************************************************
    // **                        SVES 
    // Response Address Loading
    // 
    // **
    // **
    // 
    // **
    // ****************************************************************************************
    // 01 - Residencial Address
    // 02 - Person/Claimant Address
    // 03 - District Office Address
    // 04 - Payee Mailing Address
    // 05 - Prison Address
    for(import.SvesAddressList.Index = 0; import.SvesAddressList.Index < import
      .SvesAddressList.Count; ++import.SvesAddressList.Index)
    {
      if (!import.SvesAddressList.CheckSize())
      {
        break;
      }

      if (ReadFcrSvesAddress())
      {
        try
        {
          UpdateFcrSvesAddress();

          switch(TrimEnd(import.SvesAddressList.Item.FcrSvesAddress.
            SvesAddressTypeCode))
          {
            case "01":
              ++import.TotResaddrUpdated.Count;

              break;
            case "02":
              ++import.TotPeraddrUpdated.Count;

              break;
            case "03":
              ++import.TotDisaddrUpdated.Count;

              break;
            case "04":
              ++import.TotPayaddrUpdated.Count;

              break;
            case "05":
              ++import.TotPriaddrUpdated.Count;

              break;
            default:
              break;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_ADDRESS_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_ADDRESS_PV";

              return;
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
          CreateFcrSvesAddress();

          switch(TrimEnd(import.SvesAddressList.Item.FcrSvesAddress.
            SvesAddressTypeCode))
          {
            case "01":
              ++import.TotResaddrCreated.Count;

              break;
            case "02":
              ++import.TotPeraddrCreated.Count;

              break;
            case "03":
              ++import.TotDisaddrCreated.Count;

              break;
            case "04":
              ++import.TotPayaddrCreated.Count;

              break;
            case "05":
              ++import.TotPriaddrCreated.Count;

              break;
            default:
              break;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_ADDRESS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_ADDRESS_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    import.SvesAddressList.CheckIndex();
  }

  private void CreateFcrSvesAddress()
  {
    var fcgMemberId = entities.ExistingFcrSvesGenInfo.MemberId;
    var fcgLSRspAgy =
      entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var svesAddressTypeCode =
      import.SvesAddressList.Item.FcrSvesAddress.SvesAddressTypeCode;
    var addressScrubIndicator1 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator1 ?? "";
    var addressScrubIndicator2 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator2 ?? "";
    var addressScrubIndicator3 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator3 ?? "";
    var createdBy = import.FcrSvesGenInfo.CreatedBy;
    var createdTimestamp = import.FcrSvesGenInfo.CreatedTimestamp;
    var lastUpdatedBy = local.FcrSvesAddress.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.FcrSvesAddress.LastUpdatedTimestamp;
    var state = import.SvesAddressList.Item.FcrSvesAddress.State ?? "";
    var zipCode5 = import.SvesAddressList.Item.FcrSvesAddress.ZipCode5 ?? "";
    var zipCode4 = import.SvesAddressList.Item.FcrSvesAddress.ZipCode4 ?? "";
    var zip3 = import.SvesAddressList.Item.FcrSvesAddress.Zip3 ?? "";
    var city = import.SvesAddressList.Item.FcrSvesAddress.City ?? "";
    var addressLine1 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine1 ?? "";
    var addressLine2 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine2 ?? "";
    var addressLine3 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine3 ?? "";
    var addressLine4 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine4 ?? "";

    entities.ExistingFcrSvesAddress.Populated = false;
    Update("CreateFcrSvesAddress",
      (db, command) =>
      {
        db.SetString(command, "fcgMemberId", fcgMemberId);
        db.SetString(command, "fcgLSRspAgy", fcgLSRspAgy);
        db.SetString(command, "addressType", svesAddressTypeCode);
        db.SetNullableString(command, "addrScrubInd1", addressScrubIndicator1);
        db.SetNullableString(command, "addrScrubInd2", addressScrubIndicator2);
        db.SetNullableString(command, "addrScrubInd3", addressScrubIndicator3);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "addressLine3", addressLine3);
        db.SetNullableString(command, "addressLine4", addressLine4);
      });

    entities.ExistingFcrSvesAddress.FcgMemberId = fcgMemberId;
    entities.ExistingFcrSvesAddress.FcgLSRspAgy = fcgLSRspAgy;
    entities.ExistingFcrSvesAddress.SvesAddressTypeCode = svesAddressTypeCode;
    entities.ExistingFcrSvesAddress.AddressScrubIndicator1 =
      addressScrubIndicator1;
    entities.ExistingFcrSvesAddress.AddressScrubIndicator2 =
      addressScrubIndicator2;
    entities.ExistingFcrSvesAddress.AddressScrubIndicator3 =
      addressScrubIndicator3;
    entities.ExistingFcrSvesAddress.CreatedBy = createdBy;
    entities.ExistingFcrSvesAddress.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesAddress.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesAddress.State = state;
    entities.ExistingFcrSvesAddress.ZipCode5 = zipCode5;
    entities.ExistingFcrSvesAddress.ZipCode4 = zipCode4;
    entities.ExistingFcrSvesAddress.Zip3 = zip3;
    entities.ExistingFcrSvesAddress.City = city;
    entities.ExistingFcrSvesAddress.AddressLine1 = addressLine1;
    entities.ExistingFcrSvesAddress.AddressLine2 = addressLine2;
    entities.ExistingFcrSvesAddress.AddressLine3 = addressLine3;
    entities.ExistingFcrSvesAddress.AddressLine4 = addressLine4;
    entities.ExistingFcrSvesAddress.Populated = true;
  }

  private void CreateFcrSvesGenInfo()
  {
    var memberId = import.FcrSvesGenInfo.MemberId;
    var locateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var svesMatchType = import.FcrSvesGenInfo.SvesMatchType ?? "";
    var transmitterStateTerritoryCode =
      import.FcrSvesGenInfo.TransmitterStateTerritoryCode ?? "";
    var sexCode = import.FcrSvesGenInfo.SexCode ?? "";
    var returnedDateOfBirth = import.FcrSvesGenInfo.ReturnedDateOfBirth;
    var returnedDateOfDeath = import.FcrSvesGenInfo.ReturnedDateOfDeath;
    var submittedDateOfBirth = import.FcrSvesGenInfo.SubmittedDateOfBirth;
    var ssn = import.FcrSvesGenInfo.Ssn ?? "";
    var locateClosedIndicator = import.FcrSvesGenInfo.LocateClosedIndicator ?? ""
      ;
    var fipsCountyCode = import.FcrSvesGenInfo.FipsCountyCode ?? "";
    var locateRequestType = import.FcrSvesGenInfo.LocateRequestType ?? "";
    var locateResponseCode = import.FcrSvesGenInfo.LocateResponseCode ?? "";
    var multipleSsnIndicator = import.FcrSvesGenInfo.MultipleSsnIndicator ?? "";
    var multipleSsn = import.FcrSvesGenInfo.MultipleSsn ?? "";
    var participantType = import.FcrSvesGenInfo.ParticipantType ?? "";
    var familyViolenceState1 = import.FcrSvesGenInfo.FamilyViolenceState1 ?? "";
    var familyViolenceState2 = import.FcrSvesGenInfo.FamilyViolenceState2 ?? "";
    var familyViolenceState3 = import.FcrSvesGenInfo.FamilyViolenceState3 ?? "";
    var sortStateCode = import.FcrSvesGenInfo.SortStateCode ?? "";
    var requestDate = local.FcrSvesGenInfo.RequestDate;
    var responseReceivedDate = import.FcrSvesGenInfo.ResponseReceivedDate;
    var createdBy = import.FcrSvesGenInfo.CreatedBy;
    var createdTimestamp = import.FcrSvesGenInfo.CreatedTimestamp;
    var lastUpdatedBy = local.FcrSvesGenInfo.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.FcrSvesGenInfo.LastUpdatedTimestamp;
    var returnedFirstName = import.FcrSvesGenInfo.ReturnedFirstName ?? "";
    var returnedMiddleName = import.FcrSvesGenInfo.ReturnedMiddleName ?? "";
    var returnedLastName = import.FcrSvesGenInfo.ReturnedLastName ?? "";
    var submittedFirstName = import.FcrSvesGenInfo.SubmittedFirstName ?? "";
    var submittedMiddleName = import.FcrSvesGenInfo.SubmittedMiddleName ?? "";
    var submittedLastName = import.FcrSvesGenInfo.SubmittedLastName ?? "";
    var userField = import.FcrSvesGenInfo.UserField ?? "";

    entities.ExistingFcrSvesGenInfo.Populated = false;
    Update("CreateFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", memberId);
        db.SetString(command, "locSrcRspAgyCd", locateSourceResponseAgencyCo);
        db.SetNullableString(command, "svesMatchType", svesMatchType);
        db.SetNullableString(
          command, "trnsmtrStTerrCd", transmitterStateTerritoryCode);
        db.SetNullableString(command, "sexCode", sexCode);
        db.SetNullableDate(command, "returnedDob", returnedDateOfBirth);
        db.SetNullableDate(command, "returnedDod", returnedDateOfDeath);
        db.SetNullableDate(command, "submittedDob", submittedDateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "locateClosedInd", locateClosedIndicator);
        db.SetNullableString(command, "fipsCountyCode", fipsCountyCode);
        db.SetNullableString(command, "locateRequestTyp", locateRequestType);
        db.SetNullableString(command, "locateResponseCd", locateResponseCode);
        db.SetNullableString(command, "multipleSsnInd", multipleSsnIndicator);
        db.SetNullableString(command, "multipleSsn", multipleSsn);
        db.SetNullableString(command, "participantType", participantType);
        db.SetNullableString(command, "fvState1", familyViolenceState1);
        db.SetNullableString(command, "fvState2", familyViolenceState2);
        db.SetNullableString(command, "fvState3", familyViolenceState3);
        db.SetNullableString(command, "sortStateCode", sortStateCode);
        db.SetNullableDate(command, "requestDt", requestDate);
        db.SetNullableDate(command, "responseRecevdDt", responseReceivedDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "retdFirstName", returnedFirstName);
        db.SetNullableString(command, "retdMiddleName", returnedMiddleName);
        db.SetNullableString(command, "retdLastName", returnedLastName);
        db.SetNullableString(command, "submitdFirstName", submittedFirstName);
        db.SetNullableString(command, "submtdMiddleName", submittedMiddleName);
        db.SetNullableString(command, "submitdLastName", submittedLastName);
        db.SetNullableString(command, "userField", userField);
      });

    entities.ExistingFcrSvesGenInfo.MemberId = memberId;
    entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
      locateSourceResponseAgencyCo;
    entities.ExistingFcrSvesGenInfo.SvesMatchType = svesMatchType;
    entities.ExistingFcrSvesGenInfo.TransmitterStateTerritoryCode =
      transmitterStateTerritoryCode;
    entities.ExistingFcrSvesGenInfo.SexCode = sexCode;
    entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth = returnedDateOfBirth;
    entities.ExistingFcrSvesGenInfo.ReturnedDateOfDeath = returnedDateOfDeath;
    entities.ExistingFcrSvesGenInfo.SubmittedDateOfBirth = submittedDateOfBirth;
    entities.ExistingFcrSvesGenInfo.Ssn = ssn;
    entities.ExistingFcrSvesGenInfo.LocateClosedIndicator =
      locateClosedIndicator;
    entities.ExistingFcrSvesGenInfo.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrSvesGenInfo.LocateRequestType = locateRequestType;
    entities.ExistingFcrSvesGenInfo.LocateResponseCode = locateResponseCode;
    entities.ExistingFcrSvesGenInfo.MultipleSsnIndicator = multipleSsnIndicator;
    entities.ExistingFcrSvesGenInfo.MultipleSsn = multipleSsn;
    entities.ExistingFcrSvesGenInfo.ParticipantType = participantType;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState1 = familyViolenceState1;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState2 = familyViolenceState2;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState3 = familyViolenceState3;
    entities.ExistingFcrSvesGenInfo.SortStateCode = sortStateCode;
    entities.ExistingFcrSvesGenInfo.RequestDate = requestDate;
    entities.ExistingFcrSvesGenInfo.ResponseReceivedDate = responseReceivedDate;
    entities.ExistingFcrSvesGenInfo.CreatedBy = createdBy;
    entities.ExistingFcrSvesGenInfo.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesGenInfo.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesGenInfo.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesGenInfo.ReturnedFirstName = returnedFirstName;
    entities.ExistingFcrSvesGenInfo.ReturnedMiddleName = returnedMiddleName;
    entities.ExistingFcrSvesGenInfo.ReturnedLastName = returnedLastName;
    entities.ExistingFcrSvesGenInfo.SubmittedFirstName = submittedFirstName;
    entities.ExistingFcrSvesGenInfo.SubmittedMiddleName = submittedMiddleName;
    entities.ExistingFcrSvesGenInfo.SubmittedLastName = submittedLastName;
    entities.ExistingFcrSvesGenInfo.UserField = userField;
    entities.ExistingFcrSvesGenInfo.Populated = true;
  }

  private bool ReadFcrSvesAddress()
  {
    entities.ExistingFcrSvesAddress.Populated = false;

    return Read("ReadFcrSvesAddress",
      (db, command) =>
      {
        db.SetString(
          command, "addressType",
          import.SvesAddressList.Item.FcrSvesAddress.SvesAddressTypeCode);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesAddress.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesAddress.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesAddress.SvesAddressTypeCode =
          db.GetString(reader, 2);
        entities.ExistingFcrSvesAddress.AddressScrubIndicator1 =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesAddress.AddressScrubIndicator2 =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesAddress.AddressScrubIndicator3 =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesAddress.CreatedBy = db.GetString(reader, 6);
        entities.ExistingFcrSvesAddress.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingFcrSvesAddress.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingFcrSvesAddress.State =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesAddress.ZipCode5 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesAddress.ZipCode4 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesAddress.City = db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesAddress.AddressLine1 =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrSvesAddress.AddressLine2 =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesAddress.AddressLine3 =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesAddress.AddressLine4 =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesAddress.Populated = true;
      });
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", import.FcrSvesGenInfo.MemberId);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.SvesMatchType =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.TransmitterStateTerritoryCode =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesGenInfo.SexCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth =
          db.GetNullableDate(reader, 5);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfDeath =
          db.GetNullableDate(reader, 6);
        entities.ExistingFcrSvesGenInfo.SubmittedDateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrSvesGenInfo.Ssn = db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesGenInfo.LocateClosedIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesGenInfo.FipsCountyCode =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesGenInfo.LocateRequestType =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesGenInfo.LocateResponseCode =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesGenInfo.MultipleSsnIndicator =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesGenInfo.MultipleSsn =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesGenInfo.ParticipantType =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState1 =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState2 =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesGenInfo.FamilyViolenceState3 =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesGenInfo.SortStateCode =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrSvesGenInfo.RequestDate =
          db.GetNullableDate(reader, 20);
        entities.ExistingFcrSvesGenInfo.ResponseReceivedDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingFcrSvesGenInfo.CreatedBy = db.GetString(reader, 22);
        entities.ExistingFcrSvesGenInfo.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.ExistingFcrSvesGenInfo.LastUpdatedBy =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrSvesGenInfo.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.ExistingFcrSvesGenInfo.ReturnedFirstName =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrSvesGenInfo.ReturnedMiddleName =
          db.GetNullableString(reader, 27);
        entities.ExistingFcrSvesGenInfo.ReturnedLastName =
          db.GetNullableString(reader, 28);
        entities.ExistingFcrSvesGenInfo.SubmittedFirstName =
          db.GetNullableString(reader, 29);
        entities.ExistingFcrSvesGenInfo.SubmittedMiddleName =
          db.GetNullableString(reader, 30);
        entities.ExistingFcrSvesGenInfo.SubmittedLastName =
          db.GetNullableString(reader, 31);
        entities.ExistingFcrSvesGenInfo.UserField =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private void UpdateFcrSvesAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFcrSvesAddress.Populated);

    var addressScrubIndicator1 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator1 ?? "";
    var addressScrubIndicator2 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator2 ?? "";
    var addressScrubIndicator3 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressScrubIndicator3 ?? "";
    var lastUpdatedBy = import.FcrSvesGenInfo.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.FcrSvesGenInfo.LastUpdatedTimestamp;
    var state = import.SvesAddressList.Item.FcrSvesAddress.State ?? "";
    var zipCode5 = import.SvesAddressList.Item.FcrSvesAddress.ZipCode5 ?? "";
    var zipCode4 = import.SvesAddressList.Item.FcrSvesAddress.ZipCode4 ?? "";
    var zip3 = import.SvesAddressList.Item.FcrSvesAddress.Zip3 ?? "";
    var city = import.SvesAddressList.Item.FcrSvesAddress.City ?? "";
    var addressLine1 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine1 ?? "";
    var addressLine2 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine2 ?? "";
    var addressLine3 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine3 ?? "";
    var addressLine4 =
      import.SvesAddressList.Item.FcrSvesAddress.AddressLine4 ?? "";

    entities.ExistingFcrSvesAddress.Populated = false;
    Update("UpdateFcrSvesAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "addrScrubInd1", addressScrubIndicator1);
        db.SetNullableString(command, "addrScrubInd2", addressScrubIndicator2);
        db.SetNullableString(command, "addrScrubInd3", addressScrubIndicator3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "addressLine3", addressLine3);
        db.SetNullableString(command, "addressLine4", addressLine4);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesAddress.FcgMemberId);
        db.SetString(
          command, "fcgLSRspAgy", entities.ExistingFcrSvesAddress.FcgLSRspAgy);
        db.SetString(
          command, "addressType",
          entities.ExistingFcrSvesAddress.SvesAddressTypeCode);
      });

    entities.ExistingFcrSvesAddress.AddressScrubIndicator1 =
      addressScrubIndicator1;
    entities.ExistingFcrSvesAddress.AddressScrubIndicator2 =
      addressScrubIndicator2;
    entities.ExistingFcrSvesAddress.AddressScrubIndicator3 =
      addressScrubIndicator3;
    entities.ExistingFcrSvesAddress.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesAddress.State = state;
    entities.ExistingFcrSvesAddress.ZipCode5 = zipCode5;
    entities.ExistingFcrSvesAddress.ZipCode4 = zipCode4;
    entities.ExistingFcrSvesAddress.Zip3 = zip3;
    entities.ExistingFcrSvesAddress.City = city;
    entities.ExistingFcrSvesAddress.AddressLine1 = addressLine1;
    entities.ExistingFcrSvesAddress.AddressLine2 = addressLine2;
    entities.ExistingFcrSvesAddress.AddressLine3 = addressLine3;
    entities.ExistingFcrSvesAddress.AddressLine4 = addressLine4;
    entities.ExistingFcrSvesAddress.Populated = true;
  }

  private void UpdateFcrSvesGenInfo()
  {
    var svesMatchType = import.FcrSvesGenInfo.SvesMatchType ?? "";
    var transmitterStateTerritoryCode =
      import.FcrSvesGenInfo.TransmitterStateTerritoryCode ?? "";
    var sexCode = import.FcrSvesGenInfo.SexCode ?? "";
    var returnedDateOfBirth = import.FcrSvesGenInfo.ReturnedDateOfBirth;
    var returnedDateOfDeath = import.FcrSvesGenInfo.ReturnedDateOfDeath;
    var submittedDateOfBirth = import.FcrSvesGenInfo.SubmittedDateOfBirth;
    var ssn = import.FcrSvesGenInfo.Ssn ?? "";
    var locateClosedIndicator = import.FcrSvesGenInfo.LocateClosedIndicator ?? ""
      ;
    var fipsCountyCode = import.FcrSvesGenInfo.FipsCountyCode ?? "";
    var locateRequestType = import.FcrSvesGenInfo.LocateRequestType ?? "";
    var locateResponseCode = import.FcrSvesGenInfo.LocateResponseCode ?? "";
    var multipleSsnIndicator = import.FcrSvesGenInfo.MultipleSsnIndicator ?? "";
    var multipleSsn = import.FcrSvesGenInfo.MultipleSsn ?? "";
    var participantType = import.FcrSvesGenInfo.ParticipantType ?? "";
    var familyViolenceState1 = import.FcrSvesGenInfo.FamilyViolenceState1 ?? "";
    var familyViolenceState2 = import.FcrSvesGenInfo.FamilyViolenceState2 ?? "";
    var familyViolenceState3 = import.FcrSvesGenInfo.FamilyViolenceState3 ?? "";
    var sortStateCode = import.FcrSvesGenInfo.SortStateCode ?? "";
    var requestDate = local.FcrSvesGenInfo.RequestDate;
    var lastUpdatedBy = import.FcrSvesGenInfo.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.FcrSvesGenInfo.LastUpdatedTimestamp;
    var returnedFirstName = import.FcrSvesGenInfo.ReturnedFirstName ?? "";
    var returnedMiddleName = import.FcrSvesGenInfo.ReturnedMiddleName ?? "";
    var returnedLastName = import.FcrSvesGenInfo.ReturnedLastName ?? "";
    var submittedFirstName = import.FcrSvesGenInfo.SubmittedFirstName ?? "";
    var submittedMiddleName = import.FcrSvesGenInfo.SubmittedMiddleName ?? "";
    var submittedLastName = import.FcrSvesGenInfo.SubmittedLastName ?? "";
    var userField = import.FcrSvesGenInfo.UserField ?? "";

    entities.ExistingFcrSvesGenInfo.Populated = false;
    Update("UpdateFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "svesMatchType", svesMatchType);
        db.SetNullableString(
          command, "trnsmtrStTerrCd", transmitterStateTerritoryCode);
        db.SetNullableString(command, "sexCode", sexCode);
        db.SetNullableDate(command, "returnedDob", returnedDateOfBirth);
        db.SetNullableDate(command, "returnedDod", returnedDateOfDeath);
        db.SetNullableDate(command, "submittedDob", submittedDateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "locateClosedInd", locateClosedIndicator);
        db.SetNullableString(command, "fipsCountyCode", fipsCountyCode);
        db.SetNullableString(command, "locateRequestTyp", locateRequestType);
        db.SetNullableString(command, "locateResponseCd", locateResponseCode);
        db.SetNullableString(command, "multipleSsnInd", multipleSsnIndicator);
        db.SetNullableString(command, "multipleSsn", multipleSsn);
        db.SetNullableString(command, "participantType", participantType);
        db.SetNullableString(command, "fvState1", familyViolenceState1);
        db.SetNullableString(command, "fvState2", familyViolenceState2);
        db.SetNullableString(command, "fvState3", familyViolenceState3);
        db.SetNullableString(command, "sortStateCode", sortStateCode);
        db.SetNullableDate(command, "requestDt", requestDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "retdFirstName", returnedFirstName);
        db.SetNullableString(command, "retdMiddleName", returnedMiddleName);
        db.SetNullableString(command, "retdLastName", returnedLastName);
        db.SetNullableString(command, "submitdFirstName", submittedFirstName);
        db.SetNullableString(command, "submtdMiddleName", submittedMiddleName);
        db.SetNullableString(command, "submitdLastName", submittedLastName);
        db.SetNullableString(command, "userField", userField);
        db.SetString(
          command, "memberId", entities.ExistingFcrSvesGenInfo.MemberId);
        db.SetString(
          command, "locSrcRspAgyCd",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
      });

    entities.ExistingFcrSvesGenInfo.SvesMatchType = svesMatchType;
    entities.ExistingFcrSvesGenInfo.TransmitterStateTerritoryCode =
      transmitterStateTerritoryCode;
    entities.ExistingFcrSvesGenInfo.SexCode = sexCode;
    entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth = returnedDateOfBirth;
    entities.ExistingFcrSvesGenInfo.ReturnedDateOfDeath = returnedDateOfDeath;
    entities.ExistingFcrSvesGenInfo.SubmittedDateOfBirth = submittedDateOfBirth;
    entities.ExistingFcrSvesGenInfo.Ssn = ssn;
    entities.ExistingFcrSvesGenInfo.LocateClosedIndicator =
      locateClosedIndicator;
    entities.ExistingFcrSvesGenInfo.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrSvesGenInfo.LocateRequestType = locateRequestType;
    entities.ExistingFcrSvesGenInfo.LocateResponseCode = locateResponseCode;
    entities.ExistingFcrSvesGenInfo.MultipleSsnIndicator = multipleSsnIndicator;
    entities.ExistingFcrSvesGenInfo.MultipleSsn = multipleSsn;
    entities.ExistingFcrSvesGenInfo.ParticipantType = participantType;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState1 = familyViolenceState1;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState2 = familyViolenceState2;
    entities.ExistingFcrSvesGenInfo.FamilyViolenceState3 = familyViolenceState3;
    entities.ExistingFcrSvesGenInfo.SortStateCode = sortStateCode;
    entities.ExistingFcrSvesGenInfo.RequestDate = requestDate;
    entities.ExistingFcrSvesGenInfo.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesGenInfo.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesGenInfo.ReturnedFirstName = returnedFirstName;
    entities.ExistingFcrSvesGenInfo.ReturnedMiddleName = returnedMiddleName;
    entities.ExistingFcrSvesGenInfo.ReturnedLastName = returnedLastName;
    entities.ExistingFcrSvesGenInfo.SubmittedFirstName = submittedFirstName;
    entities.ExistingFcrSvesGenInfo.SubmittedMiddleName = submittedMiddleName;
    entities.ExistingFcrSvesGenInfo.SubmittedLastName = submittedLastName;
    entities.ExistingFcrSvesGenInfo.UserField = userField;
    entities.ExistingFcrSvesGenInfo.Populated = true;
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
    /// <summary>A SvesAddressListGroup group.</summary>
    [Serializable]
    public class SvesAddressListGroup
    {
      /// <summary>
      /// A value of FcrSvesAddress.
      /// </summary>
      [JsonPropertyName("fcrSvesAddress")]
      public FcrSvesAddress FcrSvesAddress
      {
        get => fcrSvesAddress ??= new();
        set => fcrSvesAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private FcrSvesAddress fcrSvesAddress;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// Gets a value of SvesAddressList.
    /// </summary>
    [JsonIgnore]
    public Array<SvesAddressListGroup> SvesAddressList =>
      svesAddressList ??= new(SvesAddressListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SvesAddressList for json serialization.
    /// </summary>
    [JsonPropertyName("svesAddressList")]
    [Computed]
    public IList<SvesAddressListGroup> SvesAddressList_Json
    {
      get => svesAddressList;
      set => SvesAddressList.Assign(value);
    }

    /// <summary>
    /// A value of TotSvesGeninfoCreated.
    /// </summary>
    [JsonPropertyName("totSvesGeninfoCreated")]
    public Common TotSvesGeninfoCreated
    {
      get => totSvesGeninfoCreated ??= new();
      set => totSvesGeninfoCreated = value;
    }

    /// <summary>
    /// A value of TotSvesGeninfoUpdated.
    /// </summary>
    [JsonPropertyName("totSvesGeninfoUpdated")]
    public Common TotSvesGeninfoUpdated
    {
      get => totSvesGeninfoUpdated ??= new();
      set => totSvesGeninfoUpdated = value;
    }

    /// <summary>
    /// A value of TotResaddrCreated.
    /// </summary>
    [JsonPropertyName("totResaddrCreated")]
    public Common TotResaddrCreated
    {
      get => totResaddrCreated ??= new();
      set => totResaddrCreated = value;
    }

    /// <summary>
    /// A value of TotResaddrUpdated.
    /// </summary>
    [JsonPropertyName("totResaddrUpdated")]
    public Common TotResaddrUpdated
    {
      get => totResaddrUpdated ??= new();
      set => totResaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPeraddrCreated.
    /// </summary>
    [JsonPropertyName("totPeraddrCreated")]
    public Common TotPeraddrCreated
    {
      get => totPeraddrCreated ??= new();
      set => totPeraddrCreated = value;
    }

    /// <summary>
    /// A value of TotPeraddrUpdated.
    /// </summary>
    [JsonPropertyName("totPeraddrUpdated")]
    public Common TotPeraddrUpdated
    {
      get => totPeraddrUpdated ??= new();
      set => totPeraddrUpdated = value;
    }

    /// <summary>
    /// A value of TotDisaddrCreated.
    /// </summary>
    [JsonPropertyName("totDisaddrCreated")]
    public Common TotDisaddrCreated
    {
      get => totDisaddrCreated ??= new();
      set => totDisaddrCreated = value;
    }

    /// <summary>
    /// A value of TotDisaddrUpdated.
    /// </summary>
    [JsonPropertyName("totDisaddrUpdated")]
    public Common TotDisaddrUpdated
    {
      get => totDisaddrUpdated ??= new();
      set => totDisaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPayaddrCreated.
    /// </summary>
    [JsonPropertyName("totPayaddrCreated")]
    public Common TotPayaddrCreated
    {
      get => totPayaddrCreated ??= new();
      set => totPayaddrCreated = value;
    }

    /// <summary>
    /// A value of TotPayaddrUpdated.
    /// </summary>
    [JsonPropertyName("totPayaddrUpdated")]
    public Common TotPayaddrUpdated
    {
      get => totPayaddrUpdated ??= new();
      set => totPayaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPriaddrCreated.
    /// </summary>
    [JsonPropertyName("totPriaddrCreated")]
    public Common TotPriaddrCreated
    {
      get => totPriaddrCreated ??= new();
      set => totPriaddrCreated = value;
    }

    /// <summary>
    /// A value of TotPriaddrUpdated.
    /// </summary>
    [JsonPropertyName("totPriaddrUpdated")]
    public Common TotPriaddrUpdated
    {
      get => totPriaddrUpdated ??= new();
      set => totPriaddrUpdated = value;
    }

    private FcrSvesGenInfo fcrSvesGenInfo;
    private Array<SvesAddressListGroup> svesAddressList;
    private Common totSvesGeninfoCreated;
    private Common totSvesGeninfoUpdated;
    private Common totResaddrCreated;
    private Common totResaddrUpdated;
    private Common totPeraddrCreated;
    private Common totPeraddrUpdated;
    private Common totDisaddrCreated;
    private Common totDisaddrUpdated;
    private Common totPayaddrCreated;
    private Common totPayaddrUpdated;
    private Common totPriaddrCreated;
    private Common totPriaddrUpdated;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
    }

    private DateWorkArea null1;
    private CsePerson csePerson;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesAddress fcrSvesAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Exiting.
    /// </summary>
    [JsonPropertyName("exiting")]
    public CsePerson Exiting
    {
      get => exiting ??= new();
      set => exiting = value;
    }

    /// <summary>
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesAddress.
    /// </summary>
    [JsonPropertyName("existingFcrSvesAddress")]
    public FcrSvesAddress ExistingFcrSvesAddress
    {
      get => existingFcrSvesAddress ??= new();
      set => existingFcrSvesAddress = value;
    }

    private CsePerson exiting;
    private FplsLocateRequest existingFplsLocateRequest;
    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
