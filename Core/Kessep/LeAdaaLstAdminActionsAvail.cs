// Program: LE_ADAA_LST_ADMIN_ACTIONS_AVAIL, ID: 372581896, model: 746.
// Short name: SWEADAAP
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
/// A program: LE_ADAA_LST_ADMIN_ACTIONS_AVAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAdaaLstAdminActionsAvail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_ADAA_LST_ADMIN_ACTIONS_AVAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAdaaLstAdminActionsAvail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAdaaLstAdminActionsAvail.
  /// </summary>
  public LeAdaaLstAdminActionsAvail(IContext context, Import import,
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
    // *******************************************************************
    // Date  	  Developer      Request #  
    // Description
    // 
    // 05/05/95  C.W.Hedenskog
    // Initial Code
    // 06/22/95  S. Benton                 Add LIST Command
    //                                     
    // Remove PF2 Disp from screen
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    local.Common.Count = 0;

    if (Equal(global.Command, "DISPLAY") && Equal(global.Command, "EXIT") && Equal
      (global.Command, "SIGNOFF"))
    {
    }
    else if (!import.Group.IsEmpty)
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.AdministrativeAction.Assign(
          import.Group.Item.AdministrativeAction);
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        // *********************************************
        // Check how many selections have been made.
        // Do not allow scrolling when a selection has
        // been made.
        // *********************************************
        switch(AsChar(import.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;
            export.HiddenSelected.
              Assign(export.Group.Item.AdministrativeAction);

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        export.Group.Next();
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(global.Command, "CRIT"))
        {
          if (local.Common.Count == 0)
          {
            // *********************************************
            // No selection has been made.
            // *********************************************
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (local.Common.Count > 1)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (IsEmpty(export.Group.Item.Common.SelectChar))
            {
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
      else
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RTLIST"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    //        P F   K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *********************************************
        // DISPLAY Logic is done at the end.
        // *********************************************
        break;
      case "CRIT":
        // *********************************************
        // If PF15 CRIT is pressed, link to
        // 'Administrative Actions by Criteria' screen.
        // *********************************************
        ExitState = "ECO_LNK_TO_ADMIN_ACTION_CRITERIA";

        break;
      case "HELP":
        // *********************************************
        // All logic pertaining to using the IEF help
        // facility should be placed here.
        // At this time, this is not available.
        // *********************************************
        break;
      case "RTLIST":
        break;
      case "NEXT":
        ExitState = "ZD_ACO_NE0000_INVALID_FORWARD_3";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseLeDisplayAdminActnsAvailable();

      if (!export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
      }
    }
    else
    {
    }
  }

  private static void MoveAdminActionToGroup(LeDisplayAdminActnsAvailable.
    Export.AdminActionGroup source, Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.AdministrativeAction.Assign(source.AdministrativeAction);
  }

  private void UseLeDisplayAdminActnsAvailable()
  {
    var useImport = new LeDisplayAdminActnsAvailable.Import();
    var useExport = new LeDisplayAdminActnsAvailable.Export();

    Call(LeDisplayAdminActnsAvailable.Execute, useImport, useExport);

    useExport.AdminAction.CopyTo(export.Group, MoveAdminActionToGroup);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 102;

      private Common common;
      private AdministrativeAction administrativeAction;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
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
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 102;

      private Common common;
      private AdministrativeAction administrativeAction;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public AdministrativeAction HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    private AdministrativeAction hiddenSelected;
    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Common common;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
