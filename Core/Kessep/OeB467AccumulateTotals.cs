// Program: OE_B467_ACCUMULATE_TOTALS, ID: 374472286, model: 746.
// Short name: SWE02710
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B467_ACCUMULATE_TOTALS.
/// </summary>
[Serializable]
public partial class OeB467AccumulateTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B467_ACCUMULATE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB467AccumulateTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB467AccumulateTotals.
  /// </summary>
  public OeB467AccumulateTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.TotCountsAndAmounts.TotNbrOfDebtDtlsRead.Count += import.
      CountsAndAmounts.NbrOfDebtDtlsRead.Count;
    export.TotCountsAndAmounts.TotNbrOfDebtDtlsUpdated.Count += import.
      CountsAndAmounts.NbrOfDebtDtlsUpdated.Count;
    export.TotCountsAndAmounts.TotNbrOfFcDebtDtls.Count += import.
      CountsAndAmounts.NbrOfFcDebtDtls.Count;
    export.TotCountsAndAmounts.TotNbrOfNonFcDebtDtls.Count += import.
      CountsAndAmounts.NbrOfNonFcDebtDtls.Count;
    export.TotCountsAndAmounts.TotNbrOfImHhCreated.Count += import.
      CountsAndAmounts.NbrOfImHhCreated.Count;
    export.TotCountsAndAmounts.TotNbrOfMoUrasCreated.Count += import.
      CountsAndAmounts.NbrOfMoUrasCreated.Count;
    export.TotCountsAndAmounts.TotNbrOfMoUrasUpdated.Count += import.
      CountsAndAmounts.NbrOfMoUrasUpdated.Count;
    export.TotCountsAndAmounts.TotNbrOfErrors.Count += import.CountsAndAmounts.
      NbrOfErrors.Count;
    export.TotCountsAndAmounts.TotAmtOfFcDebtDtlsRead.TotalCurrency += import.
      CountsAndAmounts.AmtOfFcDebtDtlsRead.TotalCurrency;
    export.TotCountsAndAmounts.TotAmtOfMoUrasCreated.TotalCurrency += import.
      CountsAndAmounts.AmtOfMoUrasCreated.TotalCurrency;
    export.TotCountsAndAmounts.TotAmtOfMoUrasUpdated.TotalCurrency += import.
      CountsAndAmounts.AmtOfMoUrasUpdated.TotalCurrency;
    export.TotCountsAndAmounts.TotAmtOfErrors.TotalCurrency += import.
      CountsAndAmounts.AmtOfErrors.TotalCurrency;
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
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsRead")]
      public Common NbrOfDebtDtlsRead
      {
        get => nbrOfDebtDtlsRead ??= new();
        set => nbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of NbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsUpdated")]
      public Common NbrOfDebtDtlsUpdated
      {
        get => nbrOfDebtDtlsUpdated ??= new();
        set => nbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfFcDebtDtls")]
      public Common NbrOfFcDebtDtls
      {
        get => nbrOfFcDebtDtls ??= new();
        set => nbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfNonFcDebtDtls")]
      public Common NbrOfNonFcDebtDtls
      {
        get => nbrOfNonFcDebtDtls ??= new();
        set => nbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("nbrOfImHhCreated")]
      public Common NbrOfImHhCreated
      {
        get => nbrOfImHhCreated ??= new();
        set => nbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasCreated")]
      public Common NbrOfMoUrasCreated
      {
        get => nbrOfMoUrasCreated ??= new();
        set => nbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasUpdated")]
      public Common NbrOfMoUrasUpdated
      {
        get => nbrOfMoUrasUpdated ??= new();
        set => nbrOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of AmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("amtOfFcDebtDtlsRead")]
      public Common AmtOfFcDebtDtlsRead
      {
        get => amtOfFcDebtDtlsRead ??= new();
        set => amtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasCreated")]
      public Common AmtOfMoUrasCreated
      {
        get => amtOfMoUrasCreated ??= new();
        set => amtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasUpdated")]
      public Common AmtOfMoUrasUpdated
      {
        get => amtOfMoUrasUpdated ??= new();
        set => amtOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of AmtOfErrors.
      /// </summary>
      [JsonPropertyName("amtOfErrors")]
      public Common AmtOfErrors
      {
        get => amtOfErrors ??= new();
        set => amtOfErrors = value;
      }

      private Common nbrOfDebtDtlsRead;
      private Common nbrOfDebtDtlsUpdated;
      private Common nbrOfFcDebtDtls;
      private Common nbrOfNonFcDebtDtls;
      private Common nbrOfImHhCreated;
      private Common nbrOfMoUrasCreated;
      private Common nbrOfMoUrasUpdated;
      private Common nbrOfErrors;
      private Common amtOfFcDebtDtlsRead;
      private Common amtOfMoUrasCreated;
      private Common amtOfMoUrasUpdated;
      private Common amtOfErrors;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    private CountsAndAmountsGroup countsAndAmounts;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsRead")]
      public Common NbrOfDebtDtlsRead
      {
        get => nbrOfDebtDtlsRead ??= new();
        set => nbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of NbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsUpdated")]
      public Common NbrOfDebtDtlsUpdated
      {
        get => nbrOfDebtDtlsUpdated ??= new();
        set => nbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfFcDebtDtls")]
      public Common NbrOfFcDebtDtls
      {
        get => nbrOfFcDebtDtls ??= new();
        set => nbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfNonFcDebtDtls")]
      public Common NbrOfNonFcDebtDtls
      {
        get => nbrOfNonFcDebtDtls ??= new();
        set => nbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("nbrOfImHhCreated")]
      public Common NbrOfImHhCreated
      {
        get => nbrOfImHhCreated ??= new();
        set => nbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasCreated")]
      public Common NbrOfMoUrasCreated
      {
        get => nbrOfMoUrasCreated ??= new();
        set => nbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasUpdated")]
      public Common NbrOfMoUrasUpdated
      {
        get => nbrOfMoUrasUpdated ??= new();
        set => nbrOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of AmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("amtOfFcDebtDtlsRead")]
      public Common AmtOfFcDebtDtlsRead
      {
        get => amtOfFcDebtDtlsRead ??= new();
        set => amtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasCreated")]
      public Common AmtOfMoUrasCreated
      {
        get => amtOfMoUrasCreated ??= new();
        set => amtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasUpdated")]
      public Common AmtOfMoUrasUpdated
      {
        get => amtOfMoUrasUpdated ??= new();
        set => amtOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of AmtOfErrors.
      /// </summary>
      [JsonPropertyName("amtOfErrors")]
      public Common AmtOfErrors
      {
        get => amtOfErrors ??= new();
        set => amtOfErrors = value;
      }

      private Common nbrOfDebtDtlsRead;
      private Common nbrOfDebtDtlsUpdated;
      private Common nbrOfFcDebtDtls;
      private Common nbrOfNonFcDebtDtls;
      private Common nbrOfImHhCreated;
      private Common nbrOfMoUrasCreated;
      private Common nbrOfMoUrasUpdated;
      private Common nbrOfErrors;
      private Common amtOfFcDebtDtlsRead;
      private Common amtOfMoUrasCreated;
      private Common amtOfMoUrasUpdated;
      private Common amtOfErrors;
    }

    /// <summary>A TotCountsAndAmountsGroup group.</summary>
    [Serializable]
    public class TotCountsAndAmountsGroup
    {
      /// <summary>
      /// A value of TotNbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("totNbrOfDebtDtlsRead")]
      public Common TotNbrOfDebtDtlsRead
      {
        get => totNbrOfDebtDtlsRead ??= new();
        set => totNbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of TotNbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("totNbrOfDebtDtlsUpdated")]
      public Common TotNbrOfDebtDtlsUpdated
      {
        get => totNbrOfDebtDtlsUpdated ??= new();
        set => totNbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of TotNbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("totNbrOfFcDebtDtls")]
      public Common TotNbrOfFcDebtDtls
      {
        get => totNbrOfFcDebtDtls ??= new();
        set => totNbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of TotNbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("totNbrOfNonFcDebtDtls")]
      public Common TotNbrOfNonFcDebtDtls
      {
        get => totNbrOfNonFcDebtDtls ??= new();
        set => totNbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of TotNbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfImHhCreated")]
      public Common TotNbrOfImHhCreated
      {
        get => totNbrOfImHhCreated ??= new();
        set => totNbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfMoUrasCreated")]
      public Common TotNbrOfMoUrasCreated
      {
        get => totNbrOfMoUrasCreated ??= new();
        set => totNbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("totNbrOfMoUrasUpdated")]
      public Common TotNbrOfMoUrasUpdated
      {
        get => totNbrOfMoUrasUpdated ??= new();
        set => totNbrOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of TotNbrOfErrors.
      /// </summary>
      [JsonPropertyName("totNbrOfErrors")]
      public Common TotNbrOfErrors
      {
        get => totNbrOfErrors ??= new();
        set => totNbrOfErrors = value;
      }

      /// <summary>
      /// A value of TotAmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("totAmtOfFcDebtDtlsRead")]
      public Common TotAmtOfFcDebtDtlsRead
      {
        get => totAmtOfFcDebtDtlsRead ??= new();
        set => totAmtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of TotAmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("totAmtOfMoUrasCreated")]
      public Common TotAmtOfMoUrasCreated
      {
        get => totAmtOfMoUrasCreated ??= new();
        set => totAmtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of TotAmtOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("totAmtOfMoUrasUpdated")]
      public Common TotAmtOfMoUrasUpdated
      {
        get => totAmtOfMoUrasUpdated ??= new();
        set => totAmtOfMoUrasUpdated = value;
      }

      /// <summary>
      /// A value of TotAmtOfErrors.
      /// </summary>
      [JsonPropertyName("totAmtOfErrors")]
      public Common TotAmtOfErrors
      {
        get => totAmtOfErrors ??= new();
        set => totAmtOfErrors = value;
      }

      private Common totNbrOfDebtDtlsRead;
      private Common totNbrOfDebtDtlsUpdated;
      private Common totNbrOfFcDebtDtls;
      private Common totNbrOfNonFcDebtDtls;
      private Common totNbrOfImHhCreated;
      private Common totNbrOfMoUrasCreated;
      private Common totNbrOfMoUrasUpdated;
      private Common totNbrOfErrors;
      private Common totAmtOfFcDebtDtlsRead;
      private Common totAmtOfMoUrasCreated;
      private Common totAmtOfMoUrasUpdated;
      private Common totAmtOfErrors;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    /// <summary>
    /// Gets a value of TotCountsAndAmounts.
    /// </summary>
    [JsonPropertyName("totCountsAndAmounts")]
    public TotCountsAndAmountsGroup TotCountsAndAmounts
    {
      get => totCountsAndAmounts ?? (totCountsAndAmounts = new());
      set => totCountsAndAmounts = value;
    }

    private CountsAndAmountsGroup countsAndAmounts;
    private TotCountsAndAmountsGroup totCountsAndAmounts;
  }
#endregion
}
