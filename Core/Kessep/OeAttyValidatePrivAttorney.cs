// Program: OE_ATTY_VALIDATE_PRIV_ATTORNEY, ID: 372179493, model: 746.
// Short name: SWE00860
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
/// A program: OE_ATTY_VALIDATE_PRIV_ATTORNEY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block validates Attorney details.
/// </para>
/// </summary>
[Serializable]
public partial class OeAttyValidatePrivAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_VALIDATE_PRIV_ATTORNEY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyValidatePrivAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyValidatePrivAttorney.
  /// </summary>
  public OeAttyValidatePrivAttorney(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block validates Person Private Attorney details.
    // PROCESSING:
    // ACTION BLOCKS:
    // This action block calls CAB_VALIDATE_CODE_VALUE to validate State and 
    // Country Codes.
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  PERSON_PRIVATE_ATTORNEY	- R - -
    //  PRIVATE_ATTORNEY_ADDRESS	- R - -
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	02/22/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE	CHG REQ#	DESCRIPTION
    // govind		2/22/95			Initial coding
    // A.Kinney	04/29/97		Changed Current_Date
    // A. Convery      02/26/04    201317      Required entry of Street1, City, 
    // State,  Zip
    // A. Convery      12/01/05    PR# 232681  Allow entry of Attorney without 
    // Case Number
    // JHarden     03/17/17     CQ53818  Add email address to ATTY screen
    // JHarden    5/26/2017  CQ57453 Add fields consent, note, and bar #.
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();
    export.ErrorCodes.Index = -1;
    export.LastErrorEntryNo.Count = 0;
    export.CsePerson.Number = import.CsePerson.Number;
    MovePersonPrivateAttorney(import.PersonPrivateAttorney,
      export.PersonPrivateAttorney);
    export.PrivateAttorneyAddress.Assign(import.PrivateAttorneyAddress);
    export.CsePersonsWorkSet.FormattedName = "";

    if (ReadCsePerson())
    {
      UseCabGetClientDetails();
    }
    else
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 1;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (!ReadCase())
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 12;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    // ---------------------------------------------
    // For CREATE, edit Court Case info.
    // ---------------------------------------------
    if (Equal(import.UserAction.Command, "CREATE"))
    {
      if (import.PersonPrivateAttorney.Identifier > 0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 34;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      // ****	No Court Case Info entered
      if (IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber) && IsEmpty
        (import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
        (import.PersonPrivateAttorney.FipsStateAbbreviation) && IsEmpty
        (import.PersonPrivateAttorney.TribCountry))
      {
        // ****************************************************************
        // * Read for Legal actions related to Case
        // ****************************************************************
        local.LegalActionPerson1.Index = -1;

        foreach(var item in ReadLegalActionLegalActionPersonTribunalFips())
        {
          ++local.LegalActionPerson1.Index;
          local.LegalActionPerson1.CheckSize();

          if (local.LegalActionPerson1.Index >= Local
            .LegalActionPerson1Group.Capacity)
          {
            break;
          }

          local.LegalActionPerson1.Update.Grp1LegalAction.Assign(
            entities.ExistingLegalAction);
          local.LegalActionPerson1.Update.Grp1LegalActionPerson.Assign(
            entities.ExistingLegalActionPerson);
          MoveFips(entities.ExistingFips,
            local.LegalActionPerson1.Update.Grp1Fips);
        }

        local.LegalActionPerson1.Index = local.LegalActionPerson1.Count - 1;
        local.LegalActionPerson1.CheckSize();

        foreach(var item in ReadLegalActionLegalActionPersonTribunalFipsTribAddress())
          
        {
          if (IsEmpty(entities.ExistingFipsTribAddress.Country))
          {
            continue;
          }

          ++local.LegalActionPerson1.Index;
          local.LegalActionPerson1.CheckSize();

          if (local.LegalActionPerson1.Index >= Local
            .LegalActionPerson1Group.Capacity)
          {
            break;
          }

          local.LegalActionPerson1.Update.Grp1LegalAction.Assign(
            entities.ExistingLegalAction);
          local.LegalActionPerson1.Update.Grp1LegalActionPerson.Assign(
            entities.ExistingLegalActionPerson);
          local.LegalActionPerson1.Update.Grp1FipsTribAddress.Country =
            entities.ExistingFipsTribAddress.Country;
        }

        // ****************************************************************
        // * Compare current to prior entries on result group view.  If a break
        // * in key (CC#, County, State, Country) then move the prior
        // * entry to group view 2.  Group view 2 will contain only the unique
        // * entries.  Bypass any court case (all legal actions) that has been 
        // dismissed.
        // ****************************************************************
        local.PrevCourtCaseDismissed.Flag = "N";
        local.LegalActionPerson2.Index = -1;

        for(local.LegalActionPerson1.Index = 0; local
          .LegalActionPerson1.Index < local.LegalActionPerson1.Count; ++
          local.LegalActionPerson1.Index)
        {
          if (!local.LegalActionPerson1.CheckSize())
          {
            break;
          }

          if (local.LegalActionPerson1.Index == 0)
          {
            local.PrevLegalAction.Assign(
              local.LegalActionPerson1.Item.Grp1LegalAction);
            local.PrevLegalActionPerson.Assign(
              local.LegalActionPerson1.Item.Grp1LegalActionPerson);
            MoveFips(local.LegalActionPerson1.Item.Grp1Fips, local.PrevFips);
            local.PrevFipsTribAddress.Country =
              local.LegalActionPerson1.Item.Grp1FipsTribAddress.Country;
          }

          if (!Equal(local.LegalActionPerson1.Item.Grp1LegalAction.
            CourtCaseNumber, local.PrevLegalAction.CourtCaseNumber) || !
            Equal(local.LegalActionPerson1.Item.Grp1Fips.CountyAbbreviation,
            local.PrevFips.CountyAbbreviation) || !
            Equal(local.LegalActionPerson1.Item.Grp1Fips.StateAbbreviation,
            local.PrevFips.StateAbbreviation) || !
            Equal(local.LegalActionPerson1.Item.Grp1FipsTribAddress.Country,
            local.PrevFipsTribAddress.Country))
          {
            if (AsChar(local.PrevCourtCaseDismissed.Flag) == 'Y')
            {
              local.PrevCourtCaseDismissed.Flag = "N";
            }
            else
            {
              ++local.LegalActionPerson2.Index;
              local.LegalActionPerson2.CheckSize();

              local.LegalActionPerson2.Update.Grp2LegalAction.Assign(
                local.PrevLegalAction);
              local.LegalActionPerson2.Update.Grp2LegalActionPerson.Assign(
                local.PrevLegalActionPerson);
              MoveFips(local.PrevFips, local.LegalActionPerson2.Update.Grp2Fips);
                
              local.LegalActionPerson2.Update.Grp2FipsTribAddress.Country =
                local.PrevFipsTribAddress.Country;
            }

            local.PrevLegalAction.Assign(
              local.LegalActionPerson1.Item.Grp1LegalAction);
            local.PrevLegalActionPerson.Assign(
              local.LegalActionPerson1.Item.Grp1LegalActionPerson);
            MoveFips(local.LegalActionPerson1.Item.Grp1Fips, local.PrevFips);
            local.PrevFipsTribAddress.Country =
              local.LegalActionPerson1.Item.Grp1FipsTribAddress.Country;
          }

          if (!IsEmpty(local.LegalActionPerson1.Item.Grp1LegalAction.
            DismissedWithoutPrejudiceInd))
          {
            local.PrevCourtCaseDismissed.Flag = "Y";
          }
        }

        local.LegalActionPerson1.CheckIndex();

        if (AsChar(local.PrevCourtCaseDismissed.Flag) == 'N' && !
          IsEmpty(local.PrevLegalAction.CourtCaseNumber))
        {
          ++local.LegalActionPerson2.Index;
          local.LegalActionPerson2.CheckSize();

          local.LegalActionPerson2.Update.Grp2LegalAction.Assign(
            local.PrevLegalAction);
          local.LegalActionPerson2.Update.Grp2LegalActionPerson.Assign(
            local.PrevLegalActionPerson);
          MoveFips(local.PrevFips, local.LegalActionPerson2.Update.Grp2Fips);
          local.LegalActionPerson2.Update.Grp2FipsTribAddress.Country =
            local.PrevFipsTribAddress.Country;
        }

        for(local.LegalActionPerson2.Index = 0; local
          .LegalActionPerson2.Index < local.LegalActionPerson2.Count; ++
          local.LegalActionPerson2.Index)
        {
          if (!local.LegalActionPerson2.CheckSize())
          {
            break;
          }

          local.AttorneyRecFound.Flag = "N";

          foreach(var item in ReadPersonPrivateAttorney4())
          {
            if (Lt(entities.ExistingPersonPrivateAttorney.DateDismissed,
              local.Current.Date) && !
              Equal(entities.ExistingPersonPrivateAttorney.DateDismissed,
              local.Zero.Date))
            {
              continue;
            }

            if (Equal(entities.ExistingPersonPrivateAttorney.CourtCaseNumber,
              local.LegalActionPerson2.Item.Grp2LegalAction.CourtCaseNumber) &&
              Equal
              (entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation,
              local.LegalActionPerson2.Item.Grp2Fips.CountyAbbreviation) && Equal
              (entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation,
              local.LegalActionPerson2.Item.Grp2Fips.StateAbbreviation) && Equal
              (entities.ExistingPersonPrivateAttorney.TribCountry,
              local.LegalActionPerson2.Item.Grp2FipsTribAddress.Country))
            {
              local.AttorneyRecFound.Flag = "Y";

              break;
            }
          }

          // ****	If you haven't read an ATTY for the legal action then error
          if (AsChar(local.AttorneyRecFound.Flag) == 'N')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 22;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }

        local.LegalActionPerson2.CheckIndex();
      }
      else
      {
        // ****	Court Case Info entered
        // Andrew Convery 12/01/05  PR# 232681 Allow entry of Attorney without 
        // Case Number.
        // Error Check below commented out.
        if ((!IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
          IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation)) && !
          IsEmpty(import.PersonPrivateAttorney.TribCountry))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 24;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.FipsStateAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.TribCountry))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 25;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (!IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 26;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && !
          IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 27;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        // ****	Validate State code
        if (!IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue =
            import.PersonPrivateAttorney.FipsStateAbbreviation ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 28;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
          else
          {
            // ****	Validate County code, FIPS contains all the valid
            // 	State/County combinations
            if (ReadFips1())
            {
              goto Test1;
            }

            // ****	If you haven't ESCAPEd then no FIPS found, since
            // 	the state was already validated the county must
            // 	be invalid since FIPS contains all ST/CT valid combinations
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 29;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
        else
        {
          // ****	Validate Country code
          local.Code.CodeName = "COUNTRY CODE";
          local.CodeValue.Cdvalue =
            import.PersonPrivateAttorney.TribCountry ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 30;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }

Test1:

        // ****	Validate that the Court Case Info is related to the
        // 	Case and CSE Person
        foreach(var item in ReadLegalActionLegalActionPersonTribunal2())
        {
          if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
            import.PersonPrivateAttorney.CourtCaseNumber))
          {
            continue;
          }

          if (!IsEmpty(entities.ExistingLegalAction.DismissedWithoutPrejudiceInd))
            
          {
            local.DismissedLegalAction.Flag = "Y";
          }

          if (ReadFips2())
          {
            if (Equal(entities.ExistingFips.CountyAbbreviation,
              import.PersonPrivateAttorney.FipsCountyAbbreviation) && Equal
              (entities.ExistingFips.StateAbbreviation,
              import.PersonPrivateAttorney.FipsStateAbbreviation))
            {
              local.LegalActionFound.Flag = "Y";
            }
          }
          else if (ReadFipsTribAddress())
          {
            if (Equal(entities.ExistingFipsTribAddress.Country,
              import.PersonPrivateAttorney.TribCountry))
            {
              local.LegalActionFound.Flag = "Y";
            }
          }
        }

        // ****	If the Court Case is not found for the entered Tribunal
        // 	then the Court Case Info is not related to CSE Case
        // ****    Andrew Convery 12/01/05  PR# 232681 'AND ...' added to
        //         allow entry of lawyer without Court Case Number.
        if (AsChar(local.LegalActionFound.Flag) != 'Y' && !
          IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 31;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        // ****	If the Court Case is dismissed then error
        if (AsChar(local.LegalActionFound.Flag) != 'Y')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 35;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        // ****	Don't allow entry of Court Case info if an existing
        // 	active attorney already has the same info.
        foreach(var item in ReadPersonPrivateAttorney3())
        {
          if (Lt(entities.ExistingPersonPrivateAttorney.DateDismissed,
            local.Current.Date) && !
            Equal(entities.ExistingPersonPrivateAttorney.DateDismissed,
            local.Zero.Date))
          {
            continue;
          }

          if (Equal(entities.ExistingPersonPrivateAttorney.CourtCaseNumber,
            import.PersonPrivateAttorney.CourtCaseNumber) && Equal
            (entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation,
            import.PersonPrivateAttorney.FipsCountyAbbreviation) && Equal
            (entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation,
            import.PersonPrivateAttorney.FipsStateAbbreviation) && Equal
            (entities.ExistingPersonPrivateAttorney.TribCountry,
            import.PersonPrivateAttorney.TribCountry))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 32;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
      }
    }

    // ---------------------------------------------
    // For UPDATE, edit Court Case info.
    // ---------------------------------------------
    if (Equal(import.UserAction.Command, "UPDATE"))
    {
      if (!ReadPersonPrivateAttorney2())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      // ****	 No change in Court Case info, remains blank
      if (IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber) && IsEmpty
        (import.PersonPrivateAttorney.CourtCaseNumber) && IsEmpty
        (import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
        (import.PersonPrivateAttorney.FipsStateAbbreviation) && IsEmpty
        (import.PersonPrivateAttorney.TribCountry))
      {
        goto Test3;
      }

      // ****	Not allowed to change existing Court Case info
      if (!IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber) && (
        !Equal(import.PersonPrivateAttorney.CourtCaseNumber,
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber) || !
        Equal(import.PersonPrivateAttorney.FipsCountyAbbreviation,
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation) || !
        Equal(import.PersonPrivateAttorney.FipsStateAbbreviation,
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation) || !
        Equal(import.PersonPrivateAttorney.TribCountry,
        entities.ExistingPersonPrivateAttorney.TribCountry)))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 33;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
      else if (IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
      {
        // ****	Court Case Info entered and existing is blank
        if (IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 23;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if ((!IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
          IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation)) && !
          IsEmpty(import.PersonPrivateAttorney.TribCountry))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 24;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.FipsStateAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.TribCountry))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 25;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (!IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && IsEmpty
          (import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 26;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) && !
          IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 27;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        // ****	Validate State code
        if (!IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue =
            import.PersonPrivateAttorney.FipsStateAbbreviation ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 28;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
          else
          {
            // ****	Validate County code, FIPS contains all the valid
            // 	State/County combinations
            if (ReadFips1())
            {
              goto Test2;
            }

            // ****	If you haven't ESCAPEd then no FIPS found, since
            // 	the state was already validated the county must
            // 	be invalid since FIPS contains all ST/CT valid combinations
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 29;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
        else
        {
          // ****	Validate Country code
          local.Code.CodeName = "COUNTRY CODE";
          local.CodeValue.Cdvalue =
            import.PersonPrivateAttorney.TribCountry ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 30;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }

Test2:

        // ****	Validate that the Court Case Info is related to the
        // 	Case and CSE Person
        foreach(var item in ReadLegalActionLegalActionPersonTribunal1())
        {
          if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
            import.PersonPrivateAttorney.CourtCaseNumber))
          {
            continue;
          }

          if (ReadFips2())
          {
            if (Equal(entities.ExistingFips.CountyAbbreviation,
              import.PersonPrivateAttorney.FipsCountyAbbreviation) && Equal
              (entities.ExistingFips.StateAbbreviation,
              import.PersonPrivateAttorney.FipsStateAbbreviation))
            {
              local.LegalActionFound.Flag = "Y";

              if (AsChar(entities.ExistingLegalActionPerson.AccountType) == 'R')
              {
                local.DismissedLegalAction.Flag = "Y";
              }

              break;
            }
          }
          else if (ReadFipsTribAddress())
          {
            if (Equal(entities.ExistingFipsTribAddress.Country,
              import.PersonPrivateAttorney.TribCountry))
            {
              local.LegalActionFound.Flag = "Y";

              if (AsChar(entities.ExistingLegalActionPerson.AccountType) == 'R')
              {
                local.DismissedLegalAction.Flag = "Y";
              }

              break;
            }
          }
        }

        // ****	If the Court Case is not found for the entered Tribunal
        // 	then the Court Case Info is not related to CSE Person
        // ****    Andrew Convery 12/01/05  PR# 232681 'AND ...' added to
        //         allow entry of lawyer without Court Case Number.
        if (AsChar(local.LegalActionFound.Flag) != 'Y' && !
          IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 31;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }

      // ****	Don't allow entry of Court Case info if an existing
      // 	active attorney already has the same info.
      foreach(var item in ReadPersonPrivateAttorney3())
      {
        if (Lt(entities.ExistingPersonPrivateAttorney.DateDismissed,
          local.Current.Date) && !
          Equal(entities.ExistingPersonPrivateAttorney.DateDismissed,
          local.Zero.Date))
        {
          continue;
        }

        if (Equal(entities.ExistingPersonPrivateAttorney.CourtCaseNumber,
          import.PersonPrivateAttorney.CourtCaseNumber) && Equal
          (entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation,
          import.PersonPrivateAttorney.FipsCountyAbbreviation) && Equal
          (entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation,
          import.PersonPrivateAttorney.FipsStateAbbreviation) && Equal
          (entities.ExistingPersonPrivateAttorney.TribCountry,
          import.PersonPrivateAttorney.TribCountry))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 32;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }
    }

Test3:

    // ---------------------------------------------
    // For UPDATE or DELETE, the cse_person, case and person_private_attorney 
    // records must exist. Allow update or deletion of the latest record only.
    // ---------------------------------------------
    // **********************************************************************************************
    // For DELETE, can only delete if not tied to a legal action.  Deletion of
    // the latest record only is no longer true.  C Locke 6/01/2007
    // 
    // **********************************************************************************************
    if (Equal(import.UserAction.Command, "UPDATE"))
    {
      if (!ReadPersonPrivateAttorney1())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      local.AttorneyAddressFound.Flag = "N";

      if (ReadPrivateAttorneyAddress())
      {
        local.AttorneyAddressFound.Flag = "Y";
      }
    }

    if (Equal(import.UserAction.Command, "DELETE"))
    {
      if (ReadPersonPrivateAttorneyCase())
      {
        // **********************************************************************************************
        // The edit for whether the case is open in order to delete was removed 
        // per Jolene Bickel 6-01-2007.
        // 
        // **********************************************************************************************
        foreach(var item in ReadLegalAction())
        {
          if (!IsEmpty(entities.ExistingLegalAction.CourtCaseNumber) && !
            IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber) && Equal
            (entities.ExistingLegalAction.CourtCaseNumber,
            entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 44;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
          else
          {
            continue;
          }

          goto Read;
        }
      }
      else
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

Read:

      local.AttorneyAddressFound.Flag = "N";

      if (ReadPrivateAttorneyAddress())
      {
        local.AttorneyAddressFound.Flag = "Y";
      }
    }

    // ---------------------------------------------
    // Validate individual fields.
    // ---------------------------------------------
    if (Equal(import.UserAction.Command, "CREATE") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (Lt(local.Current.Date, export.PersonPrivateAttorney.DateRetained))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 17;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!Equal(export.PersonPrivateAttorney.DateDismissed,
        local.InitZeroSetToMax.Date) && !
        Equal(export.PersonPrivateAttorney.DateDismissed, new DateTime(2099, 12,
        31)) && !Equal(export.PersonPrivateAttorney.DateDismissed, null))
      {
        if (Lt(export.PersonPrivateAttorney.DateDismissed,
          export.PersonPrivateAttorney.DateRetained))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 4;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (Lt(local.Current.Date, export.PersonPrivateAttorney.DateDismissed))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 36;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (!IsEmpty(export.PersonPrivateAttorney.LastName) || !
        IsEmpty(export.PersonPrivateAttorney.MiddleInitial))
      {
        if (IsEmpty(export.PersonPrivateAttorney.FirstName))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 6;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (!IsEmpty(export.PersonPrivateAttorney.FirstName) || !
        IsEmpty(export.PersonPrivateAttorney.MiddleInitial))
      {
        if (IsEmpty(export.PersonPrivateAttorney.LastName))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 5;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      // ---------------------------------------------
      // Either firm name or attorney name must be specified
      // ---------------------------------------------
      if (IsEmpty(export.PersonPrivateAttorney.FirstName) && IsEmpty
        (export.PersonPrivateAttorney.LastName) && IsEmpty
        (export.PersonPrivateAttorney.FirmName))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 13;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!Lt(new DateTime(1, 1, 1), export.PrivateAttorneyAddress.EffectiveDate))
        
      {
        export.PrivateAttorneyAddress.EffectiveDate = local.Current.Date;
      }

      // ****************************************
      // 02/26/04 -  A Convery - CR# 201317 - Moved the code below out of the
      // IF export_private_attorney_address ... which is commented out below.
      // ****************************************
      if (IsEmpty(export.PrivateAttorneyAddress.Street1))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 8;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      // ************************************************************
      // 02/26/04 - A Convery - CR# 201317 - Removed first line of IF:
      //  'IF export private_attorney_address street_1 is not equal to spaces'
      // ************************************************************
      if (IsEmpty(export.PrivateAttorneyAddress.City) || IsEmpty
        (export.PrivateAttorneyAddress.State))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 37;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      local.Code.CodeName = "STATE CODE";
      local.CodeValue.Cdvalue = export.PrivateAttorneyAddress.State ?? Spaces
        (10);
      UseCabValidateCodeValue2();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 10;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (IsEmpty(export.PrivateAttorneyAddress.ZipCode5))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 15;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
      else
      {
        if (Length(TrimEnd(export.PrivateAttorneyAddress.ZipCode5)) > 0 && Length
          (TrimEnd(export.PrivateAttorneyAddress.ZipCode5)) < 5)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 38;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (Length(TrimEnd(export.PrivateAttorneyAddress.ZipCode5)) > 0 && Verify
          (TrimEnd(export.PrivateAttorneyAddress.ZipCode5), "0123456789") != 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 39;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (Length(TrimEnd(export.PrivateAttorneyAddress.ZipCode5)) == 0 && Length
          (TrimEnd(export.PrivateAttorneyAddress.ZipCode4)) > 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 40;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (Length(TrimEnd(export.PrivateAttorneyAddress.ZipCode5)) > 0 && Length
          (TrimEnd(export.PrivateAttorneyAddress.ZipCode4)) > 0)
        {
          if (Length(TrimEnd(export.PrivateAttorneyAddress.ZipCode4)) < 4)
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 41;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
          else if (Verify(export.PrivateAttorneyAddress.ZipCode4, "0123456789") !=
            0)
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 42;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }

      if (!IsEmpty(export.PrivateAttorneyAddress.Country))
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = export.PrivateAttorneyAddress.Country ?? Spaces
          (10);
        UseCabValidateCodeValue2();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 11;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (export.PersonPrivateAttorney.PhoneAreaCode.GetValueOrDefault() != 0
        || export.PersonPrivateAttorney.Phone.GetValueOrDefault() != 0 || !
        IsEmpty(export.PersonPrivateAttorney.PhoneExt))
      {
        if (export.PersonPrivateAttorney.PhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 18;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (export.PersonPrivateAttorney.Phone.GetValueOrDefault() == 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 19;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (export.PersonPrivateAttorney.FaxNumberAreaCode.GetValueOrDefault() !=
        0 || export.PersonPrivateAttorney.FaxNumber.GetValueOrDefault() != 0
        || !IsEmpty(export.PersonPrivateAttorney.FaxExt))
      {
        if (export.PersonPrivateAttorney.FaxNumberAreaCode.
          GetValueOrDefault() == 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 20;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (export.PersonPrivateAttorney.FaxNumber.GetValueOrDefault() == 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 21;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      local.CurrentPosition.Count = 1;
      local.EmailVerify.Count = 0;

      // CQ53818 add email address to ATTY screen
      if (!IsEmpty(export.PersonPrivateAttorney.EmailAddress))
      {
        do
        {
          if (local.CurrentPosition.Count >= 60)
          {
            break;
          }

          local.Postion.Text1 =
            Substring(export.PersonPrivateAttorney.EmailAddress,
            local.CurrentPosition.Count, 1);

          if (AsChar(local.Postion.Text1) == '@')
          {
            if (local.CurrentPosition.Count <= 1)
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 45;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

              return;
            }

            local.EmailVerify.Count = local.CurrentPosition.Count + 5;

            if (IsEmpty(Substring(
              export.PersonPrivateAttorney.EmailAddress,
              local.EmailVerify.Count, 1)))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 45;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

              return;
            }

            break;
          }

          ++local.CurrentPosition.Count;
        }
        while(!Equal(global.Command, "COMMAND"));

        if (local.EmailVerify.Count <= 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 45;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MovePersonPrivateAttorney(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.FaxNumberAreaCode = source.FaxNumberAreaCode;
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.FaxExt = source.FaxExt;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FirmName = source.FirmName;
    target.Phone = source.Phone;
    target.FaxNumber = source.FaxNumber;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.FipsStateAbbreviation = source.FipsStateAbbreviation;
    target.FipsCountyAbbreviation = source.FipsCountyAbbreviation;
    target.TribCountry = source.TribCountry;
    target.EmailAddress = source.EmailAddress;
    target.BarNumber = source.BarNumber;
    target.ConsentIndicator = source.ConsentIndicator;
    target.Note = source.Note;
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.InitZeroSetToMax.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.InitZeroSetToMax.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          import.PersonPrivateAttorney.FipsStateAbbreviation ?? "");
        db.SetNullableString(
          command, "countyAbbr",
          import.PersonPrivateAttorney.FipsCountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
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
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionPersonTribunal1()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPersonTribunal1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 5);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 9);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionPersonTribunal2()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPersonTribunal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 5);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 9);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionPersonTribunalFips()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingFips.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPersonTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 5);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 9);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.ExistingFips.Location = db.GetInt32(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingFips.County = db.GetInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingFips.State = db.GetInt32(reader, 14);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 15);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 16);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingFips.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionLegalActionPersonTribunalFipsTribAddress()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingFipsTribAddress.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPersonTribunalFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 5);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 9);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 15);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 16);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPrivateAttorney1()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(command, "casNumber", import.Case1.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney2()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney3()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
        db.
          SetNullableString(command, "casNumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney4()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.
          SetNullableString(command, "casNumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPrivateAttorneyCase()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;
    entities.ExistingCase.Populated = false;

    return Read("ReadPersonPrivateAttorneyCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "numb", import.Case1.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCase.Number = db.GetString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 20);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 26);
        entities.ExistingCase.Status = db.GetNullableString(reader, 27);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 28);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 29);
        entities.ExistingPersonPrivateAttorney.Populated = true;
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadPrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);
    entities.ExistingPrivateAttorneyAddress.Populated = false;

    return Read("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier",
          entities.ExistingPersonPrivateAttorney.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonPrivateAttorney.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrivateAttorneyAddress.PpaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrivateAttorneyAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrivateAttorneyAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingPrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingPrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingPrivateAttorneyAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingPrivateAttorneyAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingPrivateAttorneyAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingPrivateAttorneyAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingPrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingPrivateAttorneyAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingPrivateAttorneyAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingPrivateAttorneyAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingPrivateAttorneyAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingPrivateAttorneyAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPrivateAttorneyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPrivateAttorneyAddress.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Common userAction;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePerson csePerson;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
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
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
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
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private CodeValue country;
    private CsePerson csePerson;
    private PersonPrivateAttorney personPrivateAttorney;
    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalActionPerson1Group group.</summary>
    [Serializable]
    public class LegalActionPerson1Group
    {
      /// <summary>
      /// A value of Grp1LegalAction.
      /// </summary>
      [JsonPropertyName("grp1LegalAction")]
      public LegalAction Grp1LegalAction
      {
        get => grp1LegalAction ??= new();
        set => grp1LegalAction = value;
      }

      /// <summary>
      /// A value of Grp1LegalActionPerson.
      /// </summary>
      [JsonPropertyName("grp1LegalActionPerson")]
      public LegalActionPerson Grp1LegalActionPerson
      {
        get => grp1LegalActionPerson ??= new();
        set => grp1LegalActionPerson = value;
      }

      /// <summary>
      /// A value of Grp1Fips.
      /// </summary>
      [JsonPropertyName("grp1Fips")]
      public Fips Grp1Fips
      {
        get => grp1Fips ??= new();
        set => grp1Fips = value;
      }

      /// <summary>
      /// A value of Grp1FipsTribAddress.
      /// </summary>
      [JsonPropertyName("grp1FipsTribAddress")]
      public FipsTribAddress Grp1FipsTribAddress
      {
        get => grp1FipsTribAddress ??= new();
        set => grp1FipsTribAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction grp1LegalAction;
      private LegalActionPerson grp1LegalActionPerson;
      private Fips grp1Fips;
      private FipsTribAddress grp1FipsTribAddress;
    }

    /// <summary>A LegalActionPerson2Group group.</summary>
    [Serializable]
    public class LegalActionPerson2Group
    {
      /// <summary>
      /// A value of Grp2LegalAction.
      /// </summary>
      [JsonPropertyName("grp2LegalAction")]
      public LegalAction Grp2LegalAction
      {
        get => grp2LegalAction ??= new();
        set => grp2LegalAction = value;
      }

      /// <summary>
      /// A value of Grp2LegalActionPerson.
      /// </summary>
      [JsonPropertyName("grp2LegalActionPerson")]
      public LegalActionPerson Grp2LegalActionPerson
      {
        get => grp2LegalActionPerson ??= new();
        set => grp2LegalActionPerson = value;
      }

      /// <summary>
      /// A value of Grp2Fips.
      /// </summary>
      [JsonPropertyName("grp2Fips")]
      public Fips Grp2Fips
      {
        get => grp2Fips ??= new();
        set => grp2Fips = value;
      }

      /// <summary>
      /// A value of Grp2FipsTribAddress.
      /// </summary>
      [JsonPropertyName("grp2FipsTribAddress")]
      public FipsTribAddress Grp2FipsTribAddress
      {
        get => grp2FipsTribAddress ??= new();
        set => grp2FipsTribAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction grp2LegalAction;
      private LegalActionPerson grp2LegalActionPerson;
      private Fips grp2Fips;
      private FipsTribAddress grp2FipsTribAddress;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of EmailVerify.
    /// </summary>
    [JsonPropertyName("emailVerify")]
    public Common EmailVerify
    {
      get => emailVerify ??= new();
      set => emailVerify = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of DismissedLegalAction.
    /// </summary>
    [JsonPropertyName("dismissedLegalAction")]
    public Common DismissedLegalAction
    {
      get => dismissedLegalAction ??= new();
      set => dismissedLegalAction = value;
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
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    /// <summary>
    /// A value of InitZeroSetToMax.
    /// </summary>
    [JsonPropertyName("initZeroSetToMax")]
    public DateWorkArea InitZeroSetToMax
    {
      get => initZeroSetToMax ??= new();
      set => initZeroSetToMax = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of AttorneyAddressFound.
    /// </summary>
    [JsonPropertyName("attorneyAddressFound")]
    public Common AttorneyAddressFound
    {
      get => attorneyAddressFound ??= new();
      set => attorneyAddressFound = value;
    }

    /// <summary>
    /// Gets a value of LegalActionPerson1.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionPerson1Group> LegalActionPerson1 =>
      legalActionPerson1 ??= new(LegalActionPerson1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActionPerson1 for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionPerson1")]
    [Computed]
    public IList<LegalActionPerson1Group> LegalActionPerson1_Json
    {
      get => legalActionPerson1;
      set => LegalActionPerson1.Assign(value);
    }

    /// <summary>
    /// A value of PrevCourtCaseDismissed.
    /// </summary>
    [JsonPropertyName("prevCourtCaseDismissed")]
    public Common PrevCourtCaseDismissed
    {
      get => prevCourtCaseDismissed ??= new();
      set => prevCourtCaseDismissed = value;
    }

    /// <summary>
    /// Gets a value of LegalActionPerson2.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionPerson2Group> LegalActionPerson2 =>
      legalActionPerson2 ??= new(LegalActionPerson2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActionPerson2 for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionPerson2")]
    [Computed]
    public IList<LegalActionPerson2Group> LegalActionPerson2_Json
    {
      get => legalActionPerson2;
      set => LegalActionPerson2.Assign(value);
    }

    /// <summary>
    /// A value of PrevLegalAction.
    /// </summary>
    [JsonPropertyName("prevLegalAction")]
    public LegalAction PrevLegalAction
    {
      get => prevLegalAction ??= new();
      set => prevLegalAction = value;
    }

    /// <summary>
    /// A value of PrevLegalActionPerson.
    /// </summary>
    [JsonPropertyName("prevLegalActionPerson")]
    public LegalActionPerson PrevLegalActionPerson
    {
      get => prevLegalActionPerson ??= new();
      set => prevLegalActionPerson = value;
    }

    /// <summary>
    /// A value of PrevFips.
    /// </summary>
    [JsonPropertyName("prevFips")]
    public Fips PrevFips
    {
      get => prevFips ??= new();
      set => prevFips = value;
    }

    /// <summary>
    /// A value of PrevFipsTribAddress.
    /// </summary>
    [JsonPropertyName("prevFipsTribAddress")]
    public FipsTribAddress PrevFipsTribAddress
    {
      get => prevFipsTribAddress ??= new();
      set => prevFipsTribAddress = value;
    }

    /// <summary>
    /// A value of AttorneyRecFound.
    /// </summary>
    [JsonPropertyName("attorneyRecFound")]
    public Common AttorneyRecFound
    {
      get => attorneyRecFound ??= new();
      set => attorneyRecFound = value;
    }

    /// <summary>
    /// A value of OtherAttyRecordsFound.
    /// </summary>
    [JsonPropertyName("otherAttyRecordsFound")]
    public Common OtherAttyRecordsFound
    {
      get => otherAttyRecordsFound ??= new();
      set => otherAttyRecordsFound = value;
    }

    /// <summary>
    /// A value of LegalActionsFound.
    /// </summary>
    [JsonPropertyName("legalActionsFound")]
    public Common LegalActionsFound
    {
      get => legalActionsFound ??= new();
      set => legalActionsFound = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public PersonPrivateAttorney Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    private TextWorkArea postion;
    private Common emailVerify;
    private Common currentPosition;
    private DateWorkArea zero;
    private Common legalActionFound;
    private Common dismissedLegalAction;
    private DateWorkArea current;
    private Common checkZip;
    private DateWorkArea initZeroSetToMax;
    private DateWorkArea dateWorkArea;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common attorneyAddressFound;
    private Array<LegalActionPerson1Group> legalActionPerson1;
    private Common prevCourtCaseDismissed;
    private Array<LegalActionPerson2Group> legalActionPerson2;
    private LegalAction prevLegalAction;
    private LegalActionPerson prevLegalActionPerson;
    private Fips prevFips;
    private FipsTribAddress prevFipsTribAddress;
    private Common attorneyRecFound;
    private Common otherAttyRecordsFound;
    private Common legalActionsFound;
    private PersonPrivateAttorney init1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("existingPrivateAttorneyAddress")]
    public PrivateAttorneyAddress ExistingPrivateAttorneyAddress
    {
      get => existingPrivateAttorneyAddress ??= new();
      set => existingPrivateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of ExistingPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("existingPersonPrivateAttorney")]
    public PersonPrivateAttorney ExistingPersonPrivateAttorney
    {
      get => existingPersonPrivateAttorney ??= new();
      set => existingPersonPrivateAttorney = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private Tribunal existingTribunal;
    private CaseRole existingCaseRole;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private LegalActionPerson existingLegalActionPerson;
    private LegalAction existingLegalAction;
    private FipsTribAddress existingFipsTribAddress;
    private Fips existingFips;
    private PrivateAttorneyAddress existingPrivateAttorneyAddress;
    private PersonPrivateAttorney existingPersonPrivateAttorney;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
  }
#endregion
}
