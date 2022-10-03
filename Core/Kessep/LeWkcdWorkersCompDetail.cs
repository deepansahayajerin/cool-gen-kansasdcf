// Program: LE_WKCD_WORKERS_COMP_DETAIL, ID: 1625337202, model: 746.
// Short name: SWEWKCDP
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
/// A program: LE_WKCD_WORKERS_COMP_DETAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeWkcdWorkersCompDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_WKCD_WORKERS_COMP_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeWkcdWorkersCompDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeWkcdWorkersCompDetail.
  /// </summary>
  public LeWkcdWorkersCompDetail(IContext context, Import import, Export export):
    
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
    // 12/01/16  GVandy	CQ51923		Initial Code.  Created from a copy of EIWH.
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
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.HiddenWorkersCompClaim.Identifier =
      import.HiddenWorkersCompClaim.Identifier;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;

    if (!IsEmpty(import.CsePerson.Number))
    {
      export.CsePersonsWorkSet.Number = import.CsePerson.Number;
    }

    // -- If any of the search fields are spaces then space out the associated 
    // descriptions.
    if (IsEmpty(export.CsePersonsWorkSet.Number))
    {
      export.CsePersonsWorkSet.FormattedName = "";
    }

    // -- Zero fill any necessary search fields.
    if (!IsEmpty(export.CsePersonsWorkSet.Number))
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

      export.Export1.Update.G.Text80 = import.Import1.Item.G.Text80;
    }

    import.Import1.CheckIndex();

    switch(TrimEnd(global.Command))
    {
      case "RETNAME":
        export.PromptPerson.SelectChar = "";

        if (IsEmpty(import.FromName.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_PROMPT_SELECTION";

          return;
        }
        else
        {
          export.CsePersonsWorkSet.Number = import.FromName.Number;
        }

        global.Command = "DISPLAY";

        break;
      case "FROMMENU":
        // @@@ If person number passed from PLOM then change command to do a 
        // display.
        if (!IsEmpty(import.CsePerson.Number))
        {
          global.Command = "DISPLAY";
        }

        break;
      default:
        break;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
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
      export.CsePersonsWorkSet.Number = "";
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

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(local.PromptCount.Count)
      {
        case 0:
          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          break;
        case 1:
          break;
        default:
          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            var field1 = GetField(export.PromptPerson, "selectChar");

            field1.Error = true;
          }
          else
          {
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

      return;
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

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "DISPLAY":
        // --If cse person number changed then set workers comp claim number to 
        // 0.
        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenCsePersonsWorkSet.Number) && !
          IsEmpty(export.HiddenCsePersonsWorkSet.Number))
        {
          export.HiddenWorkersCompClaim.Identifier = 0;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        // -- Call display cab.  If multiple claims are found then set an exit 
        // state
        //    to link to WKCL will be returned.
        UseLeWkcdDisplayWcDetail();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        if (export.Export1.Index == -1)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "FROMMENU":
        // -- No processing required.
        break;
      case "WKCL":
        ExitState = "ECO_LNK_TO_WKCL";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExport1(LeWkcdDisplayWcDetail.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.G.Text80 = source.G.Text80;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseLeWkcdDisplayWcDetail()
  {
    var useImport = new LeWkcdDisplayWcDetail.Import();
    var useExport = new LeWkcdDisplayWcDetail.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.WorkersCompClaim.Identifier =
      import.HiddenWorkersCompClaim.Identifier;

    Call(LeWkcdDisplayWcDetail.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.HiddenWorkersCompClaim.Identifier =
      useExport.WorkersCompClaim.Identifier;
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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private WorkArea g;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenWorkersCompClaim.
    /// </summary>
    [JsonPropertyName("hiddenWorkersCompClaim")]
    public WorkersCompClaim HiddenWorkersCompClaim
    {
      get => hiddenWorkersCompClaim ??= new();
      set => hiddenWorkersCompClaim = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of FromName.
    /// </summary>
    [JsonPropertyName("fromName")]
    public CsePersonsWorkSet FromName
    {
      get => fromName ??= new();
      set => fromName = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptPerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private WorkersCompClaim hiddenWorkersCompClaim;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet fromName;
    private CsePerson csePerson;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private WorkArea g;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenWorkersCompClaim.
    /// </summary>
    [JsonPropertyName("hiddenWorkersCompClaim")]
    public WorkersCompClaim HiddenWorkersCompClaim
    {
      get => hiddenWorkersCompClaim ??= new();
      set => hiddenWorkersCompClaim = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptPerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private WorkersCompClaim hiddenWorkersCompClaim;
    private Array<ExportGroup> export1;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Tbd.
    /// </summary>
    [JsonPropertyName("tbd")]
    public CsePerson Tbd
    {
      get => tbd ??= new();
      set => tbd = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private NextTranInfo nullNextTranInfo;
    private DateWorkArea nullDateWorkArea;
    private CsePerson tbd;
    private Common common;
    private AbendData abendData;
    private Common promptCount;
  }
#endregion
}
