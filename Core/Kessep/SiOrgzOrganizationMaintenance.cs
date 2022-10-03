// Program: SI_ORGZ_ORGANIZATION_MAINTENANCE, ID: 371765710, model: 746.
// Short name: SWEORGZP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ORGZ_ORGANIZATION_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiOrgzOrganizationMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ORGZ_ORGANIZATION_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiOrgzOrganizationMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiOrgzOrganizationMaintenance.
  /// </summary>
  public SiOrgzOrganizationMaintenance(IContext context, Import import,
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 09/05/95  Helen Sharland	Initial Development
    // 03/19/96  P . Elie		Retrofits
    // 05/06/96  Rao Mulpuri		IDCR# 45 Changes to Display
    // 				/F7&F8 logic
    // 11/02/96  G. Lofton		Add new security and removed
    // 				old.
    // 11/06/96  H. Kennedy		Added logic to return flow
    // 				to DDMM when flowing from
    // 				there.  This required that
    // 				auto flows be removed.
    // 12-18-96  Govind  IDCR 252	Associate with FIPS
    // 09/18/98  W. Campbell           Added logic to limit the
    //                                 
    // number of rows selected for
    // CREATE
    //                                 
    // or UPDATE processing on a given
    //                                 
    // execution of this PRAD to 1 (ONE
    // ).
    // 05/25/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // ------------------------------------------------------------
    // 07/01/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // November, 2000 M. Brown, pr# 106669:  Need to display zeros for county
    // when a fips is 20-000-03, but blanks the rest of the time.
    // ------------------------------------------------------------
    // Oct, 2000 M. Brown  PR# 106234 Updated NEXT TRAN.
    // August, 2001 M. Brown, pr# 124003: Added sort desc on end date read 
    // eaches of CSE Person Address in the display cab.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Allow the user to clear the screen
      // ---------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field1 =
          GetField(export.Export1.Item.DetailCsePerson, "organizationName");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;

        var field2 = GetField(export.Export1.Item.DetailCsePerson, "taxId");

        field2.Color = "green";
        field2.Highlighting = Highlighting.Underscore;
        field2.Protected = false;

        var field3 =
          GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");

        field3.Color = "green";
        field3.Highlighting = Highlighting.Underscore;
        field3.Protected = false;

        var field4 = GetField(export.Export1.Item.DisplayState, "text2");

        field4.Color = "green";
        field4.Highlighting = Highlighting.Underscore;
        field4.Protected = false;

        var field5 = GetField(export.Export1.Item.DisplayCounty, "text3");

        field5.Color = "green";
        field5.Highlighting = Highlighting.Underscore;
        field5.Protected = false;

        var field6 = GetField(export.Export1.Item.DisplayLocation, "text2");

        field6.Color = "green";
        field6.Highlighting = Highlighting.Underscore;
        field6.Protected = false;

        var field7 = GetField(export.Export1.Item.DetailPromptFips, "oneChar");

        field7.Color = "green";
        field7.Highlighting = Highlighting.Underscore;
        field7.Protected = false;
      }

      export.Export1.CheckIndex();

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveStandard(import.Standard, export.Standard);
    export.StartingSearchCsePerson.Assign(import.StartingSearchCsePerson);
    MoveCsePerson2(import.SearchNamesLike, export.SearchNamesLike);
    export.StartingSearchFips.Assign(import.StartingSearchFips);
    export.StartingSearchCity.Assign(import.StartingSearchCity);
    export.PromptStateCode.PromptField = import.PromptStateCode.PromptField;
    export.FromDdmm.Flag = import.FromDdmm.Flag;
    export.Next.Number = import.Next.Number;
    local.Valid.Count = 0;

    for(import.Import1.Index = 0; import.Import1.Index < Import
      .ImportGroup.Capacity; ++import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      if (Equal(global.Command, "DISPLAY"))
      {
        // --- Don't move the imports to exports. They will get refreshed
      }
      else
      {
        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailCsePerson.Assign(
          import.Import1.Item.DetailCsePerson);

        if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 'S')
        {
          ++local.Valid.Count;
        }
        else
        {
        }

        export.Export1.Update.DetailFips.Assign(import.Import1.Item.DetailFips);
        export.Export1.Update.DisplayCounty.Text3 =
          import.Import1.Item.DisplayCounty.Text3;
        export.Export1.Update.DisplayState.Text2 =
          import.Import1.Item.DisplayLocation.Text2;

        export.Export1.Update.DetailPromptFips.OneChar =
          import.Import1.Item.DetailPromptFips.OneChar;
        export.Export1.Update.DetailCsePersonAddress.Assign(
          import.Import1.Item.DetailCsePersonAddress);
        export.Export1.Update.DetailOrgzIsTrib.Flag =
          import.Import1.Item.DetailOrgzIsTrib.Flag;
      }

      if (AsChar(export.Export1.Item.DetailOrgzIsTrib.Flag) == 'Y')
      {
        var field1 =
          GetField(export.Export1.Item.DetailCsePerson, "organizationName");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.DetailCsePerson, "taxId");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");

        field3.Color = "cyan";
        field3.Protected = true;

        // M Brown, pr# 106669, November 2000.
        var field4 = GetField(export.Export1.Item.DisplayState, "text2");

        field4.Color = "cyan";
        field4.Highlighting = Highlighting.Normal;
        field4.Protected = true;

        var field5 = GetField(export.Export1.Item.DisplayCounty, "text3");

        field5.Color = "cyan";
        field5.Highlighting = Highlighting.Normal;
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.DisplayLocation, "text2");

        field6.Color = "cyan";
        field6.Highlighting = Highlighting.Normal;
        field6.Protected = true;

        var field7 = GetField(export.Export1.Item.DetailPromptFips, "oneChar");

        field7.Color = "cyan";
        field7.Protected = true;
      }
      else
      {
        var field1 =
          GetField(export.Export1.Item.DetailCsePerson, "organizationName");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;

        var field2 = GetField(export.Export1.Item.DetailCsePerson, "taxId");

        field2.Color = "green";
        field2.Highlighting = Highlighting.Underscore;
        field2.Protected = false;

        var field3 =
          GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");

        field3.Color = "green";
        field3.Highlighting = Highlighting.Underscore;
        field3.Protected = false;

        // M Brown, pr# 106669, November 2000.
        var field4 = GetField(export.Export1.Item.DisplayState, "text2");

        field4.Color = "green";
        field4.Highlighting = Highlighting.Underscore;
        field4.Protected = false;

        var field5 = GetField(export.Export1.Item.DisplayCounty, "text3");

        field5.Color = "green";
        field5.Highlighting = Highlighting.Underscore;
        field5.Protected = false;

        var field6 = GetField(export.Export1.Item.DisplayLocation, "text2");

        field6.Color = "green";
        field6.Highlighting = Highlighting.Underscore;
        field6.Protected = false;

        var field7 = GetField(export.Export1.Item.DetailPromptFips, "oneChar");

        field7.Color = "green";
        field7.Highlighting = Highlighting.Underscore;
        field7.Protected = false;
      }
    }

    import.Import1.CheckIndex();

    // ------------------------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ------------------------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;
    export.HiddenPrev.PageNumber = import.HiddenPrev.PageNumber;

    if (Equal(global.Command, "DISPLAY"))
    {
      export.HiddenStandard.PageNumber = 0;
      export.HiddenPrev.PageNumber = 0;
    }
    else
    {
      for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < Import
        .HiddenPageKeysGroup.Capacity; ++import.HiddenPageKeys.Index)
      {
        if (!import.HiddenPageKeys.CheckSize())
        {
          break;
        }

        export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.HiddenPageKey.Assign(
          import.HiddenPageKeys.Item.HiddenPageKey);
      }

      import.HiddenPageKeys.CheckIndex();
    }

    if (export.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // M Brown, pr#106234, November 2000.  NEXT TRAN updates.
      if (!Equal(export.Next.Number, export.HiddenNextTranInfo.CaseNumber))
      {
        export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
        export.HiddenNextTranInfo.CsePersonNumberAp = "";
        export.HiddenNextTranInfo.CsePersonNumberObligor = "";
        export.HiddenNextTranInfo.StandardCrtOrdNumber = "";
        export.HiddenNextTranInfo.LegalActionIdentifier = 0;
        export.HiddenNextTranInfo.MiscNum1 = 0;
      }

      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETFIPL") || Equal
      (global.Command, "FIPS") || Equal(global.Command, "TRIB") || Equal
      (global.Command, "NADR") || Equal(global.Command, "NADS") || Equal
      (global.Command, "RETCDVL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------
    // Any commands specific to your procedure
    // should be added to the following CASE OF
    // statement.
    // Make sure that all unused PF keys are set to
    // INVALID on the screen definition.
    // ---------------------------------------------
    // ---------------------------------------------
    // 09/18/98  W. Campbell - The following
    // IF statement was added to limit the
    // number of rows selected for CREATE or
    // UPDATE processing on a given execution
    // of this PRAD to 1 (ONE).
    // ---------------------------------------------
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      // ---------------------------------------------------------
      // Check that only one selection has been made.
      // ---------------------------------------------------------
      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            if (local.Common.Count > 1)
            {
              // 05/25/99 M. Lachowicz      Replace zdel exit state by
              //                            by new exit state.
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            return;
        }
      }

      export.Export1.CheckIndex();
    }

    // ---------------------------------------------
    // End of IF statement added on
    // 09/18/98 by  W. Campbell
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        switch(AsChar(export.PromptStateCode.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.DlgflwRequiredCode.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            // 05/25/99 M. Lachowicz      Replace zdel exit state by
            //                            by new exit state.
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field = GetField(export.PromptStateCode, "promptField");

            field.Error = true;

            return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.DetailPromptFips.OneChar))
          {
            case ' ':
              break;
            case 'S':
              MoveFips1(export.Export1.Item.DetailFips,
                export.DlgflwRequiredFips);
              ExitState = "ECO_LNK_TO_LIST_FIPS";

              return;
            default:
              // 05/25/99 M. Lachowicz      Replace zdel exit state by
              //                            by new exit state.
              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              var field =
                GetField(export.Export1.Item.DetailPromptFips, "oneChar");

              field.Error = true;

              return;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      case "RETCDVL":
        if (AsChar(export.PromptStateCode.PromptField) == 'S')
        {
          export.PromptStateCode.PromptField = "";
          export.StartingSearchFips.StateAbbreviation =
            import.DlgflwSelectedCodeValue.Cdvalue;

          var field = GetField(export.StartingSearchFips, "countyAbbreviation");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "RETFIPL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.DetailPromptFips.OneChar))
          {
            case ' ':
              break;
            case 'S':
              export.Export1.Update.DetailPromptFips.OneChar = "";

              // M Brown, pr# 106669, November 2000.
              export.Export1.Update.DetailFips.
                Assign(import.DlgflwSelectedFips);

              if (export.Export1.Item.DetailFips.County == 0 && export
                .Export1.Item.DetailFips.Location == 0 && export
                .Export1.Item.DetailFips.State == 0)
              {
                export.Export1.Update.DisplayLocation.Text2 = "";
                export.Export1.Update.DisplayCounty.Text3 = "";
                export.Export1.Update.DisplayState.Text2 = "";
              }
              else
              {
                export.Export1.Update.DisplayLocation.Text2 =
                  NumberToString(export.Export1.Item.DetailFips.Location, 2);
                export.Export1.Update.DisplayCounty.Text3 =
                  NumberToString(export.Export1.Item.DetailFips.County, 3);
                export.Export1.Update.DisplayState.Text2 =
                  NumberToString(export.Export1.Item.DetailFips.State, 2);
              }

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Protected = false;
              field.Focused = true;

              goto Test;
            default:
              break;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKey.Number) && IsEmpty
          (export.HiddenPageKeys.Item.HiddenPageKey.OrganizationName) && IsEmpty
          (export.HiddenPageKeys.Item.HiddenPageKey.TaxId))
        {
          // 05/25/99 M. Lachowicz      Replace zdel exit state by
          //                            by new exit state.
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "CREATE":
        if (local.Valid.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(import.Import1.Index = 0; import.Import1.Index < Import
          .ImportGroup.Capacity; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (IsEmpty(import.Import1.Item.DetailCsePerson.OrganizationName))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "organizationName");

                field1.Error = true;
              }

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }

              break;
          }
        }

        import.Import1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(import.Import1.Index = 0; import.Import1.Index < Import
          .ImportGroup.Capacity; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (export.Export1.Item.DetailFips.State > 0 || export
                .Export1.Item.DetailFips.County > 0 || export
                .Export1.Item.DetailFips.Location > 0)
              {
                // 07/01/99 M.L         Change property of READ to generate
                //                      Select Only
                // ------------------------------------------------------------
                if (ReadCsePerson4())
                {
                  ExitState = "SI0000_ORGZ_AE_FOR_FIPS";

                  var field1 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePerson,
                    "organizationName");

                  field2.Error = true;

                  // M Brown, pr# 106669, November 2000.
                  var field3 =
                    GetField(export.Export1.Item.DisplayState, "text2");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Export1.Item.DisplayCounty, "text3");

                  field4.Error = true;

                  var field5 =
                    GetField(export.Export1.Item.DisplayLocation, "text2");

                  field5.Error = true;

                  break;
                }
              }

              if (!IsEmpty(export.Export1.Item.DetailCsePerson.TaxId))
              {
                if (ReadCsePerson2())
                {
                  ExitState = "SI0000_ORGZ_AE_FOR_TAX_ID";

                  var field1 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePerson, "taxId");

                  field2.Error = true;

                  var field3 =
                    GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");
                    

                  field3.Error = true;

                  break;
                }
              }

              local.CsePerson.Assign(import.Import1.Item.DetailCsePerson);
              local.CsePerson.Type1 = "O";
              UseSiCreateCsePerson();

              break;
            case ' ':
              break;
            default:
              break;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }
        }

        import.Import1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        global.Command = "DISPLAY";

        break;
      case "UPDATE":
        if (export.HiddenPrev.PageNumber < 1)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (local.Valid.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(import.Import1.Index = 0; import.Import1.Index < Import
          .ImportGroup.Capacity; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (IsEmpty(import.Import1.Item.DetailCsePerson.OrganizationName))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "organizationName");

                field1.Error = true;
              }

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }

              break;
          }
        }

        import.Import1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(import.Import1.Index = 0; import.Import1.Index < Import
          .ImportGroup.Capacity; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (!IsEmpty(export.Export1.Item.DetailCsePerson.TaxId))
              {
                if (ReadCsePerson1())
                {
                  ExitState = "SI0000_ORGZ_AE_FOR_TAX_ID";

                  var field1 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePerson, "taxId");

                  field2.Error = true;

                  var field3 =
                    GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");
                    

                  field3.Error = true;

                  break;
                }
              }

              if (export.Export1.Item.DetailFips.State > 0 || export
                .Export1.Item.DetailFips.County > 0 || export
                .Export1.Item.DetailFips.Location > 0)
              {
                // 07/01/99 M.L         Change property of READ to generate
                //                      Select Only
                // ------------------------------------------------------------
                if (ReadCsePerson3())
                {
                  ExitState = "SI0000_ORGZ_AE_FOR_FIPS";

                  var field1 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePerson,
                    "organizationName");

                  field2.Error = true;

                  // M Brown, pr# 106669, November 2000.
                  var field3 =
                    GetField(export.Export1.Item.DisplayState, "text2");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Export1.Item.DisplayCounty, "text3");

                  field4.Error = true;

                  var field5 =
                    GetField(export.Export1.Item.DisplayLocation, "text2");

                  field5.Error = true;

                  break;
                }

                if (ReadTribunal())
                {
                  // ---------------------------------------------
                  // This organization has a corresponding TRIBUNAL record. So 
                  // update it to keep both in sync.
                  // ---------------------------------------------
                  try
                  {
                    UpdateTribunal();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "TRIBUNAL_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }

              local.CsePerson.Assign(import.Import1.Item.DetailCsePerson);
              local.CsePerson.Type1 = "O";
              UseSiUpdateCsePerson();

              break;
            case ' ':
              break;
            default:
              break;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }
        }

        import.Import1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        export.HiddenStandard.PageNumber = 1;

        for(export.HiddenPageKeys.Index = 0; export.HiddenPageKeys.Index < Export
          .HiddenPageKeysGroup.Capacity; ++export.HiddenPageKeys.Index)
        {
          if (!export.HiddenPageKeys.CheckSize())
          {
            break;
          }

          export.HiddenPageKeys.Update.HiddenPageKey.Number = "";
          export.HiddenPageKeys.Update.HiddenPageKey.OrganizationName = "";
        }

        export.HiddenPageKeys.CheckIndex();

        break;
      case "EXIT":
        if (AsChar(export.FromDdmm.Flag) == 'Y')
        {
          ExitState = "ECO_XFR_TO_DBT_DIST_MNGMNT_MENU";
        }
        else
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "FIPS":
        ExitState = "ECO_LNK_TO_FIPS";

        break;
      case "TRIB":
        ExitState = "ECO_LNK_TO_TRIB";

        break;
      case "NADR":
        ExitState = "ECO_LNK_TO_NADR";

        break;
      case "NADS":
        ExitState = "ECO_LNK_TO_NADS";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        break;
    }

