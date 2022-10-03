// Program: SI_READ_CASE_COMPS_BY_NAME, ID: 371761720, model: 746.
// Short name: SWE01201
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
/// A program: SI_READ_CASE_COMPS_BY_NAME.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This AB reads all the cases and people in those cases for a given person 
/// number.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCaseCompsByName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CASE_COMPS_BY_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCaseCompsByName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCaseCompsByName.
  /// </summary>
  public SiReadCaseCompsByName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	  Developer	   Description
    // 09-11-95  Helen Sharland   Initial Development
    // 10/29/96  G. Lofton	   Added case status and family
    // 				role.
    // 11/06/98  W. Campbell      If statement added to
    //                            interpret the ADABASE returned
    //                            exit state and change it to a
    //                            more user friendly exit state,
    //                            as per the SME request.
    // --------------------------------------------------
    // 06/26/99 W.Campbell       Disabled an IF statement
    //                           and replaced it with the one
    //                           following it to fix a problem whereby the
    //                           display of case_roles on a closed case
    //                           did not display the same person with
    //                           different case_roles when for inactives.
    // ----------------------------------------
    // 07/01/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 10/06/99 M.Lachowicz      Don't allow user to process
    //                           if user entered number for organization.
    //                           PR #75470.
    // ------------------------------------------------------------
    // 11/05/99 M.Lachowicz      Change property of READ
    //                           (Select Only and Cursor Only)
    // ------------------------------------------------------------
    // 03/07/00 W. Campbell      Added new views and logic
    //                           to add Family Violence indicator
    //                           to this CAB.  The Family Violence
    //                           Indicator is to be maintained from
    //                           Transaction - COMN which is what
    //                           uses this CAB.
    //                           Work done on WR# 00162 for
    //                           PRWORA - Family Violence.
    // ------------------------------------------------------
    // 06/05/00 W. Campbell      Moved Read for CSE PERSON
    //                           to allow the display of a Person's
    //                           Name when the person is on ADABAS,
    //                           but not on db2 table CSE_PERSON.
    //                           Work done on PR# 96651.
    // ------------------------------------------------------
    // 06/20/00 W. Campbell      Increased the size of local view
    //                           named local_group from 75 to 200 to fix
    //                           the view overflow problem reported on
    //                           PR# 97799.
    // -----------------------------------------------------
    // 06/21/00 W.Campbell       Made changes for WR#173
    //                           Family Violence Enhancements.
    //                           1. Added new field on the Screen -
    //                              FV letter sent (date).
    //                           2. Changed definition of PFK20
    //                              from SOURCE to FVltr.
    //                           Logic was added/changed to
    //                           accommodate these enhancements.
    //                           Added attribute FV_Letter_sent_date
    //                           to this CAB's Export view and
    //                           Entity Action view for CSE_PERSON.
    // ------------------------------------------------------------
    // 08/03/00 W.Campbell       Added code to provide OFFICE and
    //                           Service Provider info in the second
    //                           line of output for each case
    //                           provided in the list of Cases.
    //                           Work done on WR#00182.
    // ---------------------------------------------
    // ------------------------------------------------------
    // 03/07/00 W. Campbell -  Moved the following
    // move import to export statement to the beginning
    // of the cab from down in the logic so that it will
    // only be done once.  Work done on WR# 00162
    // for PRWORA - Family Violence.  Following
    // READ was added.
    // ------------------------------------------------------
    // ----------------------------------------------------------------------
    // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen for incoming and 
    // duplicates
    // ----------------------------------------------------------------------
    // 04/20/02 M.Lachowicz      Display Office/Service Prv.
    //                           PR 140974 and PR 139962.
    // ------------------------------------------------------------
    // ***********************************************************************
    // 09/27/07  CLocke  PR 218457 Corrected logic so when reading for an  
    // Interstate Request, interstate case from a foreign country twill be read
    // ***********************************************************************
    // 05/03/10  R.Mathews  CQ16830  To correct a scrolling issue, created a 
    // separate interstate
    //  indicator field on the screen instead of displaying 'Interstate' in the 
    // case number field.
    //  Also added a blank line in between cases for readability.
    // 05/17/10  R.Mathews  Revised logic for duplicates and/or blank lines that
    // fall at page break
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    local.Current.Date = Now().Date;

    if (CharAt(import.CsePersonsWorkSet.Number, 10) == 'O')
    {
      // 07/01/99 M.L         Change property of READ to generate
      //                      Select Only
      // ------------------------------------------------------------
      if (ReadCsePerson2())
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        // 10/06/99 M.L Start
        ExitState = "SI0000_ORG_CANNOT_BE_DISPLAYED";

        return;

        // 10/06/99 M.L End
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // ------------------------------------------------------
      // 06/05/00 W. Campbell - Moved the following
      // Read for CSE PERSON to allow the display of
      // a Person's Name when the person is on
      // ADABAS, but not on db2 table CSE_PERSON.
      // Work done on PR# 96651.  The READ was
      // moved to the bottom of this ELSE stmt.
      // ------------------------------------------------------
      local.ErrOnAdabasUnavailable.Flag = "Y";
      UseCabReadAdabasPerson1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // --------------------------------------------------
        // 11/06/98 W. Campbell - If statement added
        // to interpret the ADABASE returned exit state
        // and change it to a more user friendly exit state,
        // as per the SME request.
        // --------------------------------------------------
        if (IsExitState("ACO_ADABAS_PERSON_NF_113") || IsExitState
          ("ACO_ADABAS_PERSON_NF_114") || IsExitState
          ("ACO_ADABAS_PERSON_NF_149") || IsExitState
          ("ACO_ADABAS_PERSON_NF_152") || IsExitState
          ("ACO_ADABAS_PERSON_NF_154") || IsExitState
          ("ACO_ADABAS_PERSON_NF_161"))
        {
          ExitState = "CSE_PERSON_NF";
        }

        return;
      }

      local.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
      UseSiFormatCsePersonName();
      export.CsePersonsWorkSet.FormattedName =
        local.CsePersonsWorkSet.FormattedName;

      // ------------------------------------------------------
      // 06/05/00 W. Campbell - Moved this Read for CSE PERSON
      // to allow the display of a Person's Name when
      // the person is on ADABAS, but not on db2 table
      // CSE_PERSON.  Work done on PR# 96651.
      // This READ was moved from the top of this ELSE stmt.
      // ------------------------------------------------------
      // ------------------------------------------------------
      // 03/07/00 W. Campbell - Added logic to add Family
      // Violence indicator to this CAB.  The Family Violence
      // Indicator is to be maintained from Transaction - COMN
      // which is what uses this CAB.  Work done on
      // WR# 00162 for PRWORA - Family Violence.  Following
      // READ was added.
      // ------------------------------------------------------
      if (ReadCsePerson1())
      {
        export.CsePerson.Assign(entities.CsePerson);
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    local.MoreExists.Flag = "N";
    local.Group.Index = -1;
    local.WoDup.Index = -1;

    foreach(var item in ReadCase())
    {
      // ***********************************************************************
      // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen. Only display open
      //  incoming and duplicate interstate cases
      // ***********************************************************************
      // ***********************************************************************
      // 09/27/07  CLocke  PR 218457 Corrected the read for Interstate Request 
      // to allow interstate case from a foreign country to display.
      // ***********************************************************************
      if (AsChar(entities.Case1.Status) == 'C')
      {
        // ***********************************************************************
        // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen. Only display 
        // open
        //  incoming and duplicate interstate cases
        // ***********************************************************************
      }
      else
      {
        ReadInterstateRequest();
      }

      // ------------------------------------------------------------
      // Read all the people on this case
      // ------------------------------------------------------------
      // --------------------------------
      // Read all Active Roles
      // (Start Date <= Current Date <= End Date)
      // --------------------------------
      foreach(var item1 in ReadCaseRoleCsePerson1())
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.DetStatus.Text1 = "A";
        MoveCase1(entities.Case1, local.Group.Update.DetCase);
        local.Group.Update.DetCaseRole.Type1 = entities.CaseRole.Type1;
        local.Group.Update.DetCsePerson.Assign(entities.CsePerson);

        // ***********************************************************************
        // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen
        // ***********************************************************************
        if (entities.InterstateRequest.Populated)
        {
          if (AsChar(entities.Case1.DuplicateCaseIndicator) == 'Y')
          {
            local.Group.Update.DetInterInd.Text1 = "D";
          }
          else
          {
            local.Group.Update.DetInterInd.Text1 = "I";
          }
        }

        if (!Equal(entities.CaseRole.Type1, "CH"))
        {
          // 07/01/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadCaseRole2())
          {
            local.Group.Update.DetFamily.Type1 = entities.FaMo.Type1;
          }
        }
      }

      // --------------------------------
      // Read all Pending Roles
      // (Start Date > Current Date)
      // --------------------------------
      foreach(var item1 in ReadCaseRoleCsePerson2())
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.DetStatus.Text1 = "P";
        MoveCase1(entities.Case1, local.Group.Update.DetCase);
        local.Group.Update.DetCaseRole.Type1 = entities.CaseRole.Type1;
        local.Group.Update.DetCsePerson.Assign(entities.CsePerson);

        // ***********************************************************************
        // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen
        // ***********************************************************************
        if (entities.InterstateRequest.Populated)
        {
          if (AsChar(entities.Case1.DuplicateCaseIndicator) == 'Y')
          {
            local.Group.Update.DetInterInd.Text1 = "D";
          }
          else
          {
            local.Group.Update.DetInterInd.Text1 = "I";
          }
        }

        if (!Equal(entities.CaseRole.Type1, "CH"))
        {
          // 07/01/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadCaseRole2())
          {
            local.Group.Update.DetFamily.Type1 = entities.FaMo.Type1;
          }
        }
      }

      // --------------------------------
      // Read all Inactive Roles
      // (End Date < Current Date)
      // --------------------------------
      foreach(var item1 in ReadCaseRoleCsePerson3())
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.DetStatus.Text1 = "I";
        MoveCase1(entities.Case1, local.Group.Update.DetCase);
        local.Group.Update.DetCaseRole.Type1 = entities.CaseRole.Type1;
        local.Group.Update.DetCsePerson.Assign(entities.CsePerson);

        // ***********************************************************************
        // 11/01/01  T.Bobb WR 020143 Add KS_CASE_IND to screen
        // ***********************************************************************
        if (entities.InterstateRequest.Populated)
        {
          if (AsChar(entities.Case1.DuplicateCaseIndicator) == 'Y')
          {
            local.Group.Update.DetInterInd.Text1 = "D";
          }
          else
          {
            local.Group.Update.DetInterInd.Text1 = "I";
          }
        }

        if (!Equal(entities.CaseRole.Type1, "CH"))
        {
          // 07/01/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          // 11/05/99 M.L         Change property of READ to generate
          //                      Select and cursor
          // ------------------------------------------------------------
          if (ReadCaseRole1())
          {
            local.Group.Update.DetFamily.Type1 = entities.FaMo.Type1;
          }
        }
      }

      // R.Mathews  5/3/10  Add blank line on screen in between cases
      ++local.Group.Index;
      local.Group.CheckSize();
    }

    // R.Mathews  5/3/10  Remove blank line inserted after last case number 
    // retrieved.
    --local.Group.Index;
    local.Group.CheckSize();

    local.Match.Flag = "N";

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      if (Equal(local.Group.Item.DetCase.Number, import.PageKeyCase.Number))
      {
        // --------------------------
        // Weed out Persons that have
        // already been displayed.
        // --------------------------
        if (AsChar(local.Match.Flag) == 'N')
        {
          if (AsChar(local.Group.Item.DetStatus.Text1) == AsChar
            (import.PageKeyStatus.Text1) && Equal
            (local.Group.Item.DetCaseRole.Type1, import.PageKeyCaseRole.Type1) &&
            Equal
            (local.Group.Item.DetCsePerson.Number,
            import.PageKeyCsePersonsWorkSet.Number))
          {
            local.Match.Flag = "Y";
          }
          else
          {
            continue;
          }
        }
      }

      // 5/17/10  R.Mathews  Revised for duplicates at page break
      if (local.WoDup.Count == Local.WoDupGroup.Capacity)
      {
        if (IsEmpty(local.Group.Item.DetCsePerson.Number) || Equal
          (local.Group.Item.DetCase.Number, local.WoDup.Item.DetWoDupCase.Number)
          && Equal
          (local.Group.Item.DetCsePerson.Number,
          local.WoDup.Item.DetWoDupCsePerson.Number) && Equal
          (local.Group.Item.DetCaseRole.Type1,
          local.WoDup.Item.DetWoDupCaseRole.Type1) && AsChar
          (local.Group.Item.DetStatus.Text1) == AsChar
          (local.WoDup.Item.DetWoDupStat.Text1))
        {
          continue;
        }

        export.PageKeyCase.Number = local.Group.Item.DetCase.Number;
        export.PageKeyStatus.Text1 = local.Group.Item.DetStatus.Text1;
        export.PageKeyCaseRole.Type1 = local.Group.Item.DetCaseRole.Type1;
        export.PageKeyCsePersonsWorkSet.Number =
          local.Group.Item.DetCsePerson.Number;
        local.MoreExists.Flag = "Y";

        break;
      }

      if (local.WoDup.Count == 0)
      {
        local.WoDup.Index = 0;
        local.WoDup.CheckSize();

        MoveCase1(local.Group.Item.DetCase, local.WoDup.Update.DetWoDupCase);
        local.WoDup.Update.DetWoDupCaseRole.Type1 =
          local.Group.Item.DetCaseRole.Type1;
        local.WoDup.Update.DetWoDupCsePerson.Assign(
          local.Group.Item.DetCsePerson);
        local.WoDup.Update.DetWoDupFamily.Type1 =
          local.Group.Item.DetFamily.Type1;
        local.WoDup.Update.DetWoDupStat.Text1 =
          local.Group.Item.DetStatus.Text1;
        local.WoDup.Update.DetWoDupInd.Text1 =
          local.Group.Item.DetInterInd.Text1;
      }
      else
      {
        for(local.WoDup.Index = 0; local.WoDup.Index < local.WoDup.Count; ++
          local.WoDup.Index)
        {
          if (!local.WoDup.CheckSize())
          {
            break;
          }

          if (Equal(local.Group.Item.DetCase.Number,
            local.WoDup.Item.DetWoDupCase.Number))
          {
            if (IsEmpty(local.Group.Item.DetCase.Number))
            {
              goto Test1;
            }

            // ----------------------------------------
            // 06/26/99 W.Campbell - Disabled the following
            // IF stmt and replaced it with the one
            // following it to fix a problem whereby the
            // display of case_roles on a closed case
            // did not display the same person with
            // different case_roles when for inactives.
            // ----------------------------------------
            // ----------------------------------------
            //   *  *  *  *  *  *- IF local_grp_det cse_person number IS EQUAL 
            // TO
            //   *  *  *  *  *  *              local_grp_det_wo_dup cse_person 
            // number
            //   *  * <*-----------ESCAPE
            //   *  *  *  *  *  *--
            // ----------------------------------------
            if (Equal(local.Group.Item.DetCsePerson.Number,
              local.WoDup.Item.DetWoDupCsePerson.Number))
            {
              if (AsChar(local.Group.Item.DetStatus.Text1) == 'I' && AsChar
                (local.WoDup.Item.DetWoDupStat.Text1) == 'I')
              {
                if (Equal(local.Group.Item.DetCaseRole.Type1,
                  local.WoDup.Item.DetWoDupCaseRole.Type1))
                {
                  goto Test2;
                }
              }
              else
              {
                goto Test2;
              }
            }

            // ----------------------------------------
            // 06/26/99 W.Campbell - End of changes
            // made to disabled an IF statement
            // and replaced it with the one
            // following it to fix a problem whereby the
            // display of case_roles on a closed case
            // did not display the same person with
            // different case_roles when for inactives.
            // ----------------------------------------
          }

Test1:
          ;
        }

        local.WoDup.CheckIndex();

        local.WoDup.Index = local.WoDup.Count;
        local.WoDup.CheckSize();

        // R.Mathews  5/3/10  Force extra line for office and service provider 
        // if only one line for case
        // is displayed on scroll.
        if (local.WoDup.Index == 1 && IsEmpty
          (local.Group.Item.DetCsePerson.Number))
        {
          local.WoDup.Update.DetWoDupCase.Number = import.PageKeyCase.Number;

          local.WoDup.Index = local.WoDup.Count;
          local.WoDup.CheckSize();
        }

        MoveCase1(local.Group.Item.DetCase, local.WoDup.Update.DetWoDupCase);
        local.WoDup.Update.DetWoDupCaseRole.Type1 =
          local.Group.Item.DetCaseRole.Type1;
        local.WoDup.Update.DetWoDupCsePerson.Assign(
          local.Group.Item.DetCsePerson);
        local.WoDup.Update.DetWoDupFamily.Type1 =
          local.Group.Item.DetFamily.Type1;
        local.WoDup.Update.DetWoDupStat.Text1 =
          local.Group.Item.DetStatus.Text1;
        local.WoDup.Update.DetWoDupInd.Text1 =
          local.Group.Item.DetInterInd.Text1;
      }

