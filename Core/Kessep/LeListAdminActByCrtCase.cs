// Program: LE_LIST_ADMIN_ACT_BY_CRT_CASE, ID: 372578861, model: 746.
// Short name: SWE00794
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
/// A program: LE_LIST_ADMIN_ACT_BY_CRT_CASE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and returns all administrative actions for a given 
/// obligor.
/// There is another version of this acblk exists with name 
/// LE_LIST_V2_ADMIN_ACT_BY_CRT_CASE
/// This action block will return only distinct administrative actions. i.e if 
/// an admin action is tied to more than one obligation, it will be listed only
/// once.
/// The version 2, however, will list the same admin action several times once 
/// for each obligation.
/// </para>
/// </summary>
[Serializable]
public partial class LeListAdminActByCrtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_ADMIN_ACT_BY_CRT_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListAdminActByCrtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListAdminActByCrtCase.
  /// </summary>
  public LeListAdminActByCrtCase(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    // MAINTENANCE LOG:
    // 04/28/97  JF. Caillouet		Change Current Date
    // 09/15/98  P. Sharp   		Added capability for list both auto and manual at 
    // same time.
    // 08/25/99  RJEAN        PR612-13	Change read of Admin Act Cert by Current 
    // Amt Date.
    // 01/05/00  RJEAN        PR84255	Bypass any FDSO that have date sent equal 
    // zero-date.
    // 				Populate FDSO current amount date with date sent so it displays.
    // 12/13/00  GVandy       PR109209	Correct a blank line in the repeating 
    // group when both
    // 				Automatic and Manual actions are displayed.
    // --------------------------------------------------------
    // ---------------------------------------------
    // There is another version of this action block with the name 
    // LE_LIST_V2_ADMIN_ACT_BY_CRT_CASE.
    // This version (LE_LIST_ADMIN_ACT_BY_CRT_CASE) will not list admin actions 
    // multiple times if the admin action is tied to more than one obligation
    // and if the obligor is the same. For e.g.
    //       Admin Act  Date  Obligation     Obligor  Disp
    // 1.        A1       D1     ON1            OR1    Y
    // 2.        A1       D1     ON2            OR1    N
    // 3.        A1       D1     ON3            OR1    N
    // 4.        A1       D1     ON4            OR2    Y
    // 5.        A1       D1     ON5            OR2    N
    // 6.        A1       D2     ON1            OR1    Y
    // The version 2 will list the same admin action several times, once for 
    // every obligation tied to it.
    // Currently this version is used instead of version 2 in procedure 
    // LE_AACC_LIST_ADM_ACT_TAKN_BY_CRT_CASE
    // ---------------------------------------------
    // ---------------------------------------------
    // First check if it is a valid court case number
    // ---------------------------------------------
    // *********************************************
    // Add qualifications for tribunal, fips state and county after data model 
    // change
    // *********************************************
    local.Current.Date = Now().Date;
    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;
    MoveFips(import.Fips, export.Fips);
    export.Tribunal.Identifier = import.Tribunal.Identifier;
    local.Auto.Flag = "N";
    local.Manual.Flag = "N";
    export.Common.Count = 0;

    if (IsEmpty(import.Fips.StateAbbreviation))
    {
      if (import.Tribunal.Identifier == 0)
      {
        ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

        return;
      }
      else if (!ReadLegalAction())
      {
        ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

        return;
      }
    }
    else if (ReadLegalActionFipsTribunal())
    {
      MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);
      export.Fips.Assign(entities.ExistingFips);
      export.Tribunal.Assign(entities.ExistingTribunal);
    }
    else
    {
      ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

      return;
    }

    switch(AsChar(import.ListOptManualSystAct.OneChar))
    {
      case 'M':
        break;
      case 'A':
        break;
      case ' ':
        break;
      default:
        ExitState = "LE0000_INVALID_OPT_MANUAL_AUTO";

        return;
    }

    if (!IsEmpty(import.Required.Type1))
    {
      if (ReadAdministrativeAction())
      {
        if (!IsEmpty(import.ListOptManualSystAct.OneChar))
        {
          if (AsChar(entities.ExistingAdministrativeAction.Indicator) != AsChar
            (import.ListOptManualSystAct.OneChar))
          {
            ExitState = "LE0000_LIST_NOT_MATCH_TYPE";

            return;
          }
        }
      }
      else
      {
        ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

        return;
      }
    }

    // ********  9/14/98 - The user now has 3 ways to see the group view. "A" 
    // for automatic admin action, "M" for manual  and if spaces will get both.
    // There are two separate reads for the A and M types. If the selection is
    // spaces both reads are performed and then are sorted and combined to
    // create the group view to be displayed.
    if (AsChar(import.ListOptManualSystAct.OneChar) == 'A' || IsEmpty
      (import.ListOptManualSystAct.OneChar))
    {
      // ---------------------------------------------
      // List only system generated administrative action types namely FDSO, 
      // SDSO, COAG, CRED.
      // i.e. those in the entity ADMINISTRATIVE ACT CERTIFICATION.
      // ---------------------------------------------
      local.SdsoAlreadyMoved.Flag = "N";
      local.FdsoAlreadyMoved.Flag = "N";
      local.CredAlreadyMoved.Flag = "N";
      export.AdminActions.Index = -1;
      local.AutoActions.Index = -1;

      // *********************************************
      // Add qualification for tribunal, fips state and county after data model 
      // change
      // *********************************************
      foreach(var item in ReadAdministrativeActCertificationAdministrativeAction())
        
      {
        // ****************************************************************
        // *010500	RJEAN	PR84255	Bypass any FDSO that have date sent
        // * equal zero-date.
        // ****************************************************************
        if (Equal(entities.ExistingAdministrativeActCertification.Type1, "FDSO"))
          
        {
          if (Equal(entities.ExistingAdministrativeActCertification.DateSent,
            local.InitialisedToZeros.Date))
          {
            continue;
          }

          if (!ReadFederalDebtSetoff())
          {
            ExitState = "FEDERAL_DEBT_SETOFF_NF";

            return;
          }
        }

        if (!ReadTribunal())
        {
          ExitState = "INVALID_TRIBUNAL";

          return;
        }

        if (IsEmpty(import.Fips.StateAbbreviation))
        {
          if (entities.ExistingTribunal.Identifier != import
            .Tribunal.Identifier)
          {
            continue;
          }
        }
        else if (ReadFips())
        {
          if (!Equal(entities.ExistingFips.StateAbbreviation,
            import.Fips.StateAbbreviation) || !
            Equal(entities.ExistingFips.CountyAbbreviation,
            import.Fips.CountyAbbreviation))
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        if (export.AdminActions.Index == -1 && local.AutoActions.Index == -1)
        {
        }
        else if (IsEmpty(import.ListOptManualSystAct.OneChar))
        {
          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.AutoActions.Item.AutoAdministrativeAction.Type1) && Equal
            (entities.ExistingAdministrativeActCertification.TakenDate,
            local.AutoActions.Item.AutoAdministrativeActCertification.
              TakenDate) && Equal
            (entities.ExistingObligorCsePerson.Number,
            local.AutoActions.Item.AutoObligor.Number))
          {
            continue;
          }
        }
        else if (Equal(entities.ExistingAdministrativeAction.Type1,
          export.AdminActions.Item.DetailAdministrativeAction.Type1) && Equal
          (entities.ExistingAdministrativeActCertification.TakenDate,
          export.AdminActions.Item.DetailAdministrativeActCertification.
            TakenDate) && Equal
          (entities.ExistingObligorCsePerson.Number,
          export.AdminActions.Item.DetailObligor.Number))
        {
          continue;
        }

        if (!IsEmpty(import.Required.Type1))
        {
          if (!Equal(entities.ExistingAdministrativeAction.Type1,
            import.Required.Type1))
          {
            continue;
          }
        }

        if (Lt(local.InitialisedToZeros.Date, import.StartDate.Date))
        {
          if (!Lt(entities.ExistingAdministrativeActCertification.TakenDate,
            import.StartDate.Date))
          {
            continue;
          }
        }

        if (Equal(entities.ExistingAdministrativeActCertification.Type1, "IRSC"))
          
        {
          if (!Lt(new DateTime(1, 1, 1),
            entities.ExistingAdministrativeActCertification.
              CseCentralOfficeApprovalDate))
          {
            continue;
          }
        }

        // If the import admin action type is spaces only allow the most current
        // on to be displayed for each type.
        if (IsEmpty(import.Required.Type1))
        {
          if (Equal(entities.ExistingAdministrativeAction.Type1, "SDSO"))
          {
            if (AsChar(local.SdsoAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.SdsoAlreadyMoved.Flag = "Y";
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1, "FDSO"))
          {
            if (AsChar(local.FdsoAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.FdsoAlreadyMoved.Flag = "Y";
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1, "CRED"))
          {
            if (AsChar(local.CredAlreadyMoved.Flag) == 'Y')
            {
              continue;
            }
            else
            {
              local.CredAlreadyMoved.Flag = "Y";
            }
          }
        }

        local.CsePersonsWorkSet.Number =
          entities.ExistingObligorCsePerson.Number;
        UseSiReadCsePerson();

        if (AsChar(import.ListOptManualSystAct.OneChar) == 'A')
        {
          ++export.AdminActions.Index;
          export.AdminActions.CheckSize();

          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          export.AdminActions.Update.DetailCertified.SelectChar = "Y";

          // ****************************************************************
          // *010500	RJEAN	PR84255	Populate FDSO current amount date with date 
          // sent so it displays.
          // ****************************************************************
          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            "FDSO"))
          {
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(entities.ExistingAdministrativeActCertification);
            export.AdminActions.Update.DetailAdministrativeActCertification.
              CurrentAmountDate =
                entities.ExistingAdministrativeActCertification.DateSent;
          }
          else
          {
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(entities.ExistingAdministrativeActCertification);
          }

          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            "FDSO"))
          {
            MoveAdministrativeActCertification(entities.
              ExistingFederalDebtSetoff,
              export.AdminActions.Update.DetailFederalDebtSetoff);

            if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.AdcAmount))
            {
              export.AdminActions.Update.DetailFederalDebtSetoff.NonAdcAmount =
                0;
            }
            else if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.NonAdcAmount))
            {
              export.AdminActions.Update.DetailFederalDebtSetoff.AdcAmount = 0;
            }

            export.AdminActions.Update.DetailAdministrativeActCertification.
              CurrentAmount =
                export.AdminActions.Item.DetailFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();
          }

          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          if (export.AdminActions.Index + 1 >= Export
            .AdminActionsGroup.Capacity)
          {
            goto Test1;
          }
        }
        else
        {
          ++local.AutoActions.Index;
          local.AutoActions.CheckSize();

          local.AutoActions.Update.AutoAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          local.AutoActions.Update.AutoCertified.SelectChar = "Y";

          // ****************************************************************
          // *010500	RJEAN	PR84255	Populate FDSO current amount date with date 
          // sent so it displays.
          // ****************************************************************
          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            "FDSO"))
          {
            local.AutoActions.Update.AutoAdministrativeActCertification.Assign(
              entities.ExistingAdministrativeActCertification);
            local.AutoActions.Update.AutoAdministrativeActCertification.
              CurrentAmountDate =
                entities.ExistingAdministrativeActCertification.DateSent;
          }
          else
          {
            local.AutoActions.Update.AutoAdministrativeActCertification.Assign(
              entities.ExistingAdministrativeActCertification);
          }

          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            "FDSO"))
          {
            MoveAdministrativeActCertification(entities.
              ExistingFederalDebtSetoff,
              local.AutoActions.Update.AutoFederalDebtSetoff);

            if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.AdcAmount))
            {
              local.AutoActions.Update.AutoFederalDebtSetoff.NonAdcAmount = 0;
            }
            else if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.NonAdcAmount))
            {
              local.AutoActions.Update.AutoFederalDebtSetoff.AdcAmount = 0;
            }

            local.AutoActions.Update.AutoAdministrativeActCertification.
              CurrentAmount =
                local.AutoActions.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();
          }

          local.AutoActions.Update.AutoObligation.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;
          local.AutoActions.Update.AutoObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          local.AutoActions.Update.AutoCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
          local.Auto.Flag = "Y";

          if (local.AutoActions.Index + 1 >= Local.AutoActionsGroup.Capacity)
          {
            break;
          }
        }
      }
    }

