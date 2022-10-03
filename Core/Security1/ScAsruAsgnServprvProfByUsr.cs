// Program: SC_ASRU_ASGN_SERVPRV_PROF_BY_USR, ID: 371452050, model: 746.
// Short name: SWEASRUP
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
/// A program: SC_ASRU_ASGN_SERVPRV_PROF_BY_USR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScAsruAsgnServprvProfByUsr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_ASRU_ASGN_SERVPRV_PROF_BY_USR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScAsruAsgnServprvProfByUsr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScAsruAsgnServprvProfByUsr.
  /// </summary>
  public ScAsruAsgnServprvProfByUsr(IContext context, Import import,
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
    // 01/30/97 R. Marchman		     Fix Effective Date error.
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

    export.ActiveOnly.SelectChar = import.ActiveOnly.SelectChar;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.HiddenPrev.UserId = import.HiddenPrev.UserId;
    export.PromptServProvider.PromptField =
      import.PromptServProvider.PromptField;
    export.SortBy.SelectChar = import.SortBy.SelectChar;
    export.HiddenSelected.Name = import.HiddenSelectedProfile.Name;
    export.HiddenFromSecurity.SelectChar = "1";
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
        export.Group.Update.Profile.Name = import.Group.Item.Profile.Name;
        export.Group.Update.ServiceProviderProfile.Assign(
          import.Group.Item.ServiceProviderProfile);
        MoveServiceProviderProfile(import.Group.Item.HiddenPrev,
          export.Group.Update.HiddenPrev);

        var field = GetField(export.Group.Item.Profile, "name");

        field.Color = "cyan";
        field.Protected = true;

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;
            export.HiddenSelected.Name = export.Group.Item.Profile.Name;

            // STORE SELECTED PROFILES FOR LATER USE.
            ++local.Selected.Index;
            local.Selected.CheckSize();

            local.Selected.Update.SelectedProfile.Name =
              export.Group.Item.Profile.Name;
            local.Selected.Update.SelectedServiceProviderProfile.Assign(
              export.Group.Item.ServiceProviderProfile);

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
          Now().Date))
        {
          var field1 =
            GetField(export.Group.Item.ServiceProviderProfile, "effectiveDate");
            

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
          Now().Date))
        {
          var field1 =
            GetField(export.Group.Item.ServiceProviderProfile, "discontinueDate");
            

          field1.Color = "cyan";
          field1.Protected = true;
        }
      }

      import.Group.CheckIndex();

      for(import.HiddenHistory.Index = 0; import.HiddenHistory.Index < import
        .HiddenHistory.Count; ++import.HiddenHistory.Index)
      {
        if (!import.HiddenHistory.CheckSize())
        {
          break;
        }

        export.HiddenHistory.Index = import.HiddenHistory.Index;
        export.HiddenHistory.CheckSize();

        export.HiddenHistory.Update.HiddenHistoryProfile.Name =
          import.HiddenHistory.Item.HiddenHistoryProfile.Name;
        export.HiddenHistory.Update.HiddenHistoryServiceProviderProfile.Assign(
          import.HiddenHistory.Item.HiddenHistoryServiceProviderProfile);
      }

      import.HiddenHistory.CheckIndex();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "RETFMSP"))
    {
      // this is when you come back from selecting A service provider
      if (IsEmpty(import.HiddenSelectedServiceProvider.UserId))
      {
      }
      else
      {
        export.PromptServProvider.PromptField = "";
        export.ServiceProvider.UserId =
          import.HiddenSelectedServiceProvider.UserId;
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "RETFMPRO"))
    {
      // this is when you come back from selecting A profile
      if (IsEmpty(import.HiddenSelectedProfile.Name))
      {
      }
      else
      {
        export.SortBy.SelectChar = "P";

        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.Profile.Name = import.HiddenSelectedProfile.Name;

        return;
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (Equal(import.ServiceProvider.UserId, import.HiddenPrev.UserId))
      {
      }
      else
      {
        var field = GetField(export.ServiceProvider, "userId");

        field.Error = true;

        ExitState = "SC0019_KEY_CHANGED_REDSPLAY";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (ReadServiceProvider())
      {
        export.ServiceProvider.Assign(entities.ExistingServiceProvider);
      }
      else
      {
        var field = GetField(export.ServiceProvider, "userId");

        field.Error = true;

        ExitState = "SC0005_USER_NF";

        return;
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      if (Equal(global.Command, "DELETE"))
      {
        goto Test;
      }

      // VALIDATE ALL OF THE SELECTED PROFILE NAMES
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (!ReadProfile())
          {
            var field = GetField(export.Group.Item.Profile, "name");

            field.Error = true;

            ExitState = "SC0015_PROFILE_NF";
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that the user is not currently active in another profile for the
      // requested dates.
      // THIS IS FOR PROFILES CURRENTLY BEING ADDED OR MODIFIED......
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

            if (Equal(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate) && Equal
              (export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                DiscontinueDate))
            {
              // THIS IS IT SELF, BYPASS
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

            if (Equal(global.Command, "UPDATE"))
            {
              if (Equal(export.Group.Item.ServiceProviderProfile.
                CreatedTimestamp,
                local.Selected.Item.SelectedServiceProviderProfile.
                  CreatedTimestamp))
              {
                // MUST BE UPDATING THE ONE ALREADY ON THE DATABASE
                continue;
              }
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate) && !
              Lt(local.Selected.Item.SelectedServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              local.Selected.Item.SelectedServiceProviderProfile.
                EffectiveDate) && !
              Lt(local.Selected.Item.SelectedServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.DiscontinueDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

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

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "ADD"))
    {
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

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SC0021_INVALID_DATE_RANGE";
            }
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that the user is not currently active in another profile for the
      // requested dates.
      // THIS IS FOR PROFILES CURRENTLY OUT ON THE DATABASE.....
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          for(export.HiddenHistory.Index = 0; export.HiddenHistory.Index < export
            .HiddenHistory.Count; ++export.HiddenHistory.Index)
          {
            if (!export.HiddenHistory.CheckSize())
            {
              break;
            }

            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PRIOR TO THE BEGINNING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Lt(export.HiddenHistory.Item.
              HiddenHistoryServiceProviderProfile.DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PAST TO THE ENDING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Equal(export.Group.Item.ServiceProviderProfile.CreatedTimestamp,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                CreatedTimestamp))
            {
              // MUST BE UPDATING THE ONE ALREADY ON THE DATABASE
              continue;
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate) && !
              Lt(export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate) && !
              Lt(export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.DiscontinueDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }
          }

          export.HiddenHistory.CheckIndex();
        }
      }

      export.Group.CheckIndex();
    }

    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
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

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SC0021_INVALID_DATE_RANGE";
            }
          }

          // CANNOT SET UP DATES MORE THAN 1 DAY IN THE PAST
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // insure that the user is not currently active in another profile for the
      // requested dates.
      // THIS IS FOR PROFILES CURRENTLY OUT ON THE DATABASE.....
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          for(export.HiddenHistory.Index = 0; export.HiddenHistory.Index < export
            .HiddenHistory.Count; ++export.HiddenHistory.Index)
          {
            if (!export.HiddenHistory.CheckSize())
            {
              break;
            }

            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PRIOR TO THE BEGINNING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Lt(export.HiddenHistory.Item.
              HiddenHistoryServiceProviderProfile.DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              // THIS SAYS THAT THE PROFILE EFFECTIVE DATES(BOTH EFFECTIVE AND 
              // DISCONTINUE) ARE PAST TO THE ENDING OF THE PROFILE BEING
              // CHECKED. THIS IS OK.
              continue;
            }

            if (Equal(export.Group.Item.ServiceProviderProfile.CreatedTimestamp,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                CreatedTimestamp))
            {
              // MUST BE UPDATING THE ONE ALREADY ON THE DATABASE
              continue;
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate) && !
              Lt(export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.EffectiveDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }

            if (!Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                EffectiveDate) && !
              Lt(export.HiddenHistory.Item.HiddenHistoryServiceProviderProfile.
                DiscontinueDate,
              export.Group.Item.ServiceProviderProfile.DiscontinueDate))
            {
              var field1 = GetField(export.Group.Item.Profile, "name");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field3.Error = true;

              ExitState = "SC0022_SERV_PROV_ACT_IN_ANOT_PRO";
            }
          }

          export.HiddenHistory.CheckIndex();
        }
      }

      export.Group.CheckIndex();
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
      case "RETFMPRO":
        ExitState = "SC0051_NO_PROFILE_SELECTED";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PROF":
        ExitState = "ECO_LNK_TO_PROFILES";

        return;
      case "ASRV":
        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

          return;
        }

        ExitState = "ECO_XFR_2_SC_ASGN_SRV_PRV_2_PRO";

        return;
      case "DISPLAY":
        // DISPLAY LOGIC DOWN AT THE BOTTOM OF THE PRAD
        break;
      case "LIST":
        switch(AsChar(import.PromptServProvider.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            break;
          case ' ':
            var field1 = GetField(export.PromptServProvider, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          default:
            var field2 = GetField(export.PromptServProvider, "promptField");

            field2.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        return;
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

                var field = GetField(export.Group.Item.Profile, "name");

                field.Color = "cyan";
                field.Protected = true;

                if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
                  Now().Date))
                {
                  var field1 =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "effectiveDate");

                  field1.Color = "cyan";
                  field1.Protected = true;
                }

                if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                  var field1 =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "discontinueDate");

                  field1.Color = "cyan";
                  field1.Protected = true;
                }
              }
              else if (IsExitState("SC0015_PROFILE_NF"))
              {
                var field = GetField(export.Group.Item.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field = GetField(export.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
        else
        {
          return;
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

                var field = GetField(export.Group.Item.Profile, "name");

                field.Color = "cyan";
                field.Protected = true;

                if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
                  Now().Date))
                {
                  var field1 =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "effectiveDate");

                  field1.Color = "cyan";
                  field1.Protected = true;
                }

                if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
                  Now().Date))
                {
                  var field1 =
                    GetField(export.Group.Item.ServiceProviderProfile,
                    "discontinueDate");

                  field1.Color = "cyan";
                  field1.Protected = true;
                }
              }
              else if (IsExitState("SC0015_PROFILE_NF"))
              {
                var field = GetField(export.Group.Item.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field = GetField(export.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
                var field1 = GetField(export.Group.Item.Profile, "name");

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
        else
        {
          return;
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
                var field = GetField(export.Group.Item.Profile, "name");

                field.Error = true;
              }
              else if (IsExitState("SC0005_USER_NF"))
              {
                var field = GetField(export.ServiceProvider, "userId");

                field.Error = true;
              }
              else if (IsExitState("SC0023_SERV_PROVIDER_PROFILE_NF"))
              {
                var field1 = GetField(export.ServiceProvider, "userId");

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
                var field1 = GetField(export.ServiceProvider, "userId");

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
                var field1 = GetField(export.ServiceProvider, "userId");

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
                var field1 = GetField(export.ServiceProvider, "userId");

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
        else
        {
          return;
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY"))
    {
      export.PromptServProvider.PromptField = "";

      if (ReadServiceProvider())
      {
        export.ServiceProvider.Assign(entities.ExistingServiceProvider);
        export.HiddenPrev.UserId = entities.ExistingServiceProvider.UserId;
      }
      else
      {
        var field = GetField(export.ServiceProvider, "userId");

        field.Error = true;

        ExitState = "SC0005_USER_NF";

        return;
      }

      if (IsEmpty(export.SortBy.SelectChar))
      {
        export.SortBy.SelectChar = "E";
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
      export.HiddenHistory.Index = -1;

      switch(AsChar(export.SortBy.SelectChar))
      {
        case 'P':
          foreach(var item in ReadServiceProviderProfileProfile1())
          {
            ++export.HiddenHistory.Index;
            export.HiddenHistory.CheckSize();

            export.HiddenHistory.Update.HiddenHistoryProfile.Name =
              entities.ExistingProfile.Name;
            export.HiddenHistory.Update.HiddenHistoryServiceProviderProfile.
              Assign(entities.ExistingServiceProviderProfile);

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

            export.Group.Update.Profile.Name = entities.ExistingProfile.Name;
            export.Group.Update.ServiceProviderProfile.Assign(
              entities.ExistingServiceProviderProfile);
            MoveServiceProviderProfile(entities.ExistingServiceProviderProfile,
              export.Group.Update.HiddenPrev);
          }

          break;
        case 'E':
          foreach(var item in ReadServiceProviderProfileProfile3())
          {
            ++export.HiddenHistory.Index;
            export.HiddenHistory.CheckSize();

            export.HiddenHistory.Update.HiddenHistoryProfile.Name =
              entities.ExistingProfile.Name;
            export.HiddenHistory.Update.HiddenHistoryServiceProviderProfile.
              Assign(entities.ExistingServiceProviderProfile);

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

            export.Group.Update.Profile.Name = entities.ExistingProfile.Name;
            export.Group.Update.ServiceProviderProfile.Assign(
              entities.ExistingServiceProviderProfile);
            MoveServiceProviderProfile(entities.ExistingServiceProviderProfile,
              export.Group.Update.HiddenPrev);
          }

          break;
        case 'D':
          foreach(var item in ReadServiceProviderProfileProfile2())
          {
            ++export.HiddenHistory.Index;
            export.HiddenHistory.CheckSize();

            export.HiddenHistory.Update.HiddenHistoryProfile.Name =
              entities.ExistingProfile.Name;
            export.HiddenHistory.Update.HiddenHistoryServiceProviderProfile.
              Assign(entities.ExistingServiceProviderProfile);

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

            export.Group.Update.Profile.Name = entities.ExistingProfile.Name;
            export.Group.Update.ServiceProviderProfile.Assign(
              entities.ExistingServiceProviderProfile);
            MoveServiceProviderProfile(entities.ExistingServiceProviderProfile,
              export.Group.Update.HiddenPrev);
          }

          break;
        default:
          ++export.HiddenHistory.Index;
          export.HiddenHistory.CheckSize();

          export.HiddenHistory.Update.HiddenHistoryProfile.Name =
            entities.ExistingProfile.Name;
          export.HiddenHistory.Update.HiddenHistoryServiceProviderProfile.
            Assign(entities.ExistingServiceProviderProfile);

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
            export.Group.Update.Profile.Name = import.Group.Item.Profile.Name;
            export.Group.Update.ServiceProviderProfile.Assign(
              import.Group.Item.ServiceProviderProfile);
            MoveServiceProviderProfile(import.Group.Item.HiddenPrev,
              export.Group.Update.HiddenPrev);

            var field1 = GetField(export.Group.Item.Profile, "name");

            field1.Color = "cyan";
            field1.Protected = true;

            if (Lt(export.Group.Item.ServiceProviderProfile.EffectiveDate,
              Now().Date))
            {
              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "effectiveDate");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (Lt(export.Group.Item.ServiceProviderProfile.DiscontinueDate,
              Now().Date))
            {
              var field2 =
                GetField(export.Group.Item.ServiceProviderProfile,
                "discontinueDate");

              field2.Color = "cyan";
              field2.Protected = true;
            }
          }

          import.Group.CheckIndex();

          var field = GetField(export.SortBy, "selectChar");

          field.Error = true;

          ExitState = "SC0031_MUST_BE_P_E_OR_D";

          return;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        var field1 =
          GetField(export.Group.Item.ServiceProviderProfile, "effectiveDate");

        field1.Color = "";
        field1.Protected = false;

        var field2 =
          GetField(export.Group.Item.ServiceProviderProfile, "discontinueDate");
          

        field2.Protected = false;

        var field3 = GetField(export.Group.Item.Profile, "name");

        field3.Color = "cyan";
        field3.Protected = true;

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

      export.Group.CheckIndex();

      if (Equal(global.Command, "DISPLAY"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
      }
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

    MoveServiceProviderProfile(export.Group.Item.ServiceProviderProfile,
      useImport.ServiceProviderProfile);
    useImport.ServiceProvider.UserId = export.ServiceProvider.UserId;
    useImport.Profile.Name = export.Group.Item.Profile.Name;

    Call(CreateServProvProfile.Execute, useImport, useExport);

    export.Group.Update.ServiceProviderProfile.CreatedTimestamp =
      useExport.ServiceProviderProfile.CreatedTimestamp;
  }

  private void UseDeleteServProvProfile()
  {
    var useImport = new DeleteServProvProfile.Import();
    var useExport = new DeleteServProvProfile.Export();

    useImport.Profile.Name = export.Group.Item.Profile.Name;
    useImport.ServiceProvider.UserId = export.ServiceProvider.UserId;
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

    useImport.ServiceProviderProfile.Assign(
      export.Group.Item.ServiceProviderProfile);
    useImport.Profile.Name = export.Group.Item.Profile.Name;
    useImport.ServiceProvider.UserId = export.ServiceProvider.UserId;

    Call(UpdateServProvProfile.Execute, useImport, useExport);
  }

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", export.Group.Item.Profile.Name);
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
        db.SetString(command, "userId", export.ServiceProvider.UserId);
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

  private IEnumerable<bool> ReadServiceProviderProfileProfile1()
  {
    entities.ExistingServiceProviderProfile.Populated = false;
    entities.ExistingProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileProfile1",
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
        entities.ExistingProfile.Name = db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 5);
        entities.ExistingServiceProviderProfile.Populated = true;
        entities.ExistingProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileProfile2()
  {
    entities.ExistingServiceProviderProfile.Populated = false;
    entities.ExistingProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileProfile2",
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
        entities.ExistingProfile.Name = db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 5);
        entities.ExistingServiceProviderProfile.Populated = true;
        entities.ExistingProfile.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderProfileProfile3()
  {
    entities.ExistingServiceProviderProfile.Populated = false;
    entities.ExistingProfile.Populated = false;

    return ReadEach("ReadServiceProviderProfileProfile3",
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
        entities.ExistingProfile.Name = db.GetString(reader, 3);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 4);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 5);
        entities.ExistingServiceProviderProfile.Populated = true;
        entities.ExistingProfile.Populated = true;

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
      /// A value of Profile.
      /// </summary>
      [JsonPropertyName("profile")]
      public Profile Profile
      {
        get => profile ??= new();
        set => profile = value;
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
      /// A value of HiddenPrev.
      /// </summary>
      [JsonPropertyName("hiddenPrev")]
      public ServiceProviderProfile HiddenPrev
      {
        get => hiddenPrev ??= new();
        set => hiddenPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 130;

      private Common common;
      private Profile profile;
      private ServiceProviderProfile serviceProviderProfile;
      private ServiceProviderProfile hiddenPrev;
    }

    /// <summary>A HiddenHistoryGroup group.</summary>
    [Serializable]
    public class HiddenHistoryGroup
    {
      /// <summary>
      /// A value of HiddenHistoryProfile.
      /// </summary>
      [JsonPropertyName("hiddenHistoryProfile")]
      public Profile HiddenHistoryProfile
      {
        get => hiddenHistoryProfile ??= new();
        set => hiddenHistoryProfile = value;
      }

      /// <summary>
      /// A value of HiddenHistoryServiceProviderProfile.
      /// </summary>
      [JsonPropertyName("hiddenHistoryServiceProviderProfile")]
      public ServiceProviderProfile HiddenHistoryServiceProviderProfile
      {
        get => hiddenHistoryServiceProviderProfile ??= new();
        set => hiddenHistoryServiceProviderProfile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Profile hiddenHistoryProfile;
      private ServiceProviderProfile hiddenHistoryServiceProviderProfile;
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
    /// A value of HiddenSelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectedServiceProvider")]
    public ServiceProvider HiddenSelectedServiceProvider
    {
      get => hiddenSelectedServiceProvider ??= new();
      set => hiddenSelectedServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public ServiceProvider HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of HiddenSelectedProfile.
    /// </summary>
    [JsonPropertyName("hiddenSelectedProfile")]
    public Profile HiddenSelectedProfile
    {
      get => hiddenSelectedProfile ??= new();
      set => hiddenSelectedProfile = value;
    }

    /// <summary>
    /// A value of PromptServProvider.
    /// </summary>
    [JsonPropertyName("promptServProvider")]
    public Standard PromptServProvider
    {
      get => promptServProvider ??= new();
      set => promptServProvider = value;
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
    /// Gets a value of HiddenHistory.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenHistoryGroup> HiddenHistory => hiddenHistory ??= new(
      HiddenHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenHistory for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenHistory")]
    [Computed]
    public IList<HiddenHistoryGroup> HiddenHistory_Json
    {
      get => hiddenHistory;
      set => HiddenHistory.Assign(value);
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

    private Common activeOnly;
    private ServiceProvider hiddenSelectedServiceProvider;
    private ServiceProvider hiddenPrev;
    private ServiceProvider serviceProvider;
    private Common sortBy;
    private Profile hiddenSelectedProfile;
    private Standard promptServProvider;
    private Array<GroupGroup> group;
    private Array<HiddenHistoryGroup> hiddenHistory;
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
      /// A value of Profile.
      /// </summary>
      [JsonPropertyName("profile")]
      public Profile Profile
      {
        get => profile ??= new();
        set => profile = value;
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
      /// A value of HiddenPrev.
      /// </summary>
      [JsonPropertyName("hiddenPrev")]
      public ServiceProviderProfile HiddenPrev
      {
        get => hiddenPrev ??= new();
        set => hiddenPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 130;

      private Common common;
      private Profile profile;
      private ServiceProviderProfile serviceProviderProfile;
      private ServiceProviderProfile hiddenPrev;
    }

    /// <summary>A HiddenHistoryGroup group.</summary>
    [Serializable]
    public class HiddenHistoryGroup
    {
      /// <summary>
      /// A value of HiddenHistoryProfile.
      /// </summary>
      [JsonPropertyName("hiddenHistoryProfile")]
      public Profile HiddenHistoryProfile
      {
        get => hiddenHistoryProfile ??= new();
        set => hiddenHistoryProfile = value;
      }

      /// <summary>
      /// A value of HiddenHistoryServiceProviderProfile.
      /// </summary>
      [JsonPropertyName("hiddenHistoryServiceProviderProfile")]
      public ServiceProviderProfile HiddenHistoryServiceProviderProfile
      {
        get => hiddenHistoryServiceProviderProfile ??= new();
        set => hiddenHistoryServiceProviderProfile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Profile hiddenHistoryProfile;
      private ServiceProviderProfile hiddenHistoryServiceProviderProfile;
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
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Profile HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public ServiceProvider HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of PromptServProvider.
    /// </summary>
    [JsonPropertyName("promptServProvider")]
    public Standard PromptServProvider
    {
      get => promptServProvider ??= new();
      set => promptServProvider = value;
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
    /// Gets a value of HiddenHistory.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenHistoryGroup> HiddenHistory => hiddenHistory ??= new(
      HiddenHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenHistory for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenHistory")]
    [Computed]
    public IList<HiddenHistoryGroup> HiddenHistory_Json
    {
      get => hiddenHistory;
      set => HiddenHistory.Assign(value);
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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

    private Common activeOnly;
    private Profile hiddenSelected;
    private ServiceProvider serviceProvider;
    private ServiceProvider hiddenPrev;
    private Common sortBy;
    private Standard promptServProvider;
    private Array<GroupGroup> group;
    private Array<HiddenHistoryGroup> hiddenHistory;
    private Common hiddenFromSecurity;
    private Security2 hiddenSecurity;
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

      private Profile selectedProfile;
      private ServiceProviderProfile selectedServiceProviderProfile;
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
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public CsePersonsWorkSet Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    private ServiceProviderProfile currentMinus1Day;
    private Array<SelectedGroup> selected;
    private DateWorkArea dateWorkArea;
    private Common common;
    private CsePersonsWorkSet dummy;
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
