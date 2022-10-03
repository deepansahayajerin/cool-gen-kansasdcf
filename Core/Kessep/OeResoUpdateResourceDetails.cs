// Program: OE_RESO_UPDATE_RESOURCE_DETAILS, ID: 371818040, model: 746.
// Short name: SWE00963
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
/// A program: OE_RESO_UPDATE_RESOURCE_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB		
/// Update existing person resource and address for resource location and/or 
/// lien holder.
/// </para>
/// </summary>
[Serializable]
public partial class OeResoUpdateResourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESO_UPDATE_RESOURCE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeResoUpdateResourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeResoUpdateResourceDetails.
  /// </summary>
  public OeResoUpdateResourceDetails(IContext context, Import import,
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
    // T.O.Redmond	12/28/95  	Disallow update
    // of Lienholder Name and Address if a Legal Action
    // Person Resource is found.
    // T.O.Redmond	02/28/96	If an Identifier
    // is passed back from Income Source, then associate
    // the income source with the person resource.
    // G.Lofton	03/18/96	If an identifier
    // is passed back from External Agency, then
    // associate the external agency with the cse person
    // resource.
    // SHERAZ MALIK	04/29/97	CHANGE CURRENT_DATE
    // ******** END MAINTENANCE LOG ****************
    local.CurrentDateWorkArea.Date = Now().Date;
    export.CsePersonResource.Assign(import.CsePersonResource);

    if (ReadCsePersonResource1())
    {
      foreach(var item in ReadLegalActionPersonResource())
      {
        local.LegalResourceFound.Flag = "Y";
      }

      UpdateCsePersonResource();
      export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
      export.LastUpdated.LastUpdatedBy =
        entities.ExistingCsePersonResource.LastUpdatedBy;
      export.LastUpdated.LastUpdatedTimestamp =
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
        if (ReadExternalAgency1())
        {
          AssociateCsePersonResource3();
        }
      }
      else if (import.Previous.Identifier > 0)
      {
        if (ReadExternalAgency2())
        {
          DisassociateExternalAgency();
        }
      }
    }

    if (IsEmpty(import.ResourceLocationAddress.City) && IsEmpty
      (import.ResourceLocationAddress.AddressType) && IsEmpty
      (import.ResourceLocationAddress.Country) && Equal
      (import.ResourceLocationAddress.EffectiveDate, null) && IsEmpty
      (import.ResourceLocationAddress.PostalCode) && IsEmpty
      (import.ResourceLocationAddress.Province) && IsEmpty
      (import.ResourceLocationAddress.State) && IsEmpty
      (import.ResourceLocationAddress.Street1) && IsEmpty
      (import.ResourceLocationAddress.Street2) && IsEmpty
      (import.ResourceLocationAddress.Zip3) && IsEmpty
      (import.ResourceLocationAddress.ZipCode4) && IsEmpty
      (import.ResourceLocationAddress.ZipCode5))
    {
      foreach(var item in ReadResourceLocationAddress2())
      {
        DeleteResourceLocationAddress();
      }
    }

    if (AsChar(local.LegalResourceFound.Flag) != 'Y')
    {
      if (IsEmpty(import.ResourceLienHolderAddress.City) && IsEmpty
        (import.ResourceLienHolderAddress.AddressType) && IsEmpty
        (import.ResourceLienHolderAddress.Country) && Equal
        (import.ResourceLienHolderAddress.EffectiveDate, null) && IsEmpty
        (import.ResourceLienHolderAddress.PostalCode) && IsEmpty
        (import.ResourceLienHolderAddress.Province) && IsEmpty
        (import.ResourceLienHolderAddress.State) && IsEmpty
        (import.ResourceLienHolderAddress.Street1) && IsEmpty
        (import.ResourceLienHolderAddress.Street2) && IsEmpty
        (import.ResourceLienHolderAddress.Zip3) && IsEmpty
        (import.ResourceLienHolderAddress.ZipCode4) && IsEmpty
        (import.ResourceLienHolderAddress.ZipCode5))
      {
        foreach(var item in ReadResourceLienHolderAddress2())
        {
          DeleteResourceLienHolderAddress();
        }
      }
    }

    if (!IsEmpty(import.ResourceLocationAddress.City) || !
      IsEmpty(import.ResourceLocationAddress.State) || !
      IsEmpty(import.ResourceLocationAddress.ZipCode5) || !
      IsEmpty(import.ResourceLocationAddress.Street1))
    {
      if (Equal(import.ResourceLocationAddress.EffectiveDate, null))
      {
        local.ResourceLocationAddress.EffectiveDate =
          local.CurrentDateWorkArea.Date;
      }
      else
      {
        local.ResourceLocationAddress.EffectiveDate =
          import.ResourceLocationAddress.EffectiveDate;
      }

      local.ResourceLocnAddrFound.Flag = "N";

      if (ReadResourceLocationAddress1())
      {
        local.ResourceLocnAddrFound.Flag = "Y";
        local.CurrentResourceLocationAddress.Assign(
          entities.ExistingResourceLocationAddress);
        DeleteResourceLocationAddress();
        CreateResourceLocationAddress();
        export.ResourceLocationAddress.Assign(
          entities.ExistingResourceLocationAddress);
        export.LastUpdated.LastUpdatedBy = global.UserId;
        export.LastUpdated.LastUpdatedTimestamp = Now();
      }

      if (AsChar(local.ResourceLocnAddrFound.Flag) == 'N')
      {
        CreateResourceLocationAddress();
        export.ResourceLocationAddress.Assign(
          entities.ExistingResourceLocationAddress);
        export.LastUpdated.LastUpdatedBy = global.UserId;
        export.LastUpdated.LastUpdatedTimestamp = Now();
      }
    }

    if (AsChar(local.LegalResourceFound.Flag) != 'Y')
    {
      if (!IsEmpty(import.ResourceLienHolderAddress.Street1) || !
        IsEmpty(import.ResourceLienHolderAddress.City) || !
        IsEmpty(import.ResourceLienHolderAddress.State) || !
        IsEmpty(import.ResourceLienHolderAddress.ZipCode5))
      {
        if (Equal(import.ResourceLienHolderAddress.EffectiveDate, null))
        {
          local.ResourceLienHolderAddress.EffectiveDate =
            local.CurrentDateWorkArea.Date;
        }
        else
        {
          local.ResourceLienHolderAddress.EffectiveDate =
            import.ResourceLienHolderAddress.EffectiveDate;
        }

        local.ResourceLienhAddrFound.Flag = "N";

        if (ReadResourceLienHolderAddress1())
        {
          local.ResourceLienhAddrFound.Flag = "Y";
          DeleteResourceLienHolderAddress();
          local.CurrentResourceLienHolderAddress.Assign(
            entities.ExistingResourceLienHolderAddress);
          CreateResourceLienHolderAddress2();
          MoveResourceLienHolderAddress(entities.
            ExistingResourceLienHolderAddress,
            export.ResourceLienHolderAddress);
          export.LastUpdated.LastUpdatedBy = global.UserId;
          export.LastUpdated.LastUpdatedTimestamp = Now();
        }

        if (AsChar(local.ResourceLienhAddrFound.Flag) == 'N')
        {
          CreateResourceLienHolderAddress1();
          MoveResourceLienHolderAddress(entities.
            ExistingResourceLienHolderAddress,
            export.ResourceLienHolderAddress);
          export.LastUpdated.LastUpdatedBy = global.UserId;
          export.LastUpdated.LastUpdatedTimestamp = Now();
        }
      }
    }

    if (ReadCsePersonVehicle1())
    {
      if (entities.ExistingCurrent.Identifier == import
        .CsePersonVehicle.Identifier)
      {
        // ---------------------------------------------
        // No action needed
        // ---------------------------------------------
        return;
      }
      else
      {
        DisassociateCsePersonVehicle();
      }
    }

    if (import.CsePersonVehicle.Identifier != 0)
    {
      if (ReadCsePersonVehicle2())
      {
        if (ReadCsePersonResource2())
        {
          ExitState = "OE0000_ANOTHR_RESOURCE_ALRD_ASSC";

          return;
        }

        AssociateCsePersonResource2();
      }
      else
      {
        ExitState = "CSE_PERSON_VEHICLE_NF";
      }
    }
  }

  private static void MoveResourceLienHolderAddress(
    ResourceLienHolderAddress source, ResourceLienHolderAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void AssociateCsePersonResource1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);

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
    System.Diagnostics.Debug.Assert(entities.ExistingNew.Populated);
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

    var cprCResourceNo = entities.ExistingCsePersonResource.ResourceNo;
    var cspCNumber = entities.ExistingCsePersonResource.CspNumber;

    entities.ExistingNew.Populated = false;
    Update("AssociateCsePersonResource2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cprCResourceNo", cprCResourceNo);
        db.SetNullableString(command, "cspCNumber", cspCNumber);
        db.SetString(command, "cspNumber", entities.ExistingNew.CspNumber);
        db.SetInt32(command, "identifier", entities.ExistingNew.Identifier);
      });

    entities.ExistingNew.CprCResourceNo = cprCResourceNo;
    entities.ExistingNew.CspCNumber = cspCNumber;
    entities.ExistingNew.Populated = true;
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

  private void CreateResourceLienHolderAddress1()
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

    entities.ExistingResourceLienHolderAddress.Populated = false;
    Update("CreateResourceLienHolderAddress1",
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

    entities.ExistingResourceLienHolderAddress.CprResourceNo = cprResourceNo;
    entities.ExistingResourceLienHolderAddress.CspNumber = cspNumber;
    entities.ExistingResourceLienHolderAddress.EffectiveDate = effectiveDate;
    entities.ExistingResourceLienHolderAddress.Street1 = street1;
    entities.ExistingResourceLienHolderAddress.Street2 = street2;
    entities.ExistingResourceLienHolderAddress.City = city;
    entities.ExistingResourceLienHolderAddress.State = state;
    entities.ExistingResourceLienHolderAddress.Province = province;
    entities.ExistingResourceLienHolderAddress.PostalCode = postalCode;
    entities.ExistingResourceLienHolderAddress.ZipCode5 = zipCode5;
    entities.ExistingResourceLienHolderAddress.ZipCode4 = zipCode4;
    entities.ExistingResourceLienHolderAddress.Zip3 = zip3;
    entities.ExistingResourceLienHolderAddress.Country = country;
    entities.ExistingResourceLienHolderAddress.AddressType = addressType;
    entities.ExistingResourceLienHolderAddress.CreatedBy = createdBy;
    entities.ExistingResourceLienHolderAddress.CreatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLienHolderAddress.LastUpdatedBy = createdBy;
    entities.ExistingResourceLienHolderAddress.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLienHolderAddress.Populated = true;
  }

  private void CreateResourceLienHolderAddress2()
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ExistingResourceLienHolderAddress.Populated = false;
    Update("CreateResourceLienHolderAddress2",
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
        db.SetNullableString(command, "addressType", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ExistingResourceLienHolderAddress.CprResourceNo = cprResourceNo;
    entities.ExistingResourceLienHolderAddress.CspNumber = cspNumber;
    entities.ExistingResourceLienHolderAddress.EffectiveDate = effectiveDate;
    entities.ExistingResourceLienHolderAddress.Street1 = street1;
    entities.ExistingResourceLienHolderAddress.Street2 = street2;
    entities.ExistingResourceLienHolderAddress.City = city;
    entities.ExistingResourceLienHolderAddress.State = state;
    entities.ExistingResourceLienHolderAddress.Province = province;
    entities.ExistingResourceLienHolderAddress.PostalCode = postalCode;
    entities.ExistingResourceLienHolderAddress.ZipCode5 = zipCode5;
    entities.ExistingResourceLienHolderAddress.ZipCode4 = zipCode4;
    entities.ExistingResourceLienHolderAddress.Zip3 = zip3;
    entities.ExistingResourceLienHolderAddress.Country = country;
    entities.ExistingResourceLienHolderAddress.AddressType = "";
    entities.ExistingResourceLienHolderAddress.CreatedBy = createdBy;
    entities.ExistingResourceLienHolderAddress.CreatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLienHolderAddress.LastUpdatedBy = createdBy;
    entities.ExistingResourceLienHolderAddress.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLienHolderAddress.Populated = true;
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
    var zip3 = import.ResourceLocationAddress.Zip3 ?? "";
    var country = import.ResourceLocationAddress.Country ?? "";
    var addressType = import.ResourceLocationAddress.AddressType ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ExistingResourceLocationAddress.Populated = false;
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
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ExistingResourceLocationAddress.CprResourceNo = cprResourceNo;
    entities.ExistingResourceLocationAddress.CspNumber = cspNumber;
    entities.ExistingResourceLocationAddress.EffectiveDate = effectiveDate;
    entities.ExistingResourceLocationAddress.Street1 = street1;
    entities.ExistingResourceLocationAddress.Street2 = street2;
    entities.ExistingResourceLocationAddress.City = city;
    entities.ExistingResourceLocationAddress.State = state;
    entities.ExistingResourceLocationAddress.Province = province;
    entities.ExistingResourceLocationAddress.PostalCode = postalCode;
    entities.ExistingResourceLocationAddress.ZipCode5 = zipCode5;
    entities.ExistingResourceLocationAddress.ZipCode4 = zipCode4;
    entities.ExistingResourceLocationAddress.Zip3 = zip3;
    entities.ExistingResourceLocationAddress.Country = country;
    entities.ExistingResourceLocationAddress.AddressType = addressType;
    entities.ExistingResourceLocationAddress.CreatedBy = createdBy;
    entities.ExistingResourceLocationAddress.CreatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLocationAddress.LastUpdatedBy = createdBy;
    entities.ExistingResourceLocationAddress.LastUpdatedTimestamp =
      createdTimestamp;
    entities.ExistingResourceLocationAddress.Populated = true;
  }

  private void DeleteResourceLienHolderAddress()
  {
    Update("DeleteResourceLienHolderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingResourceLienHolderAddress.CprResourceNo);
        db.SetString(
          command, "cspNumber",
          entities.ExistingResourceLienHolderAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingResourceLienHolderAddress.EffectiveDate.
            GetValueOrDefault());
      });
  }

  private void DeleteResourceLocationAddress()
  {
    Update("DeleteResourceLocationAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingResourceLocationAddress.CprResourceNo);
        db.SetString(
          command, "cspNumber",
          entities.ExistingResourceLocationAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingResourceLocationAddress.EffectiveDate.
            GetValueOrDefault());
      });
  }

  private void DisassociateCsePersonVehicle()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    entities.ExistingCurrent.Populated = false;
    Update("DisassociateCsePersonVehicle#1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ExistingCurrent.CspNumber);
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    Update("DisassociateCsePersonVehicle#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo",
          entities.ExistingCurrent.CprCResourceNo.GetValueOrDefault());
        db.SetString(
          command, "cspNumber2", entities.ExistingCurrent.CspCNumber ?? "");
      });

    entities.ExistingCurrent.CprCResourceNo = null;
    entities.ExistingCurrent.CspCNumber = null;
    entities.ExistingCurrent.Populated = true;
  }

  private void DisassociateExternalAgency()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingCsePersonResource.Populated = false;
    Update("DisassociateExternalAgency",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetInt32(
          command, "resourceNo", entities.ExistingCsePersonResource.ResourceNo);
          
      });

    entities.ExistingCsePersonResource.ExaId = null;
    entities.ExistingCsePersonResource.Populated = true;
  }

  private bool ReadCsePersonResource1()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource1",
      (db, command) =>
      {
        db.SetInt32(command, "resourceNo", import.CsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
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
    System.Diagnostics.Debug.Assert(entities.ExistingNew.Populated);
    entities.ExistingCurrentAnother.Populated = false;

    return Read("ReadCsePersonResource2",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo",
          entities.ExistingNew.CprCResourceNo.GetValueOrDefault());
        db.
          SetString(command, "cspNumber", entities.ExistingNew.CspCNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.ExistingCurrentAnother.CspNumber = db.GetString(reader, 0);
        entities.ExistingCurrentAnother.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCurrentAnother.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingCurrent.Populated = false;

    return Read("ReadCsePersonVehicle1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableInt32(
          command, "cprCResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetNullableString(
          command, "cspCNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCurrent.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCurrent.CspCNumber = db.GetNullableString(reader, 3);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle2()
  {
    entities.ExistingNew.Populated = false;

    return Read("ReadCsePersonVehicle2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", import.CsePersonVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingNew.CspNumber = db.GetString(reader, 0);
        entities.ExistingNew.Identifier = db.GetInt32(reader, 1);
        entities.ExistingNew.CprCResourceNo = db.GetNullableInt32(reader, 2);
        entities.ExistingNew.CspCNumber = db.GetNullableString(reader, 3);
        entities.ExistingNew.Populated = true;
      });
  }

  private bool ReadExternalAgency1()
  {
    entities.ExistingExternalAgency.Populated = false;

    return Read("ReadExternalAgency1",
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

  private bool ReadExternalAgency2()
  {
    entities.ExistingExternalAgency.Populated = false;

    return Read("ReadExternalAgency2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Previous.Identifier);
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

  private IEnumerable<bool> ReadLegalActionPersonResource()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingLegalActionPersonResource.Populated = false;

    return ReadEach("ReadLegalActionPersonResource",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetDate(
          command, "effectiveDt",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPersonResource.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingLegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.ExistingLegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingLegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPersonResource.Identifier =
          db.GetInt32(reader, 6);
        entities.ExistingLegalActionPersonResource.Populated = true;

        return true;
      });
  }

  private bool ReadResourceLienHolderAddress1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLienHolderAddress.Populated = false;

    return Read("ReadResourceLienHolderAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLienHolderAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLienHolderAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLienHolderAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLienHolderAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLienHolderAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLienHolderAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLienHolderAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLienHolderAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLienHolderAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLienHolderAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLienHolderAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLienHolderAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLienHolderAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLienHolderAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLienHolderAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLienHolderAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLienHolderAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLienHolderAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLienHolderAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadResourceLienHolderAddress2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLienHolderAddress.Populated = false;

    return ReadEach("ReadResourceLienHolderAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLienHolderAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLienHolderAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLienHolderAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLienHolderAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLienHolderAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLienHolderAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLienHolderAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLienHolderAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLienHolderAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLienHolderAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLienHolderAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLienHolderAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLienHolderAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLienHolderAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLienHolderAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLienHolderAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLienHolderAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLienHolderAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLienHolderAddress.Populated = true;

        return true;
      });
  }

  private bool ReadResourceLocationAddress1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLocationAddress.Populated = false;

    return Read("ReadResourceLocationAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLocationAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLocationAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLocationAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLocationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLocationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLocationAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLocationAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLocationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLocationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLocationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLocationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLocationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLocationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLocationAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLocationAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLocationAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLocationAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLocationAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLocationAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadResourceLocationAddress2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLocationAddress.Populated = false;

    return ReadEach("ReadResourceLocationAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLocationAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLocationAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLocationAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLocationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLocationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLocationAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLocationAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLocationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLocationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLocationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLocationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLocationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLocationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLocationAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLocationAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLocationAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLocationAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLocationAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLocationAddress.Populated = true;

        return true;
      });
  }

  private void UpdateCsePersonResource()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);

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
    var lastUpdatedTimestamp = Now();
    var otherLienPlacedDate = import.CsePersonResource.OtherLienPlacedDate;
    var otherLienRemovedDate = import.CsePersonResource.OtherLienRemovedDate;

    entities.ExistingCsePersonResource.Populated = false;
    Update("UpdateCsePersonResource",
      (db, command) =>
      {
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
        db.SetString(command, "lastUpdatedBy", verifiedUserId);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableDate(command, "otherLienPlcdDt", otherLienPlacedDate);
        db.SetNullableDate(command, "otherLienRmvdDt", otherLienRemovedDate);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetInt32(
          command, "resourceNo", entities.ExistingCsePersonResource.ResourceNo);
          
      });

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
    entities.ExistingCsePersonResource.LastUpdatedBy = verifiedUserId;
    entities.ExistingCsePersonResource.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingCsePersonResource.OtherLienPlacedDate =
      otherLienPlacedDate;
    entities.ExistingCsePersonResource.OtherLienRemovedDate =
      otherLienRemovedDate;
    entities.ExistingCsePersonResource.Populated = true;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ExternalAgency Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

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

    private ExternalAgency previous;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ExternalAgency Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public CsePersonResource LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
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

    private ExternalAgency previous;
    private CsePersonResource lastUpdated;
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("currentResourceLienHolderAddress")]
    public ResourceLienHolderAddress CurrentResourceLienHolderAddress
    {
      get => currentResourceLienHolderAddress ??= new();
      set => currentResourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of CurrentResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("currentResourceLocationAddress")]
    public ResourceLocationAddress CurrentResourceLocationAddress
    {
      get => currentResourceLocationAddress ??= new();
      set => currentResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienhAddrFound.
    /// </summary>
    [JsonPropertyName("resourceLienhAddrFound")]
    public Common ResourceLienhAddrFound
    {
      get => resourceLienhAddrFound ??= new();
      set => resourceLienhAddrFound = value;
    }

    /// <summary>
    /// A value of ResourceLocnAddrFound.
    /// </summary>
    [JsonPropertyName("resourceLocnAddrFound")]
    public Common ResourceLocnAddrFound
    {
      get => resourceLocnAddrFound ??= new();
      set => resourceLocnAddrFound = value;
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

    /// <summary>
    /// A value of LegalResourceFound.
    /// </summary>
    [JsonPropertyName("legalResourceFound")]
    public Common LegalResourceFound
    {
      get => legalResourceFound ??= new();
      set => legalResourceFound = value;
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

    private DateWorkArea currentDateWorkArea;
    private ResourceLienHolderAddress currentResourceLienHolderAddress;
    private ResourceLocationAddress currentResourceLocationAddress;
    private Common resourceLienhAddrFound;
    private Common resourceLocnAddrFound;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
    private Common legalResourceFound;
    private DateWorkArea nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("existingLegalActionPersonResource")]
    public LegalActionPersonResource ExistingLegalActionPersonResource
    {
      get => existingLegalActionPersonResource ??= new();
      set => existingLegalActionPersonResource = value;
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

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public CsePersonVehicle ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    /// <summary>
    /// A value of ExistingNew.
    /// </summary>
    [JsonPropertyName("existingNew")]
    public CsePersonVehicle ExistingNew
    {
      get => existingNew ??= new();
      set => existingNew = value;
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
    /// A value of ExistingResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("existingResourceLocationAddress")]
    public ResourceLocationAddress ExistingResourceLocationAddress
    {
      get => existingResourceLocationAddress ??= new();
      set => existingResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ExistingResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("existingResourceLienHolderAddress")]
    public ResourceLienHolderAddress ExistingResourceLienHolderAddress
    {
      get => existingResourceLienHolderAddress ??= new();
      set => existingResourceLienHolderAddress = value;
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
    /// A value of ExistingExternalAgency.
    /// </summary>
    [JsonPropertyName("existingExternalAgency")]
    public ExternalAgency ExistingExternalAgency
    {
      get => existingExternalAgency ??= new();
      set => existingExternalAgency = value;
    }

    private LegalActionPersonResource existingLegalActionPersonResource;
    private CsePersonResource existingCurrentAnother;
    private CsePersonVehicle existingCurrent;
    private CsePersonVehicle existingNew;
    private CsePerson existingCsePerson;
    private CsePersonResource existingCsePersonResource;
    private ResourceLocationAddress existingResourceLocationAddress;
    private ResourceLienHolderAddress existingResourceLienHolderAddress;
    private IncomeSource existingIncomeSource;
    private ExternalAgency existingExternalAgency;
  }
#endregion
}
