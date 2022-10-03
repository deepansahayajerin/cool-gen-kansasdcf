// Program: OE_UPDATE_LOCATE_RESPONSES, ID: 374437026, model: 746.
// Short name: SWE02612
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_UPDATE_LOCATE_RESPONSES.
/// </summary>
[Serializable]
public partial class OeUpdateLocateResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_LOCATE_RESPONSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateLocateResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateLocateResponses.
  /// </summary>
  public OeUpdateLocateResponses(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------
    // 06/27/00	SWSRPDP
    // Initial Developement
    // CHANGE LOG:
    // 02/2001	SWSRPRM
    // WR # 271 => License suspension
    // ------------------------------------------------------
    // 12/30/2008  Arun Mathias CQ#7432 Populate the License
    //             source name and Comment out the DMV logic.
    // ------------------------------------------------------
    export.LocateRequest.Assign(import.LocateRequest);
    local.Agency.Cdvalue = Substring(import.LocateRequest.AgencyNumber, 2, 4);

    // *** CQ7432 Changes Start Here ***
    // *** Set the value
    local.Current.Date = Now().Date;
    local.SequenceNbrFromFile.Text2 =
      NumberToString(import.LocateRequest.SequenceNumber, 14, 2);

    // *** CQ7432 Changes End   Here ***
    // *** CQ7432 Changes Start Here ***
    // Comment Starts Here
    // Below DMV code is commented out because of the new DMV process written.
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // * IS the License number greater than spaces?
    // * *   IF NO DMV License - JUST BYPASS - NOT an ERROR
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Comment Ends Here
    // *** CQ7432 Changes End   Here ***
    if (ReadLocateRequest2())
    {
      // -----------------------------------------
      // Determine name of SUB agency if available
      // -----------------------------------------
      // *** CQ#7432 Changes Begin Here ***
      // *** Changed the condition with hardcorded value to the file value and 
      // also, added to check the effective and end date
      if (ReadCodeCodeValue2())
      {
        // *** CQ#7432 Changes Ends Here ***
        local.SourceAgencyLength.Count =
          Find(entities.CodeValue.Description, ";") + 2;
        local.TotalLength.Count = Length(entities.CodeValue.Description);

        if (local.SourceAgencyLength.Count == 2)
        {
          // -----------------------------------------------------------------
          // If this member has a professional license and CSE is waiting
          // for a match from a participating agency, send the License
          // Suspension document LICNSUSP
          // -----------------------------------------------------------------
          if (!IsEmpty(import.LocateRequest.LicenseNumber) || Lt
            (local.NullDate.Date, import.LocateRequest.LicenseIssuedDate))
          {
            if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'W')
            {
              local.LocateRequest.LicenseSuspensionInd = "Y";
              local.LicSuspDoc.Flag = "Y";
            }
            else if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y')
            {
              local.LocateRequest.LicenseSuspensionInd = "Y";
            }
            else
            {
              local.LocateRequest.LicenseSuspensionInd = "N";
            }
          }
          else
          {
            local.LocateRequest.LicenseSuspensionInd = "N";
          }

          // -----------------------------------------------------
          // In the instance we are dealing with an agency with no
          // mulitple sources, the agency name will be spaces
          // -----------------------------------------------------
          // *** CQ#7432 Changes Start Here ***
          local.LocateRequest.LicenseSourceName =
            Substring(entities.CodeValue.Description, 1,
            Length(TrimEnd(entities.CodeValue.Description)));

          // *** CQ#7432 Changes End   Here ***
          goto Read;
        }
        else
        {
          local.LocateRequest.LicenseSourceName =
            Substring(entities.CodeValue.Description,
            local.SourceAgencyLength.Count,
            Length(TrimEnd(entities.CodeValue.Description)) - local
            .SourceAgencyLength.Count + 1);
        }

        // -----------------------------------------------------------------
        // If this member has a professional license and CSE is waiting
        // for a match from a participating agency, send the License
        // Suspension document LICNSUSP
        // -----------------------------------------------------------------
        if (!IsEmpty(import.LocateRequest.LicenseNumber) || Lt
          (local.NullDate.Date, import.LocateRequest.LicenseIssuedDate))
        {
          if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'W')
          {
            local.LocateRequest.LicenseSuspensionInd = "Y";
            local.LicSuspDoc.Flag = "Y";
          }
          else if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y')
          {
            local.LocateRequest.LicenseSuspensionInd = "Y";
          }
          else
          {
            local.LocateRequest.LicenseSuspensionInd = "N";
          }
        }
        else
        {
          local.LocateRequest.LicenseSuspensionInd = "N";
        }

        // -------------------------
        // Determine sequence number
        // -------------------------
        local.TextSequenceNumber.Text2 =
          Substring(entities.CodeValue.Cdvalue, 5, 2);
        local.LocateRequest.SequenceNumber =
          (int)StringToNumber(local.TextSequenceNumber.Text2);
      }
      else
      {
        ExitState = "CODE_VALUE_NF";

        return;
      }

