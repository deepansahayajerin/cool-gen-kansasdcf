// Program: LE_TRIB_UPDATE_TRIBUNAL, ID: 372021818, model: 746.
// Short name: SWE00824
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
/// A program: LE_TRIB_UPDATE_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block updates Tribunal and Tribunal Address entity type 
/// occurrences.
/// </para>
/// </summary>
[Serializable]
public partial class LeTribUpdateTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_UPDATE_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribUpdateTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribUpdateTribunal.
  /// </summary>
  public LeTribUpdateTribunal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 10/08/98	D. Jean     			Combined two update statements of tribunal, add 
    // default values to mandatory attributes in create statement.
    // 09/15/2000	GVandy		PR 102557	Added tribunal document header attributes.
    // *******************************************************************
    // *********************************************
    // Need to fix: This acblk updates the corresponding CSE PERSON ORGANIZATION
    // based on FIPS. But it does not check if all the three fields are
    // nonzero (state, county and location). Based on what the users want about
    // fips, we may need to fix this to update only those organization records
    // associated with a fips with valid state, county AND location.
    // *********************************************
    export.Tribunal.Assign(import.Tribunal);
    export.Fips.Assign(import.Fips);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Update.DetailListAddrTp.PromptField =
        import.Import1.Item.DetailListAddrTp.PromptField;
      export.Export1.Update.DetailListStates.PromptField =
        import.Import1.Item.DetailListStates.PromptField;
      export.Export1.Update.DetailSelAddr.SelectChar =
        import.Import1.Item.DetailSelAddr.SelectChar;
      export.Export1.Next();
    }

    if (!ReadTribunal1())
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    if (!Equal(entities.ExistingTribunal.Name, import.Tribunal.Name) || !
      Equal(entities.ExistingTribunal.JudicialDistrict,
      import.Tribunal.JudicialDistrict) || !
      Equal(entities.ExistingTribunal.JudicialDivision,
      import.Tribunal.JudicialDivision))
    {
      try
      {
        UpdateTribunal1();
        export.Tribunal.Assign(entities.ExistingTribunal);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "TRIBUNAL_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "TRIBUNAL_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (ReadFips1())
    {
      local.Fips.Assign(entities.ExistingFips);

      if (entities.ExistingFips.State != import.Fips.State || entities
        .ExistingFips.County != import.Fips.County || entities
        .ExistingFips.Location != import.Fips.Location)
      {
        if (import.Fips.State == 0 && import.Fips.County == 0 && import
          .Fips.Location == 0)
        {
          // -------------------------------------------------------------
          // Don't allow a US tribunal to be changed to a Foreign Tribunal. This
          // would cause a problem. A US tribunal when displayed will have
          // address details protected. So the user will not be able to enter
          // the address details. When updated, the tribunal will not have any
          // address. So LTRB will not be able to see that foreign tribunal
          // because it uses the country in the address for selecting records.
          // So you will not be able to access the tribunal updated.
          // -------------------------------------------------------------
          ExitState = "LE0000_CANT_CHANGE_US_TRIB_FORGN";

          return;
        }

        // --- FIPS has been changed.
        foreach(var item in ReadFipsTribAddress5())
        {
          // --- The following read is not necessary. It is just to clean up if 
          // any bad data exists
          if (ReadFips3())
          {
            DisassociateFipsTribAddress();
          }
          else
          {
            DeleteFipsTribAddress();
          }
        }

        DisassociateTribunal();
      }
      else
      {
        // --- No change in fips. So no action on relationship from 
        // fips_trib_address
      }
    }
    else if (import.Fips.State != 0 || import.Fips.County != 0 || import
      .Fips.Location != 0)
    {
      // --- Currently not associated with a fips. But a new fips has been 
      // specified. So delete the current tribunal address
      foreach(var item in ReadFipsTribAddress5())
      {
        // --- The following read is not necessary. It is just to clean up if 
        // any bad data exists
        if (ReadFips3())
        {
          DisassociateFipsTribAddress();
        }
        else
        {
          DeleteFipsTribAddress();
        }
      }
    }
    else
    {
      // --- It was not associated with a fips. New fips has not been specified.
      // So update the existing fips_trib_address record without any change to
      // the relationships.
    }

    if (import.Fips.State != 0 || import.Fips.County != 0 || import
      .Fips.Location != 0)
    {
      if (ReadFips4())
      {
        export.Fips.Assign(entities.NewFips);
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }

      if (ReadTribunal2())
      {
        ExitState = "LE0000_ANOTHER_TRIB_WITH_FIPS_AE";

        return;
      }

      if (ReadCsePerson())
      {
        local.NewOrgz.Assign(entities.ExistingUsTribOrgz);
        MoveTribunal(import.Tribunal, local.New1);

        // --- Now compare and modify the values
        // *********************************************
        // 9/29/97	E. Parker -DIR	Remove compare logic
        // *********************************************
        local.NewOrgz.OrganizationName = import.Tribunal.Name;
        local.NewOrgz.TaxId = import.Tribunal.TaxId ?? "";
        local.NewOrgz.TaxIdSuffix = import.Tribunal.TaxIdSuffix ?? "";
        local.NewOrgz.Type1 = "O";
        local.NewOrgz.Number = entities.ExistingUsTribOrgz.Number;
        UseSiUpdateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }
      else
      {
        // --- Create Organization
        local.NewOrgz.Type1 = "O";
        local.NewOrgz.OrganizationName = import.Tribunal.Name;
        local.NewOrgz.TaxId = import.Tribunal.TaxId ?? "";
        local.NewOrgz.TaxIdSuffix = import.Tribunal.TaxIdSuffix ?? "";
        UseSiCreateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // *********************************************
        // Now the Tribunal can be updated with the new Tax ID and Suffix.
        // H00026547 - RCG 01/21/98
        // *********************************************
        MoveTribunal(import.Tribunal, local.New1);
      }

      try
      {
        UpdateTribunal2();
        export.Tribunal.Assign(entities.ExistingTribunal);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "TRIBUNAL_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (import.Fips.State != local.Fips.State || import.Fips.County != local
        .Fips.County || import.Fips.Location != local.Fips.Location)
      {
        AssociateTribunal1();

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadFipsTribAddress6())
        {
          AssociateTribunal2();
          MoveFipsTribAddress(entities.NewFipsTribAddress,
            export.Export1.Update.Detail);
          export.Export1.Next();
        }
      }
    }

    if (entities.NewFips.Populated)
    {
      // --- The tribunal has a fips associated with it. So no address change 
      // can be performed here
      return;
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (IsEmpty(export.Export1.Item.DetailSelAddr.SelectChar))
      {
        continue;
      }

      if (export.Export1.Item.Detail.Identifier == 0)
      {
        // --- The user is trying to add a new address
        if (ReadFipsTribAddress2())
        {
          ExitState = "LE0000_ADDR_TYPE_ALREADY_EXISTS";

          return;
        }

        if (ReadFipsTribAddress4())
        {
          local.Last.Identifier = entities.ExistingFipsTribAddress.Identifier;
        }

        try
        {
          CreateFipsTribAddress();
          MoveFipsTribAddress(entities.NewFipsTribAddress,
            export.Export1.Update.Detail);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "TRIBUNAL_ADDRESS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "TRIBUNAL_ADDRESS_PV";

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
        if (ReadFipsTribAddress1())
        {
          ExitState = "LE0000_ADDR_TYPE_ALREADY_EXISTS";

          return;
        }

        if (ReadFipsTribAddress3())
        {
          if (ReadFips2())
          {
            // --- The address is tied to FIPS. So it cannot be changed here.
            ExitState = "LE0000_CHANGE_ADDR_IN_FIPS";

            return;
          }
        }

        if (export.Export1.Item.Detail.AreaCode.GetValueOrDefault() == 0 && IsEmpty
          (export.Export1.Item.Detail.City) && export
          .Export1.Item.Detail.FaxAreaCode.GetValueOrDefault() == 0 && IsEmpty
          (export.Export1.Item.Detail.FaxExtension) && export
          .Export1.Item.Detail.FaxNumber.GetValueOrDefault() == 0 && IsEmpty
          (export.Export1.Item.Detail.PhoneExtension) && export
          .Export1.Item.Detail.PhoneNumber.GetValueOrDefault() == 0 && IsEmpty
          (export.Export1.Item.Detail.PostalCode) && IsEmpty
          (export.Export1.Item.Detail.Province) && IsEmpty
          (export.Export1.Item.Detail.Street4) && IsEmpty
          (export.Export1.Item.Detail.Street1) && IsEmpty
          (export.Export1.Item.Detail.Street2) && IsEmpty
          (export.Export1.Item.Detail.Street3) && IsEmpty
          (export.Export1.Item.Detail.Zip3) && IsEmpty
          (export.Export1.Item.Detail.Zip4) && IsEmpty
          (export.Export1.Item.Detail.ZipCode))
        {
          // --- User blanked out all the fields. So delete the address record.
          DeleteFipsTribAddress();
        }
        else
        {
          try
          {
            UpdateFipsTribAddress();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "TRIBUNAL_ADDRESS_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "TRIBUNAL_ADDRESS_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveFipsTribAddress(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.Identifier = source.Identifier;
    target.FaxExtension = source.FaxExtension;
    target.FaxAreaCode = source.FaxAreaCode;
    target.PhoneExtension = source.PhoneExtension;
    target.AreaCode = source.AreaCode;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.PhoneNumber = source.PhoneNumber;
    target.FaxNumber = source.FaxNumber;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.Name = source.Name;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePerson()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    useImport.Fips.Assign(import.Fips);
    MoveCsePerson(local.NewOrgz, useImport.CsePerson);

    Call(SiCreateCsePerson.Execute, useImport, useExport);

    local.NewOrgz.Number = useExport.CsePerson.Number;
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    useImport.Fips.Assign(import.Fips);
    MoveCsePerson(local.NewOrgz, useImport.CsePerson);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void AssociateTribunal1()
  {
    var fipLocation = entities.NewFips.Location;
    var fipCounty = entities.NewFips.County;
    var fipState = entities.NewFips.State;

    entities.ExistingTribunal.Populated = false;
    Update("AssociateTribunal1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", fipLocation);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipState", fipState);
        db.
          SetInt32(command, "identifier", entities.ExistingTribunal.Identifier);
          
      });

    entities.ExistingTribunal.FipLocation = fipLocation;
    entities.ExistingTribunal.FipCounty = fipCounty;
    entities.ExistingTribunal.FipState = fipState;
    entities.ExistingTribunal.Populated = true;
  }

  private void AssociateTribunal2()
  {
    var trbId = entities.ExistingTribunal.Identifier;

    entities.NewFipsTribAddress.Populated = false;
    Update("AssociateTribunal2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetInt32(
          command, "identifier", entities.NewFipsTribAddress.Identifier);
      });

    entities.NewFipsTribAddress.TrbId = trbId;
    entities.NewFipsTribAddress.Populated = true;
  }

  private void CreateFipsTribAddress()
  {
    var identifier = local.Last.Identifier + 1;
    var faxExtension = export.Export1.Item.Detail.FaxExtension ?? "";
    var faxAreaCode =
      export.Export1.Item.Detail.FaxAreaCode.GetValueOrDefault();
    var phoneExtension = export.Export1.Item.Detail.PhoneExtension ?? "";
    var areaCode = export.Export1.Item.Detail.AreaCode.GetValueOrDefault();
    var type1 = export.Export1.Item.Detail.Type1;
    var street1 = export.Export1.Item.Detail.Street1;
    var street2 = export.Export1.Item.Detail.Street2 ?? "";
    var city = export.Export1.Item.Detail.City;
    var zipCode = export.Export1.Item.Detail.ZipCode;
    var zip4 = export.Export1.Item.Detail.Zip4 ?? "";
    var zip3 = export.Export1.Item.Detail.Zip3 ?? "";
    var street3 = export.Export1.Item.Detail.Street3 ?? "";
    var street4 = export.Export1.Item.Detail.Street4 ?? "";
    var province = export.Export1.Item.Detail.Province ?? "";
    var postalCode = export.Export1.Item.Detail.PostalCode ?? "";
    var country = import.FipsTribAddress.Country ?? "";
    var phoneNumber =
      export.Export1.Item.Detail.PhoneNumber.GetValueOrDefault();
    var faxNumber = export.Export1.Item.Detail.FaxNumber.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var trbId = entities.ExistingTribunal.Identifier;

    entities.NewFipsTribAddress.Populated = false;
    Update("CreateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "faxExtension", faxExtension);
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", "");
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableInt32(command, "trbId", trbId);
      });

    entities.NewFipsTribAddress.Identifier = identifier;
    entities.NewFipsTribAddress.FaxExtension = faxExtension;
    entities.NewFipsTribAddress.FaxAreaCode = faxAreaCode;
    entities.NewFipsTribAddress.PhoneExtension = phoneExtension;
    entities.NewFipsTribAddress.AreaCode = areaCode;
    entities.NewFipsTribAddress.Type1 = type1;
    entities.NewFipsTribAddress.Street1 = street1;
    entities.NewFipsTribAddress.Street2 = street2;
    entities.NewFipsTribAddress.City = city;
    entities.NewFipsTribAddress.State = "";
    entities.NewFipsTribAddress.ZipCode = zipCode;
    entities.NewFipsTribAddress.Zip4 = zip4;
    entities.NewFipsTribAddress.Zip3 = zip3;
    entities.NewFipsTribAddress.County = "";
    entities.NewFipsTribAddress.Street3 = street3;
    entities.NewFipsTribAddress.Street4 = street4;
    entities.NewFipsTribAddress.Province = province;
    entities.NewFipsTribAddress.PostalCode = postalCode;
    entities.NewFipsTribAddress.Country = country;
    entities.NewFipsTribAddress.PhoneNumber = phoneNumber;
    entities.NewFipsTribAddress.FaxNumber = faxNumber;
    entities.NewFipsTribAddress.CreatedBy = createdBy;
    entities.NewFipsTribAddress.CreatedTstamp = createdTstamp;
    entities.NewFipsTribAddress.LastUpdatedBy = "";
    entities.NewFipsTribAddress.LastUpdatedTstamp = null;
    entities.NewFipsTribAddress.FipState = null;
    entities.NewFipsTribAddress.FipCounty = null;
    entities.NewFipsTribAddress.FipLocation = null;
    entities.NewFipsTribAddress.TrbId = trbId;
    entities.NewFipsTribAddress.Populated = true;
  }

  private void DeleteFipsTribAddress()
  {
    Update("DeleteFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ExistingFipsTribAddress.Identifier);
      });
  }

  private void DisassociateFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;
    Update("DisassociateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ExistingFipsTribAddress.Identifier);
      });

    entities.ExistingFipsTribAddress.TrbId = null;
    entities.ExistingFipsTribAddress.Populated = true;
  }

  private void DisassociateTribunal()
  {
    entities.ExistingTribunal.Populated = false;
    Update("DisassociateTribunal",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", entities.ExistingTribunal.Identifier);
          
      });

    entities.ExistingTribunal.FipLocation = null;
    entities.ExistingTribunal.FipCounty = null;
    entities.ExistingTribunal.FipState = null;
    entities.ExistingTribunal.Populated = true;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.NewFips.Populated);
    entities.ExistingUsTribOrgz.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.NewFips.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingUsTribOrgz.Number = db.GetString(reader, 0);
        entities.ExistingUsTribOrgz.Type1 = db.GetString(reader, 1);
        entities.ExistingUsTribOrgz.TaxId = db.GetNullableString(reader, 2);
        entities.ExistingUsTribOrgz.OrganizationName =
          db.GetNullableString(reader, 3);
        entities.ExistingUsTribOrgz.TaxIdSuffix =
          db.GetNullableString(reader, 4);
        entities.ExistingUsTribOrgz.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingUsTribOrgz.Type1);
      });
  }

  private bool ReadFips1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFipsTribAddress.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingFipsTribAddress.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingFipsTribAddress.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingFipsTribAddress.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFipsTribAddress.Populated);
    entities.ExistingTemp.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingFipsTribAddress.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingFipsTribAddress.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingFipsTribAddress.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTemp.State = db.GetInt32(reader, 0);
        entities.ExistingTemp.County = db.GetInt32(reader, 1);
        entities.ExistingTemp.Location = db.GetInt32(reader, 2);
        entities.ExistingTemp.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingTemp.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingTemp.Populated = true;
      });
  }

  private bool ReadFips4()
  {
    entities.NewFips.Populated = false;

    return Read("ReadFips4",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.NewFips.State = db.GetInt32(reader, 0);
        entities.NewFips.County = db.GetInt32(reader, 1);
        entities.NewFips.Location = db.GetInt32(reader, 2);
        entities.NewFips.CountyDescription = db.GetNullableString(reader, 3);
        entities.NewFips.StateAbbreviation = db.GetString(reader, 4);
        entities.NewFips.CspNumber = db.GetNullableString(reader, 5);
        entities.NewFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
        db.SetString(command, "type", export.Export1.Item.Detail.Type1);
        db.
          SetInt32(command, "identifier", export.Export1.Item.Detail.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
        db.SetString(command, "type", export.Export1.Item.Detail.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress3()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress3",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", export.Export1.Item.Detail.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress4()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress4",
      null,
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress5()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress6()
  {
    return ReadEach("ReadFipsTribAddress6",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.NewFips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.NewFips.County);
        db.SetNullableInt32(command, "fipState", entities.NewFips.State);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.NewFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.NewFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.NewFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.NewFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.NewFipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.NewFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.NewFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.NewFipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.NewFipsTribAddress.City = db.GetString(reader, 8);
        entities.NewFipsTribAddress.State = db.GetString(reader, 9);
        entities.NewFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.NewFipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.NewFipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.NewFipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.NewFipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.NewFipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.NewFipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.NewFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.NewFipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.NewFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.NewFipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.NewFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.NewFipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.NewFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.NewFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.NewFipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.NewFipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.NewFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.NewFipsTribAddress.TrbId = db.GetNullableInt32(reader, 28);
        entities.NewFipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.CreatedBy = db.GetString(reader, 4);
        entities.ExistingTribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.ExistingTribunal.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingTribunal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 8);
        entities.ExistingTribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.ExistingTribunal.TaxId = db.GetNullableString(reader, 10);
        entities.ExistingTribunal.DocumentHeader1 =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.DocumentHeader2 =
          db.GetNullableString(reader, 12);
        entities.ExistingTribunal.DocumentHeader3 =
          db.GetNullableString(reader, 13);
        entities.ExistingTribunal.DocumentHeader4 =
          db.GetNullableString(reader, 14);
        entities.ExistingTribunal.DocumentHeader5 =
          db.GetNullableString(reader, 15);
        entities.ExistingTribunal.DocumentHeader6 =
          db.GetNullableString(reader, 16);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 17);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 18);
        entities.ExistingTribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.Overlapping.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.NewFips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.NewFips.County);
        db.SetNullableInt32(command, "fipState", entities.NewFips.State);
        db.
          SetInt32(command, "identifier", entities.ExistingTribunal.Identifier);
          
      },
      (db, reader) =>
      {
        entities.Overlapping.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Overlapping.Identifier = db.GetInt32(reader, 1);
        entities.Overlapping.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Overlapping.FipState = db.GetNullableInt32(reader, 3);
        entities.Overlapping.Populated = true;
      });
  }

  private void UpdateFipsTribAddress()
  {
    var faxExtension = export.Export1.Item.Detail.FaxExtension ?? "";
    var faxAreaCode =
      export.Export1.Item.Detail.FaxAreaCode.GetValueOrDefault();
    var phoneExtension = export.Export1.Item.Detail.PhoneExtension ?? "";
    var areaCode = export.Export1.Item.Detail.AreaCode.GetValueOrDefault();
    var type1 = export.Export1.Item.Detail.Type1;
    var street1 = export.Export1.Item.Detail.Street1;
    var street2 = export.Export1.Item.Detail.Street2 ?? "";
    var city = export.Export1.Item.Detail.City;
    var state = export.Export1.Item.Detail.State;
    var zipCode = export.Export1.Item.Detail.ZipCode;
    var zip4 = export.Export1.Item.Detail.Zip4 ?? "";
    var zip3 = export.Export1.Item.Detail.Zip3 ?? "";
    var county = export.Export1.Item.Detail.County ?? "";
    var street3 = export.Export1.Item.Detail.Street3 ?? "";
    var street4 = export.Export1.Item.Detail.Street4 ?? "";
    var province = export.Export1.Item.Detail.Province ?? "";
    var postalCode = export.Export1.Item.Detail.PostalCode ?? "";
    var country = export.Export1.Item.Detail.Country ?? "";
    var phoneNumber =
      export.Export1.Item.Detail.PhoneNumber.GetValueOrDefault();
    var faxNumber = export.Export1.Item.Detail.FaxNumber.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.ExistingFipsTribAddress.Populated = false;
    Update("UpdateFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "faxExtension", faxExtension);
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(
          command, "identifier", entities.ExistingFipsTribAddress.Identifier);
      });

    entities.ExistingFipsTribAddress.FaxExtension = faxExtension;
    entities.ExistingFipsTribAddress.FaxAreaCode = faxAreaCode;
    entities.ExistingFipsTribAddress.PhoneExtension = phoneExtension;
    entities.ExistingFipsTribAddress.AreaCode = areaCode;
    entities.ExistingFipsTribAddress.Type1 = type1;
    entities.ExistingFipsTribAddress.Street1 = street1;
    entities.ExistingFipsTribAddress.Street2 = street2;
    entities.ExistingFipsTribAddress.City = city;
    entities.ExistingFipsTribAddress.State = state;
    entities.ExistingFipsTribAddress.ZipCode = zipCode;
    entities.ExistingFipsTribAddress.Zip4 = zip4;
    entities.ExistingFipsTribAddress.Zip3 = zip3;
    entities.ExistingFipsTribAddress.County = county;
    entities.ExistingFipsTribAddress.Street3 = street3;
    entities.ExistingFipsTribAddress.Street4 = street4;
    entities.ExistingFipsTribAddress.Province = province;
    entities.ExistingFipsTribAddress.PostalCode = postalCode;
    entities.ExistingFipsTribAddress.Country = country;
    entities.ExistingFipsTribAddress.PhoneNumber = phoneNumber;
    entities.ExistingFipsTribAddress.FaxNumber = faxNumber;
    entities.ExistingFipsTribAddress.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFipsTribAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.ExistingFipsTribAddress.Populated = true;
  }

  private void UpdateTribunal1()
  {
    var judicialDivision = import.Tribunal.JudicialDivision ?? "";
    var name = import.Tribunal.Name;
    var judicialDistrict = import.Tribunal.JudicialDistrict;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var documentHeader1 = import.Tribunal.DocumentHeader1 ?? "";
    var documentHeader2 = import.Tribunal.DocumentHeader2 ?? "";
    var documentHeader3 = import.Tribunal.DocumentHeader3 ?? "";
    var documentHeader4 = import.Tribunal.DocumentHeader4 ?? "";
    var documentHeader5 = import.Tribunal.DocumentHeader5 ?? "";
    var documentHeader6 = import.Tribunal.DocumentHeader6 ?? "";

    entities.ExistingTribunal.Populated = false;
    Update("UpdateTribunal1",
      (db, command) =>
      {
        db.SetNullableString(command, "judicialDivision", judicialDivision);
        db.SetString(command, "tribunalNm", name);
        db.SetString(command, "judicialDistrict", judicialDistrict);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "documentHeader1", documentHeader1);
        db.SetNullableString(command, "documentHeader2", documentHeader2);
        db.SetNullableString(command, "documentHeader3", documentHeader3);
        db.SetNullableString(command, "documentHeader4", documentHeader4);
        db.SetNullableString(command, "documentHeader5", documentHeader5);
        db.SetNullableString(command, "documentHeader6", documentHeader6);
        db.
          SetInt32(command, "identifier", entities.ExistingTribunal.Identifier);
          
      });

    entities.ExistingTribunal.JudicialDivision = judicialDivision;
    entities.ExistingTribunal.Name = name;
    entities.ExistingTribunal.JudicialDistrict = judicialDistrict;
    entities.ExistingTribunal.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingTribunal.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.ExistingTribunal.DocumentHeader1 = documentHeader1;
    entities.ExistingTribunal.DocumentHeader2 = documentHeader2;
    entities.ExistingTribunal.DocumentHeader3 = documentHeader3;
    entities.ExistingTribunal.DocumentHeader4 = documentHeader4;
    entities.ExistingTribunal.DocumentHeader5 = documentHeader5;
    entities.ExistingTribunal.DocumentHeader6 = documentHeader6;
    entities.ExistingTribunal.Populated = true;
  }

  private void UpdateTribunal2()
  {
    var name = local.New1.Name;
    var taxIdSuffix = local.New1.TaxIdSuffix ?? "";
    var taxId = local.New1.TaxId ?? "";
    var documentHeader1 = import.Tribunal.DocumentHeader1 ?? "";
    var documentHeader2 = import.Tribunal.DocumentHeader2 ?? "";
    var documentHeader3 = import.Tribunal.DocumentHeader3 ?? "";
    var documentHeader4 = import.Tribunal.DocumentHeader4 ?? "";
    var documentHeader5 = import.Tribunal.DocumentHeader5 ?? "";
    var documentHeader6 = import.Tribunal.DocumentHeader6 ?? "";

    entities.ExistingTribunal.Populated = false;
    Update("UpdateTribunal2",
      (db, command) =>
      {
        db.SetString(command, "tribunalNm", name);
        db.SetNullableString(command, "taxIdSuffix", taxIdSuffix);
        db.SetNullableString(command, "taxId", taxId);
        db.SetNullableString(command, "documentHeader1", documentHeader1);
        db.SetNullableString(command, "documentHeader2", documentHeader2);
        db.SetNullableString(command, "documentHeader3", documentHeader3);
        db.SetNullableString(command, "documentHeader4", documentHeader4);
        db.SetNullableString(command, "documentHeader5", documentHeader5);
        db.SetNullableString(command, "documentHeader6", documentHeader6);
        db.
          SetInt32(command, "identifier", entities.ExistingTribunal.Identifier);
          
      });

    entities.ExistingTribunal.Name = name;
    entities.ExistingTribunal.TaxIdSuffix = taxIdSuffix;
    entities.ExistingTribunal.TaxId = taxId;
    entities.ExistingTribunal.DocumentHeader1 = documentHeader1;
    entities.ExistingTribunal.DocumentHeader2 = documentHeader2;
    entities.ExistingTribunal.DocumentHeader3 = documentHeader3;
    entities.ExistingTribunal.DocumentHeader4 = documentHeader4;
    entities.ExistingTribunal.DocumentHeader5 = documentHeader5;
    entities.ExistingTribunal.DocumentHeader6 = documentHeader6;
    entities.ExistingTribunal.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private Tribunal tribunal;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Fips fips;
    private Tribunal tribunal;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public FipsTribAddress Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public FipsTribAddress Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of NewOrgz.
    /// </summary>
    [JsonPropertyName("newOrgz")]
    public CsePerson NewOrgz
    {
      get => newOrgz ??= new();
      set => newOrgz = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Tribunal New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private FipsTribAddress initialised;
    private FipsTribAddress last;
    private Fips fips;
    private CsePerson newOrgz;
    private Tribunal new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Overlapping.
    /// </summary>
    [JsonPropertyName("overlapping")]
    public Tribunal Overlapping
    {
      get => overlapping ??= new();
      set => overlapping = value;
    }

    /// <summary>
    /// A value of ExistingTemp.
    /// </summary>
    [JsonPropertyName("existingTemp")]
    public Fips ExistingTemp
    {
      get => existingTemp ??= new();
      set => existingTemp = value;
    }

    /// <summary>
    /// A value of NewFips.
    /// </summary>
    [JsonPropertyName("newFips")]
    public Fips NewFips
    {
      get => newFips ??= new();
      set => newFips = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of NewFipsTribAddress.
    /// </summary>
    [JsonPropertyName("newFipsTribAddress")]
    public FipsTribAddress NewFipsTribAddress
    {
      get => newFipsTribAddress ??= new();
      set => newFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingUsTribOrgz.
    /// </summary>
    [JsonPropertyName("existingUsTribOrgz")]
    public CsePerson ExistingUsTribOrgz
    {
      get => existingUsTribOrgz ??= new();
      set => existingUsTribOrgz = value;
    }

    private Tribunal overlapping;
    private Fips existingTemp;
    private Fips newFips;
    private Fips existingFips;
    private FipsTribAddress newFipsTribAddress;
    private FipsTribAddress existingFipsTribAddress;
    private Tribunal existingTribunal;
    private CsePerson existingUsTribOrgz;
  }
#endregion
}
