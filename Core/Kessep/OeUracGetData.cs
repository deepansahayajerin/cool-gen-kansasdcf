// Program: OE_URAC_GET_DATA, ID: 374461089, model: 746.
// Short name: SWE02705
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAC_GET_DATA.
/// </summary>
[Serializable]
public partial class OeUracGetData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAC_GET_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUracGetData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUracGetData.
  /// </summary>
  public OeUracGetData(IContext context, Import import, Export export):
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
    // Fangman      06-03-00  Initial code.
    // Fangman      08-07-00  Changed medical code from 11 to 19.
    // ******** END MAINTENANCE LOG ****************
    local.FirstTimeThru.Flag = "Y";

    foreach(var item in ReadImHouseholdMbrMnthlySumCsePerson())
    {
      if (AsChar(local.FirstTimeThru.Flag) == 'Y')
      {
        local.FirstTimeThru.Flag = "N";

        export.Detail.Index = 0;
        export.Detail.CheckSize();

        MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum, local.Hold);
          
        export.Detail.Update.DtlDateWorkAttributes.TextMonthYear =
          NumberToString(entities.ImHouseholdMbrMnthlySum.Month, 14, 2) + NumberToString
          (entities.ImHouseholdMbrMnthlySum.Year, 12, 4);
        export.Detail.Update.DtlGrpLabel1.Text8 = "AF/FC:";
        export.Detail.Update.DtlGrpLabel2.Text8 = "Medical:";
      }
      else if (entities.ImHouseholdMbrMnthlySum.Year != local.Hold.Year || entities
        .ImHouseholdMbrMnthlySum.Month != local.Hold.Month)
      {
        export.Total.Tot.GrantAmount =
          export.Total.Tot.GrantAmount.GetValueOrDefault() + export
          .Detail.Item.DtlImHouseholdMbrMnthlySum.GrantAmount.
            GetValueOrDefault();
        export.Total.Tot.GrantMedicalAmount =
          export.Total.Tot.GrantMedicalAmount.GetValueOrDefault() + export
          .Detail.Item.DtlImHouseholdMbrMnthlySum.GrantMedicalAmount.
            GetValueOrDefault();
        export.Total.Tot.UraAmount =
          export.Total.Tot.UraAmount.GetValueOrDefault() + export
          .Detail.Item.DtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
          
        export.Total.Tot.UraMedicalAmount =
          export.Total.Tot.UraMedicalAmount.GetValueOrDefault() + export
          .Detail.Item.DtlImHouseholdMbrMnthlySum.UraMedicalAmount.
            GetValueOrDefault();
        export.Total.TotGrpAfFcColl.CollectionAmountApplied += export.Detail.
          Item.DtlGrpAfFcColl.CollectionAmountApplied;
        export.Total.TotGrpMedicalColl.CollectionAmountApplied += export.Detail.
          Item.DtlGrpMedicalColl.CollectionAmountApplied;
        export.Total.TotGrpSsColl.CollectionAmountApplied += export.Detail.Item.
          DtlGrpSsColl.CollectionAmountApplied;
        export.Total.TotGrpAfFc.AdjustmentAmount += export.Detail.Item.
          DtlGrpAfFc.AdjustmentAmount;
        export.Total.TotGrpMedical.AdjustmentAmount += export.Detail.Item.
          DtlGrpMedical.AdjustmentAmount;

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

        MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum, local.Hold);
          
        export.Detail.Update.DtlDateWorkAttributes.TextMonthYear =
          NumberToString(entities.ImHouseholdMbrMnthlySum.Month, 14, 2) + NumberToString
          (entities.ImHouseholdMbrMnthlySum.Year, 12, 4);
        export.Detail.Update.DtlGrpLabel1.Text8 = "AF/FC:";
        export.Detail.Update.DtlGrpLabel2.Text8 = "Medical:";
      }

      export.Detail.Update.DtlImHouseholdMbrMnthlySum.GrantAmount =
        export.Detail.Item.DtlImHouseholdMbrMnthlySum.GrantAmount.
          GetValueOrDefault() + entities
        .ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
      export.Detail.Update.DtlImHouseholdMbrMnthlySum.GrantMedicalAmount =
        export.Detail.Item.DtlImHouseholdMbrMnthlySum.GrantMedicalAmount.
          GetValueOrDefault() + entities
        .ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
      export.Detail.Update.DtlImHouseholdMbrMnthlySum.UraAmount =
        export.Detail.Item.DtlImHouseholdMbrMnthlySum.UraAmount.
          GetValueOrDefault() + entities
        .ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
      export.Detail.Update.DtlImHouseholdMbrMnthlySum.UraMedicalAmount =
        export.Detail.Item.DtlImHouseholdMbrMnthlySum.UraMedicalAmount.
          GetValueOrDefault() + entities
        .ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault();

      foreach(var item1 in ReadImHouseholdMbrMnthlyAdj())
      {
        if (AsChar(entities.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          export.Detail.Update.DtlGrpAfFc.AdjustmentAmount =
            export.Detail.Item.DtlGrpAfFc.AdjustmentAmount + entities
            .ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
        }
        else
        {
          export.Detail.Update.DtlGrpMedical.AdjustmentAmount =
            export.Detail.Item.DtlGrpMedical.AdjustmentAmount + entities
            .ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
        }
      }

      foreach(var item1 in ReadUraCollectionApplicationObligationType())
      {
        if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 10 || entities
          .ObligationType.SystemGeneratedIdentifier == 19)
        {
          export.Detail.Update.DtlGrpMedicalColl.CollectionAmountApplied =
            export.Detail.Item.DtlGrpMedicalColl.CollectionAmountApplied + entities
            .UraCollectionApplication.CollectionAmountApplied;
        }
        else if (entities.ObligationType.SystemGeneratedIdentifier == 2 || entities
          .ObligationType.SystemGeneratedIdentifier == 17)
        {
          export.Detail.Update.DtlGrpSsColl.CollectionAmountApplied =
            export.Detail.Item.DtlGrpSsColl.CollectionAmountApplied + entities
            .UraCollectionApplication.CollectionAmountApplied;
        }
        else
        {
          export.Detail.Update.DtlGrpAfFcColl.CollectionAmountApplied =
            export.Detail.Item.DtlGrpAfFcColl.CollectionAmountApplied + entities
            .UraCollectionApplication.CollectionAmountApplied;
        }
      }
    }

    if (export.Detail.Count > 0)
    {
      export.Total.Tot.GrantAmount =
        export.Total.Tot.GrantAmount.GetValueOrDefault() + export
        .Detail.Item.DtlImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
        
      export.Total.Tot.GrantMedicalAmount =
        export.Total.Tot.GrantMedicalAmount.GetValueOrDefault() + export
        .Detail.Item.DtlImHouseholdMbrMnthlySum.GrantMedicalAmount.
          GetValueOrDefault();
      export.Total.Tot.UraAmount =
        export.Total.Tot.UraAmount.GetValueOrDefault() + export
        .Detail.Item.DtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
      export.Total.Tot.UraMedicalAmount =
        export.Total.Tot.UraMedicalAmount.GetValueOrDefault() + export
        .Detail.Item.DtlImHouseholdMbrMnthlySum.UraMedicalAmount.
          GetValueOrDefault();
      export.Total.TotGrpAfFcColl.CollectionAmountApplied += export.Detail.Item.
        DtlGrpAfFcColl.CollectionAmountApplied;
      export.Total.TotGrpMedicalColl.CollectionAmountApplied += export.Detail.
        Item.DtlGrpMedicalColl.CollectionAmountApplied;
      export.Total.TotGrpSsColl.CollectionAmountApplied += export.Detail.Item.
        DtlGrpSsColl.CollectionAmountApplied;
      export.Total.TotGrpAfFc.AdjustmentAmount += export.Detail.Item.DtlGrpAfFc.
        AdjustmentAmount;
      export.Total.TotGrpMedical.AdjustmentAmount += export.Detail.Item.
        DtlGrpMedical.AdjustmentAmount;
    }
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlyAdj()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.ImHouseholdMbrMnthlyAdj.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlyAdj",
      (db, command) =>
      {
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlyAdj.Type1 = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount =
          db.GetDecimal(reader, 1);
        entities.ImHouseholdMbrMnthlyAdj.CreatedTmst =
          db.GetDateTime(reader, 2);
        entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlyAdj.CspNumber = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlyAdj.ImsMonth = db.GetInt32(reader, 5);
        entities.ImHouseholdMbrMnthlyAdj.ImsYear = db.GetInt32(reader, 6);
        entities.ImHouseholdMbrMnthlyAdj.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "year1", import.From.Year);
        db.SetInt32(command, "month1", import.From.Month);
        db.SetInt32(command, "year2", import.To.Year);
        db.SetInt32(command, "month2", import.To.Month);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 6);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 7);
        entities.CsePerson.Number = db.GetString(reader, 7);
        entities.CsePerson.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadUraCollectionApplicationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.UraCollectionApplication.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadUraCollectionApplicationObligationType",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber0", entities.ImHouseholdMbrMnthlySum.CspNumber);
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CollectionAmountApplied =
          db.GetDecimal(reader, 0);
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 1);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 2);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 3);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 4);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 5);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 6);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 7);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 9);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 10);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 11);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 12);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 13);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 14);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 15);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.UraCollectionApplication.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
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

    private CsePerson csePerson;
    private ImHouseholdMbrMnthlySum from;
    private ImHouseholdMbrMnthlySum to;
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
    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public ImHouseholdMbrMnthlySum Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DebtDetail Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private ImHouseholdMbrMnthlySum hold;
    private Common firstTimeThru;
    private DebtDetail initialized;
    private Common work;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private CsePerson csePerson;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private UraCollectionApplication uraCollectionApplication;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount supported;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
  }
#endregion
}
