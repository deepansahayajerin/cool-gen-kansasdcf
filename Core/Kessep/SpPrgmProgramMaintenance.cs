// Program: SP_PRGM_PROGRAM_MAINTENANCE, ID: 371745810, model: 746.
// Short name: SWEPRGMP
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
/// A program: SP_PRGM_PROGRAM_MAINTENANCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpPrgmProgramMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRGM_PROGRAM_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrgmProgramMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrgmProgramMaintenance.
  /// </summary>
  public SpPrgmProgramMaintenance(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 02/18/96     A.  HACKLER    RETORFITS
    // ** 08/28/97     J. Rookard     Modify display logic to support
    // **                             OFCA Caseload Program selection.
    // ** 09/17/98     SWSRKEH        Phase 2 changes
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenFromOfca.Flag = import.HiddenFromOfca.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ZD_CO0000_CLEAR_SUCCESSFUL_1";

      return;
    }

    export.Search.Code = import.Search.Code;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.Program.Assign(import.Import1.Item.Program);

      if (!IsEmpty(export.Export1.Item.Program.Code))
      {
        var field = GetField(export.Export1.Item.Program, "code");

        field.Color = "cyan";
        field.Protected = true;
      }

      export.Export1.Update.ProgramIndicator.Assign(
        import.Import1.Item.ProgramIndicator);
      export.Export1.Update.HiddenProgram.Assign(
        import.Import1.Item.HiddenProgram);
      export.Export1.Update.HiddenProgramIndicator.Assign(
        import.Import1.Item.HiddenProgramIndicator);
      MoveCommon(import.Import1.Item.ListProgramType,
        export.Export1.Update.ListProgramType);
      export.Export1.Update.ListCsRetentionCd.SelectChar =
        import.Import1.Item.ListCsRetentionCd.SelectChar;

      if (AsChar(export.Export1.Item.ListProgramType.SelectChar) == 'S' && Equal
        (global.Command, "RLCVAL"))
      {
        // *********************************************
        // RETURNED FROM LIST CODE VALUES
        // *********************************************
        if (!IsEmpty(import.CodeValue.Cdvalue))
        {
          export.Export1.Update.Program.DistributionProgramType =
            import.CodeValue.Cdvalue;
        }

        export.Export1.Update.ListProgramType.SelectChar = "";

        var field = GetField(export.Export1.Item.ListProgramType, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(import.Import1.Item.ListCsRetentionCd.SelectChar) == 'S' && Equal
        (global.Command, "RLCVAL"))
      {
        // *********************************************
        // RETURNED FROM LIST CODE VALUES
        // *********************************************
        if (!IsEmpty(import.CodeValue.Cdvalue))
        {
          export.Export1.Update.ProgramIndicator.ChildSupportRetentionCode =
            import.CodeValue.Cdvalue;
        }

        export.Export1.Update.ListCsRetentionCd.SelectChar = "";

        var field =
          GetField(export.Export1.Item.ListCsRetentionCd, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && Equal
        (global.Command, "RETPFRH"))
      {
        export.Export1.Update.Common.SelectChar = "";
      }

      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ****  Added 'CASE OF'  KEH 9/16/98 ****
    // **** Trap all the commands that require only housekeeping  ****
    // **** (i.e.  rolling the import  views to the export views) and the 
    // commands   ****
    // **** that needs to be changed to the 'DISPLAY' command                 **
    // **
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "XXFMMENU":
        global.Command = "DISPLAY";

        break;
      case "XXNEXTXX":
        global.Command = "DISPLAY";

        break;
      case "RLCVAL":
        return;
      case "RETPFRH":
        return;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        return;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        return;
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ***********************************************
          // Flowing from here to next tran.
          // Note: there are no case person or other identifiers to move to 
          // hidden next tran
          // ***********************************************
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ****  changed (= to) to (not = to) KEH 9/16/98 ****
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;
          }

          return;
        }

        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
      case "INVALID":
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.HiddenFromOfca.Flag) == 'Y')
      {
        if (!ReadCode())
        {
          ExitState = "CODE_NF";

          return;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCodeValue())
        {
          if (ReadProgram1())
          {
            export.Export1.Update.Program.Assign(entities.Program);
            export.Export1.Update.HiddenProgram.Assign(entities.Program);
            local.DateWorkArea.Date =
              export.Export1.Item.Program.DiscontinueDate;
            export.Export1.Update.Program.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();

            if (ReadProgramIndicator())
            {
              export.Export1.Update.ProgramIndicator.Assign(
                entities.ProgramIndicator);

              var field = GetField(export.Export1.Item.Program, "code");

              field.Color = "cyan";
              field.Protected = true;

              export.Export1.Update.HiddenProgramIndicator.Assign(
                entities.ProgramIndicator);
            }

            export.Export1.Update.ListProgramType.SelectChar = "";
            export.Export1.Update.ListCsRetentionCd.SelectChar = "";
            export.Export1.Update.Common.SelectChar = "";
            local.NullDate.Date = entities.Program.DiscontinueDate;
          }
          else
          {
            var field = GetField(export.Export1.Item.Program, "code");

            field.Error = true;

            ExitState = "ZD_PROGRAM_NF_2";
            export.Export1.Next();

            return;
          }

          export.Export1.Next();
        }
      }
      else
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadProgram3())
        {
          export.Export1.Update.Program.Assign(entities.Program);

          var field = GetField(export.Export1.Item.Program, "code");

          field.Color = "cyan";
          field.Protected = true;

          export.Export1.Update.HiddenProgram.Assign(entities.Program);
          local.DateWorkArea.Date = export.Export1.Item.Program.DiscontinueDate;
          export.Export1.Update.Program.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();

          if (ReadProgramIndicator())
          {
            export.Export1.Update.ProgramIndicator.Assign(
              entities.ProgramIndicator);
            export.Export1.Update.HiddenProgramIndicator.Assign(
              entities.ProgramIndicator);
          }

          export.Export1.Update.ListProgramType.SelectChar = "";
          export.Export1.Update.ListCsRetentionCd.SelectChar = "";
          export.Export1.Update.Common.SelectChar = "";
          local.NullDate.Date = entities.Program.DiscontinueDate;
          export.Export1.Next();
        }
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        return;
      }

      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

      return;
    }

    // **  To reach this point of the program the command can only be one
    // **  of the following:  add, update, delete, list, history
    if (!Equal(global.Command, "ADD") && !Equal(global.Command, "DELETE") && !
      Equal(global.Command, "HISTORY") && !Equal(global.Command, "LIST") && !
      Equal(global.Command, "UPDATE") && !Equal(global.Command, "RETURN"))
    {
      ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

      return;
    }

    // Check to see if a selection has been made.
    local.SelCount.Count = 0;
    local.CodeCount.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!IsEmpty(export.Export1.Item.Common.SelectChar))
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
        {
          export.Export1.Update.Common.SelectChar = "";
        }
        else
        {
          ++local.SelCount.Count;
        }
      }

      if (!IsEmpty(export.Export1.Item.ListCsRetentionCd.SelectChar))
      {
        ++local.CodeCount.Count;
      }

      if (!IsEmpty(export.Export1.Item.ListProgramType.SelectChar))
      {
        ++local.CodeCount.Count;
      }
    }

    // Take the appropriate action base on number records selected.
    // At this point No selection is an error.
    // Selection Char has to be a 'S'.
    // More than one Selection is not allowed.
    switch(local.SelCount.Count)
    {
      case 0:
        if (!Equal(global.Command, "RETURN"))
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        break;
      case 1:
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar) && AsChar
            (export.Export1.Item.Common.SelectChar) != 'S')
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";
          }
        }

        break;
      default:
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.ListCsRetentionCd.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.ListCsRetentionCd, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.ListProgramType.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.ListProgramType, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        break;
    }

    switch(local.CodeCount.Count)
    {
      case 0:
        if (Equal(global.Command, "LIST"))
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case 1:
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.ListCsRetentionCd.SelectChar) && AsChar
            (export.Export1.Item.ListCsRetentionCd.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.ListCsRetentionCd, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";
          }

          if (!IsEmpty(export.Export1.Item.ListProgramType.SelectChar) && AsChar
            (export.Export1.Item.ListProgramType.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.ListProgramType, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";
          }
        }

        break;
      default:
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.ListCsRetentionCd.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.ListCsRetentionCd, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.ListProgramType.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.ListProgramType, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "RETURN") && local.SelCount.Count == 0)
      {
        ExitState = "ACO_NN0000_ALL_OK";

        goto Test;
      }

      return;
    }

