// Program: OE_URAC_URA_BY_COURT_ORDER, ID: 372673629, model: 746.
// Short name: SWEURACP
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
/// A program: OE_URAC_URA_BY_COURT_ORDER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUracUraByCourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAC_URA_BY_COURT_ORDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUracUraByCourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUracUraByCourtOrder.
  /// </summary>
  public OeUracUraByCourtOrder(IContext context, Import import, Export export):
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
    // Fangman        06-03-00  Deleted all code & views & copied code from 
    // URAH.
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
    export.CourtOrder.PromptField = import.CourtOrder.PromptField;
    export.Search.StandardNumber = import.Search.StandardNumber;
    export.SearchFrom.TextMonthYear = import.SearchFrom.TextMonthYear;
    export.SearchTo.TextMonthYear = import.SearchTo.TextMonthYear;
    MoveCsePersonsWorkSet(import.First, export.First);
    MoveCsePersonsWorkSet(import.Second, export.Second);

    if (Equal(global.Command, "RETLACS"))
    {
      if (!IsEmpty(import.FromLacs.StandardNumber))
      {
        export.Search.StandardNumber = import.FromLacs.StandardNumber ?? "";
      }

      global.Command = "DISPLAY";
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

    if (IsExitState("ACO_NI0000_NO_CHG_TO_SEARCH_CRIT"))
    {
      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      local.NextTranInfo.StandardCrtOrdNumber =
        export.Search.StandardNumber ?? "";
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

    if (Equal(global.Command, "IMHH") || Equal(global.Command, "URAH") || Equal
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

            break;
          default:
            var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

            return;
        }
      }

      export.DialogFlowLegalAction.StandardNumber =
        export.Search.StandardNumber;

      switch(TrimEnd(global.Command))
      {
        case "IMHH":
          ExitState = "ECO_LNK_TO_IMHH";

          break;
        case "URAH":
          ExitState = "ECO_LNK_TO_URAH";

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
        if (IsEmpty(export.Search.StandardNumber))
        {
          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }
        else if (ReadLegalAction())
        {
          // Get the obligors for the court order (primaty & secondary).
          if (ReadObligationCsePerson())
          {
            export.First.Number = entities.CsePerson.Number;

            if (AsChar(local.TraceMode.Flag) != 'Y')
            {
              UseSiReadCsePerson1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.First.FormattedName =
                  "Not able to get name from ADABAS.";
                ExitState = "ACO_NN0000_ALL_OK";
              }
              else
              {
                export.First.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
              }
            }

            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              if (ReadCsePerson2())
              {
                // Continue
              }
              else if (ReadCsePerson1())
              {
                // Continue
              }
              else
              {
                goto Test;
              }

              export.Second.Number = entities.Other.Number;

              if (AsChar(local.TraceMode.Flag) != 'Y')
              {
                UseSiReadCsePerson2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Second.FormattedName =
                    "Not able to get name from ADABAS.";
                  ExitState = "ACO_NN0000_ALL_OK";
                }
                else
                {
                  export.Second.FormattedName =
                    local.CsePersonsWorkSet.FormattedName;
                }
              }
            }

Test:

            if (IsEmpty(export.First.Number))
            {
              ExitState = "FN0000_OBLIGOR_NF";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_OBLIGATION_NF";

            return;
          }

          // Get all people associated to court order.
          foreach(var item in ReadCsePerson3())
          {
            local.PeopleOnCtOrder.Index = -1;

            while(local.PeopleOnCtOrder.Index + 1 < local.PeopleOnCtOrder.Count)
            {
              ++local.PeopleOnCtOrder.Index;
              local.PeopleOnCtOrder.CheckSize();

              if (Equal(entities.CsePerson.Number,
                local.PeopleOnCtOrder.Item.PeopleOnCtOrderGrpDtl.Number))
              {
                goto ReadEach;
              }
            }

            ++local.PeopleOnCtOrder.Index;
            local.PeopleOnCtOrder.CheckSize();

            local.PeopleOnCtOrder.Update.PeopleOnCtOrderGrpDtl.Number =
              entities.CsePerson.Number;

ReadEach:
            ;
          }

          if (local.PeopleOnCtOrder.Count < 1)
          {
            ExitState = "OE0000_NO_PERSON_FOUND_ON_CT_ORD";

            return;
          }
        }
        else
        {
          var field = GetField(export.Search, "standardNumber");

          field.Error = true;

          ExitState = "FN0000_COURT_ORDER_NF";

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
          ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

          return;
        }

        if (local.PeopleOnCtOrder.Count > 0)
        {
          local.PeopleOnCtOrder.Index = -1;

          while(local.PeopleOnCtOrder.Index + 1 < local.PeopleOnCtOrder.Count)
          {
            ++local.PeopleOnCtOrder.Index;
            local.PeopleOnCtOrder.CheckSize();

            UseOeUracGetData();

            if (!local.Detail.IsEmpty)
            {
              UseOeUracCombineData();
            }

            UseFnGetTotalOwedForSupported();
            export.TotGrpAfFcOwed.TotalCurrency += local.TotGrpAfFcOwed.
              TotalCurrency;
            export.TotGrpMedicalOwed.TotalCurrency += local.TotGrpMedicalOwed.
              TotalCurrency;
          }
        }

        if (IsExitState("ACO_NI0000_LIST_EXCEED_MAX_LNGTH"))
        {
          // Continue
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
        if (AsChar(export.CourtOrder.PromptField) == 'S')
        {
          export.CourtOrder.PromptField = "";
          ExitState = "ECO_LNK_TO_LACS";
        }
        else if (IsEmpty(export.CourtOrder.PromptField))
        {
          var field = GetField(export.CourtOrder, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else
        {
          var field = GetField(export.CourtOrder, "promptField");

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

  private static void MoveDetail1(OeUracCombineData.Export.DetailGroup source,
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

  private static void MoveDetail2(OeUracGetData.Export.DetailGroup source,
    Local.DetailGroup target)
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

  private static void MoveDetail3(Local.DetailGroup source,
    OeUracCombineData.Import.DetailGrp1Group target)
  {
    target.DtlGrp1Common.SelectChar = source.DtlCommon.SelectChar;
    target.DtlGrp1DateWorkAttributes.TextMonthYear =
      source.DtlDateWorkAttributes.TextMonthYear;
    target.DtlGrp1Label1.Text8 = source.DtlGrpLabel1.Text8;
    target.DtlGrp1Label2.Text8 = source.DtlGrpLabel2.Text8;
    target.DtlGrp1ImHouseholdMbrMnthlySum.Assign(
      source.DtlImHouseholdMbrMnthlySum);
    target.DtlGrp1AfFcColl.CollectionAmountApplied =
      source.DtlGrpAfFcColl.CollectionAmountApplied;
    target.DtlGrp1MedicalColl.CollectionAmountApplied =
      source.DtlGrpMedicalColl.CollectionAmountApplied;
    target.DtlGrp1SsColl.CollectionAmountApplied =
      source.DtlGrpSsColl.CollectionAmountApplied;
    target.DtlGrp1AfFc.AdjustmentAmount = source.DtlGrpAfFc.AdjustmentAmount;
    target.DtlGrp1Medical.AdjustmentAmount =
      source.DtlGrpMedical.AdjustmentAmount;
  }

  private static void MoveDetail4(Export.DetailGroup source,
    OeUracCombineData.Import.DetailGrp2Group target)
  {
    target.DtlGrp2Common.SelectChar = source.DtlCommon.SelectChar;
    target.DtlGrp2DateWorkAttributes.TextMonthYear =
      source.DtlDateWorkAttributes.TextMonthYear;
    target.DtlGrp2Label1.Text8 = source.DtlGrpLabel1.Text8;
    target.DtlGrp2Label2.Text8 = source.DtlGrpLabel2.Text8;
    target.DtlGrp2ImHouseholdMbrMnthlySum.Assign(
      source.DtlImHouseholdMbrMnthlySum);
    target.DtlGrp2AfFcColl.CollectionAmountApplied =
      source.DtlGrpAfFcColl.CollectionAmountApplied;
    target.DtlGrp2MedicalColl.CollectionAmountApplied =
      source.DtlGrpMedicalColl.CollectionAmountApplied;
    target.DtlGrp2SsColl.CollectionAmountApplied =
      source.DtlGrpSsColl.CollectionAmountApplied;
    target.DtlGrp2AfFc.AdjustmentAmount = source.DtlGrpAfFc.AdjustmentAmount;
    target.DtlGrp2Medical.AdjustmentAmount =
      source.DtlGrpMedical.AdjustmentAmount;
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private static void MoveTotal1(OeUracGetData.Export.TotalGroup source,
    Local.TotalGroup target)
  {
    target.Tot.Assign(source.Tot);
    target.TotGrpAfFcColl.CollectionAmountApplied =
      source.TotGrpAfFcColl.CollectionAmountApplied;
    target.TotGrpMedicalColl.CollectionAmountApplied =
      source.TotGrpMedicalColl.CollectionAmountApplied;
    target.TotGrpSsColl.CollectionAmountApplied =
      source.TotGrpSsColl.CollectionAmountApplied;
    target.TotGrpAfFc.AdjustmentAmount = source.TotGrpAfFc.AdjustmentAmount;
    target.TotGrpMedical.AdjustmentAmount =
      source.TotGrpMedical.AdjustmentAmount;
  }

  private static void MoveTotal2(Local.TotalGroup source,
    OeUracCombineData.Import.TotalGroup target)
  {
    target.Tot.Assign(source.Tot);
    target.TotGrpAfFcColl.CollectionAmountApplied =
      source.TotGrpAfFcColl.CollectionAmountApplied;
    target.TotGrpMedicalColl.CollectionAmountApplied =
      source.TotGrpMedicalColl.CollectionAmountApplied;
    target.TotGrpSsColl.CollectionAmountApplied =
      source.TotGrpSsColl.CollectionAmountApplied;
    target.TotGrpAfFc.AdjustmentAmount = source.TotGrpAfFc.AdjustmentAmount;
    target.TotGrpMedical.AdjustmentAmount =
      source.TotGrpMedical.AdjustmentAmount;
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

  private void UseFnGetTotalOwedForSupported()
  {
    var useImport = new FnGetTotalOwedForSupported.Import();
    var useExport = new FnGetTotalOwedForSupported.Export();

    useImport.Supported.Number =
      local.PeopleOnCtOrder.Item.PeopleOnCtOrderGrpDtl.Number;

    Call(FnGetTotalOwedForSupported.Execute, useImport, useExport);

    local.TotGrpAfFcOwed.TotalCurrency = useExport.TotAfFcOwed.TotalCurrency;
    local.TotGrpMedicalOwed.TotalCurrency =
      useExport.TotMedicalOwed.TotalCurrency;
  }

  private void UseOeUracCombineData()
  {
    var useImport = new OeUracCombineData.Import();
    var useExport = new OeUracCombineData.Export();

    local.Detail.CopyTo(useImport.DetailGrp1, MoveDetail3);
    MoveTotal2(local.Total, useImport.Total);
    export.Detail.CopyTo(useImport.DetailGrp2, MoveDetail4);
    useExport.Total.Tot.Assign(export.TotGrp);
    useExport.Total.TotGrpAfFcColl.CollectionAmountApplied =
      export.TotGrpAfFcColl.CollectionAmountApplied;
    useExport.Total.TotGrpMedicalColl.CollectionAmountApplied =
      export.TotGrpMedicalColl.CollectionAmountApplied;
    useExport.Total.TotGrpSsColl.CollectionAmountApplied =
      export.TotGrpSsColl.CollectionAmountApplied;
    useExport.Total.TotGrpAfFc.AdjustmentAmount =
      export.TotGrpAfFc.AdjustmentAmount;
    useExport.Total.TotGrpMedical.AdjustmentAmount =
      export.TotGrpMedical.AdjustmentAmount;

    Call(OeUracCombineData.Execute, useImport, useExport);

    useExport.Detail.CopyTo(export.Detail, MoveDetail1);
    export.TotGrp.Assign(useExport.Total.Tot);
    export.TotGrpAfFcColl.CollectionAmountApplied =
      useExport.Total.TotGrpAfFcColl.CollectionAmountApplied;
    export.TotGrpMedicalColl.CollectionAmountApplied =
      useExport.Total.TotGrpMedicalColl.CollectionAmountApplied;
    export.TotGrpSsColl.CollectionAmountApplied =
      useExport.Total.TotGrpSsColl.CollectionAmountApplied;
    export.TotGrpAfFc.AdjustmentAmount =
      useExport.Total.TotGrpAfFc.AdjustmentAmount;
    export.TotGrpMedical.AdjustmentAmount =
      useExport.Total.TotGrpMedical.AdjustmentAmount;
  }

  private void UseOeUracGetData()
  {
    var useImport = new OeUracGetData.Import();
    var useExport = new OeUracGetData.Export();

    MoveImHouseholdMbrMnthlySum(local.From, useImport.From);
    MoveImHouseholdMbrMnthlySum(local.To, useImport.To);
    useImport.CsePerson.Number =
      local.PeopleOnCtOrder.Item.PeopleOnCtOrderGrpDtl.Number;

    Call(OeUracGetData.Execute, useImport, useExport);

    useExport.Detail.CopyTo(local.Detail, MoveDetail2);
    MoveTotal1(useExport.Total, local.Total);
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

    useImport.LegalAction.StandardNumber = export.Search.StandardNumber;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.First.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Second.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Other.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Other.Number = db.GetString(reader, 0);
        entities.Other.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Other.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Other.Number = db.GetString(reader, 0);
        entities.Other.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
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
      public const int Capacity = 140;

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
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public Standard CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public CsePersonsWorkSet First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Second.
    /// </summary>
    [JsonPropertyName("second")]
    public CsePersonsWorkSet Second
    {
      get => second ??= new();
      set => second = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzz")]
    public LegalAction Zzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of FromLacs.
    /// </summary>
    [JsonPropertyName("fromLacs")]
    public LegalAction FromLacs
    {
      get => fromLacs ??= new();
      set => fromLacs = value;
    }

    private LegalAction search;
    private Standard courtOrder;
    private CsePersonsWorkSet first;
    private CsePersonsWorkSet second;
    private DateWorkAttributes searchFrom;
    private Standard standard;
    private LegalAction zzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzz;
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
    private LegalAction fromLacs;
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
      public const int Capacity = 140;

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
    /// A value of DialogFlowLegalAction.
    /// </summary>
    [JsonPropertyName("dialogFlowLegalAction")]
    public LegalAction DialogFlowLegalAction
    {
      get => dialogFlowLegalAction ??= new();
      set => dialogFlowLegalAction = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public Standard CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public CsePersonsWorkSet First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Second.
    /// </summary>
    [JsonPropertyName("second")]
    public CsePersonsWorkSet Second
    {
      get => second ??= new();
      set => second = value;
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
    /// A value of Zzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzz")]
    public LegalAction Zzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkAttributes Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
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

    private LegalAction dialogFlowLegalAction;
    private DateWorkAttributes dialogFlowDateWorkAttributes;
    private LegalAction search;
    private Standard courtOrder;
    private CsePersonsWorkSet first;
    private CsePersonsWorkSet second;
    private DateWorkAttributes searchFrom;
    private DateWorkAttributes searchTo;
    private LegalAction zzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private DateWorkAttributes zzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
    private Standard standard;
    private Array<DetailGroup> detail;
    private ImHouseholdMbrMnthlySum totGrp;
    private UraCollectionApplication totGrpAfFcColl;
    private UraCollectionApplication totGrpMedicalColl;
    private UraCollectionApplication totGrpSsColl;
    private ImHouseholdMbrMnthlyAdj totGrpAfFc;
    private ImHouseholdMbrMnthlyAdj totGrpMedical;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
      public const int Capacity = 140;

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

    /// <summary>A TotalGroup group.</summary>
    [Serializable]
    public class TotalGroup
    {
      /// <summary>
      /// A value of Tot.
      /// </summary>
      [JsonPropertyName("tot")]
      public ImHouseholdMbrMnthlySum Tot
      {
        get => tot ??= new();
        set => tot = value;
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

      private ImHouseholdMbrMnthlySum tot;
      private UraCollectionApplication totGrpAfFcColl;
      private UraCollectionApplication totGrpMedicalColl;
      private UraCollectionApplication totGrpSsColl;
      private ImHouseholdMbrMnthlyAdj totGrpAfFc;
      private ImHouseholdMbrMnthlyAdj totGrpMedical;
    }

    /// <summary>A PeopleOnCtOrderGroup group.</summary>
    [Serializable]
    public class PeopleOnCtOrderGroup
    {
      /// <summary>
      /// A value of PeopleOnCtOrderGrpDtl.
      /// </summary>
      [JsonPropertyName("peopleOnCtOrderGrpDtl")]
      public CsePerson PeopleOnCtOrderGrpDtl
      {
        get => peopleOnCtOrderGrpDtl ??= new();
        set => peopleOnCtOrderGrpDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson peopleOnCtOrderGrpDtl;
    }

    /// <summary>
    /// A value of TraceMode.
    /// </summary>
    [JsonPropertyName("traceMode")]
    public Common TraceMode
    {
      get => traceMode ??= new();
      set => traceMode = value;
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
    /// Gets a value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public TotalGroup Total
    {
      get => total ?? (total = new());
      set => total = value;
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
    /// Gets a value of PeopleOnCtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<PeopleOnCtOrderGroup> PeopleOnCtOrder =>
      peopleOnCtOrder ??= new(PeopleOnCtOrderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PeopleOnCtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("peopleOnCtOrder")]
    [Computed]
    public IList<PeopleOnCtOrderGroup> PeopleOnCtOrder_Json
    {
      get => peopleOnCtOrder;
      set => PeopleOnCtOrder.Assign(value);
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public DateWorkArea Zzzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    private Common traceMode;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<DetailGroup> detail;
    private TotalGroup total;
    private Common totGrpAfFcOwed;
    private Common totGrpMedicalOwed;
    private Array<PeopleOnCtOrderGroup> peopleOnCtOrder;
    private TextWorkArea textWorkArea;
    private Common noOfEntriesSelected;
    private ImHouseholdMbrMnthlySum from;
    private ImHouseholdMbrMnthlySum to;
    private NextTranInfo nextTranInfo;
    private DateWorkArea zzzzzzzzzzzzzzzzzzzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Joint.
    /// </summary>
    [JsonPropertyName("joint")]
    public Obligation Joint
    {
      get => joint ??= new();
      set => joint = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public CsePerson Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
    private Obligation obligation;
    private Obligation joint;
    private CsePersonAccount obligor;
    private CsePerson other;
    private ObligationRln obligationRln;
  }
#endregion
}
