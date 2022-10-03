// Program: FN_DISPLAY_PROGRAM_AMOUNTS_DTL, ID: 374421984, model: 746.
// Short name: SWE01547
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_PROGRAM_AMOUNTS_DTL.
/// </summary>
[Serializable]
public partial class FnDisplayProgramAmountsDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_PROGRAM_AMOUNTS_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayProgramAmountsDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayProgramAmountsDtl.
  /// </summary>
  public FnDisplayProgramAmountsDtl(IContext context, Import import,
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
    // *******************************************************************
    // Developer     :  Sree Veettil
    // Date          :  05-02-2000
    // Description:-  According to the PRWORA changes the screen has been 
    // modified into a scrollable one.The POGRAM amounts  displayed on the
    // screen  has been broken down depending upon the STATE of the program.
    // It is impossible to accomodate in one screen,so populate the totals in a 
    // goup view inorder to make it scrollable.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Group.Index = -1;

    for(local.Common.Count = 1; local.Common.Count <= 13; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.ArrearTitle.Text5 = "NA-NA";
          local.InterestTitle.Text5 = "NA-NA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaNaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaNaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaNaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaNaInterestCollected;

          break;
        case 2:
          local.ArrearTitle.Text5 = "NA-UP";
          local.InterestTitle.Text5 = "NA-UP";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUpArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUpInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUpArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUpInterestCollected;

          break;
        case 3:
          local.ArrearTitle.Text5 = "NA-UD";
          local.InterestTitle.Text5 = "NA-UD";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUdArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUdInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUdArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaUdInterestCollected;

          break;
        case 4:
          local.ArrearTitle.Text5 = "NA-CA";
          local.InterestTitle.Text5 = "NA-CA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaCaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaCaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaCaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaCaInterestCollected;

          break;
        case 5:
          local.ArrearTitle.Text5 = "AF-TA";
          local.InterestTitle.Text5 = "AF-TA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfTaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfTaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfTaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfTaInterestCollected;

          break;
        case 6:
          local.ArrearTitle.Text5 = "AF-PA";
          local.InterestTitle.Text5 = "AF-PA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfPaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfPaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfPaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfPaInterestCollected;

          break;
        case 7:
          local.ArrearTitle.Text5 = "AF-CA";
          local.InterestTitle.Text5 = "AF-CA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfCaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfCaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfCaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfCaInterestCollected;

          break;
        case 8:
          local.ArrearTitle.Text5 = "FC-PA";
          local.InterestTitle.Text5 = "FC-PA";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FcPaArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FcPaInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FcPaArrearCollected;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FcPaInterestCollected;

          break;
        case 9:
          local.ArrearTitle.Text5 = "NF";
          local.InterestTitle.Text5 = "NF";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NfArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NfInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NfArrearsColl;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NfInterestColl;

          break;
        case 10:
          local.ArrearTitle.Text5 = "NC";
          local.InterestTitle.Text5 = "NC";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NcArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NcInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NcArrearsColl;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NcInterestColl;

          break;
        case 11:
          local.ArrearTitle.Text5 = "AFI";
          local.InterestTitle.Text5 = "AFI";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfiArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfiInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfiArrearsColl;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.AfiInterestColl;

          break;
        case 12:
          local.ArrearTitle.Text5 = "NAI";
          local.InterestTitle.Text5 = "NAI";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaiArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaiInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaiArrearsColl;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.NaiInterestColl;

          break;
        case 13:
          local.ArrearTitle.Text5 = "FCI";
          local.InterestTitle.Text5 = "FCI";
          local.ArrearOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FciArrearsOwed;
          local.InterestOwedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FciInterestOwed;
          local.ArrearCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FciArrearsColl;
          local.InterestCollectedAmount.AverageCurrency =
            import.ScreenOwedAmountsDtl.FciInterestColl;

          break;
        default:
          break;
      }

      if (local.ArrearOwedAmount.AverageCurrency != 0 || local
        .InterestOwedAmount.AverageCurrency != 0 || local
        .ArrearCollectedAmount.AverageCurrency != 0 || local
        .InterestCollectedAmount.AverageCurrency != 0)
      {
        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GarrCollTitle.Text5 = local.ArrearTitle.Text5;
        export.Group.Update.GarrOweTitle.Text5 = local.ArrearTitle.Text5;
        export.Group.Update.GintCollTitle.Text5 = local.InterestTitle.Text5;
        export.Group.Update.GintOweTitle.Text5 = local.InterestTitle.Text5;
        export.Group.Update.GarrOweAmount.AverageCurrency =
          local.ArrearOwedAmount.AverageCurrency;
        export.Group.Update.GintOweAmount.AverageCurrency =
          local.InterestOwedAmount.AverageCurrency;
        export.Group.Update.GarrCollAmount.AverageCurrency =
          local.ArrearCollectedAmount.AverageCurrency;
        export.Group.Update.GintCollAmount.AverageCurrency =
          local.InterestCollectedAmount.AverageCurrency;
      }
    }
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
    /// <summary>
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
    }

    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GarrOweTitle.
      /// </summary>
      [JsonPropertyName("garrOweTitle")]
      public WorkArea GarrOweTitle
      {
        get => garrOweTitle ??= new();
        set => garrOweTitle = value;
      }

      /// <summary>
      /// A value of GarrOweAmount.
      /// </summary>
      [JsonPropertyName("garrOweAmount")]
      public Common GarrOweAmount
      {
        get => garrOweAmount ??= new();
        set => garrOweAmount = value;
      }

      /// <summary>
      /// A value of GintOweTitle.
      /// </summary>
      [JsonPropertyName("gintOweTitle")]
      public WorkArea GintOweTitle
      {
        get => gintOweTitle ??= new();
        set => gintOweTitle = value;
      }

      /// <summary>
      /// A value of GintOweAmount.
      /// </summary>
      [JsonPropertyName("gintOweAmount")]
      public Common GintOweAmount
      {
        get => gintOweAmount ??= new();
        set => gintOweAmount = value;
      }

      /// <summary>
      /// A value of GarrCollTitle.
      /// </summary>
      [JsonPropertyName("garrCollTitle")]
      public WorkArea GarrCollTitle
      {
        get => garrCollTitle ??= new();
        set => garrCollTitle = value;
      }

      /// <summary>
      /// A value of GarrCollAmount.
      /// </summary>
      [JsonPropertyName("garrCollAmount")]
      public Common GarrCollAmount
      {
        get => garrCollAmount ??= new();
        set => garrCollAmount = value;
      }

      /// <summary>
      /// A value of GintCollTitle.
      /// </summary>
      [JsonPropertyName("gintCollTitle")]
      public WorkArea GintCollTitle
      {
        get => gintCollTitle ??= new();
        set => gintCollTitle = value;
      }

      /// <summary>
      /// A value of GintCollAmount.
      /// </summary>
      [JsonPropertyName("gintCollAmount")]
      public Common GintCollAmount
      {
        get => gintCollAmount ??= new();
        set => gintCollAmount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea garrOweTitle;
      private Common garrOweAmount;
      private WorkArea gintOweTitle;
      private Common gintOweAmount;
      private WorkArea garrCollTitle;
      private Common garrCollAmount;
      private WorkArea gintCollTitle;
      private Common gintCollAmount;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterestCollectedAmount.
    /// </summary>
    [JsonPropertyName("interestCollectedAmount")]
    public Common InterestCollectedAmount
    {
      get => interestCollectedAmount ??= new();
      set => interestCollectedAmount = value;
    }

    /// <summary>
    /// A value of ArrearCollectedAmount.
    /// </summary>
    [JsonPropertyName("arrearCollectedAmount")]
    public Common ArrearCollectedAmount
    {
      get => arrearCollectedAmount ??= new();
      set => arrearCollectedAmount = value;
    }

    /// <summary>
    /// A value of InterestOwedAmount.
    /// </summary>
    [JsonPropertyName("interestOwedAmount")]
    public Common InterestOwedAmount
    {
      get => interestOwedAmount ??= new();
      set => interestOwedAmount = value;
    }

    /// <summary>
    /// A value of ArrearOwedAmount.
    /// </summary>
    [JsonPropertyName("arrearOwedAmount")]
    public Common ArrearOwedAmount
    {
      get => arrearOwedAmount ??= new();
      set => arrearOwedAmount = value;
    }

    /// <summary>
    /// A value of InterestTitle.
    /// </summary>
    [JsonPropertyName("interestTitle")]
    public WorkArea InterestTitle
    {
      get => interestTitle ??= new();
      set => interestTitle = value;
    }

    /// <summary>
    /// A value of ArrearTitle.
    /// </summary>
    [JsonPropertyName("arrearTitle")]
    public WorkArea ArrearTitle
    {
      get => arrearTitle ??= new();
      set => arrearTitle = value;
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

    private Common interestCollectedAmount;
    private Common arrearCollectedAmount;
    private Common interestOwedAmount;
    private Common arrearOwedAmount;
    private WorkArea interestTitle;
    private WorkArea arrearTitle;
    private Common common;
  }
#endregion
}
