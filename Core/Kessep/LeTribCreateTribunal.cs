// Program: LE_TRIB_CREATE_TRIBUNAL, ID: 372021819, model: 746.
// Short name: SWE00821
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
/// A program: LE_TRIB_CREATE_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates a tribunal and associated entity types' 
/// occurrences.
/// </para>
/// </summary>
[Serializable]
public partial class LeTribCreateTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_CREATE_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribCreateTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribCreateTribunal.
  /// </summary>
  public LeTribCreateTribunal(IContext context, Import import, Export export):
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
    // 10/08/98	D. Jean     			Moved logic to before the Create Tribunal 
    // eliminating I/O, issued new exit state if entered address is not used
    // because FIPS address already exists, in create statement supplied default
    // values for all mandatory attributes
    // 09/15/2000	GVandy		PR 102557	Added tribunal document header attributes.
    // *******************************************************************
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      MoveFipsTribAddress2(import.Import1.Item.Detail,
        export.Export1.Update.Detail);
      export.Export1.Update.DetailListAddrTp.PromptField =
        import.Import1.Item.DetailListAddrTp.PromptField;
      export.Export1.Update.DetailListStates.PromptField =
        import.Import1.Item.DetailListStates.PromptField;
      export.Export1.Update.DetailSelAddr.SelectChar =
        import.Import1.Item.DetailSelAddr.SelectChar;
      export.Export1.Next();
    }

    export.FipsTribAddress.Country = import.FipsTribAddress.Country;

    if (import.Fips.State > 0 || import.Fips.County > 0 || import
      .Fips.Location > 0)
    {
      if (ReadFips())
      {
        export.Fips.Assign(entities.Existing);
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }
    }

    if (entities.Existing.Populated)
    {
      if (ReadTribunal2())
      {
        ExitState = "LE0000_TRIB_AE_FOR_FIPS";

        return;
      }
    }

    ReadTribunal1();
    MoveTribunal(import.Tribunal, local.New1);

    if (entities.Existing.Populated)
    {
      if (ReadCsePerson())
      {
        local.NewOrgz.Assign(entities.ExistingUsTribOrgz);

        // --- Now compare and modify the values
        if (!IsEmpty(import.Tribunal.Name))
        {
          if (!Equal(entities.ExistingUsTribOrgz.OrganizationName,
            import.Tribunal.Name))
          {
            local.NewOrgz.OrganizationName = import.Tribunal.Name;
          }
        }
        else if (!IsEmpty(entities.ExistingUsTribOrgz.OrganizationName))
        {
          local.New1.Name = entities.ExistingUsTribOrgz.OrganizationName ?? Spaces
            (30);
        }

        if (!IsEmpty(import.Tribunal.TaxId))
        {
          if (!Equal(entities.ExistingUsTribOrgz.TaxId, import.Tribunal.TaxId))
          {
            local.NewOrgz.TaxId = import.Tribunal.TaxId ?? "";
          }
        }
        else if (!IsEmpty(entities.ExistingUsTribOrgz.TaxId))
        {
          local.New1.TaxId = entities.ExistingUsTribOrgz.TaxId;
        }

        if (!IsEmpty(import.Tribunal.TaxIdSuffix))
        {
          if (!Equal(entities.ExistingUsTribOrgz.TaxIdSuffix,
            import.Tribunal.TaxIdSuffix))
          {
            local.NewOrgz.TaxIdSuffix = import.Tribunal.TaxIdSuffix ?? "";
          }
        }
        else if (!IsEmpty(entities.ExistingUsTribOrgz.TaxIdSuffix))
        {
          local.New1.TaxIdSuffix = entities.ExistingUsTribOrgz.TaxIdSuffix;
        }

        local.NewOrgz.Type1 = entities.ExistingUsTribOrgz.Type1;
        local.NewOrgz.Number = entities.ExistingUsTribOrgz.Number;
        UseSiUpdateCsePerson();
      }
      else
      {
        local.NewOrgz.Type1 = "O";
        local.NewOrgz.OrganizationName = import.Tribunal.Name;
        local.NewOrgz.TaxId = import.Tribunal.TaxId ?? "";
        local.NewOrgz.TaxIdSuffix = import.Tribunal.TaxIdSuffix ?? "";
        UseSiCreateCsePerson();
      }
    }

    try
    {
      CreateTribunal();

      if (entities.Existing.Populated)
      {
        AssociateTribunal1();
      }

      export.Tribunal.Assign(entities.NewTribunal);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "TRIBUNAL_AE";

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

    if (entities.Existing.Populated)
    {
      // --- The tribunal has a fips associated with it. So the address cannot 
      // be added here. Populate the export with fips address
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadFipsTribAddress2())
      {
        AssociateTribunal2();
        MoveFipsTribAddress1(entities.NewFipsTribAddress,
          export.Export1.Update.Detail);
        export.Export1.Next();
      }

      ExitState = "LE0000_TRIB_ADDRESS_NOT_CREATED";

      return;
    }

    // --- The tribunal has no fips. So create address using the address 
    // entered.
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (IsEmpty(export.Export1.Item.DetailSelAddr.SelectChar))
      {
        continue;
      }

      ReadFipsTribAddress1();

      try
      {
        CreateFipsTribAddress();
        MoveFipsTribAddress1(entities.NewFipsTribAddress,
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
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveFipsTribAddress1(FipsTribAddress source,
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

  private static void MoveFipsTribAddress2(FipsTribAddress source,
    FipsTribAddress target)
  {
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

  private void UseSiCreateCsePerson()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    useImport.Fips.Assign(entities.Existing);
    MoveCsePerson(local.NewOrgz, useImport.CsePerson);

    Call(SiCreateCsePerson.Execute, useImport, useExport);

    local.NewOrgz.Number = useExport.CsePerson.Number;
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson(local.NewOrgz, useImport.CsePerson);
    useImport.Fips.Assign(export.Fips);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void AssociateTribunal1()
  {
    var fipLocation = entities.Existing.Location;
    var fipCounty = entities.Existing.County;
    var fipState = entities.Existing.State;

    entities.NewTribunal.Populated = false;
    Update("AssociateTribunal1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", fipLocation);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipState", fipState);
        db.SetInt32(command, "identifier", entities.NewTribunal.Identifier);
      });

    entities.NewTribunal.FipLocation = fipLocation;
    entities.NewTribunal.FipCounty = fipCounty;
    entities.NewTribunal.FipState = fipState;
    entities.NewTribunal.Populated = true;
  }

  private void AssociateTribunal2()
  {
    var trbId = entities.NewTribunal.Identifier;

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
    var identifier = local.LastFipsTribAddress.Identifier + 1;
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
    var country = export.FipsTribAddress.Country ?? "";
    var phoneNumber =
      export.Export1.Item.Detail.PhoneNumber.GetValueOrDefault();
    var faxNumber = export.Export1.Item.Detail.FaxNumber.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var lastUpdatedTstamp = local.InitialisedToZeros.LastUpdatedTstamp;
    var trbId = entities.NewTribunal.Identifier;

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
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
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
    entities.NewFipsTribAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.NewFipsTribAddress.FipState = null;
    entities.NewFipsTribAddress.FipCounty = null;
    entities.NewFipsTribAddress.FipLocation = null;
    entities.NewFipsTribAddress.TrbId = trbId;
    entities.NewFipsTribAddress.Populated = true;
  }

  private void CreateTribunal()
  {
    var judicialDivision = import.Tribunal.JudicialDivision ?? "";
    var name = local.New1.Name;
    var judicialDistrict = import.Tribunal.JudicialDistrict;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var lastUpdatedTstamp = local.InitialisedToZeros.LastUpdatedTstamp;
    var identifier = local.LastTribunal.Identifier + 1;
    var taxIdSuffix = local.New1.TaxIdSuffix ?? "";
    var taxId = local.New1.TaxId ?? "";
    var documentHeader1 = import.Tribunal.DocumentHeader1 ?? "";
    var documentHeader2 = import.Tribunal.DocumentHeader2 ?? "";
    var documentHeader3 = import.Tribunal.DocumentHeader3 ?? "";
    var documentHeader4 = import.Tribunal.DocumentHeader4 ?? "";
    var documentHeader5 = import.Tribunal.DocumentHeader5 ?? "";
    var documentHeader6 = import.Tribunal.DocumentHeader6 ?? "";

    entities.NewTribunal.Populated = false;
    Update("CreateTribunal",
      (db, command) =>
      {
        db.SetNullableString(command, "judicialDivision", judicialDivision);
        db.SetString(command, "tribunalNm", name);
        db.SetString(command, "judicialDistrict", judicialDistrict);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "taxIdSuffix", taxIdSuffix);
        db.SetNullableString(command, "taxId", taxId);
        db.SetNullableString(command, "documentHeader1", documentHeader1);
        db.SetNullableString(command, "documentHeader2", documentHeader2);
        db.SetNullableString(command, "documentHeader3", documentHeader3);
        db.SetNullableString(command, "documentHeader4", documentHeader4);
        db.SetNullableString(command, "documentHeader5", documentHeader5);
        db.SetNullableString(command, "documentHeader6", documentHeader6);
      });

    entities.NewTribunal.JudicialDivision = judicialDivision;
    entities.NewTribunal.Name = name;
    entities.NewTribunal.FipLocation = null;
    entities.NewTribunal.JudicialDistrict = judicialDistrict;
    entities.NewTribunal.CreatedBy = createdBy;
    entities.NewTribunal.CreatedTstamp = createdTstamp;
    entities.NewTribunal.LastUpdatedBy = "";
    entities.NewTribunal.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.NewTribunal.Identifier = identifier;
    entities.NewTribunal.TaxIdSuffix = taxIdSuffix;
    entities.NewTribunal.TaxId = taxId;
    entities.NewTribunal.DocumentHeader1 = documentHeader1;
    entities.NewTribunal.DocumentHeader2 = documentHeader2;
    entities.NewTribunal.DocumentHeader3 = documentHeader3;
    entities.NewTribunal.DocumentHeader4 = documentHeader4;
    entities.NewTribunal.DocumentHeader5 = documentHeader5;
    entities.NewTribunal.DocumentHeader6 = documentHeader6;
    entities.NewTribunal.FipCounty = null;
    entities.NewTribunal.FipState = null;
    entities.NewTribunal.Populated = true;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingUsTribOrgz.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspNumber ?? "");
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

  private bool ReadFips()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.StateAbbreviation = db.GetString(reader, 6);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Existing.CspNumber = db.GetNullableString(reader, 8);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    local.LastFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      null,
      (db, reader) =>
      {
        local.LastFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        local.LastFipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress2()
  {
    return ReadEach("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
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
    local.LastTribunal.Populated = false;

    return Read("ReadTribunal1",
      null,
      (db, reader) =>
      {
        local.LastTribunal.Identifier = db.GetInt32(reader, 0);
        local.LastTribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.NewTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
      },
      (db, reader) =>
      {
        entities.NewTribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.NewTribunal.Name = db.GetString(reader, 1);
        entities.NewTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.NewTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.NewTribunal.CreatedBy = db.GetString(reader, 4);
        entities.NewTribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.NewTribunal.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.NewTribunal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.NewTribunal.Identifier = db.GetInt32(reader, 8);
        entities.NewTribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.NewTribunal.TaxId = db.GetNullableString(reader, 10);
        entities.NewTribunal.DocumentHeader1 = db.GetNullableString(reader, 11);
        entities.NewTribunal.DocumentHeader2 = db.GetNullableString(reader, 12);
        entities.NewTribunal.DocumentHeader3 = db.GetNullableString(reader, 13);
        entities.NewTribunal.DocumentHeader4 = db.GetNullableString(reader, 14);
        entities.NewTribunal.DocumentHeader5 = db.GetNullableString(reader, 15);
        entities.NewTribunal.DocumentHeader6 = db.GetNullableString(reader, 16);
        entities.NewTribunal.FipCounty = db.GetNullableInt32(reader, 17);
        entities.NewTribunal.FipState = db.GetNullableInt32(reader, 18);
        entities.NewTribunal.Populated = true;
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

    private FipsTribAddress fipsTribAddress;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Tribunal New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LastFipsTribAddress.
    /// </summary>
    [JsonPropertyName("lastFipsTribAddress")]
    public FipsTribAddress LastFipsTribAddress
    {
      get => lastFipsTribAddress ??= new();
      set => lastFipsTribAddress = value;
    }

    /// <summary>
    /// A value of LastTribunal.
    /// </summary>
    [JsonPropertyName("lastTribunal")]
    public Tribunal LastTribunal
    {
      get => lastTribunal ??= new();
      set => lastTribunal = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public Tribunal InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private Tribunal new1;
    private CsePerson newOrgz;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FipsTribAddress lastFipsTribAddress;
    private Tribunal lastTribunal;
    private Tribunal initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingUsTribOrgz.
    /// </summary>
    [JsonPropertyName("existingUsTribOrgz")]
    public CsePerson ExistingUsTribOrgz
    {
      get => existingUsTribOrgz ??= new();
      set => existingUsTribOrgz = value;
    }

    /// <summary>
    /// A value of ExistingLastFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingLastFipsTribAddress")]
    public FipsTribAddress ExistingLastFipsTribAddress
    {
      get => existingLastFipsTribAddress ??= new();
      set => existingLastFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingLastTribunal.
    /// </summary>
    [JsonPropertyName("existingLastTribunal")]
    public Tribunal ExistingLastTribunal
    {
      get => existingLastTribunal ??= new();
      set => existingLastTribunal = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of NewTribunal.
    /// </summary>
    [JsonPropertyName("newTribunal")]
    public Tribunal NewTribunal
    {
      get => newTribunal ??= new();
      set => newTribunal = value;
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

    private CsePerson existingUsTribOrgz;
    private FipsTribAddress existingLastFipsTribAddress;
    private Tribunal existingLastTribunal;
    private Fips existing;
    private Tribunal newTribunal;
    private FipsTribAddress newFipsTribAddress;
  }
#endregion
}
