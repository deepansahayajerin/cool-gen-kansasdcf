// Program: FN_RDRL_LST_DFLT_RECAP_INSTRUC, ID: 371741077, model: 746.
// Short name: SWERDRLP
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
/// A program: FN_RDRL_LST_DFLT_RECAP_INSTRUC.
/// </para>
/// <para>
/// This procedure will list all of the obligation types that are subject to 
/// being recaptured by default. This means that we can recapture disbursements
/// to pay recovery obligations that are of certain types without having to
/// negotiate the recapture with the obligor. This list can include a history of
/// obligation types that were at one time subject to recapturing by default
/// but are not currently subject to recapture by default. This list will be
/// ordered by the recapture default effective date. Any one of the obligation
/// types listed can be selected and carried forward to another screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRdrlLstDfltRecapInstruc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RDRL_LST_DFLT_RECAP_INSTRUC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRdrlLstDfltRecapInstruc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRdrlLstDfltRecapInstruc.
  /// </summary>
  public FnRdrlLstDfltRecapInstruc(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------
    // List Default Recapture Rules
    // Date Created    Created by
    // 07/20/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 03/20/1996      R.B.Mohapatra - MTW
    //      Log : * Standards enforcement
    //            * Retrofit Security & Nexttran
    //            * Integration with neighbours
    // 12/03/96	R. Marchman	Add new security and next tran
    // 
    // 10/21/03        B. Lee          Changed code so command of display goes
    // thru security check.
    // ------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Hidden.Assign(import.Hidden);
    MoveCommon(import.ShowHistory, export.ShowHistory);
    MoveCommon(import.PrevShowHistory, export.PrevShowHistory);
    export.Last.EffectiveDate = import.Last.EffectiveDate;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailSelect.SelectChar =
        import.Import1.Item.DetailSelect.SelectChar;
      MoveObligationType(import.Import1.Item.DetailObligationType,
        export.Export1.Update.DetailObligationType);
      export.Export1.Update.DetailDefaultRule.Assign(
        import.Import1.Item.DetailDefaultRule);
      export.Export1.Next();
    }

    // ***  NEXT TRAN logic  ***
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
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

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ***  Validate Show_History field input  ***
    switch(AsChar(import.ShowHistory.SelectChar))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        export.ShowHistory.SelectChar = "N";

        break;
      default:
        var field = GetField(export.ShowHistory, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NE00_INVALID_SELECTION_YN";
        }

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      // *** Check any selections made ***
      if (!import.Import1.IsEmpty && AsChar(import.ShowHistory.SelectChar) == AsChar
        (import.PrevShowHistory.SelectChar))
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (AsChar(import.Import1.Item.DetailSelect.SelectChar) == 'S')
          {
            MoveObligationType(export.Export1.Item.DetailObligationType,
              export.HiddenSelectedObligationType);
            export.HiddenSelectedDefaultRule.Assign(
              export.Export1.Item.DetailDefaultRule);
            ++local.Work.Count;
          }
          else if (IsEmpty(import.Import1.Item.DetailSelect.SelectChar))
          {
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailSelect, "selectChar");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";
            }
          }

          export.Export1.Next();
        }

        if (IsExitState("ZD_ACO_NE0000_INVALID_SEL_CODE1"))
        {
          return;
        }
      }
    }

    // *** to validate action level security ***
    if (Equal(global.Command, "RDEF"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *** Populate export group view ***
        UseFnGrpReadDefaultRecaptRules();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCommon(import.ShowHistory, export.PrevShowHistory);

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_MORE_DATA_EXISTS";
          }
        }
        else
        {
          // Continue
        }

        break;
      case "RETURN":
        if (local.Work.Count > 1)
        {
          ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ZD_ACO_NE0000_INVALID_FORWARD_1";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RDEF":
        ExitState = "ECO_XFR_TO_MTN_DEF_RECAP_INSTR";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveExport1(FnGrpReadDefaultRecaptRules.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailSelect.SelectChar = source.DetailCommon.SelectChar;
    MoveObligationType(source.DetailObligationType, target.DetailObligationType);
      
    MoveRecaptureRule(source.DetailDefaultRule, target.DetailDefaultRule);
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

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveRecaptureRule(RecaptureRule source,
    RecaptureRule target)
  {
    target.NonAdcArrearsPercentage = source.NonAdcArrearsPercentage;
    target.NonAdcCurrentPercentage = source.NonAdcCurrentPercentage;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PassthruPercentage = source.PassthruPercentage;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseFnGrpReadDefaultRecaptRules()
  {
    var useImport = new FnGrpReadDefaultRecaptRules.Import();
    var useExport = new FnGrpReadDefaultRecaptRules.Export();

    useImport.Last.EffectiveDate = import.Last.EffectiveDate;
    useImport.ShowHistory.Flag = export.ShowHistory.Flag;

    Call(FnGrpReadDefaultRecaptRules.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailDefaultRule.
      /// </summary>
      [JsonPropertyName("detailDefaultRule")]
      public RecaptureRule DetailDefaultRule
      {
        get => detailDefaultRule ??= new();
        set => detailDefaultRule = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common detailSelect;
      private ObligationType detailObligationType;
      private RecaptureRule detailDefaultRule;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public RecaptureRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of PrevShowHistory.
    /// </summary>
    [JsonPropertyName("prevShowHistory")]
    public Common PrevShowHistory
    {
      get => prevShowHistory ??= new();
      set => prevShowHistory = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private RecaptureRule last;
    private Common showHistory;
    private Common prevShowHistory;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hidden;
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
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailDefaultRule.
      /// </summary>
      [JsonPropertyName("detailDefaultRule")]
      public RecaptureRule DetailDefaultRule
      {
        get => detailDefaultRule ??= new();
        set => detailDefaultRule = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common detailSelect;
      private ObligationType detailObligationType;
      private RecaptureRule detailDefaultRule;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public RecaptureRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of PrevShowHistory.
    /// </summary>
    [JsonPropertyName("prevShowHistory")]
    public Common PrevShowHistory
    {
      get => prevShowHistory ??= new();
      set => prevShowHistory = value;
    }

    /// <summary>
    /// A value of HiddenSelectedObligationType.
    /// </summary>
    [JsonPropertyName("hiddenSelectedObligationType")]
    public ObligationType HiddenSelectedObligationType
    {
      get => hiddenSelectedObligationType ??= new();
      set => hiddenSelectedObligationType = value;
    }

    /// <summary>
    /// A value of HiddenSelectedDefaultRule.
    /// </summary>
    [JsonPropertyName("hiddenSelectedDefaultRule")]
    public RecaptureRule HiddenSelectedDefaultRule
    {
      get => hiddenSelectedDefaultRule ??= new();
      set => hiddenSelectedDefaultRule = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private RecaptureRule last;
    private Common showHistory;
    private Common prevShowHistory;
    private ObligationType hiddenSelectedObligationType;
    private RecaptureRule hiddenSelectedDefaultRule;
    private Array<ExportGroup> export1;
    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkBeginSearch.
    /// </summary>
    [JsonPropertyName("workBeginSearch")]
    public RecaptureRule WorkBeginSearch
    {
      get => workBeginSearch ??= new();
      set => workBeginSearch = value;
    }

    /// <summary>
    /// A value of WorkEndSearch.
    /// </summary>
    [JsonPropertyName("workEndSearch")]
    public RecaptureRule WorkEndSearch
    {
      get => workEndSearch ??= new();
      set => workEndSearch = value;
    }

    /// <summary>
    /// A value of WorkImportSearch.
    /// </summary>
    [JsonPropertyName("workImportSearch")]
    public RecaptureRule WorkImportSearch
    {
      get => workImportSearch ??= new();
      set => workImportSearch = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    private RecaptureRule workBeginSearch;
    private RecaptureRule workEndSearch;
    private RecaptureRule workImportSearch;
    private Common work;
  }
#endregion
}