Test2:
      ;
    }

    local.Group.CheckIndex();
    local.Prev.Number = "";
    local.WoDup.Index = 0;

    for(var limit = local.WoDup.Count; local.WoDup.Index < limit; ++
      local.WoDup.Index)
    {
      if (!local.WoDup.CheckSize())
      {
        break;
      }

      export.Export1.Index = local.WoDup.Index;
      export.Export1.CheckSize();

      if (Equal(local.WoDup.Item.DetWoDupCase.Number, local.Prev.Number))
      {
        export.Export1.Update.DetCase1.Number = "";
        export.Export1.Update.DetCase1.Status = "";
        export.Export1.Update.DetInter.Text1 = "";

        // ---------------------------------------------
        // 08/03/00 W.Campbell - Added code to
        // provide OFFICE and Service Provider
        // info in the second line of output for each
        // case provided in the list of Cases.
        // Work done on WR#00182.
        // ---------------------------------------------
        if (AsChar(local.CaseDisplayed.Flag) == 'Y')
        {
          local.CaseDisplayed.Flag = "N";

          // ---------------------------------------------
          // 08/03/00 W.Campbell - Call the CAB
          // to get the Abbreviated Office name and
          // the Service Provider's Name
          // Work done on WR#00182.
          // ---------------------------------------------
          UseSiReadOfficeOspHeader();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ---------------------------------------------
          // 08/03/00 W.Campbell - Put the 1st 3 chars
          // of Office name, a space and the 1st 6 chars
          // of the Service Provider's Name into the Case
          // number display field on the screen on the 2nd
          // line of output for the Case.
          // Work done on WR#00182.
          // ---------------------------------------------
          export.Export1.Update.DetCase1.Number =
            Substring(local.Office.Name, 1, 3);
          export.Export1.Update.DetCase1.Number =
            Substring(export.Export1.Item.DetCase1.Number,
            Case1.Number_MaxLength, 1, 4) + local.ServiceProvider.LastName;
        }

        // ---------------------------------------------
        // 08/03/00 W.Campbell - End of added code to
        // provide OFFICE and Service Provider
        // info in the second line of output for each
        // case provided in the list of Cases.
        // Work done on WR#00182.
        // ---------------------------------------------
      }
      else
      {
        local.Prev.Number = local.WoDup.Item.DetWoDupCase.Number;
        export.Export1.Update.DetCase1.Number =
          local.WoDup.Item.DetWoDupCase.Number;
        export.Export1.Update.DetCase1.Status =
          local.WoDup.Item.DetWoDupCase.Status ?? "";
        export.Export1.Update.DetInter.Text1 =
          local.WoDup.Item.DetWoDupInd.Text1;

        // ---------------------------------------------
        // 08/03/00 W.Campbell - Added following stmt to
        // provide OFFICE and Service Provider
        // info in the second line of output for each
        // case provided in the list of Cases.
        // Work done on WR#00182.
        // ---------------------------------------------
        local.CaseDisplayed.Flag = "Y";
      }

      export.Export1.Update.DetCase2.Type1 =
        local.WoDup.Item.DetWoDupCaseRole.Type1;
      export.Export1.Update.DetFamily.Type1 =
        local.WoDup.Item.DetWoDupFamily.Type1;
      export.Export1.Update.DetRoleStatus.Text1 =
        local.WoDup.Item.DetWoDupStat.Text1;

      if (AsChar(local.WoDup.Item.DetWoDupCsePerson.Type1) == 'O')
      {
        export.Export1.Update.DetCsePersonsWorkSet.Number =
          local.WoDup.Item.DetWoDupCsePerson.Number;
        export.Export1.Update.DetCsePersonsWorkSet.FormattedName =
          local.WoDup.Item.DetWoDupCsePerson.OrganizationName ?? Spaces(33);
      }
      else if (IsEmpty(local.WoDup.Item.DetWoDupCsePerson.Number))
      {
      }
      else
      {
        local.CsePersonsWorkSet.Number =
          local.WoDup.Item.DetWoDupCsePerson.Number;
        UseCabReadAdabasPerson2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --------------------------------------------------
          // 11/06/98 W. Campbell - If statement added
          // to interpret the ADABASE returned exit state
          // and change it to a more user friendly exit state,
          // as per the SME request.
          // --------------------------------------------------
          if (IsExitState("ACO_ADABAS_PERSON_NF_113") || IsExitState
            ("ACO_ADABAS_PERSON_NF_114") || IsExitState
            ("ACO_ADABAS_PERSON_NF_149") || IsExitState
            ("ACO_ADABAS_PERSON_NF_152") || IsExitState
            ("ACO_ADABAS_PERSON_NF_154") || IsExitState
            ("ACO_ADABAS_PERSON_NF_161"))
          {
            ExitState = "CSE_PERSON_NF";
          }

          export.Export1.Update.DetCsePersonsWorkSet.Number =
            local.WoDup.Item.DetWoDupCsePerson.Number;

          return;
        }

        UseSiFormatCsePersonName();
        export.Export1.Update.DetCsePersonsWorkSet.Assign(
          local.CsePersonsWorkSet);

        // ------------------------------------------------------
        // 03/07/00 W. Campbell - Added logic to add Family
        // Violence indicator to this CAB.  The Family Violence
        // Indicator is to be maintained from Transaction - COMN
        // which is what uses this CAB.  Work done on
        // WR# 00162 for PRWORA - Family Violence.  Following
        // MOVE statement was added.
        // ------------------------------------------------------
        MoveCsePerson(local.WoDup.Item.DetWoDupCsePerson,
          export.Export1.Update.DetCsePerson);
      }
    }

    local.WoDup.CheckIndex();

    if (import.Standard.PageNumber == 1)
    {
      if (AsChar(local.MoreExists.Flag) == 'Y')
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (AsChar(local.MoreExists.Flag) == 'Y')
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.FamilyViolenceIndicator = source.FamilyViolenceIndicator;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabReadAdabasPerson1()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPerson2()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = local.Prev.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    local.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, local.Office);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetString(command, "numb", import.PageKeyCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.FaMo.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FaMo.CasNumber = db.GetString(reader, 0);
        entities.FaMo.CspNumber = db.GetString(reader, 1);
        entities.FaMo.Type1 = db.GetString(reader, 2);
        entities.FaMo.Identifier = db.GetInt32(reader, 3);
        entities.FaMo.StartDate = db.GetNullableDate(reader, 4);
        entities.FaMo.EndDate = db.GetNullableDate(reader, 5);
        entities.FaMo.Populated = true;
        CheckValid<CaseRole>("Type1", entities.FaMo.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.FaMo.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FaMo.CasNumber = db.GetString(reader, 0);
        entities.FaMo.CspNumber = db.GetString(reader, 1);
        entities.FaMo.Type1 = db.GetString(reader, 2);
        entities.FaMo.Identifier = db.GetInt32(reader, 3);
        entities.FaMo.StartDate = db.GetNullableDate(reader, 4);
        entities.FaMo.EndDate = db.GetNullableDate(reader, 5);
        entities.FaMo.Populated = true;
        CheckValid<CaseRole>("Type1", entities.FaMo.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 9);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 9);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 9);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PageKeyCase.
    /// </summary>
    [JsonPropertyName("pageKeyCase")]
    public Case1 PageKeyCase
    {
      get => pageKeyCase ??= new();
      set => pageKeyCase = value;
    }

    /// <summary>
    /// A value of PageKeyStatus.
    /// </summary>
    [JsonPropertyName("pageKeyStatus")]
    public WorkArea PageKeyStatus
    {
      get => pageKeyStatus ??= new();
      set => pageKeyStatus = value;
    }

    /// <summary>
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
    }

    /// <summary>
    /// A value of PageKeyCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("pageKeyCsePersonsWorkSet")]
    public CsePersonsWorkSet PageKeyCsePersonsWorkSet
    {
      get => pageKeyCsePersonsWorkSet ??= new();
      set => pageKeyCsePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 pageKeyCase;
    private WorkArea pageKeyStatus;
    private CaseRole pageKeyCaseRole;
    private CsePersonsWorkSet pageKeyCsePersonsWorkSet;
    private Standard standard;
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
      /// A value of DetInter.
      /// </summary>
      [JsonPropertyName("detInter")]
      public WorkArea DetInter
      {
        get => detInter ??= new();
        set => detInter = value;
      }

      /// <summary>
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCase1.
      /// </summary>
      [JsonPropertyName("detCase1")]
      public Case1 DetCase1
      {
        get => detCase1 ??= new();
        set => detCase1 = value;
      }

      /// <summary>
      /// A value of DetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detCsePersonsWorkSet")]
      public CsePersonsWorkSet DetCsePersonsWorkSet
      {
        get => detCsePersonsWorkSet ??= new();
        set => detCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetCsePerson.
      /// </summary>
      [JsonPropertyName("detCsePerson")]
      public CsePerson DetCsePerson
      {
        get => detCsePerson ??= new();
        set => detCsePerson = value;
      }

      /// <summary>
      /// A value of DetCase2.
      /// </summary>
      [JsonPropertyName("detCase2")]
      public CaseRole DetCase2
      {
        get => detCase2 ??= new();
        set => detCase2 = value;
      }

      /// <summary>
      /// A value of DetFamily.
      /// </summary>
      [JsonPropertyName("detFamily")]
      public CaseRole DetFamily
      {
        get => detFamily ??= new();
        set => detFamily = value;
      }

      /// <summary>
      /// A value of DetRoleStatus.
      /// </summary>
      [JsonPropertyName("detRoleStatus")]
      public WorkArea DetRoleStatus
      {
        get => detRoleStatus ??= new();
        set => detRoleStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private WorkArea detInter;
      private Common detCommon;
      private Case1 detCase1;
      private CsePersonsWorkSet detCsePersonsWorkSet;
      private CsePerson detCsePerson;
      private CaseRole detCase2;
      private CaseRole detFamily;
      private WorkArea detRoleStatus;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PageKeyCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("pageKeyCsePersonsWorkSet")]
    public CsePersonsWorkSet PageKeyCsePersonsWorkSet
    {
      get => pageKeyCsePersonsWorkSet ??= new();
      set => pageKeyCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PageKeyStatus.
    /// </summary>
    [JsonPropertyName("pageKeyStatus")]
    public WorkArea PageKeyStatus
    {
      get => pageKeyStatus ??= new();
      set => pageKeyStatus = value;
    }

    /// <summary>
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
    }

    /// <summary>
    /// A value of PageKeyCase.
    /// </summary>
    [JsonPropertyName("pageKeyCase")]
    public Case1 PageKeyCase
    {
      get => pageKeyCase ??= new();
      set => pageKeyCase = value;
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

    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private CsePersonsWorkSet pageKeyCsePersonsWorkSet;
    private WorkArea pageKeyStatus;
    private CaseRole pageKeyCaseRole;
    private Case1 pageKeyCase;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetCase.
      /// </summary>
      [JsonPropertyName("detCase")]
      public Case1 DetCase
      {
        get => detCase ??= new();
        set => detCase = value;
      }

      /// <summary>
      /// A value of DetCsePerson.
      /// </summary>
      [JsonPropertyName("detCsePerson")]
      public CsePerson DetCsePerson
      {
        get => detCsePerson ??= new();
        set => detCsePerson = value;
      }

      /// <summary>
      /// A value of DetStatus.
      /// </summary>
      [JsonPropertyName("detStatus")]
      public WorkArea DetStatus
      {
        get => detStatus ??= new();
        set => detStatus = value;
      }

      /// <summary>
      /// A value of DetCaseRole.
      /// </summary>
      [JsonPropertyName("detCaseRole")]
      public CaseRole DetCaseRole
      {
        get => detCaseRole ??= new();
        set => detCaseRole = value;
      }

      /// <summary>
      /// A value of DetFamily.
      /// </summary>
      [JsonPropertyName("detFamily")]
      public CaseRole DetFamily
      {
        get => detFamily ??= new();
        set => detFamily = value;
      }

      /// <summary>
      /// A value of DetInter.
      /// </summary>
      [JsonPropertyName("detInter")]
      public TextWorkArea DetInter
      {
        get => detInter ??= new();
        set => detInter = value;
      }

      /// <summary>
      /// A value of DetInterInd.
      /// </summary>
      [JsonPropertyName("detInterInd")]
      public TextWorkArea DetInterInd
      {
        get => detInterInd ??= new();
        set => detInterInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 400;

      private Case1 detCase;
      private CsePerson detCsePerson;
      private WorkArea detStatus;
      private CaseRole detCaseRole;
      private CaseRole detFamily;
      private TextWorkArea detInter;
      private TextWorkArea detInterInd;
    }

    /// <summary>A WoDupGroup group.</summary>
    [Serializable]
    public class WoDupGroup
    {
      /// <summary>
      /// A value of DetWoDupCase.
      /// </summary>
      [JsonPropertyName("detWoDupCase")]
      public Case1 DetWoDupCase
      {
        get => detWoDupCase ??= new();
        set => detWoDupCase = value;
      }

      /// <summary>
      /// A value of DetWoDupCsePerson.
      /// </summary>
      [JsonPropertyName("detWoDupCsePerson")]
      public CsePerson DetWoDupCsePerson
      {
        get => detWoDupCsePerson ??= new();
        set => detWoDupCsePerson = value;
      }

      /// <summary>
      /// A value of DetWoDupStat.
      /// </summary>
      [JsonPropertyName("detWoDupStat")]
      public WorkArea DetWoDupStat
      {
        get => detWoDupStat ??= new();
        set => detWoDupStat = value;
      }

      /// <summary>
      /// A value of DetWoDupCaseRole.
      /// </summary>
      [JsonPropertyName("detWoDupCaseRole")]
      public CaseRole DetWoDupCaseRole
      {
        get => detWoDupCaseRole ??= new();
        set => detWoDupCaseRole = value;
      }

      /// <summary>
      /// A value of DetWoDupFamily.
      /// </summary>
      [JsonPropertyName("detWoDupFamily")]
      public CaseRole DetWoDupFamily
      {
        get => detWoDupFamily ??= new();
        set => detWoDupFamily = value;
      }

      /// <summary>
      /// A value of DetWoDupInter.
      /// </summary>
      [JsonPropertyName("detWoDupInter")]
      public TextWorkArea DetWoDupInter
      {
        get => detWoDupInter ??= new();
        set => detWoDupInter = value;
      }

      /// <summary>
      /// A value of DetWoDupInd.
      /// </summary>
      [JsonPropertyName("detWoDupInd")]
      public TextWorkArea DetWoDupInd
      {
        get => detWoDupInd ??= new();
        set => detWoDupInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Case1 detWoDupCase;
      private CsePerson detWoDupCsePerson;
      private WorkArea detWoDupStat;
      private CaseRole detWoDupCaseRole;
      private CaseRole detWoDupFamily;
      private TextWorkArea detWoDupInter;
      private TextWorkArea detWoDupInd;
    }

    /// <summary>
    /// A value of PrevInter.
    /// </summary>
    [JsonPropertyName("prevInter")]
    public TextWorkArea PrevInter
    {
      get => prevInter ??= new();
      set => prevInter = value;
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
    /// A value of CaseDisplayed.
    /// </summary>
    [JsonPropertyName("caseDisplayed")]
    public Common CaseDisplayed
    {
      get => caseDisplayed ??= new();
      set => caseDisplayed = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of MoreExists.
    /// </summary>
    [JsonPropertyName("moreExists")]
    public Common MoreExists
    {
      get => moreExists ??= new();
      set => moreExists = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// Gets a value of WoDup.
    /// </summary>
    [JsonIgnore]
    public Array<WoDupGroup> WoDup => woDup ??= new(WoDupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WoDup for json serialization.
    /// </summary>
    [JsonPropertyName("woDup")]
    [Computed]
    public IList<WoDupGroup> WoDup_Json
    {
      get => woDup;
      set => WoDup.Assign(value);
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of WoDupCheck.
    /// </summary>
    [JsonPropertyName("woDupCheck")]
    public Case1 WoDupCheck
    {
      get => woDupCheck ??= new();
      set => woDupCheck = value;
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
    /// A value of Match.
    /// </summary>
    [JsonPropertyName("match")]
    public Common Match
    {
      get => match ??= new();
      set => match = value;
    }

    private TextWorkArea prevInter;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseDisplayed;
    private DateWorkArea current;
    private Case1 prev;
    private Common moreExists;
    private Array<GroupGroup> group;
    private Array<WoDupGroup> woDup;
    private Common errOnAdabasUnavailable;
    private Case1 woDupCheck;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common match;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of FaMo.
    /// </summary>
    [JsonPropertyName("faMo")]
    public CaseRole FaMo
    {
      get => faMo ??= new();
      set => faMo = value;
    }

    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseRole faMo;
  }
#endregion
}