Test1:

    if (AsChar(import.ListOptManualSystAct.OneChar) == 'M' || IsEmpty
      (import.ListOptManualSystAct.OneChar))
    {
      local.SdsoAlreadyMoved.Flag = "N";
      export.AdminActions.Index = -1;
      local.ManualActions.Index = -1;

      // ---------------------------------------------
      // List only manual administrative action types namely CALL, LETR etc
      // i.e. those in the entity OBLIGATION_ADMINISTRATIVE_ACTION
      // ---------------------------------------------
      // *********************************************
      // Add qualification for tribunal, fips state and county after data model 
      // change
      // *********************************************
      foreach(var item in ReadAdministrativeActionObligationAdministrativeAction())
        
      {
        if (!ReadTribunal())
        {
          ExitState = "INVALID_TRIBUNAL";

          return;
        }

        if (IsEmpty(import.Fips.StateAbbreviation))
        {
          if (entities.ExistingTribunal.Identifier != import
            .Tribunal.Identifier)
          {
            continue;
          }
        }
        else if (ReadFips())
        {
          if (!Equal(entities.ExistingFips.StateAbbreviation,
            import.Fips.StateAbbreviation) || !
            Equal(entities.ExistingFips.CountyAbbreviation,
            import.Fips.CountyAbbreviation))
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        if (export.AdminActions.Index == -1 && local.ManualActions.Index == -1)
        {
        }
        else if (IsEmpty(import.ListOptManualSystAct.OneChar))
        {
          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.ManualActions.Item.ManualAdministrativeAction.Type1) && Equal
            (entities.ExistingObligationAdministrativeAction.TakenDate,
            local.ManualActions.Item.ManualObligationAdministrativeAction.
              TakenDate) && Equal
            (entities.ExistingObligorCsePerson.Number,
            local.ManualActions.Item.ManualObligor.Number))
          {
            continue;
          }
        }
        else if (Equal(entities.ExistingAdministrativeAction.Type1,
          export.AdminActions.Item.DetailAdministrativeAction.Type1) && Equal
          (entities.ExistingObligationAdministrativeAction.TakenDate,
          export.AdminActions.Item.DetailObligationAdministrativeAction.
            TakenDate) && Equal
          (entities.ExistingObligorCsePerson.Number,
          export.AdminActions.Item.DetailObligor.Number))
        {
          continue;
        }

        if (!IsEmpty(import.Required.Type1))
        {
          if (!Equal(entities.ExistingAdministrativeAction.Type1,
            import.Required.Type1))
          {
            continue;
          }
        }

        if (Lt(local.InitialisedToZeros.Date, import.StartDate.Date))
        {
          if (!Lt(entities.ExistingObligationAdministrativeAction.TakenDate,
            import.StartDate.Date))
          {
            continue;
          }
        }

        local.CsePersonsWorkSet.Number =
          entities.ExistingObligorCsePerson.Number;
        UseSiReadCsePerson();

        if (AsChar(import.ListOptManualSystAct.OneChar) == 'M')
        {
          ++export.AdminActions.Index;
          export.AdminActions.CheckSize();

          export.AdminActions.Update.DetailObligationAdministrativeAction.
            TakenDate =
              entities.ExistingObligationAdministrativeAction.TakenDate;
          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          export.AdminActions.Update.DetailCertified.SelectChar = "N";
          export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          // ---------------------------------------------
          // The following statement is needed since the same screen field is 
          // used for both entity types administration_act_certification and
          // obligation_administrative_action.
          // ---------------------------------------------
          export.AdminActions.Update.DetailAdministrativeActCertification.
            TakenDate =
              entities.ExistingObligationAdministrativeAction.TakenDate;

          if (export.AdminActions.Index + 1 >= Export
            .AdminActionsGroup.Capacity)
          {
            goto Test2;
          }
        }
        else
        {
          ++local.ManualActions.Index;
          local.ManualActions.CheckSize();

          local.ManualActions.Update.ManualObligationAdministrativeAction.
            TakenDate =
              entities.ExistingObligationAdministrativeAction.TakenDate;
          local.ManualActions.Update.ManualAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          local.ManualActions.Update.ManualObligation.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          local.ManualActions.Update.ManualObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          local.ManualActions.Update.ManualCertified.SelectChar = "N";
          local.ManualActions.Update.ManualCsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;

          // ---------------------------------------------
          // The following statement is needed since the same screen field is 
          // used for both entity types administration_act_certification and
          // obligation_administrative_action.
          // ---------------------------------------------
          local.ManualActions.Update.ManualAdministrativeActCertification.
            TakenDate =
              entities.ExistingObligationAdministrativeAction.TakenDate;
          local.Manual.Flag = "Y";

          if (local.ManualActions.Index + 1 >= Local
            .ManualActionsGroup.Capacity)
          {
            break;
          }
        }
      }
    }

