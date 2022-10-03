// Program: OE_URAC_ADD_DTLS, ID: 374460465, model: 746.
// Short name: SWE02703
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAC_ADD_DTLS.
/// </summary>
[Serializable]
public partial class OeUracAddDtls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAC_ADD_DTLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUracAddDtls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUracAddDtls.
  /// </summary>
  public OeUracAddDtls(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Detail.DtlDateWorkAttributes.TextMonthYear =
      import.DetailGrp1.DtlGrp1DateWorkAttributes.TextMonthYear;
    export.Detail.DtlGrpLabel1.Text8 = import.DetailGrp1.DtlGrp1Label1.Text8;
    export.Detail.DtlGrpLabel2.Text8 = import.DetailGrp1.DtlGrp1Label2.Text8;
    export.Detail.DtlImHouseholdMbrMnthlySum.GrantAmount =
      import.DetailGrp1.DtlGrp1ImHouseholdMbrMnthlySum.GrantAmount.
        GetValueOrDefault() + import
      .DetailGrp2.DtlGrp2ImHouseholdMbrMnthlySum.GrantAmount.
        GetValueOrDefault();
    export.Detail.DtlImHouseholdMbrMnthlySum.GrantMedicalAmount =
      import.DetailGrp1.DtlGrp1ImHouseholdMbrMnthlySum.GrantMedicalAmount.
        GetValueOrDefault() + import
      .DetailGrp2.DtlGrp2ImHouseholdMbrMnthlySum.GrantMedicalAmount.
        GetValueOrDefault();
    export.Detail.DtlImHouseholdMbrMnthlySum.UraAmount =
      import.DetailGrp1.DtlGrp1ImHouseholdMbrMnthlySum.UraAmount.
        GetValueOrDefault() + import
      .DetailGrp2.DtlGrp2ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
    export.Detail.DtlImHouseholdMbrMnthlySum.UraMedicalAmount =
      import.DetailGrp1.DtlGrp1ImHouseholdMbrMnthlySum.UraMedicalAmount.
        GetValueOrDefault() + import
      .DetailGrp2.DtlGrp2ImHouseholdMbrMnthlySum.UraMedicalAmount.
        GetValueOrDefault();
    export.Detail.DtlGrpAfFcColl.CollectionAmountApplied =
      import.DetailGrp1.DtlGrp1AfFcColl.CollectionAmountApplied + import
      .DetailGrp2.DtlGrp2AfFcColl.CollectionAmountApplied;
    export.Detail.DtlGrpMedicalColl.CollectionAmountApplied =
      import.DetailGrp1.DtlGrp1MedicalColl.CollectionAmountApplied + import
      .DetailGrp2.DtlGrp2MedicalColl.CollectionAmountApplied;
    export.Detail.DtlGrpSsColl.CollectionAmountApplied =
      import.DetailGrp1.DtlGrp1SsColl.CollectionAmountApplied + import
      .DetailGrp2.DtlGrp2SsColl.CollectionAmountApplied;
    export.Detail.DtlGrpAfFc.AdjustmentAmount =
      import.DetailGrp1.DtlGrp1AfFc.AdjustmentAmount + import
      .DetailGrp2.DtlGrp2AfFc.AdjustmentAmount;
    export.Detail.DtlGrpMedical.AdjustmentAmount =
      import.DetailGrp1.DtlGrp1Medical.AdjustmentAmount + import
      .DetailGrp2.DtlGrp2Medical.AdjustmentAmount;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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

    /// <summary>
    /// Gets a value of DetailGrp1.
    /// </summary>
    [JsonPropertyName("detailGrp1")]
    public DetailGrp1Group DetailGrp1
    {
      get => detailGrp1 ?? (detailGrp1 = new());
      set => detailGrp1 = value;
    }

    /// <summary>
    /// Gets a value of DetailGrp2.
    /// </summary>
    [JsonPropertyName("detailGrp2")]
    public DetailGrp2Group DetailGrp2
    {
      get => detailGrp2 ?? (detailGrp2 = new());
      set => detailGrp2 = value;
    }

    private DetailGrp1Group detailGrp1;
    private DetailGrp2Group detailGrp2;
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
    /// Gets a value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public DetailGroup Detail
    {
      get => detail ?? (detail = new());
      set => detail = value;
    }

    private DetailGroup detail;
  }
#endregion
}
