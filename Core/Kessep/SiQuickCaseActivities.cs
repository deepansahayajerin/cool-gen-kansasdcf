// Program: SI_QUICK_CASE_ACTIVITIES, ID: 374537236, model: 746.
// Short name: SWE03115
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
/// A program: SI_QUICK_CASE_ACTIVITIES.
/// </para>
/// <para>
/// Case Activities
/// </para>
/// </summary>
[Serializable]
public partial class SiQuickCaseActivities: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CASE_ACTIVITIES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickCaseActivities(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickCaseActivities.
  /// </summary>
  public SiQuickCaseActivities(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // IMPORTANT
    // !!********************************************
    // *    IF IMPORT OR EXPORT VIEWS ARE MODIFIED THEN THE  *
    // *    DB2 STORED PROCEDURE MUST BE UPDATED TO MATCH!!  *
    // *******************************************************
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 09/01/2009	L Smith		CQ# 211		Initial development
    // 		J Huss
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Min.Date = new DateTime(1, 1, 1);
    UseSiQuickGetCpHeader();

    if (IsExitState("CASE_NF"))
    {
      export.QuickErrorMessages.ErrorCode = "406";
      export.QuickErrorMessages.ErrorMessage = "Case Not Found";

      return;
    }

    export.QuickCpHeader.Assign(local.QuickCpHeader);

    // **********************************************************************
    // If family violence is found for any case participant on the case
    // then no data will be returned.  This includes all active and
    // inactive case participants.  This is true for open or closed cases.
    // **********************************************************************
    if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
    {
      export.QuickErrorMessages.ErrorCode = "407";
      export.QuickErrorMessages.ErrorMessage =
        "Disclosure prohibited on the case requested.";

      return;
    }

    // *************************************************************************
    // Case - Open and Closure
    // *************************************************************************
    if (ReadCase())
    {
      export.QuickCaseActivities.CaseOpenDate =
        NumberToString(DateToInt(entities.Case1.CseOpenDate), 8);

      if (AsChar(entities.Case1.Status) == 'C')
      {
        export.QuickCaseActivities.CaseClosedDate =
          NumberToString(DateToInt(entities.Case1.StatusDate), 8);

        // Translate our closure reason to the closure reason defined by OCSE.
        switch(TrimEnd(entities.Case1.ClosureReason))
        {
          case "CC":
            export.QuickCaseActivities.CaseClosureReason = "01";

            break;
          case "MJ":
            export.QuickCaseActivities.CaseClosureReason = "01";

            break;
          case "RO":
            export.QuickCaseActivities.CaseClosureReason = "01";

            break;
          case "CV":
            export.QuickCaseActivities.CaseClosureReason = "01";

            break;
          case "DC":
            export.QuickCaseActivities.CaseClosureReason = "02";

            break;
          case "NP":
            export.QuickCaseActivities.CaseClosureReason = "03";

            break;
          case "EM":
            export.QuickCaseActivities.CaseClosureReason = "03";

            break;
          case "NL":
            export.QuickCaseActivities.CaseClosureReason = "04";

            break;
          case "AB":
            export.QuickCaseActivities.CaseClosureReason = "05";

            break;
          case "FO":
            export.QuickCaseActivities.CaseClosureReason = "06";

            break;
          case "LO":
            export.QuickCaseActivities.CaseClosureReason = "07";

            break;
          case "AR":
            export.QuickCaseActivities.CaseClosureReason = "08";

            break;
          case "GC":
            export.QuickCaseActivities.CaseClosureReason = "09";

            break;
          case "LC":
            export.QuickCaseActivities.CaseClosureReason = "10";

            break;
          case "FC":
            export.QuickCaseActivities.CaseClosureReason = "11";

            break;
          case "4D":
            export.QuickCaseActivities.CaseClosureReason = "12";

            break;
          default:
            export.QuickCaseActivities.CaseClosureReason = "01";

            break;
        }
      }
    }

    // ******************************************************************************
    // Locate - CP and NCP Addresses
    // Get the most recent non-end-dated mailing (M) and residential (R)
    // addresses for all CPs on the case and the NCP in the header.
    // Use the records with the most recent created date.
    // In QUICK, only the CP address has a field for the person name.
    // This is why we return addresses for all CPs on the case but only
    // the NCP that's listed in the header.
    // No data is provided if the CP is the State of Kansas or JJA.
    // ******************************************************************************
    export.QuickPersonAddr.Index = -1;
    local.Previous.Number = "";

    foreach(var item in ReadCsePersonCaseRole())
    {
      // Do not populate the address if it is the State of Kansas (000000017o)
      // or if it is Kansas Juvenille Justice Authority (000004029o).
      if (Equal(entities.CsePerson.Number, "000000017O") || Equal
        (entities.CsePerson.Number, "000004029O"))
      {
        continue;
      }

      // Don't process the same person twice.
      if (Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        continue;
      }

      local.Previous.Number = entities.CsePerson.Number;

      // Reset the M and R flags for this person.
      local.MailAddrFound.Flag = "N";
      local.ResiAddrFound.Flag = "N";

      // Find the most recently added addresses that aren't end dated.
      foreach(var item1 in ReadCsePersonAddress())
      {
        // If we already have a mailing or residential address for this person, 
        // don't pick another one.
        if (AsChar(entities.CsePersonAddress.Type1) == 'M' && AsChar
          (local.MailAddrFound.Flag) == 'Y' || AsChar
          (entities.CsePersonAddress.Type1) == 'R' && AsChar
          (local.ResiAddrFound.Flag) == 'Y')
        {
          continue;
        }

        ++export.QuickPersonAddr.Index;
        export.QuickPersonAddr.CheckSize();

        if (export.QuickPersonAddr.Index >= Export
          .QuickPersonAddrGroup.Capacity)
        {
          goto ReadEach;
        }

        // Record what we found
        if (AsChar(entities.CsePersonAddress.Type1) == 'M')
        {
          local.MailAddrFound.Flag = "Y";
        }
        else
        {
          local.ResiAddrFound.Flag = "Y";
        }

        // Set attributes that are common to both foreign and domestic addresses
        export.QuickPersonAddr.Update.QuickPersonAddress.Date =
          NumberToString(DateToInt(
            Date(entities.CsePersonAddress.LastUpdatedTimestamp)), 8, 8);
        export.QuickPersonAddr.Update.QuickPersonAddress.AddressType =
          entities.CsePersonAddress.Type1 ?? Spaces(1);
        export.QuickPersonAddr.Update.QuickPersonAddress.LocationType =
          entities.CsePersonAddress.LocationType;
        export.QuickPersonAddr.Update.QuickPersonAddress.Street1 =
          entities.CsePersonAddress.Street1 ?? Spaces(25);
        export.QuickPersonAddr.Update.QuickPersonAddress.Street2 =
          entities.CsePersonAddress.Street2 ?? Spaces(25);
        export.QuickPersonAddr.Update.QuickPersonAddress.City =
          entities.CsePersonAddress.City ?? Spaces(15);

        if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
        {
          // Set attributes that are specific to (D)omestic addresses.
          export.QuickPersonAddr.Update.QuickPersonAddress.State =
            entities.CsePersonAddress.State ?? Spaces(2);
          export.QuickPersonAddr.Update.QuickPersonAddress.Zip =
            entities.CsePersonAddress.ZipCode ?? Spaces(5);
          export.QuickPersonAddr.Update.QuickPersonAddress.Zip4 =
            entities.CsePersonAddress.Zip4 ?? Spaces(4);
        }
        else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
        {
          // Set attributes that are specific to (F)oreign addresses.
          export.QuickPersonAddr.Update.QuickPersonAddress.Street3 =
            entities.CsePersonAddress.Street3 ?? Spaces(25);
          export.QuickPersonAddr.Update.QuickPersonAddress.Street4 =
            entities.CsePersonAddress.Street4 ?? Spaces(25);
          export.QuickPersonAddr.Update.QuickPersonAddress.Country =
            entities.CsePersonAddress.Country ?? Spaces(2);
          export.QuickPersonAddr.Update.QuickPersonAddress.Province =
            entities.CsePersonAddress.Province ?? Spaces(5);
          export.QuickPersonAddr.Update.QuickPersonAddress.PostalCode =
            entities.CsePersonAddress.PostalCode ?? Spaces(10);
        }

        // In QUICK, only CP addresses have a field for the CP name.
        if (Equal(entities.CaseRole.Type1, "AR"))
        {
          if (AsChar(entities.CsePerson.Type1) == 'C')
          {
            // CP is a (C)ustomer
            if (ReadCsePersonDetail1())
            {
              export.QuickPersonAddr.Update.Loc.FirstName =
                entities.CsePersonDetail.FirstName;
              export.QuickPersonAddr.Update.Loc.MiddleInitial =
                entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
              export.QuickPersonAddr.Update.Loc.LastName =
                entities.CsePersonDetail.LastName;
            }
            else
            {
              // Person may not have been copied from ADABAS to the CSE Person 
              // Detail table yet
            }
          }
          else
          {
            // CP is an (O)rganization
            export.QuickPersonAddr.Update.Loc.OrganizationName =
              export.QuickCpHeader.CpOrganizationName;
          }

          export.QuickPersonAddr.Update.PersonRole.Text3 = "CP";
        }
        else
        {
          export.QuickPersonAddr.Update.PersonRole.Text3 = "NCP";
        }
      }
    }

ReadEach:

    // *************************************************************************
    // Locate - Incarceration & Incarceration Release
    // *************************************************************************
    // Check for incarceration of NCP.
    if (ReadIncarceration())
    {
      // If the end date is greater than the current date, then the NCP is 
      // Incarcerated.
      // The JAIL screen has been updating the end_date field incorrectly, so 
      // the
      // check for null date is necessary as well.
      if ((Lt(local.Current.Date, entities.Incarceration.EndDate) || Equal
        (entities.Incarceration.EndDate, local.Null1.Date)) && AsChar
        (entities.Incarceration.Incarcerated) == 'Y')
      {
        export.QuickCaseActivities.LocIncarceratedInd = "Y";

        // If we know the Incarceration start date, then provide it.
        if (Lt(local.Min.Date, entities.Incarceration.StartDate))
        {
          export.QuickCaseActivities.LocIncarceratedDate =
            NumberToString(DateToInt(entities.Incarceration.StartDate), 8);
        }
      }

      // If the person was (not necessarily is) incarcerated.
      if (AsChar(entities.Incarceration.Incarcerated) == 'Y')
      {
        // If an end date exists, then the NCP either was released or has an 
        // expected release date.
        if (Lt(entities.Incarceration.EndDate, local.Max.Date) && Lt
          (local.Min.Date, entities.Incarceration.EndDate))
        {
          // If the end date is not max date, then some specific release date 
          // was set.  Provide it.
          export.QuickCaseActivities.LocIncarceratedReleaseDate =
            NumberToString(DateToInt(entities.Incarceration.EndDate), 8);
        }
        else if (Lt(local.Min.Date, entities.Incarceration.ParoleEligibilityDate))
          
        {
          // If we don't have a release date, but we do have a parole 
          // eligibility date, provide the parole eligibility date.
          export.QuickCaseActivities.LocIncarceratedReleaseDate =
            NumberToString(DateToInt(
              entities.Incarceration.ParoleEligibilityDate), 8);
        }
      }
    }

    // *************************************************************************
    // Locate - Date of Death
    // *************************************************************************
    if (ReadCsePerson())
    {
      export.QuickCaseActivities.LocDateOfDeath =
        NumberToString(DateToInt(entities.CsePerson.DateOfDeath), 8);
    }

    // *************************************************************************
    // Paternity
    // *************************************************************************
    export.QuickPaternity.Index = -1;

    foreach(var item in ReadCaseCaseRoleCsePerson())
    {
      if (Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        continue;
      }

      local.Previous.Number = entities.CsePerson.Number;
      local.TempPaternityCommon.Flag = "";
      local.TempPaternityDateWorkArea.Date = local.Null1.Date;

      // If paternity could not be established then the case is closed for that 
      // reason.
      if (Equal(entities.Case1.ClosureReason, "NP") && AsChar
        (entities.CsePerson.PaternityEstablishedIndicator) == 'N')
      {
        // *************************************************************************
        // Paternity - Not Established.
        // *************************************************************************
        local.TempPaternityCommon.Flag = "N";
        local.TempPaternityDateWorkArea.Date = entities.Case1.StatusDate;
      }
      else if (AsChar(entities.CsePerson.PaternityEstablishedIndicator) == 'Y')
      {
        // *************************************************************************
        // Paternity - Established.
        // *************************************************************************
        local.TempPaternityCommon.Flag = "E";
        local.TempPaternityDateWorkArea.Date =
          entities.CsePerson.DatePaternEstab;
      }
      else if (AsChar(entities.CsePerson.PaternityEstablishedIndicator) == 'N')
      {
        // *************************************************************************
        // Paternity - Voluntarily Acknowledgement.
        // *************************************************************************
        if (ReadInfrastructureLegalActionPersonLegalAction())
        {
          local.TempPaternityCommon.Flag = "A";
          local.TempPaternityDateWorkArea.Date =
            Date(entities.Infrastructure.CreatedTimestamp);
        }
      }
      else
      {
        continue;
      }

      // Paternity has not yet been established
      if (IsEmpty(local.TempPaternityCommon.Flag))
      {
        continue;
      }

      ++export.QuickPaternity.Index;
      export.QuickPaternity.CheckSize();

      if (export.QuickPaternity.Index >= Export.QuickPaternityGroup.Capacity)
      {
        break;
      }

      export.QuickPaternity.Update.QuickPaternityInfo.PatInd =
        local.TempPaternityCommon.Flag;
      export.QuickPaternity.Update.QuickPaternityInfo.PatDate =
        NumberToString(DateToInt(local.TempPaternityDateWorkArea.Date), 8);

      if (ReadCsePersonDetail1())
      {
        export.QuickPaternity.Update.Pat.FirstName =
          entities.CsePersonDetail.FirstName;
        export.QuickPaternity.Update.Pat.MiddleInitial =
          entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
        export.QuickPaternity.Update.Pat.LastName =
          entities.CsePersonDetail.LastName;
      }
      else
      {
        // Person may not have been copied from ADABAS to the CSE Person Detail 
        // table yet
      }
    }

    // *************************************************************************
    // Order Establishment - Child/Medical Support Orders
    // Unfortunately, this is a frustratingly complex operation.  Basically 
    // what's happening is that
    // we're looking for any active CS or HIC legal action detail for the 
    // children on the case being queried.
    // A record is returned for each child that contains a (c)hild, (m)edical, 
    // or (b)oth support indicator
    // and a most recent date.
    // *************************************************************************
    export.QuickOe.Index = 0;
    export.QuickOe.CheckSize();

    local.ChildTemp.Number = "";

    foreach(var item in ReadLegalActionDetailCsePerson())
    {
      if (export.QuickOe.Index >= Export.QuickOeGroup.Capacity)
      {
        break;
      }

      // Check to see if it's a HIC legal detail.  If not, check to see if it's 
      // a CS legal detail.
      // If it's not one of those two, we're not interested in the record.
      if (!Equal(entities.LegalActionDetail.NonFinOblgType, "HIC"))
      {
        local.ObligationType.Code = "";

        if (ReadObligationType())
        {
          local.ObligationType.Code = entities.ObligationType.Code;
        }
        else
        {
          continue;
        }
      }

      // If the temp person number is different, then it's either the first 
      // iteration, or we've moved to the next child.
      if (!Equal(local.ChildTemp.Number, entities.ChildCsePerson.Number))
      {
        // If the temp person number is not spaces, then we've moved to the next
        // child
        if (!IsEmpty(local.ChildTemp.Number))
        {
          if (ReadCsePersonDetail2())
          {
            export.QuickOe.Update.Oe.FirstName =
              entities.CsePersonDetail.FirstName;
            export.QuickOe.Update.Oe.MiddleInitial =
              entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
            export.QuickOe.Update.Oe.LastName =
              entities.CsePersonDetail.LastName;
          }
          else
          {
            // Person may not have been copied from ADABAS to the CSE Person 
            // Detail table yet
          }

          if (AsChar(local.CsOrderFound.Flag) == 'Y' && AsChar
            (local.MsOrderFound.Flag) == 'Y')
          {
            // (B)oth child support and medical support.
            export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "B";

            // If the child has both medical support and child support, provide 
            // the effective date of the most recent l.a.d.
            if (Lt(local.OeMsMaxDate.Date, local.OeCsMaxDate.Date))
            {
              export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
                NumberToString(DateToInt(local.OeCsMaxDate.Date), 8);
            }
            else
            {
              export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
                NumberToString(DateToInt(local.OeMsMaxDate.Date), 8);
            }
          }
          else if (AsChar(local.CsOrderFound.Flag) == 'Y')
          {
            // (C)hild support only.
            export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "C";
            export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
              NumberToString(DateToInt(local.OeCsMaxDate.Date), 8);
          }
          else if (AsChar(local.MsOrderFound.Flag) == 'Y')
          {
            // (M)edical support only.
            export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "M";
            export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
              NumberToString(DateToInt(local.OeMsMaxDate.Date), 8);
          }

          ++export.QuickOe.Index;
          export.QuickOe.CheckSize();
        }

        // Reset child specific flags
        local.ChildTemp.Number = entities.ChildCsePerson.Number;
        local.CsOrderFound.Flag = "N";
        local.MsOrderFound.Flag = "N";
        local.OeCsMaxDate.Date = local.Null1.Date;
        local.OeMsMaxDate.Date = local.Null1.Date;
      }

      if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC"))
      {
        local.MsOrderFound.Flag = "Y";

        // Since the read is sorted by l.a.d. effective date, the first valid l.
        // a.d.
        // encountered will have the most recent date.  Provide that date.
        if (Equal(local.OeMsMaxDate.Date, local.Null1.Date))
        {
          local.OeMsMaxDate.Date = entities.LegalActionDetail.EffectiveDate;
        }
      }

      if (Equal(local.ObligationType.Code, "CS"))
      {
        local.CsOrderFound.Flag = "Y";

        // Since the read is sorted by l.a.d. effective date, the first valid l.
        // a.d.
        // encountered will have the most recent date.  Provide that date.
        if (Equal(local.OeCsMaxDate.Date, local.Null1.Date))
        {
          local.OeCsMaxDate.Date = entities.LegalActionDetail.EffectiveDate;
        }
      }
    }

    // Add the final child found.  If the temp person number is still equal to 
    // spaces, no records were found.
    if (!IsEmpty(local.ChildTemp.Number))
    {
      if (ReadCsePersonDetail2())
      {
        export.QuickOe.Update.Oe.FirstName = entities.CsePersonDetail.FirstName;
        export.QuickOe.Update.Oe.MiddleInitial =
          entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
        export.QuickOe.Update.Oe.LastName = entities.CsePersonDetail.LastName;
      }
      else
      {
        // Person may not have been copied from ADABAS to the CSE Person Detail 
        // table yet
      }

      if (AsChar(local.CsOrderFound.Flag) == 'Y' && AsChar
        (local.MsOrderFound.Flag) == 'Y')
      {
        // (B)oth child support and medical support.
        export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "B";

        // If the child has both medical support and child support, provide the 
        // effective date of the most recent l.a.d.
        if (Lt(local.OeMsMaxDate.Date, local.OeCsMaxDate.Date))
        {
          export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
            NumberToString(DateToInt(local.OeCsMaxDate.Date), 8);
        }
        else
        {
          export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
            NumberToString(DateToInt(local.OeMsMaxDate.Date), 8);
        }
      }
      else if (AsChar(local.CsOrderFound.Flag) == 'Y')
      {
        // (C)hild support only.
        export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "C";
        export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
          NumberToString(DateToInt(local.OeCsMaxDate.Date), 8);
      }
      else if (AsChar(local.MsOrderFound.Flag) == 'Y')
      {
        // (M)edical support only.
        export.QuickOe.Update.QuickOrderEstabInfo.OeInd = "M";
        export.QuickOe.Update.QuickOrderEstabInfo.OeDate =
          NumberToString(DateToInt(local.OeMsMaxDate.Date), 8);
      }
    }

    // *************************************************************************
    // Order Establishment - Last Review Date
    // Valid events are:
    // 8 - 48 - MODFNRVWDT
    // 120 - 1 - LEGAL36MOREVIEW
    // *************************************************************************
    if (ReadInfrastructureEventDetail5())
    {
      export.QuickCaseActivities.OeLastReviewDate =
        NumberToString(
          DateToInt(Date(entities.Infrastructure.CreatedTimestamp)), 8, 8);
    }

    // *************************************************************************
    // Order Establishment - Last Modification Date
    // Valid events are:
    // 30 - 463 - FMODSUPPO
    // 30 - 512 - FREGMODO
    // 95 - 478 - AMODBC
    // 95 - 471 - AJEFMOD
    // 95 - 338 - AREGMODO
    // 95 - 492 - AREGMODNJ
    // 95 - 447 - ACONMODJ
    // *************************************************************************
    if (ReadInfrastructureEventDetail4())
    {
      export.QuickCaseActivities.OeLastModifiedDate =
        NumberToString(
          DateToInt(Date(entities.Infrastructure.CreatedTimestamp)), 8, 8);
    }

    // *************************************************************************
    // Order Enforcement - Controlling Order
    // Valid events are:
    // 20 - 236 - CONTORDN
    // *************************************************************************
    if (ReadInfrastructureEventDetail3())
    {
      export.QuickCaseActivities.EnfCntrlOrderDetermineDate =
        NumberToString(
          DateToInt(Date(entities.Infrastructure.CreatedTimestamp)), 8, 8);
    }

    // *************************************************************************
    // Order Enforcement - IWO
    // Valid events are:
    // 20 - 241 - ORDIWO2A
    // 20 - 227 - ORDIWO2
    // *************************************************************************
    local.TempIncomeSource.Identifier = local.Null1.Timestamp;

    if (ReadInfrastructureEventDetail1())
    {
      // Some documents add the income source ID to the infrastructure record.  
      // Use it if it's available.
      if (!Equal(entities.Infrastructure.DenormTimestamp, local.Null1.Timestamp))
        
      {
        if (ReadIncomeSource())
        {
          export.QuickCaseActivities.EnfIwoEmployerName =
            entities.IncomeSource.Name ?? Spaces(60);
        }
      }
      else
      {
        // As part of the QUICK project, a change was made to the way ORDIWO2s 
        // are generated
        // such that when an ORDIWO2 is generated, the system writes the name of
        // the employer
        // to the detail field of the infrastructure record.  Prior to the 
        // change it was spaces.
        // If it's available, use it.
        if (!IsEmpty(entities.Infrastructure.Detail))
        {
          // Infrastructure detail is recorded as:
          // EMP: <Employer Name>; LOC: <Employer Location>
          local.Position.Count =
            Find(entities.Infrastructure.Detail, "; LOC: ");

          if (local.Position.Count > 0)
          {
            export.QuickCaseActivities.EnfIwoEmployerName =
              Substring(entities.Infrastructure.Detail, 6,
              local.Position.Count - 6);
          }
        }
        else
        {
          // Income source was not recorded on infrastructure record.  Check to 
          // see if it's still available
          // from the actual document that was sent.
          if (ReadFieldValue2())
          {
            export.QuickCaseActivities.EnfIwoEmployerName =
              entities.FieldValue.Value ?? Spaces(60);
          }
          else
          {
            // At this point, we don't have any easily available record of who 
            // we sent the IWO to.
            // All records have been archived.  Set employer name to indicate 
            // data is being retrieved
            // and set trigger to retrieve document values during overnight 
            // batch run
            export.QuickCaseActivities.EnfIwoEmployerName =
              "'archived information, please check again tomorrow.'";
            UseSpDocCreateRetFldValTrg();
          }
        }
      }

      export.QuickCaseActivities.EnfIwoDate =
        NumberToString(
          DateToInt(Date(entities.Infrastructure.CreatedTimestamp)), 8, 8);
    }

    // *************************************************************************
    // Order Enforcement - NMSN
    // Valid events are:
    // 20 - 268 - MWONOHCA
    // 20 - 244 - MOWNOTHC
    // *************************************************************************
    local.TempIncomeSource.Identifier = local.Null1.Timestamp;

    if (ReadInfrastructureEventDetail2())
    {
      // Some documents add the income source ID to the infrastructure record.  
      // Use it if it's available.
      if (!Equal(entities.Infrastructure.DenormTimestamp, local.Null1.Timestamp))
        
      {
        if (ReadIncomeSource())
        {
          export.QuickCaseActivities.EnfNmsnEmployerName =
            entities.IncomeSource.Name ?? Spaces(60);
        }
      }
      else
      {
        // As part of the QUICK project, a change was made to the way MWONOTHCs 
        // are generated
        // such that when an MWONOTHC is generated, the system writes the name 
        // of the employer
        // to the detail field of the infrastructure record.  Prior to the 
        // change it was spaces.
        // If it's available, use it.
        if (!IsEmpty(entities.Infrastructure.Detail))
        {
          // Infrastructure detail is recorded as:
          // EMP: <Employer Name>; LOC: <Employer Location>
          local.Position.Count =
            Find(entities.Infrastructure.Detail, "; LOC: ");

          if (local.Position.Count > 0)
          {
            export.QuickCaseActivities.EnfNmsnEmployerName =
              Substring(entities.Infrastructure.Detail, 6,
              local.Position.Count - 6);
          }
        }
        else
        {
          // Income source was not recorded on infrastructure record.  Check to 
          // see if it's still available
          // from the actual document that was sent.
          if (ReadFieldValue1())
          {
            export.QuickCaseActivities.EnfNmsnEmployerName =
              entities.FieldValue.Value ?? Spaces(60);
          }
          else
          {
            // At this point, we don't have any easily available record of who 
            // we sent the NMSN to.
            // All records have been archived.  Set employer name to indicate 
            // data is being retrieved
            // and set trigger to retrieve document values during overnight 
            // batch run
            export.QuickCaseActivities.EnfNmsnEmployerName =
              "'archived information, please check again tomorrow.'";
            UseSpDocCreateRetFldValTrg();
          }
        }
      }

      export.QuickCaseActivities.EnfNmsnDate =
        NumberToString(
          DateToInt(Date(entities.Infrastructure.CreatedTimestamp)), 8, 8);
    }

    // *************************************************************************
    // Order Enforcement - Medical Coverage
    // Provide the most recent date that we determined the NCP is providing 
    // Health
    // Insurance for any of the children on the case.
    // *************************************************************************
    if (ReadHealthInsuranceCoveragePersonalHealthInsurance())
    {
      // If a verified date exists, provide it.  Otherwise, provide the start 
      // date.
      if (Lt(local.Min.Date,
        entities.PersonalHealthInsurance.CoverageVerifiedDate))
      {
        export.QuickCaseActivities.EnfMedicalCoverageDate =
          NumberToString(DateToInt(
            entities.PersonalHealthInsurance.CoverageVerifiedDate), 8);
      }
      else
      {
        export.QuickCaseActivities.EnfMedicalCoverageDate =
          NumberToString(DateToInt(
            entities.PersonalHealthInsurance.CoverageBeginDate), 8);
      }
    }

    // *************************************************************************
    // Order Enforcement - Credit Bureau
    // *************************************************************************
    // Check for cancelations and deletions
    local.TempDateWorkArea.Date = local.Min.Date;

    if (ReadCreditReportingAction2())
    {
      local.TempDateWorkArea.Date = entities.CreditReportingAction.CraTransDate;
    }

    // Use most recent UPD date if available, otherwise use most recent ISS 
    // date.
    // Whichever date is chosen, it must be greater than any cancelation or 
    // stayed date
    if (ReadCreditReportingAction1())
    {
      export.QuickCaseActivities.EnfCreditBureauDate =
        NumberToString(DateToInt(entities.CreditReportingAction.CraTransDate), 8);
        
    }

    // *************************************************************************
    // Order Enforcement - SDSO
    // *************************************************************************
    if (ReadAdministrativeActCertification2())
    {
      // Most recent SDSO record is for decertification.  Do not return any date
      // for SDSO submission.
      if (Lt(local.Min.Date,
        entities.AdministrativeActCertification.DecertifiedDate))
      {
        goto Read1;
      }

      // Check to see if NCP has been given exemption for SDSO on *any* 
      // obligation.
      if (ReadObligationAdmActionExemption1())
      {
        // NCP has been exempted from SDSO on one obligation.  Do not return any
        // date for SDSO submission.
        goto Read1;
      }

      export.QuickCaseActivities.EnfSdsoDate =
        NumberToString(DateToInt(
          entities.AdministrativeActCertification.TakenDate), 8);
    }

Read1:

    // *************************************************************************
    // Order Enforcement - Lottery
    // Lottery interception is part of the SDSO.  Use the SDSO date for Lottery 
    // as well.
    // *************************************************************************
    export.QuickCaseActivities.EnfLotteryDate =
      export.QuickCaseActivities.EnfSdsoDate;

    // *************************************************************************
    // Order Enforcement - FDSO
    // *************************************************************************
    if (ReadAdministrativeActCertification1())
    {
      // Most recent FDSO record is for decertification.  Do not return any date
      // for FDSO submission.
      if (Lt(local.Min.Date,
        entities.AdministrativeActCertification.DecertifiedDate))
      {
        goto Read2;
      }

      // Check to see if NCP has been given exemption for FDSO on *any* 
      // obligation.
      if (ReadObligationAdmActionExemption3())
      {
        // NCP has been exempted from FDSO on one obligation.  Do not return any
        // date for FDSO submission.
        goto Read2;
      }

      export.QuickCaseActivities.EnfFsdoDate =
        NumberToString(DateToInt(
          entities.AdministrativeActCertification.TakenDate), 8);
    }

Read2:

    // *************************************************************************
    // Order Enforcement - Passport Denial
    // *************************************************************************
    if (ReadAdministrativeActCertification1())
    {
      // Most recent FDSO record is for decertification.  Do not return any date
      // for Passport Denial.
      if (Lt(local.Min.Date,
        entities.AdministrativeActCertification.DecertifiedDate))
      {
        goto Read3;
      }

      // Check to see if NCP has been given exemption for Passport Denial on *
      // any* obligation.
      if (ReadObligationAdmActionExemption2())
      {
        // NCP has been exempted from Passport Denial on one obligation.  Do not
        // return any date for Passport Denial submission.
        goto Read3;
      }

      export.QuickCaseActivities.EnfPassportDenialDate =
        NumberToString(DateToInt(
          entities.AdministrativeActCertification.TakenDate), 8);
    }

Read3:

    // *************************************************************************
    // Order Enforcement - FIDM
    // Look for garnishments that have resources attached to them and are non-
    // wage.
    // Valid events are:
    // 96 - 205 - FGARNRQNW
    // 96 - 201 - FGARNAFFT
    // 20 - 215 - GARNO
    // 20 - 216 - GARNREQ
    // Valid resources are:
    // BA - BANK ACCOUNT
    // CB - CASH BALANCE - FIDM
    // CD - TERM DEPOSIT CERTIFICATE - FIDM
    // CH - CHECKING/DEMAND DEPOSIT - FIDM
    // CM - COMPOUND ACCOUNT - FIDM
    // ER - ERISA PLAN ACCOUNT - FIDM
    // MM - MONEY MARKET ACCOUNT - FIDM
    // NA - NOT APPLICABLE - FIDM
    // *************************************************************************
    if (ReadInfrastructureEventDetailLegalActionLegalActionPersonResource())
    {
      export.QuickCaseActivities.EnfFidmDate =
        NumberToString(DateToInt(entities.LegalAction.FiledDate), 8);
      export.QuickCaseActivities.EnfFinancialInstitutionName =
        entities.CsePersonResource.Location ?? Spaces(40);
    }

    // *************************************************************************
    // Order Enforcement - Driver's License
    // *************************************************************************
    if (ReadKsDriversLicense())
    {
      export.QuickCaseActivities.EnfDriversLicenseDate =
        NumberToString(DateToInt(entities.KsDriversLicense.RestrictedDate), 8);
    }

    // *************************************************************************
    // Order Enforcement - Lien
    // *************************************************************************
    if (ReadObligationAdministrativeAction())
    {
      export.QuickCaseActivities.EnfLienDate =
        NumberToString(DateToInt(
          entities.ObligationAdministrativeAction.TakenDate), 8);
    }
  }

  private void UseSiQuickGetCpHeader()
  {
    var useImport = new SiQuickGetCpHeader.Import();
    var useExport = new SiQuickGetCpHeader.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickGetCpHeader.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    local.QuickCpHeader.Assign(useExport.QuickCpHeader);
  }

  private void UseSpDocCreateRetFldValTrg()
  {
    var useImport = new SpDocCreateRetFldValTrg.Import();
    var useExport = new SpDocCreateRetFldValTrg.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;

    Call(SpDocCreateRetFldValTrg.Execute, useImport, useExport);
  }

  private bool ReadAdministrativeActCertification1()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 7);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeActCertification2()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 7);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.CspNumber = db.GetString(reader, 5);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.CsePerson.Type1 = db.GetString(reader, 10);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 11);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 12);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 13);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCreditReportingAction1()
  {
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetNullableDate(
          command, "craTransDate",
          local.TempDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 2);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 3);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 4);
        entities.CreditReportingAction.AacType = db.GetString(reader, 5);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 6);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 7);
        entities.CreditReportingAction.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.CreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.CreditReportingAction.AacType);
      });
  }

  private bool ReadCreditReportingAction2()
  {
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 2);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 3);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 4);
        entities.CreditReportingAction.AacType = db.GetString(reader, 5);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 6);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 7);
        entities.CreditReportingAction.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.CreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.CreditReportingAction.AacType);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.QuickCpHeader.NcpPersonNumber);
        db.SetNullableDate(
          command, "dateOfDeath", local.Min.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 18);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "numb", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 4);
        entities.CaseRole.CasNumber = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePersonDetail1()
  {
    entities.CsePersonDetail.Populated = false;

    return Read("ReadCsePersonDetail1",
      (db, command) =>
      {
        db.SetString(command, "personNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDetail.PersonNumber = db.GetString(reader, 0);
        entities.CsePersonDetail.FirstName = db.GetString(reader, 1);
        entities.CsePersonDetail.LastName = db.GetString(reader, 2);
        entities.CsePersonDetail.MiddleInitial =
          db.GetNullableString(reader, 3);
        entities.CsePersonDetail.Populated = true;
      });
  }

  private bool ReadCsePersonDetail2()
  {
    entities.CsePersonDetail.Populated = false;

    return Read("ReadCsePersonDetail2",
      (db, command) =>
      {
        db.SetString(command, "personNumber", local.ChildTemp.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDetail.PersonNumber = db.GetString(reader, 0);
        entities.CsePersonDetail.FirstName = db.GetString(reader, 1);
        entities.CsePersonDetail.LastName = db.GetString(reader, 2);
        entities.CsePersonDetail.MiddleInitial =
          db.GetNullableString(reader, 3);
        entities.CsePersonDetail.Populated = true;
      });
  }

  private bool ReadFieldValue1()
  {
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infIdentifier",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue2()
  {
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infIdentifier",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.HealthInsuranceCoverage.Populated = false;
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetNullableDate(
          command, "coverEndDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 2);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 3);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 4);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 5);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 6);
        entities.CaseRole.CasNumber = db.GetString(reader, 7);
        entities.Case1.Number = db.GetString(reader, 7);
        entities.CaseRole.CspNumber = db.GetString(reader, 8);
        entities.CaseRole.Type1 = db.GetString(reader, 9);
        entities.CaseRole.Identifier = db.GetInt32(reader, 10);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 13);
        entities.Case1.Status = db.GetNullableString(reader, 14);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 15);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 16);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 5);
        entities.Incarceration.Incarcerated = db.GetNullableString(reader, 6);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          entities.Infrastructure.DenormTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Name = db.GetNullableString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetail1()
  {
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureEventDetail1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetail2()
  {
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureEventDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetail3()
  {
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureEventDetail3",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetail4()
  {
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureEventDetail4",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetail5()
  {
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureEventDetail5",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureEventDetailLegalActionLegalActionPersonResource()
    
  {
    entities.CsePersonResource.Populated = false;
    entities.EventDetail.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.LegalAction.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return Read(
      "ReadInfrastructureEventDetailLegalActionLegalActionPersonResource",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Min.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.EventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.EventDetail.EveNo = db.GetInt32(reader, 12);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 13);
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 14);
        entities.CsePersonResource.CspNumber = db.GetString(reader, 14);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 15);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 15);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 16);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 17);
        entities.CsePersonResource.Type1 = db.GetNullableString(reader, 18);
        entities.CsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 19);
        entities.CsePersonResource.Location = db.GetNullableString(reader, 20);
        entities.CsePersonResource.Populated = true;
        entities.EventDetail.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.LegalAction.Populated = true;
        entities.LegalActionPersonResource.Populated = true;
      });
  }

  private bool ReadInfrastructureLegalActionPersonLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Infrastructure.Populated = false;
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadInfrastructureLegalActionPersonLegalAction",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetDate(command, "effectiveDt", date);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "croId", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNum", entities.CaseRole.CspNumber);
        db.SetString(command, "casNum", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 11);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 12);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.LegalAction.Identifier = db.GetInt32(reader, 13);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 14);
        entities.LegalActionPerson.Role = db.GetString(reader, 15);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 16);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 17);
        entities.Infrastructure.Populated = true;
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadKsDriversLicense()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNum", local.QuickCpHeader.NcpPersonNumber);
        db.SetNullableDate(
          command, "restrictedDate1", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "restrictedDate2", local.Min.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 1);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 2);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 3);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailCsePerson()
  {
    entities.ChildCsePerson.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Min.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableString(
          command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 6);
        entities.ChildCsePerson.Number = db.GetString(reader, 7);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 8);
        entities.ChildCsePerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);

        return true;
      });
  }

  private bool ReadObligationAdmActionExemption1()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationAdmActionExemption2()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationAdmActionExemption3()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A QuickPersonAddrGroup group.</summary>
    [Serializable]
    public class QuickPersonAddrGroup
    {
      /// <summary>
      /// A value of Loc.
      /// </summary>
      [JsonPropertyName("loc")]
      public QuickPersonsWorkSet Loc
      {
        get => loc ??= new();
        set => loc = value;
      }

      /// <summary>
      /// A value of PersonRole.
      /// </summary>
      [JsonPropertyName("personRole")]
      public WorkArea PersonRole
      {
        get => personRole ??= new();
        set => personRole = value;
      }

      /// <summary>
      /// A value of QuickPersonAddress.
      /// </summary>
      [JsonPropertyName("quickPersonAddress")]
      public QuickPersonAddress QuickPersonAddress
      {
        get => quickPersonAddress ??= new();
        set => quickPersonAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPersonsWorkSet loc;
      private WorkArea personRole;
      private QuickPersonAddress quickPersonAddress;
    }

    /// <summary>A QuickOeGroup group.</summary>
    [Serializable]
    public class QuickOeGroup
    {
      /// <summary>
      /// A value of QuickOrderEstabInfo.
      /// </summary>
      [JsonPropertyName("quickOrderEstabInfo")]
      public QuickOrderEstabInfo QuickOrderEstabInfo
      {
        get => quickOrderEstabInfo ??= new();
        set => quickOrderEstabInfo = value;
      }

      /// <summary>
      /// A value of Oe.
      /// </summary>
      [JsonPropertyName("oe")]
      public QuickPersonsWorkSet Oe
      {
        get => oe ??= new();
        set => oe = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickOrderEstabInfo quickOrderEstabInfo;
      private QuickPersonsWorkSet oe;
    }

    /// <summary>A QuickPaternityGroup group.</summary>
    [Serializable]
    public class QuickPaternityGroup
    {
      /// <summary>
      /// A value of QuickPaternityInfo.
      /// </summary>
      [JsonPropertyName("quickPaternityInfo")]
      public QuickPaternityInfo QuickPaternityInfo
      {
        get => quickPaternityInfo ??= new();
        set => quickPaternityInfo = value;
      }

      /// <summary>
      /// A value of Pat.
      /// </summary>
      [JsonPropertyName("pat")]
      public QuickPersonsWorkSet Pat
      {
        get => pat ??= new();
        set => pat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPaternityInfo quickPaternityInfo;
      private QuickPersonsWorkSet pat;
    }

    /// <summary>
    /// Gets a value of QuickPersonAddr.
    /// </summary>
    [JsonIgnore]
    public Array<QuickPersonAddrGroup> QuickPersonAddr =>
      quickPersonAddr ??= new(QuickPersonAddrGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickPersonAddr for json serialization.
    /// </summary>
    [JsonPropertyName("quickPersonAddr")]
    [Computed]
    public IList<QuickPersonAddrGroup> QuickPersonAddr_Json
    {
      get => quickPersonAddr;
      set => QuickPersonAddr.Assign(value);
    }

    /// <summary>
    /// A value of QuickCaseActivities.
    /// </summary>
    [JsonPropertyName("quickCaseActivities")]
    public QuickCaseActivities QuickCaseActivities
    {
      get => quickCaseActivities ??= new();
      set => quickCaseActivities = value;
    }

    /// <summary>
    /// Gets a value of QuickOe.
    /// </summary>
    [JsonIgnore]
    public Array<QuickOeGroup> QuickOe => quickOe ??= new(
      QuickOeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickOe for json serialization.
    /// </summary>
    [JsonPropertyName("quickOe")]
    [Computed]
    public IList<QuickOeGroup> QuickOe_Json
    {
      get => quickOe;
      set => QuickOe.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickPaternity.
    /// </summary>
    [JsonIgnore]
    public Array<QuickPaternityGroup> QuickPaternity => quickPaternity ??= new(
      QuickPaternityGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickPaternity for json serialization.
    /// </summary>
    [JsonPropertyName("quickPaternity")]
    [Computed]
    public IList<QuickPaternityGroup> QuickPaternity_Json
    {
      get => quickPaternity;
      set => QuickPaternity.Assign(value);
    }

    /// <summary>
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
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
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
    }

    private Array<QuickPersonAddrGroup> quickPersonAddr;
    private QuickCaseActivities quickCaseActivities;
    private Array<QuickOeGroup> quickOe;
    private Array<QuickPaternityGroup> quickPaternity;
    private QuickCpHeader quickCpHeader;
    private Case1 case1;
    private QuickErrorMessages quickErrorMessages;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of NcpMailAddrFound.
    /// </summary>
    [JsonPropertyName("ncpMailAddrFound")]
    public Common NcpMailAddrFound
    {
      get => ncpMailAddrFound ??= new();
      set => ncpMailAddrFound = value;
    }

    /// <summary>
    /// A value of NcpResiAddrFound.
    /// </summary>
    [JsonPropertyName("ncpResiAddrFound")]
    public Common NcpResiAddrFound
    {
      get => ncpResiAddrFound ??= new();
      set => ncpResiAddrFound = value;
    }

    /// <summary>
    /// A value of MailAddrFound.
    /// </summary>
    [JsonPropertyName("mailAddrFound")]
    public Common MailAddrFound
    {
      get => mailAddrFound ??= new();
      set => mailAddrFound = value;
    }

    /// <summary>
    /// A value of ResiAddrFound.
    /// </summary>
    [JsonPropertyName("resiAddrFound")]
    public Common ResiAddrFound
    {
      get => resiAddrFound ??= new();
      set => resiAddrFound = value;
    }

    /// <summary>
    /// A value of ChildTemp.
    /// </summary>
    [JsonPropertyName("childTemp")]
    public CsePerson ChildTemp
    {
      get => childTemp ??= new();
      set => childTemp = value;
    }

    /// <summary>
    /// A value of CsOrderFound.
    /// </summary>
    [JsonPropertyName("csOrderFound")]
    public Common CsOrderFound
    {
      get => csOrderFound ??= new();
      set => csOrderFound = value;
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
    /// A value of Fv.
    /// </summary>
    [JsonPropertyName("fv")]
    public WorkArea Fv
    {
      get => fv ??= new();
      set => fv = value;
    }

    /// <summary>
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of MsOrderFound.
    /// </summary>
    [JsonPropertyName("msOrderFound")]
    public Common MsOrderFound
    {
      get => msOrderFound ??= new();
      set => msOrderFound = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of OeCsMaxDate.
    /// </summary>
    [JsonPropertyName("oeCsMaxDate")]
    public DateWorkArea OeCsMaxDate
    {
      get => oeCsMaxDate ??= new();
      set => oeCsMaxDate = value;
    }

    /// <summary>
    /// A value of OeMsMaxDate.
    /// </summary>
    [JsonPropertyName("oeMsMaxDate")]
    public DateWorkArea OeMsMaxDate
    {
      get => oeMsMaxDate ??= new();
      set => oeMsMaxDate = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of PatDate.
    /// </summary>
    [JsonPropertyName("patDate")]
    public TextWorkArea PatDate
    {
      get => patDate ??= new();
      set => patDate = value;
    }

    /// <summary>
    /// A value of PatTypeInd.
    /// </summary>
    [JsonPropertyName("patTypeInd")]
    public Common PatTypeInd
    {
      get => patTypeInd ??= new();
      set => patTypeInd = value;
    }

    /// <summary>
    /// A value of TempDateWorkArea.
    /// </summary>
    [JsonPropertyName("tempDateWorkArea")]
    public DateWorkArea TempDateWorkArea
    {
      get => tempDateWorkArea ??= new();
      set => tempDateWorkArea = value;
    }

    /// <summary>
    /// A value of TempIncomeSource.
    /// </summary>
    [JsonPropertyName("tempIncomeSource")]
    public IncomeSource TempIncomeSource
    {
      get => tempIncomeSource ??= new();
      set => tempIncomeSource = value;
    }

    /// <summary>
    /// A value of TempPaternityDateWorkArea.
    /// </summary>
    [JsonPropertyName("tempPaternityDateWorkArea")]
    public DateWorkArea TempPaternityDateWorkArea
    {
      get => tempPaternityDateWorkArea ??= new();
      set => tempPaternityDateWorkArea = value;
    }

    /// <summary>
    /// A value of TempPaternityCommon.
    /// </summary>
    [JsonPropertyName("tempPaternityCommon")]
    public Common TempPaternityCommon
    {
      get => tempPaternityCommon ??= new();
      set => tempPaternityCommon = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private QuickCpHeader quickCpHeader;
    private Common ncpMailAddrFound;
    private Common ncpResiAddrFound;
    private Common mailAddrFound;
    private Common resiAddrFound;
    private CsePerson childTemp;
    private Common csOrderFound;
    private DateWorkArea current;
    private WorkArea fv;
    private DateWorkArea min;
    private DateWorkArea max;
    private Common msOrderFound;
    private DateWorkArea null1;
    private ObligationType obligationType;
    private DateWorkArea oeCsMaxDate;
    private DateWorkArea oeMsMaxDate;
    private Common position;
    private CsePerson previous;
    private TextWorkArea patDate;
    private Common patTypeInd;
    private DateWorkArea tempDateWorkArea;
    private IncomeSource tempIncomeSource;
    private DateWorkArea tempPaternityDateWorkArea;
    private Common tempPaternityCommon;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApLegalActionPerson.
    /// </summary>
    [JsonPropertyName("apLegalActionPerson")]
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ChildLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("childLegalActionCaseRole")]
    public LegalActionCaseRole ChildLegalActionCaseRole
    {
      get => childLegalActionCaseRole ??= new();
      set => childLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChildLegalActionPerson.
    /// </summary>
    [JsonPropertyName("childLegalActionPerson")]
    public LegalActionPerson ChildLegalActionPerson
    {
      get => childLegalActionPerson ??= new();
      set => childLegalActionPerson = value;
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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CsePersonDetail.
    /// </summary>
    [JsonPropertyName("csePersonDetail")]
    public CsePersonDetail CsePersonDetail
    {
      get => csePersonDetail ??= new();
      set => csePersonDetail = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeAction administrativeAction;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LegalActionPerson apLegalActionPerson;
    private Case1 case1;
    private CaseRole caseRole;
    private CaseRole childCaseRole;
    private CsePerson childCsePerson;
    private LegalActionCaseRole childLegalActionCaseRole;
    private LegalActionPerson childLegalActionPerson;
    private Code code;
    private CodeValue codeValue;
    private CreditReportingAction creditReportingAction;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonAddress csePersonAddress;
    private CsePersonDetail csePersonDetail;
    private CsePersonResource csePersonResource;
    private DocumentField documentField;
    private EventDetail eventDetail;
    private Field field;
    private FieldValue fieldValue;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Incarceration incarceration;
    private IncomeSource incomeSource;
    private Infrastructure infrastructure;
    private KsDriversLicense ksDriversLicense;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionDetail legalActionDetail;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalActionPerson legalActionPerson;
    private LegalActionPersonResource legalActionPersonResource;
    private Obligation obligation;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private ObligationType obligationType;
    private OutgoingDocument outgoingDocument;
    private PersonalHealthInsurance personalHealthInsurance;
  }
#endregion
}
