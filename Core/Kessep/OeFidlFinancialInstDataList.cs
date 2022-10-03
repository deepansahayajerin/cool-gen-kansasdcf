// Program: OE_FIDL_FINANCIAL_INST_DATA_LIST, ID: 374392818, model: 746.
// Short name: SWEFIDLP
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
/// A program: OE_FIDL_FINANCIAL_INST_DATA_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFidlFinancialInstDataList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FIDL_FINANCIAL_INST_DATA_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFidlFinancialInstDataList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFidlFinancialInstDataList.
  /// </summary>
  public OeFidlFinancialInstDataList(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------
    // Date             Developer Name         Description
    // 05/01/2000       Sherri Newman-SRS      Source
    // 07/18/2000       George Vandy           PR 98226 - Modifications for FIDM
    // identifier
    //                                         
    // change.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (Equal(global.Command, "SPACES"))
    {
      global.Command = "DISPLAY";
    }

    export.FinancialInstitutionDataMatch.CsePersonNumber =
      import.FinancialInstitutionDataMatch.CsePersonNumber;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);

    if (!IsEmpty(import.CsePerson.Number))
    {
      export.FinancialInstitutionDataMatch.CsePersonNumber =
        import.CsePerson.Number;
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (IsEmpty(import.PassCsePersonsWorkSet.Number))
      {
      }
      else
      {
        export.FinancialInstitutionDataMatch.CsePersonNumber =
          import.PassCsePersonsWorkSet.Number;
        export.CsePersonsWorkSet.FormattedName =
          import.PassCsePersonsWorkSet.FormattedName;
        global.Command = "DISPLAY";
      }
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.FinancialInstitutionDataMatch.Assign(
        import.Group.Item.FinancialInstitutionDataMatch);
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.AccountType.Text3 =
        import.Group.Item.AccountType.Text3;
      export.Group.Update.MatchSource.Text6 =
        import.Group.Item.MatchSource.Text6;
      export.Group.Update.RunDate.Text6 = import.Group.Item.RunDate.Text6;
      export.Group.Next();
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    if (Equal(global.Command, "RETNAME"))
    {
      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber =
        export.FinancialInstitutionDataMatch.CsePersonNumber;
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

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      if (!IsEmpty(import.PassFinancialInstitutionDataMatch.CsePersonNumber))
      {
        export.FinancialInstitutionDataMatch.CsePersonNumber =
          import.PassFinancialInstitutionDataMatch.CsePersonNumber;
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "FIDM"))
    {
      local.CsePerson.Number = import.CsePersonsWorkSet.Number;
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.FinancialInstitutionDataMatch.CsePersonNumber))
        {
          var field =
            GetField(export.FinancialInstitutionDataMatch, "csePersonNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          local.TextWorkArea.Text10 =
            export.FinancialInstitutionDataMatch.CsePersonNumber;
          UseEabPadLeftWithZeros();
          export.FinancialInstitutionDataMatch.CsePersonNumber =
            local.TextWorkArea.Text10;
          UseOeDisplayFidlList();

          if (!IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
            var field =
              GetField(export.FinancialInstitutionDataMatch, "csePersonNumber");
              

            field.Error = true;

            return;
          }

          if (export.Group.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

            return;
          }

          if (export.Group.IsFull)
          {
            ExitState = "ACO_NI0000_LST_RETURNED_FULL";

            return;
          }

          if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL"))
          {
          }
        }

        break;
      case "FIDM":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              ++local.Counter.Count;

              if (local.Counter.Count > 1)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              if (Equal(global.Command, "RETLINK"))
              {
                export.Group.Update.Common.SelectChar = "*";
              }
            }
            else if (AsChar(export.Group.Item.Common.SelectChar) == '*')
            {
            }
            else if (AsChar(export.Group.Item.Common.SelectChar) == '_')
            {
            }
            else
            {
              ++local.Counter.Count;

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        if (local.Counter.Count == 0)
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            continue;
          }
          else
          {
            MoveFinancialInstitutionDataMatch(export.Group.Item.
              FinancialInstitutionDataMatch,
              export.PassFinancialInstitutionDataMatch);
            export.PassFinancialInstitutionDataMatch.CsePersonNumber =
              export.FinancialInstitutionDataMatch.CsePersonNumber;
            export.Group.Update.Common.SelectChar = "*";
            ExitState = "ECO_LINK_TO_FIDM";

            return;
          }
        }

        break;
      case "RESL":
        if (IsEmpty(export.FinancialInstitutionDataMatch.CsePersonNumber))
        {
          var field =
            GetField(export.FinancialInstitutionDataMatch, "csePersonNumber");

          field.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";

          return;
        }

        export.PassFinancialInstitutionDataMatch.CsePersonNumber =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        export.PassCsePerson.Number =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        ExitState = "ECO_LNK_TO_RESL_RESOURCE_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "LIST":
        switch(AsChar(import.PromptName.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            break;
          case ' ':
            var field1 = GetField(export.PromptName, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          default:
            var field2 = GetField(export.PromptName, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "EXIT":
        export.Hidden.CsePersonNumber =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "HELP":
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

  private static void MoveFinancialInstitutionDataMatch(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.InstitutionTin = source.InstitutionTin;
    target.MatchedPayeeAccountNumber = source.MatchedPayeeAccountNumber;
    target.MatchRunDate = source.MatchRunDate;
    target.AccountBalance = source.AccountBalance;
    target.AccountType = source.AccountType;
  }

  private static void MoveGroup(OeDisplayFidlList.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.FinancialInstitutionDataMatch.Assign(
      source.FinancialInstitutionDataMatch);
    target.AccountType.Text3 = source.AccountType.Text3;
    target.RunDate.Text6 = source.RunDate.Text6;
    target.MatchSource.Text6 = source.MatchSource.Text6;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeDisplayFidlList()
  {
    var useImport = new OeDisplayFidlList.Import();
    var useExport = new OeDisplayFidlList.Export();

    useImport.FinancialInstitutionDataMatch.CsePersonNumber =
      export.FinancialInstitutionDataMatch.CsePersonNumber;

    Call(OeDisplayFidlList.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.CsePersonNumber = useExport.NextTranInfo.CsePersonNumber;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.CsePersonNumber = export.Hidden.CsePersonNumber;
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

    useImport.CsePerson.Number = local.CsePerson.Number;

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
      /// A value of FinancialInstitutionDataMatch.
      /// </summary>
      [JsonPropertyName("financialInstitutionDataMatch")]
      public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
      {
        get => financialInstitutionDataMatch ??= new();
        set => financialInstitutionDataMatch = value;
      }

      /// <summary>
      /// A value of AccountType.
      /// </summary>
      [JsonPropertyName("accountType")]
      public WorkArea AccountType
      {
        get => accountType ??= new();
        set => accountType = value;
      }

      /// <summary>
      /// A value of RunDate.
      /// </summary>
      [JsonPropertyName("runDate")]
      public WorkArea RunDate
      {
        get => runDate ??= new();
        set => runDate = value;
      }

      /// <summary>
      /// A value of MatchSource.
      /// </summary>
      [JsonPropertyName("matchSource")]
      public WorkArea MatchSource
      {
        get => matchSource ??= new();
        set => matchSource = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private FinancialInstitutionDataMatch financialInstitutionDataMatch;
      private WorkArea accountType;
      private WorkArea runDate;
      private WorkArea matchSource;
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
    /// A value of PassCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("passCsePersonsWorkSet")]
    public CsePersonsWorkSet PassCsePersonsWorkSet
    {
      get => passCsePersonsWorkSet ??= new();
      set => passCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PassFinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("passFinancialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch PassFinancialInstitutionDataMatch
    {
      get => passFinancialInstitutionDataMatch ??= new();
      set => passFinancialInstitutionDataMatch = value;
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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
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
    /// A value of PromptName.
    /// </summary>
    [JsonPropertyName("promptName")]
    public Standard PromptName
    {
      get => promptName ??= new();
      set => promptName = value;
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

    private CsePerson csePerson;
    private CsePersonsWorkSet passCsePersonsWorkSet;
    private FinancialInstitutionDataMatch passFinancialInstitutionDataMatch;
    private NextTranInfo hidden;
    private Standard standard;
    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard promptName;
    private Array<GroupGroup> group;
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
      /// A value of FinancialInstitutionDataMatch.
      /// </summary>
      [JsonPropertyName("financialInstitutionDataMatch")]
      public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
      {
        get => financialInstitutionDataMatch ??= new();
        set => financialInstitutionDataMatch = value;
      }

      /// <summary>
      /// A value of AccountType.
      /// </summary>
      [JsonPropertyName("accountType")]
      public WorkArea AccountType
      {
        get => accountType ??= new();
        set => accountType = value;
      }

      /// <summary>
      /// A value of RunDate.
      /// </summary>
      [JsonPropertyName("runDate")]
      public WorkArea RunDate
      {
        get => runDate ??= new();
        set => runDate = value;
      }

      /// <summary>
      /// A value of MatchSource.
      /// </summary>
      [JsonPropertyName("matchSource")]
      public WorkArea MatchSource
      {
        get => matchSource ??= new();
        set => matchSource = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private FinancialInstitutionDataMatch financialInstitutionDataMatch;
      private WorkArea accountType;
      private WorkArea runDate;
      private WorkArea matchSource;
    }

    /// <summary>
    /// A value of PassCsePerson.
    /// </summary>
    [JsonPropertyName("passCsePerson")]
    public CsePerson PassCsePerson
    {
      get => passCsePerson ??= new();
      set => passCsePerson = value;
    }

    /// <summary>
    /// A value of PassFinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("passFinancialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch PassFinancialInstitutionDataMatch
    {
      get => passFinancialInstitutionDataMatch ??= new();
      set => passFinancialInstitutionDataMatch = value;
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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
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
    /// A value of PromptName.
    /// </summary>
    [JsonPropertyName("promptName")]
    public Standard PromptName
    {
      get => promptName ??= new();
      set => promptName = value;
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

    private CsePerson passCsePerson;
    private FinancialInstitutionDataMatch passFinancialInstitutionDataMatch;
    private NextTranInfo hidden;
    private Standard standard;
    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard promptName;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    private TextWorkArea textWorkArea;
    private CsePerson csePerson;
    private Common counter;
  }
#endregion
}