Test:

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "FIPS") || Equal
      (global.Command, "TRIB") || Equal(global.Command, "NADR") || Equal
      (global.Command, "NADS"))
    {
      // ---------------------------------------------------------
      // Check that only one selection has been made before
      // returning to calling procedure.
      // ---------------------------------------------------------
      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            if (local.Common.Count > 1)
            {
              // 05/25/99 M. Lachowicz      Replace zdel exit state by
              //                            by new exit state.
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            export.HiddenSelected.FormattedName =
              export.Export1.Item.DetailCsePerson.OrganizationName ?? Spaces
              (33);
            export.HiddenSelected.Number =
              export.Export1.Item.DetailCsePerson.Number;
            MoveFips1(export.Export1.Item.DetailFips, export.DlgflwRequiredFips);
              

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            return;
        }
      }

      export.Export1.CheckIndex();

      return;
    }

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      if (!IsEmpty(export.StartingSearchCsePerson.Number))
      {
        local.InCsePerson.Text10 = export.StartingSearchCsePerson.Number;
        UseEabPadLeftWithZeros();
        export.StartingSearchCsePerson.Number = local.OutCsePerson.Text10;

        if (CharAt(export.StartingSearchCsePerson.Number, 10) != 'O')
        {
          ExitState = "ORGANIZATION_NBR_INVALID";

          var field = GetField(export.StartingSearchCsePerson, "number");

          field.Error = true;

          return;
        }
      }

      if (!IsEmpty(export.StartingSearchCsePerson.Number))
      {
        if (!IsEmpty(export.StartingSearchCsePerson.OrganizationName))
        {
          var field1 = GetField(export.StartingSearchCsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.StartingSearchCsePerson, "organizationName");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }

        if (!IsEmpty(export.StartingSearchCsePerson.TaxId))
        {
          var field1 = GetField(export.StartingSearchCsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.StartingSearchCsePerson, "taxId");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }
      }

      if (!IsEmpty(export.StartingSearchCsePerson.OrganizationName))
      {
        if (!IsEmpty(export.StartingSearchCsePerson.Number))
        {
          var field1 = GetField(export.StartingSearchCsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.StartingSearchCsePerson, "organizationName");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }

        if (!IsEmpty(export.StartingSearchCsePerson.TaxId))
        {
          var field1 =
            GetField(export.StartingSearchCsePerson, "organizationName");

          field1.Error = true;

          var field2 = GetField(export.StartingSearchCsePerson, "taxId");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }
      }

      if (!IsEmpty(export.StartingSearchCsePerson.TaxId))
      {
        if (!IsEmpty(export.StartingSearchCsePerson.Number))
        {
          var field1 = GetField(export.StartingSearchCsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.StartingSearchCsePerson, "taxId");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }

        if (!IsEmpty(export.StartingSearchCsePerson.OrganizationName))
        {
          var field1 =
            GetField(export.StartingSearchCsePerson, "organizationName");

          field1.Error = true;

          var field2 = GetField(export.StartingSearchCsePerson, "taxId");

          field2.Error = true;

          ExitState = "SI0000_ORGZ_INV_SORT_CRITERIA";

          return;
        }
      }

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      // ---------------------------------------------
      // Blank out the group views to eliminate the previous display's contents
      // ---------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar = "";
        export.Export1.Update.DetailCsePerson.Number = "";
        export.Export1.Update.DetailCsePerson.OrganizationName = "";
        export.Export1.Update.DetailCsePerson.TaxId = "";
        export.Export1.Update.DetailCsePerson.TaxIdSuffix = "";
        export.Export1.Update.DetailCsePerson.Type1 = "";
        export.Export1.Update.DetailCsePersonAddress.City = "";
        export.Export1.Update.DetailCsePersonAddress.LocationType = "";
        export.Export1.Update.DetailCsePersonAddress.ZipCode = "";
        export.Export1.Update.DetailCsePersonAddress.Zip4 = "";
        export.Export1.Update.DetailFips.State = 0;
        export.Export1.Update.DetailFips.County = 0;
        export.Export1.Update.DetailFips.Location = 0;
        export.Export1.Update.DetailFips.StateAbbreviation = "";
        export.Export1.Update.DetailFips.CountyAbbreviation = "";
        export.Export1.Update.DetailFips.LocationDescription =
          Spaces(Fips.LocationDescription_MaxLength);
        export.Export1.Update.DetailOrgzIsTrib.Flag = "";
        export.Export1.Update.DetailPromptFips.OneChar = "";

        // M Brown, pr# 106669, November 2000.
        export.Export1.Update.DisplayCounty.Text3 = "";
        export.Export1.Update.DisplayLocation.Text2 = "";
        export.Export1.Update.DisplayState.Text2 = "";
      }

      export.Export1.CheckIndex();
      UseSiReadOrganizations();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (IsEmpty(export.Export1.Item.DetailCsePerson.Number))
        {
          ExitState = "SI0000_NO_ORGZ_FOUND_FOR_DISP";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (export.Export1.Item.DetailFips.County == 0 && export
            .Export1.Item.DetailFips.Location == 0 && export
            .Export1.Item.DetailFips.State == 0)
          {
            export.Export1.Update.DisplayCounty.Text3 = "";
            export.Export1.Update.DisplayLocation.Text2 = "";
            export.Export1.Update.DisplayState.Text2 = "";
          }
          else
          {
            export.Export1.Update.DisplayLocation.Text2 =
              NumberToString(export.Export1.Item.DetailFips.Location, 2);
            export.Export1.Update.DisplayCounty.Text3 =
              NumberToString(export.Export1.Item.DetailFips.County, 3);
            export.Export1.Update.DisplayState.Text2 =
              NumberToString(export.Export1.Item.DetailFips.State, 2);
          }

          if (AsChar(export.Export1.Item.DetailOrgzIsTrib.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePerson, "organizationName");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.DetailCsePerson, "taxId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");

            field3.Color = "cyan";
            field3.Protected = true;

            // M Brown, pr# 106669, November 2000.
            var field4 = GetField(export.Export1.Item.DisplayState, "text2");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Export1.Item.DisplayCounty, "text3");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Export1.Item.DisplayLocation, "text2");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.DetailPromptFips, "oneChar");

            field7.Color = "cyan";
            field7.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePerson, "organizationName");
              

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;

            var field2 = GetField(export.Export1.Item.DetailCsePerson, "taxId");

            field2.Color = "green";
            field2.Highlighting = Highlighting.Underscore;
            field2.Protected = false;

            var field3 =
              GetField(export.Export1.Item.DetailCsePerson, "taxIdSuffix");

            field3.Color = "green";
            field3.Highlighting = Highlighting.Underscore;
            field3.Protected = false;

            // M Brown, pr# 106669, November 2000.
            var field4 = GetField(export.Export1.Item.DisplayState, "text2");

            field4.Color = "green";
            field4.Highlighting = Highlighting.Underscore;
            field4.Protected = false;

            var field5 = GetField(export.Export1.Item.DisplayCounty, "text3");

            field5.Color = "green";
            field5.Highlighting = Highlighting.Underscore;
            field5.Protected = false;

            var field6 = GetField(export.Export1.Item.DisplayLocation, "text2");

            field6.Color = "green";
            field6.Highlighting = Highlighting.Underscore;
            field6.Protected = false;

            var field7 =
              GetField(export.Export1.Item.DetailPromptFips, "oneChar");

            field7.Color = "green";
            field7.Highlighting = Highlighting.Underscore;
            field7.Protected = false;
          }
        }

        export.Export1.CheckIndex();
      }

      // -----------------
      // DISPLAY
      // -----------------
      export.HiddenPrev.PageNumber = export.HiddenStandard.PageNumber;

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKey.Assign(local.Next);
    }
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.TaxIdSuffix = source.TaxIdSuffix;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveExport1(SiReadOrganizations.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.DetailCsePersonAddress.Assign(source.DetailCsePersonAddress);
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCsePerson.Assign(source.DetailCsePerson);
    target.DetailFips.Assign(source.DetailFips);
    target.DetailPromptFips.OneChar = source.DetailPromptFips.OneChar;
    target.DetailOrgzIsTrib.Flag = source.DetailOrgzIsTrib.Flag;
  }

  private static void MoveFips1(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.County = source.County;
    target.Location = source.Location;
    target.LocationDescription = source.LocationDescription;
  }

  private static void MoveFips2(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.InCsePerson.Text10;
    useExport.TextWorkArea.Text10 = local.OutCsePerson.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.OutCsePerson.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePerson()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    MoveCsePerson1(local.CsePerson, useImport.CsePerson);
    useImport.Fips.Assign(export.Export1.Item.DetailFips);

    Call(SiCreateCsePerson.Execute, useImport, useExport);

    local.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseSiReadOrganizations()
  {
    var useImport = new SiReadOrganizations.Import();
    var useExport = new SiReadOrganizations.Export();

    MoveCsePerson2(export.SearchNamesLike, useImport.SearchNamesLike);
    useImport.SearchCsePersonAddress.Assign(export.StartingSearchCity);
    MoveFips2(export.StartingSearchFips, useImport.SearchFips);
    useImport.StartingSearch.Assign(export.StartingSearchCsePerson);
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    useImport.PageKey.Assign(export.HiddenPageKeys.Item.HiddenPageKey);

    Call(SiReadOrganizations.Execute, useImport, useExport);

    local.Next.Assign(useExport.Next);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson1(local.CsePerson, useImport.CsePerson);
    useImport.Fips.Assign(import.Import1.Item.DetailFips);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private bool ReadCsePerson1()
  {
    entities.Another.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetString(command, "numb", export.Export1.Item.DetailCsePerson.Number);
          
        db.SetNullableString(
          command, "taxId", export.Export1.Item.DetailCsePerson.TaxId ?? "");
        db.SetNullableString(
          command, "taxIdSuffix",
          export.Export1.Item.DetailCsePerson.TaxIdSuffix ?? "");
      },
      (db, reader) =>
      {
        entities.Another.Number = db.GetString(reader, 0);
        entities.Another.Type1 = db.GetString(reader, 1);
        entities.Another.TaxId = db.GetNullableString(reader, 2);
        entities.Another.OrganizationName = db.GetNullableString(reader, 3);
        entities.Another.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.Another.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Another.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Another.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "taxId", export.Export1.Item.DetailCsePerson.TaxId ?? "");
        db.SetNullableString(
          command, "taxIdSuffix",
          export.Export1.Item.DetailCsePerson.TaxIdSuffix ?? "");
      },
      (db, reader) =>
      {
        entities.Another.Number = db.GetString(reader, 0);
        entities.Another.Type1 = db.GetString(reader, 1);
        entities.Another.TaxId = db.GetNullableString(reader, 2);
        entities.Another.OrganizationName = db.GetNullableString(reader, 3);
        entities.Another.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.Another.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Another.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.Another.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Export1.Item.DetailFips.State);
        db.SetInt32(command, "county", export.Export1.Item.DetailFips.County);
        db.
          SetInt32(command, "location", export.Export1.Item.DetailFips.Location);
          
        db.
          SetString(command, "numb", export.Export1.Item.DetailCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.Another.Number = db.GetString(reader, 0);
        entities.Another.Type1 = db.GetString(reader, 1);
        entities.Another.TaxId = db.GetNullableString(reader, 2);
        entities.Another.OrganizationName = db.GetNullableString(reader, 3);
        entities.Another.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.Another.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Another.Type1);
      });
  }

  private bool ReadCsePerson4()
  {
    entities.Another.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Export1.Item.DetailFips.State);
        db.SetInt32(command, "county", export.Export1.Item.DetailFips.County);
        db.
          SetInt32(command, "location", export.Export1.Item.DetailFips.Location);
          
      },
      (db, reader) =>
      {
        entities.Another.Number = db.GetString(reader, 0);
        entities.Another.Type1 = db.GetString(reader, 1);
        entities.Another.TaxId = db.GetNullableString(reader, 2);
        entities.Another.OrganizationName = db.GetNullableString(reader, 3);
        entities.Another.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.Another.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Another.Type1);
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fipState", export.Export1.Item.DetailFips.State);
        db.SetNullableInt32(
          command, "fipCounty", export.Export1.Item.DetailFips.County);
        db.SetNullableInt32(
          command, "fipLocation", export.Export1.Item.DetailFips.Location);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private void UpdateTribunal()
  {
    var name =
      Substring(export.Export1.Item.DetailCsePerson.OrganizationName, 1, 30);
    var taxIdSuffix = export.Export1.Item.DetailCsePerson.TaxIdSuffix ?? "";
    var taxId = export.Export1.Item.DetailCsePerson.TaxId ?? "";

    entities.Tribunal.Populated = false;
    Update("UpdateTribunal",
      (db, command) =>
      {
        db.SetString(command, "tribunalNm", name);
        db.SetNullableString(command, "taxIdSuffix", taxIdSuffix);
        db.SetNullableString(command, "taxId", taxId);
        db.SetInt32(command, "identifier", entities.Tribunal.Identifier);
      });

    entities.Tribunal.Name = name;
    entities.Tribunal.TaxIdSuffix = taxIdSuffix;
    entities.Tribunal.TaxId = taxId;
    entities.Tribunal.Populated = true;
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
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
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

      /// <summary>
      /// A value of DisplayState.
      /// </summary>
      [JsonPropertyName("displayState")]
      public WorkArea DisplayState
      {
        get => displayState ??= new();
        set => displayState = value;
      }

      /// <summary>
      /// A value of DisplayCounty.
      /// </summary>
      [JsonPropertyName("displayCounty")]
      public WorkArea DisplayCounty
      {
        get => displayCounty ??= new();
        set => displayCounty = value;
      }

      /// <summary>
      /// A value of DisplayLocation.
      /// </summary>
      [JsonPropertyName("displayLocation")]
      public WorkArea DisplayLocation
      {
        get => displayLocation ??= new();
        set => displayLocation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private Fips detailFips;
      private Standard detailPromptFips;
      private CsePersonAddress detailCsePersonAddress;
      private Common detailOrgzIsTrib;
      private WorkArea displayState;
      private WorkArea displayCounty;
      private WorkArea displayLocation;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePerson HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hiddenPageKey;
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
    /// A value of PromptStateCode.
    /// </summary>
    [JsonPropertyName("promptStateCode")]
    public Standard PromptStateCode
    {
      get => promptStateCode ??= new();
      set => promptStateCode = value;
    }

    /// <summary>
    /// A value of StartingSearchCity.
    /// </summary>
    [JsonPropertyName("startingSearchCity")]
    public CsePersonAddress StartingSearchCity
    {
      get => startingSearchCity ??= new();
      set => startingSearchCity = value;
    }

    /// <summary>
    /// A value of StartingSearchFips.
    /// </summary>
    [JsonPropertyName("startingSearchFips")]
    public Fips StartingSearchFips
    {
      get => startingSearchFips ??= new();
      set => startingSearchFips = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedFips.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFips")]
    public Fips DlgflwSelectedFips
    {
      get => dlgflwSelectedFips ??= new();
      set => dlgflwSelectedFips = value;
    }

    /// <summary>
    /// A value of FromDdmm.
    /// </summary>
    [JsonPropertyName("fromDdmm")]
    public Common FromDdmm
    {
      get => fromDdmm ??= new();
      set => fromDdmm = value;
    }

    /// <summary>
    /// A value of StartingSearchCsePerson.
    /// </summary>
    [JsonPropertyName("startingSearchCsePerson")]
    public CsePerson StartingSearchCsePerson
    {
      get => startingSearchCsePerson ??= new();
      set => startingSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Standard HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCodeValue.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCodeValue")]
    public CodeValue DlgflwSelectedCodeValue
    {
      get => dlgflwSelectedCodeValue ??= new();
      set => dlgflwSelectedCodeValue = value;
    }

    private CsePerson searchNamesLike;
    private Standard promptStateCode;
    private CsePersonAddress startingSearchCity;
    private Fips startingSearchFips;
    private Fips dlgflwSelectedFips;
    private Common fromDdmm;
    private CsePerson startingSearchCsePerson;
    private Standard hiddenPrev;
    private Case1 next;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue dlgflwSelectedCodeValue;
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

      /// <summary>
      /// A value of DisplayState.
      /// </summary>
      [JsonPropertyName("displayState")]
      public WorkArea DisplayState
      {
        get => displayState ??= new();
        set => displayState = value;
      }

      /// <summary>
      /// A value of DisplayCounty.
      /// </summary>
      [JsonPropertyName("displayCounty")]
      public WorkArea DisplayCounty
      {
        get => displayCounty ??= new();
        set => displayCounty = value;
      }

      /// <summary>
      /// A value of DisplayLocation.
      /// </summary>
      [JsonPropertyName("displayLocation")]
      public WorkArea DisplayLocation
      {
        get => displayLocation ??= new();
        set => displayLocation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private CsePersonAddress detailCsePersonAddress;
      private Common detailCommon;
      private CsePerson detailCsePerson;
      private Fips detailFips;
      private Standard detailPromptFips;
      private Common detailOrgzIsTrib;
      private WorkArea displayState;
      private WorkArea displayCounty;
      private WorkArea displayLocation;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePerson HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hiddenPageKey;
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
    /// A value of PromptStateCode.
    /// </summary>
    [JsonPropertyName("promptStateCode")]
    public Standard PromptStateCode
    {
      get => promptStateCode ??= new();
      set => promptStateCode = value;
    }

    /// <summary>
    /// A value of StartingSearchCity.
    /// </summary>
    [JsonPropertyName("startingSearchCity")]
    public CsePersonAddress StartingSearchCity
    {
      get => startingSearchCity ??= new();
      set => startingSearchCity = value;
    }

    /// <summary>
    /// A value of StartingSearchFips.
    /// </summary>
    [JsonPropertyName("startingSearchFips")]
    public Fips StartingSearchFips
    {
      get => startingSearchFips ??= new();
      set => startingSearchFips = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredFips.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredFips")]
    public Fips DlgflwRequiredFips
    {
      get => dlgflwRequiredFips ??= new();
      set => dlgflwRequiredFips = value;
    }

    /// <summary>
    /// A value of FromDdmm.
    /// </summary>
    [JsonPropertyName("fromDdmm")]
    public Common FromDdmm
    {
      get => fromDdmm ??= new();
      set => fromDdmm = value;
    }

    /// <summary>
    /// A value of StartingSearchCsePerson.
    /// </summary>
    [JsonPropertyName("startingSearchCsePerson")]
    public CsePerson StartingSearchCsePerson
    {
      get => startingSearchCsePerson ??= new();
      set => startingSearchCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Standard HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public CsePersonsWorkSet HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredCode.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredCode")]
    public Code DlgflwRequiredCode
    {
      get => dlgflwRequiredCode ??= new();
      set => dlgflwRequiredCode = value;
    }

    private CsePerson searchNamesLike;
    private Standard promptStateCode;
    private CsePersonAddress startingSearchCity;
    private Fips startingSearchFips;
    private Fips dlgflwRequiredFips;
    private Common fromDdmm;
    private CsePerson startingSearchCsePerson;
    private Standard hiddenPrev;
    private CsePersonsWorkSet hiddenSelected;
    private Case1 next;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Code dlgflwRequiredCode;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of OutCsePerson.
    /// </summary>
    [JsonPropertyName("outCsePerson")]
    public TextWorkArea OutCsePerson
    {
      get => outCsePerson ??= new();
      set => outCsePerson = value;
    }

    /// <summary>
    /// A value of InCsePerson.
    /// </summary>
    [JsonPropertyName("inCsePerson")]
    public TextWorkArea InCsePerson
    {
      get => inCsePerson ??= new();
      set => inCsePerson = value;
    }

    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Tribunal tribunal;
    private TextWorkArea outCsePerson;
    private TextWorkArea inCsePerson;
    private Common valid;
    private Common common;
    private CsePerson csePerson;
    private CsePerson next;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Another.
    /// </summary>
    [JsonPropertyName("another")]
    public CsePerson Another
    {
      get => another ??= new();
      set => another = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsePerson Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private CsePerson another;
    private Tribunal tribunal;
    private Fips fips;
    private CsePerson zdel;
  }
#endregion
}
