// Program: OE_CALS_FCR_CASE_LIST, ID: 374571689, model: 746.
// Short name: SWECALSP
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
/// A program: OE_CALS_FCR_CASE_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCalsFcrCaseList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CALS_FCR_CASE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCalsFcrCaseList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCalsFcrCaseList.
  /// </summary>
  public OeCalsFcrCaseList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date		Developer	Request #      Description
    // -------------------------------------------------------------------------------------
    // 07/21/2009	M Fan		CQ7190	       Initial Dev
    // 02/18/2010      Raj S           CQ7190         Added code to give access 
    // to the data
    //                                                
    // if the logged in user is a supervisor
    //                                                
    // to th selected case service provider.
    // 08/25/2010      LSS             CQ21409        Added FLOW from ALOM menu.
    //                                                
    // Added 'Display Successful' with display
    //                                                
    // and 'Displayed Successfully' for scrolling.
    //                                                
    // Removed code setting Starting Case# to
    //                                                
    // "0000000000".
    //                                                
    // Made Edit Screen changes.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_ALOM_MENU";

      return;
    }

    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -------------------------------------------------------
    // Move imports to exports
    // -------------------------------------------------------
    // CQ21409
    if (!IsEmpty(import.Pass.Number))
    {
      export.Starting.CaseId = import.Pass.Number;
    }
    else
    {
      export.Starting.CaseId = import.Starting.CaseId;
    }

    export.FilterSel.SelectChar = import.FilterSel.SelectChar;
    export.FilterFrom.Date = import.FilterFrom.Date;
    export.FilterTo.Date = import.FilterTo.Date;

    // CQ21409
    if (!IsEmpty(export.Starting.CaseId))
    {
      local.ZeroFill.Number = export.Starting.CaseId;
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Starting, "caseId");

        field.Error = true;

        return;
      }
      else
      {
        export.Starting.CaseId = local.ZeroFill.Number;
      }
    }

    export.HiddenFilterPrevStart.CaseId = import.HiddenFilterPrevStart.CaseId;
    export.HiddenFilterPrevSel.SelectChar =
      import.HiddenFilterPrevSel.SelectChar;
    export.HiddenFilterPrevFrom.Date = import.HiddenFilterPrevFrom.Date;
    export.HiddenFilterPrevTo.Date = import.HiddenFilterPrevTo.Date;
    export.HiddenPrevPageStarting.CaseId = import.HiddenPrevPageStarting.CaseId;
    export.HiddenPrevPageEndCase.CaseId = import.HiddenPrevPageEndCase.CaseId;
    export.HiddenPrevPageEndDate.Date = import.HiddenPrevPageEndDate.Date;
    export.HiddenMoreThan1TblFg.SelectChar =
      import.HiddenMoreThan1TblFg.SelectChar;
    MoveNonstandardScrolling(import.Scrolling, export.Scrolling);
    export.NextTran.NextTransaction = import.NextTran.NextTransaction;
    export.Hidden.Assign(import.Hidden);

    if (!Equal(global.Command, "DISPLAY"))
    {
      for(import.FcrCaseList.Index = 0; import.FcrCaseList.Index < import
        .FcrCaseList.Count; ++import.FcrCaseList.Index)
      {
        if (!import.FcrCaseList.CheckSize())
        {
          break;
        }

        export.FcrCaseList.Index = import.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        export.FcrCaseList.Update.SelectChar.SelectChar =
          import.FcrCaseList.Item.SelectChar.SelectChar;
        export.FcrCaseList.Update.FcrCaseInfo.Assign(
          import.FcrCaseList.Item.FcrCaseInfo);
      }

      import.FcrCaseList.CheckIndex();
    }

    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    if ((Equal(global.Command, "NEXT1") || Equal(global.Command, "PREV") || Equal
      (global.Command, "CADS") || Equal(global.Command, "MBLS")) && export
      .Scrolling.PageNumber < 1)
    {
      ExitState = "OE_0206_MUST_DISPLAY_FIRST";

      return;
    }

    // Check selection for command CADS, MBLS & RETURN
    if (Equal(global.Command, "CADS") || Equal(global.Command, "MBLS") || Equal
      (global.Command, "RETURN"))
    {
      for(export.FcrCaseList.Index = 0; export.FcrCaseList.Index < export
        .FcrCaseList.Count; ++export.FcrCaseList.Index)
      {
        if (!export.FcrCaseList.CheckSize())
        {
          break;
        }

        if (AsChar(export.FcrCaseList.Item.SelectChar.SelectChar) == 'S')
        {
          ++local.SelCount.Count;
          export.PassCaseNumber.Number =
            export.FcrCaseList.Item.FcrCaseInfo.CaseId;
          export.PassFcrCaseId.CaseId =
            export.FcrCaseList.Item.FcrCaseInfo.CaseId;
        }
        else if (!IsEmpty(export.FcrCaseList.Item.SelectChar.SelectChar))
        {
          var field =
            GetField(export.FcrCaseList.Item.SelectChar, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
      }

      export.FcrCaseList.CheckIndex();

      if (local.SelCount.Count > 1)
      {
        ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

        return;
      }

      if (Equal(global.Command, "RETURN"))
      {
        ExitState = "ACO_NE0000_RETURN_NM";

        return;
      }

      if (local.SelCount.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    // Check if the user requested a next tran action
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Starting.CaseId;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // Check if the user is comming into this procedure on a next tran action.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Starting.CaseId = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    // Check if flowing from the menu
    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // Validate security
    if (Equal(global.Command, "DISPLAY"))
    {
      local.Pass.Number = export.Starting.CaseId;
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ********************************************************************************************
        // The below code ESCAPE code will be removed and supervisor check code 
        // will be uncommented out
        // once business decides to give access to field workers. The code to 
        // check the logged in user
        // is the supervisor for the selected CASE is very similar to FPLS 
        // screen code.
        // ********************************************************************************************
        return;
      }
    }

    // ---------------------------------------------------------------
    //              *** Edit Screen ***
    // ---------------------------------------------------------------
    // CQ21409
    if (IsEmpty(export.Starting.CaseId) && IsEmpty
      (export.FilterSel.SelectChar) && Equal
      (export.FilterFrom.Date, local.Null1.Date))
    {
      var field1 = GetField(export.Starting, "caseId");

      field1.Error = true;

      var field2 = GetField(export.FilterSel, "selectChar");

      field2.Error = true;

      ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

      return;
    }

    // CQ21409 - added edit
    if (Lt(local.Null1.Date, export.FilterFrom.Date) && IsEmpty
      (export.Starting.CaseId) && IsEmpty(export.FilterSel.SelectChar))
    {
      var field = GetField(export.FilterSel, "selectChar");

      field.Error = true;

      ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

      return;
    }

    if (!IsEmpty(export.Starting.CaseId) && (
      !IsEmpty(export.FilterSel.SelectChar) || Lt
      (local.Null1.Date, export.FilterFrom.Date) || Lt
      (local.Null1.Date, export.FilterTo.Date)))
    {
      var field = GetField(export.Starting, "caseId");

      field.Error = true;

      ExitState = "OE_0204_INVALID_SELECTION";

      return;
    }

    if (AsChar(export.FilterSel.SelectChar) != 'S' && AsChar
      (export.FilterSel.SelectChar) != 'R' && !
      IsEmpty(export.FilterSel.SelectChar))
    {
      var field = GetField(export.FilterSel, "selectChar");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

      return;
    }

    if (AsChar(export.FilterSel.SelectChar) == 'S' || AsChar
      (export.FilterSel.SelectChar) == 'R')
    {
      if (!Lt(local.Null1.Date, export.FilterFrom.Date))
      {
        var field = GetField(export.FilterFrom, "date");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
      else if (!Lt(local.Null1.Date, export.FilterTo.Date))
      {
        export.FilterTo.Date = new DateTime(2099, 12, 31);
      }
      else if (Lt(export.FilterTo.Date, export.FilterFrom.Date))
      {
        var field = GetField(export.FilterFrom, "date");

        field.Error = true;

        ExitState = "OE0000_FROM_DATE_SHOULD_BE_LESS";

        return;
      }
    }

    if ((!Equal(export.Starting.CaseId, export.HiddenFilterPrevStart.CaseId) ||
      AsChar(export.FilterSel.SelectChar) != AsChar
      (export.HiddenFilterPrevSel.SelectChar) || !
      Equal(export.FilterFrom.Date, export.HiddenFilterPrevFrom.Date) || !
      Equal(export.FilterTo.Date, export.HiddenFilterPrevTo.Date)) && !
      Equal(global.Command, "DISPLAY"))
    {
      ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

      return;
    }

    // --------------------------------------------------------
    //         M A I N   C A S E   O F   C O M M A N D
    // --------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.HiddenFilterPrevStart.CaseId = export.Starting.CaseId;
        export.HiddenFilterPrevSel.SelectChar = export.FilterSel.SelectChar;
        export.HiddenFilterPrevFrom.Date = export.FilterFrom.Date;
        export.HiddenFilterPrevTo.Date = export.FilterTo.Date;

        if (AsChar(export.FilterSel.SelectChar) == 'S' || AsChar
          (export.FilterSel.SelectChar) == 'R')
        {
          UseOeCalsBuildFcrCaseList5();
        }
        else
        {
          UseOeCalsBuildFcrCaseList4();
        }

        if (export.FcrCaseList.IsEmpty)
        {
          export.Scrolling.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.Scrolling.PageNumber = 1;

        break;
      case "NEXT1":
        if (AsChar(export.HiddenMoreThan1TblFg.SelectChar) != 'Y')
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        if (AsChar(export.FilterSel.SelectChar) == 'S' || AsChar
          (export.FilterSel.SelectChar) == 'R')
        {
          export.FcrCaseList.Index = 5;
          export.FcrCaseList.CheckSize();

          export.HiddenPrevPageEndCase.CaseId =
            export.FcrCaseList.Item.FcrCaseInfo.CaseId;

          if (AsChar(export.FilterSel.SelectChar) == 'S')
          {
            export.HiddenPrevPageEndDate.Date =
              export.FcrCaseList.Item.FcrCaseInfo.CaseSentDateToFcr;
          }
          else
          {
            export.HiddenPrevPageEndDate.Date =
              export.FcrCaseList.Item.FcrCaseInfo.FcrCaseResponseDate;
          }

          // ********************************
          export.FcrCaseList.Index = 5;
          export.FcrCaseList.CheckSize();

          export.HiddenPrevPageEndCase.CaseId =
            export.FcrCaseList.Item.FcrCaseInfo.CaseId;

          if (AsChar(export.FilterSel.SelectChar) == 'S')
          {
            export.HiddenPrevPageEndDate.Date =
              export.FcrCaseList.Item.FcrCaseInfo.CaseSentDateToFcr;
          }
          else
          {
            export.HiddenPrevPageEndDate.Date =
              export.FcrCaseList.Item.FcrCaseInfo.FcrCaseResponseDate;
          }

          UseOeCalsBuildFcrCaseList2();
        }
        else
        {
          export.FcrCaseList.Index = 5;
          export.FcrCaseList.CheckSize();

          export.HiddenPrevPageEndCase.CaseId =
            export.FcrCaseList.Item.FcrCaseInfo.CaseId;
          UseOeCalsBuildFcrCaseList3();
        }

        if (export.FcrCaseList.IsEmpty)
        {
          export.Scrolling.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        ++export.Scrolling.PageNumber;

        break;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        if (AsChar(export.FilterSel.SelectChar) == 'S' || AsChar
          (export.FilterSel.SelectChar) == 'R')
        {
          if (export.Scrolling.PageNumber == 2)
          {
            export.HiddenPrevPageEndDate.Date = local.Null1.Date;
            export.HiddenPrevPageEndCase.CaseId = "";
            global.Command = "DISPLAY";
          }
          else
          {
            export.FcrCaseList.Index = 0;
            export.FcrCaseList.CheckSize();

            export.HiddenPrevPageEndCase.CaseId =
              export.FcrCaseList.Item.FcrCaseInfo.CaseId;

            if (AsChar(export.FilterSel.SelectChar) == 'S')
            {
              export.HiddenPrevPageEndDate.Date =
                export.FcrCaseList.Item.FcrCaseInfo.CaseSentDateToFcr;
            }
            else
            {
              export.HiddenPrevPageEndDate.Date =
                export.FcrCaseList.Item.FcrCaseInfo.FcrCaseResponseDate;
            }
          }

          UseOeCalsBuildFcrCaseList2();
        }
        else
        {
          if (export.Scrolling.PageNumber == 2)
          {
            global.Command = "DISPLAY";
          }
          else
          {
            export.FcrCaseList.Index = 0;
            export.FcrCaseList.CheckSize();

            export.HiddenPrevPageEndCase.CaseId =
              export.FcrCaseList.Item.FcrCaseInfo.CaseId;
          }

          UseOeCalsBuildFcrCaseList1();
        }

        if (export.FcrCaseList.IsEmpty)
        {
          export.Scrolling.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        --export.Scrolling.PageNumber;

        break;
      case "CADS":
        ExitState = "ECO_LNK_TO_LST_CASE_DETAILS";

        return;
      case "MBLS":
        ExitState = "ECO_LINK_TO_MBLS";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (export.Scrolling.PageNumber == 1)
    {
      if (AsChar(export.HiddenMoreThan1TblFg.SelectChar) == 'Y')
      {
        export.Scrolling.ScrollingMessage = "More  +";
      }
      else
      {
        export.Scrolling.ScrollingMessage = "More";
      }
    }
    else if (AsChar(export.HiddenMoreThan1TblFg.SelectChar) == 'Y')
    {
      export.Scrolling.ScrollingMessage = "More - +";
    }
    else
    {
      export.Scrolling.ScrollingMessage = "More  -";
    }
  }

  private static void MoveFcrCaseList(OeCalsBuildFcrCaseList.Export.
    FcrCaseListGroup source, Export.FcrCaseListGroup target)
  {
    target.SelectChar.SelectChar = source.SelectChar.SelectChar;
    target.FcrCaseInfo.Assign(source.FcrCaseInfo);
  }

  private static void MoveNonstandardScrolling(NonstandardScrolling source,
    NonstandardScrolling target)
  {
    target.PageNumber = source.PageNumber;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.ZeroFill.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.ZeroFill.Number = useImport.Case1.Number;
  }

  private void UseOeCalsBuildFcrCaseList1()
  {
    var useImport = new OeCalsBuildFcrCaseList.Import();
    var useExport = new OeCalsBuildFcrCaseList.Export();

    useImport.PrevPageEndingCase.CaseId = export.HiddenPrevPageEndCase.CaseId;
    useImport.Starting.CaseId = export.Starting.CaseId;

    Call(OeCalsBuildFcrCaseList.Execute, useImport, useExport);

    export.HiddenPrevPageEndCase.CaseId = useExport.PrevPageEndCase.CaseId;
    export.HiddenMoreThan1TblFg.SelectChar =
      useExport.MoreThan1Table.SelectChar;
    useExport.FcrCaseList.CopyTo(export.FcrCaseList, MoveFcrCaseList);
  }

  private void UseOeCalsBuildFcrCaseList2()
  {
    var useImport = new OeCalsBuildFcrCaseList.Import();
    var useExport = new OeCalsBuildFcrCaseList.Export();

    useImport.PrevPageEndingDate.Date = export.HiddenPrevPageEndDate.Date;
    useImport.PrevPageEndingCase.CaseId = export.HiddenPrevPageEndCase.CaseId;
    useImport.Sel.SelectChar = export.FilterSel.SelectChar;
    useImport.From.Date = export.FilterFrom.Date;
    useImport.To.Date = export.FilterTo.Date;

    Call(OeCalsBuildFcrCaseList.Execute, useImport, useExport);

    export.HiddenMoreThan1TblFg.SelectChar =
      useExport.MoreThan1Table.SelectChar;
    useExport.FcrCaseList.CopyTo(export.FcrCaseList, MoveFcrCaseList);
  }

  private void UseOeCalsBuildFcrCaseList3()
  {
    var useImport = new OeCalsBuildFcrCaseList.Import();
    var useExport = new OeCalsBuildFcrCaseList.Export();

    useImport.PrevPageEndingCase.CaseId = export.HiddenPrevPageEndCase.CaseId;
    useImport.Starting.CaseId = export.Starting.CaseId;

    Call(OeCalsBuildFcrCaseList.Execute, useImport, useExport);

    export.HiddenMoreThan1TblFg.SelectChar =
      useExport.MoreThan1Table.SelectChar;
    useExport.FcrCaseList.CopyTo(export.FcrCaseList, MoveFcrCaseList);
  }

  private void UseOeCalsBuildFcrCaseList4()
  {
    var useImport = new OeCalsBuildFcrCaseList.Import();
    var useExport = new OeCalsBuildFcrCaseList.Export();

    useImport.Starting.CaseId = export.Starting.CaseId;

    Call(OeCalsBuildFcrCaseList.Execute, useImport, useExport);

    export.HiddenMoreThan1TblFg.SelectChar =
      useExport.MoreThan1Table.SelectChar;
    useExport.FcrCaseList.CopyTo(export.FcrCaseList, MoveFcrCaseList);
  }

  private void UseOeCalsBuildFcrCaseList5()
  {
    var useImport = new OeCalsBuildFcrCaseList.Import();
    var useExport = new OeCalsBuildFcrCaseList.Export();

    useImport.Sel.SelectChar = export.FilterSel.SelectChar;
    useImport.From.Date = export.FilterFrom.Date;
    useImport.To.Date = export.FilterTo.Date;

    Call(OeCalsBuildFcrCaseList.Execute, useImport, useExport);

    export.HiddenMoreThan1TblFg.SelectChar =
      useExport.MoreThan1Table.SelectChar;
    useExport.FcrCaseList.CopyTo(export.FcrCaseList, MoveFcrCaseList);
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
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.Case1.Number = local.Pass.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
    /// <summary>A FcrCaseListGroup group.</summary>
    [Serializable]
    public class FcrCaseListGroup
    {
      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of FcrCaseInfo.
      /// </summary>
      [JsonPropertyName("fcrCaseInfo")]
      public FcrCaseMaster FcrCaseInfo
      {
        get => fcrCaseInfo ??= new();
        set => fcrCaseInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common selectChar;
      private FcrCaseMaster fcrCaseInfo;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Case1 Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageEndCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageEndCase")]
    public FcrCaseMaster HiddenPrevPageEndCase
    {
      get => hiddenPrevPageEndCase ??= new();
      set => hiddenPrevPageEndCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageEndDate.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageEndDate")]
    public DateWorkArea HiddenPrevPageEndDate
    {
      get => hiddenPrevPageEndDate ??= new();
      set => hiddenPrevPageEndDate = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public FcrCaseMaster Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FilterSel.
    /// </summary>
    [JsonPropertyName("filterSel")]
    public Common FilterSel
    {
      get => filterSel ??= new();
      set => filterSel = value;
    }

    /// <summary>
    /// A value of FilterFrom.
    /// </summary>
    [JsonPropertyName("filterFrom")]
    public DateWorkArea FilterFrom
    {
      get => filterFrom ??= new();
      set => filterFrom = value;
    }

    /// <summary>
    /// A value of FilterTo.
    /// </summary>
    [JsonPropertyName("filterTo")]
    public DateWorkArea FilterTo
    {
      get => filterTo ??= new();
      set => filterTo = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevStart.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevStart")]
    public FcrCaseMaster HiddenFilterPrevStart
    {
      get => hiddenFilterPrevStart ??= new();
      set => hiddenFilterPrevStart = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevSel.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevSel")]
    public Common HiddenFilterPrevSel
    {
      get => hiddenFilterPrevSel ??= new();
      set => hiddenFilterPrevSel = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevFrom.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevFrom")]
    public DateWorkArea HiddenFilterPrevFrom
    {
      get => hiddenFilterPrevFrom ??= new();
      set => hiddenFilterPrevFrom = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevTo.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevTo")]
    public DateWorkArea HiddenFilterPrevTo
    {
      get => hiddenFilterPrevTo ??= new();
      set => hiddenFilterPrevTo = value;
    }

    /// <summary>
    /// A value of HiddenMoreThan1TblFg.
    /// </summary>
    [JsonPropertyName("hiddenMoreThan1TblFg")]
    public Common HiddenMoreThan1TblFg
    {
      get => hiddenMoreThan1TblFg ??= new();
      set => hiddenMoreThan1TblFg = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageStarting.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageStarting")]
    public FcrCaseMaster HiddenPrevPageStarting
    {
      get => hiddenPrevPageStarting ??= new();
      set => hiddenPrevPageStarting = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public NonstandardScrolling Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Standard NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
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
    /// Gets a value of FcrCaseList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrCaseListGroup> FcrCaseList => fcrCaseList ??= new(
      FcrCaseListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FcrCaseList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrCaseList")]
    [Computed]
    public IList<FcrCaseListGroup> FcrCaseList_Json
    {
      get => fcrCaseList;
      set => FcrCaseList.Assign(value);
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

    private Case1 pass;
    private FcrCaseMaster hiddenPrevPageEndCase;
    private DateWorkArea hiddenPrevPageEndDate;
    private FcrCaseMaster starting;
    private Common filterSel;
    private DateWorkArea filterFrom;
    private DateWorkArea filterTo;
    private FcrCaseMaster hiddenFilterPrevStart;
    private Common hiddenFilterPrevSel;
    private DateWorkArea hiddenFilterPrevFrom;
    private DateWorkArea hiddenFilterPrevTo;
    private Common hiddenMoreThan1TblFg;
    private FcrCaseMaster hiddenPrevPageStarting;
    private NonstandardScrolling scrolling;
    private Standard nextTran;
    private NextTranInfo hidden;
    private Array<FcrCaseListGroup> fcrCaseList;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FcrCaseListGroup group.</summary>
    [Serializable]
    public class FcrCaseListGroup
    {
      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of FcrCaseInfo.
      /// </summary>
      [JsonPropertyName("fcrCaseInfo")]
      public FcrCaseMaster FcrCaseInfo
      {
        get => fcrCaseInfo ??= new();
        set => fcrCaseInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common selectChar;
      private FcrCaseMaster fcrCaseInfo;
    }

    /// <summary>
    /// A value of PassCaseNumber.
    /// </summary>
    [JsonPropertyName("passCaseNumber")]
    public Case1 PassCaseNumber
    {
      get => passCaseNumber ??= new();
      set => passCaseNumber = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageEndCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageEndCase")]
    public FcrCaseMaster HiddenPrevPageEndCase
    {
      get => hiddenPrevPageEndCase ??= new();
      set => hiddenPrevPageEndCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageEndDate.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageEndDate")]
    public DateWorkArea HiddenPrevPageEndDate
    {
      get => hiddenPrevPageEndDate ??= new();
      set => hiddenPrevPageEndDate = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public FcrCaseMaster Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FilterSel.
    /// </summary>
    [JsonPropertyName("filterSel")]
    public Common FilterSel
    {
      get => filterSel ??= new();
      set => filterSel = value;
    }

    /// <summary>
    /// A value of FilterFrom.
    /// </summary>
    [JsonPropertyName("filterFrom")]
    public DateWorkArea FilterFrom
    {
      get => filterFrom ??= new();
      set => filterFrom = value;
    }

    /// <summary>
    /// A value of FilterTo.
    /// </summary>
    [JsonPropertyName("filterTo")]
    public DateWorkArea FilterTo
    {
      get => filterTo ??= new();
      set => filterTo = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevStart.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevStart")]
    public FcrCaseMaster HiddenFilterPrevStart
    {
      get => hiddenFilterPrevStart ??= new();
      set => hiddenFilterPrevStart = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevSel.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevSel")]
    public Common HiddenFilterPrevSel
    {
      get => hiddenFilterPrevSel ??= new();
      set => hiddenFilterPrevSel = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevFrom.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevFrom")]
    public DateWorkArea HiddenFilterPrevFrom
    {
      get => hiddenFilterPrevFrom ??= new();
      set => hiddenFilterPrevFrom = value;
    }

    /// <summary>
    /// A value of HiddenFilterPrevTo.
    /// </summary>
    [JsonPropertyName("hiddenFilterPrevTo")]
    public DateWorkArea HiddenFilterPrevTo
    {
      get => hiddenFilterPrevTo ??= new();
      set => hiddenFilterPrevTo = value;
    }

    /// <summary>
    /// A value of HiddenMoreThan1TblFg.
    /// </summary>
    [JsonPropertyName("hiddenMoreThan1TblFg")]
    public Common HiddenMoreThan1TblFg
    {
      get => hiddenMoreThan1TblFg ??= new();
      set => hiddenMoreThan1TblFg = value;
    }

    /// <summary>
    /// A value of HiddenPrevPageStarting.
    /// </summary>
    [JsonPropertyName("hiddenPrevPageStarting")]
    public FcrCaseMaster HiddenPrevPageStarting
    {
      get => hiddenPrevPageStarting ??= new();
      set => hiddenPrevPageStarting = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public NonstandardScrolling Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Standard NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
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
    /// Gets a value of FcrCaseList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrCaseListGroup> FcrCaseList => fcrCaseList ??= new(
      FcrCaseListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FcrCaseList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrCaseList")]
    [Computed]
    public IList<FcrCaseListGroup> FcrCaseList_Json
    {
      get => fcrCaseList;
      set => FcrCaseList.Assign(value);
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
    /// A value of PassFcrCaseId.
    /// </summary>
    [JsonPropertyName("passFcrCaseId")]
    public FcrCaseMaster PassFcrCaseId
    {
      get => passFcrCaseId ??= new();
      set => passFcrCaseId = value;
    }

    private Case1 passCaseNumber;
    private FcrCaseMaster hiddenPrevPageEndCase;
    private DateWorkArea hiddenPrevPageEndDate;
    private FcrCaseMaster starting;
    private Common filterSel;
    private DateWorkArea filterFrom;
    private DateWorkArea filterTo;
    private FcrCaseMaster hiddenFilterPrevStart;
    private Common hiddenFilterPrevSel;
    private DateWorkArea hiddenFilterPrevFrom;
    private DateWorkArea hiddenFilterPrevTo;
    private Common hiddenMoreThan1TblFg;
    private FcrCaseMaster hiddenPrevPageStarting;
    private NonstandardScrolling scrolling;
    private Standard nextTran;
    private NextTranInfo hidden;
    private Array<FcrCaseListGroup> fcrCaseList;
    private Standard standard;
    private FcrCaseMaster passFcrCaseId;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of TableFullFlag.
    /// </summary>
    [JsonPropertyName("tableFullFlag")]
    public WorkArea TableFullFlag
    {
      get => tableFullFlag ??= new();
      set => tableFullFlag = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public Case1 ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentServiceProvider.
    /// </summary>
    [JsonPropertyName("currentServiceProvider")]
    public ServiceProvider CurrentServiceProvider
    {
      get => currentServiceProvider ??= new();
      set => currentServiceProvider = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Case1 Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of AccessOnThisCase.
    /// </summary>
    [JsonPropertyName("accessOnThisCase")]
    public Common AccessOnThisCase
    {
      get => accessOnThisCase ??= new();
      set => accessOnThisCase = value;
    }

    private Common supervisor;
    private WorkArea tableFullFlag;
    private DateWorkArea null1;
    private Case1 zeroFill;
    private Common selCount;
    private DateWorkArea currentDateWorkArea;
    private ServiceProvider currentServiceProvider;
    private Case1 pass;
    private Common accessOnThisCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    private FcrCaseMaster fcrCaseMaster;
  }
#endregion
}