Test2:

    if (AsChar(local.Auto.Flag) == 'Y' && AsChar(local.Manual.Flag) == 'Y')
    {
      local.AutoActions.Index = 0;
      local.AutoActions.CheckSize();

      export.AdminActions.Index = -1;

      local.ManualActions.Index = 0;
      local.ManualActions.CheckSize();

      // ****** The following will combine the auto and manual group views if 
      // the option was spaces. The group view is sorted based on taken date,
      // cse person number and type.
      for(export.AdminActions.Index = 0; export.AdminActions.Index < Export
        .AdminActionsGroup.Capacity; ++export.AdminActions.Index)
      {
        if (!export.AdminActions.CheckSize())
        {
          break;
        }

        if (Lt(local.ManualActions.Item.ManualAdministrativeActCertification.
          TakenDate,
          local.AutoActions.Item.AutoAdministrativeActCertification.TakenDate))
        {
          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            local.AutoActions.Item.AutoAdministrativeAction.Type1;
          export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
            local.AutoActions.Item.AutoCsePersonsWorkSet.FormattedName;
          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              local.AutoActions.Item.AutoObligation.SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailObligationAdministrativeAction.
            TakenDate =
              local.AutoActions.Item.AutoObligationAdministrativeAction.
              TakenDate;
          export.AdminActions.Update.DetailCertified.SelectChar =
            local.AutoActions.Item.AutoCertified.SelectChar;
          export.AdminActions.Update.DetailObligor.Number =
            local.AutoActions.Item.AutoObligor.Number;
          export.AdminActions.Update.DetailAdministrativeActCertification.
            Assign(local.AutoActions.Item.AutoAdministrativeActCertification);
          MoveAdministrativeActCertification(local.AutoActions.Item.
            AutoFederalDebtSetoff,
            export.AdminActions.Update.DetailFederalDebtSetoff);

          ++local.AutoActions.Index;
          local.AutoActions.CheckSize();

          continue;
        }

        if (Lt(local.AutoActions.Item.AutoAdministrativeActCertification.
          TakenDate,
          local.ManualActions.Item.ManualAdministrativeActCertification.
            TakenDate))
        {
          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            local.ManualActions.Item.ManualAdministrativeAction.Type1;
          export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
            local.ManualActions.Item.ManualCsePersonsWorkSet.FormattedName;
          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              local.ManualActions.Item.ManualObligation.
              SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailObligationAdministrativeAction.
            TakenDate =
              local.ManualActions.Item.ManualObligationAdministrativeAction.
              TakenDate;

          export.AdminActions.Update.DetailObligor.Number =
            local.ManualActions.Item.ManualObligor.Number;
          export.AdminActions.Update.DetailAdministrativeActCertification.
            Assign(local.ManualActions.Item.ManualAdministrativeActCertification);
            

          ++local.ManualActions.Index;
          local.ManualActions.CheckSize();

          continue;
        }

        if (Equal(local.AutoActions.Item.AutoAdministrativeActCertification.
          TakenDate,
          local.ManualActions.Item.ManualAdministrativeActCertification.
            TakenDate))
        {
          if (Equal(local.AutoActions.Item.AutoAdministrativeActCertification.
            TakenDate, local.InitialisedToZeros.Date) && IsEmpty
            (local.AutoActions.Item.AutoObligor.Number) && Equal
            (local.AutoActions.Item.AutoAdministrativeActCertification.
              TakenDate, local.InitialisedToZeros.Date) && IsEmpty
            (local.ManualActions.Item.ManualObligor.Number))
          {
            goto Test3;
          }

          if (Lt(local.ManualActions.Item.ManualObligor.Number,
            local.AutoActions.Item.AutoObligor.Number))
          {
            export.AdminActions.Update.DetailAdministrativeAction.Type1 =
              local.AutoActions.Item.AutoAdministrativeAction.Type1;
            export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
              local.AutoActions.Item.AutoCsePersonsWorkSet.FormattedName;
            export.AdminActions.Update.DetailObligation.
              SystemGeneratedIdentifier =
                local.AutoActions.Item.AutoObligation.SystemGeneratedIdentifier;
              
            export.AdminActions.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.AutoActions.Item.AutoObligationAdministrativeAction.
                TakenDate;
            export.AdminActions.Update.DetailCertified.SelectChar =
              local.AutoActions.Item.AutoCertified.SelectChar;
            export.AdminActions.Update.DetailObligor.Number =
              local.AutoActions.Item.AutoObligor.Number;
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(local.AutoActions.Item.AutoAdministrativeActCertification);
              
            MoveAdministrativeActCertification(local.AutoActions.Item.
              AutoFederalDebtSetoff,
              export.AdminActions.Update.DetailFederalDebtSetoff);

            ++local.AutoActions.Index;
            local.AutoActions.CheckSize();

            continue;
          }

          if (Lt(local.AutoActions.Item.AutoObligor.Number,
            local.ManualActions.Item.ManualObligor.Number))
          {
            export.AdminActions.Update.DetailAdministrativeAction.Type1 =
              local.ManualActions.Item.ManualAdministrativeAction.Type1;
            export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
              local.ManualActions.Item.ManualCsePersonsWorkSet.FormattedName;
            export.AdminActions.Update.DetailObligation.
              SystemGeneratedIdentifier =
                local.ManualActions.Item.ManualObligation.
                SystemGeneratedIdentifier;
            export.AdminActions.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.ManualActions.Item.ManualObligationAdministrativeAction.
                TakenDate;

            export.AdminActions.Update.DetailObligor.Number =
              local.ManualActions.Item.ManualObligor.Number;
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(local.ManualActions.Item.
                ManualAdministrativeActCertification);

            ++local.ManualActions.Index;
            local.ManualActions.CheckSize();

            continue;
          }

          if (Equal(local.AutoActions.Item.AutoObligor.Number,
            local.ManualActions.Item.ManualObligor.Number))
          {
            if (Lt(local.ManualActions.Item.ManualAdministrativeAction.Type1,
              local.AutoActions.Item.AutoAdministrativeActCertification.Type1))
            {
              export.AdminActions.Update.DetailAdministrativeAction.Type1 =
                local.AutoActions.Item.AutoAdministrativeAction.Type1;
              export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
                local.AutoActions.Item.AutoCsePersonsWorkSet.FormattedName;
              export.AdminActions.Update.DetailObligation.
                SystemGeneratedIdentifier =
                  local.AutoActions.Item.AutoObligation.
                  SystemGeneratedIdentifier;
              export.AdminActions.Update.DetailObligationAdministrativeAction.
                TakenDate =
                  local.AutoActions.Item.AutoObligationAdministrativeAction.
                  TakenDate;
              export.AdminActions.Update.DetailCertified.SelectChar =
                local.AutoActions.Item.AutoCertified.SelectChar;
              export.AdminActions.Update.DetailObligor.Number =
                local.AutoActions.Item.AutoObligor.Number;
              export.AdminActions.Update.DetailAdministrativeActCertification.
                Assign(local.AutoActions.Item.AutoAdministrativeActCertification);
                
              MoveAdministrativeActCertification(local.AutoActions.Item.
                AutoFederalDebtSetoff,
                export.AdminActions.Update.DetailFederalDebtSetoff);

              ++local.AutoActions.Index;
              local.AutoActions.CheckSize();

              continue;
            }

            if (Lt(local.AutoActions.Item.AutoAdministrativeActCertification.
              Type1,
              local.ManualActions.Item.ManualAdministrativeAction.Type1))
            {
              export.AdminActions.Update.DetailAdministrativeAction.Type1 =
                local.ManualActions.Item.ManualAdministrativeAction.Type1;
              export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
                local.ManualActions.Item.ManualCsePersonsWorkSet.FormattedName;
              export.AdminActions.Update.DetailObligation.
                SystemGeneratedIdentifier =
                  local.ManualActions.Item.ManualObligation.
                  SystemGeneratedIdentifier;
              export.AdminActions.Update.DetailObligationAdministrativeAction.
                TakenDate =
                  local.ManualActions.Item.ManualObligationAdministrativeAction.
                  TakenDate;

              export.AdminActions.Update.DetailObligor.Number =
                local.ManualActions.Item.ManualObligor.Number;
              export.AdminActions.Update.DetailAdministrativeActCertification.
                Assign(local.ManualActions.Item.
                  ManualAdministrativeActCertification);

              ++local.ManualActions.Index;
              local.ManualActions.CheckSize();

              continue;
            }
          }
        }
      }

      export.AdminActions.CheckIndex();
    }
    else if (AsChar(local.Auto.Flag) == 'Y' && AsChar(local.Manual.Flag) == 'N')
    {
      for(local.AutoActions.Index = 0; local.AutoActions.Index < local
        .AutoActions.Count; ++local.AutoActions.Index)
      {
        if (!local.AutoActions.CheckSize())
        {
          break;
        }

        export.AdminActions.Index = local.AutoActions.Index;
        export.AdminActions.CheckSize();

        export.AdminActions.Update.DetailAdministrativeAction.Type1 =
          local.AutoActions.Item.AutoAdministrativeAction.Type1;
        export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
          local.AutoActions.Item.AutoCsePersonsWorkSet.FormattedName;
        export.AdminActions.Update.DetailObligation.SystemGeneratedIdentifier =
          local.AutoActions.Item.AutoObligation.SystemGeneratedIdentifier;
        export.AdminActions.Update.DetailObligationAdministrativeAction.
          TakenDate =
            local.AutoActions.Item.AutoObligationAdministrativeAction.TakenDate;
          
        export.AdminActions.Update.DetailCertified.SelectChar =
          local.AutoActions.Item.AutoCertified.SelectChar;
        export.AdminActions.Update.DetailObligor.Number =
          local.AutoActions.Item.AutoObligor.Number;
        export.AdminActions.Update.DetailAdministrativeActCertification.Assign(
          local.AutoActions.Item.AutoAdministrativeActCertification);
        MoveAdministrativeActCertification(local.AutoActions.Item.
          AutoFederalDebtSetoff,
          export.AdminActions.Update.DetailFederalDebtSetoff);
      }

      local.AutoActions.CheckIndex();
    }
    else if (AsChar(local.Manual.Flag) == 'Y' && AsChar(local.Auto.Flag) == 'N')
    {
      for(local.ManualActions.Index = 0; local.ManualActions.Index < local
        .ManualActions.Count; ++local.ManualActions.Index)
      {
        if (!local.ManualActions.CheckSize())
        {
          break;
        }

        export.AdminActions.Index = local.ManualActions.Index;
        export.AdminActions.CheckSize();

        export.AdminActions.Update.DetailAdministrativeAction.Type1 =
          local.ManualActions.Item.ManualAdministrativeAction.Type1;
        export.AdminActions.Update.DetailCsePersonsWorkSet.FormattedName =
          local.ManualActions.Item.ManualCsePersonsWorkSet.FormattedName;
        export.AdminActions.Update.DetailObligation.SystemGeneratedIdentifier =
          local.ManualActions.Item.ManualObligation.SystemGeneratedIdentifier;
        export.AdminActions.Update.DetailObligationAdministrativeAction.
          TakenDate =
            local.ManualActions.Item.ManualObligationAdministrativeAction.
            TakenDate;

        export.AdminActions.Update.DetailObligor.Number =
          local.ManualActions.Item.ManualObligor.Number;
        export.AdminActions.Update.DetailAdministrativeActCertification.Assign(
          local.ManualActions.Item.ManualAdministrativeActCertification);
      }

      local.ManualActions.CheckIndex();
    }
    else
    {
    }

