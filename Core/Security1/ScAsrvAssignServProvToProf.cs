// Program: SC_ASRV_ASSIGN_SERV_PROV_TO_PROF, ID: 371452323, model: 746.
// Short name: SWEASRVP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: SC_ASRV_ASSIGN_SERV_PROV_TO_PROF.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScAsrvAssignServProvToProf: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_ASRV_ASSIGN_SERV_PROV_TO_PROF program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScAsrvAssignServProvToProf(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScAsrvAssignServProvToProf.
  /// </summary>
  public ScAsrvAssignServProvToProf(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      request #   Description
    // 12/10/95 Alan Hackler                Initial Development
    // 12/12/96 R. Marchman                 Add new security/next tran
    // ---------------------------------------------
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. Fixed 
    // the IF loop.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.StartingWith.Text12 = import.StartingWith.Text12;
    MoveProfile(import.Profile, export.Profile);
    export.HiddenPrev.Name = import.HiddenPrev.Name;
    export.PromptProfile.PromptField = import.PromptProfile.PromptField;
    export.SortBy.SelectChar = import.SortBy.SelectChar;
    export.ActiveOnly.SelectChar = import.ActiveOnly.SelectChar;
    export.HiddenFromSecurity.SelectChar = "M";
    local.CurrentMinus1Day.EffectiveDate = Now().Date.AddDays(-1);

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      local.Selected.Index = -1;

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.ServiceProviderProfile.Assign(
          import.Group.Item.ServiceProviderProfile);
        export.Group.Update.ServiceProvider.Assign(
          import.Group.Item.ServiceProvider);
        export.Group.Update.Error.Name = export.Group.Item.Error.Name;
        MoveServiceProviderProfile(import.Group.Item.HiddenPrev,
          export.Group.Update.HiddenPrev);

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;
            export.HiddenSelected.UserId =
              export.Group.Item.ServiceProvider.UserId;

            // STORE SELECTED PROFILES FOR LATER USE.
            ++local.Selected.Index;
            local.Selected.CheckSize();

            local.Selected.Update.SelectedServiceProvider.UserId =
              export.Group.Item.ServiceProvider.UserId;
            local.Selected.Update.SelectedServiceProviderProfile.Assign(
              export.Group.Item.ServiceProviderProfile);

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
          Now().Date))
        {
          var field =
            GetField(export.Group.Item.ServiceProviderProfile, "effectiveDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }

        if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
          Now().Date))
        {
          var field =
            GetField(export.Group.Item.ServiceProviderProfile, "discontinueDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }
      }

      import.Group.CheckIndex();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "RETFMPRO"))
    {
      // this is when you come back from selecting a profile
      export.PromptProfile.PromptField = "";

      if (IsEmpty(import.HiddenSelected.Name))
      {
        // nothing was returned from the profile list
      }
      else
      {
        export.PromptProfile.PromptField = "";
        MoveProfile(import.HiddenSelected, export.Profile);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETFMSP"))
    {
      // this is when you come back from selecting service providers
      if (!import.HiddenSelectedSp.IsEmpty)
      {
        export.Group.Index = export.Group.Count - 1;
        export.Group.CheckSize();

        for(import.HiddenSelectedSp.Index = 0; import.HiddenSelectedSp.Index < import
          .HiddenSelectedSp.Count; ++import.HiddenSelectedSp.Index)
        {
          if (!import.HiddenSelectedSp.CheckSize())
          {
            break;
          }

          if (export.Group.IsFull)
          {
            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.ServiceProvider.Assign(
            import.HiddenSelectedSp.Item.HiddenSelected);
          export.Group.Update.Common.SelectChar = "S";
        }

        import.HiddenSelectedSp.CheckIndex();
      }

      return;
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (Equal(import.Profile.Name, import.HiddenPrev.Name))
      {
      }
      else
      {
        var field = GetField(export.Profile, "name");

        field.Error = true;

        ExitState = "SC0019_KEY_CHANGED_REDSPLAY";

        return;
      }
    }

    if (Equal(global.Command, "ADD"))
    {
      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      // insure that dates are entered.
      export.Group.Index = 0;

      for(var limit = export.Group.Count; export.Group.Index < limit; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (Equal(export.Group.Item.ServiceProviderProfile.EffectiveDate, null))
            
          {
            export.Group.Update.ServiceProviderProfile.EffectiveDate =
              Now().Date;
          }

          if (Equal(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
            null))
          {
            UseCabSetMaximumDiscontinueDate();
            export.Group.Update.ServiceProviderProfile.DiscontinueDate =
              local.DateWorkArea.Date;
          }
        }
      }

      export.Group.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that dates are in a valid date range.
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (!Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
            export.Group.Item.ServiceProviderProfile.EffectiveDate))
          {
          }
          else
          {
            var field1 =
              GetField(export.Group.Item.ServiceProviderProfile, "effectiveDate");
              

            field1.Error = true;

            var field2 =
              GetField(export.Group.Item.ServiceProviderProfile,
              "discontinueDate");

            field2.Error = true;

            ExitState = "SC0021_INVALID_DATE_RANGE";
          }

          // CANNOT SET UP DATES MORE THAN 1 DAY IN THE PAST
          if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
            local.CurrentMinus1Day.EffectiveDate))
          {
            var field =
              GetField(export.Group.Item.ServiceProviderProfile, "effectiveDate");
              

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SC0040_DATE_MORE_THAN_1_DAY_BACK";
            }
          }

          if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
            local.CurrentMinus1Day.EffectiveDate))
          {
            var field =
              GetField(export.Group.Item.ServiceProviderProfile,
              "discontinueDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SC0040_DATE_MORE_THAN_1_DAY_BACK";
            }
          }
        }
      }

      export.Group.CheckIndex();

      if (Equal(global.Command, "UPDATE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              local.CurrentMinus1Day.EffectiveDate))
            {
              var field =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "SC0040_DATE_MORE_THAN_1_DAY_BACK";
              }
            }
          }
        }

        export.Group.CheckIndex();
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that the user is not currently active in another profile for the
      // requested dates.
      // *** THIS WILL BE FOR RECORDS CURRENTLY OUT ON THE DATA BASE....
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (ReadServiceProvider())
          {
            export.Group.Update.ServiceProvider.Assign(
              entities.ExistingServiceProvider);
          }
          else
          {
            var field = GetField(export.Group.Item.ServiceProvider, "userId");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;

            ExitState = "SC0005_USER_NF";

            break;
          }

          foreach(var item in ReadServiceProviderProfile())
          {
            if (Equal(entities.ExistingServiceProviderProfile.CreatedTimestamp,
              export.Group.Item.ServiceProviderProfile.CreatedTimestamp))
            {
              continue;
            }

            if (Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PRIOR TO THE BEGINNING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              entities.ExistingServiceProviderProfile.EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PAST TO THE ENDING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            local.Error.Flag = "N";

            if (!Lt(entities.ExistingServiceProviderProfile.EffectiveDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate) && !
              Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              entities.ExistingServiceProviderProfile.EffectiveDate))
            {
              local.Error.Flag = "Y";
            }

            if (!Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate) && !
              Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              entities.ExistingServiceProviderProfile.DiscontinueDate))
            {
              local.Error.Flag = "Y";
            }

            export.Group.Update.Error.Name = "";

            if (AsChar(local.Error.Flag) == 'Y')
            {
              if (ReadProfile2())
              {
                export.Group.Update.Error.Name = entities.ExistingProfile.Name;
              }

              var field1 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field2.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }
          }
        }
      }

      export.Group.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that the user is not currently active in another profile for the
      // requested dates.
      // *** THIS WILL BE FOR RECORDS CURRENTLY SELECTED IN THE IMPORT VIEW....
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          for(local.Selected.Index = 0; local.Selected.Index < local
            .Selected.Count; ++local.Selected.Index)
          {
            if (!local.Selected.CheckSize())
            {
              break;
            }

            if (Equal(export.Group.Item.ServiceProvider.UserId,
              local.Selected.Item.SelectedServiceProvider.UserId))
            {
            }
            else
            {
              continue;
            }

            if (Equal(export.Group.Item.ServiceProviderProfile.CreatedTimestamp,
              local.Selected.Item.SelectedServiceProviderProfile.
                CreatedTimestamp))
            {
              continue;
            }

            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PRIOR TO THE BEGINNING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Lt(local.Selected.Item.SelectedServiceProviderProfile.
              DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PAST TO THE ENDING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            local.Error.Flag = "N";

            if (!Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate) && !
              Lt(local.Selected.Item.SelectedServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              local.Error.Flag = "Y";
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate) && !
              Lt(local.Selected.Item.SelectedServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.DiscontinueDate))
            {
              local.Error.Flag = "Y";
            }

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field2.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }
          }

          local.Selected.CheckIndex();
        }
      }

      export.Group.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The above security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. 
    // Fixed the IF loop.
    // ---------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();
    }
    else
    {
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ASPU":
        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

          return;
        }

        export.HiddenCommon.SelectChar = "N";
        ExitState = "ECO_XFR_TO_ASSGN_USR_2_PROF_USR";

        break;
      case "DISPLAY":
        export.PromptProfile.PromptField = "";

        if (ReadProfile1())
        {
          MoveProfile(entities.ExistingProfile, export.Profile);
          export.HiddenPrev.Name = entities.ExistingProfile.Name;
        }
        else
        {
          var field = GetField(export.Profile, "name");

          field.Error = true;

          ExitState = "SC0015_PROFILE_NF";

          return;
        }

        if (IsEmpty(export.SortBy.SelectChar))
        {
          export.SortBy.SelectChar = "U";
        }

        if (AsChar(export.ActiveOnly.SelectChar) == 'Y' || AsChar
          (import.ActiveOnly.SelectChar) == 'N')
        {
        }
        else
        {
          export.ActiveOnly.SelectChar = "Y";
        }

        export.Group.Index = -1;

        switch(AsChar(export.SortBy.SelectChar))
        {
          case 'U':
            foreach(var item in ReadServiceProviderProfileServiceProvider2())
            {
              if (AsChar(export.ActiveOnly.SelectChar) == 'Y')
              {
                if (!Lt(Now().Date,
                  entities.ExistingServiceProviderProfile.EffectiveDate) && !
                  Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                }
                else
                {
                  continue;
                }
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ServiceProvider.Assign(
                entities.ExistingServiceProvider);
              export.Group.Update.ServiceProviderProfile.Assign(
                entities.ExistingServiceProviderProfile);
              MoveServiceProviderProfile(entities.
                ExistingServiceProviderProfile, export.Group.Update.HiddenPrev);
                
              export.Group.Update.Error.Name = "";

              if (Lt(entities.ExistingServiceProviderProfile.EffectiveDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

                break;
              }
            }

            break;
          case 'L':
            foreach(var item in ReadServiceProviderProfileServiceProvider1())
            {
              if (AsChar(export.ActiveOnly.SelectChar) == 'Y')
              {
                if (!Lt(Now().Date,
                  entities.ExistingServiceProviderProfile.EffectiveDate) && !
                  Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                }
                else
                {
                  continue;
                }
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ServiceProvider.Assign(
                entities.ExistingServiceProvider);
              export.Group.Update.ServiceProviderProfile.Assign(
                entities.ExistingServiceProviderProfile);
              MoveServiceProviderProfile(entities.
                ExistingServiceProviderProfile, export.Group.Update.HiddenPrev);
                
              export.Group.Update.Error.Name = "";

              if (Lt(entities.ExistingServiceProviderProfile.EffectiveDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

                break;
              }
            }

            break;
          case 'E':
            foreach(var item in ReadServiceProviderProfileServiceProvider4())
            {
              if (AsChar(export.ActiveOnly.SelectChar) == 'Y')
              {
                if (!Lt(Now().Date,
                  entities.ExistingServiceProviderProfile.EffectiveDate) && !
                  Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                }
                else
                {
                  continue;
                }
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ServiceProvider.Assign(
                entities.ExistingServiceProvider);
              export.Group.Update.ServiceProviderProfile.Assign(
                entities.ExistingServiceProviderProfile);
              MoveServiceProviderProfile(entities.
                ExistingServiceProviderProfile, export.Group.Update.HiddenPrev);
                
              export.Group.Update.Error.Name = "";

              if (Lt(entities.ExistingServiceProviderProfile.EffectiveDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }
            }

            break;
          case 'D':
            foreach(var item in ReadServiceProviderProfileServiceProvider3())
            {
              if (AsChar(export.ActiveOnly.SelectChar) == 'Y')
              {
                if (!Lt(Now().Date,
                  entities.ExistingServiceProviderProfile.EffectiveDate) && !
                  Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                }
                else
                {
                  continue;
                }
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.ServiceProvider.Assign(
                entities.ExistingServiceProvider);
              export.Group.Update.ServiceProviderProfile.Assign(
                entities.ExistingServiceProviderProfile);
              MoveServiceProviderProfile(entities.
                ExistingServiceProviderProfile, export.Group.Update.HiddenPrev);
                
              export.Group.Update.Error.Name = "";

              if (Lt(entities.ExistingServiceProviderProfile.EffectiveDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }

              if (Lt(entities.ExistingServiceProviderProfile.DiscontinueDate,
                Now().Date))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field1.Color = "cyan";
                field1.Protected = true;
              }
            }

            break;
          default:
            var field = GetField(export.SortBy, "selectChar");

            field.Error = true;

            ExitState = "SC0024_MUST_BE_U_L_E_D";

            return;
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "LSRVP":
        ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

        break;
      case "LIST":
        switch(AsChar(import.PromptProfile.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_PROFILES";

            break;
          case ' ':
            var field1 = GetField(export.PromptProfile, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          default:
            var field2 = GetField(export.PromptProfile, "promptField");

            field2.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        break;
      case "ADD":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseCreateServProvProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";

                if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "effectiveDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }

                if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }
              }
              else if (IsExitState("SC0015_PROFILE_NF"))
              {
                var field = GetField(export.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_AE"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NU"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseUpdateServProvProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";

                if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "effectiveDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }

                if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }
              }
              else if (IsExitState("SC0015_PROFILE_NF"))
              {
                var field = GetField(export.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_AE"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NU"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseDeleteServProvProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else if (IsExitState("SC0015_PROFILE_NF"))
              {
                var field = GetField(export.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_AE"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NU"))
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "effectiveDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.ServiceProviderProfile,
                  "discontinueDate");

                field3.Error = true;
              }
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
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

  private static void MoveProfile(Profile source, Profile target)
  {
    target.Name = source.Name;
    target.Desc = source.Desc;
  }

  private static void MoveServiceProviderProfile(ServiceProviderProfile source,
    ServiceProviderProfile target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseCreateServProvProfile()
  {
    var useImport = new CreateServProvProfile.Import();
    var useExport = new CreateServProvProfile.Export();

    useImport.Profile.Name = export.Profile.Name;
    useImport.ServiceProvider.UserId = export.Group.Item.ServiceProvider.UserId;
    MoveServiceProviderProfile(export.Group.Item.ServiceProviderProfile,
      useImport.ServiceProviderProfile);

    Call(CreateServProvProfile.Execute, useImport, useExport);

    export.Group.Update.ServiceProviderProfile.CreatedTimestamp =
      useExport.ServiceProviderProfile.CreatedTimestamp;
  }

  private void UseDeleteServProvProfile()
  {
    var useImport = new DeleteServProvProfile.Import();
    var useExport = new DeleteServProvProfile.Export();

    useImport.Profile.Name = export.Profile.Name;
    useImport.ServiceProvider.UserId = export.Group.Item.ServiceProvider.UserId;
    useImport.ServiceProviderProfile.CreatedTimestamp =
      export.Group.Item.ServiceProviderProfile.CreatedTimestamp;

    Call(DeleteServProvProfile.Execute, useImport, useExport);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseUpdateServProvProfile()
  {
    var useImport = new UpdateServProvProfile.Import();
    var useExport = new UpdateServProvProfile.Export();

    useImport.Profile.Name = export.Profile.Name;
    useImport.ServiceProvider.UserId = export.Group.Item.ServiceProvider.UserId;
    useImport.ServiceProviderProfile.Assign(
      export.Group.Item.ServiceProviderProfile);

    Call(UpdateServProvProfile.Execute, useImport, useExport);
  }

  private bool ReadProfile1()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile1",
      (db, command) =>
      {
        db.SetString(command, "name", export.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadProfile2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingServiceProviderProfile.Populated);
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile2",
      (db, command) =>
      {
        db.SetString(
          command, "name", entities.ExistingServiceProviderProfile.ProName);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "userId", export.Group.Item.ServiceProvider.UserId);
          
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfile()
  {
    entities.ExistingServiceProviderProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfile",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGenId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProviderProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingServiceProviderProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "proName", entities.ExistingProfile.Name);
        db.SetString(command, "text12", export.StartingWith.Text12);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 5);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 6);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 8);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingServiceProviderProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingServiceProviderProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "proName", entities.ExistingProfile.Name);
        db.SetString(command, "text12", export.StartingWith.Text12);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 5);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 6);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 8);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingServiceProviderProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileServiceProvider3()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingServiceProviderProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileServiceProvider3",
      (db, command) =>
      {
        db.SetString(command, "proName", entities.ExistingProfile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 5);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 6);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 8);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingServiceProviderProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileServiceProvider4()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingServiceProviderProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileServiceProvider4",
      (db, command) =>
      {
        db.SetString(command, "proName", entities.ExistingProfile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 5);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 6);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 8);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingServiceProviderProfile.Populated = true;

        return true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ServiceProviderProfile.
      /// </summary>
      [JsonPropertyName("serviceProviderProfile")]
      public ServiceProviderProfile ServiceProviderProfile
      {
        get => serviceProviderProfile ??= new();
        set => serviceProviderProfile = value;
      }

      /// <summary>
      /// A value of Error.
      /// </summary>
      [JsonPropertyName("error")]
      public Profile Error
      {
        get => error ??= new();
        set => error = value;
      }

      /// <summary>
      /// A value of HiddenPrev.
      /// </summary>
      [JsonPropertyName("hiddenPrev")]
      public ServiceProviderProfile HiddenPrev
      {
        get => hiddenPrev ??= new();
        set => hiddenPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 145;

      private Common common;
      private ServiceProvider serviceProvider;
      private ServiceProviderProfile serviceProviderProfile;
      private Profile error;
      private ServiceProviderProfile hiddenPrev;
    }

    /// <summary>A HiddenSelectedSpGroup group.</summary>
    [Serializable]
    public class HiddenSelectedSpGroup
    {
      /// <summary>
      /// A value of HiddenSelected.
      /// </summary>
      [JsonPropertyName("hiddenSelected")]
      public ServiceProvider HiddenSelected
      {
        get => hiddenSelected ??= new();
        set => hiddenSelected = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 145;

      private ServiceProvider hiddenSelected;
    }

    /// <summary>
    /// A value of StartingWith.
    /// </summary>
    [JsonPropertyName("startingWith")]
    public WorkArea StartingWith
    {
      get => startingWith ??= new();
      set => startingWith = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Profile HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Profile HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of PromptProfile.
    /// </summary>
    [JsonPropertyName("promptProfile")]
    public Standard PromptProfile
    {
      get => promptProfile ??= new();
      set => promptProfile = value;
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
    /// Gets a value of HiddenSelectedSp.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelectedSpGroup> HiddenSelectedSp =>
      hiddenSelectedSp ??= new(HiddenSelectedSpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelectedSp for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelectedSp")]
    [Computed]
    public IList<HiddenSelectedSpGroup> HiddenSelectedSp_Json
    {
      get => hiddenSelectedSp;
      set => HiddenSelectedSp.Assign(value);
    }

    /// <summary>
    /// A value of ActiveOnly.
    /// </summary>
    [JsonPropertyName("activeOnly")]
    public Common ActiveOnly
    {
      get => activeOnly ??= new();
      set => activeOnly = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private WorkArea startingWith;
    private Common sortBy;
    private Profile hiddenPrev;
    private Profile profile;
    private Profile hiddenSelected;
    private Standard promptProfile;
    private Array<GroupGroup> group;
    private Array<HiddenSelectedSpGroup> hiddenSelectedSp;
    private Common activeOnly;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ServiceProviderProfile.
      /// </summary>
      [JsonPropertyName("serviceProviderProfile")]
      public ServiceProviderProfile ServiceProviderProfile
      {
        get => serviceProviderProfile ??= new();
        set => serviceProviderProfile = value;
      }

      /// <summary>
      /// A value of Error.
      /// </summary>
      [JsonPropertyName("error")]
      public Profile Error
      {
        get => error ??= new();
        set => error = value;
      }

      /// <summary>
      /// A value of HiddenPrev.
      /// </summary>
      [JsonPropertyName("hiddenPrev")]
      public ServiceProviderProfile HiddenPrev
      {
        get => hiddenPrev ??= new();
        set => hiddenPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 145;

      private Common common;
      private ServiceProvider serviceProvider;
      private ServiceProviderProfile serviceProviderProfile;
      private Profile error;
      private ServiceProviderProfile hiddenPrev;
    }

    /// <summary>
    /// A value of HiddenCommon.
    /// </summary>
    [JsonPropertyName("hiddenCommon")]
    public Common HiddenCommon
    {
      get => hiddenCommon ??= new();
      set => hiddenCommon = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public ServiceProvider HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of StartingWith.
    /// </summary>
    [JsonPropertyName("startingWith")]
    public WorkArea StartingWith
    {
      get => startingWith ??= new();
      set => startingWith = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Profile HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of PromptProfile.
    /// </summary>
    [JsonPropertyName("promptProfile")]
    public Standard PromptProfile
    {
      get => promptProfile ??= new();
      set => promptProfile = value;
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
    /// A value of ActiveOnly.
    /// </summary>
    [JsonPropertyName("activeOnly")]
    public Common ActiveOnly
    {
      get => activeOnly ??= new();
      set => activeOnly = value;
    }

    /// <summary>
    /// A value of HiddenFromSecurity.
    /// </summary>
    [JsonPropertyName("hiddenFromSecurity")]
    public Common HiddenFromSecurity
    {
      get => hiddenFromSecurity ??= new();
      set => hiddenFromSecurity = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common hiddenCommon;
    private ServiceProvider hiddenSelected;
    private WorkArea startingWith;
    private Common sortBy;
    private Profile hiddenPrev;
    private Profile profile;
    private Standard promptProfile;
    private Array<GroupGroup> group;
    private Common activeOnly;
    private Common hiddenFromSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SelectedGroup group.</summary>
    [Serializable]
    public class SelectedGroup
    {
      /// <summary>
      /// A value of SelectedServiceProvider.
      /// </summary>
      [JsonPropertyName("selectedServiceProvider")]
      public ServiceProvider SelectedServiceProvider
      {
        get => selectedServiceProvider ??= new();
        set => selectedServiceProvider = value;
      }

      /// <summary>
      /// A value of SelectedProfile.
      /// </summary>
      [JsonPropertyName("selectedProfile")]
      public Profile SelectedProfile
      {
        get => selectedProfile ??= new();
        set => selectedProfile = value;
      }

      /// <summary>
      /// A value of SelectedServiceProviderProfile.
      /// </summary>
      [JsonPropertyName("selectedServiceProviderProfile")]
      public ServiceProviderProfile SelectedServiceProviderProfile
      {
        get => selectedServiceProviderProfile ??= new();
        set => selectedServiceProviderProfile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ServiceProvider selectedServiceProvider;
      private Profile selectedProfile;
      private ServiceProviderProfile selectedServiceProviderProfile;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of CurrentMinus1Day.
    /// </summary>
    [JsonPropertyName("currentMinus1Day")]
    public ServiceProviderProfile CurrentMinus1Day
    {
      get => currentMinus1Day ??= new();
      set => currentMinus1Day = value;
    }

    /// <summary>
    /// Gets a value of Selected.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedGroup> Selected => selected ??= new(
      SelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Selected for json serialization.
    /// </summary>
    [JsonPropertyName("selected")]
    [Computed]
    public IList<SelectedGroup> Selected_Json
    {
      get => selected;
      set => Selected.Assign(value);
    }

    private Common error;
    private DateWorkArea dateWorkArea;
    private Common common;
    private ServiceProviderProfile currentMinus1Day;
    private Array<SelectedGroup> selected;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
    }

    /// <summary>
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
    }

    private ServiceProvider existingServiceProvider;
    private ServiceProviderProfile existingServiceProviderProfile;
    private Profile existingProfile;
  }
#endregion
}
