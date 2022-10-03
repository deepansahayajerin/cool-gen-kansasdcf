// Program: OE_RESO_CREATE_RESOURCE_DETAILS, ID: 371818041, model: 746.
// Short name: SWE00960
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_RESO_CREATE_RESOURCE_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB		
/// Create a person's new resource (generate the resource number) and create 
/// corresponding resource address and resource lien holder address, when
/// necessary.
/// </para>
/// </summary>
[Serializable]
public partial class OeResoCreateResourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESO_CREATE_RESOURCE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeResoCreateResourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeResoCreateResourceDetails.
  /// </summary>
  public OeResoCreateResourceDetails(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	02/28/96	If an Identifier
    // is passed back from Income Source, then associate
    // that income source with the cse person resource.
    // G.Lofton	03/18/96	If an identifier
    // is passed back from External Agency, then
    // associate that external agency with the cse
    // person resource.
    // ******** END MAINTENANCE LOG ****************
    UseOeCabSetMnemonics();
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonResource.Assign(import.CsePersonResource);
    export.ResourceLienHolderAddress.EffectiveDate =
      import.ResourceLienHolderAddress.EffectiveDate;
    export.ResourceLocationAddress.Assign(import.ResourceLocationAddress);
    export.CsePersonVehicle.Identifier = import.CsePersonVehicle.Identifier;

    if (ReadCsePerson())
    {
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ------------------------------------------------------------
    // Read resources for person to determine last resource number.
    // Add one to that number and use as the new resource number.
    // ------------------------------------------------------------
    if (ReadCsePersonResource1())
    {
      local.New1.ResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    }

    ++local.New1.ResourceNo;

    try
    {
      CreateCsePersonResource();
      export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
      export.UpdatedStamp.LastUpdatedBy =
        entities.ExistingCsePersonResource.LastUpdatedBy;
      export.UpdatedStamp.LastUpdatedTimestamp =
        entities.ExistingCsePersonResource.LastUpdatedTimestamp;

      if (!Equal(import.FromIncomeSource.Identifier, local.NullDate.Timestamp))
      {
        if (ReadIncomeSource())
        {
          AssociateCsePersonResource1();
        }
      }

      if (import.FromExternalAgency.Identifier > 0)
      {
        if (ReadExternalAgency())
        {
          AssociateCsePersonResource3();
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_RESOURCE_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CSE_PERSON_RESOURCE_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsEmpty(import.ResourceLocationAddress.City) || !
      IsEmpty(import.ResourceLocationAddress.State) || !
      IsEmpty(import.ResourceLocationAddress.ZipCode5) || !
      IsEmpty(import.ResourceLocationAddress.Street1))
    {
      if (Equal(import.ResourceLocationAddress.EffectiveDate, null))
      {
        local.ResourceLocationAddress.EffectiveDate = Now().Date;
      }
      else
      {
        local.ResourceLocationAddress.EffectiveDate =
          import.ResourceLocationAddress.EffectiveDate;
      }

      try
      {
        CreateResourceLocationAddress();
        export.ResourceLocationAddress.Assign(
          entities.NewResourceLocationAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "RESOURCE_LOCATION_ADDRESS_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "RESOURCE_LOCATION_ADDRESS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsEmpty(import.ResourceLienHolderAddress.Street1) || !
      IsEmpty(import.ResourceLienHolderAddress.City) || !
      IsEmpty(import.ResourceLienHolderAddress.State) || !
      IsEmpty(import.ResourceLienHolderAddress.ZipCode5))
    {
      if (Equal(import.ResourceLienHolderAddress.EffectiveDate, null))
      {
        local.ResourceLienHolderAddress.EffectiveDate = Now().Date;
      }
      else
      {
        local.ResourceLienHolderAddress.EffectiveDate =
          import.ResourceLienHolderAddress.EffectiveDate;
      }

      try
      {
        CreateResourceLienHolderAddress();
        export.ResourceLienHolderAddress.EffectiveDate =
          entities.NewResourceLienHolderAddress.EffectiveDate;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "RESOURCE_LIEN_HOLDER_ADDRESS_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "RESOURCE_LIEN_HOLDER_ADDRESS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (import.CsePersonVehicle.Identifier != 0)
    {
      if (!ReadCsePersonVehicle())
      {
        ExitState = "CSE_PERSON_VEHICLE_NF";

        return;
      }

      if (ReadCsePersonResource2())
      {
        ExitState = "OE0000_ANOTHR_RESOURCE_ALRD_ASSC";

        return;
      }

      AssociateCsePersonResource2();
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void AssociateCsePersonResource1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var cprResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    var cspNumber = entities.ExistingCsePersonResource.CspNumber;

    entities.ExistingIncomeSource.Populated = false;
    Update("AssociateCsePersonResource1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cprResourceNo", cprResourceNo);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetDateTime(
          command, "identifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
        db.SetString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
      });

    entities.ExistingIncomeSource.CprResourceNo = cprResourceNo;
    entities.ExistingIncomeSource.CspNumber = cspNumber;
    entities.ExistingIncomeSource.Populated = true;
  }

  private void AssociateCsePersonResource2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonVehicle.Populated);
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var cprCResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    var cspCNumber = entities.ExistingCsePersonResource.CspNumber;

    entities.ExistingCsePersonVehicle.Populated = false;
    Update("AssociateCsePersonResource2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cprCResourceNo", cprCResourceNo);
        db.SetNullableString(command, "cspCNumber", cspCNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonVehicle.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingCsePersonVehicle.Identifier);
      });

    entities.ExistingCsePersonVehicle.CprCResourceNo = cprCResourceNo;
    entities.ExistingCsePersonVehicle.CspCNumber = cspCNumber;
    entities.ExistingCsePersonVehicle.Populated = true;
  }

  private void AssociateCsePersonResource3()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var exaId = entities.ExistingExternalAgency.Identifier;

    entities.ExistingCsePersonResource.Populated = false;
    Update("AssociateCsePersonResource3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "exaId", exaId);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetInt32(
          command, "resourceNo", entities.ExistingCsePersonResource.ResourceNo);
          
      });

    entities.ExistingCsePersonResource.ExaId = exaId;
    entities.ExistingCsePersonResource.Populated = true;
  }

  private void CreateCsePersonResource()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var resourceNo = local.New1.ResourceNo;
    var locationCounty = import.CsePersonResource.LocationCounty ?? "";
    var lienHolderStateOfKansasInd =
      import.CsePersonResource.LienHolderStateOfKansasInd ?? "";
    var otherLienHolderName = import.CsePersonResource.OtherLienHolderName ?? ""
      ;
    var coOwnerName = import.CsePersonResource.CoOwnerName ?? "";
    var verifiedUserId = global.UserId;
    var resourceDisposalDate = import.CsePersonResource.ResourceDisposalDate;
    var verifiedDate = import.CsePersonResource.VerifiedDate;
    var lienIndicator = import.CsePersonResource.LienIndicator ?? "";
    var type1 = import.CsePersonResource.Type1 ?? "";
    var accountHolderName = import.CsePersonResource.AccountHolderName ?? "";
    var accountBalance =
      import.CsePersonResource.AccountBalance.GetValueOrDefault();
    var accountNumber = import.CsePersonResource.AccountNumber ?? "";
    var resourceDescription = import.CsePersonResource.ResourceDescription ?? ""
      ;
    var location = import.CsePersonResource.Location ?? "";
    var value = import.CsePersonResource.Value.GetValueOrDefault();
    var equity = import.CsePersonResource.Equity.GetValueOrDefault();
    var cseActionTakenCode = import.CsePersonResource.CseActionTakenCode ?? "";
    var createdTimestamp = Now();
    var otherLienPlacedDate = import.CsePersonResource.OtherLienPlacedDate;
    var otherLienRemovedDate = import.CsePersonResource.OtherLienRemovedDate;

    entities.ExistingCsePersonResource.Populated = false;
    Update("CreateCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "resourceNo", resourceNo);
        db.SetNullableString(command, "locationCounty", locationCounty);
        db.SetNullableString(
          command, "lienHolderKsInd", lienHolderStateOfKansasInd);
        db.SetNullableString(command, "otherLienHldrNm", otherLienHolderName);
        db.SetNullableString(command, "coOwnerName", coOwnerName);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableDate(command, "resourceDispDate", resourceDisposalDate);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableString(command, "lienIndicator", lienIndicator);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "acHolderName", accountHolderName);
        db.SetNullableDecimal(command, "accountBalance", accountBalance);
        db.SetNullableString(command, "accountNumber", accountNumber);
        db.SetNullableString(command, "resourceDesc", resourceDescription);
        db.SetNullableString(command, "location", location);
        db.SetNullableDecimal(command, "valu", value);
        db.SetNullableDecimal(command, "equity", equity);
        db.SetNullableString(command, "cseActionCode", cseActionTakenCode);
        db.SetString(command, "createdBy", verifiedUserId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", verifiedUserId);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDate(command, "otherLienPlcdDt", otherLienPlacedDate);
        db.SetNullableDate(command, "otherLienRmvdDt", otherLienRemovedDate);
      });

    entities.ExistingCsePersonResource.CspNumber = cspNumber;
    entities.ExistingCsePersonResource.ResourceNo = resourceNo;
    entities.ExistingCsePersonResource.LocationCounty = locationCounty;
    entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
      lienHolderStateOfKansasInd;
    entities.ExistingCsePersonResource.OtherLienHolderName =
      otherLienHolderName;
    entities.ExistingCsePersonResource.CoOwnerName = coOwnerName;
    entities.ExistingCsePersonResource.VerifiedUserId = verifiedUserId;
    entities.ExistingCsePersonResource.ResourceDisposalDate =
      resourceDisposalDate;
    entities.ExistingCsePersonResource.VerifiedDate = verifiedDate;
    entities.ExistingCsePersonResource.LienIndicator = lienIndicator;
    entities.ExistingCsePersonResource.Type1 = type1;
    entities.ExistingCsePersonResource.AccountHolderName = accountHolderName;
    entities.ExistingCsePersonResource.AccountBalance = accountBalance;
    entities.ExistingCsePersonResource.AccountNumber = accountNumber;
    entities.ExistingCsePersonResource.ResourceDescription =
      resourceDescription;
    entities.ExistingCsePersonResource.Location = location;
    entities.ExistingCsePersonResource.Value = value;
    entities.ExistingCsePersonResource.Equity = equity;
    entities.ExistingCsePersonResource.CseActionTakenCode = cseActionTakenCode;
    entities.ExistingCsePersonResource.CreatedBy = verifiedUserId;
    entities.ExistingCsePersonResource.CreatedTimestamp = createdTimestamp;
    entities.ExistingCsePersonResource.LastUpdatedBy = verifiedUserId;
    entities.ExistingCsePersonResource.LastUpdatedTimestamp = createdTimestamp;
    entities.ExistingCsePersonResource.ExaId = null;
    entities.ExistingCsePersonResource.OtherLienPlacedDate =
      otherLienPlacedDate;
    entities.ExistingCsePersonResource.OtherLienRemovedDate =
      otherLienRemovedDate;
    entities.ExistingCsePersonResource.Populated = true;
  }

  private void CreateResourceLienHolderAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var cprResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    var cspNumber = entities.ExistingCsePersonResource.CspNumber;
    var effectiveDate = local.ResourceLienHolderAddress.EffectiveDate;
    var street1 = import.ResourceLienHolderAddress.Street1 ?? "";
    var street2 = import.ResourceLienHolderAddress.Street2 ?? "";
    var city = import.ResourceLienHolderAddress.City ?? "";
    var state = import.ResourceLienHolderAddress.State ?? "";
    var province = import.ResourceLienHolderAddress.Province ?? "";
    var postalCode = import.ResourceLienHolderAddress.PostalCode ?? "";
    var zipCode5 = import.ResourceLienHolderAddress.ZipCode5 ?? "";
    var zipCode4 = import.ResourceLienHolderAddress.ZipCode4 ?? "";
    var zip3 = import.ResourceLienHolderAddress.Zip3 ?? "";
    var country = import.ResourceLienHolderAddress.Country ?? "";
    var addressType = import.ResourceLienHolderAddress.AddressType ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.NewResourceLienHolderAddress.Populated = false;
    Update("CreateResourceLienHolderAddress",
      (db, command) =>
      {
        db.SetInt32(command, "cprResourceNo", cprResourceNo);
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
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.NewResourceLienHolderAddress.CprResourceNo = cprResourceNo;
    entities.NewResourceLienHolderAddress.CspNumber = cspNumber;
    entities.NewResourceLienHolderAddress.EffectiveDate = effectiveDate;
    entities.NewResourceLienHolderAddress.Street1 = street1;
    entities.NewResourceLienHolderAddress.Street2 = street2;
    entities.NewResourceLienHolderAddress.City = city;
    entities.NewResourceLienHolderAddress.State = state;
    entities.NewResourceLienHolderAddress.Province = province;
    entities.NewResourceLienHolderAddress.PostalCode = postalCode;
    entities.NewResourceLienHolderAddress.ZipCode5 = zipCode5;
    entities.NewResourceLienHolderAddress.ZipCode4 = zipCode4;
    entities.NewResourceLienHolderAddress.Zip3 = zip3;
    entities.NewResourceLienHolderAddress.Country = country;
    entities.NewResourceLienHolderAddress.AddressType = addressType;
    entities.NewResourceLienHolderAddress.CreatedBy = createdBy;
    entities.NewResourceLienHolderAddress.CreatedTimestamp = createdTimestamp;
    entities.NewResourceLienHolderAddress.LastUpdatedBy = createdBy;
    entities.NewResourceLienHolderAddress.LastUpdatedTimestamp =
      createdTimestamp;
    entities.NewResourceLienHolderAddress.Populated = true;
  }

  private void CreateResourceLocationAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var cprResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    var cspNumber = entities.ExistingCsePersonResource.CspNumber;
    var effectiveDate = local.ResourceLocationAddress.EffectiveDate;
    var street1 = import.ResourceLocationAddress.Street1 ?? "";
    var street2 = import.ResourceLocationAddress.Street2 ?? "";
    var city = import.ResourceLocationAddress.City ?? "";
    var state = import.ResourceLocationAddress.State ?? "";
    var province = import.ResourceLocationAddress.Province ?? "";
    var postalCode = import.ResourceLocationAddress.PostalCode ?? "";
    var zipCode5 = import.ResourceLocationAddress.ZipCode5 ?? "";
    var zipCode4 = import.ResourceLocationAddress.ZipCode4 ?? "";
    var country = import.ResourceLocationAddress.Country ?? "";
    var addressType = import.ResourceLocationAddress.AddressType ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.NewResourceLocationAddress.Populated = false;
    Update("CreateResourceLocationAddress",
      (db, command) =>
      {
        db.SetInt32(command, "cprResourceNo", cprResourceNo);
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
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.NewResourceLocationAddress.CprResourceNo = cprResourceNo;
    entities.NewResourceLocationAddress.CspNumber = cspNumber;
    entities.NewResourceLocationAddress.EffectiveDate = effectiveDate;
    entities.NewResourceLocationAddress.Street1 = street1;
    entities.NewResourceLocationAddress.Street2 = street2;
    entities.NewResourceLocationAddress.City = city;
    entities.NewResourceLocationAddress.State = state;
    entities.NewResourceLocationAddress.Province = province;
    entities.NewResourceLocationAddress.PostalCode = postalCode;
    entities.NewResourceLocationAddress.ZipCode5 = zipCode5;
    entities.NewResourceLocationAddress.ZipCode4 = zipCode4;
    entities.NewResourceLocationAddress.Zip3 = "";
    entities.NewResourceLocationAddress.Country = country;
    entities.NewResourceLocationAddress.AddressType = addressType;
    entities.NewResourceLocationAddress.CreatedBy = createdBy;
    entities.NewResourceLocationAddress.CreatedTimestamp = createdTimestamp;
    entities.NewResourceLocationAddress.LastUpdatedBy = createdBy;
    entities.NewResourceLocationAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.NewResourceLocationAddress.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonResource1()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 23);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonResource2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonVehicle.Populated);
    entities.ExistingCurrentAnother.Populated = false;

    return Read("ReadCsePersonResource2",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo",
          entities.ExistingCsePersonVehicle.CprCResourceNo.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber",
          entities.ExistingCsePersonVehicle.CspCNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCurrentAnother.CspNumber = db.GetString(reader, 0);
        entities.ExistingCurrentAnother.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCurrentAnother.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle()
  {
    entities.ExistingCsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.CsePersonVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCsePersonVehicle.CspCNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonVehicle.Populated = true;
      });
  }

  private bool ReadExternalAgency()
  {
    entities.ExistingExternalAgency.Populated = false;

    return Read("ReadExternalAgency",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", import.FromExternalAgency.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.FromIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 1);
        entities.ExistingIncomeSource.CprResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingIncomeSource.CspNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingIncomeSource.Populated = true;
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
    /// A value of FromExternalAgency.
    /// </summary>
    [JsonPropertyName("fromExternalAgency")]
    public ExternalAgency FromExternalAgency
    {
      get => fromExternalAgency ??= new();
      set => fromExternalAgency = value;
    }

    /// <summary>
    /// A value of FromIncomeSource.
    /// </summary>
    [JsonPropertyName("fromIncomeSource")]
    public IncomeSource FromIncomeSource
    {
      get => fromIncomeSource ??= new();
      set => fromIncomeSource = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
    }

    private ExternalAgency fromExternalAgency;
    private IncomeSource fromIncomeSource;
    private CsePersonVehicle csePersonVehicle;
    private CsePerson csePerson;
    private CsePersonResource csePersonResource;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of UpdatedStamp.
    /// </summary>
    [JsonPropertyName("updatedStamp")]
    public CsePersonResource UpdatedStamp
    {
      get => updatedStamp ??= new();
      set => updatedStamp = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
    }

    private CsePersonResource updatedStamp;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CsePersonVehicle csePersonVehicle;
    private CsePersonResource csePersonResource;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CsePersonResource New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
    }

    private Code maxDate;
    private DateWorkArea nullDate;
    private CsePersonResource new1;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingExternalAgency.
    /// </summary>
    [JsonPropertyName("existingExternalAgency")]
    public ExternalAgency ExistingExternalAgency
    {
      get => existingExternalAgency ??= new();
      set => existingExternalAgency = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("existingCsePersonVehicle")]
    public CsePersonVehicle ExistingCsePersonVehicle
    {
      get => existingCsePersonVehicle ??= new();
      set => existingCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonResource.
    /// </summary>
    [JsonPropertyName("existingCsePersonResource")]
    public CsePersonResource ExistingCsePersonResource
    {
      get => existingCsePersonResource ??= new();
      set => existingCsePersonResource = value;
    }

    /// <summary>
    /// A value of NewResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("newResourceLocationAddress")]
    public ResourceLocationAddress NewResourceLocationAddress
    {
      get => newResourceLocationAddress ??= new();
      set => newResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of NewResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("newResourceLienHolderAddress")]
    public ResourceLienHolderAddress NewResourceLienHolderAddress
    {
      get => newResourceLienHolderAddress ??= new();
      set => newResourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of ExistingCurrentAnother.
    /// </summary>
    [JsonPropertyName("existingCurrentAnother")]
    public CsePersonResource ExistingCurrentAnother
    {
      get => existingCurrentAnother ??= new();
      set => existingCurrentAnother = value;
    }

    private ExternalAgency existingExternalAgency;
    private IncomeSource existingIncomeSource;
    private CsePersonVehicle existingCsePersonVehicle;
    private CsePerson existingCsePerson;
    private CsePersonResource existingCsePersonResource;
    private ResourceLocationAddress newResourceLocationAddress;
    private ResourceLienHolderAddress newResourceLienHolderAddress;
    private CsePersonResource existingCurrentAnother;
  }
#endregion
}
