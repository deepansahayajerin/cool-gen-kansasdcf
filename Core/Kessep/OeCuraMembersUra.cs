// Program: OE_CURA_MEMBERS_URA, ID: 372700067, model: 746.
// Short name: SWEURAMP
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
/// A program: OE_CURA_MEMBERS_URA.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Lists child's URA on descending year and month order starting from most 
/// recent month.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCuraMembersUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CURA_MEMBERS_URA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCuraMembersUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCuraMembersUra.
  /// </summary>
  public OeCuraMembersUra(IContext context, Import import, Export export):
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
    export.NextTran.NextTransaction = import.NextTran.NextTransaction;
    export.CsePerson.PromptField = import.CsePerson.PromptField;

    if (!IsEmpty(import.SearchCsePersonsWorkSet.Number))
    {
      MoveCsePersonsWorkSet(import.SearchCsePersonsWorkSet, export.Search);
    }
    else if (!IsEmpty(import.SearchCsePerson.Number))
    {
      export.Search.Number = import.SearchCsePerson.Number;
    }

    if (!IsEmpty(export.Search.Number))
    {
      // ****  Pad AE case number with leading zeros ****
      local.TextWorkArea.Text10 = export.Search.Number;
      UseEabPadLeftWithZeros();
      export.Search.Number = local.TextWorkArea.Text10;
    }

    export.SearchFrom.TextMonthYear = import.SearchFrom.TextMonthYear;
    export.SearchTo.TextMonthYear = import.SearchTo.TextMonthYear;

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
        export.Detail.Update.DtlGrpAfFc.AdjustmentAmount =
          import.Detail.Item.DtlGrpAfFc.AdjustmentAmount;
        export.Detail.Update.DtlGrpMedical.AdjustmentAmount =
          import.Detail.Item.DtlGrpMedical.AdjustmentAmount;
        export.Detail.Update.DtlImHousehold.AeCaseNo =
          import.Detail.Item.DtlImHousehold.AeCaseNo;
        export.Detail.Next();
      }

      export.TotGrp.Assign(import.TotGrp);
      export.TotGrpAfFcColl.CollectionAmountApplied =
        import.TotGrpAfFcColl.CollectionAmountApplied;
      export.TotGrpMedicalColl.CollectionAmountApplied =
        import.TotGrpMedicalColl.CollectionAmountApplied;
      export.TotGrpAfFc.AdjustmentAmount = import.TotGrpAfFc.AdjustmentAmount;
      export.TotGrpMedical.AdjustmentAmount =
        import.TotGrpMedical.AdjustmentAmount;
      export.TotGrpAfFcOwed.TotalCurrency = import.TotGrpAfFcOwed.TotalCurrency;
      export.TotGrpMedicalOwed.TotalCurrency =
        import.TotGrpMedicalOwed.TotalCurrency;
    }

    if (!IsEmpty(import.NextTran.NextTransaction))
    {
      local.NextTranInfo.CsePersonNumber = export.Search.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.NextTran, "nextTransaction");

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

    if (Equal(global.Command, "IMHH") || Equal(global.Command, "URAH") || Equal
      (global.Command, "URAC") || Equal(global.Command, "URAA") || Equal
      (global.Command, "URAL") || Equal(global.Command, "UCOL") || Equal
      (global.Command, "UHMM"))
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
            export.DialogFlowImHousehold.AeCaseNo =
              export.Detail.Item.DtlImHousehold.AeCaseNo;

            break;
          default:
            var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

            return;
        }
      }

      export.DialogFlowCsePersonsWorkSet.Number = export.Search.Number;

      if (Equal(global.Command, "URAA"))
      {
        if (local.NoOfEntriesSelected.Count == 0)
        {
          ExitState = "OE0000_AT_LEAST_1_ENTRY_REQD";

          return;
        }
      }

      switch(TrimEnd(global.Command))
      {
        case "IMHH":
          ExitState = "ECO_LNK_TO_IMHH";

          break;
        case "URAH":
          ExitState = "ECO_LNK_TO_URAH";

          break;
        case "URAC":
          ExitState = "ECO_LNK_TO_URAC";

          break;
        case "URAA":
          ExitState = "ECO_LNK_TO_URAA";

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
        if (IsEmpty(export.Search.Number))
        {
          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }
        else if (ReadCsePerson())
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Search.FormattedName = "Not able to get name from ADABAS.";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            export.Search.FormattedName = local.CsePersonsWorkSet.FormattedName;
          }
        }
        else
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

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

        local.To.Month = export.SearchTo.NumericalMonth;
        local.To.Year = export.SearchTo.NumericalYear;

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

        local.From.Month = export.SearchFrom.NumericalMonth;
        local.From.Year = export.SearchFrom.NumericalYear;

        if (Lt(export.SearchTo.TextYearMonth, export.SearchFrom.TextYearMonth))
        {
          ExitState = "OE0000_FROM_DATE_SHOULD_BE_LESS";

          return;
        }

        UseOeCuraGetData();

        if (IsExitState("ACO_NI0000_LIST_EXCEED_MAX_LNGTH"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
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
      case "LIST":
        if (AsChar(export.CsePerson.PromptField) == 'S')
        {
          export.CsePerson.PromptField = "";
          ExitState = "ECO_LNK_TO_NAME";
        }
        else if (IsEmpty(export.CsePerson.PromptField))
        {
          var field = GetField(export.CsePerson, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else
        {
          var field = GetField(export.CsePerson, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

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

  private static void MoveDateWorkAttributes(DateWorkAttributes source,
    DateWorkAttributes target)
  {
    target.TextMonthYear = source.TextMonthYear;
    target.TextYearMonth = source.TextYearMonth;
    target.NumericalMonth = source.NumericalMonth;
    target.NumericalYear = source.NumericalYear;
  }

  private static void MoveDetail(OeCuraGetData.Export.DetailGroup source,
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
    target.DtlGrpAfFc.AdjustmentAmount = source.DtlGrpAfFc.AdjustmentAmount;
    target.DtlGrpMedical.AdjustmentAmount =
      source.DtlGrpMedical.AdjustmentAmount;
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
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

  private void UseOeCuraGetData()
  {
    var useImport = new OeCuraGetData.Import();
    var useExport = new OeCuraGetData.Export();

    MoveImHouseholdMbrMnthlySum(local.From, useImport.From);
    MoveImHouseholdMbrMnthlySum(local.To, useImport.To);
    useImport.CsePersonsWorkSet.Number = export.Search.Number;

    Call(OeCuraGetData.Execute, useImport, useExport);

    useExport.Detail.CopyTo(export.Detail, MoveDetail);
    export.TotGrp.Assign(useExport.TotGrp);
    export.TotGrpAfFcColl.CollectionAmountApplied =
      useExport.TotGrpAfFcColl.CollectionAmountApplied;
    export.TotGrpMedicalColl.CollectionAmountApplied =
      useExport.TotGrpMedicalColl.CollectionAmountApplied;
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

    useImport.Standard.NextTransaction = import.NextTran.NextTransaction;

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Search.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
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

      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 140;

      private Common dtlCommon;
      private DateWorkAttributes dtlDateWorkAttributes;
      private TextWorkArea dtlGrpLabel1;
      private TextWorkArea dtlGrpLabel2;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrpAfFcColl;
      private UraCollectionApplication dtlGrpMedicalColl;
      private ImHouseholdMbrMnthlyAdj dtlGrpAfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrpMedical;
      private ImHousehold dtlImHousehold;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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
    /// A value of ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet")]
    public CsePersonsWorkSet ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet = value;
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
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes")]
    public DateWorkAttributes ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes = value;
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
    /// A value of ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase")]
    public Case1 ZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase = value;
    }

    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private Standard csePerson;
    private CsePerson searchCsePerson;
    private DateWorkAttributes searchFrom;
    private DateWorkAttributes searchTo;
    private CsePersonsWorkSet zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCsePersonsWorkSet;
    private Array<DetailGroup> detail;
    private ImHouseholdMbrMnthlySum totGrp;
    private UraCollectionApplication totGrpAfFcColl;
    private UraCollectionApplication totGrpMedicalColl;
    private UraCollectionApplication totGrpSsColl;
    private ImHouseholdMbrMnthlyAdj totGrpAfFc;
    private ImHouseholdMbrMnthlyAdj totGrpMedical;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzDateWorkAttributes;
      
    private Standard nextTran;
    private Case1 zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzCase;
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

      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 140;

      private Common dtlCommon;
      private DateWorkAttributes dtlDateWorkAttributes;
      private TextWorkArea dtlGrpLabel1;
      private TextWorkArea dtlGrpLabel2;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrpAfFcColl;
      private UraCollectionApplication dtlGrpMedicalColl;
      private ImHouseholdMbrMnthlyAdj dtlGrpAfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrpMedical;
      private ImHousehold dtlImHousehold;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of DialogFlowImHousehold.
    /// </summary>
    [JsonPropertyName("dialogFlowImHousehold")]
    public ImHousehold DialogFlowImHousehold
    {
      get => dialogFlowImHousehold ??= new();
      set => dialogFlowImHousehold = value;
    }

    /// <summary>
    /// A value of DialogFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("dialogFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet DialogFlowCsePersonsWorkSet
    {
      get => dialogFlowCsePersonsWorkSet ??= new();
      set => dialogFlowCsePersonsWorkSet = value;
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
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public CsePersonsWorkSet Zzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Standard NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    private CsePersonsWorkSet search;
    private Standard csePerson;
    private DateWorkAttributes searchFrom;
    private DateWorkAttributes searchTo;
    private ImHousehold dialogFlowImHousehold;
    private CsePersonsWorkSet dialogFlowCsePersonsWorkSet;
    private DateWorkAttributes dialogFlowDateWorkAttributes;
    private DateWorkArea dialogFlowDateWorkArea;
    private Array<DetailGroup> detail;
    private ImHouseholdMbrMnthlySum totGrp;
    private UraCollectionApplication totGrpAfFcColl;
    private UraCollectionApplication totGrpMedicalColl;
    private ImHouseholdMbrMnthlyAdj totGrpAfFc;
    private ImHouseholdMbrMnthlyAdj totGrpMedical;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
    private CsePersonsWorkSet zzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private Standard nextTran;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public ImHouseholdMbrMnthlySum From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public ImHouseholdMbrMnthlySum To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx")]
    public DateWorkArea Zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private NextTranInfo nextTranInfo;
    private TextWorkArea textWorkArea;
    private Common noOfEntriesSelected;
    private ImHouseholdMbrMnthlySum from;
    private ImHouseholdMbrMnthlySum to;
    private DateWorkArea zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzx;
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
