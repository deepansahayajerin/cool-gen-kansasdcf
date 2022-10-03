// Program: OE_URAH_HOUSEHOLD_URA, ID: 372632165, model: 746.
// Short name: SWEURAHP
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
/// A program: OE_URAH_HOUSEHOLD_URA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUrahHouseholdUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAH_HOUSEHOLD_URA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUrahHouseholdUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUrahHouseholdUra.
  /// </summary>
  public OeUrahHouseholdUra(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // Fangman      06-03-00  Deleted all code & views & copied code from URAH 
    // then changed logic to be driven by person instead of household.
    // K.Doshi      10-02-02  Fix screen help Id.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** Move Imports to Exports. ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Search.AeCaseNo))
    {
      // ****  Pad AE case number with leading zeros ****
      local.TextWorkArea.Text10 = import.Search.AeCaseNo;
      UseEabPadLeftWithZeros();
      export.Search.AeCaseNo = Substring(local.TextWorkArea.Text10, 3, 8);
    }

    export.SearchFrom.TextMonthYear = import.SearchFrom.TextMonthYear;
    export.SearchTo.TextMonthYear = import.SearchTo.TextMonthYear;

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.Search.AeCaseNo) && IsEmpty
        (export.SearchFrom.TextMonthYear) && IsEmpty
        (export.SearchTo.TextMonthYear))
      {
        ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

        return;
      }
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Detail.Index = 0;
      export.Detail.Clear();

      for(import.Detail.Index = 0; import.Detail.Index < import.Detail.Count; ++
        import.Detail.Index)
      {
        if (export.Detail.IsFull)
        {
          break;
        }

        export.Detail.Update.DtlCommon.SelectChar =
          import.Detail.Item.DtlCommon.SelectChar;
        export.Detail.Update.DtlDateWorkAttributes.TextMonthYear =
          import.Detail.Item.DtlDateWorkAttributes.TextMonthYear;
        export.Detail.Update.DtlGrpLabel1.Text8 =
          import.Detail.Item.DtlGrpLabel1.Text8;
        export.Detail.Update.DtlGrpLabel2.Text8 =
          import.Detail.Item.DtlGrpLabel2.Text8;
        export.Detail.Update.DtlImHouseholdMbrMnthlySum.Assign(
          import.Detail.Item.DtlImHouseholdMbrMnthlySum);
        export.Detail.Update.DtlGrpAfFcColl.CollectionAmountApplied =
          import.Detail.Item.DtlGrpAfFcColl.CollectionAmountApplied;
        export.Detail.Update.DtlGrpMedicalColl.CollectionAmountApplied =
          import.Detail.Item.DtlGrpMedicalColl.CollectionAmountApplied;
        export.Detail.Update.DtlGrpSsColl.CollectionAmountApplied =
          import.Detail.Item.DtlGrpSsColl.CollectionAmountApplied;
        export.Detail.Update.DtlGrpAfFc.AdjustmentAmount =
          import.Detail.Item.DtlGrpAfFc.AdjustmentAmount;
        export.Detail.Update.DtlGrpMedical.AdjustmentAmount =
          import.Detail.Item.DtlGrpMedical.AdjustmentAmount;
        export.Detail.Next();
      }

      export.TotGrp.Assign(import.TotGrp);
      export.TotGrpAfFcColl.CollectionAmountApplied =
        import.TotGrpAfFcColl.CollectionAmountApplied;
      export.TotGrpMedicalColl.CollectionAmountApplied =
        import.TotGrpMedicalColl.CollectionAmountApplied;
      export.TotGrpSsColl.CollectionAmountApplied =
        import.TotGrpSsColl.CollectionAmountApplied;
      export.TotGrpAfFc.AdjustmentAmount = import.TotGrpAfFc.AdjustmentAmount;
      export.TotGrpMedical.AdjustmentAmount =
        import.TotGrpMedical.AdjustmentAmount;
      export.TotGrpAfFcOwed.TotalCurrency = import.TotGrpAfFcOwed.TotalCurrency;
      export.TotGrpMedicalOwed.TotalCurrency =
        import.TotGrpMedicalOwed.TotalCurrency;
    }

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

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "IMHH") || Equal(global.Command, "URAC") || Equal
      (global.Command, "CURA") || Equal(global.Command, "URAL") || Equal
      (global.Command, "UCOL") || Equal(global.Command, "UHMM"))
    {
      local.NoOfEntriesSelected.Count = 0;

      for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
        export.Detail.Index)
      {
        switch(AsChar(export.Detail.Item.DtlCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.NoOfEntriesSelected.Count;

            if (local.NoOfEntriesSelected.Count > 1)
            {
              var field1 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field1.Error = true;

              ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

              return;
            }

            export.DialogFlowDateWorkAttributes.TextMonthYear =
              export.Detail.Item.DtlDateWorkAttributes.TextMonthYear;
            export.DialogFlowDateWorkArea.Month =
              (int)StringToNumber(Substring(
                export.DialogFlowDateWorkAttributes.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 1, 2));
            export.DialogFlowDateWorkArea.Year =
              (int)StringToNumber(Substring(
                export.DialogFlowDateWorkAttributes.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 3, 4));

            break;
          default:
            var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

            return;
        }
      }

      export.DialogFlowImHousehold.AeCaseNo = export.Search.AeCaseNo;

      switch(TrimEnd(global.Command))
      {
        case "IMHH":
          ExitState = "ECO_LNK_TO_IMHH";

          break;
        case "URAC":
          ExitState = "ECO_LNK_TO_URAC";

          break;
        case "CURA":
          ExitState = "ECO_LNK_TO_CURA";

          break;
        case "URAL":
          ExitState = "ECO_LNK_TO_URAL";

          break;
        case "UCOL":
          ExitState = "ECO_LNK_TO_UCOL";

          break;
        case "UHMM":
          ExitState = "ECO_LNK_TO_UHMM";

          break;
        default:
          break;
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.Search.AeCaseNo))
        {
          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }
        else if (!ReadImHousehold())
        {
          var field = GetField(export.Search, "aeCaseNo");

          field.Error = true;

          ExitState = "IM_HOUSEHOLD_NF";

          return;
        }

        if (IsEmpty(export.SearchTo.TextMonthYear))
        {
          export.SearchTo.TextMonthYear =
            NumberToString(Now().Date.Month, 14, 2) + NumberToString
            (Now().Date.Year, 12, 4);
        }

        UseCabEditMonthYear2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.SearchTo, "textMonthYear");

          field.Error = true;

          return;
        }

        local.ToImHouseholdMbrMnthlySum.Month = export.SearchTo.NumericalMonth;
        local.ToImHouseholdMbrMnthlySum.Year = export.SearchTo.NumericalYear;

        if (IsEmpty(export.SearchFrom.TextMonthYear))
        {
          export.SearchFrom.TextMonthYear =
            Substring(export.SearchTo.TextMonthYear,
            DateWorkAttributes.TextMonthYear_MaxLength, 1, 2) + NumberToString
            (export.SearchTo.NumericalYear - 5, 12, 4);
        }

        UseCabEditMonthYear1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.SearchFrom, "textMonthYear");

          field.Error = true;

          return;
        }

        local.FromImHouseholdMbrMnthlySum.Month =
          export.SearchFrom.NumericalMonth;
        local.FromImHouseholdMbrMnthlySum.Year =
          export.SearchFrom.NumericalYear;

        if (Lt(export.SearchTo.TextYearMonth, export.SearchFrom.TextYearMonth))
        {
          ExitState = "OE0000_FROM_DATE_SHOULD_BE_LESS";

          return;
        }

        UseOeUrahGetData();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.Detail.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else if (export.Detail.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDateWorkAttributes(DateWorkAttributes source,
    DateWorkAttributes target)
  {
    target.TextMonthYear = source.TextMonthYear;
    target.TextYearMonth = source.TextYearMonth;
    target.NumericalMonth = source.NumericalMonth;
    target.NumericalYear = source.NumericalYear;
  }

  private static void MoveDetail(OeUrahGetData.Export.DetailGroup source,
    Export.DetailGroup target)
  {
    target.DtlCommon.SelectChar = source.DtlCommon.SelectChar;
    target.DtlDateWorkAttributes.TextMonthYear =
      source.DtlDateWorkAttributes.TextMonthYear;
    target.DtlGrpLabel1.Text8 = source.DtlGrpLabel1.Text8;
    target.DtlGrpLabel2.Text8 = source.DtlGrpLabel2.Text8;
    target.DtlImHouseholdMbrMnthlySum.Assign(source.DtlImHouseholdMbrMnthlySum);
    target.DtlGrpAfFcColl.CollectionAmountApplied =
      source.DtlGrpAfFcColl.CollectionAmountApplied;
    target.DtlGrpMedicalColl.CollectionAmountApplied =
      source.DtlGrpMedicalColl.CollectionAmountApplied;
    target.DtlGrpSsColl.CollectionAmountApplied =
      source.DtlGrpSsColl.CollectionAmountApplied;
    target.DtlGrpAfFc.AdjustmentAmount = source.DtlGrpAfFc.AdjustmentAmount;
    target.DtlGrpMedical.AdjustmentAmount =
      source.DtlGrpMedical.AdjustmentAmount;
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private void UseCabEditMonthYear1()
  {
    var useImport = new CabEditMonthYear.Import();
    var useExport = new CabEditMonthYear.Export();

    MoveDateWorkAttributes(export.SearchFrom, useImport.DateWorkAttributes);

    Call(CabEditMonthYear.Execute, useImport, useExport);

    export.SearchFrom.Assign(useExport.DateWorkAttributes);
  }

  private void UseCabEditMonthYear2()
  {
    var useImport = new CabEditMonthYear.Import();
    var useExport = new CabEditMonthYear.Export();

    MoveDateWorkAttributes(export.SearchTo, useImport.DateWorkAttributes);

    Call(CabEditMonthYear.Execute, useImport, useExport);

    export.SearchTo.Assign(useExport.DateWorkAttributes);
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

  private void UseOeUrahGetData()
  {
    var useImport = new OeUrahGetData.Import();
    var useExport = new OeUrahGetData.Export();

    MoveImHouseholdMbrMnthlySum(local.FromImHouseholdMbrMnthlySum,
      useImport.From);
    MoveImHouseholdMbrMnthlySum(local.ToImHouseholdMbrMnthlySum, useImport.To);
    useImport.ImHousehold.AeCaseNo = export.Search.AeCaseNo;

    Call(OeUrahGetData.Execute, useImport, useExport);

    useExport.Detail.CopyTo(export.Detail, MoveDetail);
    export.TotGrp.Assign(useExport.TotGrp);
    export.TotGrpAfFcColl.CollectionAmountApplied =
      useExport.TotGrpAfFcColl.CollectionAmountApplied;
    export.TotGrpMedicalColl.CollectionAmountApplied =
      useExport.TotGrpMedicalColl.CollectionAmountApplied;
    export.TotGrpSsColl.CollectionAmountApplied =
      useExport.TotGrpSsColl.CollectionAmountApplied;
    export.TotGrpAfFc.AdjustmentAmount = useExport.TotGrpAfFc.AdjustmentAmount;
    export.TotGrpMedical.AdjustmentAmount =
      useExport.TotGrpMedical.AdjustmentAmount;
    export.TotGrpAfFcOwed.TotalCurrency =
      useExport.TotGrpAfFcOwed.TotalCurrency;
    export.TotGrpMedicalOwed.TotalCurrency =
      useExport.TotGrpMedicalOwed.TotalCurrency;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

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

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", export.Search.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
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
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlDateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dtlDateWorkAttributes")]
      public DateWorkAttributes DtlDateWorkAttributes
      {
        get => dtlDateWorkAttributes ??= new();
        set => dtlDateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DtlGrpLabel1.
      /// </summary>
      [JsonPropertyName("dtlGrpLabel1")]
      public TextWorkArea DtlGrpLabel1
      {
        get => dtlGrpLabel1 ??= new();
        set => dtlGrpLabel1 = value;
      }

      /// <summary>
      /// A value of DtlGrpLabel2.
      /// </summary>
      [JsonPropertyName("dtlGrpLabel2")]
      public TextWorkArea DtlGrpLabel2
      {
        get => dtlGrpLabel2 ??= new();
        set => dtlGrpLabel2 = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlGrpAfFcColl.
      /// </summary>
      [JsonPropertyName("dtlGrpAfFcColl")]
      public UraCollectionApplication DtlGrpAfFcColl
      {
        get => dtlGrpAfFcColl ??= new();
        set => dtlGrpAfFcColl = value;
      }

      /// <summary>
      /// A value of DtlGrpMedicalColl.
      /// </summary>
      [JsonPropertyName("dtlGrpMedicalColl")]
      public UraCollectionApplication DtlGrpMedicalColl
      {
        get => dtlGrpMedicalColl ??= new();
        set => dtlGrpMedicalColl = value;
      }

      /// <summary>
      /// A value of DtlGrpSsColl.
      /// </summary>
      [JsonPropertyName("dtlGrpSsColl")]
      public UraCollectionApplication DtlGrpSsColl
      {
        get => dtlGrpSsColl ??= new();
        set => dtlGrpSsColl = value;
      }

      /// <summary>
      /// A value of DtlGrpAfFc.
      /// </summary>
      [JsonPropertyName("dtlGrpAfFc")]
      public ImHouseholdMbrMnthlyAdj DtlGrpAfFc
      {
        get => dtlGrpAfFc ??= new();
        set => dtlGrpAfFc = value;
      }

      /// <summary>
      /// A value of DtlGrpMedical.
      /// </summary>
      [JsonPropertyName("dtlGrpMedical")]
      public ImHouseholdMbrMnthlyAdj DtlGrpMedical
      {
        get => dtlGrpMedical ??= new();
        set => dtlGrpMedical = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common dtlCommon;
      private DateWorkAttributes dtlDateWorkAttributes;
      private TextWorkArea dtlGrpLabel1;
      private TextWorkArea dtlGrpLabel2;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrpAfFcColl;
      private UraCollectionApplication dtlGrpMedicalColl;
      private UraCollectionApplication dtlGrpSsColl;
      private ImHouseholdMbrMnthlyAdj dtlGrpAfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrpMedical;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ImHousehold Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkAttributes SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkAttributes SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// A value of TotGrp.
    /// </summary>
    [JsonPropertyName("totGrp")]
    public ImHouseholdMbrMnthlySum TotGrp
    {
      get => totGrp ??= new();
      set => totGrp = value;
    }

    /// <summary>
    /// A value of TotGrpAfFcColl.
    /// </summary>
    [JsonPropertyName("totGrpAfFcColl")]
    public UraCollectionApplication TotGrpAfFcColl
    {
      get => totGrpAfFcColl ??= new();
      set => totGrpAfFcColl = value;
    }

    /// <summary>
    /// A value of TotGrpMedicalColl.
    /// </summary>
    [JsonPropertyName("totGrpMedicalColl")]
    public UraCollectionApplication TotGrpMedicalColl
    {
      get => totGrpMedicalColl ??= new();
      set => totGrpMedicalColl = value;
    }

    /// <summary>
    /// A value of TotGrpSsColl.
    /// </summary>
    [JsonPropertyName("totGrpSsColl")]
    public UraCollectionApplication TotGrpSsColl
    {
      get => totGrpSsColl ??= new();
      set => totGrpSsColl = value;
    }

    /// <summary>
    /// A value of TotGrpAfFc.
    /// </summary>
    [JsonPropertyName("totGrpAfFc")]
    public ImHouseholdMbrMnthlyAdj TotGrpAfFc
    {
      get => totGrpAfFc ??= new();
      set => totGrpAfFc = value;
    }

    /// <summary>
    /// A value of TotGrpMedical.
    /// </summary>
    [JsonPropertyName("totGrpMedical")]
    public ImHouseholdMbrMnthlyAdj TotGrpMedical
    {
      get => totGrpMedical ??= new();
      set => totGrpMedical = value;
    }

    /// <summary>
    /// A value of TotGrpAfFcOwed.
    /// </summary>
    [JsonPropertyName("totGrpAfFcOwed")]
    public Common TotGrpAfFcOwed
    {
      get => totGrpAfFcOwed ??= new();
      set => totGrpAfFcOwed = value;
    }

    /// <summary>
    /// A value of TotGrpMedicalOwed.
    /// </summary>
    [JsonPropertyName("totGrpMedicalOwed")]
    public Common TotGrpMedicalOwed
    {
      get => totGrpMedicalOwed ??= new();
      set => totGrpMedicalOwed = value;
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
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public ImHousehold Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of ZzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes")]
    public DateWorkAttributes ZzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of ZzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary")]
    public MonthlyObligeeSummary ZzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi")]
    public Case1 Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi = value;
    }

    private ImHousehold search;
    private DateWorkAttributes searchFrom;
    private DateWorkAttributes searchTo;
    private Array<DetailGroup> detail;
    private ImHouseholdMbrMnthlySum totGrp;
    private UraCollectionApplication totGrpAfFcColl;
    private UraCollectionApplication totGrpMedicalColl;
    private UraCollectionApplication totGrpSsColl;
    private ImHouseholdMbrMnthlyAdj totGrpAfFc;
    private ImHouseholdMbrMnthlyAdj totGrpMedical;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
    private Standard standard;
    private ImHousehold zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes;
    private MonthlyObligeeSummary zzzzzzzzzzzzzzzzzzzzzzzMonthlyObligeeSummary;
    private Case1 zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzi;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlDateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dtlDateWorkAttributes")]
      public DateWorkAttributes DtlDateWorkAttributes
      {
        get => dtlDateWorkAttributes ??= new();
        set => dtlDateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DtlGrpLabel1.
      /// </summary>
      [JsonPropertyName("dtlGrpLabel1")]
      public TextWorkArea DtlGrpLabel1
      {
        get => dtlGrpLabel1 ??= new();
        set => dtlGrpLabel1 = value;
      }

      /// <summary>
      /// A value of DtlGrpLabel2.
      /// </summary>
      [JsonPropertyName("dtlGrpLabel2")]
      public TextWorkArea DtlGrpLabel2
      {
        get => dtlGrpLabel2 ??= new();
        set => dtlGrpLabel2 = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlGrpAfFcColl.
      /// </summary>
      [JsonPropertyName("dtlGrpAfFcColl")]
      public UraCollectionApplication DtlGrpAfFcColl
      {
        get => dtlGrpAfFcColl ??= new();
        set => dtlGrpAfFcColl = value;
      }

      /// <summary>
      /// A value of DtlGrpMedicalColl.
      /// </summary>
      [JsonPropertyName("dtlGrpMedicalColl")]
      public UraCollectionApplication DtlGrpMedicalColl
      {
        get => dtlGrpMedicalColl ??= new();
        set => dtlGrpMedicalColl = value;
      }

      /// <summary>
      /// A value of DtlGrpSsColl.
      /// </summary>
      [JsonPropertyName("dtlGrpSsColl")]
      public UraCollectionApplication DtlGrpSsColl
      {
        get => dtlGrpSsColl ??= new();
        set => dtlGrpSsColl = value;
      }

      /// <summary>
      /// A value of DtlGrpAfFc.
      /// </summary>
      [JsonPropertyName("dtlGrpAfFc")]
      public ImHouseholdMbrMnthlyAdj DtlGrpAfFc
      {
        get => dtlGrpAfFc ??= new();
        set => dtlGrpAfFc = value;
      }

      /// <summary>
      /// A value of DtlGrpMedical.
      /// </summary>
      [JsonPropertyName("dtlGrpMedical")]
      public ImHouseholdMbrMnthlyAdj DtlGrpMedical
      {
        get => dtlGrpMedical ??= new();
        set => dtlGrpMedical = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common dtlCommon;
      private DateWorkAttributes dtlDateWorkAttributes;
      private TextWorkArea dtlGrpLabel1;
      private TextWorkArea dtlGrpLabel2;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrpAfFcColl;
      private UraCollectionApplication dtlGrpMedicalColl;
      private UraCollectionApplication dtlGrpSsColl;
      private ImHouseholdMbrMnthlyAdj dtlGrpAfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrpMedical;
    }

    /// <summary>
    /// A value of DialogFlowImHousehold.
    /// </summary>
    [JsonPropertyName("dialogFlowImHousehold")]
    public ImHousehold DialogFlowImHousehold
    {
      get => dialogFlowImHousehold ??= new();
      set => dialogFlowImHousehold = value;
    }

    /// <summary>
    /// A value of DialogFlowDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dialogFlowDateWorkAttributes")]
    public DateWorkAttributes DialogFlowDateWorkAttributes
    {
      get => dialogFlowDateWorkAttributes ??= new();
      set => dialogFlowDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of DialogFlowDateWorkArea.
    /// </summary>
    [JsonPropertyName("dialogFlowDateWorkArea")]
    public DateWorkArea DialogFlowDateWorkArea
    {
      get => dialogFlowDateWorkArea ??= new();
      set => dialogFlowDateWorkArea = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ImHousehold Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkAttributes SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkAttributes SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// A value of TotGrp.
    /// </summary>
    [JsonPropertyName("totGrp")]
    public ImHouseholdMbrMnthlySum TotGrp
    {
      get => totGrp ??= new();
      set => totGrp = value;
    }

    /// <summary>
    /// A value of TotGrpAfFcColl.
    /// </summary>
    [JsonPropertyName("totGrpAfFcColl")]
    public UraCollectionApplication TotGrpAfFcColl
    {
      get => totGrpAfFcColl ??= new();
      set => totGrpAfFcColl = value;
    }

    /// <summary>
    /// A value of TotGrpMedicalColl.
    /// </summary>
    [JsonPropertyName("totGrpMedicalColl")]
    public UraCollectionApplication TotGrpMedicalColl
    {
      get => totGrpMedicalColl ??= new();
      set => totGrpMedicalColl = value;
    }

    /// <summary>
    /// A value of TotGrpSsColl.
    /// </summary>
    [JsonPropertyName("totGrpSsColl")]
    public UraCollectionApplication TotGrpSsColl
    {
      get => totGrpSsColl ??= new();
      set => totGrpSsColl = value;
    }

    /// <summary>
    /// A value of TotGrpAfFc.
    /// </summary>
    [JsonPropertyName("totGrpAfFc")]
    public ImHouseholdMbrMnthlyAdj TotGrpAfFc
    {
      get => totGrpAfFc ??= new();
      set => totGrpAfFc = value;
    }

    /// <summary>
    /// A value of TotGrpMedical.
    /// </summary>
    [JsonPropertyName("totGrpMedical")]
    public ImHouseholdMbrMnthlyAdj TotGrpMedical
    {
      get => totGrpMedical ??= new();
      set => totGrpMedical = value;
    }

    /// <summary>
    /// A value of TotGrpAfFcOwed.
    /// </summary>
    [JsonPropertyName("totGrpAfFcOwed")]
    public Common TotGrpAfFcOwed
    {
      get => totGrpAfFcOwed ??= new();
      set => totGrpAfFcOwed = value;
    }

    /// <summary>
    /// A value of TotGrpMedicalOwed.
    /// </summary>
    [JsonPropertyName("totGrpMedicalOwed")]
    public Common TotGrpMedicalOwed
    {
      get => totGrpMedicalOwed ??= new();
      set => totGrpMedicalOwed = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzz")]
    public ImHousehold Zzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of Zzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzz")]
    public Case1 Zzzzzzzzzzzz
    {
      get => zzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzz = value;
    }

    private ImHousehold dialogFlowImHousehold;
    private DateWorkAttributes dialogFlowDateWorkAttributes;
    private DateWorkArea dialogFlowDateWorkArea;
    private ImHousehold search;
    private DateWorkAttributes searchFrom;
    private DateWorkAttributes searchTo;
    private Array<DetailGroup> detail;
    private ImHouseholdMbrMnthlySum totGrp;
    private UraCollectionApplication totGrpAfFcColl;
    private UraCollectionApplication totGrpMedicalColl;
    private UraCollectionApplication totGrpSsColl;
    private ImHouseholdMbrMnthlyAdj totGrpAfFc;
    private ImHouseholdMbrMnthlyAdj totGrpMedical;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
    private ImHousehold zzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private Standard standard;
    private Case1 zzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FromDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("fromDateWorkAttributes")]
    public DateWorkAttributes FromDateWorkAttributes
    {
      get => fromDateWorkAttributes ??= new();
      set => fromDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of ToDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("toDateWorkAttributes")]
    public DateWorkAttributes ToDateWorkAttributes
    {
      get => toDateWorkAttributes ??= new();
      set => toDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of FromImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("fromImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum FromImHouseholdMbrMnthlySum
    {
      get => fromImHouseholdMbrMnthlySum ??= new();
      set => fromImHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ToImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("toImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ToImHouseholdMbrMnthlySum
    {
      get => toImHouseholdMbrMnthlySum ??= new();
      set => toImHouseholdMbrMnthlySum = value;
    }

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
    /// A value of NoOfEntriesSelected.
    /// </summary>
    [JsonPropertyName("noOfEntriesSelected")]
    public Common NoOfEntriesSelected
    {
      get => noOfEntriesSelected ??= new();
      set => noOfEntriesSelected = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkArea Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    private DateWorkAttributes fromDateWorkAttributes;
    private DateWorkAttributes toDateWorkAttributes;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum fromImHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlySum toImHouseholdMbrMnthlySum;
    private TextWorkArea textWorkArea;
    private Common noOfEntriesSelected;
    private DateWorkArea zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
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

    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private UraCollectionApplication uraCollectionApplication;
    private CsePerson csePerson;
  }
#endregion
}
