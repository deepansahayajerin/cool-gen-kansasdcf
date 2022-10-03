// Program: SI_READ_ORGANIZATIONS, ID: 371765747, model: 746.
// Short name: SWE01227
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_ORGANIZATIONS.
/// </summary>
[Serializable]
public partial class SiReadOrganizations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_ORGANIZATIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadOrganizations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadOrganizations.
  /// </summary>
  public SiReadOrganizations(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 09/06/95  Helen Sharland                   Initial Development
    // 05/04/96  Rao S Mulpuri	   IDCR# 45 Changes to Search logic
    // 05/05/97  Govind	   IDCR 252 Added filters State, County, City, ZIP. The 
    // logic for 'LIKE' orgz name was not working and was fixed.
    // ----------------------------------------------------------
    // 07/02/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 08/22/01 M.Brown      PR# 124003
    // Added sort descending end date on read each of address.
    // ------------------------------------------------------------
    // 05/22/02. PR 143825. Added check to ensure that the next search at the 
    // organization name search is searching for the next available organization
    // number, because organization names can be the same. LJB
    export.Export1.Index = -1;
    local.Dummy.Flag = "Y";

    if (AsChar(local.Dummy.Flag) == 'Y')
    {
      if (!IsEmpty(import.StartingSearch.Number))
      {
        foreach(var item in ReadCsePerson1())
        {
          if (!IsEmpty(import.SearchNamesLike.OrganizationName))
          {
            if (Find(String(
              entities.CsePerson.OrganizationName,
              CsePerson.OrganizationName_MaxLength),
              TrimEnd(import.SearchNamesLike.OrganizationName)) == 0)
            {
              continue;
            }
          }

          local.FipsFound.Flag = "N";
          local.FipsAddressFound.Flag = "N";
          local.CsePersonAddrFound.Flag = "N";

          // 07/02/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadFips2())
          {
            local.FipsFound.Flag = "Y";

            if (!IsEmpty(import.SearchFips.StateAbbreviation))
            {
              if (!Equal(entities.Fips.StateAbbreviation,
                import.SearchFips.StateAbbreviation))
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchFips.CountyAbbreviation))
            {
              if (!Equal(entities.Fips.CountyAbbreviation,
                import.SearchFips.CountyAbbreviation))
              {
                continue;
              }
            }

            if (ReadFipsTribAddress())
            {
              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.FipsTribAddress.City,
                  FipsTribAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.FipsTribAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.FipsAddressFound.Flag = "Y";
              local.FipsTribAddress.Assign(entities.FipsTribAddress);
            }
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'N')
          {
            // 08/22/01 M.Brown      PR# 124003
            if (ReadCsePersonAddress())
            {
              if (!IsEmpty(import.SearchFips.StateAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.State,
                  import.SearchFips.StateAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchFips.CountyAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.County,
                  import.SearchFips.CountyAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.CsePersonAddress.City,
                  CsePersonAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.CsePersonAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.CsePersonAddrFound.Flag = "Y";
              local.CsePersonAddress.Assign(entities.CsePersonAddress);
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation) || !
            IsEmpty(import.SearchFips.CountyAbbreviation))
          {
            if (AsChar(local.FipsFound.Flag) == 'N' && AsChar
              (local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchCsePersonAddress.City) || !
            IsEmpty(import.SearchCsePersonAddress.ZipCode))
          {
            if (AsChar(local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.DetailCsePerson.Assign(entities.CsePerson);

          if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.Assign(
              entities.CsePersonAddress);
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.City =
              local.FipsTribAddress.City;
            export.Export1.Update.DetailCsePersonAddress.ZipCode =
              local.FipsTribAddress.ZipCode;
            export.Export1.Update.DetailCsePersonAddress.Zip4 =
              local.FipsTribAddress.Zip4 ?? "";
          }

          if (AsChar(local.FipsFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.Assign(entities.Fips);

            if (ReadTribunal())
            {
              export.Export1.Update.DetailOrgzIsTrib.Flag = "Y";
            }
          }
          else if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.StateAbbreviation =
              local.CsePersonAddress.State ?? Spaces(2);
            export.Export1.Update.DetailFips.CountyAbbreviation =
              local.CsePersonAddress.County ?? "";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            export.Next.Number = entities.CsePerson.Number;

            break;
          }
        }

        // ---------------------------------------------
        // All the processing for starting cse person number is complete
        // ---------------------------------------------
        goto Test;
      }

      // 05/22/02. PR 143825. Added check to ensure that the next search at the 
      // organization name search is searching for the next available
      // organization number, because organization names can be the same. LJB
      if (!IsEmpty(import.StartingSearch.OrganizationName))
      {
        foreach(var item in ReadCsePerson2())
        {
          if (!IsEmpty(import.SearchNamesLike.OrganizationName))
          {
            if (Find(String(
              entities.CsePerson.OrganizationName,
              CsePerson.OrganizationName_MaxLength),
              TrimEnd(import.SearchNamesLike.OrganizationName)) == 0)
            {
              continue;
            }
          }

          local.FipsFound.Flag = "N";
          local.FipsAddressFound.Flag = "N";
          local.CsePersonAddrFound.Flag = "N";

          // 07/02/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadFips2())
          {
            local.FipsFound.Flag = "Y";

            if (!IsEmpty(import.SearchFips.StateAbbreviation))
            {
              if (!Equal(entities.Fips.StateAbbreviation,
                import.SearchFips.StateAbbreviation))
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchFips.CountyAbbreviation))
            {
              if (!Equal(entities.Fips.CountyAbbreviation,
                import.SearchFips.CountyAbbreviation))
              {
                continue;
              }
            }

            if (ReadFipsTribAddress())
            {
              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.FipsTribAddress.City,
                  FipsTribAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.FipsTribAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.FipsAddressFound.Flag = "Y";
              local.FipsTribAddress.Assign(entities.FipsTribAddress);
            }
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'N')
          {
            // 08/22/01 M.Brown      PR# 124003
            if (ReadCsePersonAddress())
            {
              if (!IsEmpty(import.SearchFips.StateAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.State,
                  import.SearchFips.StateAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchFips.CountyAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.County,
                  import.SearchFips.CountyAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.CsePersonAddress.City,
                  CsePersonAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.CsePersonAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.CsePersonAddrFound.Flag = "Y";
              local.CsePersonAddress.Assign(entities.CsePersonAddress);
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation) || !
            IsEmpty(import.SearchFips.CountyAbbreviation))
          {
            if (AsChar(local.FipsFound.Flag) == 'N' && AsChar
              (local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchCsePersonAddress.City) || !
            IsEmpty(import.SearchCsePersonAddress.ZipCode))
          {
            if (AsChar(local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.DetailCsePerson.Assign(entities.CsePerson);

          if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.Assign(
              entities.CsePersonAddress);
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.City =
              local.FipsTribAddress.City;
            export.Export1.Update.DetailCsePersonAddress.ZipCode =
              local.FipsTribAddress.ZipCode;
            export.Export1.Update.DetailCsePersonAddress.Zip4 =
              local.FipsTribAddress.Zip4 ?? "";
          }

          if (AsChar(local.FipsFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.Assign(entities.Fips);

            if (ReadTribunal())
            {
              export.Export1.Update.DetailOrgzIsTrib.Flag = "Y";
            }
          }
          else if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.StateAbbreviation =
              local.CsePersonAddress.State ?? Spaces(2);
            export.Export1.Update.DetailFips.CountyAbbreviation =
              local.CsePersonAddress.County ?? "";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            export.Next.OrganizationName = entities.CsePerson.OrganizationName;
            export.Next.Number = entities.CsePerson.Number;

            break;
          }
        }

        // ---------------------------------------------
        // All the processing for starting cse person organization name is 
        // complete
        // ---------------------------------------------
        goto Test;
      }

      if (!IsEmpty(import.StartingSearch.TaxId))
      {
        foreach(var item in ReadCsePerson3())
        {
          if (!IsEmpty(import.SearchNamesLike.OrganizationName))
          {
            if (Find(String(
              entities.CsePerson.OrganizationName,
              CsePerson.OrganizationName_MaxLength),
              TrimEnd(import.SearchNamesLike.OrganizationName)) == 0)
            {
              continue;
            }
          }

          local.FipsFound.Flag = "N";
          local.FipsAddressFound.Flag = "N";
          local.CsePersonAddrFound.Flag = "N";

          // 07/02/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadFips2())
          {
            local.FipsFound.Flag = "Y";

            if (!IsEmpty(import.SearchFips.StateAbbreviation))
            {
              if (!Equal(entities.Fips.StateAbbreviation,
                import.SearchFips.StateAbbreviation))
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchFips.CountyAbbreviation))
            {
              if (!Equal(entities.Fips.CountyAbbreviation,
                import.SearchFips.CountyAbbreviation))
              {
                continue;
              }
            }

            if (ReadFipsTribAddress())
            {
              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.FipsTribAddress.City,
                  FipsTribAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.FipsTribAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.FipsAddressFound.Flag = "Y";
              local.FipsTribAddress.Assign(entities.FipsTribAddress);
            }
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'N')
          {
            // 08/22/01 M.Brown      PR# 124003
            if (ReadCsePersonAddress())
            {
              if (!IsEmpty(import.SearchFips.StateAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.State,
                  import.SearchFips.StateAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchFips.CountyAbbreviation))
              {
                if (!Equal(entities.CsePersonAddress.County,
                  import.SearchFips.CountyAbbreviation))
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.City))
              {
                if (Find(String(
                  entities.CsePersonAddress.City,
                  CsePersonAddress.City_MaxLength),
                  TrimEnd(import.SearchCsePersonAddress.City)) == 0)
                {
                  continue;
                }
              }

              if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
              {
                if (Equal(entities.CsePersonAddress.ZipCode,
                  import.SearchCsePersonAddress.ZipCode))
                {
                }
                else
                {
                  continue;
                }
              }

              local.CsePersonAddrFound.Flag = "Y";
              local.CsePersonAddress.Assign(entities.CsePersonAddress);
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation) || !
            IsEmpty(import.SearchFips.CountyAbbreviation))
          {
            if (AsChar(local.FipsFound.Flag) == 'N' && AsChar
              (local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchCsePersonAddress.City) || !
            IsEmpty(import.SearchCsePersonAddress.ZipCode))
          {
            if (AsChar(local.FipsAddressFound.Flag) == 'N' && AsChar
              (local.CsePersonAddrFound.Flag) == 'N')
            {
              continue;
            }
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.DetailCsePerson.Assign(entities.CsePerson);

          if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.Assign(
              entities.CsePersonAddress);
          }

          if (AsChar(local.FipsAddressFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailCsePersonAddress.City =
              local.FipsTribAddress.City;
            export.Export1.Update.DetailCsePersonAddress.ZipCode =
              local.FipsTribAddress.ZipCode;
            export.Export1.Update.DetailCsePersonAddress.Zip4 =
              local.FipsTribAddress.Zip4 ?? "";
          }

          if (AsChar(local.FipsFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.Assign(entities.Fips);

            if (ReadTribunal())
            {
              export.Export1.Update.DetailOrgzIsTrib.Flag = "Y";
            }
          }
          else if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
          {
            export.Export1.Update.DetailFips.StateAbbreviation =
              local.CsePersonAddress.State ?? Spaces(2);
            export.Export1.Update.DetailFips.CountyAbbreviation =
              local.CsePersonAddress.County ?? "";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            export.Next.TaxId = entities.CsePerson.TaxId;

            break;
          }
        }

        // ---------------------------------------------
        // All the processing for starting cse person Tax ID is complete
        // ---------------------------------------------
        goto Test;
      }

      // ---------------------------------------------
      // In all other situations
      // ---------------------------------------------
      // 5-22-02. PR143825. Added check to ensure next organization number is 
      // greater than the starting search one.
      foreach(var item in ReadCsePerson2())
      {
        if (!IsEmpty(import.SearchNamesLike.OrganizationName))
        {
          if (Find(String(
            entities.CsePerson.OrganizationName,
            CsePerson.OrganizationName_MaxLength),
            TrimEnd(import.SearchNamesLike.OrganizationName)) == 0)
          {
            continue;
          }
        }

        local.FipsFound.Flag = "N";
        local.FipsAddressFound.Flag = "N";
        local.CsePersonAddrFound.Flag = "N";

        // 07/02/99 M.L         Change property of READ to generate
        //                      Select Only
        // ------------------------------------------------------------
        if (ReadFips2())
        {
          local.FipsFound.Flag = "Y";

          if (!IsEmpty(import.SearchFips.StateAbbreviation))
          {
            if (!Equal(entities.Fips.StateAbbreviation,
              import.SearchFips.StateAbbreviation))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchFips.CountyAbbreviation))
          {
            if (!Equal(entities.Fips.CountyAbbreviation,
              import.SearchFips.CountyAbbreviation))
            {
              continue;
            }
          }

          if (ReadFipsTribAddress())
          {
            if (!IsEmpty(import.SearchCsePersonAddress.City))
            {
              if (Find(String(
                entities.FipsTribAddress.City, FipsTribAddress.City_MaxLength),
                TrimEnd(import.SearchCsePersonAddress.City)) == 0)
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
            {
              if (Equal(entities.FipsTribAddress.ZipCode,
                import.SearchCsePersonAddress.ZipCode))
              {
              }
              else
              {
                continue;
              }
            }

            local.FipsAddressFound.Flag = "Y";
            local.FipsTribAddress.Assign(entities.FipsTribAddress);
          }
        }

        if (AsChar(local.FipsAddressFound.Flag) == 'N')
        {
          // 08/22/01 M.Brown      PR# 124003
          if (ReadCsePersonAddress())
          {
            if (!IsEmpty(import.SearchFips.StateAbbreviation))
            {
              if (!Equal(entities.CsePersonAddress.State,
                import.SearchFips.StateAbbreviation))
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchFips.CountyAbbreviation))
            {
              if (!Equal(entities.CsePersonAddress.County,
                import.SearchFips.CountyAbbreviation))
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchCsePersonAddress.City))
            {
              if (Find(String(
                entities.CsePersonAddress.City,
                CsePersonAddress.City_MaxLength),
                TrimEnd(import.SearchCsePersonAddress.City)) == 0)
              {
                continue;
              }
            }

            if (!IsEmpty(import.SearchCsePersonAddress.ZipCode))
            {
              if (Equal(entities.CsePersonAddress.ZipCode,
                import.SearchCsePersonAddress.ZipCode))
              {
              }
              else
              {
                continue;
              }
            }

            local.CsePersonAddrFound.Flag = "Y";
            local.CsePersonAddress.Assign(entities.CsePersonAddress);
          }
        }

        if (!IsEmpty(import.SearchFips.StateAbbreviation) || !
          IsEmpty(import.SearchFips.CountyAbbreviation))
        {
          if (AsChar(local.FipsFound.Flag) == 'N' && AsChar
            (local.FipsAddressFound.Flag) == 'N' && AsChar
            (local.CsePersonAddrFound.Flag) == 'N')
          {
            continue;
          }
        }

        if (!IsEmpty(import.SearchCsePersonAddress.City) || !
          IsEmpty(import.SearchCsePersonAddress.ZipCode))
        {
          if (AsChar(local.FipsAddressFound.Flag) == 'N' && AsChar
            (local.CsePersonAddrFound.Flag) == 'N')
          {
            continue;
          }
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailCsePerson.Assign(entities.CsePerson);

        if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
        {
          export.Export1.Update.DetailCsePersonAddress.Assign(
            entities.CsePersonAddress);
        }

        if (AsChar(local.FipsAddressFound.Flag) == 'Y')
        {
          export.Export1.Update.DetailCsePersonAddress.City =
            local.FipsTribAddress.City;
          export.Export1.Update.DetailCsePersonAddress.ZipCode =
            local.FipsTribAddress.ZipCode;
          export.Export1.Update.DetailCsePersonAddress.Zip4 =
            local.FipsTribAddress.Zip4 ?? "";
        }

        if (ReadFips1())
        {
          export.Export1.Update.DetailFips.Assign(entities.Fips);

          if (ReadTribunal())
          {
            export.Export1.Update.DetailOrgzIsTrib.Flag = "Y";
          }
        }
        else if (AsChar(local.CsePersonAddrFound.Flag) == 'Y')
        {
          export.Export1.Update.DetailFips.StateAbbreviation =
            local.CsePersonAddress.State ?? Spaces(2);
          export.Export1.Update.DetailFips.CountyAbbreviation =
            local.CsePersonAddress.County ?? "";
        }

        if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
        {
          export.Next.OrganizationName = entities.CsePerson.OrganizationName;
          export.Next.Number = entities.CsePerson.Number;

          break;
        }
      }
    }

Test:

    if (import.Standard.PageNumber == 1)
    {
      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private IEnumerable<bool> ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb1", import.PageKey.Number);
        db.SetString(command, "numb2", import.StartingSearch.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "organizationName1", import.PageKey.OrganizationName ?? "");
        db.SetString(command, "numb1", import.PageKey.Number);
        db.SetNullableString(
          command, "organizationName2",
          import.StartingSearch.OrganizationName ?? "");
        db.SetString(command, "numb2", import.StartingSearch.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "taxId1", import.PageKey.TaxId ?? "");
        db.SetNullableString(
          command, "taxId2", import.StartingSearch.TaxId ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 7);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.CspNumber = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.CspNumber = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.City = db.GetString(reader, 1);
        entities.FipsTribAddress.State = db.GetString(reader, 2);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 3);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 4);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 5);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 6);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 7);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 8);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 5);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 6);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 7);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Populated = true;
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
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of SearchNamesLike.
    /// </summary>
    [JsonPropertyName("searchNamesLike")]
    public CsePerson SearchNamesLike
    {
      get => searchNamesLike ??= new();
      set => searchNamesLike = value;
    }

    /// <summary>
    /// A value of SearchCsePersonAddress.
    /// </summary>
    [JsonPropertyName("searchCsePersonAddress")]
    public CsePersonAddress SearchCsePersonAddress
    {
      get => searchCsePersonAddress ??= new();
      set => searchCsePersonAddress = value;
    }

    /// <summary>
    /// A value of StartingSearch.
    /// </summary>
    [JsonPropertyName("startingSearch")]
    public CsePerson StartingSearch
    {
      get => startingSearch ??= new();
      set => startingSearch = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public CsePerson PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
    }

    private Fips searchFips;
    private CsePerson searchNamesLike;
    private CsePersonAddress searchCsePersonAddress;
    private CsePerson startingSearch;
    private Standard standard;
    private CsePerson pageKey;
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
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailPromptFips.
      /// </summary>
      [JsonPropertyName("detailPromptFips")]
      public Standard DetailPromptFips
      {
        get => detailPromptFips ??= new();
        set => detailPromptFips = value;
      }

      /// <summary>
      /// A value of DetailOrgzIsTrib.
      /// </summary>
      [JsonPropertyName("detailOrgzIsTrib")]
      public Common DetailOrgzIsTrib
      {
        get => detailOrgzIsTrib ??= new();
        set => detailOrgzIsTrib = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private CsePersonAddress detailCsePersonAddress;
      private Common detailCommon;
      private CsePerson detailCsePerson;
      private Fips detailFips;
      private Standard detailPromptFips;
      private Common detailOrgzIsTrib;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public CsePerson Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Standard standard;
    private CsePerson next;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of FipsAddressFound.
    /// </summary>
    [JsonPropertyName("fipsAddressFound")]
    public Common FipsAddressFound
    {
      get => fipsAddressFound ??= new();
      set => fipsAddressFound = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of FipsFound.
    /// </summary>
    [JsonPropertyName("fipsFound")]
    public Common FipsFound
    {
      get => fipsFound ??= new();
      set => fipsFound = value;
    }

    /// <summary>
    /// A value of CsePersonAddrFound.
    /// </summary>
    [JsonPropertyName("csePersonAddrFound")]
    public Common CsePersonAddrFound
    {
      get => csePersonAddrFound ??= new();
      set => csePersonAddrFound = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Common Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePerson Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Filler.
    /// </summary>
    [JsonPropertyName("filler")]
    public CsePerson Filler
    {
      get => filler ??= new();
      set => filler = value;
    }

    private Common dummy;
    private Common fipsAddressFound;
    private CsePersonAddress csePersonAddress;
    private FipsTribAddress fipsTribAddress;
    private Common fipsFound;
    private Common csePersonAddrFound;
    private Common blank;
    private CsePerson work;
    private CsePerson filler;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private CsePersonAddress csePersonAddress;
    private Fips fips;
    private CsePerson csePerson;
  }
#endregion
}
