// Program: SI_RAGE_REGISTERED_AGENT_MAINT, ID: 371767407, model: 746.
// Short name: SWERAGEP
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
/// A program: SI_RAGE_REGISTERED_AGENT_MAINT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRageRegisteredAgentMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RAGE_REGISTERED_AGENT_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRageRegisteredAgentMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRageRegisteredAgentMaint.
  /// </summary>
  public SiRageRegisteredAgentMaint(IContext context, Import import,
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
    //     M A I N T E N A N C E    L O G
    // Date	  Developer		Description
    // 09-24-95  K Evans		Initial Development
    // 02-06-96  P.Elie		Retrofit Security/Next Tran
    // 11/03/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // ------------------------------------------------------------
    // 10/29/98 W. Campbell      Modified an IF stmt so
    //                           that it will only be true for
    //                           commands: DISPLAY, NEXT
    //                           and PREV.  This allows other
    //                           commands to bypass the IF statement.
    // ------------------------------------------------------------
    // 10/29/98 W. Campbell      Assigned function keys
    //                           13-24 the command of INVALID.
    // ------------------------------------------------------------
    // 05/26/99 W.Campbell        Replaced zd exit states.
    // -------------------------------------------------------------------
    // 04/25/01  Madhu Kumar    Added zip code validations
    //                          for 4 digit and 5 digit zip codes .
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Next.Number = import.Next.Number;
    export.SearchRegisteredAgent.Name = import.SearchRegisteredAgent.Name;
    MoveRegisteredAgentAddress3(import.SearchRegisteredAgentAddress,
      export.SearchRegisteredAgentAddress);
    export.Minus.OneChar = import.Minus.OneChar;
    export.Plus.OneChar = import.Plus.OneChar;
    export.SearchRegisteredAgentAddress.State = "KS";

    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GdetailRegisteredAgent.Assign(
          import.Group.Item.GdetailRegisteredAgent);
        export.Group.Update.GdetailRegisteredAgentAddress.Assign(
          import.Group.Item.GdetailRegisteredAgentAddress);
        export.Group.Update.GdetailPrompt.Flag =
          import.Group.Item.GdetailPrompt.Flag;
        export.Group.Update.GdetailCommon.SelectChar =
          import.Group.Item.GdetailCommon.SelectChar;

        switch(AsChar(import.Group.Item.GdetailCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.RtnRegisteredAgent.Assign(
              import.Group.Item.GdetailRegisteredAgent);
            export.RtnRegisteredAgentAddress.Assign(
              import.Group.Item.GdetailRegisteredAgentAddress);
            ++local.Select.Count;

            break;
          case '*':
            export.Group.Update.GdetailCommon.SelectChar = "";

            break;
          default:
            var field = GetField(export.Group.Item.GdetailCommon, "selectChar");

            field.Error = true;

            ++local.Select.Count;
            ++local.Error.Count;

            break;
        }
      }

      import.Group.CheckIndex();
    }

    if (!import.PageKeys.IsEmpty)
    {
      for(import.PageKeys.Index = 0; import.PageKeys.Index < import
        .PageKeys.Count; ++import.PageKeys.Index)
      {
        if (!import.PageKeys.CheckSize())
        {
          break;
        }

        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        MoveRegisteredAgent(import.PageKeys.Item.GpageKeyRegisteredAgent,
          export.PageKeys.Update.GpageKeyRegisteredAgent);
        MoveRegisteredAgentAddress2(import.PageKeys.Item.
          GpageKeyRegisteredAgentAddress,
          export.PageKeys.Update.GpageKeyRegisteredAgentAddress);
      }

      import.PageKeys.CheckIndex();
    }

    export.Standard.Assign(import.Standard);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.Hidden.CaseNumber = export.Next.Number;
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
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "RTLIST"))
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

    if (local.Select.Count > 0 && (Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Error.Count > 0)
    {
      // ---------------------------------------------
      // 05/26/99 W.Campbell - Replaced zd exit states.
      // ---------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    // ----------------------------------------------------
    // 10/29/98 W. Campbell  Modified the following
    // IF stmt so that it will only be true for commands:
    // DISPLAY, NEXT and PREV.  This allows other
    // commands to bypass the IF statement.
    // ----------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      if (IsEmpty(import.SearchRegisteredAgent.Name))
      {
        var field = GetField(export.SearchRegisteredAgent, "name");

        field.Error = true;

        ExitState = "ACO_NI0000_SEARCH_CRITERIA_REQD";

        return;
      }
    }

    // *********************************************
    // Screen validations
    // *********************************************
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
      {
        if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
        {
          if (Length(TrimEnd(
            export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5)) > 0 && Length
            (TrimEnd(export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5)) <
            5)
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress,
              "zipCode5");

            field.Error = true;

            ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

            return;
          }

          if (Length(TrimEnd(
            export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5)) > 0 && Verify
            (export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5,
            "0123456789") != 0)
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress,
              "zipCode5");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }

          if (Length(TrimEnd(
            export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5)) == 0 && Length
            (TrimEnd(export.Group.Item.GdetailRegisteredAgentAddress.ZipCode4)) >
            0)
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress,
              "zipCode5");

            field.Error = true;

            ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

            return;
          }

          if (Length(TrimEnd(
            export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5)) > 0 && Length
            (TrimEnd(export.Group.Item.GdetailRegisteredAgentAddress.ZipCode4)) >
            0)
          {
            if (Length(TrimEnd(
              export.Group.Item.GdetailRegisteredAgentAddress.ZipCode4)) < 4)
            {
              var field =
                GetField(export.Group.Item.GdetailRegisteredAgentAddress,
                "zipCode4");

              field.Error = true;

              ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

              return;
            }
            else if (Verify(export.Group.Item.GdetailRegisteredAgentAddress.
              ZipCode4, "0123456789") != 0)
            {
              var field =
                GetField(export.Group.Item.GdetailRegisteredAgentAddress,
                "zipCode4");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }
          }

          if (IsEmpty(export.Group.Item.GdetailRegisteredAgent.Name))
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgent, "name");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailRegisteredAgentAddress.Street1))
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress,
              "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailRegisteredAgentAddress.City))
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress, "city");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailRegisteredAgentAddress.State))
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress, "state");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailRegisteredAgentAddress.ZipCode5))
          {
            var field =
              GetField(export.Group.Item.GdetailRegisteredAgentAddress,
              "zipCode5");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    export.Group.CheckIndex();

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.SearchRegisteredAgent.Name = export.SearchRegisteredAgent.Name;
        MoveRegisteredAgentAddress3(export.SearchRegisteredAgentAddress,
          local.SearchRegisteredAgentAddress);
        local.Command.Command = global.Command;
        UseSiBuildRegisteredAgent();

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        export.Standard.PageNumber = 1;

        if (export.Group.IsFull)
        {
          export.Plus.OneChar = "+";

          export.PageKeys.Index = 0;
          export.PageKeys.CheckSize();

          export.Group.Index = 0;
          export.Group.CheckSize();

          if (!IsEmpty(export.SearchRegisteredAgent.Name))
          {
            export.PageKeys.Update.GpageKeyRegisteredAgent.Name =
              export.SearchRegisteredAgent.Name;
          }

          export.PageKeys.Update.GpageKeyRegisteredAgent.Identifier = "0";
          MoveRegisteredAgentAddress2(export.Group.Item.
            GdetailRegisteredAgentAddress,
            export.PageKeys.Update.GpageKeyRegisteredAgentAddress);

          ++export.PageKeys.Index;
          export.PageKeys.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 2;
          export.Group.CheckSize();

          MoveRegisteredAgent(export.Group.Item.GdetailRegisteredAgent,
            export.PageKeys.Update.GpageKeyRegisteredAgent);
          MoveRegisteredAgentAddress2(export.Group.Item.
            GdetailRegisteredAgentAddress,
            export.PageKeys.Update.GpageKeyRegisteredAgentAddress);
        }
        else
        {
          export.Plus.OneChar = "";
        }

        export.Minus.OneChar = "";

        break;
      case "NEXT":
        if (import.PageKeys.IsFull)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        if (IsEmpty(import.Plus.OneChar))
        {
          // ---------------------------------------------
          // 05/26/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Standard.PageNumber;

        export.PageKeys.Index = export.Standard.PageNumber - 1;
        export.PageKeys.CheckSize();

        MoveRegisteredAgent(export.PageKeys.Item.GpageKeyRegisteredAgent,
          local.SearchRegisteredAgent);
        MoveRegisteredAgentAddress2(export.PageKeys.Item.
          GpageKeyRegisteredAgentAddress, local.SearchRegisteredAgentAddress);
        local.Command.Command = global.Command;
        UseSiBuildRegisteredAgent();

        if (export.Group.IsFull)
        {
          export.Plus.OneChar = "+";

          ++export.PageKeys.Index;
          export.PageKeys.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 2;
          export.Group.CheckSize();

          MoveRegisteredAgent(export.Group.Item.GdetailRegisteredAgent,
            export.PageKeys.Update.GpageKeyRegisteredAgent);
          MoveRegisteredAgentAddress2(export.Group.Item.
            GdetailRegisteredAgentAddress,
            export.PageKeys.Update.GpageKeyRegisteredAgentAddress);
        }
        else
        {
          export.Plus.OneChar = "";
        }

        export.Minus.OneChar = "-";

        break;
      case "PREV":
        if (IsEmpty(import.Minus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Standard.PageNumber;

        export.PageKeys.Index = export.Standard.PageNumber - 1;
        export.PageKeys.CheckSize();

        MoveRegisteredAgent(export.PageKeys.Item.GpageKeyRegisteredAgent,
          local.SearchRegisteredAgent);
        MoveRegisteredAgentAddress2(export.PageKeys.Item.
          GpageKeyRegisteredAgentAddress, local.SearchRegisteredAgentAddress);
        local.Command.Command = global.Command;
        UseSiBuildRegisteredAgent();

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.OneChar = "-";
        }
        else
        {
          export.Minus.OneChar = "";
        }

        export.Plus.OneChar = "+";

        break;
      case "ADD":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (!Equal(export.Group.Item.GdetailRegisteredAgentAddress.State,
              "KS"))
            {
              var field =
                GetField(export.Group.Item.GdetailRegisteredAgentAddress,
                "state");

              field.Error = true;

              ExitState = "STATE_MUST_BE_KS";

              return;
            }

            local.ControlTable.Identifier = "REGISTERED AGENT";
            UseAccessControlTable();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              return;
            }

            export.Group.Update.GdetailRegisteredAgent.Identifier =
              NumberToString(local.ControlTable.LastUsedNumber, 9);
            UseSiCreateRegisteredAgent();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            local.ControlTable.Identifier = "REGISTERED AGENT ADDR";
            UseAccessControlTable();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              return;
            }

            export.Group.Update.GdetailRegisteredAgentAddress.Identifier =
              NumberToString(local.ControlTable.LastUsedNumber, 9);
            UseSiCreateRegisteredAgentAddr();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }

        export.Group.CheckIndex();

        break;
      case "UPDATE":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (!Equal(export.Group.Item.GdetailRegisteredAgentAddress.State,
              "KS"))
            {
              var field =
                GetField(export.Group.Item.GdetailRegisteredAgentAddress,
                "state");

              field.Error = true;

              ExitState = "STATE_MUST_BE_KS";

              return;
            }

            UseSiUpdateRegisteredAgent();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            UseSiUpdateRegisteredAgentAddr();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }

        export.Group.CheckIndex();

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
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

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            UseSiDeleteRegisteredAgent();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";

            // ---------------------------------------------
            // 05/26/99 W.Campbell - Replaced zd exit states.
            // ---------------------------------------------
            ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
          }
        }

        export.Group.CheckIndex();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.GdetailPrompt.Flag) == 'S')
            {
              export.Code.CodeName = "STATE CODE";
              ExitState = "ECO_LNK_TO_CODE_TABLES";

              return;
            }
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "RTLIST":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.GdetailPrompt.Flag) == 'S')
            {
              export.Group.Update.GdetailPrompt.Flag = "";

              var field = GetField(export.Group.Item.GdetailPrompt, "flag");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.CodeValue.Cdvalue))
              {
                export.Group.Update.GdetailRegisteredAgentAddress.State =
                  import.CodeValue.Cdvalue;
              }

              return;
            }
          }
        }

        export.Group.CheckIndex();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveGroup(SiBuildRegisteredAgent.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.GdetailCommon.SelectChar = source.GdetailCommon.SelectChar;
    target.GdetailRegisteredAgent.Assign(source.GdetailRegisteredAgent);
    target.GdetailRegisteredAgentAddress.Assign(
      source.GdetailRegisteredAgentAddress);
    target.GdetailPrompt.Flag = source.GdetailPrompt.Flag;
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

  private static void MoveRegisteredAgent(RegisteredAgent source,
    RegisteredAgent target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveRegisteredAgentAddress1(RegisteredAgentAddress source,
    RegisteredAgentAddress target)
  {
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
  }

  private static void MoveRegisteredAgentAddress2(RegisteredAgentAddress source,
    RegisteredAgentAddress target)
  {
    target.Identifier = source.Identifier;
    target.City = source.City;
  }

  private static void MoveRegisteredAgentAddress3(RegisteredAgentAddress source,
    RegisteredAgentAddress target)
  {
    target.City = source.City;
    target.State = source.State;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

  private void UseSiBuildRegisteredAgent()
  {
    var useImport = new SiBuildRegisteredAgent.Import();
    var useExport = new SiBuildRegisteredAgent.Export();

    useImport.RegisteredAgentAddress.City =
      local.SearchRegisteredAgentAddress.City;
    MoveRegisteredAgent(local.SearchRegisteredAgent, useImport.RegisteredAgent);
    useImport.Common.Command = local.Command.Command;
    useImport.SearchRegisteredAgentAddress.City =
      export.SearchRegisteredAgentAddress.City;
    useImport.SearchRegisteredAgent.Name = export.SearchRegisteredAgent.Name;

    Call(SiBuildRegisteredAgent.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSiCreateRegisteredAgent()
  {
    var useImport = new SiCreateRegisteredAgent.Import();
    var useExport = new SiCreateRegisteredAgent.Export();

    useImport.RegisteredAgent.Assign(export.Group.Item.GdetailRegisteredAgent);

    Call(SiCreateRegisteredAgent.Execute, useImport, useExport);
  }

  private void UseSiCreateRegisteredAgentAddr()
  {
    var useImport = new SiCreateRegisteredAgentAddr.Import();
    var useExport = new SiCreateRegisteredAgentAddr.Export();

    useImport.RegisteredAgent.Identifier =
      export.Group.Item.GdetailRegisteredAgent.Identifier;
    MoveRegisteredAgentAddress1(export.Group.Item.GdetailRegisteredAgentAddress,
      useImport.RegisteredAgentAddress);

    Call(SiCreateRegisteredAgentAddr.Execute, useImport, useExport);
  }

  private void UseSiDeleteRegisteredAgent()
  {
    var useImport = new SiDeleteRegisteredAgent.Import();
    var useExport = new SiDeleteRegisteredAgent.Export();

    useImport.RegisteredAgent.Identifier =
      export.Group.Item.GdetailRegisteredAgent.Identifier;

    Call(SiDeleteRegisteredAgent.Execute, useImport, useExport);
  }

  private void UseSiUpdateRegisteredAgent()
  {
    var useImport = new SiUpdateRegisteredAgent.Import();
    var useExport = new SiUpdateRegisteredAgent.Export();

    useImport.RegisteredAgent.Assign(export.Group.Item.GdetailRegisteredAgent);

    Call(SiUpdateRegisteredAgent.Execute, useImport, useExport);
  }

  private void UseSiUpdateRegisteredAgentAddr()
  {
    var useImport = new SiUpdateRegisteredAgentAddr.Import();
    var useExport = new SiUpdateRegisteredAgentAddr.Export();

    useImport.RegisteredAgent.Identifier =
      export.Group.Item.GdetailRegisteredAgent.Identifier;
    MoveRegisteredAgentAddress1(export.Group.Item.GdetailRegisteredAgentAddress,
      useImport.RegisteredAgentAddress);

    Call(SiUpdateRegisteredAgentAddr.Execute, useImport, useExport);
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
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgent.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgent")]
      public RegisteredAgent GdetailRegisteredAgent
      {
        get => gdetailRegisteredAgent ??= new();
        set => gdetailRegisteredAgent = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgentAddress.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgentAddress")]
      public RegisteredAgentAddress GdetailRegisteredAgentAddress
      {
        get => gdetailRegisteredAgentAddress ??= new();
        set => gdetailRegisteredAgentAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gdetailCommon;
      private RegisteredAgent gdetailRegisteredAgent;
      private RegisteredAgentAddress gdetailRegisteredAgentAddress;
      private Common gdetailPrompt;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of GpageKeyRegisteredAgentAddress.
      /// </summary>
      [JsonPropertyName("gpageKeyRegisteredAgentAddress")]
      public RegisteredAgentAddress GpageKeyRegisteredAgentAddress
      {
        get => gpageKeyRegisteredAgentAddress ??= new();
        set => gpageKeyRegisteredAgentAddress = value;
      }

      /// <summary>
      /// A value of GpageKeyRegisteredAgent.
      /// </summary>
      [JsonPropertyName("gpageKeyRegisteredAgent")]
      public RegisteredAgent GpageKeyRegisteredAgent
      {
        get => gpageKeyRegisteredAgent ??= new();
        set => gpageKeyRegisteredAgent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private RegisteredAgentAddress gpageKeyRegisteredAgentAddress;
      private RegisteredAgent gpageKeyRegisteredAgent;
    }

    /// <summary>
    /// A value of SearchRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgentAddress")]
    public RegisteredAgentAddress SearchRegisteredAgentAddress
    {
      get => searchRegisteredAgentAddress ??= new();
      set => searchRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgent.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgent")]
    public RegisteredAgent SearchRegisteredAgent
    {
      get => searchRegisteredAgent ??= new();
      set => searchRegisteredAgent = value;
    }

    /// <summary>
    /// A value of RtnRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("rtnRegisteredAgentAddress")]
    public RegisteredAgentAddress RtnRegisteredAgentAddress
    {
      get => rtnRegisteredAgentAddress ??= new();
      set => rtnRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of RtnRegisteredAgent.
    /// </summary>
    [JsonPropertyName("rtnRegisteredAgent")]
    public RegisteredAgent RtnRegisteredAgent
    {
      get => rtnRegisteredAgent ??= new();
      set => rtnRegisteredAgent = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
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

    private RegisteredAgentAddress searchRegisteredAgentAddress;
    private RegisteredAgent searchRegisteredAgent;
    private RegisteredAgentAddress rtnRegisteredAgentAddress;
    private RegisteredAgent rtnRegisteredAgent;
    private CodeValue codeValue;
    private Code code;
    private Standard minus;
    private Standard plus;
    private Case1 next;
    private Standard standard;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private NextTranInfo hidden;
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
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgent.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgent")]
      public RegisteredAgent GdetailRegisteredAgent
      {
        get => gdetailRegisteredAgent ??= new();
        set => gdetailRegisteredAgent = value;
      }

      /// <summary>
      /// A value of GdetailRegisteredAgentAddress.
      /// </summary>
      [JsonPropertyName("gdetailRegisteredAgentAddress")]
      public RegisteredAgentAddress GdetailRegisteredAgentAddress
      {
        get => gdetailRegisteredAgentAddress ??= new();
        set => gdetailRegisteredAgentAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gdetailCommon;
      private RegisteredAgent gdetailRegisteredAgent;
      private RegisteredAgentAddress gdetailRegisteredAgentAddress;
      private Common gdetailPrompt;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of GpageKeyRegisteredAgentAddress.
      /// </summary>
      [JsonPropertyName("gpageKeyRegisteredAgentAddress")]
      public RegisteredAgentAddress GpageKeyRegisteredAgentAddress
      {
        get => gpageKeyRegisteredAgentAddress ??= new();
        set => gpageKeyRegisteredAgentAddress = value;
      }

      /// <summary>
      /// A value of GpageKeyRegisteredAgent.
      /// </summary>
      [JsonPropertyName("gpageKeyRegisteredAgent")]
      public RegisteredAgent GpageKeyRegisteredAgent
      {
        get => gpageKeyRegisteredAgent ??= new();
        set => gpageKeyRegisteredAgent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private RegisteredAgentAddress gpageKeyRegisteredAgentAddress;
      private RegisteredAgent gpageKeyRegisteredAgent;
    }

    /// <summary>
    /// A value of SearchRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgentAddress")]
    public RegisteredAgentAddress SearchRegisteredAgentAddress
    {
      get => searchRegisteredAgentAddress ??= new();
      set => searchRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgent.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgent")]
    public RegisteredAgent SearchRegisteredAgent
    {
      get => searchRegisteredAgent ??= new();
      set => searchRegisteredAgent = value;
    }

    /// <summary>
    /// A value of RtnRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("rtnRegisteredAgentAddress")]
    public RegisteredAgentAddress RtnRegisteredAgentAddress
    {
      get => rtnRegisteredAgentAddress ??= new();
      set => rtnRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of RtnRegisteredAgent.
    /// </summary>
    [JsonPropertyName("rtnRegisteredAgent")]
    public RegisteredAgent RtnRegisteredAgent
    {
      get => rtnRegisteredAgent ??= new();
      set => rtnRegisteredAgent = value;
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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
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

    private RegisteredAgentAddress searchRegisteredAgentAddress;
    private RegisteredAgent searchRegisteredAgent;
    private RegisteredAgentAddress rtnRegisteredAgentAddress;
    private RegisteredAgent rtnRegisteredAgent;
    private CodeValue codeValue;
    private Code code;
    private Standard minus;
    private Standard plus;
    private Case1 next;
    private Standard standard;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgentAddress")]
    public RegisteredAgentAddress SearchRegisteredAgentAddress
    {
      get => searchRegisteredAgentAddress ??= new();
      set => searchRegisteredAgentAddress = value;
    }

    /// <summary>
    /// A value of SearchRegisteredAgent.
    /// </summary>
    [JsonPropertyName("searchRegisteredAgent")]
    public RegisteredAgent SearchRegisteredAgent
    {
      get => searchRegisteredAgent ??= new();
      set => searchRegisteredAgent = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    private ControlTable controlTable;
    private RegisteredAgentAddress searchRegisteredAgentAddress;
    private RegisteredAgent searchRegisteredAgent;
    private Common command;
    private Employer searchEmployer;
    private EmployerAddress searchEmployerAddress;
    private Common error;
    private Common select;
  }
#endregion
}
