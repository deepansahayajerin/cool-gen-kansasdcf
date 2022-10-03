// Program: OE_FCRV_FCR_CASE_VIEW, ID: 374573613, model: 746.
// Short name: SWEFCRVP
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
/// A program: OE_FCRV_FCR_CASE_VIEW.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFcrvFcrCaseView: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCRV_FCR_CASE_VIEW program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrvFcrCaseView(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrvFcrCaseView.
  /// </summary>
  public OeFcrvFcrCaseView(IContext context, Import import, Export export):
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
    // 08/25/2009	M Fan		CQ7190	       Initial Dev
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

    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -------------------------------------------------------
    // Move imports to exports
    // -------------------------------------------------------
    if (Lt("0000000000", import.Pass.Number))
    {
      export.FilterCaseId.CaseId = import.Pass.Number;
    }
    else
    {
      export.FilterCaseId.CaseId = import.FilterCaseId.CaseId;
    }

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

    export.HiddnFilterPrevCaseId.CaseId = import.HiddnFilterPrevCaseId.CaseId;
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveStandard(import.Scrolling, export.Scrolling);

    // Check if the user requested a next tran action
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

    // Check if the user is comming into this procedure on a next tran action.
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

    // Check if flowing from the menu
    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
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

        export.FcrMemberList.Update.SelChar.SelectChar =
          import.FcrMemberList.Item.SelCharx.SelectChar;
        export.FcrMemberList.Update.Comma.SelectChar =
          import.FcrMemberList.Item.Comma.SelectChar;
        MoveFcrCaseMaster(import.FcrMemberList.Item.FcrCase,
          export.FcrMemberList.Update.FcrCase);
        export.FcrMemberList.Update.FcrMember.Assign(
          import.FcrMemberList.Item.FcrMember);

        switch(TrimEnd(export.FcrMemberList.Item.FcrMember.ParticipantType ?? ""))
          
        {
          case "AP":
            export.PassAp.Number = export.FcrMemberList.Item.FcrMember.MemberId;

            break;
          case "AR":
            export.PassAr.Number = export.FcrMemberList.Item.FcrMember.MemberId;
            export.PassAr.FirstName =
              export.FcrMemberList.Item.FcrMember.FirstName ?? Spaces(12);
            export.PassAr.LastName =
              export.FcrMemberList.Item.FcrMember.LastName ?? Spaces(17);
            export.PassAr.MiddleInitial =
              export.FcrMemberList.Item.FcrMember.MiddleName ?? Spaces(1);

            break;
          default:
            break;
        }

        export.FcrMemberList.Next();
      }
    }

    // ---------------------------------------------------------------
    //              *** Edit Screen ***
    // ---------------------------------------------------------------
    if (IsEmpty(export.FilterCaseId.CaseId) && Equal(global.Command, "DISPLAY"))
    {
      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      return;
    }

    if ((Equal(global.Command, "PREV") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "CADS") || Equal(global.Command, "MBLS") || Equal
      (global.Command, "MBDT")) && !
      Equal(export.HiddnFilterPrevCaseId.CaseId, export.FilterCaseId.CaseId))
    {
      ExitState = "OE_0206_MUST_DISPLAY_FIRST";

      return;
    }

    // Validate security
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

    // Check selection for command MBDT
    if (Equal(global.Command, "MBDT"))
    {
      for(export.FcrMemberList.Index = 0; export.FcrMemberList.Index < export
        .FcrMemberList.Count; ++export.FcrMemberList.Index)
      {
        if (AsChar(export.FcrMemberList.Item.SelChar.SelectChar) == 'S')
        {
          ++local.SelCount.Count;
          export.PassMemberId.MemberId =
            export.FcrMemberList.Item.FcrMember.MemberId;

          if (Equal(export.FcrMemberList.Item.FcrMember.ParticipantType, "CH"))
          {
            export.PassCh.Number = export.FcrMemberList.Item.FcrMember.MemberId;
            export.PassCh.FirstName =
              export.FcrMemberList.Item.FcrMember.FirstName ?? Spaces(12);
            export.PassCh.LastName =
              export.FcrMemberList.Item.FcrMember.LastName ?? Spaces(17);
            export.PassCh.MiddleInitial =
              export.FcrMemberList.Item.FcrMember.MiddleName ?? Spaces(1);
          }
        }
        else if (!IsEmpty(export.FcrMemberList.Item.SelChar.SelectChar))
        {
          var field = GetField(export.FcrMemberList.Item.SelChar, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

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
      case "CADS":
        export.PassCaseNumber.Number = export.FilterCaseId.CaseId;
        ExitState = "ECO_LNK_TO_LST_CASE_DETAILS";

        break;
      case "MBLS":
        ExitState = "ECO_LINK_TO_MBLS";

        break;
      case "MBDT":
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
    target.SelChar.SelectChar = source.SelChar.SelectChar;
    target.FcrMember.Assign(source.FcrMember);
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
      /// A value of SelCharx.
      /// </summary>
      [JsonPropertyName("selCharx")]
      public Common SelCharx
      {
        get => selCharx ??= new();
        set => selCharx = value;
      }

      /// <summary>
      /// A value of FcrMember.
      /// </summary>
      [JsonPropertyName("fcrMember")]
      public FcrCaseMembers FcrMember
      {
        get => fcrMember ??= new();
        set => fcrMember = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selCharx;
      private FcrCaseMembers fcrMember;
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
    /// A value of HiddnFilterPrevCaseId.
    /// </summary>
    [JsonPropertyName("hiddnFilterPrevCaseId")]
    public FcrCaseMaster HiddnFilterPrevCaseId
    {
      get => hiddnFilterPrevCaseId ??= new();
      set => hiddnFilterPrevCaseId = value;
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

    private Case1 pass;
    private FcrCaseMaster hiddnFilterPrevCaseId;
    private FcrCaseMaster filterCaseId;
    private Array<FcrMemberListGroup> fcrMemberList;
    private NextTranInfo hidden;
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
      /// A value of SelChar.
      /// </summary>
      [JsonPropertyName("selChar")]
      public Common SelChar
      {
        get => selChar ??= new();
        set => selChar = value;
      }

      /// <summary>
      /// A value of FcrMember.
      /// </summary>
      [JsonPropertyName("fcrMember")]
      public FcrCaseMembers FcrMember
      {
        get => fcrMember ??= new();
        set => fcrMember = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 65;

      private FcrCaseMaster fcrCase;
      private Common comma;
      private Common selChar;
      private FcrCaseMembers fcrMember;
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
    /// A value of FilterCaseId.
    /// </summary>
    [JsonPropertyName("filterCaseId")]
    public FcrCaseMaster FilterCaseId
    {
      get => filterCaseId ??= new();
      set => filterCaseId = value;
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
    /// A value of PassCh.
    /// </summary>
    [JsonPropertyName("passCh")]
    public CsePersonsWorkSet PassCh
    {
      get => passCh ??= new();
      set => passCh = value;
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
    /// A value of PassAr.
    /// </summary>
    [JsonPropertyName("passAr")]
    public CsePersonsWorkSet PassAr
    {
      get => passAr ??= new();
      set => passAr = value;
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

    private FcrCaseMaster hiddnFilterPrevCaseId;
    private FcrCaseMaster filterCaseId;
    private Array<FcrMemberListGroup> fcrMemberList;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard scrolling;
    private CsePersonsWorkSet passCh;
    private CsePersonsWorkSet passAp;
    private CsePersonsWorkSet passAr;
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
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
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
    private Case1 zeroFill;
    private Common selCount;
    private Case1 pass;
    private Common accessOnThisCase;
  }
#endregion
}
