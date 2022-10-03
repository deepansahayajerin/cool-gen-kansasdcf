// Program: OE_URAC_COMBINE_DATA, ID: 374460590, model: 746.
// Short name: SWE02704
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAC_COMBINE_DATA.
/// </summary>
[Serializable]
public partial class OeUracCombineData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAC_COMBINE_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUracCombineData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUracCombineData.
  /// </summary>
  public OeUracCombineData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    import.DetailGrp1.Index = -1;
    import.DetailGrp2.Index = -1;
    export.Detail.Index = -1;

    if (import.DetailGrp2.Count > 0)
    {
      import.DetailGrp1.Index = 0;
      import.DetailGrp1.CheckSize();

      import.DetailGrp2.Index = 0;
      import.DetailGrp2.CheckSize();

      while(import.DetailGrp1.Index < import.DetailGrp1.Count && import
        .DetailGrp2.Index < import.DetailGrp2.Count)
      {
        local.Loc1DateMonth.Text2 =
          Substring(import.DetailGrp1.Item.DtlGrp1DateWorkAttributes.
            TextMonthYear, 1, 2);
        local.Loc1DateYear.Text4 =
          Substring(import.DetailGrp1.Item.DtlGrp1DateWorkAttributes.
            TextMonthYear, 3, 4);
        local.Loc2DateMonth.Text2 =
          Substring(import.DetailGrp2.Item.DtlGrp2DateWorkAttributes.
            TextMonthYear, 1, 2);
        local.Loc2DateYear.Text4 =
          Substring(import.DetailGrp2.Item.DtlGrp2DateWorkAttributes.
            TextMonthYear, 3, 4);

        if (Lt(local.Loc2DateYear.Text4, local.Loc1DateYear.Text4) || Equal
          (local.Loc1DateYear.Text4, local.Loc2DateYear.Text4) && Lt
          (local.Loc2DateMonth.Text2, local.Loc1DateMonth.Text2))
        {
          // Move imp 1 to exp
          if (export.Detail.Index + 1 < Export.DetailGroup.Capacity)
          {
            ++export.Detail.Index;
            export.Detail.CheckSize();
          }
          else
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          UseOeUracAddDtls2();

          if (import.DetailGrp1.Index + 1 >= import.DetailGrp1.Count)
          {
            --import.DetailGrp2.Index;
            import.DetailGrp2.CheckSize();

            goto Test;
          }

          ++import.DetailGrp1.Index;
          import.DetailGrp1.CheckSize();
        }
        else if (Equal(import.DetailGrp1.Item.DtlGrp1DateWorkAttributes.
          TextMonthYear,
          import.DetailGrp2.Item.DtlGrp2DateWorkAttributes.TextMonthYear))
        {
          // Add imp 1 to imp 2 giving exp
          if (export.Detail.Index + 1 < Export.DetailGroup.Capacity)
          {
            ++export.Detail.Index;
            export.Detail.CheckSize();
          }
          else
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          UseOeUracAddDtls1();

          if (import.DetailGrp1.Index + 1 >= import.DetailGrp1.Count)
          {
            if (import.DetailGrp2.Index + 1 >= import.DetailGrp2.Count)
            {
              goto Test;
            }
            else
            {
              --import.DetailGrp2.Index;
              import.DetailGrp2.CheckSize();

              goto Test;
            }
          }
          else if (import.DetailGrp2.Index + 1 >= import.DetailGrp2.Count)
          {
            --import.DetailGrp1.Index;
            import.DetailGrp1.CheckSize();

            goto Test;
          }

          ++import.DetailGrp1.Index;
          import.DetailGrp1.CheckSize();

          ++import.DetailGrp2.Index;
          import.DetailGrp2.CheckSize();
        }
        else
        {
          // Move imp 2 to exp
          if (export.Detail.Index + 1 < Export.DetailGroup.Capacity)
          {
            ++export.Detail.Index;
            export.Detail.CheckSize();
          }
          else
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          UseOeUracAddDtls3();

          if (import.DetailGrp2.Index + 1 >= import.DetailGrp2.Count)
          {
            --import.DetailGrp1.Index;
            import.DetailGrp1.CheckSize();

            goto Test;
          }

          ++import.DetailGrp2.Index;
          import.DetailGrp2.CheckSize();
        }
      }
    }

