// Program: OE_JAIL_UPDATE_INCARCERAT_DETLS, ID: 371794506, model: 746.
// Short name: SWE00934
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
/// A program: OE_JAIL_UPDATE_INCARCERAT_DETLS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeJailUpdateIncarceratDetls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_JAIL_UPDATE_INCARCERAT_DETLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeJailUpdateIncarceratDetls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeJailUpdateIncarceratDetls.
  /// </summary>
  public OeJailUpdateIncarceratDetls(IContext context, Import import,
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
    // ---------------------------------------------
    // Date		Author		Request		Reason
    // Jan 1995	Rebecca Grimes  		Initial Development
    // 02/22/95	Sid             		Completion
    // 02/09/10	J. Huss		CQ #15314	Set empty end dates to max date prior to 
    // update
    // ---------------------------------------------
    export.Incarceration.Assign(import.Incarceration);
    export.IncarcerationAddress.Assign(import.IncarcerationAddress);
    UseOeCabSetMnemonics();

    // ---------------------------------------------
    // Read the incarceration record to be updated.
    // ---------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // If the record that is being updated does
    // not contain an end date, verify that a CSE
    // Person record does not already exist without
    // an end date.
    // ---------------------------------------------
    if (Equal(import.Incarceration.EndDate, null))
    {
      foreach(var item in ReadIncarceration2())
      {
        // ---------------------------------------------
        // Cannot UPDATE a new incarceration record if
        // an incarceration record exists without an end
        // date.
        // ---------------------------------------------
        if (Equal(entities.Incarceration.EndDate, null) || Equal
          (entities.Incarceration.EndDate, local.MaxDate.ExpirationDate))
        {
          if (AsChar(entities.Incarceration.Incarcerated) == 'N' && Lt
            (local.Blank.Date, entities.Incarceration.VerifiedDate))
          {
            // ---------------------------------------------------------------------------
            // Per WR# 273, if 'Incarcerated' indicator is equal to N and 
            // Verified_date is greater than '0001-01-01' , the record is ended.
            // User can enter new incarceration record.
            //                                                                
            // Vithal (03/09/2001)
            // ----------------------------------------------------------------------------
          }
          else
          {
            if (AsChar(entities.Incarceration.Type1) == 'T' || AsChar
              (entities.Incarceration.Type1) == 'P')
            {
              export.Common.Flag = "P";
            }
            else
            {
              export.Common.Flag = "J";
            }

            export.Incarceration.Assign(entities.Incarceration);

            if (ReadIncarcerationAddress())
            {
              export.IncarcerationAddress.Assign(entities.IncarcerationAddress);

              if (IsEmpty(entities.IncarcerationAddress.Country))
              {
                export.IncarcerationAddress.Country = "US";
              }
            }

            ExitState = "ALREADY_INCARCERATED";

            return;
          }
        }

        export.Incarceration.EndDate = local.MaxDate.ExpirationDate;
      }
    }

    if (ReadIncarceration1())
    {
      if (Equal(import.Incarceration.VerifiedDate, null))
      {
        export.Incarceration.VerifiedUserId = "";
      }
      else
      {
        export.Incarceration.VerifiedUserId = global.UserId;
      }

      // 02/09/10  J. Huss  Set empty end dates to max date prior to update
      if (Equal(import.Incarceration.EndDate, null))
      {
        export.Incarceration.EndDate = local.MaxDate.ExpirationDate;
      }

      try
      {
        UpdateIncarceration();
        export.Incarceration.Assign(entities.Incarceration);

        if (Equal(entities.Incarceration.EndDate, local.MaxDate.ExpirationDate))
        {
          export.Incarceration.EndDate = null;
        }

        if (ReadIncarcerationAddress())
        {
          if (IsEmpty(import.IncarcerationAddress.Country))
          {
            export.IncarcerationAddress.Country = "US";
          }

          try
          {
            UpdateIncarcerationAddress();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "INCARCERATION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "INCARCERATION_PV";

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
          if (Equal(entities.Incarceration.EndDate, local.MaxDate.ExpirationDate))
            
          {
            export.Incarceration.EndDate = null;
          }

          try
          {
            CreateIncarcerationAddress();
            export.IncarcerationAddress.Assign(entities.IncarcerationAddress);
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "INCARCERATION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "INCARCERATION_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCARCERATION_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCARCERATION_PV";

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
      ExitState = "INCARCERATION_NF";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateIncarcerationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Incarceration.Populated);

    var incIdentifier = entities.Incarceration.Identifier;
    var cspNumber = entities.Incarceration.CspNumber;
    var effectiveDate = export.IncarcerationAddress.EffectiveDate;
    var street1 = export.IncarcerationAddress.Street1 ?? "";
    var street2 = export.IncarcerationAddress.Street2 ?? "";
    var city = export.IncarcerationAddress.City ?? "";
    var state = export.IncarcerationAddress.State ?? "";
    var province = export.IncarcerationAddress.Province ?? "";
    var postalCode = export.IncarcerationAddress.PostalCode ?? "";
    var zipCode5 = export.IncarcerationAddress.ZipCode5 ?? "";
    var zipCode4 = export.IncarcerationAddress.ZipCode4 ?? "";
    var zip3 = export.IncarcerationAddress.Zip3 ?? "";
    var country = export.IncarcerationAddress.Country ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.IncarcerationAddress.Populated = false;
    Update("CreateIncarcerationAddress",
      (db, command) =>
      {
        db.SetInt32(command, "incIdentifier", incIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.IncarcerationAddress.IncIdentifier = incIdentifier;
    entities.IncarcerationAddress.CspNumber = cspNumber;
    entities.IncarcerationAddress.EffectiveDate = effectiveDate;
    entities.IncarcerationAddress.Street1 = street1;
    entities.IncarcerationAddress.Street2 = street2;
    entities.IncarcerationAddress.City = city;
    entities.IncarcerationAddress.State = state;
    entities.IncarcerationAddress.Province = province;
    entities.IncarcerationAddress.PostalCode = postalCode;
    entities.IncarcerationAddress.ZipCode5 = zipCode5;
    entities.IncarcerationAddress.ZipCode4 = zipCode4;
    entities.IncarcerationAddress.Zip3 = zip3;
    entities.IncarcerationAddress.Country = country;
    entities.IncarcerationAddress.CreatedBy = createdBy;
    entities.IncarcerationAddress.CreatedTimestamp = createdTimestamp;
    entities.IncarcerationAddress.LastUpdatedBy = createdBy;
    entities.IncarcerationAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.IncarcerationAddress.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadIncarceration1()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Incarceration.Identifier);
        db.SetNullableString(command, "type", import.Incarceration.Type1 ?? "");
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedUserId = db.GetNullableString(reader, 2);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.InmateNumber = db.GetNullableString(reader, 4);
        entities.Incarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 5);
        entities.Incarceration.WorkReleaseInd = db.GetNullableString(reader, 6);
        entities.Incarceration.ConditionsForRelease =
          db.GetNullableString(reader, 7);
        entities.Incarceration.ParoleOfficersTitle =
          db.GetNullableString(reader, 8);
        entities.Incarceration.Phone = db.GetNullableInt32(reader, 9);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 10);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 11);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 12);
        entities.Incarceration.InstitutionName =
          db.GetNullableString(reader, 13);
        entities.Incarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 14);
        entities.Incarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 15);
        entities.Incarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 16);
        entities.Incarceration.CreatedBy = db.GetString(reader, 17);
        entities.Incarceration.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.Incarceration.LastUpdatedBy = db.GetString(reader, 19);
        entities.Incarceration.LastUpdatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.Incarceration.PhoneExt = db.GetNullableString(reader, 21);
        entities.Incarceration.PhoneAreaCode = db.GetNullableInt32(reader, 22);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 23);
        entities.Incarceration.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncarceration2()
  {
    entities.Incarceration.Populated = false;

    return ReadEach("ReadIncarceration2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "identifier", import.Incarceration.Identifier);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedUserId = db.GetNullableString(reader, 2);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.InmateNumber = db.GetNullableString(reader, 4);
        entities.Incarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 5);
        entities.Incarceration.WorkReleaseInd = db.GetNullableString(reader, 6);
        entities.Incarceration.ConditionsForRelease =
          db.GetNullableString(reader, 7);
        entities.Incarceration.ParoleOfficersTitle =
          db.GetNullableString(reader, 8);
        entities.Incarceration.Phone = db.GetNullableInt32(reader, 9);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 10);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 11);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 12);
        entities.Incarceration.InstitutionName =
          db.GetNullableString(reader, 13);
        entities.Incarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 14);
        entities.Incarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 15);
        entities.Incarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 16);
        entities.Incarceration.CreatedBy = db.GetString(reader, 17);
        entities.Incarceration.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.Incarceration.LastUpdatedBy = db.GetString(reader, 19);
        entities.Incarceration.LastUpdatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.Incarceration.PhoneExt = db.GetNullableString(reader, 21);
        entities.Incarceration.PhoneAreaCode = db.GetNullableInt32(reader, 22);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 23);
        entities.Incarceration.Populated = true;

        return true;
      });
  }

  private bool ReadIncarcerationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Incarceration.Populated);
    entities.IncarcerationAddress.Populated = false;

    return Read("ReadIncarcerationAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "incIdentifier", entities.Incarceration.Identifier);
          
        db.SetString(command, "cspNumber", entities.Incarceration.CspNumber);
      },
      (db, reader) =>
      {
        entities.IncarcerationAddress.IncIdentifier = db.GetInt32(reader, 0);
        entities.IncarcerationAddress.CspNumber = db.GetString(reader, 1);
        entities.IncarcerationAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.IncarcerationAddress.Street1 = db.GetNullableString(reader, 3);
        entities.IncarcerationAddress.Street2 = db.GetNullableString(reader, 4);
        entities.IncarcerationAddress.City = db.GetNullableString(reader, 5);
        entities.IncarcerationAddress.State = db.GetNullableString(reader, 6);
        entities.IncarcerationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.IncarcerationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.IncarcerationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.IncarcerationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.IncarcerationAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.IncarcerationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.IncarcerationAddress.CreatedBy = db.GetString(reader, 13);
        entities.IncarcerationAddress.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.IncarcerationAddress.LastUpdatedBy = db.GetString(reader, 15);
        entities.IncarcerationAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.IncarcerationAddress.Populated = true;
      });
  }

  private void UpdateIncarceration()
  {
    System.Diagnostics.Debug.Assert(entities.Incarceration.Populated);

    var verifiedUserId = export.Incarceration.VerifiedUserId ?? "";
    var verifiedDate = export.Incarceration.VerifiedDate;
    var inmateNumber = export.Incarceration.InmateNumber ?? "";
    var paroleEligibilityDate = export.Incarceration.ParoleEligibilityDate;
    var workReleaseInd = export.Incarceration.WorkReleaseInd ?? "";
    var conditionsForRelease = export.Incarceration.ConditionsForRelease ?? "";
    var paroleOfficersTitle = export.Incarceration.ParoleOfficersTitle ?? "";
    var phone = export.Incarceration.Phone.GetValueOrDefault();
    var endDate = export.Incarceration.EndDate;
    var startDate = export.Incarceration.StartDate;
    var type1 = export.Incarceration.Type1 ?? "";
    var institutionName = export.Incarceration.InstitutionName ?? "";
    var paroleOfficerLastName = export.Incarceration.ParoleOfficerLastName ?? ""
      ;
    var paroleOfficerFirstName =
      export.Incarceration.ParoleOfficerFirstName ?? "";
    var paroleOfficerMiddleInitial =
      export.Incarceration.ParoleOfficerMiddleInitial ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var phoneExt = export.Incarceration.PhoneExt ?? "";
    var phoneAreaCode = export.Incarceration.PhoneAreaCode.GetValueOrDefault();
    var incarcerated = export.Incarceration.Incarcerated ?? "";

    entities.Incarceration.Populated = false;
    Update("UpdateIncarceration",
      (db, command) =>
      {
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "inmateNumber", inmateNumber);
        db.SetNullableDate(command, "paroleEligDate", paroleEligibilityDate);
        db.SetNullableString(command, "workReleaseInd", workReleaseInd);
        db.SetNullableString(command, "condForRelease", conditionsForRelease);
        db.SetNullableString(command, "poffrTitle", paroleOfficersTitle);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "institutionName", institutionName);
        db.SetNullableString(command, "poffrLastName", paroleOfficerLastName);
        db.SetNullableString(command, "poffrFirstName", paroleOfficerFirstName);
        db.SetNullableString(command, "poffrMi", paroleOfficerMiddleInitial);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableInt32(command, "phoneArea", phoneAreaCode);
        db.SetNullableString(command, "incarcerated", incarcerated);
        db.SetString(command, "cspNumber", entities.Incarceration.CspNumber);
        db.SetInt32(command, "identifier", entities.Incarceration.Identifier);
      });

    entities.Incarceration.VerifiedUserId = verifiedUserId;
    entities.Incarceration.VerifiedDate = verifiedDate;
    entities.Incarceration.InmateNumber = inmateNumber;
    entities.Incarceration.ParoleEligibilityDate = paroleEligibilityDate;
    entities.Incarceration.WorkReleaseInd = workReleaseInd;
    entities.Incarceration.ConditionsForRelease = conditionsForRelease;
    entities.Incarceration.ParoleOfficersTitle = paroleOfficersTitle;
    entities.Incarceration.Phone = phone;
    entities.Incarceration.EndDate = endDate;
    entities.Incarceration.StartDate = startDate;
    entities.Incarceration.Type1 = type1;
    entities.Incarceration.InstitutionName = institutionName;
    entities.Incarceration.ParoleOfficerLastName = paroleOfficerLastName;
    entities.Incarceration.ParoleOfficerFirstName = paroleOfficerFirstName;
    entities.Incarceration.ParoleOfficerMiddleInitial =
      paroleOfficerMiddleInitial;
    entities.Incarceration.LastUpdatedBy = lastUpdatedBy;
    entities.Incarceration.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Incarceration.PhoneExt = phoneExt;
    entities.Incarceration.PhoneAreaCode = phoneAreaCode;
    entities.Incarceration.Incarcerated = incarcerated;
    entities.Incarceration.Populated = true;
  }

  private void UpdateIncarcerationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncarcerationAddress.Populated);

    var effectiveDate = export.IncarcerationAddress.EffectiveDate;
    var street1 = export.IncarcerationAddress.Street1 ?? "";
    var street2 = export.IncarcerationAddress.Street2 ?? "";
    var city = export.IncarcerationAddress.City ?? "";
    var state = export.IncarcerationAddress.State ?? "";
    var province = export.IncarcerationAddress.Province ?? "";
    var postalCode = export.IncarcerationAddress.PostalCode ?? "";
    var zipCode5 = export.IncarcerationAddress.ZipCode5 ?? "";
    var zipCode4 = export.IncarcerationAddress.ZipCode4 ?? "";
    var zip3 = export.IncarcerationAddress.Zip3 ?? "";
    var country = export.IncarcerationAddress.Country ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IncarcerationAddress.Populated = false;
    Update("UpdateIncarcerationAddress",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate1", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "incIdentifier",
          entities.IncarcerationAddress.IncIdentifier);
        db.SetString(
          command, "cspNumber", entities.IncarcerationAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate2",
          entities.IncarcerationAddress.EffectiveDate.GetValueOrDefault());
      });

    entities.IncarcerationAddress.EffectiveDate = effectiveDate;
    entities.IncarcerationAddress.Street1 = street1;
    entities.IncarcerationAddress.Street2 = street2;
    entities.IncarcerationAddress.City = city;
    entities.IncarcerationAddress.State = state;
    entities.IncarcerationAddress.Province = province;
    entities.IncarcerationAddress.PostalCode = postalCode;
    entities.IncarcerationAddress.ZipCode5 = zipCode5;
    entities.IncarcerationAddress.ZipCode4 = zipCode4;
    entities.IncarcerationAddress.Zip3 = zip3;
    entities.IncarcerationAddress.Country = country;
    entities.IncarcerationAddress.LastUpdatedBy = lastUpdatedBy;
    entities.IncarcerationAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncarcerationAddress.Populated = true;
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
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    private IncarcerationAddress incarcerationAddress;
    private CsePerson csePerson;
    private Incarceration incarceration;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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

    private IncarcerationAddress incarcerationAddress;
    private Incarceration incarceration;
    private Common common;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private Code maxDate;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    private IncarcerationAddress incarcerationAddress;
    private CsePerson csePerson;
    private Incarceration incarceration;
  }
#endregion
}
