// Program: OE_MBLS_FCR_CASE_MEMBER_LIST, ID: 374576014, model: 746.
// Short name: SWEMBLSP
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
/// A program: OE_MBLS_FCR_CASE_MEMBER_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeMblsFcrCaseMemberList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MBLS_FCR_CASE_MEMBER_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMblsFcrCaseMemberList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMblsFcrCaseMemberList.
  /// </summary>
  public OeMblsFcrCaseMemberList(IContext context, Import import, Export export):
    
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
    // Date		Developer	Request #      Description
    // -------------------------------------------------------------------------------------
    // 09/02/2009	M Fan		CQ7190	       Initial Dev
    // 02/18/2010      Raj S           CQ7190         Added code to give access 
    // to the data
    //                                                
    // if the logged in user is a supervisor
    //                                                
    // to th selected case service provider.
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

    // *********************************************************************************
    // When user issues a CLEAR command, preservce only the next tran info and 
    // go back to
    // screen to clear all screen values.
    // *********************************************************************************
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN_NM";

      return;
    }

    export.FilterCaseId.CaseId = import.FilterCaseId.CaseId;

    if (!IsEmpty(export.FilterCaseId.CaseId))
    {
      local.ZeroFill.Number = export.FilterCaseId.CaseId;
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.FilterCaseId, "caseId");

        field.Error = true;

        return;
      }
      else
      {
        export.FilterCaseId.CaseId = local.ZeroFill.Number;
      }
    }

    export.CasePrompt.SelectChar = import.CasePrompt.SelectChar;
    export.HiddnFilterPrevCaseId.CaseId = import.HiddnFilterPrevCaseId.CaseId;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveStandard(import.Scrolling, export.Scrolling);

    // *********************************************************************************
    // Check if user requested next tran action.
    // *********************************************************************************
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.FilterCaseId.CaseId;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // *********************************************************************************
    // Check if the user is comming into this procedure on a next tran action.
    // *********************************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.FilterCaseId.CaseId = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    // *********************************************************************************
    // Check if flowing from the menu
    // *********************************************************************************
    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      if (!IsEmpty(import.PassFcrCaseId.CaseId))
      {
        export.FilterCaseId.CaseId = import.PassFcrCaseId.CaseId;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.CasePrompt.SelectChar) == 'S')
      {
        export.CasePrompt.SelectChar = "";
      }
    }
    else
    {
      export.FcrMemberList.Index = 0;
      export.FcrMemberList.Clear();

      for(import.FcrMemberList.Index = 0; import.FcrMemberList.Index < import
        .FcrMemberList.Count; ++import.FcrMemberList.Index)
      {
        if (export.FcrMemberList.IsFull)
        {
          break;
        }

        export.FcrMemberList.Update.SelectChar.SelectChar =
          import.FcrMemberList.Item.SelectChar.SelectChar;
        export.FcrMemberList.Update.FcrCaseMembers.Assign(
          import.FcrMemberList.Item.FcrCaseMembers);

        switch(TrimEnd(export.FcrMemberList.Item.FcrCaseMembers.
          ParticipantType ?? ""))
        {
          case "AP":
            export.PassAp.Number =
              export.FcrMemberList.Item.FcrCaseMembers.MemberId;

            break;
          case "AR":
            export.PassAr.Number =
              export.FcrMemberList.Item.FcrCaseMembers.MemberId;
            export.PassAr.FirstName =
              export.FcrMemberList.Item.FcrCaseMembers.FirstName ?? Spaces(12);
            export.PassAr.LastName =
              export.FcrMemberList.Item.FcrCaseMembers.LastName ?? Spaces(17);
            export.PassAr.MiddleInitial =
              export.FcrMemberList.Item.FcrCaseMembers.MiddleName ?? Spaces(1);

            break;
          default:
            break;
        }

        export.FcrMemberList.Next();
      }
    }

    // Check FCR case ID prompt
    switch(AsChar(export.CasePrompt.SelectChar))
    {
      case 'S':
        if (!Equal(global.Command, "LIST"))
        {
          var field1 = GetField(export.CasePrompt, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }
        else
        {
          ExitState = "ECO_LINK_TO_CALS";

          return;
        }

        break;
      case '+':
        break;
      case ' ':
        break;
      default:
        var field = GetField(export.CasePrompt, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
    }

    if (Equal(global.Command, "LIST"))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    // *********************************************************************************
    // Check to see whether Case Id is modified by the user and the command is 
    // NOT
    // DISPLAY then dipslay error message stating display first.
    // *********************************************************************************
    if (!Equal(global.Command, "DISPLAY") && !
      Equal(export.HiddnFilterPrevCaseId.CaseId, export.FilterCaseId.CaseId))
    {
      ExitState = "OE_0206_MUST_DISPLAY_FIRST";

      return;
    }

    // *********************************************************************************
    // Validate security
    // *********************************************************************************
    if (Equal(global.Command, "DISPLAY"))
    {
      local.Pass.Number = export.FilterCaseId.CaseId;
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
    if (IsEmpty(export.FilterCaseId.CaseId))
    {
      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      return;
    }

    // *********************************************************************************
    // Check selection for command DETAIL, APDS, ARDS, CHDS, & ALTS
    // *********************************************************************************
    if (Equal(global.Command, "DETAIL") || Equal(global.Command, "APDS") || Equal
      (global.Command, "ARDS") || Equal(global.Command, "CHDS") || Equal
      (global.Command, "ALTS"))
    {
      export.PassCaseNumber.Number = export.FilterCaseId.CaseId;

      for(export.FcrMemberList.Index = 0; export.FcrMemberList.Index < export
        .FcrMemberList.Count; ++export.FcrMemberList.Index)
      {
        if (AsChar(export.FcrMemberList.Item.SelectChar.SelectChar) == 'S')
        {
          ++local.SelCount.Count;
          export.PassMemberId.MemberId =
            export.FcrMemberList.Item.FcrCaseMembers.MemberId;

          if (Equal(export.FcrMemberList.Item.FcrCaseMembers.ParticipantType,
            "CH"))
          {
            export.PassCh.Number =
              export.FcrMemberList.Item.FcrCaseMembers.MemberId;
            export.PassCh.FirstName =
              export.FcrMemberList.Item.FcrCaseMembers.FirstName ?? Spaces(12);
            export.PassCh.LastName =
              export.FcrMemberList.Item.FcrCaseMembers.LastName ?? Spaces(17);
            export.PassCh.MiddleInitial =
              export.FcrMemberList.Item.FcrCaseMembers.MiddleName ?? Spaces(1);
          }
        }
        else if (!IsEmpty(export.FcrMemberList.Item.SelectChar.SelectChar))
        {
          var field =
            GetField(export.FcrMemberList.Item.SelectChar, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECTION";

          return;
        }
      }

      if (local.SelCount.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      if (local.SelCount.Count > 1)
      {
        ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

        return;
      }
    }

    // --------------------------------------------------------
    //         M A I N   C A S E   O F   C O M M A N D
    // --------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.HiddnFilterPrevCaseId.CaseId = export.FilterCaseId.CaseId;
        UseOeFcrvBuildFcrCaseView();

        if (export.FcrMemberList.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
        }
        else if (export.FcrMemberList.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PAGE";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "CHDS":
        if (IsEmpty(export.PassCh.Number) || Equal
          (export.PassCh.Number, "0000000000"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECTION";

          return;
        }

        ExitState = "ECO_LNK_TO_CHDS";

        break;
      case "APDS":
        if (IsEmpty(export.PassAp.Number) || Equal
          (export.PassAp.Number, "0000000000"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECTION";

          return;
        }

        ExitState = "ECO_LNK_TO_AP_DETAILS";

        break;
      case "ARDS":
        if (IsEmpty(export.PassAr.Number) || Equal
          (export.PassAr.Number, "0000000000"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECTION";

          return;
        }

        ExitState = "ECO_LNK_TO_AR_DETAILS";

        break;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        break;
      case "DETAIL":
        ExitState = "ECO_LINK_TO_MBDT";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveFcrCaseMaster(FcrCaseMaster source,
    FcrCaseMaster target)
  {
    target.CaseSentDateToFcr = source.CaseSentDateToFcr;
    target.FcrCaseResponseDate = source.FcrCaseResponseDate;
  }

  private static void MoveFcrMemberList(OeFcrvBuildFcrCaseView.Export.
    FcrMemberListGroup source, Export.FcrMemberListGroup target)
  {
    MoveFcrCaseMaster(source.FcrCase, target.FcrCase);
    target.Comma.SelectChar = source.Comma.SelectChar;
    target.SelectChar.SelectChar = source.SelChar.SelectChar;
    target.FcrCaseMembers.Assign(source.FcrMember);
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.ZeroFill.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.ZeroFill.Number = useImport.Case1.Number;
  }

  private void UseOeFcrvBuildFcrCaseView()
  {
    var useImport = new OeFcrvBuildFcrCaseView.Import();
    var useExport = new OeFcrvBuildFcrCaseView.Export();

    useImport.FcrCaseId.CaseId = export.FilterCaseId.CaseId;

    Call(OeFcrvBuildFcrCaseView.Execute, useImport, useExport);

    useExport.FcrMemberList.CopyTo(export.FcrMemberList, MoveFcrMemberList);
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
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A FcrMemberListGroup group.</summary>
    [Serializable]
    public class FcrMemberListGroup
    {
      /// <summary>
      /// A value of FcrCase.
      /// </summary>
      [JsonPropertyName("fcrCase")]
      public FcrCaseMaster FcrCase
      {
        get => fcrCase ??= new();
        set => fcrCase = value;
      }

      /// <summary>
      /// A value of Comma.
      /// </summary>
      [JsonPropertyName("comma")]
      public Common Comma
      {
        get => comma ??= new();
        set => comma = value;
      }

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
      /// A value of FcrCaseMembers.
      /// </summary>
      [JsonPropertyName("fcrCaseMembers")]
      public FcrCaseMembers FcrCaseMembers
      {
        get => fcrCaseMembers ??= new();
        set => fcrCaseMembers = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selectChar;
      private FcrCaseMembers fcrCaseMembers;
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

    /// <summary>
    /// Gets a value of FcrMemberList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrMemberListGroup> FcrMemberList => fcrMemberList ??= new(
      FcrMemberListGroup.Capacity);

    /// <summary>
    /// Gets a value of FcrMemberList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrMemberList")]
    [Computed]
    public IList<FcrMemberListGroup> FcrMemberList_Json
    {
      get => fcrMemberList;
      set => FcrMemberList.Assign(value);
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
    /// A value of FilterCaseId.
    /// </summary>
    [JsonPropertyName("filterCaseId")]
    public FcrCaseMaster FilterCaseId
    {
      get => filterCaseId ??= new();
      set => filterCaseId = value;
    }

    /// <summary>
    /// A value of CasePrompt.
    /// </summary>
    [JsonPropertyName("casePrompt")]
    public Common CasePrompt
    {
      get => casePrompt ??= new();
      set => casePrompt = value;
    }

    /// <summary>
    /// A value of HiddnFilterPrevCaseId.
    /// </summary>
    [JsonPropertyName("hiddnFilterPrevCaseId")]
    public FcrCaseMaster HiddnFilterPrevCaseId
    {
      get => hiddnFilterPrevCaseId ??= new();
      set => hiddnFilterPrevCaseId = value;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    private FcrCaseMaster passFcrCaseId;
    private Array<FcrMemberListGroup> fcrMemberList;
    private NextTranInfo hidden;
    private FcrCaseMaster filterCaseId;
    private Common casePrompt;
    private FcrCaseMaster hiddnFilterPrevCaseId;
    private Standard standard;
    private Standard scrolling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FcrMemberListGroup group.</summary>
    [Serializable]
    public class FcrMemberListGroup
    {
      /// <summary>
      /// A value of FcrCase.
      /// </summary>
      [JsonPropertyName("fcrCase")]
      public FcrCaseMaster FcrCase
      {
        get => fcrCase ??= new();
        set => fcrCase = value;
      }

      /// <summary>
      /// A value of Comma.
      /// </summary>
      [JsonPropertyName("comma")]
      public Common Comma
      {
        get => comma ??= new();
        set => comma = value;
      }

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
      /// A value of FcrCaseMembers.
      /// </summary>
      [JsonPropertyName("fcrCaseMembers")]
      public FcrCaseMembers FcrCaseMembers
      {
        get => fcrCaseMembers ??= new();
        set => fcrCaseMembers = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selectChar;
      private FcrCaseMembers fcrCaseMembers;
    }

    /// <summary>
    /// A value of PassCh.
    /// </summary>
    [JsonPropertyName("passCh")]
    public CsePersonsWorkSet PassCh
    {
      get => passCh ??= new();
      set => passCh = value;
    }

    /// <summary>
    /// A value of PassAr.
    /// </summary>
    [JsonPropertyName("passAr")]
    public CsePersonsWorkSet PassAr
    {
      get => passAr ??= new();
      set => passAr = value;
    }

    /// <summary>
    /// A value of PassAp.
    /// </summary>
    [JsonPropertyName("passAp")]
    public CsePersonsWorkSet PassAp
    {
      get => passAp ??= new();
      set => passAp = value;
    }

    /// <summary>
    /// A value of ShowHistoryFlag.
    /// </summary>
    [JsonPropertyName("showHistoryFlag")]
    public Common ShowHistoryFlag
    {
      get => showHistoryFlag ??= new();
      set => showHistoryFlag = value;
    }

    /// <summary>
    /// Gets a value of FcrMemberList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrMemberListGroup> FcrMemberList => fcrMemberList ??= new(
      FcrMemberListGroup.Capacity);

    /// <summary>
    /// Gets a value of FcrMemberList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrMemberList")]
    [Computed]
    public IList<FcrMemberListGroup> FcrMemberList_Json
    {
      get => fcrMemberList;
      set => FcrMemberList.Assign(value);
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
    /// A value of FilterCaseId.
    /// </summary>
    [JsonPropertyName("filterCaseId")]
    public FcrCaseMaster FilterCaseId
    {
      get => filterCaseId ??= new();
      set => filterCaseId = value;
    }

    /// <summary>
    /// A value of CasePrompt.
    /// </summary>
    [JsonPropertyName("casePrompt")]
    public Common CasePrompt
    {
      get => casePrompt ??= new();
      set => casePrompt = value;
    }

    /// <summary>
    /// A value of HiddnFilterPrevCaseId.
    /// </summary>
    [JsonPropertyName("hiddnFilterPrevCaseId")]
    public FcrCaseMaster HiddnFilterPrevCaseId
    {
      get => hiddnFilterPrevCaseId ??= new();
      set => hiddnFilterPrevCaseId = value;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
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
    /// A value of PassMemberId.
    /// </summary>
    [JsonPropertyName("passMemberId")]
    public FcrCaseMembers PassMemberId
    {
      get => passMemberId ??= new();
      set => passMemberId = value;
    }

    private CsePersonsWorkSet passCh;
    private CsePersonsWorkSet passAr;
    private CsePersonsWorkSet passAp;
    private Common showHistoryFlag;
    private Array<FcrMemberListGroup> fcrMemberList;
    private NextTranInfo hidden;
    private FcrCaseMaster filterCaseId;
    private Common casePrompt;
    private FcrCaseMaster hiddnFilterPrevCaseId;
    private Standard standard;
    private Standard scrolling;
    private Case1 passCaseNumber;
    private FcrCaseMembers passMemberId;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public Case1 ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
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
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
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

    private Common selCount;
    private Case1 zeroFill;
    private Case1 pass;
    private Common supervisor;
    private Common accessOnThisCase;
  }
#endregion
}
