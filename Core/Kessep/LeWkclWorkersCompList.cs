// Program: LE_WKCL_WORKERS_COMP_LIST, ID: 1625339408, model: 746.
// Short name: SWEWKCLP
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
/// A program: LE_WKCL_WORKERS_COMP_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeWkclWorkersCompList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_WKCL_WORKERS_COMP_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeWkclWorkersCompList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeWkclWorkersCompList.
  /// </summary>
  public LeWkclWorkersCompList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/16  GVandy	CQ51923		Initial Code.  Created from a copy of EIWL.
    // 6/17/19  AHockman       CQ51923 phase II  additions to the code to change
    // the display
    //                                           
    // and add filter for docket
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.CsePerson.Number = import.CsePerson.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.DocketFilter.Text1 = import.DocketFilterWorkArea.Text1;
    export.PromptDocketFilter.SelectChar = import.PromptDocketFilter.SelectChar;
    local.Max.Date = new DateTime(2099, 12, 31);
    export.MoreIndicator.Text9 = import.MoreIndicator.Text9;
    export.PageNumber.Count = import.PageNumber.Count;

    for(import.Paging.Index = 0; import.Paging.Index < import.Paging.Count; ++
      import.Paging.Index)
    {
      if (!import.Paging.CheckSize())
      {
        break;
      }

      export.Paging.Index = import.Paging.Index;
      export.Paging.CheckSize();

      MoveWorkersCompClaim2(import.Paging.Item.GimportPaging,
        export.Paging.Update.GexportPaging);
    }

    import.Paging.CheckIndex();

    if (!IsEmpty(import.CsePersonsWorkSet.Number) && IsEmpty
      (import.CsePerson.Number))
    {
      export.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    // -- If any of the search fields are spaces then space out the associated 
    // descriptions.
    if (IsEmpty(export.CsePerson.Number))
    {
      export.CsePersonsWorkSet.FormattedName = "";
    }

    // -- Zero fill any necessary search fields.
    if (!IsEmpty(export.CsePerson.Number))
    {
      UseCabZeroFillNumber();
    }

    // -- Move import group to export group.
    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      MoveWorkersCompClaim1(import.Import1.Item.GworkersCompClaim,
        export.Export1.Update.GworkersCompClaim);
      export.Export1.Update.GexportDocketWorkArea.Text1 =
        import.Import1.Item.GimportDocketWorkArea.Text1;
    }

    import.Import1.CheckIndex();

    if (Equal(global.Command, "RETCDVL"))
    {
      export.PromptDocketFilter.SelectChar = "";
      export.DocketFilter.Text1 =
        Substring(import.DocketFilterCodeValue.Cdvalue, 1, 1);
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETNAME":
        export.PromptPerson.SelectChar = "";

        if (IsEmpty(import.FromName.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          export.CsePerson.Number = import.FromName.Number;
        }

        global.Command = "DISPLAY";

        break;
      case "FROMMENU":
        if (IsEmpty(import.CsePerson.Number))
        {
          // -- Don't change command if a person number was not passed from 
          // PLOM.
        }
        else
        {
          // -- Change command to a display using the person number passed from 
          // PLOM.
          global.Command = "DISPLAY";
        }

        break;
      default:
        // -- Continue
        break;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
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
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
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
    // -- Prompt field validation.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      local.PromptCount.Count = 0;

      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptDocketFilter.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptDocketFilter, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(local.PromptCount.Count)
      {
        case 0:
          var field1 = GetField(export.PromptPerson, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptDocketFilter, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          break;
        case 1:
          break;
        default:
          var field3 = GetField(export.PromptPerson, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PromptDocketFilter, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(export.PromptDocketFilter.SelectChar) == 'S')
      {
        export.Prompt.CodeName = "WORKER COMP FILTER";
        ExitState = "ECO_LNK_TO_CDVL";

        return;
      }
      else if (AsChar(export.PromptPerson.SelectChar) == 'S')
      {
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      }
    }
    else
    {
      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Check how many selections have been made.
    // ---------------------------------------------
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.Gcommon.SelectChar))
      {
        case 'S':
          ++local.Common.Count;
          export.Selected.Identifier =
            export.Export1.Item.GworkersCompClaim.Identifier;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.Gcommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }
    }

    export.Export1.CheckIndex();

    if (local.Common.Count == 0 && Equal(global.Command, "WKCD"))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    if (local.Common.Count > 1 && (Equal(global.Command, "WKCD") || Equal
      (global.Command, "RETURN")))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Gcommon, "selectChar");

          field.Error = true;
        }
      }

      export.Export1.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        // return to wkcd, this is the only screen this screen can return to. 
        // exit will take you back to the menu plom.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
          {
            ++local.RecordCount.Count;
          }
        }

        export.Export1.CheckIndex();

        if (local.RecordCount.Count == 0)
        {
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        }
        else if (local.RecordCount.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
            {
              export.Selected.Identifier =
                export.Export1.Item.GworkersCompClaim.Identifier;
              export.Export1.Update.Gcommon.SelectChar = "*";

              break;
            }
          }

          export.Export1.CheckIndex();
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
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

            if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
            {
              var field = GetField(export.Export1.Item.Gcommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";
            }
          }

          export.Export1.CheckIndex();

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "WKCD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
          {
            ++local.RecordCount.Count;
          }
        }

        export.Export1.CheckIndex();

        if (local.RecordCount.Count == 0)
        {
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        }
        else if (local.RecordCount.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
            {
              export.Selected.Identifier =
                export.Export1.Item.GworkersCompClaim.Identifier;

              break;
            }
          }

          export.Export1.CheckIndex();
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
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

            if (!IsEmpty(export.Export1.Item.Gcommon.SelectChar))
            {
              var field = GetField(export.Export1.Item.Gcommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";
            }
          }

          export.Export1.CheckIndex();

          return;
        }

        ExitState = "ECO_LINK_TO_WKCD";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "PREV":
        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          ExitState = "ACO_PREV_INVALID_WITH_KEY_CHANGE";

          return;
        }

        if (export.PageNumber.Count <= 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        --export.PageNumber.Count;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        UseLeWkclDisplayWcList2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.PageNumber.Count == 1)
        {
          export.MoreIndicator.Text9 = "MORE:   +";

          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveWorkersCompClaim2(local.NextPage,
            export.Paging.Update.GexportPaging);
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveWorkersCompClaim2(local.NextPage,
            export.Paging.Update.GexportPaging);
          export.MoreIndicator.Text9 = "MORE: - +";
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "NEXT":
        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          ExitState = "ACO_PREV_INVALID_WITH_KEY_CHANGE";

          return;
        }

        if (export.PageNumber.Count == Export.PagingGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";

          return;
        }

        if (export.PageNumber.Count == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        if (export.PageNumber.Count == export.Paging.Count)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        ++export.PageNumber.Count;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        UseLeWkclDisplayWcList2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(local.NextPage.CreatedTimestamp, local.Null1.Timestamp))
        {
          export.MoreIndicator.Text9 = "MORE: -";
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveWorkersCompClaim2(local.NextPage,
            export.Paging.Update.GexportPaging);
          export.MoreIndicator.Text9 = "MORE: - +";
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "DISPLAY":
        export.PageNumber.Count = 1;

        export.Paging.Index = export.PageNumber.Count - 1;
        export.Paging.CheckSize();

        export.Paging.Update.GexportPaging.CreatedTimestamp =
          new DateTime(2099, 12, 31);
        export.Paging.Update.GexportPaging.DateOfLoss = local.Max.Date;
        export.Paging.Count = 1;
        export.Export1.Count = 0;
        export.MoreIndicator.Text9 = "MORE:";

        if (IsEmpty(export.CsePerson.Number))
        {
        }
        else
        {
          if (!ReadCsePerson())
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // insure that code is valid
        local.Code.CodeName = "WORKER COMP FILTER";
        local.CodeValue.Cdvalue = export.DocketFilter.Text1;

        // *** if there is a code in the field validate it  otherwise display 
        // all ***
        if (!IsEmpty(export.DocketFilter.Text1))
        {
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.DocketFilter, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }

        UseLeWkclDisplayWcList1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(local.NextPage.CreatedTimestamp, local.Null1.Timestamp))
        {
          export.MoreIndicator.Text9 = "MORE:";
        }
        else
        {
          export.Paging.Index = export.PageNumber.Count;
          export.Paging.CheckSize();

          MoveWorkersCompClaim2(local.NextPage,
            export.Paging.Update.GexportPaging);
          export.MoreIndicator.Text9 = "MORE:   +";
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;

        if (export.Export1.Count == 0)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "FROMMENU":
        // -- No processing required.
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(LeWkclDisplayWcList.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.GworkersCompClaim.Assign(source.GworkersCompClaim);
    target.GexportDocketWorkArea.Text1 = source.GexportDocket.Text1;
  }

  private static void MoveWorkersCompClaim1(WorkersCompClaim source,
    WorkersCompClaim target)
  {
    target.Identifier = source.Identifier;
    target.InsurerName = source.InsurerName;
    target.DateOfLoss = source.DateOfLoss;
    target.AdministrativeClaimNo = source.AdministrativeClaimNo;
  }

  private static void MoveWorkersCompClaim2(WorkersCompClaim source,
    WorkersCompClaim target)
  {
    target.DateOfLoss = source.DateOfLoss;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseLeWkclDisplayWcList1()
  {
    var useImport = new LeWkclDisplayWcList.Import();
    var useExport = new LeWkclDisplayWcList.Export();

    MoveWorkersCompClaim2(export.Paging.Item.GexportPaging, useImport.Starting);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.DocketFilter.Text1 = export.DocketFilter.Text1;

    Call(LeWkclDisplayWcList.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    export.CsePerson.Number = useExport.CsePerson.Number;
    MoveWorkersCompClaim2(useExport.NextPage, local.NextPage);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseLeWkclDisplayWcList2()
  {
    var useImport = new LeWkclDisplayWcList.Import();
    var useExport = new LeWkclDisplayWcList.Export();

    MoveWorkersCompClaim2(export.Paging.Item.GexportPaging, useImport.Starting);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(LeWkclDisplayWcList.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    export.CsePerson.Number = useExport.CsePerson.Number;
    MoveWorkersCompClaim2(useExport.NextPage, local.NextPage);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GworkersCompClaim.
      /// </summary>
      [JsonPropertyName("gworkersCompClaim")]
      public WorkersCompClaim GworkersCompClaim
      {
        get => gworkersCompClaim ??= new();
        set => gworkersCompClaim = value;
      }

      /// <summary>
      /// A value of GimportDocketWorkArea.
      /// </summary>
      [JsonPropertyName("gimportDocketWorkArea")]
      public WorkArea GimportDocketWorkArea
      {
        get => gimportDocketWorkArea ??= new();
        set => gimportDocketWorkArea = value;
      }

      /// <summary>
      /// A value of GimportDocketCommon.
      /// </summary>
      [JsonPropertyName("gimportDocketCommon")]
      public Common GimportDocketCommon
      {
        get => gimportDocketCommon ??= new();
        set => gimportDocketCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common gcommon;
      private WorkersCompClaim gworkersCompClaim;
      private WorkArea gimportDocketWorkArea;
      private Common gimportDocketCommon;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of GimportPaging.
      /// </summary>
      [JsonPropertyName("gimportPaging")]
      public WorkersCompClaim GimportPaging
      {
        get => gimportPaging ??= new();
        set => gimportPaging = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private WorkersCompClaim gimportPaging;
    }

    /// <summary>
    /// A value of DocketFilterCodeValue.
    /// </summary>
    [JsonPropertyName("docketFilterCodeValue")]
    public CodeValue DocketFilterCodeValue
    {
      get => docketFilterCodeValue ??= new();
      set => docketFilterCodeValue = value;
    }

    /// <summary>
    /// A value of PromptDocketFilter.
    /// </summary>
    [JsonPropertyName("promptDocketFilter")]
    public Common PromptDocketFilter
    {
      get => promptDocketFilter ??= new();
      set => promptDocketFilter = value;
    }

    /// <summary>
    /// A value of DocketFilterWorkArea.
    /// </summary>
    [JsonPropertyName("docketFilterWorkArea")]
    public WorkArea DocketFilterWorkArea
    {
      get => docketFilterWorkArea ??= new();
      set => docketFilterWorkArea = value;
    }

    /// <summary>
    /// A value of Docket.
    /// </summary>
    [JsonPropertyName("docket")]
    public Common Docket
    {
      get => docket ??= new();
      set => docket = value;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public WorkArea MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of FromName.
    /// </summary>
    [JsonPropertyName("fromName")]
    public CsePersonsWorkSet FromName
    {
      get => fromName ??= new();
      set => fromName = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    private CodeValue docketFilterCodeValue;
    private Common promptDocketFilter;
    private WorkArea docketFilterWorkArea;
    private Common docket;
    private WorkArea moreIndicator;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common promptPerson;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet fromName;
    private Array<PagingGroup> paging;
    private Common pageNumber;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GworkersCompClaim.
      /// </summary>
      [JsonPropertyName("gworkersCompClaim")]
      public WorkersCompClaim GworkersCompClaim
      {
        get => gworkersCompClaim ??= new();
        set => gworkersCompClaim = value;
      }

      /// <summary>
      /// A value of GexportDocketWorkArea.
      /// </summary>
      [JsonPropertyName("gexportDocketWorkArea")]
      public WorkArea GexportDocketWorkArea
      {
        get => gexportDocketWorkArea ??= new();
        set => gexportDocketWorkArea = value;
      }

      /// <summary>
      /// A value of GexportDocketCommon.
      /// </summary>
      [JsonPropertyName("gexportDocketCommon")]
      public Common GexportDocketCommon
      {
        get => gexportDocketCommon ??= new();
        set => gexportDocketCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common gcommon;
      private WorkersCompClaim gworkersCompClaim;
      private WorkArea gexportDocketWorkArea;
      private Common gexportDocketCommon;
    }

    /// <summary>A PagingGroup group.</summary>
    [Serializable]
    public class PagingGroup
    {
      /// <summary>
      /// A value of GexportPaging.
      /// </summary>
      [JsonPropertyName("gexportPaging")]
      public WorkersCompClaim GexportPaging
      {
        get => gexportPaging ??= new();
        set => gexportPaging = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private WorkersCompClaim gexportPaging;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of PromptDocketFilter.
    /// </summary>
    [JsonPropertyName("promptDocketFilter")]
    public Common PromptDocketFilter
    {
      get => promptDocketFilter ??= new();
      set => promptDocketFilter = value;
    }

    /// <summary>
    /// A value of DocketFilter.
    /// </summary>
    [JsonPropertyName("docketFilter")]
    public WorkArea DocketFilter
    {
      get => docketFilter ??= new();
      set => docketFilter = value;
    }

    /// <summary>
    /// A value of Docket.
    /// </summary>
    [JsonPropertyName("docket")]
    public Common Docket
    {
      get => docket ??= new();
      set => docket = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public WorkersCompClaim Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of MoreIndicator.
    /// </summary>
    [JsonPropertyName("moreIndicator")]
    public WorkArea MoreIndicator
    {
      get => moreIndicator ??= new();
      set => moreIndicator = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// Gets a value of Paging.
    /// </summary>
    [JsonIgnore]
    public Array<PagingGroup> Paging => paging ??= new(PagingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Paging for json serialization.
    /// </summary>
    [JsonPropertyName("paging")]
    [Computed]
    public IList<PagingGroup> Paging_Json
    {
      get => paging;
      set => Paging.Assign(value);
    }

    private Code prompt;
    private Common promptDocketFilter;
    private WorkArea docketFilter;
    private Common docket;
    private WorkersCompClaim selected;
    private WorkArea moreIndicator;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common promptPerson;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson hiddenCsePerson;
    private Common pageNumber;
    private Array<PagingGroup> paging;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public WorkersCompClaim NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common invalid;
    private DateWorkArea max;
    private Common recordCount;
    private WorkersCompClaim nextPage;
    private Common promptCount;
    private DateWorkArea null1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }
#endregion
}