Test3:

    if (AsChar(local.Auto.Flag) == 'Y' && AsChar(local.Manual.Flag) == 'Y')
    {
      export.Common.Count = export.AdminActions.Index;
      export.AdminActions.Count = export.AdminActions.Index;
    }
    else
    {
      export.Common.Count = export.AdminActions.Index + 1;
    }
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool>
    ReadAdministrativeActCertificationAdministrativeAction()
  {
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingAdministrativeActCertification.Populated = false;
    entities.ExistingAdministrativeAction.Populated = false;
    entities.ExistingObligorCsePersonAccount.Populated = false;
    entities.ExistingObligorCsePerson.Populated = false;

    return ReadEach("ReadAdministrativeActCertificationAdministrativeAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.ExistingObligorCsePersonAccount.Type1 =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingObligorCsePersonAccount.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeActCertification.Type1 =
          db.GetString(reader, 2);
        entities.ExistingAdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.ExistingAdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 4);
        entities.ExistingAdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingAdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingAdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingAdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingAdministrativeActCertification.
          CseCentralOfficeApprovalDate = db.GetNullableDate(reader, 9);
        entities.ExistingAdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 10);
        entities.ExistingAdministrativeActCertification.TanfCode =
          db.GetString(reader, 11);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 12);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 13);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 14);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 15);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 16);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 18);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 20);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 21);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 22);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 23);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 24);
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingAdministrativeActCertification.Populated = true;
        entities.ExistingAdministrativeAction.Populated = true;
        entities.ExistingObligorCsePersonAccount.Populated = true;
        entities.ExistingObligorCsePerson.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingAdministrativeActCertification.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingObligorCsePersonAccount.Type1);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingAdministrativeActCertification.Type1);

        return true;
      });
  }

  private bool ReadAdministrativeAction()
  {
    entities.ExistingAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.Required.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadAdministrativeActionObligationAdministrativeAction()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingObligationAdministrativeAction.Populated = false;
    entities.ExistingAdministrativeAction.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligorCsePersonAccount.Populated = false;
    entities.ExistingObligorCsePerson.Populated = false;

    return ReadEach("ReadAdministrativeActionObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingObligationAdministrativeAction.AatType =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 1);
        entities.ExistingObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 4);
        entities.ExistingObligorCsePersonAccount.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligorCsePersonAccount.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 4);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 4);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 4);
        entities.ExistingObligationAdministrativeAction.CpaType =
          db.GetString(reader, 5);
        entities.ExistingObligation.CpaType = db.GetString(reader, 5);
        entities.ExistingObligorCsePersonAccount.Type1 =
          db.GetString(reader, 5);
        entities.ExistingObligorCsePersonAccount.Type1 =
          db.GetString(reader, 5);
        entities.ExistingObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 6);
        entities.ExistingObligationAdministrativeAction.ResponseDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingObligationAdministrativeAction.Response =
          db.GetNullableString(reader, 8);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 9);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 9);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 12);
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingObligationAdministrativeAction.Populated = true;
        entities.ExistingAdministrativeAction.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligorCsePersonAccount.Populated = true;
        entities.ExistingObligorCsePerson.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingObligorCsePersonAccount.Type1);
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingObligorCsePersonAccount.Type1);

        return true;
      });
  }

  private bool ReadFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingFederalDebtSetoff.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.SetDate(
          command, "takenDt",
          entities.ExistingAdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetString(
          command, "tanfCode",
          entities.ExistingAdministrativeActCertification.TanfCode);
        db.SetString(
          command, "type",
          entities.ExistingAdministrativeActCertification.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingFederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.ExistingFederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.ExistingFederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.ExistingFederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.ExistingFederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingFederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingFederalDebtSetoff.TanfCode = db.GetString(reader, 6);
        entities.ExistingFederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingFederalDebtSetoff.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingFederalDebtSetoff.Type1);
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
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionFipsTribunal()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingFips.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionFipsTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFips.State = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.ExistingFips.County = db.GetInt32(reader, 5);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingFips.Location = db.GetInt32(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 7);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 8);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 9);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 10);
        entities.ExistingTribunal.Name = db.GetString(reader, 11);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 12);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingFips.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of ListOptManualSystAct.
    /// </summary>
    [JsonPropertyName("listOptManualSystAct")]
    public Standard ListOptManualSystAct
    {
      get => listOptManualSystAct ??= new();
      set => listOptManualSystAct = value;
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
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private Standard listOptManualSystAct;
    private LegalAction legalAction;
    private AdministrativeAction required;
    private DateWorkArea startDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdminActionsGroup group.</summary>
    [Serializable]
    public class AdminActionsGroup
    {
      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCertified.
      /// </summary>
      [JsonPropertyName("detailCertified")]
      public Common DetailCertified
      {
        get => detailCertified ??= new();
        set => detailCertified = value;
      }

      /// <summary>
      /// A value of DetailObligor.
      /// </summary>
      [JsonPropertyName("detailObligor")]
      public CsePerson DetailObligor
      {
        get => detailObligor ??= new();
        set => detailObligor = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailCertified;
      private CsePerson detailObligor;
      private Obligation detailObligation;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// Gets a value of AdminActions.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionsGroup> AdminActions => adminActions ??= new(
      AdminActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminActions for json serialization.
    /// </summary>
    [JsonPropertyName("adminActions")]
    [Computed]
    public IList<AdminActionsGroup> AdminActions_Json
    {
      get => adminActions;
      set => AdminActions.Assign(value);
    }

    /// <summary>
    /// A value of OptionAutoManualAdmAc.
    /// </summary>
    [JsonPropertyName("optionAutoManualAdmAc")]
    public Standard OptionAutoManualAdmAc
    {
      get => optionAutoManualAdmAc ??= new();
      set => optionAutoManualAdmAc = value;
    }

    private Common common;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private Array<AdminActionsGroup> adminActions;
    private Standard optionAutoManualAdmAc;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ManualActionsGroup group.</summary>
    [Serializable]
    public class ManualActionsGroup
    {
      /// <summary>
      /// A value of ManualCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("manualCsePersonsWorkSet")]
      public CsePersonsWorkSet ManualCsePersonsWorkSet
      {
        get => manualCsePersonsWorkSet ??= new();
        set => manualCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ManualCertified.
      /// </summary>
      [JsonPropertyName("manualCertified")]
      public Common ManualCertified
      {
        get => manualCertified ??= new();
        set => manualCertified = value;
      }

      /// <summary>
      /// A value of ManualObligor.
      /// </summary>
      [JsonPropertyName("manualObligor")]
      public CsePerson ManualObligor
      {
        get => manualObligor ??= new();
        set => manualObligor = value;
      }

      /// <summary>
      /// A value of ManualObligation.
      /// </summary>
      [JsonPropertyName("manualObligation")]
      public Obligation ManualObligation
      {
        get => manualObligation ??= new();
        set => manualObligation = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualAdministrativeAction")]
      public AdministrativeAction ManualAdministrativeAction
      {
        get => manualAdministrativeAction ??= new();
        set => manualAdministrativeAction = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("manualAdministrativeActCertification")]
      public AdministrativeActCertification ManualAdministrativeActCertification
      {
        get => manualAdministrativeActCertification ??= new();
        set => manualAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of ManualFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("manualFederalDebtSetoff")]
      public AdministrativeActCertification ManualFederalDebtSetoff
      {
        get => manualFederalDebtSetoff ??= new();
        set => manualFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of ManualObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualObligationAdministrativeAction")]
      public ObligationAdministrativeAction ManualObligationAdministrativeAction
      {
        get => manualObligationAdministrativeAction ??= new();
        set => manualObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet manualCsePersonsWorkSet;
      private Common manualCertified;
      private CsePerson manualObligor;
      private Obligation manualObligation;
      private AdministrativeAction manualAdministrativeAction;
      private AdministrativeActCertification manualAdministrativeActCertification;
        
      private AdministrativeActCertification manualFederalDebtSetoff;
      private ObligationAdministrativeAction manualObligationAdministrativeAction;
        
    }

    /// <summary>A AutoActionsGroup group.</summary>
    [Serializable]
    public class AutoActionsGroup
    {
      /// <summary>
      /// A value of AutoCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("autoCsePersonsWorkSet")]
      public CsePersonsWorkSet AutoCsePersonsWorkSet
      {
        get => autoCsePersonsWorkSet ??= new();
        set => autoCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of AutoCertified.
      /// </summary>
      [JsonPropertyName("autoCertified")]
      public Common AutoCertified
      {
        get => autoCertified ??= new();
        set => autoCertified = value;
      }

      /// <summary>
      /// A value of AutoObligor.
      /// </summary>
      [JsonPropertyName("autoObligor")]
      public CsePerson AutoObligor
      {
        get => autoObligor ??= new();
        set => autoObligor = value;
      }

      /// <summary>
      /// A value of AutoObligation.
      /// </summary>
      [JsonPropertyName("autoObligation")]
      public Obligation AutoObligation
      {
        get => autoObligation ??= new();
        set => autoObligation = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoAdministrativeAction")]
      public AdministrativeAction AutoAdministrativeAction
      {
        get => autoAdministrativeAction ??= new();
        set => autoAdministrativeAction = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("autoAdministrativeActCertification")]
      public AdministrativeActCertification AutoAdministrativeActCertification
      {
        get => autoAdministrativeActCertification ??= new();
        set => autoAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of AutoFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("autoFederalDebtSetoff")]
      public AdministrativeActCertification AutoFederalDebtSetoff
      {
        get => autoFederalDebtSetoff ??= new();
        set => autoFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of AutoObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoObligationAdministrativeAction")]
      public ObligationAdministrativeAction AutoObligationAdministrativeAction
      {
        get => autoObligationAdministrativeAction ??= new();
        set => autoObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet autoCsePersonsWorkSet;
      private Common autoCertified;
      private CsePerson autoObligor;
      private Obligation autoObligation;
      private AdministrativeAction autoAdministrativeAction;
      private AdministrativeActCertification autoAdministrativeActCertification;
      private AdministrativeActCertification autoFederalDebtSetoff;
      private ObligationAdministrativeAction autoObligationAdministrativeAction;
    }

    /// <summary>
    /// A value of CredAlreadyMoved.
    /// </summary>
    [JsonPropertyName("credAlreadyMoved")]
    public Common CredAlreadyMoved
    {
      get => credAlreadyMoved ??= new();
      set => credAlreadyMoved = value;
    }

    /// <summary>
    /// A value of FdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("fdsoAlreadyMoved")]
    public Common FdsoAlreadyMoved
    {
      get => fdsoAlreadyMoved ??= new();
      set => fdsoAlreadyMoved = value;
    }

    /// <summary>
    /// A value of ManualDate.
    /// </summary>
    [JsonPropertyName("manualDate")]
    public DateWorkArea ManualDate
    {
      get => manualDate ??= new();
      set => manualDate = value;
    }

    /// <summary>
    /// A value of AutoDate.
    /// </summary>
    [JsonPropertyName("autoDate")]
    public DateWorkArea AutoDate
    {
      get => autoDate ??= new();
      set => autoDate = value;
    }

    /// <summary>
    /// A value of SdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("sdsoAlreadyMoved")]
    public Common SdsoAlreadyMoved
    {
      get => sdsoAlreadyMoved ??= new();
      set => sdsoAlreadyMoved = value;
    }

    /// <summary>
    /// A value of Manual.
    /// </summary>
    [JsonPropertyName("manual")]
    public Common Manual
    {
      get => manual ??= new();
      set => manual = value;
    }

    /// <summary>
    /// A value of Auto.
    /// </summary>
    [JsonPropertyName("auto")]
    public Common Auto
    {
      get => auto ??= new();
      set => auto = value;
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
    /// Gets a value of ManualActions.
    /// </summary>
    [JsonIgnore]
    public Array<ManualActionsGroup> ManualActions => manualActions ??= new(
      ManualActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ManualActions for json serialization.
    /// </summary>
    [JsonPropertyName("manualActions")]
    [Computed]
    public IList<ManualActionsGroup> ManualActions_Json
    {
      get => manualActions;
      set => ManualActions.Assign(value);
    }

    /// <summary>
    /// Gets a value of AutoActions.
    /// </summary>
    [JsonIgnore]
    public Array<AutoActionsGroup> AutoActions => autoActions ??= new(
      AutoActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AutoActions for json serialization.
    /// </summary>
    [JsonPropertyName("autoActions")]
    [Computed]
    public IList<AutoActionsGroup> AutoActions_Json
    {
      get => autoActions;
      set => AutoActions.Assign(value);
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private Common credAlreadyMoved;
    private Common fdsoAlreadyMoved;
    private DateWorkArea manualDate;
    private DateWorkArea autoDate;
    private Common sdsoAlreadyMoved;
    private Common manual;
    private Common auto;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ManualActionsGroup> manualActions;
    private Array<AutoActionsGroup> autoActions;
    private DateWorkArea current;
    private DateWorkArea initialisedToZeros;
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
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingLegalActionDetail")]
    public LegalActionDetail ExistingLegalActionDetail
    {
      get => existingLegalActionDetail ??= new();
      set => existingLegalActionDetail = value;
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
    /// A value of ExistingObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingObligationAdministrativeAction")]
    public ObligationAdministrativeAction ExistingObligationAdministrativeAction
    {
      get => existingObligationAdministrativeAction ??= new();
      set => existingObligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingAdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("existingAdminActionCertObligation")]
    public AdminActionCertObligation ExistingAdminActionCertObligation
    {
      get => existingAdminActionCertObligation ??= new();
      set => existingAdminActionCertObligation = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("existingAdministrativeActCertification")]
    public AdministrativeActCertification ExistingAdministrativeActCertification
    {
      get => existingAdministrativeActCertification ??= new();
      set => existingAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of ExistingFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingFederalDebtSetoff")]
    public AdministrativeActCertification ExistingFederalDebtSetoff
    {
      get => existingFederalDebtSetoff ??= new();
      set => existingFederalDebtSetoff = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAction")]
    public AdministrativeAction ExistingAdministrativeAction
    {
      get => existingAdministrativeAction ??= new();
      set => existingAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingObligorCsePersonAccount")]
    public CsePersonAccount ExistingObligorCsePersonAccount
    {
      get => existingObligorCsePersonAccount ??= new();
      set => existingObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
    }

    private Tribunal existingTribunal;
    private Fips existingFips;
    private LegalActionDetail existingLegalActionDetail;
    private LegalActionPerson existingLegalActionPerson;
    private LegalAction existingLegalAction;
    private ObligationAdministrativeAction existingObligationAdministrativeAction;
      
    private AdminActionCertObligation existingAdminActionCertObligation;
    private AdministrativeActCertification existingAdministrativeActCertification;
      
    private AdministrativeActCertification existingFederalDebtSetoff;
    private AdministrativeAction existingAdministrativeAction;
    private Obligation existingObligation;
    private CsePersonAccount existingObligorCsePersonAccount;
    private CsePerson existingObligorCsePerson;
  }
#endregion
}