Read:

      if (AsChar(local.LicSuspDoc.Flag) == 'Y')
      {
        // ----------------------------------------------------------------
        // For document processing, an address must exist in order to
        // send the document.  The check will be made here to
        // determine if ANY address does exist.  If one does, continue
        // to document processing where the valid address is determined.
        // ----------------------------------------------------------------
        if (ReadCsePersonAddress())
        {
          // -------------------------
          // Previous values will hold
          // -------------------------
        }
        else if (AsChar(local.LocateRequest.LicenseSuspensionInd) == 'Y')
        {
          local.LocateRequest.LicenseSuspensionInd = "W";
        }
        else
        {
          // -------------------------
          // Previous values will hold
          // -------------------------
        }
      }

      try
      {
        UpdateLocateRequest();
        export.LocateRequest.Assign(entities.LocateRequest);

        if (AsChar(local.LicSuspDoc.Flag) == 'Y' && AsChar
          (local.LocateRequest.LicenseSuspensionInd) == 'Y')
        {
          local.Document.Name = "LICNSUSP";
          local.SpDocKey.KeyPerson = entities.LocateRequest.CsePersonNumber;
          local.SpDocKey.KeyLocateRequestAgency =
            entities.LocateRequest.AgencyNumber;
          local.SpDocKey.KeyLocateRequestSource =
            entities.LocateRequest.SequenceNumber;
          UseSpCreateDocumentInfrastruct();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LOCATE_REQUEST_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LOCATE_REQUEST_PV";

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
      // -------------------------------------------------------------
      // For locate requests determine if the Agency contains multiple
      // sources within it.   If yes, create the additional locate
      // request(s).  Because the first sequence number has already
      // been handled, bypass it.
      // -------------------------------------------------------------
      local.SaveSequence.Text2 =
        NumberToString(import.LocateRequest.SequenceNumber, 14, 2);

      // *** CQ#7432 Changes Begin Here ***
      // *** Added the effective and end date checks to the READ EACH statement.
      // Also, added the code value found flag
      local.CodeValueFoundFlag.Flag = "N";

      if (ReadCodeCodeValue1())
      {
        local.CodeValueFoundFlag.Flag = "Y";

        // *** CQ#7432 Changes End Here ***
        // --------------------------------------
        // Determine status of license suspension
        // --------------------------------------
        if (AsChar(import.ProcessAllLocateReq.Flag) == 'Y')
        {
          if (ReadLocateRequest1())
          {
            if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y' || AsChar
              (entities.LocateRequest.LicenseSuspensionInd) == 'W')
            {
              if (!IsEmpty(import.LocateRequest.LicenseNumber) || Lt
                (local.NullDate.Date, import.LocateRequest.LicenseIssuedDate))
              {
                local.LicSuspDoc.Flag = "Y";
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
          }
        }
        else if (!IsEmpty(import.LocateRequest.LicenseNumber) || Lt
          (local.NullDate.Date, import.LocateRequest.LicenseIssuedDate))
        {
          local.LicSuspDoc.Flag = "Y";
          local.LocateRequest.LicenseSuspensionInd = "Y";
        }
        else
        {
          local.LocateRequest.LicenseSuspensionInd = "W";
        }

        // ----------------------------
        // Determine name of SUB agency
        // ----------------------------
        local.TotalLength.Count = Length(entities.CodeValue.Description);
        local.SourceAgencyLength.Count =
          Find(entities.CodeValue.Description, ";") + 2;

        // *** CQ#7432 Changes Begin Here ***
        if (local.SourceAgencyLength.Count == 2)
        {
          local.LocateRequest.LicenseSourceName =
            Substring(entities.CodeValue.Description, 1,
            Length(TrimEnd(entities.CodeValue.Description)));
        }
        else
        {
          // *** CQ#7432 Changes End Here ***
          local.LocateRequest.LicenseSourceName =
            Substring(entities.CodeValue.Description,
            local.SourceAgencyLength.Count,
            Length(TrimEnd(entities.CodeValue.Description)) - local
            .SourceAgencyLength.Count + 1);
        }

        if (AsChar(local.LicSuspDoc.Flag) == 'Y')
        {
          // ----------------------------------------------------------------
          // For document processing, an address must exist in order to
          // send the document.  The check will be made here to
          // determine if ANY address does exist.  If one does, continue
          // to document processing where the valid address is determined.
          // ----------------------------------------------------------------
          if (ReadCsePersonAddress())
          {
            // -------------------------
            // Previous values will hold
            // -------------------------
          }
          else if (AsChar(local.LocateRequest.LicenseSuspensionInd) == 'Y')
          {
            local.LocateRequest.LicenseSuspensionInd = "W";
          }
          else
          {
            // -------------------------
            // Previous values will hold
            // -------------------------
          }
        }

        try
        {
          CreateLocateRequest();
          export.LocateRequest.Assign(entities.LocateRequest);

          // ------------------------------------------------------------
          // For license suspension, the participating agency(ies) will be
          // sending SRS information they possess concerning this
          // member => we cannot create it
          // ------------------------------------------------------------
          // --------------------------------------------------------
          // Member will receive a suspension document sent for every
          // professional license possessed
          // --------------------------------------------------------
          if (AsChar(local.LicSuspDoc.Flag) == 'Y' && AsChar
            (local.LocateRequest.LicenseSuspensionInd) == 'Y')
          {
            local.Document.Name = "LICNSUSP";
            local.SpDocKey.KeyPerson = entities.LocateRequest.CsePersonNumber;
            local.SpDocKey.KeyLocateRequestAgency =
              entities.LocateRequest.AgencyNumber;
            local.SpDocKey.KeyLocateRequestSource =
              entities.LocateRequest.SequenceNumber;
            UseSpCreateDocumentInfrastruct();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LOCATE_REQUEST_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LOCATE_REQUEST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // *** CQ#7432 Changes Start Here ***
      if (AsChar(local.CodeValueFoundFlag.Flag) == 'N')
      {
        ExitState = "CODE_VALUE_NF";
      }

      // *** CQ#7432 Changes End   Here ***
    }
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyLocateRequestAgency = source.KeyLocateRequestAgency;
    target.KeyLocateRequestSource = source.KeyLocateRequestSource;
    target.KeyPerson = source.KeyPerson;
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Assign(local.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void CreateLocateRequest()
  {
    var socialSecurityNumber = import.LocateRequest.SocialSecurityNumber ?? "";
    var dateOfBirth = import.LocateRequest.DateOfBirth;
    var csePersonNumber = import.LocateRequest.CsePersonNumber;
    var requestDate = import.LocateRequest.RequestDate;
    var responseDate = import.LocateRequest.ResponseDate;
    var licenseIssuedDate = import.LocateRequest.LicenseIssuedDate;
    var licenseExpirationDate = import.LocateRequest.LicenseExpirationDate;
    var licenseSuspendedDate = import.LocateRequest.LicenseSuspendedDate;
    var licenseNumber = import.LocateRequest.LicenseNumber ?? "";
    var agencyNumber = import.LocateRequest.AgencyNumber;
    var sequenceNumber = import.LocateRequest.SequenceNumber;
    var licenseSourceName = local.LocateRequest.LicenseSourceName ?? "";
    var street1 = import.LocateRequest.Street1 ?? "";
    var addressType = import.LocateRequest.AddressType ?? "";
    var street2 = import.LocateRequest.Street2 ?? "";
    var street3 = import.LocateRequest.Street3 ?? "";
    var street4 = import.LocateRequest.Street4 ?? "";
    var city = import.LocateRequest.City ?? "";
    var state = import.LocateRequest.State ?? "";
    var zipCode5 = import.LocateRequest.ZipCode5 ?? "";
    var zipCode4 = import.LocateRequest.ZipCode4 ?? "";
    var zipCode3 = import.LocateRequest.ZipCode3 ?? "";
    var province = import.LocateRequest.Province ?? "";
    var postalCode = import.LocateRequest.PostalCode ?? "";
    var country = import.LocateRequest.Country ?? "";
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
        db.SetNullableDate(command, "requestDate", requestDate);
        db.SetNullableDate(command, "responseDate", responseDate);
        db.SetNullableDate(command, "licenseIssuedDt", licenseIssuedDate);
        db.SetNullableDate(command, "licenseExpDate", licenseExpirationDate);
        db.SetNullableDate(command, "licenseSuspDate", licenseSuspendedDate);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetString(command, "agencyNumber", agencyNumber);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "licSourceName", licenseSourceName);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "addressType", addressType);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetNullableString(command, "licSuspensionInd", licenseSuspensionInd);
      });

    entities.LocateRequest.SocialSecurityNumber = socialSecurityNumber;
    entities.LocateRequest.DateOfBirth = dateOfBirth;
    entities.LocateRequest.CsePersonNumber = csePersonNumber;
    entities.LocateRequest.RequestDate = requestDate;
    entities.LocateRequest.ResponseDate = responseDate;
    entities.LocateRequest.LicenseIssuedDate = licenseIssuedDate;
    entities.LocateRequest.LicenseExpirationDate = licenseExpirationDate;
    entities.LocateRequest.LicenseSuspendedDate = licenseSuspendedDate;
    entities.LocateRequest.LicenseNumber = licenseNumber;
    entities.LocateRequest.AgencyNumber = agencyNumber;
    entities.LocateRequest.SequenceNumber = sequenceNumber;
    entities.LocateRequest.LicenseSourceName = licenseSourceName;
    entities.LocateRequest.Street1 = street1;
    entities.LocateRequest.AddressType = addressType;
    entities.LocateRequest.Street2 = street2;
    entities.LocateRequest.Street3 = street3;
    entities.LocateRequest.Street4 = street4;
    entities.LocateRequest.City = city;
    entities.LocateRequest.State = state;
    entities.LocateRequest.ZipCode5 = zipCode5;
    entities.LocateRequest.ZipCode4 = zipCode4;
    entities.LocateRequest.ZipCode3 = zipCode3;
    entities.LocateRequest.Province = province;
    entities.LocateRequest.PostalCode = postalCode;
    entities.LocateRequest.Country = country;
    entities.LocateRequest.CreatedTimestamp = createdTimestamp;
    entities.LocateRequest.CreatedBy = createdBy;
    entities.LocateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.LocateRequest.LastUpdatedBy = createdBy;
    entities.LocateRequest.LicenseSuspensionInd = licenseSuspensionInd;
    entities.LocateRequest.Populated = true;
  }

  private bool ReadCodeCodeValue1()
  {
    entities.Code.Populated = false;
    entities.CodeValue.Populated = false;

    return Read("ReadCodeCodeValue1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cdvalue", local.Agency.Cdvalue);
        db.SetString(command, "text2", local.SaveSequence.Text2);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Description = db.GetString(reader, 8);
        entities.Code.Populated = true;
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeCodeValue2()
  {
    entities.Code.Populated = false;
    entities.CodeValue.Populated = false;

    return Read("ReadCodeCodeValue2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cdvalue", local.Agency.Cdvalue);
        db.SetString(command, "text2", local.SequenceNbrFromFile.Text2);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Description = db.GetString(reader, 8);
        entities.Code.Populated = true;
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", import.LocateRequest.CsePersonNumber);
          
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private bool ReadLocateRequest1()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest1",
      (db, command) =>
      {
        db.
          SetString(command, "agencyNumber", import.LocateRequest.AgencyNumber);
          
        db.SetString(
          command, "csePersonNumber", import.LocateRequest.CsePersonNumber);
        db.SetInt32(
          command, "sequenceNumber", import.LocateRequest.SequenceNumber);
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

  private bool ReadLocateRequest2()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest2",
      (db, command) =>
      {
        db.SetString(
          command, "csePersonNumber", import.LocateRequest.CsePersonNumber);
        db.
          SetString(command, "agencyNumber", import.LocateRequest.AgencyNumber);
          
        db.SetInt32(
          command, "sequenceNumber", import.LocateRequest.SequenceNumber);
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
    var socialSecurityNumber = import.LocateRequest.SocialSecurityNumber ?? "";
    var dateOfBirth = import.LocateRequest.DateOfBirth;
    var requestDate = import.LocateRequest.RequestDate;
    var responseDate = import.LocateRequest.ResponseDate;
    var licenseIssuedDate = import.LocateRequest.LicenseIssuedDate;
    var licenseExpirationDate = import.LocateRequest.LicenseExpirationDate;
    var licenseSuspendedDate = import.LocateRequest.LicenseSuspendedDate;
    var licenseNumber = import.LocateRequest.LicenseNumber ?? "";
    var licenseSourceName = local.LocateRequest.LicenseSourceName ?? "";
    var street1 = import.LocateRequest.Street1 ?? "";
    var addressType = import.LocateRequest.AddressType ?? "";
    var street2 = import.LocateRequest.Street2 ?? "";
    var street3 = import.LocateRequest.Street3 ?? "";
    var street4 = import.LocateRequest.Street4 ?? "";
    var city = import.LocateRequest.City ?? "";
    var state = import.LocateRequest.State ?? "";
    var zipCode5 = import.LocateRequest.ZipCode5 ?? "";
    var zipCode4 = import.LocateRequest.ZipCode4 ?? "";
    var zipCode3 = import.LocateRequest.ZipCode3 ?? "";
    var province = import.LocateRequest.Province ?? "";
    var postalCode = import.LocateRequest.PostalCode ?? "";
    var country = import.LocateRequest.Country ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var licenseSuspensionInd = local.LocateRequest.LicenseSuspensionInd ?? "";

    entities.LocateRequest.Populated = false;
    Update("UpdateLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "ssnNumber", socialSecurityNumber);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableDate(command, "requestDate", requestDate);
        db.SetNullableDate(command, "responseDate", responseDate);
        db.SetNullableDate(command, "licenseIssuedDt", licenseIssuedDate);
        db.SetNullableDate(command, "licenseExpDate", licenseExpirationDate);
        db.SetNullableDate(command, "licenseSuspDate", licenseSuspendedDate);
        db.SetNullableString(command, "licenseNumber", licenseNumber);
        db.SetNullableString(command, "licSourceName", licenseSourceName);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "addressType", addressType);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "licSuspensionInd", licenseSuspensionInd);
        db.SetString(
          command, "csePersonNumber", entities.LocateRequest.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", entities.LocateRequest.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", entities.LocateRequest.SequenceNumber);
      });

    entities.LocateRequest.SocialSecurityNumber = socialSecurityNumber;
    entities.LocateRequest.DateOfBirth = dateOfBirth;
    entities.LocateRequest.RequestDate = requestDate;
    entities.LocateRequest.ResponseDate = responseDate;
    entities.LocateRequest.LicenseIssuedDate = licenseIssuedDate;
    entities.LocateRequest.LicenseExpirationDate = licenseExpirationDate;
    entities.LocateRequest.LicenseSuspendedDate = licenseSuspendedDate;
    entities.LocateRequest.LicenseNumber = licenseNumber;
    entities.LocateRequest.LicenseSourceName = licenseSourceName;
    entities.LocateRequest.Street1 = street1;
    entities.LocateRequest.AddressType = addressType;
    entities.LocateRequest.Street2 = street2;
    entities.LocateRequest.Street3 = street3;
    entities.LocateRequest.Street4 = street4;
    entities.LocateRequest.City = city;
    entities.LocateRequest.State = state;
    entities.LocateRequest.ZipCode5 = zipCode5;
    entities.LocateRequest.ZipCode4 = zipCode4;
    entities.LocateRequest.ZipCode3 = zipCode3;
    entities.LocateRequest.Province = province;
    entities.LocateRequest.PostalCode = postalCode;
    entities.LocateRequest.Country = country;
    entities.LocateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LocateRequest.LastUpdatedBy = lastUpdatedBy;
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
    /// A value of ProcessAllLocateReq.
    /// </summary>
    [JsonPropertyName("processAllLocateReq")]
    public Common ProcessAllLocateReq
    {
      get => processAllLocateReq ??= new();
      set => processAllLocateReq = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    private Common processAllLocateReq;
    private LocateRequest locateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LicSuspensionDocument.
    /// </summary>
    [JsonPropertyName("licSuspensionDocument")]
    public LocateRequest LicSuspensionDocument
    {
      get => licSuspensionDocument ??= new();
      set => licSuspensionDocument = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    private LocateRequest licSuspensionDocument;
    private LocateRequest locateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LicSuspDoc.
    /// </summary>
    [JsonPropertyName("licSuspDoc")]
    public Common LicSuspDoc
    {
      get => licSuspDoc ??= new();
      set => licSuspDoc = value;
    }

    /// <summary>
    /// A value of Id.
    /// </summary>
    [JsonPropertyName("id")]
    public CsePersonLicense Id
    {
      get => id ??= new();
      set => id = value;
    }

    /// <summary>
    /// A value of CreateLicense.
    /// </summary>
    [JsonPropertyName("createLicense")]
    public Common CreateLicense
    {
      get => createLicense ??= new();
      set => createLicense = value;
    }

    /// <summary>
    /// A value of SaveSequence.
    /// </summary>
    [JsonPropertyName("saveSequence")]
    public TextWorkArea SaveSequence
    {
      get => saveSequence ??= new();
      set => saveSequence = value;
    }

    /// <summary>
    /// A value of SourceAgencyLength.
    /// </summary>
    [JsonPropertyName("sourceAgencyLength")]
    public Common SourceAgencyLength
    {
      get => sourceAgencyLength ??= new();
      set => sourceAgencyLength = value;
    }

    /// <summary>
    /// A value of TotalLength.
    /// </summary>
    [JsonPropertyName("totalLength")]
    public Common TotalLength
    {
      get => totalLength ??= new();
      set => totalLength = value;
    }

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
    /// A value of TextSequenceNumber.
    /// </summary>
    [JsonPropertyName("textSequenceNumber")]
    public TextWorkArea TextSequenceNumber
    {
      get => textSequenceNumber ??= new();
      set => textSequenceNumber = value;
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public CsePersonLicense Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Infrastructure Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of SequenceNbrFromFile.
    /// </summary>
    [JsonPropertyName("sequenceNbrFromFile")]
    public TextWorkArea SequenceNbrFromFile
    {
      get => sequenceNbrFromFile ??= new();
      set => sequenceNbrFromFile = value;
    }

    /// <summary>
    /// A value of CodeValueFoundFlag.
    /// </summary>
    [JsonPropertyName("codeValueFoundFlag")]
    public Common CodeValueFoundFlag
    {
      get => codeValueFoundFlag ??= new();
      set => codeValueFoundFlag = value;
    }

    private Common licSuspDoc;
    private CsePersonLicense id;
    private Common createLicense;
    private TextWorkArea saveSequence;
    private Common sourceAgencyLength;
    private Common totalLength;
    private LocateRequest locateRequest;
    private TextWorkArea textSequenceNumber;
    private CodeValue agency;
    private CsePersonLicense update;
    private Infrastructure pass;
    private DateWorkArea current;
    private CsePersonAddress csePersonAddress;
    private DateWorkArea nullDate;
    private Document document;
    private SpDocKey spDocKey;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private TextWorkArea sequenceNbrFromFile;
    private Common codeValueFoundFlag;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CsePerson Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CsePersonAddress csePersonAddress;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
    private LocateRequest locateRequest;
    private Code code;
    private CodeValue codeValue;
    private CsePerson existing;
  }
#endregion
}
