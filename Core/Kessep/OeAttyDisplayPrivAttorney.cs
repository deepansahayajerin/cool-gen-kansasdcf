// Program: OE_ATTY_DISPLAY_PRIV_ATTORNEY, ID: 372179497, model: 746.
// Short name: SWE00858
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
/// A program: OE_ATTY_DISPLAY_PRIV_ATTORNEY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action reads and populates the person private attorney and private 
/// attorney address views for display.
/// </para>
/// </summary>
[Serializable]
public partial class OeAttyDisplayPrivAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_DISPLAY_PRIV_ATTORNEY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyDisplayPrivAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyDisplayPrivAttorney.
  /// </summary>
  public OeAttyDisplayPrivAttorney(IContext context, Import import,
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
    // This action block populates views of Person Private Attorney and Private 
    // Attorney Address to display Attorney details.
    // PROCESSING:
    // If the user pressed DISPLAY key, it reads the given/next PERSON PRIVATE 
    // ATTORNEY record. (i.e. If user typed IDENTIFIER as 0, it will read the
    // next available attorney record. If the user typed IDENTIFIER as 3, then
    // it will read the record for IDENTIFIER = 3).
    // If the user pressed NEXT key, it will read the next attorney record.
    // If the user pressed PREV key, it will read the previous attorney record.
    // ACTION BLOCKS:
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
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	02/22/95			Initial coding
    // Madhu Kumar      04/16/2001                 PR117009 .
    // Dispay after adding or updating the record not working.
    // JHarden  3/17/17  CQ53818      add email address to ATTY screen
    // 	
    // JHarden     5/26/2017  CQ57453 Add fields consent, note, and bar #.
    // ************************************************
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    MovePersonPrivateAttorney(import.PersonPrivateAttorney,
      export.PersonPrivateAttorney);
    export.AttyDisplayed.Flag = "N";

    if (IsEmpty(import.CsePerson.Number))
    {
      ExitState = "OE0026_INVALID_CSE_PERSON_NO";

      return;
    }

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
      UseCabGetClientDetails();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Command DISPLAY displays given Attorney rec.
    // Command PREV displays previous Attorney rec.
    // Command NEXT displays next Attorney rec.
    // ---------------------------------------------
    switch(TrimEnd(import.UserAction.Command))
    {
      case "ADDTASK":
        // ****************************************************************
        // * Read for Case.  If Case number entered, read for specific
        // * Case; if Case number blank, read each for latest and greatest.
        // ****************************************************************
        export.PersonPrivateAttorney.Assign(local.InitPersonPrivateAttorney);

        if (!IsEmpty(import.Case1.Number))
        {
          if (ReadCase3())
          {
            if (AsChar(entities.ExistingCase.Status) == 'O')
            {
              ++local.CasesFound.Count;
              export.Case1.Number = entities.ExistingCase.Number;
            }
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }
        }
        else
        {
          foreach(var item in ReadCase5())
          {
            if (AsChar(entities.ExistingCase.Status) == 'O')
            {
              ++local.CasesFound.Count;
              export.Case1.Number = entities.ExistingCase.Number;
            }
          }
        }

        // ****************************************************************
        // * Test for read results; if 0 found, error can't add ATTY. If >1
        // * found, must enter Case number. If = 1 found, read for ATTY related.
        // ****************************************************************
        switch(local.CasesFound.Count)
        {
          case 0:
            ExitState = "OE0000_CSE_PERSN_NOT_REL_TO_CASE";

            return;
          case 1:
            // ****	Re-read Case to establish currency for prior read each
            if (IsEmpty(import.Case1.Number))
            {
              if (ReadCase2())
              {
                export.Case1.Number = entities.ExistingCase.Number;
              }
              else
              {
                ExitState = "CASE_NF";

                return;
              }
            }

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
            // * Compare current to prior entries on result group view.  If a 
            // break
            // * in key (CC#, County, State, Country) then move the prior
            // * entry to group view 2.  Group view 2 will contain only the 
            // unique
            // * entries.  Bypass any court case (all legal actions) that has 
            // been dismissed.
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
                  MoveFips(local.PrevFips,
                    local.LegalActionPerson2.Update.Grp2Fips);
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

            if (AsChar(local.PrevCourtCaseDismissed.Flag) == 'N')
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

              foreach(var item in ReadPersonPrivateAttorney3())
              {
                if (Lt(entities.ExistingPersonPrivateAttorney.DateDismissed,
                  Now().Date) && !
                  Equal(entities.ExistingPersonPrivateAttorney.DateDismissed,
                  local.Zero.Date))
                {
                  continue;
                }

                if (Equal(entities.ExistingPersonPrivateAttorney.
                  CourtCaseNumber,
                  local.LegalActionPerson2.Item.Grp2LegalAction.
                    CourtCaseNumber) && Equal
                  (entities.ExistingPersonPrivateAttorney.
                    FipsCountyAbbreviation,
                  local.LegalActionPerson2.Item.Grp2Fips.CountyAbbreviation) &&
                  Equal
                  (entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation,
                  local.LegalActionPerson2.Item.Grp2Fips.StateAbbreviation) && Equal
                  (entities.ExistingPersonPrivateAttorney.TribCountry,
                  local.LegalActionPerson2.Item.Grp2FipsTribAddress.Country))
                {
                  local.AttorneyRecFound.Flag = "Y";

                  break;
                }
              }

              if (AsChar(local.AttorneyRecFound.Flag) == 'Y')
              {
                local.OtherAttyRecordsFound.Flag = "Y";
                local.AttorneyRecFound.Flag = "N";
              }
              else
              {
                ++local.LegalActionsFound.Count;

                if (local.LegalActionsFound.Count > 1)
                {
                  export.PersonPrivateAttorney.Assign(
                    local.InitPersonPrivateAttorney);
                  ExitState = "OE0000_MULT_LEGAL_ACTIONS_EXIST";

                  return;
                }

                export.PersonPrivateAttorney.FipsCountyAbbreviation =
                  local.LegalActionPerson2.Item.Grp2Fips.CountyAbbreviation ?? ""
                  ;
                export.PersonPrivateAttorney.FipsStateAbbreviation =
                  local.LegalActionPerson2.Item.Grp2Fips.StateAbbreviation;
                export.PersonPrivateAttorney.CourtCaseNumber =
                  local.LegalActionPerson2.Item.Grp2LegalAction.
                    CourtCaseNumber ?? "";
                export.PersonPrivateAttorney.TribCountry =
                  local.LegalActionPerson2.Item.Grp2FipsTribAddress.Country ?? ""
                  ;
              }
            }

            local.LegalActionPerson2.CheckIndex();

            if (AsChar(local.OtherAttyRecordsFound.Flag) == 'Y')
            {
              ExitState = "OE0000_OTHR_ATTY_FOUND";
            }

            break;
          default:
            export.Case1.Number = local.InitCase.Number;
            ExitState = "OE0000_CSE_PERSON_REL_MULT_CASES";

            return;
        }

        // ****	For this command don't do any further processing in the CAB
        return;
      case "DISPLAY":
        if (import.PersonPrivateAttorney.Identifier > 0)
        {
          foreach(var item in ReadPersonPrivateAttorney5())
          {
            // *******************************************************************
            // Madhu Kumar      04/16/2001                 PR117009 .
            // Dispay after adding or updating the record not working.
            // Moved the read for case to the beginning .
            // *******************************************************************
            if (!IsEmpty(import.Case1.Number))
            {
              if (ReadCase1())
              {
                export.Case1.Number = entities.ExistingCase.Number;
              }
              else
              {
                ExitState = "CASE_NF";

                return;
              }
            }
            else if (ReadCase4())
            {
              export.Case1.Number = entities.ExistingCase.Number;
            }
            else
            {
              ExitState = "CASE_NF";

              return;
            }

            if (!IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber) || !
              IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
              IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation) || !
              IsEmpty(import.PersonPrivateAttorney.TribCountry))
            {
              if (!Equal(entities.ExistingPersonPrivateAttorney.CourtCaseNumber,
                import.PersonPrivateAttorney.CourtCaseNumber) || !
                Equal(entities.ExistingPersonPrivateAttorney.
                  FipsCountyAbbreviation,
                import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
                Equal(import.PersonPrivateAttorney.FipsStateAbbreviation,
                import.PersonPrivateAttorney.FipsStateAbbreviation) || !
                Equal(entities.ExistingPersonPrivateAttorney.TribCountry,
                import.PersonPrivateAttorney.TribCountry))
              {
                ExitState = "NO_PRIVATE_ATTORNEY_FOR_PERSON";

                return;
              }
            }

            if (!IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
              
            {
              foreach(var item1 in ReadLegalActionTribunal())
              {
                if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
                  entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
                {
                  continue;
                }

                if (ReadFips())
                {
                  if (!Equal(entities.ExistingFips.CountyAbbreviation,
                    entities.ExistingPersonPrivateAttorney.
                      FipsCountyAbbreviation) || !
                    Equal(entities.ExistingFips.StateAbbreviation,
                    entities.ExistingPersonPrivateAttorney.
                      FipsStateAbbreviation))
                  {
                    continue;
                  }
                }
                else if (ReadFipsTribAddress())
                {
                  if (!Equal(entities.ExistingFipsTribAddress.Country,
                    entities.ExistingPersonPrivateAttorney.TribCountry))
                  {
                    continue;
                  }
                }

                if (!IsEmpty(entities.ExistingLegalAction.
                  DismissedWithoutPrejudiceInd))
                {
                  export.CourtCaseDismissed.Flag = "Y";

                  break;
                }
              }
            }

            export.PersonPrivateAttorney.Assign(
              entities.ExistingPersonPrivateAttorney);
            local.AttorneyRecFound.Flag = "Y";

            if (ReadPrivateAttorneyAddress())
            {
              MovePrivateAttorneyAddress(entities.
                ExistingPrivateAttorneyAddress, export.PrivateAttorneyAddress);
            }

            if (!entities.ExistingPersonPrivateAttorney.Populated)
            {
              ExitState = "NO_PRIVATE_ATTORNEY_FOR_PERSON";

              return;
            }

            if (AsChar(local.AttorneyRecFound.Flag) == 'Y')
            {
              goto Test;
            }
          }
        }
        else
        {
          // ****************************************************************
          // * Read for Case.  If Case number entered, read for specific
          // * Case; if Case number blank, read each for latest and greatest.
          // ****************************************************************
          if (!IsEmpty(import.Case1.Number))
          {
            if (ReadCase3())
            {
              export.Case1.Number = entities.ExistingCase.Number;
              local.AttorneyRecFound.Flag = "N";

              // QQQ
              foreach(var item in ReadPersonPrivateAttorney4())
              {
                if (!IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber) || !
                  IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) ||
                  !
                  IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation) ||
                  !IsEmpty(import.PersonPrivateAttorney.TribCountry))
                {
                  if (!Equal(entities.ExistingPersonPrivateAttorney.
                    CourtCaseNumber,
                    import.PersonPrivateAttorney.CourtCaseNumber) || !
                    Equal(entities.ExistingPersonPrivateAttorney.
                      FipsCountyAbbreviation,
                    import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
                    Equal(import.PersonPrivateAttorney.FipsStateAbbreviation,
                    import.PersonPrivateAttorney.FipsStateAbbreviation) || !
                    Equal(entities.ExistingPersonPrivateAttorney.TribCountry,
                    import.PersonPrivateAttorney.TribCountry))
                  {
                    continue;
                  }
                }

                if (!IsEmpty(entities.ExistingPersonPrivateAttorney.
                  CourtCaseNumber))
                {
                  foreach(var item1 in ReadLegalActionTribunal())
                  {
                    if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
                      entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
                    {
                      continue;
                    }

                    if (ReadFips())
                    {
                      if (!Equal(entities.ExistingFips.CountyAbbreviation,
                        entities.ExistingPersonPrivateAttorney.
                          FipsCountyAbbreviation) || !
                        Equal(entities.ExistingFips.StateAbbreviation,
                        entities.ExistingPersonPrivateAttorney.
                          FipsStateAbbreviation))
                      {
                        continue;
                      }
                    }
                    else if (ReadFipsTribAddress())
                    {
                      if (!Equal(entities.ExistingFipsTribAddress.Country,
                        entities.ExistingPersonPrivateAttorney.TribCountry))
                      {
                        continue;
                      }
                    }

                    if (!IsEmpty(entities.ExistingLegalAction.
                      DismissedWithoutPrejudiceInd))
                    {
                      export.CourtCaseDismissed.Flag = "Y";

                      break;
                    }
                  }
                }

                export.PersonPrivateAttorney.Assign(
                  entities.ExistingPersonPrivateAttorney);
                local.AttorneyRecFound.Flag = "Y";

                break;
              }

              if (AsChar(local.AttorneyRecFound.Flag) == 'N')
              {
                ExitState = "NO_PRIVATE_ATTORNEY_FOR_PERSON";

                return;
              }
              else if (ReadPrivateAttorneyAddress())
              {
                MovePrivateAttorneyAddress(entities.
                  ExistingPrivateAttorneyAddress,
                  export.PrivateAttorneyAddress);
              }
            }
            else
            {
              ExitState = "CASE_NF";

              return;
            }
          }
          else
          {
            local.AttorneyRecFound.Flag = "N";

            foreach(var item in ReadCasePersonPrivateAttorney3())
            {
              if (!IsEmpty(import.PersonPrivateAttorney.CourtCaseNumber) || !
                IsEmpty(import.PersonPrivateAttorney.FipsCountyAbbreviation) ||
                !
                IsEmpty(import.PersonPrivateAttorney.FipsStateAbbreviation) || !
                IsEmpty(import.PersonPrivateAttorney.TribCountry))
              {
                if (!Equal(entities.ExistingPersonPrivateAttorney.
                  CourtCaseNumber,
                  import.PersonPrivateAttorney.CourtCaseNumber) || !
                  Equal(entities.ExistingPersonPrivateAttorney.
                    FipsCountyAbbreviation,
                  import.PersonPrivateAttorney.FipsCountyAbbreviation) || !
                  Equal(import.PersonPrivateAttorney.FipsStateAbbreviation,
                  import.PersonPrivateAttorney.FipsStateAbbreviation) || !
                  Equal(entities.ExistingPersonPrivateAttorney.TribCountry,
                  import.PersonPrivateAttorney.TribCountry))
                {
                  continue;
                }
              }

              if (!IsEmpty(entities.ExistingPersonPrivateAttorney.
                CourtCaseNumber))
              {
                foreach(var item1 in ReadLegalActionTribunal())
                {
                  if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
                    entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
                  {
                    continue;
                  }

                  if (ReadFips())
                  {
                    if (!Equal(entities.ExistingFips.CountyAbbreviation,
                      entities.ExistingPersonPrivateAttorney.
                        FipsCountyAbbreviation) || !
                      Equal(entities.ExistingFips.StateAbbreviation,
                      entities.ExistingPersonPrivateAttorney.
                        FipsStateAbbreviation))
                    {
                      continue;
                    }
                  }
                  else if (ReadFipsTribAddress())
                  {
                    if (!Equal(entities.ExistingFipsTribAddress.Country,
                      entities.ExistingPersonPrivateAttorney.TribCountry))
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(entities.ExistingLegalAction.
                    DismissedWithoutPrejudiceInd))
                  {
                    export.CourtCaseDismissed.Flag = "Y";

                    break;
                  }
                }
              }

              export.Case1.Number = entities.ExistingCase.Number;
              export.PersonPrivateAttorney.Assign(
                entities.ExistingPersonPrivateAttorney);
              local.AttorneyRecFound.Flag = "Y";

              break;
            }

            if (AsChar(local.AttorneyRecFound.Flag) == 'N')
            {
              ExitState = "NO_PRIVATE_ATTORNEY_FOR_PERSON";

              return;
            }
            else if (ReadPrivateAttorneyAddress())
            {
              MovePrivateAttorneyAddress(entities.
                ExistingPrivateAttorneyAddress, export.PrivateAttorneyAddress);
            }
          }
        }

        // QQQ
        break;
      case "NEXT":
        local.AttorneyRecFound.Flag = "N";

        if (ReadCasePersonPrivateAttorney1())
        {
          if (!IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
          {
            foreach(var item in ReadLegalActionTribunal())
            {
              if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
                entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
              {
                continue;
              }

              if (ReadFips())
              {
                if (!Equal(entities.ExistingFips.CountyAbbreviation,
                  entities.ExistingPersonPrivateAttorney.
                    FipsCountyAbbreviation) || !
                  Equal(entities.ExistingFips.StateAbbreviation,
                  entities.ExistingPersonPrivateAttorney.
                    FipsStateAbbreviation))
                {
                  continue;
                }
              }
              else if (ReadFipsTribAddress())
              {
                if (!Equal(entities.ExistingFipsTribAddress.Country,
                  entities.ExistingPersonPrivateAttorney.TribCountry))
                {
                  continue;
                }
              }

              if (!IsEmpty(entities.ExistingLegalAction.
                DismissedWithoutPrejudiceInd))
              {
                export.CourtCaseDismissed.Flag = "Y";

                break;
              }
            }
          }

          export.Case1.Number = entities.ExistingCase.Number;
          export.PersonPrivateAttorney.Assign(
            entities.ExistingPersonPrivateAttorney);
          local.AttorneyRecFound.Flag = "Y";
        }

        if (AsChar(local.AttorneyRecFound.Flag) == 'N')
        {
          ExitState = "OE0079_NO_MORE_ATTR_TO_DISPLAY";
        }
        else if (ReadPrivateAttorneyAddress())
        {
          MovePrivateAttorneyAddress(entities.ExistingPrivateAttorneyAddress,
            export.PrivateAttorneyAddress);
        }

        // QQQ
        break;
      case "PREV":
        local.AttorneyRecFound.Flag = "N";

        if (ReadCasePersonPrivateAttorney2())
        {
          if (!IsEmpty(entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
          {
            foreach(var item in ReadLegalActionTribunal())
            {
              if (!Equal(entities.ExistingLegalAction.CourtCaseNumber,
                entities.ExistingPersonPrivateAttorney.CourtCaseNumber))
              {
                continue;
              }

              if (ReadFips())
              {
                if (!Equal(entities.ExistingFips.CountyAbbreviation,
                  entities.ExistingPersonPrivateAttorney.
                    FipsCountyAbbreviation) || !
                  Equal(entities.ExistingFips.StateAbbreviation,
                  entities.ExistingPersonPrivateAttorney.
                    FipsStateAbbreviation))
                {
                  continue;
                }
              }
              else if (ReadFipsTribAddress())
              {
                if (!Equal(entities.ExistingFipsTribAddress.Country,
                  entities.ExistingPersonPrivateAttorney.TribCountry))
                {
                  continue;
                }
              }

              if (!IsEmpty(entities.ExistingLegalAction.
                DismissedWithoutPrejudiceInd))
              {
                export.CourtCaseDismissed.Flag = "Y";

                break;
              }
            }
          }

          export.Case1.Number = entities.ExistingCase.Number;
          export.PersonPrivateAttorney.Assign(
            entities.ExistingPersonPrivateAttorney);
          local.AttorneyRecFound.Flag = "Y";
        }

        if (AsChar(local.AttorneyRecFound.Flag) == 'N')
        {
          ExitState = "OE0079_NO_MORE_ATTR_TO_DISPLAY";
        }
        else if (ReadPrivateAttorneyAddress())
        {
          MovePrivateAttorneyAddress(entities.ExistingPrivateAttorneyAddress,
            export.PrivateAttorneyAddress);
        }

        break;
      default:
        break;
    }

Test:

    if (AsChar(local.AttorneyRecFound.Flag) == 'Y')
    {
      export.AttyDisplayed.Flag = "Y";

      // ---------------------------------------------
      // Set the user id and timestamp of last update.
      // ---------------------------------------------
      export.UpdateStamp.LastUpdatedBy = export.PersonPrivateAttorney.CreatedBy;
      export.UpdateStamp.LastUpdatedTimestamp =
        export.PersonPrivateAttorney.CreatedTimestamp;

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        export.PersonPrivateAttorney.LastUpdatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedBy =
          export.PersonPrivateAttorney.LastUpdatedBy;
        export.UpdateStamp.LastUpdatedTimestamp =
          export.PersonPrivateAttorney.LastUpdatedTimestamp;
      }

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        export.PrivateAttorneyAddress.CreatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedTimestamp =
          export.PrivateAttorneyAddress.CreatedTimestamp;
        export.UpdateStamp.LastUpdatedBy =
          export.PrivateAttorneyAddress.CreatedBy;
      }

      if (Lt(export.UpdateStamp.LastUpdatedTimestamp,
        export.PrivateAttorneyAddress.LastUpdatedTimestamp))
      {
        export.UpdateStamp.LastUpdatedTimestamp =
          export.PrivateAttorneyAddress.LastUpdatedTimestamp;
        export.UpdateStamp.LastUpdatedBy =
          export.PrivateAttorneyAddress.LastUpdatedBy;
      }
    }

    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (ReadPersonPrivateAttorney2())
    {
      export.ScrollingAttributes.MinusFlag = "-";
    }

    if (ReadPersonPrivateAttorney1())
    {
      export.ScrollingAttributes.PlusFlag = "+";
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MovePersonPrivateAttorney(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.FipsStateAbbreviation = source.FipsStateAbbreviation;
    target.FipsCountyAbbreviation = source.FipsCountyAbbreviation;
    target.TribCountry = source.TribCountry;
  }

  private static void MovePrivateAttorneyAddress(PrivateAttorneyAddress source,
    PrivateAttorneyAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);
    entities.ExistingCase.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(
          command, "numb1",
          entities.ExistingPersonPrivateAttorney.CasNumber ?? "");
        db.SetString(command, "numb2", import.Case1.Number);
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

  private bool ReadCase2()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
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

  private bool ReadCase3()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase3",
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

  private bool ReadCase4()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);
    entities.ExistingCase.Populated = false;

    return Read("ReadCase4",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ExistingPersonPrivateAttorney.CasNumber ?? ""
          );
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

  private IEnumerable<bool> ReadCase5()
  {
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCase5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private bool ReadCasePersonPrivateAttorney1()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;
    entities.ExistingCase.Populated = false;

    return Read("ReadCasePersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 5);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 6);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 7);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 11);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 12);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 20);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 27);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 28);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 29);
        entities.ExistingPersonPrivateAttorney.Populated = true;
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCasePersonPrivateAttorney2()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;
    entities.ExistingCase.Populated = false;

    return Read("ReadCasePersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 5);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 6);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 7);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 11);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 12);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 20);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 27);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 28);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 29);
        entities.ExistingPersonPrivateAttorney.Populated = true;
        entities.ExistingCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCasePersonPrivateAttorney3()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCasePersonPrivateAttorney3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.StatusDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 5);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 6);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 7);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 11);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 12);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPersonPrivateAttorney.FaxNumberAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingPersonPrivateAttorney.FaxExt =
          db.GetNullableString(reader, 19);
        entities.ExistingPersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 20);
        entities.ExistingPersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingPersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 22);
        entities.ExistingPersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 23);
        entities.ExistingPersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 24);
        entities.ExistingPersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 25);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 26);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 27);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 28);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 29);
        entities.ExistingPersonPrivateAttorney.Populated = true;
        entities.ExistingCase.Populated = true;

        return true;
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

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
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

  private IEnumerable<bool> ReadLegalActionLegalActionPersonTribunalFips()
  {
    entities.ExistingFips.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

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
        entities.ExistingFips.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionLegalActionPersonTribunalFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

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
        entities.ExistingFipsTribAddress.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
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
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 5);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 8);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPrivateAttorney1()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", export.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.Identifier = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevOrNext.FipsStateAbbreviation =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevOrNext.FipsCountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingPrevOrNext.TribCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney2()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", export.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.Identifier = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevOrNext.FipsStateAbbreviation =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevOrNext.FipsCountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingPrevOrNext.TribCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney3()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney3",
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

  private IEnumerable<bool> ReadPersonPrivateAttorney5()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
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
        entities.ExistingPrivateAttorneyAddress.CreatedBy =
          db.GetString(reader, 13);
        entities.ExistingPrivateAttorneyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
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
    /// <summary>
    /// A value of AttyDisplayed.
    /// </summary>
    [JsonPropertyName("attyDisplayed")]
    public Common AttyDisplayed
    {
      get => attyDisplayed ??= new();
      set => attyDisplayed = value;
    }

    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public PersonPrivateAttorney UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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

    /// <summary>
    /// A value of CourtCaseDismissed.
    /// </summary>
    [JsonPropertyName("courtCaseDismissed")]
    public Common CourtCaseDismissed
    {
      get => courtCaseDismissed ??= new();
      set => courtCaseDismissed = value;
    }

    private Common attyDisplayed;
    private PersonPrivateAttorney updateStamp;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PersonPrivateAttorney personPrivateAttorney;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private CsePerson csePerson;
    private Case1 case1;
    private Common courtCaseDismissed;
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
    /// A value of InitPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("initPersonPrivateAttorney")]
    public PersonPrivateAttorney InitPersonPrivateAttorney
    {
      get => initPersonPrivateAttorney ??= new();
      set => initPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of InitCase.
    /// </summary>
    [JsonPropertyName("initCase")]
    public Case1 InitCase
    {
      get => initCase ??= new();
      set => initCase = value;
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
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of SelectedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("selectedLegalActionPerson")]
    public LegalActionPerson SelectedLegalActionPerson
    {
      get => selectedLegalActionPerson ??= new();
      set => selectedLegalActionPerson = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedFipsTribAddress.
    /// </summary>
    [JsonPropertyName("selectedFipsTribAddress")]
    public FipsTribAddress SelectedFipsTribAddress
    {
      get => selectedFipsTribAddress ??= new();
      set => selectedFipsTribAddress = value;
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
    /// A value of CasesFound.
    /// </summary>
    [JsonPropertyName("casesFound")]
    public Common CasesFound
    {
      get => casesFound ??= new();
      set => casesFound = value;
    }

    /// <summary>
    /// A value of LegalActionsObligor.
    /// </summary>
    [JsonPropertyName("legalActionsObligor")]
    public Common LegalActionsObligor
    {
      get => legalActionsObligor ??= new();
      set => legalActionsObligor = value;
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
    /// A value of AttorneyRecFound.
    /// </summary>
    [JsonPropertyName("attorneyRecFound")]
    public Common AttorneyRecFound
    {
      get => attorneyRecFound ??= new();
      set => attorneyRecFound = value;
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
    /// A value of PrevFipsTribAddress.
    /// </summary>
    [JsonPropertyName("prevFipsTribAddress")]
    public FipsTribAddress PrevFipsTribAddress
    {
      get => prevFipsTribAddress ??= new();
      set => prevFipsTribAddress = value;
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
    /// A value of OtherAttyRecordsFound.
    /// </summary>
    [JsonPropertyName("otherAttyRecordsFound")]
    public Common OtherAttyRecordsFound
    {
      get => otherAttyRecordsFound ??= new();
      set => otherAttyRecordsFound = value;
    }

    private PersonPrivateAttorney initPersonPrivateAttorney;
    private Case1 initCase;
    private DateWorkArea zero;
    private LegalAction selectedLegalAction;
    private LegalActionPerson selectedLegalActionPerson;
    private Fips selectedFips;
    private FipsTribAddress selectedFipsTribAddress;
    private Array<LegalActionPerson1Group> legalActionPerson1;
    private Array<LegalActionPerson2Group> legalActionPerson2;
    private Common casesFound;
    private Common legalActionsObligor;
    private Common legalActionsFound;
    private Common attorneyRecFound;
    private Common prevCourtCaseDismissed;
    private LegalAction prevLegalAction;
    private LegalActionPerson prevLegalActionPerson;
    private FipsTribAddress prevFipsTribAddress;
    private Fips prevFips;
    private Common otherAttyRecordsFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
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
    /// A value of ExistingLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("existingLaPersonLaCaseRole")]
    public LaPersonLaCaseRole ExistingLaPersonLaCaseRole
    {
      get => existingLaPersonLaCaseRole ??= new();
      set => existingLaPersonLaCaseRole = value;
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
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingPrevOrNext.
    /// </summary>
    [JsonPropertyName("existingPrevOrNext")]
    public PersonPrivateAttorney ExistingPrevOrNext
    {
      get => existingPrevOrNext ??= new();
      set => existingPrevOrNext = value;
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

    /// <summary>
    /// A value of ExistingPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("existingPrivateAttorneyAddress")]
    public PrivateAttorneyAddress ExistingPrivateAttorneyAddress
    {
      get => existingPrivateAttorneyAddress ??= new();
      set => existingPrivateAttorneyAddress = value;
    }

    private FipsTribAddress existingFipsTribAddress;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private LaPersonLaCaseRole existingLaPersonLaCaseRole;
    private LegalActionPerson existingLegalActionPerson;
    private LegalAction existingLegalAction;
    private CaseRole existingCaseRole;
    private PersonPrivateAttorney existingPrevOrNext;
    private PersonPrivateAttorney existingPersonPrivateAttorney;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private PrivateAttorneyAddress existingPrivateAttorneyAddress;
  }
#endregion
}
