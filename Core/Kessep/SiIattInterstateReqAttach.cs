// Program: SI_IATT_INTERSTATE_REQ_ATTACH, ID: 372381086, model: 746.
// Short name: SWEIATTP
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
/// A program: SI_IATT_INTERSTATE_REQ_ATTACH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIattInterstateReqAttach: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_INTERSTATE_REQ_ATTACH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattInterstateReqAttach(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattInterstateReqAttach.
  /// </summary>
  public SiIattInterstateReqAttach(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    //                       M A I N T E N A N C E     L O G
    //   Date    Developer   WR#/PR#   Description
    // ---------------------------------------------------------------------------------------
    // 	  Sid Chowdhary		Initial Development
    // 11/04/96  G. Lofton		Add new security.
    // 01/31/97  Sid Chowdhary		Completion.
    // 03/24/99  C. Ott                Display all information on closed
    //                                 
    // cases.
    // 06/18/99  C. Ott                Add functionality to post
    //                                 
    // attachments received.
    // 12/10/99  C. Scroggins          Added handling for multiple
    //                                 
    // interstate requests - Added
    // prompt
    //                                 
    // for State field, made State
    // field
    //                                 
    // enterable, added validation for
    //                                 
    // value entered, added flow to
    // code
    //                                 
    // table for prompt selection,
    //                                 
    // and added appropriate exit
    // states
    //                                 
    // for error handling in the case
    // of
    //                                 
    // invalid values, State not
    // entered,
    //                                 
    // and multiple interstate
    //                                 
    // requests.
    // 12/23/99  C. Scroggins          Put in fixes for PR #1002.
    // ---------------------------------------------------------------------------------------
    // 12/05/00  M Lachowicz           Changed header line WR 298
    // 02/19/01  C Fairley  I00113227  Added read of fips to obtain the numeric 
    // State value
    // 05/30/01  C Fairley  I00120592  Added automatic flow to IREQ, when 
    // multiple Interstate
    //                                 
    // Requests exist and NO state has
    // been entered.
    //                                 
    // Removed OLD commented out code.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    UseOeCabSetMnemonics();

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Null1.Date = null;
    export.DisplayOnly.Number = import.DisplayOnly.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    export.Next.Number = import.Next.Number;
    MoveInterstateRequest2(import.InterstateRequest, export.InterstateRequest);
    export.State.State = import.State.State;
    MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.PromptAttachment.SelectChar = import.PromptAttachment.SelectChar;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.IattStatePrompt.SelectChar = import.IattStatePrompt.SelectChar;
    MoveFips(import.OtherState, export.OtherState);
    export.HiddenPf15Pressed.Flag = import.HiddenPf15Pressed.Flag;
    export.HiddenPf16Pressed.Flag = import.HiddenPf16Pressed.Flag;
    export.HiddenCase.Number = import.HiddenCase.Number;
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Select.SelectChar =
        import.Import1.Item.Select.SelectChar;
      export.Export1.Update.Details.Assign(import.Import1.Item.Details);

      if (!IsEmpty(export.Export1.Item.Select.SelectChar) && AsChar
        (export.Export1.Item.Select.SelectChar) != 'S' && AsChar
        (export.Export1.Item.Select.SelectChar) != '*')
      {
        var field = GetField(export.Export1.Item.Select, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
      }

      if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
      {
        ++local.Selected.Count;
      }

      if (Lt(local.Null1.Date, export.Export1.Item.Details.SentDate))
      {
        var field = GetField(export.Export1.Item.Details, "receivedDate");

        field.Color = "cyan";
        field.Intensity = Intensity.Dark;
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.Export1.Item.Details, "receivedDate");

        field.Color = "cyan";
        field.Intensity = Intensity.Normal;
        field.Protected = true;
      }

      export.Export1.Next();
    }

    if (!IsEmpty(export.Next.Number))
    {
      local.ZeroFill.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros();
      export.Next.Number = local.ZeroFill.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      global.Command = "DISPLAY";

      if (!IsEmpty(export.IattStatePrompt.SelectChar))
      {
        export.State.State = import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.State, "state");

        field.Protected = false;
        field.Focused = true;

        export.IattStatePrompt.SelectChar = "";

        if (!IsEmpty(export.State.State))
        {
          local.State.State = export.State.State;
          UseSiValidateStateFips();
          export.State.State = export.OtherState.StateAbbreviation;

          if (AsChar(local.FipsError.Flag) == 'Y')
          {
            var field1 = GetField(export.State, "state");

            field1.Error = true;

            return;
          }
        }

        export.IattStatePrompt.SelectChar = "";
      }
      else if (!IsEmpty(export.PromptAttachment.SelectChar))
      {
        import.DlgflwMultSelectn.Index = 0;
        import.DlgflwMultSelectn.CheckSize();

        export.PromptAttachment.SelectChar = "";

        export.Export1.Index = export.Export1.Count;
        export.Export1.CheckIndex();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (!IsEmpty(import.DlgflwMultSelectn.Item.DetailMultSelect.Cdvalue) &&
            !
            Equal(import.DlgflwMultSelectn.Item.DetailMultSelect.Cdvalue, "OTHR"))
            
          {
            export.Export1.Update.Select.SelectChar = "S";
            export.Export1.Update.Details.DataTypeCode =
              import.DlgflwMultSelectn.Item.DetailMultSelect.Cdvalue;
            export.Export1.Update.Details.Note =
              import.DlgflwMultSelectn.Item.DetailMultSelect.Description;

            var field = GetField(export.Export1.Item.Details, "receivedDate");

            field.Color = "cyan";
            field.Intensity = Intensity.Dark;
            field.Protected = true;
          }
          else if (Equal(import.DlgflwMultSelectn.Item.DetailMultSelect.Cdvalue,
            "OTHR"))
          {
            export.Export1.Update.Select.SelectChar = "S";
            export.Export1.Update.Details.DataTypeCode =
              import.DlgflwMultSelectn.Item.DetailMultSelect.Cdvalue;

            var field1 = GetField(export.Export1.Item.Details, "note");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
            field1.Focused = true;

            var field2 = GetField(export.Export1.Item.Details, "receivedDate");

            field2.Color = "cyan";
            field2.Intensity = Intensity.Dark;
            field2.Protected = true;
          }
          else
          {
            export.Export1.Next();

            return;
          }

          ++import.DlgflwMultSelectn.Index;
          import.DlgflwMultSelectn.CheckSize();

          export.Export1.Next();
        }
        while(!export.Export1.IsFull);

        return;
      }
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.ApCsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
        MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
      }

      export.PromptPerson.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.DisplayOnly.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp =
        export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Standard.NextTransaction = import.Standard.NextTransaction;

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
        export.ApCsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
        export.DisplayOnly.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.Next.Number = export.DisplayOnly.Number;
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y' && !
      Equal(global.Command, "SEND"))
    {
      export.HiddenPf15Pressed.Flag = "";
    }

    if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y' && !
      Equal(global.Command, "REQUEST"))
    {
      export.HiddenPf16Pressed.Flag = "";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "SEND"))
    {
      local.CommandSendOrRequest.Flag = "S";
      local.SendMsg.Text60 = "THE REQUESTED/REQUIRED DOCUMENTS WERE SENT :";

      if (!export.Export1.IsEmpty)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S' && Equal
            (export.Export1.Item.Details.DataTypeCode, "OTHR") && IsEmpty
            (export.Export1.Item.Details.Note))
          {
            var field = GetField(export.Export1.Item.Details, "note");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
            field.Focused = true;

            ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

            return;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Details.SentDate = Now().Date;
            export.Export1.Update.Details.RequestDate = local.Null1.Date;
            export.Export1.Update.Details.ReceivedDate = local.Null1.Date;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      global.Command = "ADD";
    }
    else if (Equal(global.Command, "REQUEST"))
    {
      local.CommandSendOrRequest.Flag = "R";
      local.RequestMsg.Text60 = "PLEASE SEND THE REQUESTED DOCUMENTS :";

      if (!export.Export1.IsEmpty)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S' && Equal
            (export.Export1.Item.Details.DataTypeCode, "OTHR") && IsEmpty
            (export.Export1.Item.Details.Note))
          {
            var field = GetField(export.Export1.Item.Details, "note");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
            field.Focused = true;

            ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

            return;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Details.RequestDate = Now().Date;
            export.Export1.Update.Details.SentDate = local.Null1.Date;
            export.Export1.Update.Details.ReceivedDate = local.Null1.Date;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      global.Command = "ADD";
    }
    else
    {
      local.CommandSendOrRequest.Flag = "";
    }

    if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y' && !
      Equal(global.Command, "ADD"))
    {
      export.HiddenPf15Pressed.Flag = "";
    }

    if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y' && !
      Equal(global.Command, "ADD"))
    {
      export.HiddenPf16Pressed.Flag = "";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "HELP":
        break;
      case "UPDATE":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field4 = GetField(export.DisplayOnly, "number");

          field4.Error = true;

          var field5 = GetField(export.Next, "number");

          field5.Error = true;

          ExitState = "SI0000_DISPLAY_REQD_FOR_IATT";

          return;
        }

        if (export.InterstateRequest.IntHGeneratedId == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              export.Export1.Update.Details.SentDate = local.Null1.Date;
              export.Export1.Update.Details.RequestDate = local.Null1.Date;
              export.Export1.Update.Details.ReceivedDate = local.Null1.Date;
            }
          }

          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "CASE_NOT_INTERSTATE";

          return;
        }

        export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
        local.Error.Flag = "Y";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            local.Error.Flag = "";

            if (Lt(local.Null1.Date, export.Export1.Item.Details.ReceivedDate))
            {
              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "SI0000_IATT_RQST_ALREADY_RECEIVD";
              }
            }

            if (Lt(local.Null1.Date, export.Export1.Item.Details.SentDate))
            {
              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "SI0000_IATT_INVALID_RECEIVE";
              }
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

          return;
        }

        if (!export.Export1.IsEmpty)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              MoveInterstateRequestAttachment(export.Export1.Item.Details,
                local.Update);
              local.Update.ReceivedDate = Now().Date;
              UseSiIattUpdateIsRequestAttach();

              if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                export.Export1.Update.Details.ReceivedDate =
                  local.Update.ReceivedDate;
                export.Export1.Update.Select.SelectChar = "*";
              }
              else
              {
                var field = GetField(export.Export1.Item.Select, "selectChar");

                field.Error = true;
              }
            }
          }
        }
        else
        {
          ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

          return;
        }

        break;
      case "DELETE":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field4 = GetField(export.DisplayOnly, "number");

          field4.Error = true;

          var field5 = GetField(export.Next, "number");

          field5.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (local.Selected.Count <= 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        UseSiIattDeleteIsRequestAttach();

        if (IsExitState("SI0000_IATT_DELETE_SUCCESSFUL"))
        {
          local.DeleteSuccessful.Flag = "Y";
          global.Command = "DISPLAY";
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Update.Details.Assign(local.Local1.Item.Details);
            export.Export1.Update.Select.SelectChar =
              local.Local1.Item.Select.SelectChar;

            if (AsChar(local.Local1.Item.Select.SelectChar) == 'E')
            {
              export.Export1.Update.Select.SelectChar = "S";

              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;
            }

            export.Export1.Next();
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DISPLAY":
        // *** Problem report I00113227
        // *** 02/19/01 swsrchf
        // *** start
        if (!IsEmpty(export.State.State))
        {
          local.State.State = export.State.State;
          UseSiValidateStateFips();

          if (AsChar(local.FipsError.Flag) == 'Y')
          {
            var field = GetField(export.State, "state");

            field.Error = true;

            return;
          }
        }

        // *** end
        // *** 02/19/01 swsrchf
        // *** Problem report I00113227
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        local.Invalid.Count = 0;

        if (!IsEmpty(export.PromptPerson.SelectChar))
        {
          ++local.Invalid.Count;
        }

        if (!IsEmpty(export.PromptAttachment.SelectChar))
        {
          ++local.Invalid.Count;
        }

        if (!IsEmpty(export.IattStatePrompt.SelectChar))
        {
          ++local.Invalid.Count;
        }

        if (local.Invalid.Count > 1)
        {
          if (!IsEmpty(export.PromptPerson.SelectChar))
          {
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptAttachment.SelectChar))
          {
            var field = GetField(export.PromptAttachment, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.IattStatePrompt.SelectChar))
          {
            var field = GetField(export.IattStatePrompt, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (!IsEmpty(export.PromptPerson.SelectChar))
        {
          if (IsEmpty(export.Next.Number))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";

            return;
          }

          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }
        }

        if (!IsEmpty(export.PromptAttachment.SelectChar))
        {
          if (AsChar(export.PromptAttachment.SelectChar) != 'S')
          {
            var field = GetField(export.PromptAttachment, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }
          else
          {
            export.HiddenReturnMultRecs.Flag = "Y";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        if (!IsEmpty(export.IattStatePrompt.SelectChar))
        {
          if (AsChar(export.IattStatePrompt.SelectChar) != 'S')
          {
            var field = GetField(export.IattStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }
          else
          {
            export.InterstateReqAttachment.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        var field1 = GetField(export.PromptPerson, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.PromptAttachment, "selectChar");

        field2.Error = true;

        var field3 = GetField(export.IattStatePrompt, "selectChar");

        field3.Error = true;

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "ADD":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field4 = GetField(export.DisplayOnly, "number");

          field4.Error = true;

          var field5 = GetField(export.Next, "number");

          field5.Error = true;

          ExitState = "SI0000_DISPLAY_REQD_FOR_IATT";

          return;
        }

        if (export.InterstateRequest.IntHGeneratedId == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              export.Export1.Update.Details.SentDate = null;
              export.Export1.Update.Details.RequestDate = null;
              export.Export1.Update.Details.ReceivedDate = null;
            }
          }

          ExitState = "SI0000_NO_IC_WITH_OTHER_STATE";

          return;
        }

        export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
        local.Error.Flag = "Y";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            local.Error.Flag = "";

            break;
          }
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

          return;
        }

        // -------------------------------------------------------------------
        // Add check to see if Interstate Request record exists.
        // -------------------------------------------------------------------
        if (ReadInterstateRequest())
        {
          export.InterstateRequest.OtherStateFips =
            entities.InterstateRequest.OtherStateFips;
        }
        else
        {
          ExitState = "SI0000_NO_IC_WITH_OTHER_STATE";

          return;
        }

        // -------------------------------------------------------------------
        // Add check to see if State is CSENet ready and if we are exchanging 
        // information with them for this specific transaction type.
        // -------------------------------------------------------------------
        local.CsenetStateTable.StateCode = export.State.State;
        UseSiReadCsenetStateTable();

        if (IsExitState("CO0000_CSENET_STATE_NF"))
        {
          var field = GetField(export.State, "state");

          field.Error = true;
        }
        else if (AsChar(local.CsenetStateTable.RecStateInd) != 'Y')
        {
          var field = GetField(export.State, "state");

          field.Error = true;

          ExitState = "CO0000_CSENET_NOT_SENT_TO_STATE";
        }
        else if (AsChar(local.CsenetStateTable.QuickLocate) != 'Y')
        {
          var field = GetField(export.State, "state");

          field.Error = true;

          ExitState = "CO0000_STATE_DOES_ACCEPT_LO1";
        }

        // -------------------------------------------------------------------
        // Add code for PR# 1021.
        // -------------------------------------------------------------------
        if (AsChar(local.CommandSendOrRequest.Flag) == 'S')
        {
          if (AsChar(local.CsenetStateTable.CsenetReadyInd) != 'Y')
          {
            if (AsChar(export.HiddenPf15Pressed.Flag) != 'Y')
            {
              export.HiddenPf15Pressed.Flag = "Y";
              ExitState = "SI0000_OTH_STATE_NO_CSENET_PF15";

              return;
            }
          }
        }

        if (AsChar(local.CommandSendOrRequest.Flag) == 'R')
        {
          if (AsChar(local.CsenetStateTable.CsenetReadyInd) != 'Y')
          {
            if (AsChar(export.HiddenPf16Pressed.Flag) != 'Y')
            {
              export.HiddenPf16Pressed.Flag = "Y";
              ExitState = "SI0000_OTH_STATE_NO_CSENET_PF16";

              return;
            }
          }
        }

        if (!export.Export1.IsEmpty)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
            {
              if (export.NoteLength.Count + Length
                (TrimEnd(export.Export1.Item.Details.Note)) <= 196)
              {
                export.NoteLength.Count += Length(
                  TrimEnd(export.Export1.Item.Details.Note));
                UseSiIattCreateIsRequestAttach();

                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                {
                  export.Export1.Update.Select.SelectChar = "*";
                }
                else if (IsExitState("SI0000_NO_CSENET_OUT_CLOSED_CASE"))
                {
                  var field = GetField(export.Next, "number");

                  field.Error = true;

                  return;
                }
                else if (IsExitState("SI0000_IATT_DUPL_VALUE_EXISTS"))
                {
                  global.Command = "DISPLAY";

                  var field =
                    GetField(export.Export1.Item.Select, "selectChar");

                  field.Error = true;

                  local.DuplicateExists.Flag = "Y";

                  goto Test;
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Select, "selectChar");

                  field.Error = true;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

          return;
        }

        if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y' && AsChar
          (local.CsenetStateTable.RecStateInd) != 'Y')
        {
          ExitState = "SI0000_CASE_CREATD_SND_MAN_NOTC";

          return;
        }

        if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y' && AsChar
          (local.CsenetStateTable.RecStateInd) != 'Y')
        {
          ExitState = "SI0000_CASE_CREATD_SND_WRIT_REQ";

          return;
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          if (ReadCase())
          {
            local.InterstateRequestHistory.AttachmentIndicator = "Y";
            local.InterstateRequestHistory.FunctionalTypeCode = "MSC";
            local.InterstateRequestHistory.TransactionDirectionInd = "O";
            local.InterstateRequestHistory.TransactionDate = Now().Date;

            if (AsChar(local.CommandSendOrRequest.Flag) == 'S')
            {
              local.InterstateCase.ActionCode = "P";
              local.InterstateCase.FunctionalTypeCode = "MSC";
              local.InterstateCase.ActionReasonCode = "GSUPD";
              local.InterstateRequestHistory.ActionCode = "P";
              local.InterstateRequestHistory.ActionReasonCode = "GSUPD";
              local.Input1.Value = local.SendMsg.Text60;
            }
            else if (AsChar(local.CommandSendOrRequest.Flag) == 'R')
            {
              local.Input1.Value = local.RequestMsg.Text60;
              local.InterstateCase.ActionCode = "R";
              local.InterstateCase.FunctionalTypeCode = "MSC";
              local.InterstateCase.ActionReasonCode = "GRPOC";
              local.InterstateRequestHistory.ActionCode = "R";
              local.InterstateRequestHistory.ActionReasonCode = "GRPOC";
            }

            local.Input2.Value = Spaces(FieldValue.Value_MaxLength);
            UseSpEabConcat();

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Select.SelectChar) == '*')
              {
                local.Input1.Value = local.Output.Value ?? "";
                local.Input2.Value =
                  (export.Export1.Item.Details.Note ?? "") + ";";

                if (Length(TrimEnd(local.Output.Value)) + Length
                  (TrimEnd(local.Input2.Value)) <= 240)
                {
                  UseSpEabConcat();
                }
              }
            }

            local.InterstateRequestHistory.Note = local.Output.Value ?? "";
            UseSiCreateIsRequestHistory();

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              return;
            }

            // ***************************************************************
            // 02/08/1999    C. Ott     New CAB was added so that attachments 
            // can be deleted.  This CAB creates a connection between the
            // Interstate Request History and the Interstate Request Attachment.
            // ***************************************************************
            UseSiIattConnectAttchmntToHist();

            if (IsExitState("ACO_RE0000_DB_INTEGRITY_ERROR"))
            {
              return;
            }
          }

          local.InterstateCase.AttachmentsInd =
            local.InterstateRequestHistory.AttachmentIndicator ?? Spaces(1);
          local.InterstateCase.CaseDataInd = 1;
          local.InterstateCase.ApIdentificationInd = 0;
          local.InterstateCase.ApLocateDataInd = 0;
          local.InterstateCase.ParticipantDataInd = 0;
          local.InterstateCase.OrderDataInd = 0;
          local.InterstateCase.CollectionDataInd = 0;
          local.InterstateCase.InformationInd = 1;
          UseSiCreateOgCsenetCaseDb();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            local.AddSuccessful.Flag = "Y";

            // -------------------------------------------------------------------
            // Add code for PR# 1021.
            // -------------------------------------------------------------------
            if (AsChar(local.CommandSendOrRequest.Flag) == 'S')
            {
              if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
              {
                ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
              }
              else if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y')
              {
                export.HiddenPf15Pressed.Flag = "";
                ExitState = "SI0000_CASE_CREATD_SND_MAN_NOTC";
              }
            }

            if (AsChar(local.CommandSendOrRequest.Flag) == 'R')
            {
              if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
              {
                ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
              }
              else if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y')
              {
                export.HiddenPf16Pressed.Flag = "";
                ExitState = "SI0000_CASE_CREATD_SND_WRIT_REQ";
              }
            }

            global.Command = "DISPLAY";
          }
        }
        else
        {
          return;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test:

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!Equal(export.HiddenCase.Number, export.Next.Number))
      {
        export.State.State = "";
        export.HiddenCase.Number = export.Next.Number;
      }

      if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
      {
        export.State.State = export.OtherState.StateAbbreviation;
      }

      if (IsEmpty(export.Next.Number))
      {
        export.DisplayOnly.Number = local.RefreshCase.Number;
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
        export.InterstateRequest.Assign(local.RefreshInterstateRequest);
        export.State.State = local.RefreshCommon.State;

        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";

        if (!Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
        }

        UseSiReadCaseHeaderInformation();

        if (IsExitState("CASE_NF"))
        {
          export.DisplayOnly.Number = local.RefreshCase.Number;
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);

          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }
      }

      UseSiReadOfficeOspHeader();

      if (IsExitState("SI0000_CASE_CREATD_SND_WRIT_REQ"))
      {
      }
      else if (IsExitState("SI0000_CASE_CREATD_SND_MAN_NOTC"))
      {
      }
      else if (IsExitState("SI0000_OTH_STATE_NO_CSENET_PF15"))
      {
      }
      else if (IsExitState("SI0000_OTH_STATE_NO_CSENET_PF16"))
      {
      }
      else if (IsExitState("SI0000_CSENET_TRANSACTION_SUCCES"))
      {
      }
      else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
      }
      else if (IsExitState("SI0000_IATT_DELETE_SUCCESSFUL"))
      {
      }
      else if (IsExitState("SI0000_ATTACHMENT_DELETE_INVALID"))
      {
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.InterstateRequest.Assign(local.RefreshInterstateRequest);

        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
        IsEmpty(export.DisplayOnly.Number))
      {
        export.ApCsePerson.Number = "";
        export.ApCsePersonsWorkSet.Number = "";
        export.ApCsePersonsWorkSet.FormattedName = "";
      }

      if (IsEmpty(export.OtherState.StateAbbreviation))
      {
        export.OtherState.StateAbbreviation = export.State.State;
      }

      UseSiIattListIsRequestAttachmn();

      if (IsExitState("SI0000_MULTIPLE_IR_EXISTS_FOR_AP"))
      {
        // *** Problem report I00120592
        // *** 05/30/01 swsrchf
        // *** start
        export.AutoFlow.Flag = "Y";
        ExitState = "ECO_LNK_TO_IREQ";

        return;

        // *** end
        // *** 05/30/01 swsrchf
        // *** Probem report I00120592
      }

      if (AsChar(local.CaseOpen.Flag) == 'N' && !
        IsExitState("CASE_NOT_INTERSTATE"))
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }

      if (IsExitState("SI0000_DISPLAY_OK_FOR_IC_INT_CAS") || IsExitState
        ("SI0000_DISPLAY_OK_FOR_OG_INT_CAS"))
      {
        if (AsChar(local.AddSuccessful.Flag) == 'Y')
        {
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
          {
            ExitState = "SI0000_SEND_OK_CLOSED_INT_REQ";
          }
          else
          {
            // -------------------------------------------------------------------
            // Add code for PR# 1021.
            // -------------------------------------------------------------------
            if (AsChar(local.CommandSendOrRequest.Flag) == 'S')
            {
              if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
              {
                ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
              }
              else if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y')
              {
                export.HiddenPf15Pressed.Flag = "";
                ExitState = "SI0000_CASE_CREATD_SND_MAN_NOTC";
              }
            }

            if (AsChar(local.CommandSendOrRequest.Flag) == 'R')
            {
              if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
              {
                ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
              }
              else if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y')
              {
                export.HiddenPf16Pressed.Flag = "";
                ExitState = "SI0000_CASE_CREATD_SND_WRIT_REQ";
              }
            }
          }
        }

        if (AsChar(local.DeleteSuccessful.Flag) == 'Y')
        {
          ExitState = "SI0000_IATT_DELETE_SUCCESSFUL";
        }

        if (AsChar(local.DuplicateExists.Flag) == 'Y')
        {
          ExitState = "SI0000_IATT_DUPL_VALUE_EXISTS";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;

          if (export.Export1.Item.Details.SentDate != null)
          {
            var field1 = GetField(export.Export1.Item.Details, "receivedDate");

            field1.Color = "cyan";
            field1.Intensity = Intensity.Dark;
            field1.Protected = true;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Details, "receivedDate");

            field1.Color = "cyan";
            field1.Intensity = Intensity.Normal;
            field1.Protected = true;
          }
        }

        // -------------------------------------------------------------------
        // Add code for PR# 1021.
        // -------------------------------------------------------------------
        if (AsChar(local.CommandSendOrRequest.Flag) == 'S')
        {
          if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
          {
            ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
          }
          else if (AsChar(export.HiddenPf15Pressed.Flag) == 'Y')
          {
            export.HiddenPf15Pressed.Flag = "";
            ExitState = "SI0000_CASE_CREATD_SND_MAN_NOTC";
          }
        }

        if (AsChar(local.CommandSendOrRequest.Flag) == 'R')
        {
          if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'Y')
          {
            ExitState = "SI0000_CSENET_TRANSACTION_SUCCES";
          }
          else if (AsChar(export.HiddenPf16Pressed.Flag) == 'Y')
          {
            export.HiddenPf16Pressed.Flag = "";
            ExitState = "SI0000_CASE_CREATD_SND_WRIT_REQ";
          }
        }
      }
    }
    else
    {
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(SiIattListIsRequestAttachmn.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Select.SelectChar = source.Select.SelectChar;
    target.Details.Assign(source.Details);
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    SiIattDeleteIsRequestAttach.Import.ImportGroup target)
  {
    target.Select.SelectChar = source.Select.SelectChar;
    target.Details.Assign(source.Details);
  }

  private static void MoveExport1ToLocal1(SiIattDeleteIsRequestAttach.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Select.SelectChar = source.Select.SelectChar;
    target.Details.Assign(source.Details);
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.AssnDeactDt = source.AssnDeactDt;
    target.AssnDeactInd = source.AssnDeactInd;
    target.LastDeferDt = source.LastDeferDt;
    target.Memo = source.Memo;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
  }

  private static void MoveInterstateRequest3(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
  }

  private static void MoveInterstateRequestAttachment(
    InterstateRequestAttachment source, InterstateRequestAttachment target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.TransactionDate = source.TransactionDate;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    export.InterstateReqAttachment.CodeName =
      useExport.InterstateReqAttachment.CodeName;
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
    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Case1.Number = export.DisplayOnly.Number;
    useImport.Ap.Number = export.ApCsePerson.Number;
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    local.InterstateRequestHistory.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSiCreateOgCsenetCaseDb()
  {
    var useImport = new SiCreateOgCsenetCaseDb.Import();
    var useExport = new SiCreateOgCsenetCaseDb.Export();

    useImport.Ap.Number = export.ApCsePerson.Number;
    MoveCase1(entities.Case1, useImport.Case1);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    MoveInterstateCase(local.InterstateCase, useImport.InterstateCase);
    MoveInterstateRequestHistory(local.InterstateRequestHistory,
      useImport.InterstateRequestHistory);

    Call(SiCreateOgCsenetCaseDb.Execute, useImport, useExport);

    MoveInterstateCase(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiIattConnectAttchmntToHist()
  {
    var useImport = new SiIattConnectAttchmntToHist.Import();
    var useExport = new SiIattConnectAttchmntToHist.Export();

    useImport.InterstateRequestHistory.CreatedTimestamp =
      local.InterstateRequestHistory.CreatedTimestamp;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiIattConnectAttchmntToHist.Execute, useImport, useExport);
  }

  private void UseSiIattCreateIsRequestAttach()
  {
    var useImport = new SiIattCreateIsRequestAttach.Import();
    var useExport = new SiIattCreateIsRequestAttach.Export();

    useImport.Case1.Number = export.DisplayOnly.Number;
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.InterstateRequestAttachment.Assign(export.Export1.Item.Details);

    Call(SiIattCreateIsRequestAttach.Execute, useImport, useExport);

    export.Export1.Update.Details.Assign(useExport.InterstateRequestAttachment);
  }

  private void UseSiIattDeleteIsRequestAttach()
  {
    var useImport = new SiIattDeleteIsRequestAttach.Import();
    var useExport = new SiIattDeleteIsRequestAttach.Export();

    useImport.Case1.Number = export.Next.Number;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(SiIattDeleteIsRequestAttach.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseSiIattListIsRequestAttachmn()
  {
    var useImport = new SiIattListIsRequestAttachmn.Import();
    var useExport = new SiIattListIsRequestAttachmn.Export();

    useImport.CaseOpen.Flag = local.CaseOpen.Flag;
    useImport.Case1.Number = export.Next.Number;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);
    useImport.OtherState.StateAbbreviation =
      export.OtherState.StateAbbreviation;
    MoveInterstateRequest3(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiIattListIsRequestAttachmn.Execute, useImport, useExport);

    export.DisplayOnly.Number = useExport.Case1.Number;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    export.State.State = useExport.State.State;
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.OtherState.StateAbbreviation =
      useExport.OtherState.StateAbbreviation;
  }

  private void UseSiIattUpdateIsRequestAttach()
  {
    var useImport = new SiIattUpdateIsRequestAttach.Import();
    var useExport = new SiIattUpdateIsRequestAttach.Export();

    MoveInterstateRequestAttachment(local.Update,
      useImport.InterstateRequestAttachment);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiIattUpdateIsRequestAttach.Execute, useImport, useExport);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Next.Number;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.OspServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.OspOffice);
  }

  private void UseSiValidateStateFips()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = local.State.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.FipsError.Flag = useExport.Error.Flag;
    MoveFips(useExport.Fips, export.OtherState);
  }

  private void UseSpEabConcat()
  {
    var useImport = new SpEabConcat.Import();
    var useExport = new SpEabConcat.Export();

    useImport.Input2.Value = local.Input2.Value;
    useImport.Input1.Value = local.Input1.Value;
    useExport.FieldValue.Value = local.Output.Value;

    Call(SpEabConcat.Execute, useImport, useExport);

    local.Output.Value = useExport.FieldValue.Value;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Next.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "othrStateFipsCd", export.OtherState.State);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 1);
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
    /// <summary>A DlgflwMultSelectnGroup group.</summary>
    [Serializable]
    public class DlgflwMultSelectnGroup
    {
      /// <summary>
      /// A value of DetailMultSelect.
      /// </summary>
      [JsonPropertyName("detailMultSelect")]
      public CodeValue DetailMultSelect
      {
        get => detailMultSelect ??= new();
        set => detailMultSelect = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CodeValue detailMultSelect;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
    }

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
    /// A value of PromptAttachment.
    /// </summary>
    [JsonPropertyName("promptAttachment")]
    public Common PromptAttachment
    {
      get => promptAttachment ??= new();
      set => promptAttachment = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// Gets a value of DlgflwMultSelectn.
    /// </summary>
    [JsonIgnore]
    public Array<DlgflwMultSelectnGroup> DlgflwMultSelectn =>
      dlgflwMultSelectn ??= new(DlgflwMultSelectnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DlgflwMultSelectn for json serialization.
    /// </summary>
    [JsonPropertyName("dlgflwMultSelectn")]
    [Computed]
    public IList<DlgflwMultSelectnGroup> DlgflwMultSelectn_Json
    {
      get => dlgflwMultSelectn;
      set => DlgflwMultSelectn.Assign(value);
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of IattStatePrompt.
    /// </summary>
    [JsonPropertyName("iattStatePrompt")]
    public Common IattStatePrompt
    {
      get => iattStatePrompt ??= new();
      set => iattStatePrompt = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenPf15Pressed.
    /// </summary>
    [JsonPropertyName("hiddenPf15Pressed")]
    public Common HiddenPf15Pressed
    {
      get => hiddenPf15Pressed ??= new();
      set => hiddenPf15Pressed = value;
    }

    /// <summary>
    /// A value of HiddenPf16Pressed.
    /// </summary>
    [JsonPropertyName("hiddenPf16Pressed")]
    public Common HiddenPf16Pressed
    {
      get => hiddenPf16Pressed ??= new();
      set => hiddenPf16Pressed = value;
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Fips otherState;
    private Case1 displayOnly;
    private Case1 next;
    private Common state;
    private InterstateRequest interstateRequest;
    private Common promptAttachment;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Array<DlgflwMultSelectnGroup> dlgflwMultSelectn;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Common promptPerson;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private Common iattStatePrompt;
    private CodeValue selectedCodeValue;
    private Common hiddenPf15Pressed;
    private Common hiddenPf16Pressed;
    private Case1 hiddenCase;
    private WorkArea headerLine;
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
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of NoteLength.
    /// </summary>
    [JsonPropertyName("noteLength")]
    public Common NoteLength
    {
      get => noteLength ??= new();
      set => noteLength = value;
    }

    /// <summary>
    /// A value of InterstateReqAttachment.
    /// </summary>
    [JsonPropertyName("interstateReqAttachment")]
    public Code InterstateReqAttachment
    {
      get => interstateReqAttachment ??= new();
      set => interstateReqAttachment = value;
    }

    /// <summary>
    /// A value of HiddenReturnMultRecs.
    /// </summary>
    [JsonPropertyName("hiddenReturnMultRecs")]
    public Common HiddenReturnMultRecs
    {
      get => hiddenReturnMultRecs ??= new();
      set => hiddenReturnMultRecs = value;
    }

    /// <summary>
    /// A value of PromptAttachment.
    /// </summary>
    [JsonPropertyName("promptAttachment")]
    public Common PromptAttachment
    {
      get => promptAttachment ??= new();
      set => promptAttachment = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of IattStatePrompt.
    /// </summary>
    [JsonPropertyName("iattStatePrompt")]
    public Common IattStatePrompt
    {
      get => iattStatePrompt ??= new();
      set => iattStatePrompt = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HiddenPf15Pressed.
    /// </summary>
    [JsonPropertyName("hiddenPf15Pressed")]
    public Common HiddenPf15Pressed
    {
      get => hiddenPf15Pressed ??= new();
      set => hiddenPf15Pressed = value;
    }

    /// <summary>
    /// A value of HiddenPf16Pressed.
    /// </summary>
    [JsonPropertyName("hiddenPf16Pressed")]
    public Common HiddenPf16Pressed
    {
      get => hiddenPf16Pressed ??= new();
      set => hiddenPf16Pressed = value;
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of AutoFlow.
    /// </summary>
    [JsonPropertyName("autoFlow")]
    public Common AutoFlow
    {
      get => autoFlow ??= new();
      set => autoFlow = value;
    }

    private Fips otherState;
    private Common noteLength;
    private Code interstateReqAttachment;
    private Common hiddenReturnMultRecs;
    private Common promptAttachment;
    private Array<ExportGroup> export1;
    private Case1 displayOnly;
    private Case1 next;
    private InterstateRequest interstateRequest;
    private Common state;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet ar;
    private Standard standard;
    private Common promptPerson;
    private CsePerson apCsePerson;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private Common iattStatePrompt;
    private CodeValue selected;
    private Common hiddenPf15Pressed;
    private Common hiddenPf16Pressed;
    private Case1 hiddenCase;
    private WorkArea headerLine;
    private Common autoFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    /// <summary>
    /// A value of RefreshCommon.
    /// </summary>
    [JsonPropertyName("refreshCommon")]
    public Common RefreshCommon
    {
      get => refreshCommon ??= new();
      set => refreshCommon = value;
    }

    /// <summary>
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of FipsError.
    /// </summary>
    [JsonPropertyName("fipsError")]
    public Common FipsError
    {
      get => fipsError ??= new();
      set => fipsError = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public InterstateRequestAttachment Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DuplicateExists.
    /// </summary>
    [JsonPropertyName("duplicateExists")]
    public Common DuplicateExists
    {
      get => duplicateExists ??= new();
      set => duplicateExists = value;
    }

    /// <summary>
    /// A value of DeleteSuccessful.
    /// </summary>
    [JsonPropertyName("deleteSuccessful")]
    public Common DeleteSuccessful
    {
      get => deleteSuccessful ??= new();
      set => deleteSuccessful = value;
    }

    /// <summary>
    /// A value of AddSuccessful.
    /// </summary>
    [JsonPropertyName("addSuccessful")]
    public Common AddSuccessful
    {
      get => addSuccessful ??= new();
      set => addSuccessful = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of RefreshInterstateRequest.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequest")]
    public InterstateRequest RefreshInterstateRequest
    {
      get => refreshInterstateRequest ??= new();
      set => refreshInterstateRequest = value;
    }

    /// <summary>
    /// A value of Output.
    /// </summary>
    [JsonPropertyName("output")]
    public FieldValue Output
    {
      get => output ??= new();
      set => output = value;
    }

    /// <summary>
    /// A value of Input2.
    /// </summary>
    [JsonPropertyName("input2")]
    public FieldValue Input2
    {
      get => input2 ??= new();
      set => input2 = value;
    }

    /// <summary>
    /// A value of Input1.
    /// </summary>
    [JsonPropertyName("input1")]
    public FieldValue Input1
    {
      get => input1 ??= new();
      set => input1 = value;
    }

    /// <summary>
    /// A value of RequestMsg.
    /// </summary>
    [JsonPropertyName("requestMsg")]
    public WorkArea RequestMsg
    {
      get => requestMsg ??= new();
      set => requestMsg = value;
    }

    /// <summary>
    /// A value of SendMsg.
    /// </summary>
    [JsonPropertyName("sendMsg")]
    public WorkArea SendMsg
    {
      get => sendMsg ??= new();
      set => sendMsg = value;
    }

    /// <summary>
    /// A value of CommandSendOrRequest.
    /// </summary>
    [JsonPropertyName("commandSendOrRequest")]
    public Common CommandSendOrRequest
    {
      get => commandSendOrRequest ??= new();
      set => commandSendOrRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
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
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    private CsenetStateTable csenetStateTable;
    private Common refreshCommon;
    private Common invalid;
    private Common fipsError;
    private Common state;
    private InterstateRequestAttachment update;
    private DateWorkArea null1;
    private Common duplicateExists;
    private Common deleteSuccessful;
    private Common addSuccessful;
    private Array<LocalGroup> local1;
    private Common caseOpen;
    private Common selected;
    private InterstateRequest refreshInterstateRequest;
    private FieldValue output;
    private FieldValue input2;
    private FieldValue input1;
    private WorkArea requestMsg;
    private WorkArea sendMsg;
    private Common commandSendOrRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateCase interstateCase;
    private TextWorkArea zeroFill;
    private Common error;
    private Case1 refreshCase;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private InterstateRequest interstateRequest;
    private Case1 case1;
  }
#endregion
}
