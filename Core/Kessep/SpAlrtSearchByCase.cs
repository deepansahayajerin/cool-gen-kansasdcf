// Program: SP_ALRT_SEARCH_BY_CASE, ID: 1902640516, model: 746.
// Short name: SWE01281
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ALRT_SEARCH_BY_CASE.
/// </summary>
[Serializable]
public partial class SpAlrtSearchByCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ALRT_SEARCH_BY_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAlrtSearchByCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAlrtSearchByCase.
  /// </summary>
  public SpAlrtSearchByCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Filter.EventType = import.FilterCodeValue.Cdvalue;
    local.Filter.DenormText12 = import.FilterLegalAction.CourtCaseNumber ?? "";
    export.Export1.Index = -1;
    export.Export1.Count = 0;

    switch(TrimEnd(import.SortDesc.Cdvalue))
    {
      case "DA":
        // -- Sort by Date Ascending
        foreach(var item in ReadInfrastructureOfficeServiceProviderAlert3())
        {
          MoveLegalAction(local.InitialLa, local.La);
          MoveCsePersonsWorkSet(local.Initial, local.CsePersonsWorkSet);

          // -- Apply additional filtering criteria
          if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
          {
            if (!Equal(entities.Infrastructure.CsePersonNumber,
              import.FilterInfrastructure.CsePersonNumber))
            {
              continue;
            }
          }

          if (!IsEmpty(local.Filter.EventType))
          {
            if (!Equal(entities.Infrastructure.EventType, local.Filter.EventType))
              
            {
              continue;
            }
          }

          if (!IsEmpty(import.FilterOfficeServiceProviderAlert.Message))
          {
            if (!Equal(entities.OfficeServiceProviderAlert.Message,
              import.FilterOfficeServiceProviderAlert.Message))
            {
              continue;
            }
          }

          if (import.FilterOffice.SystemGeneratedId != 0 || import
            .FilterServiceProvider.SystemGeneratedId != 0 || !
            IsEmpty(import.FilterServiceProvider.UserId) || !
            IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
          {
            if (!ReadOfficeOfficeServiceProviderServiceProvider())
            {
              continue;
            }

            if (import.FilterOffice.SystemGeneratedId != 0)
            {
              if (entities.Office.SystemGeneratedId != import
                .FilterOffice.SystemGeneratedId)
              {
                continue;
              }
            }

            if (import.FilterServiceProvider.SystemGeneratedId != 0)
            {
              if (entities.ServiceProvider.SystemGeneratedId != import
                .FilterServiceProvider.SystemGeneratedId)
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterServiceProvider.UserId))
            {
              if (!Equal(entities.ServiceProvider.UserId,
                import.FilterServiceProvider.UserId))
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
            {
              if (!Equal(entities.OfficeServiceProvider.RoleCode,
                import.FilterOfficeServiceProvider.RoleCode))
              {
                continue;
              }
            }

            if (!Equal(import.FilterOfficeServiceProvider.EffectiveDate,
              local.Null1.Date))
            {
              if (!Equal(entities.OfficeServiceProvider.EffectiveDate,
                import.FilterOfficeServiceProvider.EffectiveDate))
              {
                continue;
              }
            }
          }

          if (!IsEmpty(local.Filter.DenormText12))
          {
            if (!Equal(entities.Infrastructure.DenormText12,
              local.Filter.DenormText12))
            {
              // --The court order number does not match the import filter value
              // requested.
              continue;
            }

            // --Determine if the tribunal matches the import filter values.
            switch(TrimEnd(entities.OfficeServiceProviderAlert.TypeCode))
            {
              case "AUT":
                local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
                local.La.Identifier =
                  (int)entities.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
                  Equal(import.FilterFipsTribAddress.Country, "US"))
                {
                  if (ReadLegalAction2())
                  {
                    local.La.CourtCaseNumber =
                      entities.LegalAction.CourtCaseNumber;
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (ReadLegalAction1())
                {
                  local.La.CourtCaseNumber =
                    entities.LegalAction.CourtCaseNumber;
                }
                else
                {
                  continue;
                }

                break;
              case "MAN":
                // -- Manual alerts store the Tribunal Info (Country or State/
                // County) appended to
                //    the infrastructure detail attribute preceeded by a 
                // semicolon.
                if (Find(entities.Infrastructure.Detail, ";") == 0)
                {
                  // --Semicolon was not found in the infrastructure detail.  
                  // There is no tribunal info
                  //   stored in the detail attribute.  Skip this alert.
                  continue;
                }
                else
                {
                  local.Fips.Text4 =
                    Substring(entities.Infrastructure.Detail,
                    Find(entities.Infrastructure.Detail, ";") + 1, 4);

                  switch(Length(TrimEnd(local.Fips.Text4)))
                  {
                    case 2:
                      // --This is a foreign tribunal.  The 2 digit country 
                      // abbreviation is stored on the detail.
                      if (!IsEmpty(import.FilterFipsTribAddress.Country) && Equal
                        (import.FilterFipsTribAddress.Country, local.Fips.Text4,
                        1, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    case 4:
                      // --This is a domestic tribunal.  The 2 digit state 
                      // abbreviation is stored on the
                      //   detail followed by the 2 digit county abbreviation.
                      if (!IsEmpty(import.FilterFips.StateAbbreviation) && Equal
                        (import.FilterFips.StateAbbreviation, local.Fips.Text4,
                        1, 2) && !
                        IsEmpty(import.FilterFips.CountyAbbreviation) && Equal
                        (import.FilterFips.CountyAbbreviation, local.Fips.Text4,
                        3, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    default:
                      // --There are an unrecognizable number of characters.  
                      // Skip the alert.
                      //   This shouldn't happen.
                      continue;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA") || Equal
            (entities.OfficeServiceProviderAlert.TypeCode, "MAN"))
          {
            local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
            local.La.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();
          }

          if (!IsEmpty(entities.Infrastructure.CsePersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              entities.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              local.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            local.CsePersonsWorkSet.FormattedName = "";
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveOfficeServiceProviderAlert(entities.OfficeServiceProviderAlert,
            export.Export1.Update.GxfficeServiceProviderAlert);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.La.CourtCaseNumber;
          export.Export1.Update.Gxnfrastructure.Assign(entities.Infrastructure);
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.Export1.Item.GxfficeServiceProviderAlert.
            SystemGeneratedIdentifier == import
            .RepositionRecord.SystemGeneratedIdentifier)
          {
            export.Export1.Update.Gxommon.SelectChar = "S";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        break;
      case "DD":
        // -- Sort by Date Descending
        foreach(var item in ReadInfrastructureOfficeServiceProviderAlert5())
        {
          MoveLegalAction(local.InitialLa, local.La);
          MoveCsePersonsWorkSet(local.Initial, local.CsePersonsWorkSet);

          // -- Apply additional filtering criteria
          if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
          {
            if (!Equal(entities.Infrastructure.CsePersonNumber,
              import.FilterInfrastructure.CsePersonNumber))
            {
              continue;
            }
          }

          if (!IsEmpty(local.Filter.EventType))
          {
            if (!Equal(entities.Infrastructure.EventType, local.Filter.EventType))
              
            {
              continue;
            }
          }

          if (!IsEmpty(import.FilterOfficeServiceProviderAlert.Message))
          {
            if (!Equal(entities.OfficeServiceProviderAlert.Message,
              import.FilterOfficeServiceProviderAlert.Message))
            {
              continue;
            }
          }

          if (import.FilterOffice.SystemGeneratedId != 0 || import
            .FilterServiceProvider.SystemGeneratedId != 0 || !
            IsEmpty(import.FilterServiceProvider.UserId) || !
            IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
          {
            if (!ReadOfficeOfficeServiceProviderServiceProvider())
            {
              continue;
            }

            if (import.FilterOffice.SystemGeneratedId != 0)
            {
              if (entities.Office.SystemGeneratedId != import
                .FilterOffice.SystemGeneratedId)
              {
                continue;
              }
            }

            if (import.FilterServiceProvider.SystemGeneratedId != 0)
            {
              if (entities.ServiceProvider.SystemGeneratedId != import
                .FilterServiceProvider.SystemGeneratedId)
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterServiceProvider.UserId))
            {
              if (!Equal(entities.ServiceProvider.UserId,
                import.FilterServiceProvider.UserId))
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
            {
              if (!Equal(entities.OfficeServiceProvider.RoleCode,
                import.FilterOfficeServiceProvider.RoleCode))
              {
                continue;
              }
            }

            if (!Equal(import.FilterOfficeServiceProvider.EffectiveDate,
              local.Null1.Date))
            {
              if (!Equal(entities.OfficeServiceProvider.EffectiveDate,
                import.FilterOfficeServiceProvider.EffectiveDate))
              {
                continue;
              }
            }
          }

          if (!IsEmpty(local.Filter.DenormText12))
          {
            if (!Equal(entities.Infrastructure.DenormText12,
              local.Filter.DenormText12))
            {
              // --The court order number does not match the import filter value
              // requested.
              continue;
            }

            // --Determine if the tribunal matches the import filter values.
            switch(TrimEnd(entities.OfficeServiceProviderAlert.TypeCode))
            {
              case "AUT":
                local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
                local.La.Identifier =
                  (int)entities.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
                  Equal(import.FilterFipsTribAddress.Country, "US"))
                {
                  if (ReadLegalAction2())
                  {
                    local.La.CourtCaseNumber =
                      entities.LegalAction.CourtCaseNumber;
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (ReadLegalAction1())
                {
                  local.La.CourtCaseNumber =
                    entities.LegalAction.CourtCaseNumber;
                }
                else
                {
                  continue;
                }

                break;
              case "MAN":
                // -- Manual alerts store the Tribunal Info (Country or State/
                // County) appended to
                //    the infrastructure detail attribute preceeded by a 
                // semicolon.
                if (Find(entities.Infrastructure.Detail, ";") == 0)
                {
                  // --Semicolon was not found in the infrastructure detail.  
                  // There is no tribunal info
                  //   stored in the detail attribute.  Skip this alert.
                  continue;
                }
                else
                {
                  local.Fips.Text4 =
                    Substring(entities.Infrastructure.Detail,
                    Find(entities.Infrastructure.Detail, ";") + 1, 4);

                  switch(Length(TrimEnd(local.Fips.Text4)))
                  {
                    case 2:
                      // --This is a foreign tribunal.  The 2 digit country 
                      // abbreviation is stored on the detail.
                      if (!IsEmpty(import.FilterFipsTribAddress.Country) && Equal
                        (import.FilterFipsTribAddress.Country, local.Fips.Text4,
                        1, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    case 4:
                      // --This is a domestic tribunal.  The 2 digit state 
                      // abbreviation is stored on the
                      //   detail followed by the 2 digit county abbreviation.
                      if (!IsEmpty(import.FilterFips.StateAbbreviation) && Equal
                        (import.FilterFips.StateAbbreviation, local.Fips.Text4,
                        1, 2) && !
                        IsEmpty(import.FilterFips.CountyAbbreviation) && Equal
                        (import.FilterFips.CountyAbbreviation, local.Fips.Text4,
                        3, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    default:
                      // --There are an unrecognizable number of characters.  
                      // Skip the alert.
                      //   This shouldn't happen.
                      continue;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA") || Equal
            (entities.OfficeServiceProviderAlert.TypeCode, "MAN"))
          {
            local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
            local.La.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();
          }

          if (!IsEmpty(entities.Infrastructure.CsePersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              entities.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              local.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            local.CsePersonsWorkSet.FormattedName = "";
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveOfficeServiceProviderAlert(entities.OfficeServiceProviderAlert,
            export.Export1.Update.GxfficeServiceProviderAlert);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.La.CourtCaseNumber;
          export.Export1.Update.Gxnfrastructure.Assign(entities.Infrastructure);
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.Export1.Item.GxfficeServiceProviderAlert.
            SystemGeneratedIdentifier == import
            .RepositionRecord.SystemGeneratedIdentifier)
          {
            export.Export1.Update.Gxommon.SelectChar = "S";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        break;
      case "AN":
        // -- Sort by Alert Name Ascending
        foreach(var item in ReadInfrastructureOfficeServiceProviderAlert4())
        {
          MoveLegalAction(local.InitialLa, local.La);
          MoveCsePersonsWorkSet(local.Initial, local.CsePersonsWorkSet);

          // -- Apply additional filtering criteria
          if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
          {
            if (!Equal(entities.Infrastructure.CsePersonNumber,
              import.FilterInfrastructure.CsePersonNumber))
            {
              continue;
            }
          }

          if (!IsEmpty(local.Filter.EventType))
          {
            if (!Equal(entities.Infrastructure.EventType, local.Filter.EventType))
              
            {
              continue;
            }
          }

          if (!IsEmpty(import.FilterOfficeServiceProviderAlert.Message))
          {
            if (!Equal(entities.OfficeServiceProviderAlert.Message,
              import.FilterOfficeServiceProviderAlert.Message))
            {
              continue;
            }
          }

          if (import.FilterOffice.SystemGeneratedId != 0 || import
            .FilterServiceProvider.SystemGeneratedId != 0 || !
            IsEmpty(import.FilterServiceProvider.UserId) || !
            IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
          {
            if (!ReadOfficeOfficeServiceProviderServiceProvider())
            {
              continue;
            }

            if (import.FilterOffice.SystemGeneratedId != 0)
            {
              if (entities.Office.SystemGeneratedId != import
                .FilterOffice.SystemGeneratedId)
              {
                continue;
              }
            }

            if (import.FilterServiceProvider.SystemGeneratedId != 0)
            {
              if (entities.ServiceProvider.SystemGeneratedId != import
                .FilterServiceProvider.SystemGeneratedId)
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterServiceProvider.UserId))
            {
              if (!Equal(entities.ServiceProvider.UserId,
                import.FilterServiceProvider.UserId))
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
            {
              if (!Equal(entities.OfficeServiceProvider.RoleCode,
                import.FilterOfficeServiceProvider.RoleCode))
              {
                continue;
              }
            }

            if (!Equal(import.FilterOfficeServiceProvider.EffectiveDate,
              local.Null1.Date))
            {
              if (!Equal(entities.OfficeServiceProvider.EffectiveDate,
                import.FilterOfficeServiceProvider.EffectiveDate))
              {
                continue;
              }
            }
          }

          if (!IsEmpty(local.Filter.DenormText12))
          {
            if (!Equal(entities.Infrastructure.DenormText12,
              local.Filter.DenormText12))
            {
              // --The court order number does not match the import filter value
              // requested.
              continue;
            }

            // --Determine if the tribunal matches the import filter values.
            switch(TrimEnd(entities.OfficeServiceProviderAlert.TypeCode))
            {
              case "AUT":
                local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
                local.La.Identifier =
                  (int)entities.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
                  Equal(import.FilterFipsTribAddress.Country, "US"))
                {
                  if (ReadLegalAction2())
                  {
                    local.La.CourtCaseNumber =
                      entities.LegalAction.CourtCaseNumber;
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (ReadLegalAction1())
                {
                  local.La.CourtCaseNumber =
                    entities.LegalAction.CourtCaseNumber;
                }
                else
                {
                  continue;
                }

                break;
              case "MAN":
                // -- Manual alerts store the Tribunal Info (Country or State/
                // County) appended to
                //    the infrastructure detail attribute preceeded by a 
                // semicolon.
                if (Find(entities.Infrastructure.Detail, ";") == 0)
                {
                  // --Semicolon was not found in the infrastructure detail.  
                  // There is no tribunal info
                  //   stored in the detail attribute.  Skip this alert.
                  continue;
                }
                else
                {
                  local.Fips.Text4 =
                    Substring(entities.Infrastructure.Detail,
                    Find(entities.Infrastructure.Detail, ";") + 1, 4);

                  switch(Length(TrimEnd(local.Fips.Text4)))
                  {
                    case 2:
                      // --This is a foreign tribunal.  The 2 digit country 
                      // abbreviation is stored on the detail.
                      if (!IsEmpty(import.FilterFipsTribAddress.Country) && Equal
                        (import.FilterFipsTribAddress.Country, local.Fips.Text4,
                        1, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    case 4:
                      // --This is a domestic tribunal.  The 2 digit state 
                      // abbreviation is stored on the
                      //   detail followed by the 2 digit county abbreviation.
                      if (!IsEmpty(import.FilterFips.StateAbbreviation) && Equal
                        (import.FilterFips.StateAbbreviation, local.Fips.Text4,
                        1, 2) && !
                        IsEmpty(import.FilterFips.CountyAbbreviation) && Equal
                        (import.FilterFips.CountyAbbreviation, local.Fips.Text4,
                        3, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    default:
                      // --There are an unrecognizable number of characters.  
                      // Skip the alert.
                      //   This shouldn't happen.
                      continue;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA") || Equal
            (entities.OfficeServiceProviderAlert.TypeCode, "MAN"))
          {
            local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
            local.La.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();
          }

          if (!IsEmpty(entities.Infrastructure.CsePersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              entities.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              local.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            local.CsePersonsWorkSet.FormattedName = "";
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveOfficeServiceProviderAlert(entities.OfficeServiceProviderAlert,
            export.Export1.Update.GxfficeServiceProviderAlert);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.La.CourtCaseNumber;
          export.Export1.Update.Gxnfrastructure.Assign(entities.Infrastructure);
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.Export1.Item.GxfficeServiceProviderAlert.
            SystemGeneratedIdentifier == import
            .RepositionRecord.SystemGeneratedIdentifier)
          {
            export.Export1.Update.Gxommon.SelectChar = "S";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        break;
      case "CN":
        // -- Sort by Case Number Ascending
        foreach(var item in ReadInfrastructureOfficeServiceProviderAlert1())
        {
          MoveLegalAction(local.InitialLa, local.La);
          MoveCsePersonsWorkSet(local.Initial, local.CsePersonsWorkSet);

          // -- Apply additional filtering criteria
          if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
          {
            if (!Equal(entities.Infrastructure.CsePersonNumber,
              import.FilterInfrastructure.CsePersonNumber))
            {
              continue;
            }
          }

          if (!IsEmpty(local.Filter.EventType))
          {
            if (!Equal(entities.Infrastructure.EventType, local.Filter.EventType))
              
            {
              continue;
            }
          }

          if (!IsEmpty(import.FilterOfficeServiceProviderAlert.Message))
          {
            if (!Equal(entities.OfficeServiceProviderAlert.Message,
              import.FilterOfficeServiceProviderAlert.Message))
            {
              continue;
            }
          }

          if (import.FilterOffice.SystemGeneratedId != 0 || import
            .FilterServiceProvider.SystemGeneratedId != 0 || !
            IsEmpty(import.FilterServiceProvider.UserId) || !
            IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
          {
            if (!ReadOfficeOfficeServiceProviderServiceProvider())
            {
              continue;
            }

            if (import.FilterOffice.SystemGeneratedId != 0)
            {
              if (entities.Office.SystemGeneratedId != import
                .FilterOffice.SystemGeneratedId)
              {
                continue;
              }
            }

            if (import.FilterServiceProvider.SystemGeneratedId != 0)
            {
              if (entities.ServiceProvider.SystemGeneratedId != import
                .FilterServiceProvider.SystemGeneratedId)
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterServiceProvider.UserId))
            {
              if (!Equal(entities.ServiceProvider.UserId,
                import.FilterServiceProvider.UserId))
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
            {
              if (!Equal(entities.OfficeServiceProvider.RoleCode,
                import.FilterOfficeServiceProvider.RoleCode))
              {
                continue;
              }
            }

            if (!Equal(import.FilterOfficeServiceProvider.EffectiveDate,
              local.Null1.Date))
            {
              if (!Equal(entities.OfficeServiceProvider.EffectiveDate,
                import.FilterOfficeServiceProvider.EffectiveDate))
              {
                continue;
              }
            }
          }

          if (!IsEmpty(local.Filter.DenormText12))
          {
            if (!Equal(entities.Infrastructure.DenormText12,
              local.Filter.DenormText12))
            {
              // --The court order number does not match the import filter value
              // requested.
              continue;
            }

            // --Determine if the tribunal matches the import filter values.
            switch(TrimEnd(entities.OfficeServiceProviderAlert.TypeCode))
            {
              case "AUT":
                local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
                local.La.Identifier =
                  (int)entities.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
                  Equal(import.FilterFipsTribAddress.Country, "US"))
                {
                  if (ReadLegalAction2())
                  {
                    local.La.CourtCaseNumber =
                      entities.LegalAction.CourtCaseNumber;
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (ReadLegalAction1())
                {
                  local.La.CourtCaseNumber =
                    entities.LegalAction.CourtCaseNumber;
                }
                else
                {
                  continue;
                }

                break;
              case "MAN":
                // -- Manual alerts store the Tribunal Info (Country or State/
                // County) appended to
                //    the infrastructure detail attribute preceeded by a 
                // semicolon.
                if (Find(entities.Infrastructure.Detail, ";") == 0)
                {
                  // --Semicolon was not found in the infrastructure detail.  
                  // There is no tribunal info
                  //   stored in the detail attribute.  Skip this alert.
                  continue;
                }
                else
                {
                  local.Fips.Text4 =
                    Substring(entities.Infrastructure.Detail,
                    Find(entities.Infrastructure.Detail, ";") + 1, 4);

                  switch(Length(TrimEnd(local.Fips.Text4)))
                  {
                    case 2:
                      // --This is a foreign tribunal.  The 2 digit country 
                      // abbreviation is stored on the detail.
                      if (!IsEmpty(import.FilterFipsTribAddress.Country) && Equal
                        (import.FilterFipsTribAddress.Country, local.Fips.Text4,
                        1, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    case 4:
                      // --This is a domestic tribunal.  The 2 digit state 
                      // abbreviation is stored on the
                      //   detail followed by the 2 digit county abbreviation.
                      if (!IsEmpty(import.FilterFips.StateAbbreviation) && Equal
                        (import.FilterFips.StateAbbreviation, local.Fips.Text4,
                        1, 2) && !
                        IsEmpty(import.FilterFips.CountyAbbreviation) && Equal
                        (import.FilterFips.CountyAbbreviation, local.Fips.Text4,
                        3, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    default:
                      // --There are an unrecognizable number of characters.  
                      // Skip the alert.
                      //   This shouldn't happen.
                      continue;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA") || Equal
            (entities.OfficeServiceProviderAlert.TypeCode, "MAN"))
          {
            local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
            local.La.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();
          }

          if (!IsEmpty(entities.Infrastructure.CsePersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              entities.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              local.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            local.CsePersonsWorkSet.FormattedName = "";
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveOfficeServiceProviderAlert(entities.OfficeServiceProviderAlert,
            export.Export1.Update.GxfficeServiceProviderAlert);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.La.CourtCaseNumber;
          export.Export1.Update.Gxnfrastructure.Assign(entities.Infrastructure);
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.Export1.Item.GxfficeServiceProviderAlert.
            SystemGeneratedIdentifier == import
            .RepositionRecord.SystemGeneratedIdentifier)
          {
            export.Export1.Update.Gxommon.SelectChar = "S";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        break;
      case "PN":
        // -- Sort by Person Number Ascending
        foreach(var item in ReadInfrastructureOfficeServiceProviderAlert2())
        {
          MoveLegalAction(local.InitialLa, local.La);
          MoveCsePersonsWorkSet(local.Initial, local.CsePersonsWorkSet);

          // -- Apply additional filtering criteria
          if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
          {
            if (!Equal(entities.Infrastructure.CsePersonNumber,
              import.FilterInfrastructure.CsePersonNumber))
            {
              continue;
            }
          }

          if (!IsEmpty(local.Filter.EventType))
          {
            if (!Equal(entities.Infrastructure.EventType, local.Filter.EventType))
              
            {
              continue;
            }
          }

          if (!IsEmpty(import.FilterOfficeServiceProviderAlert.Message))
          {
            if (!Equal(entities.OfficeServiceProviderAlert.Message,
              import.FilterOfficeServiceProviderAlert.Message))
            {
              continue;
            }
          }

          if (import.FilterOffice.SystemGeneratedId != 0 || import
            .FilterServiceProvider.SystemGeneratedId != 0 || !
            IsEmpty(import.FilterServiceProvider.UserId) || !
            IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
          {
            if (!ReadOfficeOfficeServiceProviderServiceProvider())
            {
              continue;
            }

            if (import.FilterOffice.SystemGeneratedId != 0)
            {
              if (entities.Office.SystemGeneratedId != import
                .FilterOffice.SystemGeneratedId)
              {
                continue;
              }
            }

            if (import.FilterServiceProvider.SystemGeneratedId != 0)
            {
              if (entities.ServiceProvider.SystemGeneratedId != import
                .FilterServiceProvider.SystemGeneratedId)
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterServiceProvider.UserId))
            {
              if (!Equal(entities.ServiceProvider.UserId,
                import.FilterServiceProvider.UserId))
              {
                continue;
              }
            }

            if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
            {
              if (!Equal(entities.OfficeServiceProvider.RoleCode,
                import.FilterOfficeServiceProvider.RoleCode))
              {
                continue;
              }
            }

            if (!Equal(import.FilterOfficeServiceProvider.EffectiveDate,
              local.Null1.Date))
            {
              if (!Equal(entities.OfficeServiceProvider.EffectiveDate,
                import.FilterOfficeServiceProvider.EffectiveDate))
              {
                continue;
              }
            }
          }

          if (!IsEmpty(local.Filter.DenormText12))
          {
            if (!Equal(entities.Infrastructure.DenormText12,
              local.Filter.DenormText12))
            {
              // --The court order number does not match the import filter value
              // requested.
              continue;
            }

            // --Determine if the tribunal matches the import filter values.
            switch(TrimEnd(entities.OfficeServiceProviderAlert.TypeCode))
            {
              case "AUT":
                local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
                local.La.Identifier =
                  (int)entities.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
                  Equal(import.FilterFipsTribAddress.Country, "US"))
                {
                  if (ReadLegalAction2())
                  {
                    local.La.CourtCaseNumber =
                      entities.LegalAction.CourtCaseNumber;
                  }
                  else
                  {
                    continue;
                  }
                }
                else if (ReadLegalAction1())
                {
                  local.La.CourtCaseNumber =
                    entities.LegalAction.CourtCaseNumber;
                }
                else
                {
                  continue;
                }

                break;
              case "MAN":
                // -- Manual alerts store the Tribunal Info (Country or State/
                // County) appended to
                //    the infrastructure detail attribute preceeded by a 
                // semicolon.
                if (Find(entities.Infrastructure.Detail, ";") == 0)
                {
                  // --Semicolon was not found in the infrastructure detail.  
                  // There is no tribunal info
                  //   stored in the detail attribute.  Skip this alert.
                  continue;
                }
                else
                {
                  local.Fips.Text4 =
                    Substring(entities.Infrastructure.Detail,
                    Find(entities.Infrastructure.Detail, ";") + 1, 4);

                  switch(Length(TrimEnd(local.Fips.Text4)))
                  {
                    case 2:
                      // --This is a foreign tribunal.  The 2 digit country 
                      // abbreviation is stored on the detail.
                      if (!IsEmpty(import.FilterFipsTribAddress.Country) && Equal
                        (import.FilterFipsTribAddress.Country, local.Fips.Text4,
                        1, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    case 4:
                      // --This is a domestic tribunal.  The 2 digit state 
                      // abbreviation is stored on the
                      //   detail followed by the 2 digit county abbreviation.
                      if (!IsEmpty(import.FilterFips.StateAbbreviation) && Equal
                        (import.FilterFips.StateAbbreviation, local.Fips.Text4,
                        1, 2) && !
                        IsEmpty(import.FilterFips.CountyAbbreviation) && Equal
                        (import.FilterFips.CountyAbbreviation, local.Fips.Text4,
                        3, 2))
                      {
                        // --The tribunal matches the filter.  Continue.
                      }
                      else
                      {
                        // --The tribunal does not match the filter.  Skip this 
                        // alert.
                        continue;
                      }

                      break;
                    default:
                      // --There are an unrecognizable number of characters.  
                      // Skip the alert.
                      //   This shouldn't happen.
                      continue;
                  }
                }

                break;
              default:
                break;
            }
          }

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA") || Equal
            (entities.OfficeServiceProviderAlert.TypeCode, "MAN"))
          {
            local.La.CourtCaseNumber = entities.Infrastructure.DenormText12;
            local.La.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();
          }

          if (!IsEmpty(entities.Infrastructure.CsePersonNumber))
          {
            local.CsePersonsWorkSet.Number =
              entities.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              local.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            local.CsePersonsWorkSet.FormattedName = "";
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveOfficeServiceProviderAlert(entities.OfficeServiceProviderAlert,
            export.Export1.Update.GxfficeServiceProviderAlert);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.La.CourtCaseNumber;
          export.Export1.Update.Gxnfrastructure.Assign(entities.Infrastructure);
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.Export1.Item.GxfficeServiceProviderAlert.
            SystemGeneratedIdentifier == import
            .RepositionRecord.SystemGeneratedIdentifier)
          {
            export.Export1.Update.Gxommon.SelectChar = "S";
          }

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        break;
      default:
        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.DistributionDate = source.DistributionDate;
    target.RecipientUserId = source.RecipientUserId;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Type1 = useExport.AbendData.Type1;
  }

  private IEnumerable<bool> ReadInfrastructureOfficeServiceProviderAlert1()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureOfficeServiceProviderAlert1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber1", import.FilterInfrastructure.CaseNumber ?? ""
          );
        db.SetDate(
          command, "distributionDate1",
          import.FilterOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetString(
          command, "typeCode",
          import.FilterOfficeServiceProviderAlert.TypeCode);
        db.SetNullableString(
          command, "caseNumber2",
          import.StartPageKeyInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "distributionDate2",
          import.StartPageKeyOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.StartPageKeyOfficeServiceProviderAlert.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 9);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 10);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 11);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 14);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 15);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 17);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 18);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 19);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 20);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 23);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 24);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 25);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureOfficeServiceProviderAlert2()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureOfficeServiceProviderAlert2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.FilterInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "distributionDate1",
          import.FilterOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetString(
          command, "typeCode",
          import.FilterOfficeServiceProviderAlert.TypeCode);
        db.SetNullableString(
          command, "csePersonNum",
          import.StartPageKeyInfrastructure.CsePersonNumber ?? "");
        db.SetDate(
          command, "distributionDate2",
          import.StartPageKeyOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.StartPageKeyOfficeServiceProviderAlert.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 9);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 10);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 11);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 14);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 15);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 17);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 18);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 19);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 20);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 23);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 24);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 25);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureOfficeServiceProviderAlert3()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureOfficeServiceProviderAlert3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.FilterInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "distributionDate1",
          import.FilterOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetString(
          command, "typeCode",
          import.FilterOfficeServiceProviderAlert.TypeCode);
        db.SetDate(
          command, "distributionDate2",
          import.StartPageKeyOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.StartPageKeyOfficeServiceProviderAlert.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 9);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 10);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 11);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 14);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 15);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 17);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 18);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 19);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 20);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 23);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 24);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 25);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureOfficeServiceProviderAlert4()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureOfficeServiceProviderAlert4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.FilterInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "distributionDate1",
          import.FilterOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetString(
          command, "typeCode",
          import.FilterOfficeServiceProviderAlert.TypeCode);
        db.SetString(
          command, "message",
          import.StartPageKeyOfficeServiceProviderAlert.Message);
        db.SetDate(
          command, "distributionDate2",
          import.StartPageKeyOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.StartPageKeyOfficeServiceProviderAlert.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 9);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 10);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 11);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 14);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 15);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 17);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 18);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 19);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 20);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 23);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 24);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 25);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureOfficeServiceProviderAlert5()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureOfficeServiceProviderAlert5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.FilterInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "distributionDate1",
          import.FilterOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetString(
          command, "typeCode",
          import.FilterOfficeServiceProviderAlert.TypeCode);
        db.SetDate(
          command, "distributionDate2",
          import.StartPageKeyOfficeServiceProviderAlert.DistributionDate.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.StartPageKeyOfficeServiceProviderAlert.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.EventType = db.GetString(reader, 3);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 4);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 5);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 6);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 7);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 9);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 10);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 11);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 14);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 15);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 17);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 18);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 19);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 20);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 21);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 22);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 23);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 24);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 25);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          entities.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNo", import.FilterLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", import.FilterFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", import.FilterFips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          entities.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNo", import.FilterLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", import.FilterFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    System.Diagnostics.Debug.Assert(
      entities.OfficeServiceProviderAlert.Populated);
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeServiceProviderAlert.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.OfficeServiceProviderAlert.OspCode ?? ""
          );
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProviderAlert.OffId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProviderAlert.SpdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
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
    /// A value of FilterOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert FilterOfficeServiceProviderAlert
    {
      get => filterOfficeServiceProviderAlert ??= new();
      set => filterOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of FilterFips.
    /// </summary>
    [JsonPropertyName("filterFips")]
    public Fips FilterFips
    {
      get => filterFips ??= new();
      set => filterFips = value;
    }

    /// <summary>
    /// A value of FilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("filterFipsTribAddress")]
    public FipsTribAddress FilterFipsTribAddress
    {
      get => filterFipsTribAddress ??= new();
      set => filterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FilterInfrastructure.
    /// </summary>
    [JsonPropertyName("filterInfrastructure")]
    public Infrastructure FilterInfrastructure
    {
      get => filterInfrastructure ??= new();
      set => filterInfrastructure = value;
    }

    /// <summary>
    /// A value of FilterCodeValue.
    /// </summary>
    [JsonPropertyName("filterCodeValue")]
    public CodeValue FilterCodeValue
    {
      get => filterCodeValue ??= new();
      set => filterCodeValue = value;
    }

    /// <summary>
    /// A value of SortDesc.
    /// </summary>
    [JsonPropertyName("sortDesc")]
    public CodeValue SortDesc
    {
      get => sortDesc ??= new();
      set => sortDesc = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProvider")]
    public OfficeServiceProvider FilterOfficeServiceProvider
    {
      get => filterOfficeServiceProvider ??= new();
      set => filterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOffice.
    /// </summary>
    [JsonPropertyName("filterOffice")]
    public Office FilterOffice
    {
      get => filterOffice ??= new();
      set => filterOffice = value;
    }

    /// <summary>
    /// A value of StartPageKeyOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("startPageKeyOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert StartPageKeyOfficeServiceProviderAlert
    {
      get => startPageKeyOfficeServiceProviderAlert ??= new();
      set => startPageKeyOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of StartPageKeyInfrastructure.
    /// </summary>
    [JsonPropertyName("startPageKeyInfrastructure")]
    public Infrastructure StartPageKeyInfrastructure
    {
      get => startPageKeyInfrastructure ??= new();
      set => startPageKeyInfrastructure = value;
    }

    /// <summary>
    /// A value of RepositionRecord.
    /// </summary>
    [JsonPropertyName("repositionRecord")]
    public OfficeServiceProviderAlert RepositionRecord
    {
      get => repositionRecord ??= new();
      set => repositionRecord = value;
    }

    private OfficeServiceProviderAlert filterOfficeServiceProviderAlert;
    private Fips filterFips;
    private FipsTribAddress filterFipsTribAddress;
    private LegalAction filterLegalAction;
    private Infrastructure filterInfrastructure;
    private CodeValue filterCodeValue;
    private CodeValue sortDesc;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private ServiceProvider filterServiceProvider;
    private Office filterOffice;
    private OfficeServiceProviderAlert startPageKeyOfficeServiceProviderAlert;
    private Infrastructure startPageKeyInfrastructure;
    private OfficeServiceProviderAlert repositionRecord;
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
      /// A value of GxegalAction.
      /// </summary>
      [JsonPropertyName("gxegalAction")]
      public LegalAction GxegalAction
      {
        get => gxegalAction ??= new();
        set => gxegalAction = value;
      }

      /// <summary>
      /// A value of GxanualFipsTribAddress.
      /// </summary>
      [JsonPropertyName("gxanualFipsTribAddress")]
      public FipsTribAddress GxanualFipsTribAddress
      {
        get => gxanualFipsTribAddress ??= new();
        set => gxanualFipsTribAddress = value;
      }

      /// <summary>
      /// A value of GxanualFips.
      /// </summary>
      [JsonPropertyName("gxanualFips")]
      public Fips GxanualFips
      {
        get => gxanualFips ??= new();
        set => gxanualFips = value;
      }

      /// <summary>
      /// A value of Gxnfrastructure.
      /// </summary>
      [JsonPropertyName("gxnfrastructure")]
      public Infrastructure Gxnfrastructure
      {
        get => gxnfrastructure ??= new();
        set => gxnfrastructure = value;
      }

      /// <summary>
      /// A value of GxfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gxfficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GxfficeServiceProviderAlert
      {
        get => gxfficeServiceProviderAlert ??= new();
        set => gxfficeServiceProviderAlert = value;
      }

      /// <summary>
      /// A value of Gxommon.
      /// </summary>
      [JsonPropertyName("gxommon")]
      public Common Gxommon
      {
        get => gxommon ??= new();
        set => gxommon = value;
      }

      /// <summary>
      /// A value of GxsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gxsePersonsWorkSet")]
      public CsePersonsWorkSet GxsePersonsWorkSet
      {
        get => gxsePersonsWorkSet ??= new();
        set => gxsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalAction gxegalAction;
      private FipsTribAddress gxanualFipsTribAddress;
      private Fips gxanualFips;
      private Infrastructure gxnfrastructure;
      private OfficeServiceProviderAlert gxfficeServiceProviderAlert;
      private Common gxommon;
      private CsePersonsWorkSet gxsePersonsWorkSet;
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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public TextWorkArea Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Infrastructure Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of InitialLa.
    /// </summary>
    [JsonPropertyName("initialLa")]
    public LegalAction InitialLa
    {
      get => initialLa ??= new();
      set => initialLa = value;
    }

    /// <summary>
    /// A value of La.
    /// </summary>
    [JsonPropertyName("la")]
    public LegalAction La
    {
      get => la ??= new();
      set => la = value;
    }

    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public CsePersonsWorkSet Initial
    {
      get => initial ??= new();
      set => initial = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private TextWorkArea fips;
    private DateWorkArea null1;
    private Infrastructure filter;
    private LegalAction initialLa;
    private LegalAction la;
    private CsePersonsWorkSet initial;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
  }
#endregion
}
