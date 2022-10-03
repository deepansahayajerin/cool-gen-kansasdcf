// Program: SI_KEES_IRI_PREFERRED_N_PS_MAINT, ID: 1625311303, model: 746.
// Short name: SWEKSPSP
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
/// A program: SI_KEES_IRI_PREFERRED_N_PS_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiKeesIriPreferredNPsMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_KEES_IRI_PREFERRED_N_PS_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiKeesIriPreferredNPsMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiKeesIriPreferredNPsMaint.
  /// </summary>
  public SiKeesIriPreferredNPsMaint(IContext context, Import import,
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
    // 		M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    // Date	    Developer		Description
    // ------------------------------------------------------------
    // 02-27-2018  Raj S	        Initial Dev
    // 11-09-2020  Jagan Bobberla      CQ#65700
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------------------------------
    // Check whether SIGNOFF command is issued, if so, logging out of CICS
    // ---------------------------------------------------------------------
    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ---------------------------------------------------------------------
    // Clear Command issued NO data will be moved to clear the field vlaues.
    // ---------------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    local.ErrorFlag.Flag = "";
    local.ImportLastSubscript.Subscript = 0;
    export.InputIds.Index = -1;
    export.HiddenInputIds.Index = -1;
    local.FirstTimeFlag.Flag = "Y";

    if (Equal(global.Command, "DISPLAY"))
    {
      for(import.Ids.Index = 0; import.Ids.Index < Import.IdsGroup.Capacity; ++
        import.Ids.Index)
      {
        if (!import.Ids.CheckSize())
        {
          break;
        }

        if (!IsEmpty(import.Ids.Item.GimpInputIds.Number))
        {
          ++export.InputIds.Index;
          export.InputIds.CheckSize();

          export.InputIds.Update.GexpInputIds.Number =
            import.Ids.Item.GimpInputIds.Number;
          UseCabZeroFillNumber1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.InputIds.Item.GexpInputIds, "number");

            field.Error = true;

            local.ErrorFlag.Flag = "Y";
          }

          ++export.HiddenInputIds.Index;
          export.HiddenInputIds.CheckSize();

          export.HiddenInputIds.Update.GexpHiddenInputIds.Number =
            export.InputIds.Item.GexpInputIds.Number;
        }
      }

      import.Ids.CheckIndex();
    }
    else
    {
      for(import.Ids.Index = 0; import.Ids.Index < import.Ids.Count; ++
        import.Ids.Index)
      {
        if (!import.Ids.CheckSize())
        {
          break;
        }

        export.InputIds.Index = import.Ids.Index;
        export.InputIds.CheckSize();

        export.InputIds.Update.GexpInputIds.Number =
          import.Ids.Item.GimpInputIds.Number;

        export.HiddenInputIds.Index = import.Ids.Index;
        export.HiddenInputIds.CheckSize();

        import.HiddenInputIds.Index = import.Ids.Index;
        import.HiddenInputIds.CheckSize();

        export.HiddenInputIds.Update.GexpHiddenInputIds.Number =
          import.HiddenInputIds.Item.GimpHiddenInputIds.Number;

        if (!Equal(export.InputIds.Item.GexpInputIds.Number,
          export.HiddenInputIds.Item.GexpHiddenInputIds.Number) && Equal
          (global.Command, "SAVE"))
        {
          var field = GetField(export.InputIds.Item.GexpInputIds, "number");

          field.Error = true;

          if (AsChar(local.FirstTimeFlag.Flag) == 'Y')
          {
            var field1 = GetField(export.Hidden, "uniqueKey");

            field1.Protected = false;
            field1.Focused = true;

            local.FirstTimeFlag.Flag = "E";
          }
        }
      }

      import.Ids.CheckIndex();
    }

    // ------------------------------------------------------------------------------
    // If any of the input person IDs are errored out then display the error 
    // message.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ErrorFlag.Flag) == 'Y')
    {
      ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

      return;
    }

    // ------------------------------------------------------------------------------
    // Input ID is changed and SAVE command issued, force the user to use
    // DISPLAY command.
    // ------------------------------------------------------------------------------
    if (AsChar(local.FirstTimeFlag.Flag) == 'E')
    {
      ExitState = "CO0000_INFO_CHNGD_REDISPLAY";

      return;
    }

    local.ErrorFlag.Flag = "";
    local.InvalidPreferredIdFlag.Flag = "";
    export.ScreenList.Index = -1;

    for(import.ScreenList.Index = 0; import.ScreenList.Index < Import
      .ScreenListGroup.Capacity; ++import.ScreenList.Index)
    {
      if (!import.ScreenList.CheckSize())
      {
        break;
      }

      export.InputIds.Index = import.ScreenList.Index;
      export.InputIds.CheckSize();

      if (IsEmpty(export.InputIds.Item.GexpInputIds.Number))
      {
        continue;
      }

      export.ScreenList.Index = import.ScreenList.Index;
      export.ScreenList.CheckSize();

      export.ScreenList.Update.GexpScreen.SelectChar =
        import.ScreenList.Item.GimpScreenSel.SelectChar;
      export.ScreenList.Update.GexpScreenPersonInfo.Assign(
        import.ScreenList.Item.GimpScreenPersonInfo);
      export.ScreenList.Update.GexpScreenAeFlag.Text1 =
        import.ScreenList.Item.GimpScreenAeFlag.Text1;
      export.ScreenList.Update.GexpScreenCsFlag.Text1 =
        import.ScreenList.Item.GimpScreenCsFlag.Text1;
      export.ScreenList.Update.GexpScreenFaFlag.Text1 =
        import.ScreenList.Item.GimpScreenFaFlag.Text1;
      export.ScreenList.Update.GexpScreenKmFlag.Text1 =
        import.ScreenList.Item.GimpScreenKmFlag.Text1;
      export.ScreenList.Update.GexpScreenPreferredId.Number =
        import.ScreenList.Item.GimpScreenPreferredId.Number;

      if (!IsEmpty(export.ScreenList.Item.GexpScreenPersonInfo.Number))
      {
        var field1 = GetField(export.ScreenList.Item.GexpScreen, "selectChar");

        field1.Protected = false;

        var field2 =
          GetField(export.ScreenList.Item.GexpScreenPersonInfo,
          "replicationIndicator");

        field2.Protected = false;

        var field3 =
          GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

        field3.Protected = false;

        var field4 = GetField(export.ScreenList.Item.GexpScreenCsFlag, "text1");

        field4.Protected = false;

        var field5 = GetField(export.ScreenList.Item.GexpScreenFaFlag, "text1");

        field5.Protected = false;

        var field6 = GetField(export.ScreenList.Item.GexpScreenKmFlag, "text1");

        field6.Protected = false;

        if (IsEmpty(export.ScreenList.Item.GexpScreenPreferredId.Number))
        {
          var field =
            GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

          field.Error = true;

          local.InvalidPreferredIdFlag.Flag = "Y";
        }
      }
      else
      {
        var field1 = GetField(export.ScreenList.Item.GexpScreen, "selectChar");

        field1.Protected = true;

        var field2 =
          GetField(export.ScreenList.Item.GexpScreenPersonInfo,
          "replicationIndicator");

        field2.Protected = true;

        var field3 =
          GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

        field3.Protected = true;

        var field4 = GetField(export.ScreenList.Item.GexpScreenAeFlag, "text1");

        field4.Protected = true;

        var field5 = GetField(export.ScreenList.Item.GexpScreenCsFlag, "text1");

        field5.Protected = true;

        var field6 = GetField(export.ScreenList.Item.GexpScreenFaFlag, "text1");

        field6.Protected = true;

        var field7 = GetField(export.ScreenList.Item.GexpScreenKmFlag, "text1");

        field7.Protected = true;
      }

      if (!IsEmpty(export.ScreenList.Item.GexpScreenPreferredId.Number))
      {
        UseCabZeroFillNumber2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 =
            GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

          field1.Error = true;

          local.ErrorFlag.Flag = "Y";
        }

        var field =
          GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

        field.Protected = false;
      }
    }

    import.ScreenList.CheckIndex();

    // ------------------------------------------------------------------------------
    // Check to see whether SAVE command is issued without any change 
    // informaton.
    // ------------------------------------------------------------------------------
    if (export.InputIds.IsEmpty && Equal(global.Command, "SAVE"))
    {
      ExitState = "ACO_NE0000_DISPLAY_BEFORE";

      return;
    }

    // ------------------------------------------------------------------------------
    // If any of the input person IDs are errored out then display the error 
    // message.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ErrorFlag.Flag) == 'Y')
    {
      ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

      return;
    }

    // ------------------------------------------------------------------------------
    // If Preferred ID contains no value then display the error message.
    // ------------------------------------------------------------------------------
    if (AsChar(local.InvalidPreferredIdFlag.Flag) == 'Y')
    {
      ExitState = "SI0000_PREFERRED_ID_INVALID";

      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.EabDmlFlag.Flag = "D";
        local.SyncClientNfReadError.Flag = "";
        UseEabMaintainKeesSyncClient2();
        export.ScreenList.Count = 0;

        for(local.ScreenList.Index = 0; local.ScreenList.Index < local
          .ScreenList.Count; ++local.ScreenList.Index)
        {
          if (!local.ScreenList.CheckSize())
          {
            break;
          }

          export.ScreenList.Index = local.ScreenList.Index;
          export.ScreenList.CheckSize();

          export.ScreenList.Update.GexpScreen.SelectChar =
            local.ScreenList.Item.GlclScreen.SelectChar;

          // ------------------------------------------------------------------------------------
          // CQ65700 JR changes - Begin - 11/20/2020
          // ------------------------------------------------------------------------------------
          export.ScreenList.Update.GexpScreenPersonInfo.Assign(
            local.ScreenList.Item.GlclScreenPersonInfo);

          // ------------------------------------------------------------------------------------
          // CQ65700 JR changes - End - 11/20/2020
          // ------------------------------------------------------------------------------------
          export.ScreenList.Update.GexpScreenAeFlag.Text1 =
            local.ScreenList.Item.GlclScreenAe.Flag;
          export.ScreenList.Update.GexpScreenCsFlag.Text1 =
            local.ScreenList.Item.GlclScreenCs.Flag;
          export.ScreenList.Update.GexpScreenFaFlag.Text1 =
            local.ScreenList.Item.GlclScreenFa.Flag;
          export.ScreenList.Update.GexpScreenKmFlag.Text1 =
            local.ScreenList.Item.GlclScreenKm.Flag;
          export.ScreenList.Update.GexpScreenPreferredId.Number =
            local.ScreenList.Item.GlclScreenPreferredId.Number;
        }

        local.ScreenList.CheckIndex();

        for(export.ScreenList.Index = 0; export.ScreenList.Index < Export
          .ScreenListGroup.Capacity; ++export.ScreenList.Index)
        {
          if (!export.ScreenList.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.ScreenList.Item.GexpScreenPersonInfo.Number))
          {
            var field1 =
              GetField(export.ScreenList.Item.GexpScreen, "selectChar");

            field1.Protected = true;

            var field2 =
              GetField(export.ScreenList.Item.GexpScreenPersonInfo,
              "replicationIndicator");

            field2.Protected = true;

            var field3 =
              GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

            field3.Protected = true;

            var field4 =
              GetField(export.ScreenList.Item.GexpScreenAeFlag, "text1");

            field4.Protected = true;

            var field5 =
              GetField(export.ScreenList.Item.GexpScreenCsFlag, "text1");

            field5.Protected = true;

            var field6 =
              GetField(export.ScreenList.Item.GexpScreenFaFlag, "text1");

            field6.Protected = true;

            var field7 =
              GetField(export.ScreenList.Item.GexpScreenKmFlag, "text1");

            field7.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.ScreenList.Item.GexpScreenCsFlag, "text1");

            field1.Protected = false;

            var field2 =
              GetField(export.ScreenList.Item.GexpScreenFaFlag, "text1");

            field2.Protected = false;

            var field3 =
              GetField(export.ScreenList.Item.GexpScreenKmFlag, "text1");

            field3.Protected = false;

            var field4 =
              GetField(export.ScreenList.Item.GexpScreen, "selectChar");

            field4.Color = "green";
            field4.Highlighting = Highlighting.Underscore;
            field4.Protected = false;

            var field5 =
              GetField(export.ScreenList.Item.GexpScreenPersonInfo,
              "replicationIndicator");

            field5.Color = "green";
            field5.Highlighting = Highlighting.Underscore;
            field5.Protected = false;

            var field6 =
              GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");

            field6.Color = "green";
            field6.Highlighting = Highlighting.Underscore;
            field6.Protected = false;
          }

          if (AsChar(export.ScreenList.Item.GexpScreen.SelectChar) == 'N')
          {
            var field1 =
              GetField(export.ScreenList.Item.GexpScreen, "selectChar");

            field1.Error = true;

            export.InputIds.Index = export.ScreenList.Index;
            export.InputIds.CheckSize();

            var field2 = GetField(export.InputIds.Item.GexpInputIds, "number");

            field2.Error = true;

            local.SyncClientNfReadError.Flag = "Y";
          }

          if (AsChar(export.ScreenList.Item.GexpScreen.SelectChar) == 'E')
          {
            var field1 =
              GetField(export.ScreenList.Item.GexpScreenPersonInfo,
              "replicationIndicator");

            field1.Error = true;

            export.InputIds.Index = export.ScreenList.Index;
            export.InputIds.CheckSize();

            var field2 = GetField(export.InputIds.Item.GexpInputIds, "number");

            field2.Error = true;

            local.SyncClientNfReadError.Flag = "Y";
          }
        }

        export.ScreenList.CheckIndex();

        if (AsChar(local.SyncClientNfReadError.Flag) == 'Y')
        {
          ExitState = "SI0000_SYNC_CLIENT_RETRIVE_ERROR";

          return;
        }

        break;
      case "SAVE":
        local.EabDmlFlag.Flag = "U";
        local.ScreenValueErrorFound.Flag = "";
        local.SelectionMadeFlag.Flag = "";

        for(export.ScreenList.Index = 0; export.ScreenList.Index < export
          .ScreenList.Count; ++export.ScreenList.Index)
        {
          if (!export.ScreenList.CheckSize())
          {
            break;
          }

          local.ScreenList.Index = export.ScreenList.Index;
          local.ScreenList.CheckSize();

          local.ScreenList.Update.GlclScreen.SelectChar =
            export.ScreenList.Item.GexpScreen.SelectChar;
          local.ScreenList.Update.GlclScreenPersonInfo.Assign(
            export.ScreenList.Item.GexpScreenPersonInfo);

          // ------------------------------------------------------------------------------------
          // CQ65700 JR changes - Begin - 11/17/2020
          // ------------------------------------------------------------------------------------
          local.ScreenList.Update.GlclScreenAe.Flag =
            export.ScreenList.Item.GexpScreenAeFlag.Text1;
          local.ScreenList.Update.GlclScreenCs.Flag =
            export.ScreenList.Item.GexpScreenCsFlag.Text1;
          local.ScreenList.Update.GlclScreenFa.Flag =
            export.ScreenList.Item.GexpScreenFaFlag.Text1;
          local.ScreenList.Update.GlclScreenKm.Flag =
            export.ScreenList.Item.GexpScreenKmFlag.Text1;
          local.ScreenList.Update.GlclScreenPreferredId.Number =
            export.ScreenList.Item.GexpScreenPreferredId.Number;

          if (!IsEmpty(export.ScreenList.Item.GexpScreen.SelectChar) && AsChar
            (export.ScreenList.Item.GexpScreen.SelectChar) != 'S')
          {
            local.ScreenValueErrorFound.Flag = "Y";

            var field =
              GetField(export.ScreenList.Item.GexpScreen, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.ScreenList.Item.GexpScreen.SelectChar) == 'S')
          {
            local.SelectionMadeFlag.Flag = "Y";
          }

          // -------------------------------------------------------------
          // CQ65700 JR changes - Begin
          // Added code for system flag validation for Y or spaces
          // -------------------------------------------------------------
          if (AsChar(local.ScreenList.Item.GlclScreenAe.Flag) == 'Y' || IsEmpty
            (local.ScreenList.Item.GlclScreenAe.Flag))
          {
          }
          else
          {
            local.SystemFlagError.Flag = "Y";
          }

          if (AsChar(local.ScreenList.Item.GlclScreenCs.Flag) == 'Y' || IsEmpty
            (local.ScreenList.Item.GlclScreenCs.Flag))
          {
          }
          else
          {
            local.SystemFlagError.Flag = "Y";
          }

          if (AsChar(local.ScreenList.Item.GlclScreenFa.Flag) == 'Y' || IsEmpty
            (local.ScreenList.Item.GlclScreenFa.Flag))
          {
          }
          else
          {
            local.SystemFlagError.Flag = "Y";
          }

          if (AsChar(local.ScreenList.Item.GlclScreenKm.Flag) == 'Y' || IsEmpty
            (local.ScreenList.Item.GlclScreenKm.Flag))
          {
          }
          else
          {
            local.SystemFlagError.Flag = "Y";
          }

          // -------------------------------------------------------------
          // CQ65700 JR changes - End
          // Added code for system flag validation for Y or spaces
          // -------------------------------------------------------------
          if (!IsEmpty(export.ScreenList.Item.GexpScreenPersonInfo.
            ReplicationIndicator) && (
              AsChar(export.ScreenList.Item.GexpScreenPersonInfo.
              ReplicationIndicator) == 'P' || AsChar
            (export.ScreenList.Item.GexpScreenPersonInfo.ReplicationIndicator) ==
              'S'))
          {
          }
          else if (!IsEmpty(export.ScreenList.Item.GexpScreenPersonInfo.
            ReplicationIndicator))
          {
            local.InvalidPNSValue.Flag = "Y";

            var field =
              GetField(export.ScreenList.Item.GexpScreenPersonInfo,
              "replicationIndicator");

            field.Error = true;
          }
        }

        export.ScreenList.CheckIndex();

        if (AsChar(local.SelectionMadeFlag.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        // -------------------------------------------------------------
        // CQ65700 JR changes - Begin
        // Exit state for Invalid system flag for CS/Fax/KMIS
        // -------------------------------------------------------------
        if (AsChar(local.SystemFlagError.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_INVALID_SYSTEM_FLAG";

          return;
        }

        // -------------------------------------------------------------
        // CQ65700 JR changes - End
        // -------------------------------------------------------------
        if (AsChar(local.ScreenValueErrorFound.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        if (AsChar(local.InvalidPNSValue.Flag) == 'Y')
        {
          ExitState = "SI0000_INVALID_P_N_S_VALUE";

          return;
        }

        UseEabMaintainKeesSyncClient1();

        for(local.ScreenList.Index = 0; local.ScreenList.Index < local
          .ScreenList.Count; ++local.ScreenList.Index)
        {
          if (!local.ScreenList.CheckSize())
          {
            break;
          }

          export.ScreenList.Update.GexpScreenPersonInfo.Assign(
            local.ScreenList.Item.GlclScreenPersonInfo);
          export.ScreenList.Update.GexpScreenAeFlag.Text1 =
            local.ScreenList.Item.GlclScreenAe.Flag;
          export.ScreenList.Update.GexpScreenCsFlag.Text1 =
            local.ScreenList.Item.GlclScreenCs.Flag;
          export.ScreenList.Update.GexpScreenFaFlag.Text1 =
            local.ScreenList.Item.GlclScreenFa.Flag;
          export.ScreenList.Update.GexpScreenKmFlag.Text1 =
            local.ScreenList.Item.GlclScreenKm.Flag;
          export.ScreenList.Update.GexpScreenPreferredId.Number =
            local.ScreenList.Item.GlclScreenPreferredId.Number;
        }

        local.ScreenList.CheckIndex();

        if (local.DmlReturnCode.Count == 0)
        {
          ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
        }
        else
        {
          local.InvalidPNSValue.Flag = "";

          for(export.ScreenList.Index = 0; export.ScreenList.Index < Export
            .ScreenListGroup.Capacity; ++export.ScreenList.Index)
          {
            if (!export.ScreenList.CheckSize())
            {
              break;
            }

            if (IsEmpty(export.ScreenList.Item.GexpScreenPersonInfo.Number))
            {
              var field1 =
                GetField(export.ScreenList.Item.GexpScreen, "selectChar");

              field1.Protected = true;

              var field2 =
                GetField(export.ScreenList.Item.GexpScreenPersonInfo,
                "replicationIndicator");

              field2.Protected = true;

              var field3 =
                GetField(export.ScreenList.Item.GexpScreenPreferredId, "number");
                

              field3.Protected = true;
            }
            else
            {
              if (AsChar(export.ScreenList.Item.GexpScreen.SelectChar) == 'P')
              {
                export.ScreenList.Update.GexpScreen.SelectChar = "S";

                var field1 =
                  GetField(export.ScreenList.Item.GexpScreen, "selectChar");

                field1.Error = true;

                var field2 =
                  GetField(export.ScreenList.Item.GexpScreenPreferredId,
                  "number");

                field2.Error = true;

                export.InputIds.Index = export.ScreenList.Index;
                export.InputIds.CheckSize();

                var field3 =
                  GetField(export.InputIds.Item.GexpInputIds, "number");

                field3.Error = true;

                local.InvalidPNSValue.Flag = "Y";
              }

              if (AsChar(export.ScreenList.Item.GexpScreen.SelectChar) == 'E')
              {
                var field1 =
                  GetField(export.ScreenList.Item.GexpScreen, "selectChar");

                field1.Error = true;

                export.ScreenList.Update.GexpScreen.SelectChar = "S";

                export.InputIds.Index = export.ScreenList.Index;
                export.InputIds.CheckSize();

                var field2 =
                  GetField(export.InputIds.Item.GexpInputIds, "number");

                field2.Error = true;
              }
            }
          }

          export.ScreenList.CheckIndex();

          if (AsChar(local.InvalidPNSValue.Flag) == 'Y')
          {
            ExitState = "SI0000_INVALID_PRI_N_SEC_IND";
          }
          else
          {
            ExitState = "SI0000_SYNC_CLIENT_UPDATE_FAILED";
          }
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "":
        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      if (export.ScreenList.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
    }
  }

  private static void MoveInputIds(Export.InputIdsGroup source,
    EabMaintainKeesSyncClient.Import.IdsGroup target)
  {
    target.GimportInputIds.Number = source.GexpInputIds.Number;
  }

  private static void MoveList(EabMaintainKeesSyncClient.Export.
    ListGroup source, Local.ScreenListGroup target)
  {
    target.GlclScreen.SelectChar = source.G.SelectChar;
    target.GlclScreenPersonInfo.Assign(source.GexportPersonInfo);
    target.GlclScreenAe.Flag = source.GexportAeFlag.Flag;
    target.GlclScreenCs.Flag = source.GexportCsFlag.Flag;
    target.GlclScreenFa.Flag = source.GexportFaFlag.Flag;
    target.GlclScreenKm.Flag = source.GexportKmFlag.Flag;
    target.GlclScreenPreferredId.Number = source.GexportPreferredId.Number;
  }

  private static void MoveScreenList1(Local.ScreenListGroup source,
    EabMaintainKeesSyncClient.Import.ListGroup target)
  {
    target.G.SelectChar = source.GlclScreen.SelectChar;
    target.GimportPersonInfo.Assign(source.GlclScreenPersonInfo);
    target.GimportAeFlag.Flag = source.GlclScreenAe.Flag;
    target.GimportCsFlag.Flag = source.GlclScreenCs.Flag;
    target.GimportFaFlag.Flag = source.GlclScreenFa.Flag;
    target.GimportKmFlag.Flag = source.GlclScreenKm.Flag;
    target.GimportPreferredId.Number = source.GlclScreenPreferredId.Number;
  }

  private static void MoveScreenList2(Local.ScreenListGroup source,
    EabMaintainKeesSyncClient.Export.ListGroup target)
  {
    target.G.SelectChar = source.GlclScreen.SelectChar;
    target.GexportPersonInfo.Assign(source.GlclScreenPersonInfo);
    target.GexportAeFlag.Flag = source.GlclScreenAe.Flag;
    target.GexportCsFlag.Flag = source.GlclScreenCs.Flag;
    target.GexportFaFlag.Flag = source.GlclScreenFa.Flag;
    target.GexportKmFlag.Flag = source.GlclScreenKm.Flag;
    target.GexportPreferredId.Number = source.GlclScreenPreferredId.Number;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number =
      export.InputIds.Item.GexpInputIds.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.InputIds.Update.GexpInputIds.Number =
      useImport.CsePersonsWorkSet.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number =
      export.ScreenList.Item.GexpScreenPreferredId.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.ScreenList.Update.GexpScreenPreferredId.Number =
      useImport.CsePersonsWorkSet.Number;
  }

  private void UseEabMaintainKeesSyncClient1()
  {
    var useImport = new EabMaintainKeesSyncClient.Import();
    var useExport = new EabMaintainKeesSyncClient.Export();

    useImport.EabDmlFlag.Flag = local.EabDmlFlag.Flag;
    useImport.UpdatePersonWorkSet.Assign(
      local.ScreenList.Item.GlclScreenPersonInfo);
    useImport.UpdatePreferredId.Number = local.PreferredId.Number;
    export.InputIds.CopyTo(useImport.Ids, MoveInputIds);
    local.ScreenList.CopyTo(useImport.List, MoveScreenList1);
    useExport.DmlReturnCode.Count = local.DmlReturnCode.Count;
    local.ScreenList.CopyTo(useExport.List, MoveScreenList2);

    Call(EabMaintainKeesSyncClient.Execute, useImport, useExport);

    local.DmlReturnCode.Count = useExport.DmlReturnCode.Count;
    useExport.List.CopyTo(local.ScreenList, MoveList);
  }

  private void UseEabMaintainKeesSyncClient2()
  {
    var useImport = new EabMaintainKeesSyncClient.Import();
    var useExport = new EabMaintainKeesSyncClient.Export();

    useImport.EabDmlFlag.Flag = local.EabDmlFlag.Flag;
    export.InputIds.CopyTo(useImport.Ids, MoveInputIds);

    useExport.DmlReturnCode.Count = local.DmlReturnCode.Count;
    local.ScreenList.CopyTo(useExport.List, MoveScreenList2);

    Call(EabMaintainKeesSyncClient.Execute, useImport, useExport);

    local.DmlReturnCode.Count = useExport.DmlReturnCode.Count;
    useExport.List.CopyTo(local.ScreenList, MoveList);
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
    /// <summary>A HiddenInputIdsGroup group.</summary>
    [Serializable]
    public class HiddenInputIdsGroup
    {
      /// <summary>
      /// A value of GimpHiddenInputIds.
      /// </summary>
      [JsonPropertyName("gimpHiddenInputIds")]
      public CsePersonsWorkSet GimpHiddenInputIds
      {
        get => gimpHiddenInputIds ??= new();
        set => gimpHiddenInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gimpHiddenInputIds;
    }

    /// <summary>A IdsGroup group.</summary>
    [Serializable]
    public class IdsGroup
    {
      /// <summary>
      /// A value of GimpInputIds.
      /// </summary>
      [JsonPropertyName("gimpInputIds")]
      public CsePersonsWorkSet GimpInputIds
      {
        get => gimpInputIds ??= new();
        set => gimpInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gimpInputIds;
    }

    /// <summary>A ScreenListGroup group.</summary>
    [Serializable]
    public class ScreenListGroup
    {
      /// <summary>
      /// A value of GimpScreenSel.
      /// </summary>
      [JsonPropertyName("gimpScreenSel")]
      public Common GimpScreenSel
      {
        get => gimpScreenSel ??= new();
        set => gimpScreenSel = value;
      }

      /// <summary>
      /// A value of GimpScreenPersonInfo.
      /// </summary>
      [JsonPropertyName("gimpScreenPersonInfo")]
      public CsePersonsWorkSet GimpScreenPersonInfo
      {
        get => gimpScreenPersonInfo ??= new();
        set => gimpScreenPersonInfo = value;
      }

      /// <summary>
      /// A value of GimpScreenAeFlag.
      /// </summary>
      [JsonPropertyName("gimpScreenAeFlag")]
      public TextWorkArea GimpScreenAeFlag
      {
        get => gimpScreenAeFlag ??= new();
        set => gimpScreenAeFlag = value;
      }

      /// <summary>
      /// A value of GimpScreenCsFlag.
      /// </summary>
      [JsonPropertyName("gimpScreenCsFlag")]
      public TextWorkArea GimpScreenCsFlag
      {
        get => gimpScreenCsFlag ??= new();
        set => gimpScreenCsFlag = value;
      }

      /// <summary>
      /// A value of GimpScreenFaFlag.
      /// </summary>
      [JsonPropertyName("gimpScreenFaFlag")]
      public TextWorkArea GimpScreenFaFlag
      {
        get => gimpScreenFaFlag ??= new();
        set => gimpScreenFaFlag = value;
      }

      /// <summary>
      /// A value of GimpScreenKmFlag.
      /// </summary>
      [JsonPropertyName("gimpScreenKmFlag")]
      public TextWorkArea GimpScreenKmFlag
      {
        get => gimpScreenKmFlag ??= new();
        set => gimpScreenKmFlag = value;
      }

      /// <summary>
      /// A value of GimpScreenPreferredId.
      /// </summary>
      [JsonPropertyName("gimpScreenPreferredId")]
      public CsePersonsWorkSet GimpScreenPreferredId
      {
        get => gimpScreenPreferredId ??= new();
        set => gimpScreenPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common gimpScreenSel;
      private CsePersonsWorkSet gimpScreenPersonInfo;
      private TextWorkArea gimpScreenAeFlag;
      private TextWorkArea gimpScreenCsFlag;
      private TextWorkArea gimpScreenFaFlag;
      private TextWorkArea gimpScreenKmFlag;
      private CsePersonsWorkSet gimpScreenPreferredId;
    }

    /// <summary>
    /// Gets a value of HiddenInputIds.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenInputIdsGroup> HiddenInputIds => hiddenInputIds ??= new(
      HiddenInputIdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenInputIds for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenInputIds")]
    [Computed]
    public IList<HiddenInputIdsGroup> HiddenInputIds_Json
    {
      get => hiddenInputIds;
      set => HiddenInputIds.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ids.
    /// </summary>
    [JsonIgnore]
    public Array<IdsGroup> Ids => ids ??= new(IdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ids for json serialization.
    /// </summary>
    [JsonPropertyName("ids")]
    [Computed]
    public IList<IdsGroup> Ids_Json
    {
      get => ids;
      set => Ids.Assign(value);
    }

    /// <summary>
    /// Gets a value of ScreenList.
    /// </summary>
    [JsonIgnore]
    public Array<ScreenListGroup> ScreenList => screenList ??= new(
      ScreenListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScreenList for json serialization.
    /// </summary>
    [JsonPropertyName("screenList")]
    [Computed]
    public IList<ScreenListGroup> ScreenList_Json
    {
      get => screenList;
      set => ScreenList.Assign(value);
    }

    private Array<HiddenInputIdsGroup> hiddenInputIds;
    private Array<IdsGroup> ids;
    private Array<ScreenListGroup> screenList;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenInputIdsGroup group.</summary>
    [Serializable]
    public class HiddenInputIdsGroup
    {
      /// <summary>
      /// A value of GexpHiddenInputIds.
      /// </summary>
      [JsonPropertyName("gexpHiddenInputIds")]
      public CsePersonsWorkSet GexpHiddenInputIds
      {
        get => gexpHiddenInputIds ??= new();
        set => gexpHiddenInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gexpHiddenInputIds;
    }

    /// <summary>A InputIdsGroup group.</summary>
    [Serializable]
    public class InputIdsGroup
    {
      /// <summary>
      /// A value of GexpInputIds.
      /// </summary>
      [JsonPropertyName("gexpInputIds")]
      public CsePersonsWorkSet GexpInputIds
      {
        get => gexpInputIds ??= new();
        set => gexpInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gexpInputIds;
    }

    /// <summary>A ScreenListGroup group.</summary>
    [Serializable]
    public class ScreenListGroup
    {
      /// <summary>
      /// A value of GexpScreen.
      /// </summary>
      [JsonPropertyName("gexpScreen")]
      public Common GexpScreen
      {
        get => gexpScreen ??= new();
        set => gexpScreen = value;
      }

      /// <summary>
      /// A value of GexpScreenPersonInfo.
      /// </summary>
      [JsonPropertyName("gexpScreenPersonInfo")]
      public CsePersonsWorkSet GexpScreenPersonInfo
      {
        get => gexpScreenPersonInfo ??= new();
        set => gexpScreenPersonInfo = value;
      }

      /// <summary>
      /// A value of GexpScreenAeFlag.
      /// </summary>
      [JsonPropertyName("gexpScreenAeFlag")]
      public TextWorkArea GexpScreenAeFlag
      {
        get => gexpScreenAeFlag ??= new();
        set => gexpScreenAeFlag = value;
      }

      /// <summary>
      /// A value of GexpScreenCsFlag.
      /// </summary>
      [JsonPropertyName("gexpScreenCsFlag")]
      public TextWorkArea GexpScreenCsFlag
      {
        get => gexpScreenCsFlag ??= new();
        set => gexpScreenCsFlag = value;
      }

      /// <summary>
      /// A value of GexpScreenFaFlag.
      /// </summary>
      [JsonPropertyName("gexpScreenFaFlag")]
      public TextWorkArea GexpScreenFaFlag
      {
        get => gexpScreenFaFlag ??= new();
        set => gexpScreenFaFlag = value;
      }

      /// <summary>
      /// A value of GexpScreenKmFlag.
      /// </summary>
      [JsonPropertyName("gexpScreenKmFlag")]
      public TextWorkArea GexpScreenKmFlag
      {
        get => gexpScreenKmFlag ??= new();
        set => gexpScreenKmFlag = value;
      }

      /// <summary>
      /// A value of GexpScreenPreferredId.
      /// </summary>
      [JsonPropertyName("gexpScreenPreferredId")]
      public CsePersonsWorkSet GexpScreenPreferredId
      {
        get => gexpScreenPreferredId ??= new();
        set => gexpScreenPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common gexpScreen;
      private CsePersonsWorkSet gexpScreenPersonInfo;
      private TextWorkArea gexpScreenAeFlag;
      private TextWorkArea gexpScreenCsFlag;
      private TextWorkArea gexpScreenFaFlag;
      private TextWorkArea gexpScreenKmFlag;
      private CsePersonsWorkSet gexpScreenPreferredId;
    }

    /// <summary>
    /// Gets a value of HiddenInputIds.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenInputIdsGroup> HiddenInputIds => hiddenInputIds ??= new(
      HiddenInputIdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenInputIds for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenInputIds")]
    [Computed]
    public IList<HiddenInputIdsGroup> HiddenInputIds_Json
    {
      get => hiddenInputIds;
      set => HiddenInputIds.Assign(value);
    }

    /// <summary>
    /// Gets a value of InputIds.
    /// </summary>
    [JsonIgnore]
    public Array<InputIdsGroup> InputIds => inputIds ??= new(
      InputIdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of InputIds for json serialization.
    /// </summary>
    [JsonPropertyName("inputIds")]
    [Computed]
    public IList<InputIdsGroup> InputIds_Json
    {
      get => inputIds;
      set => InputIds.Assign(value);
    }

    /// <summary>
    /// Gets a value of ScreenList.
    /// </summary>
    [JsonIgnore]
    public Array<ScreenListGroup> ScreenList => screenList ??= new(
      ScreenListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScreenList for json serialization.
    /// </summary>
    [JsonPropertyName("screenList")]
    [Computed]
    public IList<ScreenListGroup> ScreenList_Json
    {
      get => screenList;
      set => ScreenList.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public CsePersonsWorkSet Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of FromRegi.
    /// </summary>
    [JsonPropertyName("fromRegi")]
    public Common FromRegi
    {
      get => fromRegi ??= new();
      set => fromRegi = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    private Array<HiddenInputIdsGroup> hiddenInputIds;
    private Array<InputIdsGroup> inputIds;
    private Array<ScreenListGroup> screenList;
    private CsePersonsWorkSet hidden;
    private Common fromRegi;
    private Common fromPar1;
    private Common fromIapi;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ScreenListGroup group.</summary>
    [Serializable]
    public class ScreenListGroup
    {
      /// <summary>
      /// A value of GlclScreen.
      /// </summary>
      [JsonPropertyName("glclScreen")]
      public Common GlclScreen
      {
        get => glclScreen ??= new();
        set => glclScreen = value;
      }

      /// <summary>
      /// A value of GlclScreenPersonInfo.
      /// </summary>
      [JsonPropertyName("glclScreenPersonInfo")]
      public CsePersonsWorkSet GlclScreenPersonInfo
      {
        get => glclScreenPersonInfo ??= new();
        set => glclScreenPersonInfo = value;
      }

      /// <summary>
      /// A value of GlclScreenAe.
      /// </summary>
      [JsonPropertyName("glclScreenAe")]
      public Common GlclScreenAe
      {
        get => glclScreenAe ??= new();
        set => glclScreenAe = value;
      }

      /// <summary>
      /// A value of GlclScreenCs.
      /// </summary>
      [JsonPropertyName("glclScreenCs")]
      public Common GlclScreenCs
      {
        get => glclScreenCs ??= new();
        set => glclScreenCs = value;
      }

      /// <summary>
      /// A value of GlclScreenFa.
      /// </summary>
      [JsonPropertyName("glclScreenFa")]
      public Common GlclScreenFa
      {
        get => glclScreenFa ??= new();
        set => glclScreenFa = value;
      }

      /// <summary>
      /// A value of GlclScreenKm.
      /// </summary>
      [JsonPropertyName("glclScreenKm")]
      public Common GlclScreenKm
      {
        get => glclScreenKm ??= new();
        set => glclScreenKm = value;
      }

      /// <summary>
      /// A value of GlclScreenPreferredId.
      /// </summary>
      [JsonPropertyName("glclScreenPreferredId")]
      public CsePersonsWorkSet GlclScreenPreferredId
      {
        get => glclScreenPreferredId ??= new();
        set => glclScreenPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common glclScreen;
      private CsePersonsWorkSet glclScreenPersonInfo;
      private Common glclScreenAe;
      private Common glclScreenCs;
      private Common glclScreenFa;
      private Common glclScreenKm;
      private CsePersonsWorkSet glclScreenPreferredId;
    }

    /// <summary>A InputIdsGroup group.</summary>
    [Serializable]
    public class InputIdsGroup
    {
      /// <summary>
      /// A value of GlocalInputIds.
      /// </summary>
      [JsonPropertyName("glocalInputIds")]
      public CsePersonsWorkSet GlocalInputIds
      {
        get => glocalInputIds ??= new();
        set => glocalInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet glocalInputIds;
    }

    /// <summary>
    /// A value of SecurityBypassFlg.
    /// </summary>
    [JsonPropertyName("securityBypassFlg")]
    public Common SecurityBypassFlg
    {
      get => securityBypassFlg ??= new();
      set => securityBypassFlg = value;
    }

    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
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
    /// A value of SyncClientNfReadError.
    /// </summary>
    [JsonPropertyName("syncClientNfReadError")]
    public Common SyncClientNfReadError
    {
      get => syncClientNfReadError ??= new();
      set => syncClientNfReadError = value;
    }

    /// <summary>
    /// A value of InvalidPreferredIdFlag.
    /// </summary>
    [JsonPropertyName("invalidPreferredIdFlag")]
    public Common InvalidPreferredIdFlag
    {
      get => invalidPreferredIdFlag ??= new();
      set => invalidPreferredIdFlag = value;
    }

    /// <summary>
    /// A value of FirstTimeFlag.
    /// </summary>
    [JsonPropertyName("firstTimeFlag")]
    public Common FirstTimeFlag
    {
      get => firstTimeFlag ??= new();
      set => firstTimeFlag = value;
    }

    /// <summary>
    /// A value of InvalidPNSValue.
    /// </summary>
    [JsonPropertyName("invalidPNSValue")]
    public Common InvalidPNSValue
    {
      get => invalidPNSValue ??= new();
      set => invalidPNSValue = value;
    }

    /// <summary>
    /// A value of SelectionMadeFlag.
    /// </summary>
    [JsonPropertyName("selectionMadeFlag")]
    public Common SelectionMadeFlag
    {
      get => selectionMadeFlag ??= new();
      set => selectionMadeFlag = value;
    }

    /// <summary>
    /// A value of ScreenValueErrorFound.
    /// </summary>
    [JsonPropertyName("screenValueErrorFound")]
    public Common ScreenValueErrorFound
    {
      get => screenValueErrorFound ??= new();
      set => screenValueErrorFound = value;
    }

    /// <summary>
    /// A value of ImportLastSubscript.
    /// </summary>
    [JsonPropertyName("importLastSubscript")]
    public Common ImportLastSubscript
    {
      get => importLastSubscript ??= new();
      set => importLastSubscript = value;
    }

    /// <summary>
    /// A value of NfErrorFoundFlag.
    /// </summary>
    [JsonPropertyName("nfErrorFoundFlag")]
    public Common NfErrorFoundFlag
    {
      get => nfErrorFoundFlag ??= new();
      set => nfErrorFoundFlag = value;
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
    /// A value of ErrorFlag.
    /// </summary>
    [JsonPropertyName("errorFlag")]
    public Common ErrorFlag
    {
      get => errorFlag ??= new();
      set => errorFlag = value;
    }

    /// <summary>
    /// A value of EabDmlFlag.
    /// </summary>
    [JsonPropertyName("eabDmlFlag")]
    public Common EabDmlFlag
    {
      get => eabDmlFlag ??= new();
      set => eabDmlFlag = value;
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
    /// Gets a value of ScreenList.
    /// </summary>
    [JsonIgnore]
    public Array<ScreenListGroup> ScreenList => screenList ??= new(
      ScreenListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScreenList for json serialization.
    /// </summary>
    [JsonPropertyName("screenList")]
    [Computed]
    public IList<ScreenListGroup> ScreenList_Json
    {
      get => screenList;
      set => ScreenList.Assign(value);
    }

    /// <summary>
    /// A value of DmlReturnCode.
    /// </summary>
    [JsonPropertyName("dmlReturnCode")]
    public Common DmlReturnCode
    {
      get => dmlReturnCode ??= new();
      set => dmlReturnCode = value;
    }

    /// <summary>
    /// A value of PreferredId.
    /// </summary>
    [JsonPropertyName("preferredId")]
    public CsePersonsWorkSet PreferredId
    {
      get => preferredId ??= new();
      set => preferredId = value;
    }

    /// <summary>
    /// A value of NextSubscript.
    /// </summary>
    [JsonPropertyName("nextSubscript")]
    public Common NextSubscript
    {
      get => nextSubscript ??= new();
      set => nextSubscript = value;
    }

    /// <summary>
    /// Gets a value of InputIds.
    /// </summary>
    [JsonIgnore]
    public Array<InputIdsGroup> InputIds => inputIds ??= new(
      InputIdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of InputIds for json serialization.
    /// </summary>
    [JsonPropertyName("inputIds")]
    [Computed]
    public IList<InputIdsGroup> InputIds_Json
    {
      get => inputIds;
      set => InputIds.Assign(value);
    }

    /// <summary>
    /// A value of SystemFlagError.
    /// </summary>
    [JsonPropertyName("systemFlagError")]
    public Common SystemFlagError
    {
      get => systemFlagError ??= new();
      set => systemFlagError = value;
    }

    private Common securityBypassFlg;
    private Transaction transaction;
    private ServiceProvider serviceProvider;
    private Common syncClientNfReadError;
    private Common invalidPreferredIdFlag;
    private Common firstTimeFlag;
    private Common invalidPNSValue;
    private Common selectionMadeFlag;
    private Common screenValueErrorFound;
    private Common importLastSubscript;
    private Common nfErrorFoundFlag;
    private DateWorkArea current;
    private Common errorFlag;
    private Common eabDmlFlag;
    private Common common;
    private Array<ScreenListGroup> screenList;
    private Common dmlReturnCode;
    private CsePersonsWorkSet preferredId;
    private Common nextSubscript;
    private Array<InputIdsGroup> inputIds;
    private Common systemFlagError;
  }
#endregion
}