Test:

    while(import.DetailGrp1.Index + 1 < import.DetailGrp1.Count)
    {
      ++import.DetailGrp1.Index;
      import.DetailGrp1.CheckSize();

      if (export.Detail.Index + 1 < Export.DetailGroup.Capacity)
      {
        ++export.Detail.Index;
        export.Detail.CheckSize();
      }
      else
      {
        ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

        return;
      }

      UseOeUracAddDtls2();
    }

    while(import.DetailGrp2.Index + 1 < import.DetailGrp2.Count)
    {
      ++import.DetailGrp2.Index;
      import.DetailGrp2.CheckSize();

      if (export.Detail.Index + 1 < Export.DetailGroup.Capacity)
      {
        ++export.Detail.Index;
        export.Detail.CheckSize();
      }
      else
      {
        ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

        return;
      }

      UseOeUracAddDtls3();
    }

    export.Total.Tot.GrantAmount =
      export.Total.Tot.GrantAmount.GetValueOrDefault() + import
      .Total.Tot.GrantAmount.GetValueOrDefault();
    export.Total.Tot.GrantMedicalAmount =
      export.Total.Tot.GrantMedicalAmount.GetValueOrDefault() + import
      .Total.Tot.GrantMedicalAmount.GetValueOrDefault();
    export.Total.Tot.UraAmount =
      export.Total.Tot.UraAmount.GetValueOrDefault() + import
      .Total.Tot.UraAmount.GetValueOrDefault();
    export.Total.Tot.UraMedicalAmount =
      export.Total.Tot.UraMedicalAmount.GetValueOrDefault() + import
      .Total.Tot.UraMedicalAmount.GetValueOrDefault();
    export.Total.TotGrpAfFcColl.CollectionAmountApplied += import.Total.
      TotGrpAfFcColl.CollectionAmountApplied;
    export.Total.TotGrpMedicalColl.CollectionAmountApplied += import.Total.
      TotGrpMedicalColl.CollectionAmountApplied;
    export.Total.TotGrpSsColl.CollectionAmountApplied += import.Total.
      TotGrpSsColl.CollectionAmountApplied;
    export.Total.TotGrpAfFc.AdjustmentAmount += import.Total.TotGrpAfFc.
      AdjustmentAmount;
    export.Total.TotGrpMedical.AdjustmentAmount += import.Total.TotGrpMedical.
      AdjustmentAmount;
  }

  private static void MoveDetail(OeUracAddDtls.Export.DetailGroup source,
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

  private static void MoveDetailGrp1(Import.DetailGrp1Group source,
    OeUracAddDtls.Import.DetailGrp1Group target)
  {
    target.DtlGrp1Common.SelectChar = source.DtlGrp1Common.SelectChar;
    target.DtlGrp1DateWorkAttributes.TextMonthYear =
      source.DtlGrp1DateWorkAttributes.TextMonthYear;
    target.DtlGrp1Label1.Text8 = source.DtlGrp1Label1.Text8;
    target.DtlGrp1Label2.Text8 = source.DtlGrp1Label2.Text8;
    target.DtlGrp1ImHouseholdMbrMnthlySum.Assign(
      source.DtlGrp1ImHouseholdMbrMnthlySum);
    target.DtlGrp1AfFcColl.CollectionAmountApplied =
      source.DtlGrp1AfFcColl.CollectionAmountApplied;
    target.DtlGrp1MedicalColl.CollectionAmountApplied =
      source.DtlGrp1MedicalColl.CollectionAmountApplied;
    target.DtlGrp1SsColl.CollectionAmountApplied =
      source.DtlGrp1SsColl.CollectionAmountApplied;
    target.DtlGrp1AfFc.AdjustmentAmount = source.DtlGrp1AfFc.AdjustmentAmount;
    target.DtlGrp1Medical.AdjustmentAmount =
      source.DtlGrp1Medical.AdjustmentAmount;
  }

  private static void MoveDetailGrp2(Import.DetailGrp2Group source,
    OeUracAddDtls.Import.DetailGrp2Group target)
  {
    target.DtlGrp2Common.SelectChar = source.DtlGrp2Common.SelectChar;
    target.DtlGrp2DateWorkAttributes.TextMonthYear =
      source.DtlGrp2DateWorkAttributes.TextMonthYear;
    target.DtlGrp2Label1.Text8 = source.DtlGrp2Label1.Text8;
    target.DtlGrp2Label2.Text8 = source.DtlGrp2Label2.Text8;
    target.DtlGrp2ImHouseholdMbrMnthlySum.Assign(
      source.DtlGrp2ImHouseholdMbrMnthlySum);
    target.DtlGrp2AfFcColl.CollectionAmountApplied =
      source.DtlGrp2AfFcColl.CollectionAmountApplied;
    target.DtlGrp2MedicalColl.CollectionAmountApplied =
      source.DtlGrp2MedicalColl.CollectionAmountApplied;
    target.DtlGrp2SsColl.CollectionAmountApplied =
      source.DtlGrp2SsColl.CollectionAmountApplied;
    target.DtlGrp2AfFc.AdjustmentAmount = source.DtlGrp2AfFc.AdjustmentAmount;
    target.DtlGrp2Medical.AdjustmentAmount =
      source.DtlGrp2Medical.AdjustmentAmount;
  }

  private static void MoveDetailGrp2ToDetailGrp1(Import.DetailGrp2Group source,
    OeUracAddDtls.Import.DetailGrp1Group target)
  {
    target.DtlGrp1Common.SelectChar = source.DtlGrp2Common.SelectChar;
    target.DtlGrp1DateWorkAttributes.TextMonthYear =
      source.DtlGrp2DateWorkAttributes.TextMonthYear;
    target.DtlGrp1Label1.Text8 = source.DtlGrp2Label1.Text8;
    target.DtlGrp1Label2.Text8 = source.DtlGrp2Label2.Text8;
    target.DtlGrp1ImHouseholdMbrMnthlySum.Assign(
      source.DtlGrp2ImHouseholdMbrMnthlySum);
    target.DtlGrp1AfFcColl.CollectionAmountApplied =
      source.DtlGrp2AfFcColl.CollectionAmountApplied;
    target.DtlGrp1MedicalColl.CollectionAmountApplied =
      source.DtlGrp2MedicalColl.CollectionAmountApplied;
    target.DtlGrp1SsColl.CollectionAmountApplied =
      source.DtlGrp2SsColl.CollectionAmountApplied;
    target.DtlGrp1AfFc.AdjustmentAmount = source.DtlGrp2AfFc.AdjustmentAmount;
    target.DtlGrp1Medical.AdjustmentAmount =
      source.DtlGrp2Medical.AdjustmentAmount;
  }

  private static void MoveInitDetailToDetailGrp2(Local.InitDetailGroup source,
    OeUracAddDtls.Import.DetailGrp2Group target)
  {
    target.DtlGrp2Common.SelectChar = source.InitDtlCommon.SelectChar;
    target.DtlGrp2DateWorkAttributes.TextMonthYear =
      source.InitDtlDateWorkAttributes.TextMonthYear;
    target.DtlGrp2Label1.Text8 = source.InitDtlGrpLabel1.Text8;
    target.DtlGrp2Label2.Text8 = source.InitDtlGrpLabel2.Text8;
    target.DtlGrp2ImHouseholdMbrMnthlySum.Assign(
      source.InitDtlImHouseholdMbrMnthlySum);
    target.DtlGrp2AfFcColl.CollectionAmountApplied =
      source.InitDtlGrpAfFcColl.CollectionAmountApplied;
    target.DtlGrp2MedicalColl.CollectionAmountApplied =
      source.InitDtlGrpMedicalColl.CollectionAmountApplied;
    target.DtlGrp2SsColl.CollectionAmountApplied =
      source.InitDtlGrpSsColl.CollectionAmountApplied;
    target.DtlGrp2AfFc.AdjustmentAmount =
      source.InitDtlGrpAfFc.AdjustmentAmount;
    target.DtlGrp2Medical.AdjustmentAmount =
      source.InitDtlGrpMedical.AdjustmentAmount;
  }

  private void UseOeUracAddDtls1()
  {
    var useImport = new OeUracAddDtls.Import();
    var useExport = new OeUracAddDtls.Export();

    MoveDetailGrp1(import.DetailGrp1.Item, useImport.DetailGrp1);
    MoveDetailGrp2(import.DetailGrp2.Item, useImport.DetailGrp2);

    Call(OeUracAddDtls.Execute, useImport, useExport);

    export.Detail.Clear();
    MoveDetail(useExport.Detail, export.Detail[0]);
  }

  private void UseOeUracAddDtls2()
  {
    var useImport = new OeUracAddDtls.Import();
    var useExport = new OeUracAddDtls.Export();

    MoveDetailGrp1(import.DetailGrp1.Item, useImport.DetailGrp1);
    MoveInitDetailToDetailGrp2(local.InitDetail, useImport.DetailGrp2);

    Call(OeUracAddDtls.Execute, useImport, useExport);

    export.Detail.Clear();
    MoveDetail(useExport.Detail, export.Detail[0]);
  }

  private void UseOeUracAddDtls3()
  {
    var useImport = new OeUracAddDtls.Import();
    var useExport = new OeUracAddDtls.Export();

    MoveDetailGrp2ToDetailGrp1(import.DetailGrp2.Item, useImport.DetailGrp1);
    MoveInitDetailToDetailGrp2(local.InitDetail, useImport.DetailGrp2);

    Call(OeUracAddDtls.Execute, useImport, useExport);

    export.Detail.Clear();
    MoveDetail(useExport.Detail, export.Detail[0]);
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
    /// <summary>A DetailGrp1Group group.</summary>
    [Serializable]
    public class DetailGrp1Group
    {
      /// <summary>
      /// A value of DtlGrp1Common.
      /// </summary>
      [JsonPropertyName("dtlGrp1Common")]
      public Common DtlGrp1Common
      {
        get => dtlGrp1Common ??= new();
        set => dtlGrp1Common = value;
      }

      /// <summary>
      /// A value of DtlGrp1DateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dtlGrp1DateWorkAttributes")]
      public DateWorkAttributes DtlGrp1DateWorkAttributes
      {
        get => dtlGrp1DateWorkAttributes ??= new();
        set => dtlGrp1DateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DtlGrp1Label1.
      /// </summary>
      [JsonPropertyName("dtlGrp1Label1")]
      public TextWorkArea DtlGrp1Label1
      {
        get => dtlGrp1Label1 ??= new();
        set => dtlGrp1Label1 = value;
      }

      /// <summary>
      /// A value of DtlGrp1Label2.
      /// </summary>
      [JsonPropertyName("dtlGrp1Label2")]
      public TextWorkArea DtlGrp1Label2
      {
        get => dtlGrp1Label2 ??= new();
        set => dtlGrp1Label2 = value;
      }

      /// <summary>
      /// A value of DtlGrp1ImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlGrp1ImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlGrp1ImHouseholdMbrMnthlySum
      {
        get => dtlGrp1ImHouseholdMbrMnthlySum ??= new();
        set => dtlGrp1ImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlGrp1AfFcColl.
      /// </summary>
      [JsonPropertyName("dtlGrp1AfFcColl")]
      public UraCollectionApplication DtlGrp1AfFcColl
      {
        get => dtlGrp1AfFcColl ??= new();
        set => dtlGrp1AfFcColl = value;
      }

      /// <summary>
      /// A value of DtlGrp1MedicalColl.
      /// </summary>
      [JsonPropertyName("dtlGrp1MedicalColl")]
      public UraCollectionApplication DtlGrp1MedicalColl
      {
        get => dtlGrp1MedicalColl ??= new();
        set => dtlGrp1MedicalColl = value;
      }

      /// <summary>
      /// A value of DtlGrp1SsColl.
      /// </summary>
      [JsonPropertyName("dtlGrp1SsColl")]
      public UraCollectionApplication DtlGrp1SsColl
      {
        get => dtlGrp1SsColl ??= new();
        set => dtlGrp1SsColl = value;
      }

      /// <summary>
      /// A value of DtlGrp1AfFc.
      /// </summary>
      [JsonPropertyName("dtlGrp1AfFc")]
      public ImHouseholdMbrMnthlyAdj DtlGrp1AfFc
      {
        get => dtlGrp1AfFc ??= new();
        set => dtlGrp1AfFc = value;
      }

      /// <summary>
      /// A value of DtlGrp1Medical.
      /// </summary>
      [JsonPropertyName("dtlGrp1Medical")]
      public ImHouseholdMbrMnthlyAdj DtlGrp1Medical
      {
        get => dtlGrp1Medical ??= new();
        set => dtlGrp1Medical = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 140;

      private Common dtlGrp1Common;
      private DateWorkAttributes dtlGrp1DateWorkAttributes;
      private TextWorkArea dtlGrp1Label1;
      private TextWorkArea dtlGrp1Label2;
      private ImHouseholdMbrMnthlySum dtlGrp1ImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrp1AfFcColl;
      private UraCollectionApplication dtlGrp1MedicalColl;
      private UraCollectionApplication dtlGrp1SsColl;
      private ImHouseholdMbrMnthlyAdj dtlGrp1AfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrp1Medical;
    }

    /// <summary>A DetailGrp2Group group.</summary>
    [Serializable]
    public class DetailGrp2Group
    {
      /// <summary>
      /// A value of DtlGrp2Common.
      /// </summary>
      [JsonPropertyName("dtlGrp2Common")]
      public Common DtlGrp2Common
      {
        get => dtlGrp2Common ??= new();
        set => dtlGrp2Common = value;
      }

      /// <summary>
      /// A value of DtlGrp2DateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dtlGrp2DateWorkAttributes")]
      public DateWorkAttributes DtlGrp2DateWorkAttributes
      {
        get => dtlGrp2DateWorkAttributes ??= new();
        set => dtlGrp2DateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DtlGrp2Label1.
      /// </summary>
      [JsonPropertyName("dtlGrp2Label1")]
      public TextWorkArea DtlGrp2Label1
      {
        get => dtlGrp2Label1 ??= new();
        set => dtlGrp2Label1 = value;
      }

      /// <summary>
      /// A value of DtlGrp2Label2.
      /// </summary>
      [JsonPropertyName("dtlGrp2Label2")]
      public TextWorkArea DtlGrp2Label2
      {
        get => dtlGrp2Label2 ??= new();
        set => dtlGrp2Label2 = value;
      }

      /// <summary>
      /// A value of DtlGrp2ImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlGrp2ImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlGrp2ImHouseholdMbrMnthlySum
      {
        get => dtlGrp2ImHouseholdMbrMnthlySum ??= new();
        set => dtlGrp2ImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlGrp2AfFcColl.
      /// </summary>
      [JsonPropertyName("dtlGrp2AfFcColl")]
      public UraCollectionApplication DtlGrp2AfFcColl
      {
        get => dtlGrp2AfFcColl ??= new();
        set => dtlGrp2AfFcColl = value;
      }

      /// <summary>
      /// A value of DtlGrp2MedicalColl.
      /// </summary>
      [JsonPropertyName("dtlGrp2MedicalColl")]
      public UraCollectionApplication DtlGrp2MedicalColl
      {
        get => dtlGrp2MedicalColl ??= new();
        set => dtlGrp2MedicalColl = value;
      }

      /// <summary>
      /// A value of DtlGrp2SsColl.
      /// </summary>
      [JsonPropertyName("dtlGrp2SsColl")]
      public UraCollectionApplication DtlGrp2SsColl
      {
        get => dtlGrp2SsColl ??= new();
        set => dtlGrp2SsColl = value;
      }

      /// <summary>
      /// A value of DtlGrp2AfFc.
      /// </summary>
      [JsonPropertyName("dtlGrp2AfFc")]
      public ImHouseholdMbrMnthlyAdj DtlGrp2AfFc
      {
        get => dtlGrp2AfFc ??= new();
        set => dtlGrp2AfFc = value;
      }

      /// <summary>
      /// A value of DtlGrp2Medical.
      /// </summary>
      [JsonPropertyName("dtlGrp2Medical")]
      public ImHouseholdMbrMnthlyAdj DtlGrp2Medical
      {
        get => dtlGrp2Medical ??= new();
        set => dtlGrp2Medical = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 140;

      private Common dtlGrp2Common;
      private DateWorkAttributes dtlGrp2DateWorkAttributes;
      private TextWorkArea dtlGrp2Label1;
      private TextWorkArea dtlGrp2Label2;
      private ImHouseholdMbrMnthlySum dtlGrp2ImHouseholdMbrMnthlySum;
      private UraCollectionApplication dtlGrp2AfFcColl;
      private UraCollectionApplication dtlGrp2MedicalColl;
      private UraCollectionApplication dtlGrp2SsColl;
      private ImHouseholdMbrMnthlyAdj dtlGrp2AfFc;
      private ImHouseholdMbrMnthlyAdj dtlGrp2Medical;
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

    /// <summary>
    /// Gets a value of DetailGrp1.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGrp1Group> DetailGrp1 => detailGrp1 ??= new(
      DetailGrp1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of DetailGrp1 for json serialization.
    /// </summary>
    [JsonPropertyName("detailGrp1")]
    [Computed]
    public IList<DetailGrp1Group> DetailGrp1_Json
    {
      get => detailGrp1;
      set => DetailGrp1.Assign(value);
    }

    /// <summary>
    /// Gets a value of DetailGrp2.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGrp2Group> DetailGrp2 => detailGrp2 ??= new(
      DetailGrp2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of DetailGrp2 for json serialization.
    /// </summary>
    [JsonPropertyName("detailGrp2")]
    [Computed]
    public IList<DetailGrp2Group> DetailGrp2_Json
    {
      get => detailGrp2;
      set => DetailGrp2.Assign(value);
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

    private Array<DetailGrp1Group> detailGrp1;
    private Array<DetailGrp2Group> detailGrp2;
    private TotalGroup total;
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

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity, 0);

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

    private Array<DetailGroup> detail;
    private TotalGroup total;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A InitDetailGroup group.</summary>
    [Serializable]
    public class InitDetailGroup
    {
      /// <summary>
      /// A value of InitDtlCommon.
      /// </summary>
      [JsonPropertyName("initDtlCommon")]
      public Common InitDtlCommon
      {
        get => initDtlCommon ??= new();
        set => initDtlCommon = value;
      }

      /// <summary>
      /// A value of InitDtlDateWorkAttributes.
      /// </summary>
      [JsonPropertyName("initDtlDateWorkAttributes")]
      public DateWorkAttributes InitDtlDateWorkAttributes
      {
        get => initDtlDateWorkAttributes ??= new();
        set => initDtlDateWorkAttributes = value;
      }

      /// <summary>
      /// A value of InitDtlGrpLabel1.
      /// </summary>
      [JsonPropertyName("initDtlGrpLabel1")]
      public TextWorkArea InitDtlGrpLabel1
      {
        get => initDtlGrpLabel1 ??= new();
        set => initDtlGrpLabel1 = value;
      }

      /// <summary>
      /// A value of InitDtlGrpLabel2.
      /// </summary>
      [JsonPropertyName("initDtlGrpLabel2")]
      public TextWorkArea InitDtlGrpLabel2
      {
        get => initDtlGrpLabel2 ??= new();
        set => initDtlGrpLabel2 = value;
      }

      /// <summary>
      /// A value of InitDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("initDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum InitDtlImHouseholdMbrMnthlySum
      {
        get => initDtlImHouseholdMbrMnthlySum ??= new();
        set => initDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of InitDtlGrpAfFcColl.
      /// </summary>
      [JsonPropertyName("initDtlGrpAfFcColl")]
      public UraCollectionApplication InitDtlGrpAfFcColl
      {
        get => initDtlGrpAfFcColl ??= new();
        set => initDtlGrpAfFcColl = value;
      }

      /// <summary>
      /// A value of InitDtlGrpMedicalColl.
      /// </summary>
      [JsonPropertyName("initDtlGrpMedicalColl")]
      public UraCollectionApplication InitDtlGrpMedicalColl
      {
        get => initDtlGrpMedicalColl ??= new();
        set => initDtlGrpMedicalColl = value;
      }

      /// <summary>
      /// A value of InitDtlGrpSsColl.
      /// </summary>
      [JsonPropertyName("initDtlGrpSsColl")]
      public UraCollectionApplication InitDtlGrpSsColl
      {
        get => initDtlGrpSsColl ??= new();
        set => initDtlGrpSsColl = value;
      }

      /// <summary>
      /// A value of InitDtlGrpAfFc.
      /// </summary>
      [JsonPropertyName("initDtlGrpAfFc")]
      public ImHouseholdMbrMnthlyAdj InitDtlGrpAfFc
      {
        get => initDtlGrpAfFc ??= new();
        set => initDtlGrpAfFc = value;
      }

      /// <summary>
      /// A value of InitDtlGrpMedical.
      /// </summary>
      [JsonPropertyName("initDtlGrpMedical")]
      public ImHouseholdMbrMnthlyAdj InitDtlGrpMedical
      {
        get => initDtlGrpMedical ??= new();
        set => initDtlGrpMedical = value;
      }

      private Common initDtlCommon;
      private DateWorkAttributes initDtlDateWorkAttributes;
      private TextWorkArea initDtlGrpLabel1;
      private TextWorkArea initDtlGrpLabel2;
      private ImHouseholdMbrMnthlySum initDtlImHouseholdMbrMnthlySum;
      private UraCollectionApplication initDtlGrpAfFcColl;
      private UraCollectionApplication initDtlGrpMedicalColl;
      private UraCollectionApplication initDtlGrpSsColl;
      private ImHouseholdMbrMnthlyAdj initDtlGrpAfFc;
      private ImHouseholdMbrMnthlyAdj initDtlGrpMedical;
    }

    /// <summary>
    /// Gets a value of InitDetail.
    /// </summary>
    [JsonPropertyName("initDetail")]
    public InitDetailGroup InitDetail
    {
      get => initDetail ?? (initDetail = new());
      set => initDetail = value;
    }

    /// <summary>
    /// A value of Loc1DateYear.
    /// </summary>
    [JsonPropertyName("loc1DateYear")]
    public TextWorkArea Loc1DateYear
    {
      get => loc1DateYear ??= new();
      set => loc1DateYear = value;
    }

    /// <summary>
    /// A value of Loc1DateMonth.
    /// </summary>
    [JsonPropertyName("loc1DateMonth")]
    public TextWorkArea Loc1DateMonth
    {
      get => loc1DateMonth ??= new();
      set => loc1DateMonth = value;
    }

    /// <summary>
    /// A value of Loc2DateYear.
    /// </summary>
    [JsonPropertyName("loc2DateYear")]
    public TextWorkArea Loc2DateYear
    {
      get => loc2DateYear ??= new();
      set => loc2DateYear = value;
    }

    /// <summary>
    /// A value of Loc2DateMonth.
    /// </summary>
    [JsonPropertyName("loc2DateMonth")]
    public TextWorkArea Loc2DateMonth
    {
      get => loc2DateMonth ??= new();
      set => loc2DateMonth = value;
    }

    private InitDetailGroup initDetail;
    private TextWorkArea loc1DateYear;
    private TextWorkArea loc1DateMonth;
    private TextWorkArea loc2DateYear;
    private TextWorkArea loc2DateMonth;
  }
#endregion
}
