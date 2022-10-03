// Program: FN_DETERMINE_URA_FOR_SUPP_PRSN, ID: 374486314, model: 746.
// Short name: SWE02402
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_URA_FOR_SUPP_PRSN.
/// </summary>
[Serializable]
public partial class FnDetermineUraForSuppPrsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_URA_FOR_SUPP_PRSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineUraForSuppPrsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineUraForSuppPrsn.
  /// </summary>
  public FnDetermineUraForSuppPrsn(IContext context, Import import,
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
    // : Extract the year and month into separate attributes.
    local.Collection.Year = Year(import.Collection.Date);
    local.Collection.Month = Month(import.Collection.Date);

    // : Get the Legal Actions for the Supported Person.
    for(import.Legal.Index = 0; import.Legal.Index < import.Legal.Count; ++
      import.Legal.Index)
    {
      if (Equal(import.SuppPrsn.Number, import.Legal.Item.LegalSuppPrsn1.Number))
        
      {
        local.Legal.Index = 0;
        local.Legal.Clear();

        for(import.Legal.Item.LegalDtl.Index = 0; import
          .Legal.Item.LegalDtl.Index < import.Legal.Item.LegalDtl.Count; ++
          import.Legal.Item.LegalDtl.Index)
        {
          if (local.Legal.IsFull)
          {
            break;
          }

          if (!IsEmpty(import.LegalAction.StandardNumber))
          {
            if (!Equal(import.LegalAction.StandardNumber,
              import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber))
            {
              local.Legal.Next();

              continue;
            }
          }

          local.Legal.Update.Legal1.StandardNumber =
            import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber;
          local.Legal.Next();
        }

        break;
      }
    }

    // : If there are no Legal Actions, then use ONLY the Supported Person that 
    // was passed in.
    if (local.Legal.IsEmpty)
    {
      // : Use only the Supported Person passed in.
      local.LegalSuppPrsn.Index = 0;
      local.LegalSuppPrsn.CheckSize();

      local.LegalSuppPrsn.Update.LegalSuppPrsn1.Number = import.SuppPrsn.Number;
    }
    else
    {
      // : Find all of the Supported Person's associated to the Legal Actions.
      for(local.Legal.Index = 0; local.Legal.Index < local.Legal.Count; ++
        local.Legal.Index)
      {
        for(import.Legal.Index = 0; import.Legal.Index < import.Legal.Count; ++
          import.Legal.Index)
        {
          for(import.Legal.Item.LegalDtl.Index = 0; import
            .Legal.Item.LegalDtl.Index < import.Legal.Item.LegalDtl.Count; ++
            import.Legal.Item.LegalDtl.Index)
          {
            if (Equal(local.Legal.Item.Legal1.StandardNumber,
              import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber))
            {
              if (local.LegalSuppPrsn.IsEmpty)
              {
                local.LegalSuppPrsn.Index = 0;
                local.LegalSuppPrsn.CheckSize();
              }
              else
              {
                for(local.LegalSuppPrsn.Index = 0; local.LegalSuppPrsn.Index < local
                  .LegalSuppPrsn.Count; ++local.LegalSuppPrsn.Index)
                {
                  if (!local.LegalSuppPrsn.CheckSize())
                  {
                    break;
                  }

                  if (Equal(local.LegalSuppPrsn.Item.LegalSuppPrsn1.Number,
                    import.Legal.Item.LegalSuppPrsn1.Number))
                  {
                    goto Next1;
                  }
                }

                local.LegalSuppPrsn.CheckIndex();

                local.LegalSuppPrsn.Index = local.LegalSuppPrsn.Count;
                local.LegalSuppPrsn.CheckSize();
              }

              local.LegalSuppPrsn.Update.LegalSuppPrsn1.Number =
                import.Legal.Item.LegalSuppPrsn1.Number;

              goto Next1;
            }
          }

Next1:
          ;
        }
      }
    }

    // : Get each AE Case No where each of the Legal Action related Supported 
    // Person's are either a Child "CH" or Primary Individual "PI" in the
    // Household.
    for(local.LegalSuppPrsn.Index = 0; local.LegalSuppPrsn.Index < local
      .LegalSuppPrsn.Count; ++local.LegalSuppPrsn.Index)
    {
      if (!local.LegalSuppPrsn.CheckSize())
      {
        break;
      }

      for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
        import.HhHist.Index)
      {
        if (!Equal(import.HhHist.Item.HhHistSuppPrsn.Number,
          local.LegalSuppPrsn.Item.LegalSuppPrsn1.Number))
        {
          continue;
        }

        for(import.HhHist.Item.HhHistDtl.Index = 0; import
          .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
          import.HhHist.Item.HhHistDtl.Index)
        {
          if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year > local.Collection.Year)
          {
            continue;
          }
          else if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year == local.Collection.Year && import
            .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
              Month > local.Collection.Month)
          {
            continue;
          }

          if (Equal(import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Relationship, "CH") || Equal
            (import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
              Relationship, "PI"))
          {
            if (local.Hh.IsEmpty)
            {
              local.Hh.Index = 0;
              local.Hh.CheckSize();
            }
            else
            {
              for(local.Hh.Index = 0; local.Hh.Index < local.Hh.Count; ++
                local.Hh.Index)
              {
                if (!local.Hh.CheckSize())
                {
                  break;
                }

                if (Equal(import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHousehold.AeCaseNo, local.Hh.Item.Hh1.AeCaseNo))
                {
                  goto Next2;
                }
              }

              local.Hh.CheckIndex();

              local.Hh.Index = local.Hh.Count;
              local.Hh.CheckSize();
            }

            local.Hh.Update.Hh1.AeCaseNo =
              import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo;
          }
          else
          {
            // : Add in the URA amounts for the Legal Action related Supported 
            // Persons where those Supported Person's are NOT a Child "CH" and
            // NOT aPrimary Individual "PI" in the Household.
            export.UraAmount.TotalCurrency += import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
            export.UraMedicalAmount.TotalCurrency += import.HhHist.Item.
              HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                GetValueOrDefault();

            if (AsChar(import.UraExistsForTypeOnly.Text1) == 'A' && export
              .UraAmount.TotalCurrency > 0)
            {
              return;
            }

            if (AsChar(import.UraExistsForTypeOnly.Text1) == 'M' && export
              .UraMedicalAmount.TotalCurrency > 0)
            {
              return;
            }
          }

Next2:
          ;
        }
      }
    }

    local.LegalSuppPrsn.CheckIndex();

    if (local.Hh.IsEmpty)
    {
      return;
    }

    // : Process the Household level.  Add all URA amounts for each Household/
    // Year/Month.
    for(local.Hh.Index = 0; local.Hh.Index < local.Hh.Count; ++local.Hh.Index)
    {
      if (!local.Hh.CheckSize())
      {
        break;
      }

      for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
        import.HhHist.Index)
      {
        for(import.HhHist.Item.HhHistDtl.Index = 0; import
          .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
          import.HhHist.Item.HhHistDtl.Index)
        {
          if (Equal(import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.
            AeCaseNo, local.Hh.Item.Hh1.AeCaseNo))
          {
            export.UraAmount.TotalCurrency += import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
            export.UraMedicalAmount.TotalCurrency += import.HhHist.Item.
              HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                GetValueOrDefault();

            if (AsChar(import.UraExistsForTypeOnly.Text1) == 'A' && export
              .UraAmount.TotalCurrency > 0)
            {
              return;
            }

            if (AsChar(import.UraExistsForTypeOnly.Text1) == 'M' && export
              .UraMedicalAmount.TotalCurrency > 0)
            {
              return;
            }
          }
        }
      }
    }

    local.Hh.CheckIndex();
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
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn1.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn1")]
      public CsePerson LegalSuppPrsn1
      {
        get => legalSuppPrsn1 ??= new();
        set => legalSuppPrsn1 = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn1;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of SuppPrsn.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    public CsePerson SuppPrsn
    {
      get => suppPrsn ??= new();
      set => suppPrsn = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

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
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    /// <summary>
    /// A value of UraExistsForTypeOnly.
    /// </summary>
    [JsonPropertyName("uraExistsForTypeOnly")]
    public TextWorkArea UraExistsForTypeOnly
    {
      get => uraExistsForTypeOnly ??= new();
      set => uraExistsForTypeOnly = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CashReceiptDetail DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private CsePerson suppPrsn;
    private DateWorkArea collection;
    private LegalAction legalAction;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private TextWorkArea uraExistsForTypeOnly;
    private CashReceiptDetail delMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of UraAmount.
    /// </summary>
    [JsonPropertyName("uraAmount")]
    public Common UraAmount
    {
      get => uraAmount ??= new();
      set => uraAmount = value;
    }

    /// <summary>
    /// A value of UraMedicalAmount.
    /// </summary>
    [JsonPropertyName("uraMedicalAmount")]
    public Common UraMedicalAmount
    {
      get => uraMedicalAmount ??= new();
      set => uraMedicalAmount = value;
    }

    private Common uraAmount;
    private Common uraMedicalAmount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of Legal1.
      /// </summary>
      [JsonPropertyName("legal1")]
      public LegalAction Legal1
      {
        get => legal1 ??= new();
        set => legal1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legal1;
    }

    /// <summary>A LegalSuppPrsnGroup group.</summary>
    [Serializable]
    public class LegalSuppPrsnGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn1.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn1")]
      public CsePerson LegalSuppPrsn1
      {
        get => legalSuppPrsn1 ??= new();
        set => legalSuppPrsn1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson legalSuppPrsn1;
    }

    /// <summary>A HhGroup group.</summary>
    [Serializable]
    public class HhGroup
    {
      /// <summary>
      /// A value of Hh1.
      /// </summary>
      [JsonPropertyName("hh1")]
      public ImHousehold Hh1
      {
        get => hh1 ??= new();
        set => hh1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2500;

      private ImHousehold hh1;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of LegalSuppPrsn.
    /// </summary>
    [JsonIgnore]
    public Array<LegalSuppPrsnGroup> LegalSuppPrsn => legalSuppPrsn ??= new(
      LegalSuppPrsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalSuppPrsn for json serialization.
    /// </summary>
    [JsonPropertyName("legalSuppPrsn")]
    [Computed]
    public IList<LegalSuppPrsnGroup> LegalSuppPrsn_Json
    {
      get => legalSuppPrsn;
      set => LegalSuppPrsn.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hh.
    /// </summary>
    [JsonIgnore]
    public Array<HhGroup> Hh => hh ??= new(HhGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hh for json serialization.
    /// </summary>
    [JsonPropertyName("hh")]
    [Computed]
    public IList<HhGroup> Hh_Json
    {
      get => hh;
      set => Hh.Assign(value);
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public ImHouseholdMbrMnthlySum DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private DateWorkArea collection;
    private Array<LegalGroup> legal;
    private Array<LegalSuppPrsnGroup> legalSuppPrsn;
    private Array<HhGroup> hh;
    private ImHouseholdMbrMnthlySum delMe;
  }
#endregion
}
