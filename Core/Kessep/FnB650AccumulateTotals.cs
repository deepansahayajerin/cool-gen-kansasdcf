// Program: FN_B650_ACCUMULATE_TOTALS, ID: 372896042, model: 746.
// Short name: SWE02493
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B650_ACCUMULATE_TOTALS.
/// </summary>
[Serializable]
public partial class FnB650AccumulateTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_ACCUMULATE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650AccumulateTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650AccumulateTotals.
  /// </summary>
  public FnB650AccumulateTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // 10/22/99 - SWSRKXD PR#77874
    // NC collections are errored off by B650 and money is never
    // disbursed!  Disable code to accumulate record count and
    // total amount disbursed to State. This was only reqd for NC.
    // 11/12/99 - FANGMAN  PR 78745
    // Add new count for collection not fully applied (these will not be 
    // processed).
    // 09/12/00 - Fangman  103323
    // Added new counts & amounts for errors.  This was put in with the changes 
    // to fix the disb suppr with past discontinue dates.
    // 11/26/02 - Fangman  161935
    // Add new counts to break up the Cash counts into TAF and Non-TAF.
    // -----------------------------------------------------------------
    export.FinalTotals.FinalNbrOfCollRead.Count += export.TempTotals.
      TempNbrOfCollRead.Count;
    export.FinalTotals.FinalAmtOfCollRead.TotalCurrency += export.TempTotals.
      TempAmtOfCollRead.TotalCurrency;
    export.FinalTotals.FinalNbrOfAps.Count += export.TempTotals.TrmpNbrOfAps.
      Count;
    export.FinalTotals.FinalNbrOfCollBackedOff.Count += export.TempTotals.
      TempNbrOfCollBackedOff.Count;
    export.FinalTotals.FinalAmtOfCollBackedOff.TotalCurrency += export.
      TempTotals.TempAmtOfCollBackedOff.TotalCurrency;
    export.FinalTotals.FinalNbrOfErrorsCreated.Count += export.TempTotals.
      TempNbrOfErrorsCreated.Count;
    export.FinalTotals.FinalAmtOfErrorsCreated.TotalCurrency += export.
      TempTotals.TempAmtOfErrorsCreated.TotalCurrency;
    export.FinalTotals.FinalNbrOfCollNotFulAp.Count += export.TempTotals.
      TempNbrOfCollNotFulApl.Count;
    export.FinalTotals.FinalAmtOfCollNotFulAp.TotalCurrency += export.
      TempTotals.TempAmtOfCollNotFulApl.TotalCurrency;
    export.FinalTotals.FinalNbrOfCreditsCreated.Count += export.TempTotals.
      TempNbrOfCreditsCreated.Count;
    export.FinalTotals.FinalAmtOfCreditsCreated.TotalCurrency += export.
      TempTotals.TempAmtOfCreditsCreated.TotalCurrency;
    export.FinalTotals.FinalNbrOfApsWoColFlA.Count += export.TempTotals.
      TempNbrOfApsWoCollFlA.Count;

    // ----------  Accumulate new Cash Non-TAF error sub totals  ---------
    export.FinalTotals.FinalNbrCNaKsAr.Count += export.TempTotals.
      TempNbrCNaKsAr.Count;
    export.FinalTotals.FinalAmtCNaKsAr.TotalCurrency += export.TempTotals.
      TempAmtCNaKsAr.TotalCurrency;
    export.FinalTotals.FinalNbrCNaJjAr.Count += export.TempTotals.
      TempNbrCNaJjAr.Count;
    export.FinalTotals.FinalAmtCNaJjAr.TotalCurrency += export.TempTotals.
      TempAmtCNaJjAr.TotalCurrency;
    export.FinalTotals.FinalNbrCNaNotDeterm.Count += export.TempTotals.
      TempNbrCNaNotDeterm.Count;
    export.FinalTotals.FinalAmtCNaNotDeterm.TotalCurrency += export.TempTotals.
      TempAmtCNaNotDeterm.TotalCurrency;
    export.FinalTotals.FinalNbrCNaCaseNf.Count += export.TempTotals.
      TempNbrCNaCaseNf.Count;
    export.FinalTotals.FinalAmtCNaCaseNf.TotalCurrency += export.TempTotals.
      TempAmtCNaCaseNf.TotalCurrency;
    export.FinalTotals.FinalNbrCNaInterSt.Count += export.TempTotals.
      TempNbrCNaInterSt.Count;
    export.FinalTotals.FinalAmtCNaInterSt.TotalCurrency += export.TempTotals.
      TempAmtCNaInterSt.TotalCurrency;
    export.FinalTotals.FinalNbrCNaAllOther.Count += export.TempTotals.
      TempNbrCNaAllOther.Count;
    export.FinalTotals.FinalAmtCNaAllOther.TotalCurrency += export.TempTotals.
      TempAmtCNaAllOther.TotalCurrency;

    // ----------  Accumulate new Cash TAF error sub totals  ---------
    export.FinalTotals.FinalNbrCAfKsAr.Count += export.TempTotals.
      TempNbrCAfKsAr.Count;
    export.FinalTotals.FinalAmtCAfKsAr.TotalCurrency += export.TempTotals.
      TempAmtCAfKsAr.TotalCurrency;
    export.FinalTotals.FinalNbrCAfJjAr.Count += export.TempTotals.
      TempNbrCAfJjAr.Count;
    export.FinalTotals.FinalAmtCAfJjAr.TotalCurrency += export.TempTotals.
      TempAmtCAfJjAr.TotalCurrency;
    export.FinalTotals.FinalNbrCAfNotDeterm.Count += export.TempTotals.
      TempNbrCAfNotDeterm.Count;
    export.FinalTotals.FinalAmtCAfNotDeterm.TotalCurrency += export.TempTotals.
      TempAmtCAfNotDeterm.TotalCurrency;
    export.FinalTotals.FinalNbrCAfCaseNf.Count += export.TempTotals.
      TempNbrCAfCaseNf.Count;
    export.FinalTotals.FinalAmtCAfCaseNf.TotalCurrency += export.TempTotals.
      TempAmtCAfCaseNf.TotalCurrency;
    export.FinalTotals.FinalNbrCAfInterSt.Count += export.TempTotals.
      TempNbrCAfInterSt.Count;
    export.FinalTotals.FinalAmtCAfInterSt.TotalCurrency += export.TempTotals.
      TempAmtCAfInterSt.TotalCurrency;
    export.FinalTotals.FinalNbrCAfAllOther.Count += export.TempTotals.
      TempNbrCAfAllOther.Count;
    export.FinalTotals.FinalAmtCAfAllOther.TotalCurrency += export.TempTotals.
      TempAmtCAfAllOther.TotalCurrency;

    // ----------  Accumulate new Non-Cash error sub totals  ---------
    export.FinalTotals.FinalNbrNKsAr.Count += export.TempTotals.TempNbrNKsAr.
      Count;
    export.FinalTotals.FinalAmtNKsAr.TotalCurrency += export.TempTotals.
      TempAmtNKsAr.TotalCurrency;
    export.FinalTotals.FinalNbrNJjAr.Count += export.TempTotals.TempNbrNJjAr.
      Count;
    export.FinalTotals.FinalAmtNJjAr.TotalCurrency += export.TempTotals.
      TempAmtNJjAr.TotalCurrency;
    export.FinalTotals.FinalNbrNNotDeterm.Count += export.TempTotals.
      TempNbrNNotDeterm.Count;
    export.FinalTotals.FinalAmtNNotDeterm.TotalCurrency += export.TempTotals.
      TempAmtNNotDeterm.TotalCurrency;
    export.FinalTotals.FinalNbrNCaseNf.Count += export.TempTotals.
      TempNbrNCaseNf.Count;
    export.FinalTotals.FinalAmtNCaseNf.TotalCurrency += export.TempTotals.
      TempAmtNCaseNf.TotalCurrency;
    export.FinalTotals.FinalNbrNInterSt.Count += export.TempTotals.
      TempNbrNInterSt.Count;
    export.FinalTotals.FinalAmtNInterSt.TotalCurrency += export.TempTotals.
      TempAmtNInterSt.TotalCurrency;
    export.FinalTotals.FinalNbrNAllOther.Count += export.TempTotals.
      TempNbrNAllOther.Count;
    export.FinalTotals.FinalAmtNAllOther.TotalCurrency += export.TempTotals.
      TempAmtNAllOther.TotalCurrency;
    export.TempTotals.TempNbrOfCollRead.Count = 0;
    export.TempTotals.TempAmtOfCollRead.TotalCurrency = 0;
    export.TempTotals.TrmpNbrOfAps.Count = 0;
    export.TempTotals.TempNbrOfCollBackedOff.Count = 0;
    export.TempTotals.TempAmtOfCollBackedOff.TotalCurrency = 0;
    export.TempTotals.TempNbrOfErrorsCreated.Count = 0;
    export.TempTotals.TempAmtOfErrorsCreated.TotalCurrency = 0;
    export.TempTotals.TempNbrOfCollNotFulApl.Count = 0;
    export.TempTotals.TempAmtOfCollNotFulApl.TotalCurrency = 0;
    export.TempTotals.TempNbrOfCreditsCreated.Count = 0;
    export.TempTotals.TempAmtOfCreditsCreated.TotalCurrency = 0;
    export.TempTotals.TempNbrOfApsWoCollFlA.Count = 0;

    // ----------  Initialize new Cash Non-TAF error sub totals  ----------
    export.TempTotals.TempNbrCNaKsAr.Count = 0;
    export.TempTotals.TempAmtCNaKsAr.TotalCurrency = 0;
    export.TempTotals.TempNbrCNaJjAr.Count = 0;
    export.TempTotals.TempAmtCNaJjAr.TotalCurrency = 0;
    export.TempTotals.TempNbrCNaNotDeterm.Count = 0;
    export.TempTotals.TempAmtCNaNotDeterm.TotalCurrency = 0;
    export.TempTotals.TempNbrCNaCaseNf.Count = 0;
    export.TempTotals.TempAmtCNaCaseNf.TotalCurrency = 0;
    export.TempTotals.TempNbrCNaInterSt.Count = 0;
    export.TempTotals.TempAmtCNaInterSt.TotalCurrency = 0;
    export.TempTotals.TempNbrCNaAllOther.Count = 0;
    export.TempTotals.TempAmtCNaAllOther.TotalCurrency = 0;

    // ----------  Initialize new Cash TAF error sub totals  ----------
    export.TempTotals.TempNbrCAfKsAr.Count = 0;
    export.TempTotals.TempAmtCAfKsAr.TotalCurrency = 0;
    export.TempTotals.TempNbrCAfJjAr.Count = 0;
    export.TempTotals.TempAmtCAfJjAr.TotalCurrency = 0;
    export.TempTotals.TempNbrCAfNotDeterm.Count = 0;
    export.TempTotals.TempAmtCAfNotDeterm.TotalCurrency = 0;
    export.TempTotals.TempNbrCAfCaseNf.Count = 0;
    export.TempTotals.TempAmtCAfCaseNf.TotalCurrency = 0;
    export.TempTotals.TempNbrCAfInterSt.Count = 0;
    export.TempTotals.TempAmtCAfInterSt.TotalCurrency = 0;
    export.TempTotals.TempNbrCAfAllOther.Count = 0;
    export.TempTotals.TempAmtCAfAllOther.TotalCurrency = 0;

    // ----------  Initialize new Non-Cash error sub totals  ----------
    export.TempTotals.TempNbrNKsAr.Count = 0;
    export.TempTotals.TempAmtNKsAr.TotalCurrency = 0;
    export.TempTotals.TempNbrNJjAr.Count = 0;
    export.TempTotals.TempAmtNJjAr.TotalCurrency = 0;
    export.TempTotals.TempNbrNNotDeterm.Count = 0;
    export.TempTotals.TempAmtNNotDeterm.TotalCurrency = 0;
    export.TempTotals.TempNbrNCaseNf.Count = 0;
    export.TempTotals.TempAmtNCaseNf.TotalCurrency = 0;
    export.TempTotals.TempNbrNInterSt.Count = 0;
    export.TempTotals.TempAmtNInterSt.TotalCurrency = 0;
    export.TempTotals.TempNbrNAllOther.Count = 0;
    export.TempTotals.TempAmtNAllOther.TotalCurrency = 0;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A TempTotalsGroup group.</summary>
    [Serializable]
    public class TempTotalsGroup
    {
      /// <summary>
      /// A value of TempNbrOfCollRead.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollRead")]
      public Common TempNbrOfCollRead
      {
        get => tempNbrOfCollRead ??= new();
        set => tempNbrOfCollRead = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollRead.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollRead")]
      public Common TempAmtOfCollRead
      {
        get => tempAmtOfCollRead ??= new();
        set => tempAmtOfCollRead = value;
      }

      /// <summary>
      /// A value of TrmpNbrOfAps.
      /// </summary>
      [JsonPropertyName("trmpNbrOfAps")]
      public Common TrmpNbrOfAps
      {
        get => trmpNbrOfAps ??= new();
        set => trmpNbrOfAps = value;
      }

      /// <summary>
      /// A value of TempNbrOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollBackedOff")]
      public Common TempNbrOfCollBackedOff
      {
        get => tempNbrOfCollBackedOff ??= new();
        set => tempNbrOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollBackedOff")]
      public Common TempAmtOfCollBackedOff
      {
        get => tempAmtOfCollBackedOff ??= new();
        set => tempAmtOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of TempNbrOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("tempNbrOfErrorsCreated")]
      public Common TempNbrOfErrorsCreated
      {
        get => tempNbrOfErrorsCreated ??= new();
        set => tempNbrOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of TempAmtOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("tempAmtOfErrorsCreated")]
      public Common TempAmtOfErrorsCreated
      {
        get => tempAmtOfErrorsCreated ??= new();
        set => tempAmtOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of TempNbrOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollNotFulApl")]
      public Common TempNbrOfCollNotFulApl
      {
        get => tempNbrOfCollNotFulApl ??= new();
        set => tempNbrOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollNotFulApl")]
      public Common TempAmtOfCollNotFulApl
      {
        get => tempAmtOfCollNotFulApl ??= new();
        set => tempAmtOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of TempNbrOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("tempNbrOfCreditsCreated")]
      public Common TempNbrOfCreditsCreated
      {
        get => tempNbrOfCreditsCreated ??= new();
        set => tempNbrOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of TempAmtOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("tempAmtOfCreditsCreated")]
      public Common TempAmtOfCreditsCreated
      {
        get => tempAmtOfCreditsCreated ??= new();
        set => tempAmtOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of TempNbrOfApsWoCollFlA.
      /// </summary>
      [JsonPropertyName("tempNbrOfApsWoCollFlA")]
      public Common TempNbrOfApsWoCollFlA
      {
        get => tempNbrOfApsWoCollFlA ??= new();
        set => tempNbrOfApsWoCollFlA = value;
      }

      /// <summary>
      /// A value of TempNbrCNaKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrCNaKsAr")]
      public Common TempNbrCNaKsAr
      {
        get => tempNbrCNaKsAr ??= new();
        set => tempNbrCNaKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtCNaKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtCNaKsAr")]
      public Common TempAmtCNaKsAr
      {
        get => tempAmtCNaKsAr ??= new();
        set => tempAmtCNaKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrCNaJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrCNaJjAr")]
      public Common TempNbrCNaJjAr
      {
        get => tempNbrCNaJjAr ??= new();
        set => tempNbrCNaJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtCNaJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtCNaJjAr")]
      public Common TempAmtCNaJjAr
      {
        get => tempAmtCNaJjAr ??= new();
        set => tempAmtCNaJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrCNaNotDeterm")]
      public Common TempNbrCNaNotDeterm
      {
        get => tempNbrCNaNotDeterm ??= new();
        set => tempNbrCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("tempAmtCNaNotDeterm")]
      public Common TempAmtCNaNotDeterm
      {
        get => tempAmtCNaNotDeterm ??= new();
        set => tempAmtCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of TempNbrCNaCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrCNaCaseNf")]
      public Common TempNbrCNaCaseNf
      {
        get => tempNbrCNaCaseNf ??= new();
        set => tempNbrCNaCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtCNaCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtCNaCaseNf")]
      public Common TempAmtCNaCaseNf
      {
        get => tempAmtCNaCaseNf ??= new();
        set => tempAmtCNaCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrCNaInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrCNaInterSt")]
      public Common TempNbrCNaInterSt
      {
        get => tempNbrCNaInterSt ??= new();
        set => tempNbrCNaInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtCNaInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtCNaInterSt")]
      public Common TempAmtCNaInterSt
      {
        get => tempAmtCNaInterSt ??= new();
        set => tempAmtCNaInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrCNaAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrCNaAllOther")]
      public Common TempNbrCNaAllOther
      {
        get => tempNbrCNaAllOther ??= new();
        set => tempNbrCNaAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtCNaAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtCNaAllOther")]
      public Common TempAmtCNaAllOther
      {
        get => tempAmtCNaAllOther ??= new();
        set => tempAmtCNaAllOther = value;
      }

      /// <summary>
      /// A value of TempNbrCAfKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrCAfKsAr")]
      public Common TempNbrCAfKsAr
      {
        get => tempNbrCAfKsAr ??= new();
        set => tempNbrCAfKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtCAfKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtCAfKsAr")]
      public Common TempAmtCAfKsAr
      {
        get => tempAmtCAfKsAr ??= new();
        set => tempAmtCAfKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrCAfJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrCAfJjAr")]
      public Common TempNbrCAfJjAr
      {
        get => tempNbrCAfJjAr ??= new();
        set => tempNbrCAfJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtCAfJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtCAfJjAr")]
      public Common TempAmtCAfJjAr
      {
        get => tempAmtCAfJjAr ??= new();
        set => tempAmtCAfJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrCAfNotDeterm")]
      public Common TempNbrCAfNotDeterm
      {
        get => tempNbrCAfNotDeterm ??= new();
        set => tempNbrCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("tempAmtCAfNotDeterm")]
      public Common TempAmtCAfNotDeterm
      {
        get => tempAmtCAfNotDeterm ??= new();
        set => tempAmtCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of TempNbrCAfCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrCAfCaseNf")]
      public Common TempNbrCAfCaseNf
      {
        get => tempNbrCAfCaseNf ??= new();
        set => tempNbrCAfCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtCAfCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtCAfCaseNf")]
      public Common TempAmtCAfCaseNf
      {
        get => tempAmtCAfCaseNf ??= new();
        set => tempAmtCAfCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrCAfInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrCAfInterSt")]
      public Common TempNbrCAfInterSt
      {
        get => tempNbrCAfInterSt ??= new();
        set => tempNbrCAfInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtCAfInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtCAfInterSt")]
      public Common TempAmtCAfInterSt
      {
        get => tempAmtCAfInterSt ??= new();
        set => tempAmtCAfInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrCAfAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrCAfAllOther")]
      public Common TempNbrCAfAllOther
      {
        get => tempNbrCAfAllOther ??= new();
        set => tempNbrCAfAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtCAfAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtCAfAllOther")]
      public Common TempAmtCAfAllOther
      {
        get => tempAmtCAfAllOther ??= new();
        set => tempAmtCAfAllOther = value;
      }

      /// <summary>
      /// A value of TempNbrNKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrNKsAr")]
      public Common TempNbrNKsAr
      {
        get => tempNbrNKsAr ??= new();
        set => tempNbrNKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtNKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtNKsAr")]
      public Common TempAmtNKsAr
      {
        get => tempAmtNKsAr ??= new();
        set => tempAmtNKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrNJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrNJjAr")]
      public Common TempNbrNJjAr
      {
        get => tempNbrNJjAr ??= new();
        set => tempNbrNJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtNJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtNJjAr")]
      public Common TempAmtNJjAr
      {
        get => tempAmtNJjAr ??= new();
        set => tempAmtNJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrNNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrNNotDeterm")]
      public Common TempNbrNNotDeterm
      {
        get => tempNbrNNotDeterm ??= new();
        set => tempNbrNNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtNNotDeterm.
      /// </summary>
      [JsonPropertyName("tempAmtNNotDeterm")]
      public Common TempAmtNNotDeterm
      {
        get => tempAmtNNotDeterm ??= new();
        set => tempAmtNNotDeterm = value;
      }

      /// <summary>
      /// A value of TempNbrNCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrNCaseNf")]
      public Common TempNbrNCaseNf
      {
        get => tempNbrNCaseNf ??= new();
        set => tempNbrNCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtNCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtNCaseNf")]
      public Common TempAmtNCaseNf
      {
        get => tempAmtNCaseNf ??= new();
        set => tempAmtNCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrNInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrNInterSt")]
      public Common TempNbrNInterSt
      {
        get => tempNbrNInterSt ??= new();
        set => tempNbrNInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtNInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtNInterSt")]
      public Common TempAmtNInterSt
      {
        get => tempAmtNInterSt ??= new();
        set => tempAmtNInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrNAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrNAllOther")]
      public Common TempNbrNAllOther
      {
        get => tempNbrNAllOther ??= new();
        set => tempNbrNAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtNAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtNAllOther")]
      public Common TempAmtNAllOther
      {
        get => tempAmtNAllOther ??= new();
        set => tempAmtNAllOther = value;
      }

      private Common tempNbrOfCollRead;
      private Common tempAmtOfCollRead;
      private Common trmpNbrOfAps;
      private Common tempNbrOfCollBackedOff;
      private Common tempAmtOfCollBackedOff;
      private Common tempNbrOfErrorsCreated;
      private Common tempAmtOfErrorsCreated;
      private Common tempNbrOfCollNotFulApl;
      private Common tempAmtOfCollNotFulApl;
      private Common tempNbrOfCreditsCreated;
      private Common tempAmtOfCreditsCreated;
      private Common tempNbrOfApsWoCollFlA;
      private Common tempNbrCNaKsAr;
      private Common tempAmtCNaKsAr;
      private Common tempNbrCNaJjAr;
      private Common tempAmtCNaJjAr;
      private Common tempNbrCNaNotDeterm;
      private Common tempAmtCNaNotDeterm;
      private Common tempNbrCNaCaseNf;
      private Common tempAmtCNaCaseNf;
      private Common tempNbrCNaInterSt;
      private Common tempAmtCNaInterSt;
      private Common tempNbrCNaAllOther;
      private Common tempAmtCNaAllOther;
      private Common tempNbrCAfKsAr;
      private Common tempAmtCAfKsAr;
      private Common tempNbrCAfJjAr;
      private Common tempAmtCAfJjAr;
      private Common tempNbrCAfNotDeterm;
      private Common tempAmtCAfNotDeterm;
      private Common tempNbrCAfCaseNf;
      private Common tempAmtCAfCaseNf;
      private Common tempNbrCAfInterSt;
      private Common tempAmtCAfInterSt;
      private Common tempNbrCAfAllOther;
      private Common tempAmtCAfAllOther;
      private Common tempNbrNKsAr;
      private Common tempAmtNKsAr;
      private Common tempNbrNJjAr;
      private Common tempAmtNJjAr;
      private Common tempNbrNNotDeterm;
      private Common tempAmtNNotDeterm;
      private Common tempNbrNCaseNf;
      private Common tempAmtNCaseNf;
      private Common tempNbrNInterSt;
      private Common tempAmtNInterSt;
      private Common tempNbrNAllOther;
      private Common tempAmtNAllOther;
    }

    /// <summary>A FinalTotalsGroup group.</summary>
    [Serializable]
    public class FinalTotalsGroup
    {
      /// <summary>
      /// A value of FinalNbrOfCollRead.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollRead")]
      public Common FinalNbrOfCollRead
      {
        get => finalNbrOfCollRead ??= new();
        set => finalNbrOfCollRead = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollRead.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollRead")]
      public Common FinalAmtOfCollRead
      {
        get => finalAmtOfCollRead ??= new();
        set => finalAmtOfCollRead = value;
      }

      /// <summary>
      /// A value of FinalNbrOfAps.
      /// </summary>
      [JsonPropertyName("finalNbrOfAps")]
      public Common FinalNbrOfAps
      {
        get => finalNbrOfAps ??= new();
        set => finalNbrOfAps = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollBackedOff")]
      public Common FinalNbrOfCollBackedOff
      {
        get => finalNbrOfCollBackedOff ??= new();
        set => finalNbrOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollBackedOff")]
      public Common FinalAmtOfCollBackedOff
      {
        get => finalAmtOfCollBackedOff ??= new();
        set => finalAmtOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of FinalNbrOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("finalNbrOfErrorsCreated")]
      public Common FinalNbrOfErrorsCreated
      {
        get => finalNbrOfErrorsCreated ??= new();
        set => finalNbrOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of FinalAmtOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("finalAmtOfErrorsCreated")]
      public Common FinalAmtOfErrorsCreated
      {
        get => finalAmtOfErrorsCreated ??= new();
        set => finalAmtOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCollNotFulAp.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollNotFulAp")]
      public Common FinalNbrOfCollNotFulAp
      {
        get => finalNbrOfCollNotFulAp ??= new();
        set => finalNbrOfCollNotFulAp = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollNotFulAp.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollNotFulAp")]
      public Common FinalAmtOfCollNotFulAp
      {
        get => finalAmtOfCollNotFulAp ??= new();
        set => finalAmtOfCollNotFulAp = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("finalNbrOfCreditsCreated")]
      public Common FinalNbrOfCreditsCreated
      {
        get => finalNbrOfCreditsCreated ??= new();
        set => finalNbrOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("finalAmtOfCreditsCreated")]
      public Common FinalAmtOfCreditsCreated
      {
        get => finalAmtOfCreditsCreated ??= new();
        set => finalAmtOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of FinalNbrOfApsWoColFlA.
      /// </summary>
      [JsonPropertyName("finalNbrOfApsWoColFlA")]
      public Common FinalNbrOfApsWoColFlA
      {
        get => finalNbrOfApsWoColFlA ??= new();
        set => finalNbrOfApsWoColFlA = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrCNaKsAr")]
      public Common FinalNbrCNaKsAr
      {
        get => finalNbrCNaKsAr ??= new();
        set => finalNbrCNaKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtCNaKsAr")]
      public Common FinalAmtCNaKsAr
      {
        get => finalAmtCNaKsAr ??= new();
        set => finalAmtCNaKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrCNaJjAr")]
      public Common FinalNbrCNaJjAr
      {
        get => finalNbrCNaJjAr ??= new();
        set => finalNbrCNaJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtCNaJjAr")]
      public Common FinalAmtCNaJjAr
      {
        get => finalAmtCNaJjAr ??= new();
        set => finalAmtCNaJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrCNaNotDeterm")]
      public Common FinalNbrCNaNotDeterm
      {
        get => finalNbrCNaNotDeterm ??= new();
        set => finalNbrCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtCNaNotDeterm")]
      public Common FinalAmtCNaNotDeterm
      {
        get => finalAmtCNaNotDeterm ??= new();
        set => finalAmtCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrCNaCaseNf")]
      public Common FinalNbrCNaCaseNf
      {
        get => finalNbrCNaCaseNf ??= new();
        set => finalNbrCNaCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtCNaCaseNf")]
      public Common FinalAmtCNaCaseNf
      {
        get => finalAmtCNaCaseNf ??= new();
        set => finalAmtCNaCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrCNaInterSt")]
      public Common FinalNbrCNaInterSt
      {
        get => finalNbrCNaInterSt ??= new();
        set => finalNbrCNaInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtCNaInterSt")]
      public Common FinalAmtCNaInterSt
      {
        get => finalAmtCNaInterSt ??= new();
        set => finalAmtCNaInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrCNaAllOther")]
      public Common FinalNbrCNaAllOther
      {
        get => finalNbrCNaAllOther ??= new();
        set => finalNbrCNaAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtCNaAllOther")]
      public Common FinalAmtCNaAllOther
      {
        get => finalAmtCNaAllOther ??= new();
        set => finalAmtCNaAllOther = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrCAfKsAr")]
      public Common FinalNbrCAfKsAr
      {
        get => finalNbrCAfKsAr ??= new();
        set => finalNbrCAfKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtCAfKsAr")]
      public Common FinalAmtCAfKsAr
      {
        get => finalAmtCAfKsAr ??= new();
        set => finalAmtCAfKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrCAfJjAr")]
      public Common FinalNbrCAfJjAr
      {
        get => finalNbrCAfJjAr ??= new();
        set => finalNbrCAfJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtCAfJjAr")]
      public Common FinalAmtCAfJjAr
      {
        get => finalAmtCAfJjAr ??= new();
        set => finalAmtCAfJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrCAfNotDeterm")]
      public Common FinalNbrCAfNotDeterm
      {
        get => finalNbrCAfNotDeterm ??= new();
        set => finalNbrCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtCAfNotDeterm")]
      public Common FinalAmtCAfNotDeterm
      {
        get => finalAmtCAfNotDeterm ??= new();
        set => finalAmtCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrCAfCaseNf")]
      public Common FinalNbrCAfCaseNf
      {
        get => finalNbrCAfCaseNf ??= new();
        set => finalNbrCAfCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtCAfCaseNf")]
      public Common FinalAmtCAfCaseNf
      {
        get => finalAmtCAfCaseNf ??= new();
        set => finalAmtCAfCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrCAfInterSt")]
      public Common FinalNbrCAfInterSt
      {
        get => finalNbrCAfInterSt ??= new();
        set => finalNbrCAfInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtCAfInterSt")]
      public Common FinalAmtCAfInterSt
      {
        get => finalAmtCAfInterSt ??= new();
        set => finalAmtCAfInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrCAfAllOther")]
      public Common FinalNbrCAfAllOther
      {
        get => finalNbrCAfAllOther ??= new();
        set => finalNbrCAfAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtCAfAllOther")]
      public Common FinalAmtCAfAllOther
      {
        get => finalAmtCAfAllOther ??= new();
        set => finalAmtCAfAllOther = value;
      }

      /// <summary>
      /// A value of FinalNbrNKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrNKsAr")]
      public Common FinalNbrNKsAr
      {
        get => finalNbrNKsAr ??= new();
        set => finalNbrNKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtNKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtNKsAr")]
      public Common FinalAmtNKsAr
      {
        get => finalAmtNKsAr ??= new();
        set => finalAmtNKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrNJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrNJjAr")]
      public Common FinalNbrNJjAr
      {
        get => finalNbrNJjAr ??= new();
        set => finalNbrNJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtNJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtNJjAr")]
      public Common FinalAmtNJjAr
      {
        get => finalAmtNJjAr ??= new();
        set => finalAmtNJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrNNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrNNotDeterm")]
      public Common FinalNbrNNotDeterm
      {
        get => finalNbrNNotDeterm ??= new();
        set => finalNbrNNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtNNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtNNotDeterm")]
      public Common FinalAmtNNotDeterm
      {
        get => finalAmtNNotDeterm ??= new();
        set => finalAmtNNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrNCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrNCaseNf")]
      public Common FinalNbrNCaseNf
      {
        get => finalNbrNCaseNf ??= new();
        set => finalNbrNCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtNCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtNCaseNf")]
      public Common FinalAmtNCaseNf
      {
        get => finalAmtNCaseNf ??= new();
        set => finalAmtNCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrNInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrNInterSt")]
      public Common FinalNbrNInterSt
      {
        get => finalNbrNInterSt ??= new();
        set => finalNbrNInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtNInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtNInterSt")]
      public Common FinalAmtNInterSt
      {
        get => finalAmtNInterSt ??= new();
        set => finalAmtNInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrNAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrNAllOther")]
      public Common FinalNbrNAllOther
      {
        get => finalNbrNAllOther ??= new();
        set => finalNbrNAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtNAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtNAllOther")]
      public Common FinalAmtNAllOther
      {
        get => finalAmtNAllOther ??= new();
        set => finalAmtNAllOther = value;
      }

      private Common finalNbrOfCollRead;
      private Common finalAmtOfCollRead;
      private Common finalNbrOfAps;
      private Common finalNbrOfCollBackedOff;
      private Common finalAmtOfCollBackedOff;
      private Common finalNbrOfErrorsCreated;
      private Common finalAmtOfErrorsCreated;
      private Common finalNbrOfCollNotFulAp;
      private Common finalAmtOfCollNotFulAp;
      private Common finalNbrOfCreditsCreated;
      private Common finalAmtOfCreditsCreated;
      private Common finalNbrOfApsWoColFlA;
      private Common finalNbrCNaKsAr;
      private Common finalAmtCNaKsAr;
      private Common finalNbrCNaJjAr;
      private Common finalAmtCNaJjAr;
      private Common finalNbrCNaNotDeterm;
      private Common finalAmtCNaNotDeterm;
      private Common finalNbrCNaCaseNf;
      private Common finalAmtCNaCaseNf;
      private Common finalNbrCNaInterSt;
      private Common finalAmtCNaInterSt;
      private Common finalNbrCNaAllOther;
      private Common finalAmtCNaAllOther;
      private Common finalNbrCAfKsAr;
      private Common finalAmtCAfKsAr;
      private Common finalNbrCAfJjAr;
      private Common finalAmtCAfJjAr;
      private Common finalNbrCAfNotDeterm;
      private Common finalAmtCAfNotDeterm;
      private Common finalNbrCAfCaseNf;
      private Common finalAmtCAfCaseNf;
      private Common finalNbrCAfInterSt;
      private Common finalAmtCAfInterSt;
      private Common finalNbrCAfAllOther;
      private Common finalAmtCAfAllOther;
      private Common finalNbrNKsAr;
      private Common finalAmtNKsAr;
      private Common finalNbrNJjAr;
      private Common finalAmtNJjAr;
      private Common finalNbrNNotDeterm;
      private Common finalAmtNNotDeterm;
      private Common finalNbrNCaseNf;
      private Common finalAmtNCaseNf;
      private Common finalNbrNInterSt;
      private Common finalAmtNInterSt;
      private Common finalNbrNAllOther;
      private Common finalAmtNAllOther;
    }

    /// <summary>
    /// Gets a value of TempTotals.
    /// </summary>
    [JsonPropertyName("tempTotals")]
    public TempTotalsGroup TempTotals
    {
      get => tempTotals ?? (tempTotals = new());
      set => tempTotals = value;
    }

    /// <summary>
    /// Gets a value of FinalTotals.
    /// </summary>
    [JsonPropertyName("finalTotals")]
    public FinalTotalsGroup FinalTotals
    {
      get => finalTotals ?? (finalTotals = new());
      set => finalTotals = value;
    }

    private TempTotalsGroup tempTotals;
    private FinalTotalsGroup finalTotals;
  }
#endregion
}