Test:

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.ListProgramType.SelectChar) == 'S')
            {
              export.Code.CodeName = "DISTRIBUTION PROGRAM TYPE";
              export.CodeValue.Cdvalue =
                export.Export1.Item.Program.DistributionProgramType;

              break;
            }

            if (AsChar(export.Export1.Item.ListCsRetentionCd.SelectChar) == 'S')
            {
              export.CodeValue.Cdvalue =
                export.Export1.Item.ProgramIndicator.ChildSupportRetentionCode;
              export.Code.CodeName = "CHILD SUPPORT RETENTION";

              break;
            }
          }
        }

        ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

        break;
      case "HISTORY":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.Program.Code))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field = GetField(export.Export1.Item.Program, "code");

              field.Error = true;

              return;
            }

            export.HiddenSelection.Assign(export.Export1.Item.Program);

            break;
          }
        }

        ExitState = "ECO_LNK_TO_PROGRAM_IND_HISTORY";

        break;
      case "ADD":
        local.HighDate.Date = new DateTime(2099, 12, 31);
        local.Error.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // **** Only one element of the group view can be added at a time ****
          // field edits for ADDs
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.Program.Code))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field = GetField(export.Export1.Item.Program, "code");

              field.Error = true;
            }
            else if (ReadProgram2())
            {
              ++local.BadDataError.Count;
              ExitState = "PROGRAM_CODE_AE";

              var field1 = GetField(export.Export1.Item.Program, "code");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Common, "selectChar");

              field2.Error = true;

              local.DateWorkArea.Date =
                export.Export1.Item.Program.DiscontinueDate;
              export.Export1.Update.Program.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }
            else
            {
              // **** Not found is the answer we want ****
            }

            switch(AsChar(export.Export1.Item.Program.InterstateIndicator))
            {
              case ' ':
                export.Export1.Update.Program.InterstateIndicator = "N";

                break;
              case 'Y':
                break;
              case 'N':
                break;
              default:
                ++local.BadDataError.Count;
                ExitState = "INTERSTATE_INDICATOR_Y_OR_N";

                var field =
                  GetField(export.Export1.Item.Program, "interstateIndicator");

                field.Error = true;

                break;
            }

            if (IsEmpty(export.Export1.Item.Program.Title))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field = GetField(export.Export1.Item.Program, "title");

              field.Error = true;
            }

            if (IsEmpty(export.Export1.Item.Program.DistributionProgramType))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.Program, "distributionProgramType");
                

              field.Error = true;
            }
            else
            {
              local.Code.CodeName = "DISTRIBUTION PROGRAM TYPE";
              local.CodeValue.Cdvalue =
                export.Export1.Item.Program.DistributionProgramType;

              // *********************************************
              // CALL  VALIDATE CODE VALUE TO VALIDATE THE PROGRAM TYPE CODE
              // ********************************************
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) != 'Y')
              {
                ++local.BadDataError.Count;
                ExitState = "PROGRAM_TYPE_NF";

                var field =
                  GetField(export.Export1.Item.Program,
                  "distributionProgramType");

                field.Error = true;
              }
            }

            if (IsEmpty(export.Export1.Item.ProgramIndicator.IvDFeeIndicator))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.ProgramIndicator, "ivDFeeIndicator");
                

              field.Error = true;
            }
            else if (AsChar(export.Export1.Item.ProgramIndicator.IvDFeeIndicator)
              != 'Y' && AsChar
              (export.Export1.Item.ProgramIndicator.IvDFeeIndicator) != 'N')
            {
              ++local.BadDataError.Count;
              ExitState = "INVALID_SWITCH";

              var field =
                GetField(export.Export1.Item.ProgramIndicator, "ivDFeeIndicator");
                

              field.Error = true;
            }

            if (IsEmpty(export.Export1.Item.ProgramIndicator.
              ChildSupportRetentionCode))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.ProgramIndicator,
                "childSupportRetentionCode");

              field.Error = true;
            }
            else
            {
              local.Code.CodeName = "CHILD SUPPORT RETENTION";
              local.CodeValue.Cdvalue =
                export.Export1.Item.ProgramIndicator.ChildSupportRetentionCode;

              // *********************************************
              // CALL  VALIDATE CODE VALUE TO VALIDATE THE CHILD SUPPORT 
              // RETENTITON CODE
              // ********************************************
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) != 'Y')
              {
                ++local.BadDataError.Count;
                ExitState = "CHILD_SUPPORT_RETENTION_CODE_NF";

                var field =
                  GetField(export.Export1.Item.ProgramIndicator,
                  "childSupportRetentionCode");

                field.Error = true;
              }
            }

            // *** DATE Edits ***
            if (Equal(export.Export1.Item.Program.EffectiveDate,
              local.NullDate.Date))
            {
              export.Export1.Update.Program.EffectiveDate = local.Current.Date;
            }
            else if (Lt(export.Export1.Item.Program.EffectiveDate,
              local.Current.Date))
            {
              ++local.Error.Count;

              var field =
                GetField(export.Export1.Item.Program, "effectiveDate");

              field.Error = true;

              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
            }

            if (Equal(export.Export1.Item.Program.DiscontinueDate,
              local.NullDate.Date))
            {
              local.DateWorkArea.Date =
                export.Export1.Item.Program.DiscontinueDate;
              export.Export1.Update.Program.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }

            if (Lt(export.Export1.Item.Program.DiscontinueDate,
              export.Export1.Item.Program.EffectiveDate))
            {
              ++local.BadDataError.Count;

              var field1 =
                GetField(export.Export1.Item.Program, "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.Program, "discontinueDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_END_LESS_THAN_EFF";
            }

            // End of Edits
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (local.Error.Count > 1)
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
              else if (local.BadDataError.Count > 1)
              {
                ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
              }

              if (Equal(export.Export1.Item.Program.DiscontinueDate,
                local.HighDate.Date))
              {
                local.DateWorkArea.Date = local.HighDate.Date;
                export.Export1.Update.Program.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              return;
            }

            UseSpCreateProgram();

            if (Equal(export.Export1.Item.Program.DiscontinueDate,
              local.NullDate.Date))
            {
              local.DateWorkArea.Date =
                export.Export1.Item.Program.DiscontinueDate;
              export.Export1.Update.Program.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }

            if (!IsExitState("ZD_ACO_NI0000_SUCCESSFUL_ADD_2"))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
              export.Export1.Update.Common.SelectChar = "*";
              export.Export1.Update.HiddenProgram.Assign(
                export.Export1.Item.Program);
              export.Export1.Update.HiddenProgramIndicator.Assign(
                export.Export1.Item.ProgramIndicator);
              export.Export1.Update.ListProgramType.SelectChar = "";
              export.Export1.Update.ListCsRetentionCd.SelectChar = "";
            }

            return;
          }
        }

        break;
      case "UPDATE":
        local.Error.Count = 0;
        local.ProgIndUpdFlag.Flag = "";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            // field edits for UPDATEs
            local.ProgIndUpdFlag.Flag = "";

            if (!Equal(export.Export1.Item.Program.DiscontinueDate,
              export.Export1.Item.HiddenProgram.DiscontinueDate))
            {
              local.ProgIndUpdFlag.Flag = "Y";
            }
            else if (!Equal(export.Export1.Item.Program.EffectiveDate,
              export.Export1.Item.HiddenProgram.EffectiveDate))
            {
              local.ProgIndUpdFlag.Flag = "Y";
            }
            else if (!Equal(export.Export1.Item.ProgramIndicator.
              ChildSupportRetentionCode,
              export.Export1.Item.HiddenProgramIndicator.
                ChildSupportRetentionCode))
            {
              local.ProgIndUpdFlag.Flag = "Y";
            }
            else if (AsChar(export.Export1.Item.ProgramIndicator.IvDFeeIndicator)
              != AsChar
              (export.Export1.Item.HiddenProgramIndicator.IvDFeeIndicator))
            {
              local.ProgIndUpdFlag.Flag = "Y";
            }
            else if (!Equal(export.Export1.Item.Program.DistributionProgramType,
              export.Export1.Item.HiddenProgram.DistributionProgramType))
            {
            }
            else if (AsChar(export.Export1.Item.Program.InterstateIndicator) !=
              AsChar(export.Export1.Item.HiddenProgram.InterstateIndicator))
            {
            }
            else if (!Equal(export.Export1.Item.Program.Title,
              export.Export1.Item.HiddenProgram.Title))
            {
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Color = "yellow";
              field.Intensity = Intensity.High;
              field.Protected = false;
              field.Focused = true;

              ExitState = "INVALID_UPDATE";

              return;
            }

            if (!Equal(export.Export1.Item.Program.Title,
              export.Export1.Item.HiddenProgram.Title))
            {
              if (IsEmpty(export.Export1.Item.Program.Title))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
                ++local.Error.Count;

                var field = GetField(export.Export1.Item.Program, "title");

                field.Error = true;
              }
            }

            if (IsEmpty(export.Export1.Item.Program.DistributionProgramType))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.Program, "distributionProgramType");
                

              field.Error = true;
            }

            if (IsEmpty(export.Export1.Item.ProgramIndicator.IvDFeeIndicator))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.ProgramIndicator, "ivDFeeIndicator");
                

              field.Error = true;
            }

            if (IsEmpty(export.Export1.Item.ProgramIndicator.
              ChildSupportRetentionCode))
            {
              ++local.Error.Count;
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field =
                GetField(export.Export1.Item.ProgramIndicator,
                "childSupportRetentionCode");

              field.Error = true;
            }

            switch(AsChar(export.Export1.Item.Program.InterstateIndicator))
            {
              case ' ':
                export.Export1.Update.Program.InterstateIndicator = "N";

                break;
              case 'Y':
                break;
              case 'N':
                break;
              default:
                ++local.Error.Count;
                ExitState = "INTERSTATE_INDICATOR_Y_OR_N";

                var field =
                  GetField(export.Export1.Item.Program, "interstateIndicator");

                field.Error = true;

                break;
            }

            if (AsChar(export.Export1.Item.ProgramIndicator.IvDFeeIndicator) !=
              'Y' && AsChar
              (export.Export1.Item.ProgramIndicator.IvDFeeIndicator) != 'N')
            {
              ++local.Error.Count;
              ExitState = "INVALID_SWITCH";

              var field =
                GetField(export.Export1.Item.ProgramIndicator, "ivDFeeIndicator");
                

              field.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (local.Error.Count > 1)
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }

              return;
            }

            if (!Equal(export.Export1.Item.Program.DistributionProgramType,
              export.Export1.Item.HiddenProgram.DistributionProgramType))
            {
              local.Code.CodeName = "DISTRIBUTION PROGRAM TYPE";
              local.CodeValue.Cdvalue =
                export.Export1.Item.Program.DistributionProgramType;

              // *********************************************
              // CALL  VALIDATE CODE VALUE TO VALIDATE THE PROGRAM TYPE CODE
              // ********************************************
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) != 'Y')
              {
                ++local.Error.Count;
                ExitState = "PROGRAM_TYPE_NF";

                var field =
                  GetField(export.Export1.Item.Program,
                  "distributionProgramType");

                field.Error = true;
              }
            }

            if (!Equal(export.Export1.Item.ProgramIndicator.
              ChildSupportRetentionCode,
              export.Export1.Item.HiddenProgramIndicator.
                ChildSupportRetentionCode))
            {
              local.Code.CodeName = "CHILD SUPPORT RETENTION";
              local.CodeValue.Cdvalue =
                export.Export1.Item.ProgramIndicator.ChildSupportRetentionCode;

              // *********************************************
              // CALL  VALIDATE CODE VALUE TO VALIDATE THE CHILD SUPPORT 
              // RETENTITON CODE
              // ********************************************
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnCode.Flag) != 'Y')
              {
                ++local.Error.Count;
                ExitState = "CHILD_SUPPORT_RETENTION_CODE_NF";

                var field =
                  GetField(export.Export1.Item.ProgramIndicator,
                  "childSupportRetentionCode");

                field.Error = true;
              }
            }

            // **** Date Edits
            if (Equal(export.Export1.Item.HiddenProgram.EffectiveDate,
              export.Export1.Item.Program.EffectiveDate) || !
              Lt(export.Export1.Item.Program.EffectiveDate, local.Current.Date) &&
              !
              Lt(export.Export1.Item.HiddenProgram.EffectiveDate,
              local.Current.Date))
            {
            }
            else
            {
              ++local.Error.Count;
              ExitState = "CANNOT_CHANGE_EFFECTIVE_DATE";

              var field =
                GetField(export.Export1.Item.Program, "effectiveDate");

              field.Error = true;
            }

            if (!Equal(export.Export1.Item.Program.DiscontinueDate,
              local.NullDate.Date))
            {
              if (!Equal(export.Export1.Item.Program.DiscontinueDate,
                export.Export1.Item.HiddenProgram.DiscontinueDate))
              {
                if (Lt(export.Export1.Item.Program.DiscontinueDate,
                  local.Current.Date))
                {
                  ++local.Error.Count;
                  ExitState = "CANNOT_DISCONTINUE_DISC_DATE";

                  var field =
                    GetField(export.Export1.Item.Program, "discontinueDate");

                  field.Error = true;
                }
              }

              if (Lt(export.Export1.Item.Program.DiscontinueDate,
                export.Export1.Item.Program.EffectiveDate))
              {
                var field1 =
                  GetField(export.Export1.Item.Program, "effectiveDate");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.Program, "discontinueDate");

                field2.Error = true;

                ExitState = "ACO_NE0000_END_LESS_THAN_EFF";
                ++local.Error.Count;
              }
            }
            else
            {
              local.DateWorkArea.Date =
                export.Export1.Item.Program.DiscontinueDate;
              export.Export1.Update.Program.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }

            // End of Edits
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (local.Error.Count > 1)
              {
                ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
              }

              if (!Equal(export.Export1.Item.Program.DiscontinueDate,
                local.NullDate.Date))
              {
                local.DateWorkArea.Date =
                  export.Export1.Item.Program.DiscontinueDate;
                export.Export1.Update.Program.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              return;
            }

            if (AsChar(local.ProgIndUpdFlag.Flag) == 'Y' && Lt
              (export.Export1.Item.Program.DiscontinueDate, local.Current.Date))
            {
              ExitState = "CANNOT_CHANGE_A_DISCONTINUED_REC";

              return;
            }

            if (AsChar(local.ProgIndUpdFlag.Flag) == 'Y')
            {
              UseUpdateProgramIndicator();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            UseSpUpdateProgram();

            if (!Equal(export.Export1.Item.Program.DiscontinueDate,
              local.NullDate.Date))
            {
              local.DateWorkArea.Date =
                export.Export1.Item.Program.DiscontinueDate;
              export.Export1.Update.Program.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }

            if (!IsExitState("ZD_ACO_NI0000_SUCCESSFUL_UPD_3"))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
              export.Export1.Update.Common.SelectChar = "*";
              export.Export1.Update.HiddenProgram.Assign(
                export.Export1.Item.Program);
              export.Export1.Update.HiddenProgramIndicator.Assign(
                export.Export1.Item.ProgramIndicator);
              export.Export1.Update.ListProgramType.SelectChar = "";
              export.Export1.Update.ListCsRetentionCd.SelectChar = "";
            }

            return;
          }
        }

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.Program.Code))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field = GetField(export.Export1.Item.Program, "code");

              field.Error = true;

              return;
            }

            UseSpDeleteProgram();

            if (!IsExitState("ZD_ACO_NI0000_SUCCESSFUL_DEL_3"))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;
            }
            else
            {
              export.Export1.Update.Common.SelectChar = "*";
              export.Export1.Update.HiddenProgram.Assign(
                export.Export1.Item.Program);
              export.Export1.Update.HiddenProgramIndicator.Assign(
                export.Export1.Item.ProgramIndicator);
              export.Export1.Update.ListProgramType.SelectChar = "";
              export.Export1.Update.ListCsRetentionCd.SelectChar = "";
            }

            return;
          }
        }

        break;
      case "RETURN":
        if (local.SelCount.Count != 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            // **** Only one element of the group view can be added at a time **
            // **
            // field edits for ADDs
            if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
            {
              export.HiddenSelection.Assign(export.Export1.Item.Program);

              break;
            }
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
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

  private static void MoveProgram1(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Title = source.Title;
    target.DistributionProgramType = source.DistributionProgramType;
    target.InterstateIndicator = source.InterstateIndicator;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgram2(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Title = source.Title;
    target.DistributionProgramType = source.DistributionProgramType;
    target.InterstateIndicator = source.InterstateIndicator;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatdTstamp = source.LastUpdatdTstamp;
  }

  private static void MoveProgram3(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

  private void UseSpCreateProgram()
  {
    var useImport = new SpCreateProgram.Import();
    var useExport = new SpCreateProgram.Export();

    useImport.ProgramIndicator.Assign(export.Export1.Item.ProgramIndicator);
    useImport.Program.Assign(export.Export1.Item.Program);

    Call(SpCreateProgram.Execute, useImport, useExport);

    MoveProgram1(useExport.Program, export.Export1.Update.Program);
  }

  private void UseSpDeleteProgram()
  {
    var useImport = new SpDeleteProgram.Import();
    var useExport = new SpDeleteProgram.Export();

    useImport.Program.Assign(export.Export1.Item.Program);

    Call(SpDeleteProgram.Execute, useImport, useExport);

    export.Export1.Update.Program.Assign(useExport.Program);
  }

  private void UseSpUpdateProgram()
  {
    var useImport = new SpUpdateProgram.Import();
    var useExport = new SpUpdateProgram.Export();

    useImport.ProgramIndicator.Assign(export.Export1.Item.ProgramIndicator);
    useImport.Program.Assign(export.Export1.Item.Program);

    Call(SpUpdateProgram.Execute, useImport, useExport);

    MoveProgram2(useExport.Program, export.Export1.Update.Program);
  }

  private void UseUpdateProgramIndicator()
  {
    var useImport = new UpdateProgramIndicator.Import();
    var useExport = new UpdateProgramIndicator.Export();

    useImport.ProgramIndicator.Assign(export.Export1.Item.ProgramIndicator);
    MoveProgram3(export.Export1.Item.Program, useImport.Program);

    Call(UpdateProgramIndicator.Execute, useImport, useExport);

    export.Export1.Update.ProgramIndicator.Assign(useExport.ProgramIndicator);
  }

  private bool ReadCode()
  {
    entities.Code.Populated = false;

    return Read("ReadCode",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.Code.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.Code.Id);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "cdvalue", entities.CodeValue.Cdvalue);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Title = db.GetString(reader, 2);
        entities.Program.InterstateIndicator = db.GetString(reader, 3);
        entities.Program.EffectiveDate = db.GetDate(reader, 4);
        entities.Program.DiscontinueDate = db.GetDate(reader, 5);
        entities.Program.CreatedBy = db.GetString(reader, 6);
        entities.Program.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Program.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Program.LastUpdatdTstamp = db.GetNullableDateTime(reader, 9);
        entities.Program.DistributionProgramType = db.GetString(reader, 10);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetString(command, "code", export.Export1.Item.Program.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Title = db.GetString(reader, 2);
        entities.Program.InterstateIndicator = db.GetString(reader, 3);
        entities.Program.EffectiveDate = db.GetDate(reader, 4);
        entities.Program.DiscontinueDate = db.GetDate(reader, 5);
        entities.Program.CreatedBy = db.GetString(reader, 6);
        entities.Program.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Program.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Program.LastUpdatdTstamp = db.GetNullableDateTime(reader, 9);
        entities.Program.DistributionProgramType = db.GetString(reader, 10);
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram3()
  {
    return ReadEach("ReadProgram3",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Title = db.GetString(reader, 2);
        entities.Program.InterstateIndicator = db.GetString(reader, 3);
        entities.Program.EffectiveDate = db.GetDate(reader, 4);
        entities.Program.DiscontinueDate = db.GetDate(reader, 5);
        entities.Program.CreatedBy = db.GetString(reader, 6);
        entities.Program.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Program.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Program.LastUpdatdTstamp = db.GetNullableDateTime(reader, 9);
        entities.Program.DistributionProgramType = db.GetString(reader, 10);
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadProgramIndicator()
  {
    entities.ProgramIndicator.Populated = false;

    return Read("ReadProgramIndicator",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetDate(
          command, "discontinueDate1", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate2",
          entities.Program.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramIndicator.ChildSupportRetentionCode =
          db.GetString(reader, 0);
        entities.ProgramIndicator.IvDFeeIndicator = db.GetString(reader, 1);
        entities.ProgramIndicator.EffectiveDate = db.GetDate(reader, 2);
        entities.ProgramIndicator.DiscontinueDate = db.GetDate(reader, 3);
        entities.ProgramIndicator.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ProgramIndicator.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of ListCsRetentionCd.
      /// </summary>
      [JsonPropertyName("listCsRetentionCd")]
      public Common ListCsRetentionCd
      {
        get => listCsRetentionCd ??= new();
        set => listCsRetentionCd = value;
      }

      /// <summary>
      /// A value of HiddenProgram.
      /// </summary>
      [JsonPropertyName("hiddenProgram")]
      public Program HiddenProgram
      {
        get => hiddenProgram ??= new();
        set => hiddenProgram = value;
      }

      /// <summary>
      /// A value of HiddenProgramIndicator.
      /// </summary>
      [JsonPropertyName("hiddenProgramIndicator")]
      public ProgramIndicator HiddenProgramIndicator
      {
        get => hiddenProgramIndicator ??= new();
        set => hiddenProgramIndicator = value;
      }

      /// <summary>
      /// A value of ListProgramType.
      /// </summary>
      [JsonPropertyName("listProgramType")]
      public Common ListProgramType
      {
        get => listProgramType ??= new();
        set => listProgramType = value;
      }

      /// <summary>
      /// A value of ProgramIndicator.
      /// </summary>
      [JsonPropertyName("programIndicator")]
      public ProgramIndicator ProgramIndicator
      {
        get => programIndicator ??= new();
        set => programIndicator = value;
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common listCsRetentionCd;
      private Program hiddenProgram;
      private ProgramIndicator hiddenProgramIndicator;
      private Common listProgramType;
      private ProgramIndicator programIndicator;
      private Common common;
      private Program program;
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
    /// A value of HiddenFromOfca.
    /// </summary>
    [JsonPropertyName("hiddenFromOfca")]
    public Common HiddenFromOfca
    {
      get => hiddenFromOfca ??= new();
      set => hiddenFromOfca = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Program Search
    {
      get => search ??= new();
      set => search = value;
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common hiddenFromOfca;
    private CodeValue codeValue;
    private Program search;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
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
      /// A value of ListCsRetentionCd.
      /// </summary>
      [JsonPropertyName("listCsRetentionCd")]
      public Common ListCsRetentionCd
      {
        get => listCsRetentionCd ??= new();
        set => listCsRetentionCd = value;
      }

      /// <summary>
      /// A value of HiddenProgram.
      /// </summary>
      [JsonPropertyName("hiddenProgram")]
      public Program HiddenProgram
      {
        get => hiddenProgram ??= new();
        set => hiddenProgram = value;
      }

      /// <summary>
      /// A value of HiddenProgramIndicator.
      /// </summary>
      [JsonPropertyName("hiddenProgramIndicator")]
      public ProgramIndicator HiddenProgramIndicator
      {
        get => hiddenProgramIndicator ??= new();
        set => hiddenProgramIndicator = value;
      }

      /// <summary>
      /// A value of ListProgramType.
      /// </summary>
      [JsonPropertyName("listProgramType")]
      public Common ListProgramType
      {
        get => listProgramType ??= new();
        set => listProgramType = value;
      }

      /// <summary>
      /// A value of ProgramIndicator.
      /// </summary>
      [JsonPropertyName("programIndicator")]
      public ProgramIndicator ProgramIndicator
      {
        get => programIndicator ??= new();
        set => programIndicator = value;
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common listCsRetentionCd;
      private Program hiddenProgram;
      private ProgramIndicator hiddenProgramIndicator;
      private Common listProgramType;
      private ProgramIndicator programIndicator;
      private Common common;
      private Program program;
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
    /// A value of HiddenFromOfca.
    /// </summary>
    [JsonPropertyName("hiddenFromOfca")]
    public Common HiddenFromOfca
    {
      get => hiddenFromOfca ??= new();
      set => hiddenFromOfca = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Program Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public Program HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common hiddenFromOfca;
    private Code code;
    private CodeValue codeValue;
    private Program search;
    private Array<ExportGroup> export1;
    private Program hiddenSelection;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProgIndUpdFlag.
    /// </summary>
    [JsonPropertyName("progIndUpdFlag")]
    public Common ProgIndUpdFlag
    {
      get => progIndUpdFlag ??= new();
      set => progIndUpdFlag = value;
    }

    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    /// <summary>
    /// A value of ReactivateFlag.
    /// </summary>
    [JsonPropertyName("reactivateFlag")]
    public Common ReactivateFlag
    {
      get => reactivateFlag ??= new();
      set => reactivateFlag = value;
    }

    /// <summary>
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    /// <summary>
    /// A value of BadDataError.
    /// </summary>
    [JsonPropertyName("badDataError")]
    public Common BadDataError
    {
      get => badDataError ??= new();
      set => badDataError = value;
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
    /// A value of CodeCount.
    /// </summary>
    [JsonPropertyName("codeCount")]
    public Common CodeCount
    {
      get => codeCount ??= new();
      set => codeCount = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
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

    private Common progIndUpdFlag;
    private ProgramIndicator programIndicator;
    private Common reactivateFlag;
    private DateWorkArea highDate;
    private Common badDataError;
    private Common error;
    private Common codeCount;
    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
    private Code code;
    private Common returnCode;
    private CodeValue codeValue;
    private NullDate nullDate;
    private Common selCount;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private CodeValue codeValue;
    private Code code;
    private ProgramIndicator programIndicator;
    private Program program;
  }
#endregion
}
