// Program: FN_B700_BUILD_GV_FOR_RESTART, ID: 373315966, model: 746.
// Short name: SWE02983
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_BUILD_GV_FOR_RESTART.
/// </summary>
[Serializable]
public partial class FnB700BuildGvForRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_BUILD_GV_FOR_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700BuildGvForRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700BuildGvForRestart.
  /// </summary>
  public FnB700BuildGvForRestart(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 12/03/2007	CQ295	GVandy		Federally mandated changes to OCSE34 report.
    // ***************************************************************************
    if (!ReadOcse34())
    {
      ExitState = "FN0000_OCSE34_NF";

      return;
    }

    for(export.Group.Index = 0; export.Group.Index < Export
      .GroupGroup.Capacity; ++export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      switch(export.Group.Index + 1)
      {
        case 1:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.PreviousUndistribAmount;

          break;
        case 2:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalCollectionsAmount;

          break;
        case 3:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OffsetFederalTaxrefundAmount.GetValueOrDefault();

          break;
        case 4:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OffsetStateTaxRefundAmount.GetValueOrDefault();

          break;
        case 5:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.UnemploymentCompAmount.GetValueOrDefault();

          break;
        case 6:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.AdminstrativeEnforceAmount.GetValueOrDefault();

          break;
        case 7:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.IncomeWithholdingAmount.GetValueOrDefault();

          break;
        case 8:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherStatesAmount.GetValueOrDefault();

          break;
        case 9:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherSourcesAmount.GetValueOrDefault();

          break;
        case 10:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.AdjustmentsAmount.GetValueOrDefault();

          break;
        case 11:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NonIvdCasesAmount.GetValueOrDefault();

          break;
        case 12:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherStatesCurrentIvaAmount.GetValueOrDefault();

          break;
        case 13:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherStatesCurrentIveAmount.GetValueOrDefault();

          break;
        case 14:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherstateFormerAssistAmount.GetValueOrDefault();

          break;
        case 15:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherStateNeverAssistAmount.GetValueOrDefault();

          break;
        case 16:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherStateAmtForward.GetValueOrDefault();

          break;
        case 17:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.AvailForDistributionAmount.GetValueOrDefault();

          break;
        case 18:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistribAssistReimbIvaAmount.GetValueOrDefault();

          break;
        case 19:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistribAssistReimbIveAmount.GetValueOrDefault();

          break;
        case 20:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistribAssistReimbFmrAmount.GetValueOrDefault();

          break;
        case 21:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistribAssistReimbAmount.GetValueOrDefault();

          break;
        case 22:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedMedSupportIvaAmt.GetValueOrDefault();

          break;
        case 23:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedMedSupportIveAmt.GetValueOrDefault();

          break;
        case 24:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedMedSupportFmrAmt.GetValueOrDefault();

          break;
        case 25:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedMedSupportNvrAmt.GetValueOrDefault();

          break;
        case 26:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedMedSupportAmount.GetValueOrDefault();

          break;
        case 27:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedFamilyIvaAmount.GetValueOrDefault();

          break;
        case 28:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedFamilyIveAmount.GetValueOrDefault();

          break;
        case 29:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedFamilyFormerAmt.GetValueOrDefault();

          break;
        case 30:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedFamilyNeverAmount.GetValueOrDefault();

          break;
        case 31:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistributedFamilyAmount.GetValueOrDefault();

          break;
        case 32:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalDistributedIvaAmount.GetValueOrDefault();

          break;
        case 33:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalDistributedIveAmount.GetValueOrDefault();

          break;
        case 34:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalDistributedFormerAmount.GetValueOrDefault();

          break;
        case 35:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalDistributedNeverAmount.GetValueOrDefault();

          break;
        case 36:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotalDistributedAmount.GetValueOrDefault();

          break;
        case 37:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.GrossUndistributedAmount.GetValueOrDefault();

          break;
        case 38:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.UndistributedAmount.GetValueOrDefault();

          break;
        case 39:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetUndistributedAmount.GetValueOrDefault();

          break;
        case 40:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FederalShareIvaAmount.GetValueOrDefault();

          break;
        case 41:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FederalShareIveAmount.GetValueOrDefault();

          break;
        case 42:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FederalShareFormerAmount.GetValueOrDefault();

          break;
        case 43:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FederalShareTotalAmount.GetValueOrDefault();

          break;
        case 44:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.IncentivePaymentIvaAmount.GetValueOrDefault();

          break;
        case 45:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.IncentivePaymentFormerAmount.GetValueOrDefault();

          break;
        case 46:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.IncentivePaymentAmount.GetValueOrDefault();

          break;
        case 47:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetFederalShareIvaAmount.GetValueOrDefault();

          break;
        case 48:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetFederalShareFormerAmount.GetValueOrDefault();

          break;
        case 49:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetFederalShareAmount.GetValueOrDefault();

          break;
        case 50:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FeesRetainOtherStatesAmount.GetValueOrDefault();

          break;
        case 51:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OthStFmr2Amt.GetValueOrDefault();

          break;
        case 52:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OthStNevr2Amt.GetValueOrDefault();

          break;
        case 53:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistribFmr2Amt.GetValueOrDefault();

          break;
        case 54:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistMedFmr2Amt.GetValueOrDefault();

          break;
        case 55:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistMedNvr2Amt.GetValueOrDefault();

          break;
        case 56:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistFamFmr2Amt.GetValueOrDefault();

          break;
        case 57:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.DistFamNvr2Amt.GetValueOrDefault();

          break;
        case 58:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotDistFmr2Amt.GetValueOrDefault();

          break;
        case 59:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.TotDistNvr2Amt.GetValueOrDefault();

          break;
        case 60:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetUndistPndAmt.GetValueOrDefault();

          break;
        case 61:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.NetUndistUrsAmt.GetValueOrDefault();

          break;
        case 62:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FedShrFmr2Amt.GetValueOrDefault();

          break;
        case 63:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FedShr2Amt.GetValueOrDefault();

          break;
        case 64:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OtherCountryAmt.GetValueOrDefault();

          break;
        case 65:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OutsideIvdAmt.GetValueOrDefault();

          break;
        case 66:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.OthCntryAmtForw.GetValueOrDefault();

          break;
        case 67:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.Passthru4AAmt.GetValueOrDefault();

          break;
        case 68:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.Passthru4EAmt.GetValueOrDefault();

          break;
        case 69:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.PassthruFmr4AAmt.GetValueOrDefault();

          break;
        case 70:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.PassthruFmr4EAmt.GetValueOrDefault();

          break;
        case 71:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.PassthruTotalAmt.GetValueOrDefault();

          break;
        case 72:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FeesRetNeverAmt.GetValueOrDefault();

          break;
        case 73:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FeesRetOtherAmt.GetValueOrDefault();

          break;
        case 74:
          export.Group.Update.Common.TotalCurrency =
            entities.Ocse34.FeesRetTotalAmt.GetValueOrDefault();

          break;
        default:
          break;
      }
    }

    export.Group.CheckIndex();
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", import.Ocse34.Period);
        db.SetDateTime(
          command, "createdTimestamp",
          import.Ocse34.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.PreviousUndistribAmount = db.GetInt32(reader, 1);
        entities.Ocse34.TotalCollectionsAmount = db.GetInt32(reader, 2);
        entities.Ocse34.OffsetFederalTaxrefundAmount =
          db.GetNullableInt32(reader, 3);
        entities.Ocse34.OffsetStateTaxRefundAmount =
          db.GetNullableInt32(reader, 4);
        entities.Ocse34.UnemploymentCompAmount = db.GetNullableInt32(reader, 5);
        entities.Ocse34.AdminstrativeEnforceAmount =
          db.GetNullableInt32(reader, 6);
        entities.Ocse34.IncomeWithholdingAmount =
          db.GetNullableInt32(reader, 7);
        entities.Ocse34.OtherStatesAmount = db.GetNullableInt32(reader, 8);
        entities.Ocse34.OtherSourcesAmount = db.GetNullableInt32(reader, 9);
        entities.Ocse34.AdjustmentsAmount = db.GetNullableInt32(reader, 10);
        entities.Ocse34.NonIvdCasesAmount = db.GetNullableInt32(reader, 11);
        entities.Ocse34.OtherStatesCurrentIvaAmount =
          db.GetNullableInt32(reader, 12);
        entities.Ocse34.OtherStatesCurrentIveAmount =
          db.GetNullableInt32(reader, 13);
        entities.Ocse34.OtherstateFormerAssistAmount =
          db.GetNullableInt32(reader, 14);
        entities.Ocse34.OtherStateNeverAssistAmount =
          db.GetNullableInt32(reader, 15);
        entities.Ocse34.OtherStateAmtForward = db.GetNullableInt32(reader, 16);
        entities.Ocse34.AvailForDistributionAmount =
          db.GetNullableInt32(reader, 17);
        entities.Ocse34.DistribAssistReimbIvaAmount =
          db.GetNullableInt32(reader, 18);
        entities.Ocse34.DistribAssistReimbIveAmount =
          db.GetNullableInt32(reader, 19);
        entities.Ocse34.DistribAssistReimbFmrAmount =
          db.GetNullableInt32(reader, 20);
        entities.Ocse34.DistribAssistReimbAmount =
          db.GetNullableInt32(reader, 21);
        entities.Ocse34.DistributedMedSupportIvaAmt =
          db.GetNullableInt32(reader, 22);
        entities.Ocse34.DistributedMedSupportIveAmt =
          db.GetNullableInt32(reader, 23);
        entities.Ocse34.DistributedMedSupportFmrAmt =
          db.GetNullableInt32(reader, 24);
        entities.Ocse34.DistributedMedSupportNvrAmt =
          db.GetNullableInt32(reader, 25);
        entities.Ocse34.DistributedMedSupportAmount =
          db.GetNullableInt32(reader, 26);
        entities.Ocse34.DistributedFamilyIvaAmount =
          db.GetNullableInt32(reader, 27);
        entities.Ocse34.DistributedFamilyIveAmount =
          db.GetNullableInt32(reader, 28);
        entities.Ocse34.DistributedFamilyFormerAmt =
          db.GetNullableInt32(reader, 29);
        entities.Ocse34.DistributedFamilyNeverAmount =
          db.GetNullableInt32(reader, 30);
        entities.Ocse34.DistributedFamilyAmount =
          db.GetNullableInt32(reader, 31);
        entities.Ocse34.TotalDistributedIvaAmount =
          db.GetNullableInt32(reader, 32);
        entities.Ocse34.TotalDistributedIveAmount =
          db.GetNullableInt32(reader, 33);
        entities.Ocse34.TotalDistributedFormerAmount =
          db.GetNullableInt32(reader, 34);
        entities.Ocse34.TotalDistributedNeverAmount =
          db.GetNullableInt32(reader, 35);
        entities.Ocse34.TotalDistributedAmount =
          db.GetNullableInt32(reader, 36);
        entities.Ocse34.GrossUndistributedAmount =
          db.GetNullableInt32(reader, 37);
        entities.Ocse34.UndistributedAmount = db.GetNullableInt32(reader, 38);
        entities.Ocse34.NetUndistributedAmount =
          db.GetNullableInt32(reader, 39);
        entities.Ocse34.FederalShareIvaAmount = db.GetNullableInt32(reader, 40);
        entities.Ocse34.FederalShareIveAmount = db.GetNullableInt32(reader, 41);
        entities.Ocse34.FederalShareFormerAmount =
          db.GetNullableInt32(reader, 42);
        entities.Ocse34.FederalShareTotalAmount =
          db.GetNullableInt32(reader, 43);
        entities.Ocse34.IncentivePaymentIvaAmount =
          db.GetNullableInt32(reader, 44);
        entities.Ocse34.IncentivePaymentFormerAmount =
          db.GetNullableInt32(reader, 45);
        entities.Ocse34.IncentivePaymentAmount =
          db.GetNullableInt32(reader, 46);
        entities.Ocse34.NetFederalShareIvaAmount =
          db.GetNullableInt32(reader, 47);
        entities.Ocse34.NetFederalShareFormerAmount =
          db.GetNullableInt32(reader, 48);
        entities.Ocse34.NetFederalShareAmount = db.GetNullableInt32(reader, 49);
        entities.Ocse34.FeesRetainOtherStatesAmount =
          db.GetNullableInt32(reader, 50);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 51);
        entities.Ocse34.FmapRate = db.GetDecimal(reader, 52);
        entities.Ocse34.OthStFmr2Amt = db.GetNullableInt32(reader, 53);
        entities.Ocse34.OthStNevr2Amt = db.GetNullableInt32(reader, 54);
        entities.Ocse34.DistribFmr2Amt = db.GetNullableInt32(reader, 55);
        entities.Ocse34.DistMedFmr2Amt = db.GetNullableInt32(reader, 56);
        entities.Ocse34.DistMedNvr2Amt = db.GetNullableInt32(reader, 57);
        entities.Ocse34.DistFamFmr2Amt = db.GetNullableInt32(reader, 58);
        entities.Ocse34.DistFamNvr2Amt = db.GetNullableInt32(reader, 59);
        entities.Ocse34.TotDistFmr2Amt = db.GetNullableInt32(reader, 60);
        entities.Ocse34.TotDistNvr2Amt = db.GetNullableInt32(reader, 61);
        entities.Ocse34.NetUndistPndAmt = db.GetNullableInt32(reader, 62);
        entities.Ocse34.NetUndistUrsAmt = db.GetNullableInt32(reader, 63);
        entities.Ocse34.FedShrFmr2Amt = db.GetNullableInt32(reader, 64);
        entities.Ocse34.FedShr2Amt = db.GetNullableInt32(reader, 65);
        entities.Ocse34.CseDisbCreditAmt = db.GetNullableInt32(reader, 66);
        entities.Ocse34.CseDisbDebitAmt = db.GetNullableInt32(reader, 67);
        entities.Ocse34.CseWarrantAmt = db.GetNullableInt32(reader, 68);
        entities.Ocse34.CsePaymentAmt = db.GetNullableInt32(reader, 69);
        entities.Ocse34.CseInterstateAmt = db.GetNullableInt32(reader, 70);
        entities.Ocse34.CseCshRcptDtlSuspAmt = db.GetNullableInt32(reader, 71);
        entities.Ocse34.CseDisbSuppressAmt = db.GetNullableInt32(reader, 72);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 73);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 74);
        entities.Ocse34.AdjustFooterText = db.GetNullableString(reader, 75);
        entities.Ocse34.KpcNon4DIwoCollAmt = db.GetNullableInt32(reader, 76);
        entities.Ocse34.KpcIvdNonIwoCollAmt = db.GetNullableInt32(reader, 77);
        entities.Ocse34.KpcNonIvdIwoForwCollAmt =
          db.GetNullableInt32(reader, 78);
        entities.Ocse34.KpcStaleDateAmt = db.GetNullableInt32(reader, 79);
        entities.Ocse34.KpcHeldDisbAmt = db.GetNullableInt32(reader, 80);
        entities.Ocse34.UiIvdNonIwoIntAmt = db.GetNullableInt32(reader, 81);
        entities.Ocse34.KpcUiIvdNonIwoNonIntAmt =
          db.GetNullableInt32(reader, 82);
        entities.Ocse34.KpcUiIvdNivdIwoAmt = db.GetNullableInt32(reader, 83);
        entities.Ocse34.KpcUiNonIvdIwoAmt = db.GetNullableInt32(reader, 84);
        entities.Ocse34.OtherCountryAmt = db.GetNullableInt32(reader, 85);
        entities.Ocse34.OutsideIvdAmt = db.GetNullableInt32(reader, 86);
        entities.Ocse34.FeesRetNeverAmt = db.GetNullableInt32(reader, 87);
        entities.Ocse34.FeesRetOtherAmt = db.GetNullableInt32(reader, 88);
        entities.Ocse34.FeesRetTotalAmt = db.GetNullableInt32(reader, 89);
        entities.Ocse34.OthCntryAmtForw = db.GetNullableInt32(reader, 90);
        entities.Ocse34.Passthru4AAmt = db.GetNullableInt32(reader, 91);
        entities.Ocse34.Passthru4EAmt = db.GetNullableInt32(reader, 92);
        entities.Ocse34.PassthruFmr4AAmt = db.GetNullableInt32(reader, 93);
        entities.Ocse34.PassthruFmr4EAmt = db.GetNullableInt32(reader, 94);
        entities.Ocse34.PassthruTotalAmt = db.GetNullableInt32(reader, 95);
        entities.Ocse34.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}
