// Program: FN_B700_APPLY_UPDATES, ID: 373315987, model: 746.
// Short name: SWE02981
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_APPLY_UPDATES.
/// </summary>
[Serializable]
public partial class FnB700ApplyUpdates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_APPLY_UPDATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700ApplyUpdates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700ApplyUpdates.
  /// </summary>
  public FnB700ApplyUpdates(IContext context, Import import, Export export):
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
    for(import.Group.Index = 0; import.Group.Index < Import
      .GroupGroup.Capacity; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      switch(import.Group.Index + 1)
      {
        case 1:
          local.Ocse34.PreviousUndistribAmount =
            (int)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 2:
          local.Ocse34.TotalCollectionsAmount =
            (int)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 3:
          local.Ocse34.OffsetFederalTaxrefundAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 4:
          local.Ocse34.OffsetStateTaxRefundAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 5:
          local.Ocse34.UnemploymentCompAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 6:
          local.Ocse34.AdminstrativeEnforceAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 7:
          local.Ocse34.IncomeWithholdingAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 8:
          local.Ocse34.OtherStatesAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 9:
          local.Ocse34.OtherSourcesAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 10:
          local.Ocse34.AdjustmentsAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 11:
          local.Ocse34.NonIvdCasesAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 12:
          local.Ocse34.OtherStatesCurrentIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 13:
          local.Ocse34.OtherStatesCurrentIveAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 14:
          local.Ocse34.OtherstateFormerAssistAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 15:
          local.Ocse34.OtherStateNeverAssistAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 16:
          local.Ocse34.OtherStateAmtForward =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 17:
          local.Ocse34.AvailForDistributionAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 18:
          local.Ocse34.DistribAssistReimbIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 19:
          local.Ocse34.DistribAssistReimbIveAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 20:
          local.Ocse34.DistribAssistReimbFmrAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 21:
          local.Ocse34.DistribAssistReimbAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 22:
          local.Ocse34.DistributedMedSupportIvaAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 23:
          local.Ocse34.DistributedMedSupportIveAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 24:
          local.Ocse34.DistributedMedSupportFmrAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 25:
          local.Ocse34.DistributedMedSupportNvrAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 26:
          local.Ocse34.DistributedMedSupportAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 27:
          local.Ocse34.DistributedFamilyIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 28:
          local.Ocse34.DistributedFamilyIveAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 29:
          local.Ocse34.DistributedFamilyFormerAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 30:
          local.Ocse34.DistributedFamilyNeverAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 31:
          local.Ocse34.DistributedFamilyAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 32:
          local.Ocse34.TotalDistributedIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 33:
          local.Ocse34.TotalDistributedIveAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 34:
          local.Ocse34.TotalDistributedFormerAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 35:
          local.Ocse34.TotalDistributedNeverAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 36:
          local.Ocse34.TotalDistributedAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 37:
          local.Ocse34.GrossUndistributedAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 38:
          local.Ocse34.UndistributedAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 39:
          local.Ocse34.NetUndistributedAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 40:
          local.Ocse34.FederalShareIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 41:
          local.Ocse34.FederalShareIveAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 42:
          local.Ocse34.FederalShareFormerAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 43:
          local.Ocse34.FederalShareTotalAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 44:
          local.Ocse34.IncentivePaymentIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 45:
          local.Ocse34.IncentivePaymentFormerAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 46:
          local.Ocse34.IncentivePaymentAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 47:
          local.Ocse34.NetFederalShareIvaAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 48:
          local.Ocse34.NetFederalShareFormerAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 49:
          local.Ocse34.NetFederalShareAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 50:
          local.Ocse34.FeesRetainOtherStatesAmount =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 51:
          local.Ocse34.OthStFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 52:
          local.Ocse34.OthStNevr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 53:
          local.Ocse34.DistribFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 54:
          local.Ocse34.DistMedFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 55:
          local.Ocse34.DistMedNvr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 56:
          local.Ocse34.DistFamFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 57:
          local.Ocse34.DistFamNvr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 58:
          local.Ocse34.TotDistFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 59:
          local.Ocse34.TotDistNvr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 60:
          local.Ocse34.NetUndistPndAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 61:
          local.Ocse34.NetUndistUrsAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 62:
          local.Ocse34.FedShrFmr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 63:
          local.Ocse34.FedShr2Amt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 64:
          local.Ocse34.OtherCountryAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 65:
          local.Ocse34.OutsideIvdAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 66:
          local.Ocse34.OthCntryAmtForw =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 67:
          local.Ocse34.Passthru4AAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 68:
          local.Ocse34.Passthru4EAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 69:
          local.Ocse34.PassthruFmr4AAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 70:
          local.Ocse34.PassthruFmr4EAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 71:
          local.Ocse34.PassthruTotalAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 72:
          local.Ocse34.FeesRetNeverAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 73:
          local.Ocse34.FeesRetOtherAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        case 74:
          local.Ocse34.FeesRetTotalAmt =
            (int?)Math.Round(
              import.Group.Item.Common.TotalCurrency,
            MidpointRounding.AwayFromZero);

          break;
        default:
          break;
      }
    }

    import.Group.CheckIndex();

    if (!ReadOcse34())
    {
      ExitState = "FN0000_OCSE34_NF";

      return;
    }

    try
    {
      UpdateOcse34();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OCSE34_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OCSE34_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
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
        entities.Ocse34.LastTwoDaysColl = db.GetNullableInt32(reader, 66);
        entities.Ocse34.OtherCountryAmt = db.GetNullableInt32(reader, 67);
        entities.Ocse34.OutsideIvdAmt = db.GetNullableInt32(reader, 68);
        entities.Ocse34.FeesRetNeverAmt = db.GetNullableInt32(reader, 69);
        entities.Ocse34.FeesRetOtherAmt = db.GetNullableInt32(reader, 70);
        entities.Ocse34.FeesRetTotalAmt = db.GetNullableInt32(reader, 71);
        entities.Ocse34.OthCntryAmtForw = db.GetNullableInt32(reader, 72);
        entities.Ocse34.Passthru4AAmt = db.GetNullableInt32(reader, 73);
        entities.Ocse34.Passthru4EAmt = db.GetNullableInt32(reader, 74);
        entities.Ocse34.PassthruFmr4AAmt = db.GetNullableInt32(reader, 75);
        entities.Ocse34.PassthruFmr4EAmt = db.GetNullableInt32(reader, 76);
        entities.Ocse34.PassthruTotalAmt = db.GetNullableInt32(reader, 77);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse34()
  {
    var previousUndistribAmount = local.Ocse34.PreviousUndistribAmount;
    var totalCollectionsAmount = local.Ocse34.TotalCollectionsAmount;
    var offsetFederalTaxrefundAmount =
      local.Ocse34.OffsetFederalTaxrefundAmount.GetValueOrDefault();
    var offsetStateTaxRefundAmount =
      local.Ocse34.OffsetStateTaxRefundAmount.GetValueOrDefault();
    var unemploymentCompAmount =
      local.Ocse34.UnemploymentCompAmount.GetValueOrDefault();
    var adminstrativeEnforceAmount =
      local.Ocse34.AdminstrativeEnforceAmount.GetValueOrDefault();
    var incomeWithholdingAmount =
      local.Ocse34.IncomeWithholdingAmount.GetValueOrDefault();
    var otherStatesAmount = local.Ocse34.OtherStatesAmount.GetValueOrDefault();
    var otherSourcesAmount =
      local.Ocse34.OtherSourcesAmount.GetValueOrDefault();
    var adjustmentsAmount = local.Ocse34.AdjustmentsAmount.GetValueOrDefault();
    var nonIvdCasesAmount = local.Ocse34.NonIvdCasesAmount.GetValueOrDefault();
    var otherStatesCurrentIvaAmount =
      local.Ocse34.OtherStatesCurrentIvaAmount.GetValueOrDefault();
    var otherStatesCurrentIveAmount =
      local.Ocse34.OtherStatesCurrentIveAmount.GetValueOrDefault();
    var otherstateFormerAssistAmount =
      local.Ocse34.OtherstateFormerAssistAmount.GetValueOrDefault();
    var otherStateNeverAssistAmount =
      local.Ocse34.OtherStateNeverAssistAmount.GetValueOrDefault();
    var otherStateAmtForward =
      local.Ocse34.OtherStateAmtForward.GetValueOrDefault();
    var availForDistributionAmount =
      local.Ocse34.AvailForDistributionAmount.GetValueOrDefault();
    var distribAssistReimbIvaAmount =
      local.Ocse34.DistribAssistReimbIvaAmount.GetValueOrDefault();
    var distribAssistReimbIveAmount =
      local.Ocse34.DistribAssistReimbIveAmount.GetValueOrDefault();
    var distribAssistReimbFmrAmount =
      local.Ocse34.DistribAssistReimbFmrAmount.GetValueOrDefault();
    var distribAssistReimbAmount =
      local.Ocse34.DistribAssistReimbAmount.GetValueOrDefault();
    var distributedMedSupportIvaAmt =
      local.Ocse34.DistributedMedSupportIvaAmt.GetValueOrDefault();
    var distributedMedSupportIveAmt =
      local.Ocse34.DistributedMedSupportIveAmt.GetValueOrDefault();
    var distributedMedSupportFmrAmt =
      local.Ocse34.DistributedMedSupportFmrAmt.GetValueOrDefault();
    var distributedMedSupportNvrAmt =
      local.Ocse34.DistributedMedSupportNvrAmt.GetValueOrDefault();
    var distributedMedSupportAmount =
      local.Ocse34.DistributedMedSupportAmount.GetValueOrDefault();
    var distributedFamilyIvaAmount =
      local.Ocse34.DistributedFamilyIvaAmount.GetValueOrDefault();
    var distributedFamilyIveAmount =
      local.Ocse34.DistributedFamilyIveAmount.GetValueOrDefault();
    var distributedFamilyFormerAmt =
      local.Ocse34.DistributedFamilyFormerAmt.GetValueOrDefault();
    var distributedFamilyNeverAmount =
      local.Ocse34.DistributedFamilyNeverAmount.GetValueOrDefault();
    var distributedFamilyAmount =
      local.Ocse34.DistributedFamilyAmount.GetValueOrDefault();
    var totalDistributedIvaAmount =
      local.Ocse34.TotalDistributedIvaAmount.GetValueOrDefault();
    var totalDistributedIveAmount =
      local.Ocse34.TotalDistributedIveAmount.GetValueOrDefault();
    var totalDistributedFormerAmount =
      local.Ocse34.TotalDistributedFormerAmount.GetValueOrDefault();
    var totalDistributedNeverAmount =
      local.Ocse34.TotalDistributedNeverAmount.GetValueOrDefault();
    var totalDistributedAmount =
      local.Ocse34.TotalDistributedAmount.GetValueOrDefault();
    var grossUndistributedAmount =
      local.Ocse34.GrossUndistributedAmount.GetValueOrDefault();
    var undistributedAmount =
      local.Ocse34.UndistributedAmount.GetValueOrDefault();
    var netUndistributedAmount =
      local.Ocse34.NetUndistributedAmount.GetValueOrDefault();
    var federalShareIvaAmount =
      local.Ocse34.FederalShareIvaAmount.GetValueOrDefault();
    var federalShareIveAmount =
      local.Ocse34.FederalShareIveAmount.GetValueOrDefault();
    var federalShareFormerAmount =
      local.Ocse34.FederalShareFormerAmount.GetValueOrDefault();
    var federalShareTotalAmount =
      local.Ocse34.FederalShareTotalAmount.GetValueOrDefault();
    var incentivePaymentIvaAmount =
      local.Ocse34.IncentivePaymentIvaAmount.GetValueOrDefault();
    var incentivePaymentFormerAmount =
      local.Ocse34.IncentivePaymentFormerAmount.GetValueOrDefault();
    var incentivePaymentAmount =
      local.Ocse34.IncentivePaymentAmount.GetValueOrDefault();
    var netFederalShareIvaAmount =
      local.Ocse34.NetFederalShareIvaAmount.GetValueOrDefault();
    var netFederalShareFormerAmount =
      local.Ocse34.NetFederalShareFormerAmount.GetValueOrDefault();
    var netFederalShareAmount =
      local.Ocse34.NetFederalShareAmount.GetValueOrDefault();
    var fmapRate = import.Ocse34.FmapRate;
    var othStFmr2Amt = local.Ocse34.OthStFmr2Amt.GetValueOrDefault();
    var othStNevr2Amt = local.Ocse34.OthStNevr2Amt.GetValueOrDefault();
    var distribFmr2Amt = local.Ocse34.DistribFmr2Amt.GetValueOrDefault();
    var distMedFmr2Amt = local.Ocse34.DistMedFmr2Amt.GetValueOrDefault();
    var distMedNvr2Amt = local.Ocse34.DistMedNvr2Amt.GetValueOrDefault();
    var distFamFmr2Amt = local.Ocse34.DistFamFmr2Amt.GetValueOrDefault();
    var distFamNvr2Amt = local.Ocse34.DistFamNvr2Amt.GetValueOrDefault();
    var totDistFmr2Amt = local.Ocse34.TotDistFmr2Amt.GetValueOrDefault();
    var totDistNvr2Amt = local.Ocse34.TotDistNvr2Amt.GetValueOrDefault();
    var netUndistPndAmt = local.Ocse34.NetUndistPndAmt.GetValueOrDefault();
    var netUndistUrsAmt = local.Ocse34.NetUndistUrsAmt.GetValueOrDefault();
    var fedShrFmr2Amt = local.Ocse34.FedShrFmr2Amt.GetValueOrDefault();
    var fedShr2Amt = local.Ocse34.FedShr2Amt.GetValueOrDefault();
    var lastTwoDaysColl = (int?)import.Lda.TotalCurrency;
    var otherCountryAmt = local.Ocse34.OtherCountryAmt.GetValueOrDefault();
    var outsideIvdAmt = local.Ocse34.OutsideIvdAmt.GetValueOrDefault();
    var feesRetNeverAmt = local.Ocse34.FeesRetNeverAmt.GetValueOrDefault();
    var feesRetOtherAmt = local.Ocse34.FeesRetOtherAmt.GetValueOrDefault();
    var feesRetTotalAmt = local.Ocse34.FeesRetTotalAmt.GetValueOrDefault();
    var othCntryAmtForw = local.Ocse34.OthCntryAmtForw.GetValueOrDefault();
    var passthru4AAmt = local.Ocse34.Passthru4AAmt.GetValueOrDefault();
    var passthru4EAmt = local.Ocse34.Passthru4EAmt.GetValueOrDefault();
    var passthruFmr4AAmt = local.Ocse34.PassthruFmr4AAmt.GetValueOrDefault();
    var passthruFmr4EAmt = local.Ocse34.PassthruFmr4EAmt.GetValueOrDefault();
    var passthruTotalAmt = local.Ocse34.PassthruTotalAmt.GetValueOrDefault();

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "prevUndistAmt", previousUndistribAmount);
        db.SetInt32(command, "totCollctnsAmt", totalCollectionsAmount);
        db.SetNullableInt32(
          command, "offFedRfndAmt", offsetFederalTaxrefundAmount);
        db.SetNullableInt32(
          command, "offStateRfndAmt", offsetStateTaxRefundAmount);
        db.SetNullableInt32(command, "unempCompAmt", unemploymentCompAmount);
        db.SetNullableInt32(
          command, "adminEnforceAmt", adminstrativeEnforceAmount);
        db.
          SetNullableInt32(command, "incomeWithhldAmt", incomeWithholdingAmount);
          
        db.SetNullableInt32(command, "otherStatesAmt", otherStatesAmount);
        db.SetNullableInt32(command, "otherSourcesAmt", otherSourcesAmount);
        db.SetNullableInt32(command, "adjustmentsAmt", adjustmentsAmount);
        db.SetNullableInt32(command, "non4DCasesAmt", nonIvdCasesAmount);
        db.SetNullableInt32(
          command, "othrState4AAmt", otherStatesCurrentIvaAmount);
        db.SetNullableInt32(
          command, "othrState4EAmt", otherStatesCurrentIveAmount);
        db.SetNullableInt32(
          command, "othrStateFmrAmt", otherstateFormerAssistAmount);
        db.SetNullableInt32(
          command, "othStateNevrAmt", otherStateNeverAssistAmount);
        db.SetNullableInt32(command, "otherStAmtForw", otherStateAmtForward);
        db.SetNullableInt32(
          command, "availDistribAmt", availForDistributionAmount);
        db.
          SetNullableInt32(command, "distrib4AAmt", distribAssistReimbIvaAmount);
          
        db.
          SetNullableInt32(command, "distrib4EAmt", distribAssistReimbIveAmount);
          
        db.SetNullableInt32(
          command, "distribFmrAmt", distribAssistReimbFmrAmount);
        db.SetNullableInt32(command, "distribAmt", distribAssistReimbAmount);
        db.SetNullableInt32(
          command, "distribMed4AAmt", distributedMedSupportIvaAmt);
        db.SetNullableInt32(
          command, "distribMed4EAmt", distributedMedSupportIveAmt);
        db.SetNullableInt32(
          command, "distrbMedFmrAmt", distributedMedSupportFmrAmt);
        db.SetNullableInt32(
          command, "distrbMedNvrAmt", distributedMedSupportNvrAmt);
        db.SetNullableInt32(
          command, "distribMedAmt", distributedMedSupportAmount);
        db.SetNullableInt32(
          command, "distribFam4AAmt", distributedFamilyIvaAmount);
        db.SetNullableInt32(
          command, "distribFam4EAmt", distributedFamilyIveAmount);
        db.SetNullableInt32(
          command, "distrbFamFmrAmt", distributedFamilyFormerAmt);
        db.SetNullableInt32(
          command, "distrbFamNvrAmt", distributedFamilyNeverAmount);
        db.SetNullableInt32(command, "distrbFamAmt", distributedFamilyAmount);
        db.SetNullableInt32(
          command, "totDistrib4AAmt", totalDistributedIvaAmount);
        db.SetNullableInt32(
          command, "totDistrib4EAmt", totalDistributedIveAmount);
        db.SetNullableInt32(
          command, "totDistribFmrAm", totalDistributedFormerAmount);
        db.SetNullableInt32(
          command, "totDistrbNvrAmt", totalDistributedNeverAmount);
        db.SetNullableInt32(command, "totDistrbAmt", totalDistributedAmount);
        db.SetNullableInt32(
          command, "grossUndistrbAmt", grossUndistributedAmount);
        db.SetNullableInt32(command, "undistribAmt", undistributedAmount);
        db.SetNullableInt32(command, "netUndistribAmt", netUndistributedAmount);
        db.SetNullableInt32(command, "fedShr4AAmt", federalShareIvaAmount);
        db.SetNullableInt32(command, "fedShr4EAmt", federalShareIveAmount);
        db.SetNullableInt32(command, "fedShrFmrAmt", federalShareFormerAmount);
        db.SetNullableInt32(command, "fedShrAmt", federalShareTotalAmount);
        db.
          SetNullableInt32(command, "incentPay4AAmt", incentivePaymentIvaAmount);
          
        db.SetNullableInt32(
          command, "incentPayFmrAmt", incentivePaymentFormerAmount);
        db.SetNullableInt32(command, "incentPayAmt", incentivePaymentAmount);
        db.
          SetNullableInt32(command, "netFedShr4AAmt", netFederalShareIvaAmount);
          
        db.SetNullableInt32(
          command, "netFedshrFmrAmt", netFederalShareFormerAmount);
        db.SetNullableInt32(command, "netFedShrAmt", netFederalShareAmount);
        db.SetDecimal(command, "fmapRate", fmapRate);
        db.SetNullableInt32(command, "othStFmr2Amt", othStFmr2Amt);
        db.SetNullableInt32(command, "othStNevr2Amt", othStNevr2Amt);
        db.SetNullableInt32(command, "distribFmr2Amt", distribFmr2Amt);
        db.SetNullableInt32(command, "distMedFmr2Amt", distMedFmr2Amt);
        db.SetNullableInt32(command, "distMedNvr2Amt", distMedNvr2Amt);
        db.SetNullableInt32(command, "distFamFmr2Amt", distFamFmr2Amt);
        db.SetNullableInt32(command, "distFamNvr2Amt", distFamNvr2Amt);
        db.SetNullableInt32(command, "totDistFmr2Amt", totDistFmr2Amt);
        db.SetNullableInt32(command, "totDistNvr2Amt", totDistNvr2Amt);
        db.SetNullableInt32(command, "netUndistPndAmt", netUndistPndAmt);
        db.SetNullableInt32(command, "netUndistUrsAmt", netUndistUrsAmt);
        db.SetNullableInt32(command, "fedShrFmr2Amt", fedShrFmr2Amt);
        db.SetNullableInt32(command, "fedShr2Amt", fedShr2Amt);
        db.SetNullableInt32(command, "lastTwoDaysColl", lastTwoDaysColl);
        db.SetNullableInt32(command, "otherCountryAmt", otherCountryAmt);
        db.SetNullableInt32(command, "outsideIvdAmt", outsideIvdAmt);
        db.SetNullableInt32(command, "feesRetNeverAmt", feesRetNeverAmt);
        db.SetNullableInt32(command, "feesRetOtherAmt", feesRetOtherAmt);
        db.SetNullableInt32(command, "feesRetTotalAmt", feesRetTotalAmt);
        db.SetNullableInt32(command, "othCntryAmtForw", othCntryAmtForw);
        db.SetNullableInt32(command, "passthru4AAmt", passthru4AAmt);
        db.SetNullableInt32(command, "passthru4EAmt", passthru4EAmt);
        db.SetNullableInt32(command, "passthruFmr4AAmt", passthruFmr4AAmt);
        db.SetNullableInt32(command, "passthruFmr4EAmt", passthruFmr4EAmt);
        db.SetNullableInt32(command, "passthruTotalAmt", passthruTotalAmt);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.PreviousUndistribAmount = previousUndistribAmount;
    entities.Ocse34.TotalCollectionsAmount = totalCollectionsAmount;
    entities.Ocse34.OffsetFederalTaxrefundAmount = offsetFederalTaxrefundAmount;
    entities.Ocse34.OffsetStateTaxRefundAmount = offsetStateTaxRefundAmount;
    entities.Ocse34.UnemploymentCompAmount = unemploymentCompAmount;
    entities.Ocse34.AdminstrativeEnforceAmount = adminstrativeEnforceAmount;
    entities.Ocse34.IncomeWithholdingAmount = incomeWithholdingAmount;
    entities.Ocse34.OtherStatesAmount = otherStatesAmount;
    entities.Ocse34.OtherSourcesAmount = otherSourcesAmount;
    entities.Ocse34.AdjustmentsAmount = adjustmentsAmount;
    entities.Ocse34.NonIvdCasesAmount = nonIvdCasesAmount;
    entities.Ocse34.OtherStatesCurrentIvaAmount = otherStatesCurrentIvaAmount;
    entities.Ocse34.OtherStatesCurrentIveAmount = otherStatesCurrentIveAmount;
    entities.Ocse34.OtherstateFormerAssistAmount = otherstateFormerAssistAmount;
    entities.Ocse34.OtherStateNeverAssistAmount = otherStateNeverAssistAmount;
    entities.Ocse34.OtherStateAmtForward = otherStateAmtForward;
    entities.Ocse34.AvailForDistributionAmount = availForDistributionAmount;
    entities.Ocse34.DistribAssistReimbIvaAmount = distribAssistReimbIvaAmount;
    entities.Ocse34.DistribAssistReimbIveAmount = distribAssistReimbIveAmount;
    entities.Ocse34.DistribAssistReimbFmrAmount = distribAssistReimbFmrAmount;
    entities.Ocse34.DistribAssistReimbAmount = distribAssistReimbAmount;
    entities.Ocse34.DistributedMedSupportIvaAmt = distributedMedSupportIvaAmt;
    entities.Ocse34.DistributedMedSupportIveAmt = distributedMedSupportIveAmt;
    entities.Ocse34.DistributedMedSupportFmrAmt = distributedMedSupportFmrAmt;
    entities.Ocse34.DistributedMedSupportNvrAmt = distributedMedSupportNvrAmt;
    entities.Ocse34.DistributedMedSupportAmount = distributedMedSupportAmount;
    entities.Ocse34.DistributedFamilyIvaAmount = distributedFamilyIvaAmount;
    entities.Ocse34.DistributedFamilyIveAmount = distributedFamilyIveAmount;
    entities.Ocse34.DistributedFamilyFormerAmt = distributedFamilyFormerAmt;
    entities.Ocse34.DistributedFamilyNeverAmount = distributedFamilyNeverAmount;
    entities.Ocse34.DistributedFamilyAmount = distributedFamilyAmount;
    entities.Ocse34.TotalDistributedIvaAmount = totalDistributedIvaAmount;
    entities.Ocse34.TotalDistributedIveAmount = totalDistributedIveAmount;
    entities.Ocse34.TotalDistributedFormerAmount = totalDistributedFormerAmount;
    entities.Ocse34.TotalDistributedNeverAmount = totalDistributedNeverAmount;
    entities.Ocse34.TotalDistributedAmount = totalDistributedAmount;
    entities.Ocse34.GrossUndistributedAmount = grossUndistributedAmount;
    entities.Ocse34.UndistributedAmount = undistributedAmount;
    entities.Ocse34.NetUndistributedAmount = netUndistributedAmount;
    entities.Ocse34.FederalShareIvaAmount = federalShareIvaAmount;
    entities.Ocse34.FederalShareIveAmount = federalShareIveAmount;
    entities.Ocse34.FederalShareFormerAmount = federalShareFormerAmount;
    entities.Ocse34.FederalShareTotalAmount = federalShareTotalAmount;
    entities.Ocse34.IncentivePaymentIvaAmount = incentivePaymentIvaAmount;
    entities.Ocse34.IncentivePaymentFormerAmount = incentivePaymentFormerAmount;
    entities.Ocse34.IncentivePaymentAmount = incentivePaymentAmount;
    entities.Ocse34.NetFederalShareIvaAmount = netFederalShareIvaAmount;
    entities.Ocse34.NetFederalShareFormerAmount = netFederalShareFormerAmount;
    entities.Ocse34.NetFederalShareAmount = netFederalShareAmount;
    entities.Ocse34.FmapRate = fmapRate;
    entities.Ocse34.OthStFmr2Amt = othStFmr2Amt;
    entities.Ocse34.OthStNevr2Amt = othStNevr2Amt;
    entities.Ocse34.DistribFmr2Amt = distribFmr2Amt;
    entities.Ocse34.DistMedFmr2Amt = distMedFmr2Amt;
    entities.Ocse34.DistMedNvr2Amt = distMedNvr2Amt;
    entities.Ocse34.DistFamFmr2Amt = distFamFmr2Amt;
    entities.Ocse34.DistFamNvr2Amt = distFamNvr2Amt;
    entities.Ocse34.TotDistFmr2Amt = totDistFmr2Amt;
    entities.Ocse34.TotDistNvr2Amt = totDistNvr2Amt;
    entities.Ocse34.NetUndistPndAmt = netUndistPndAmt;
    entities.Ocse34.NetUndistUrsAmt = netUndistUrsAmt;
    entities.Ocse34.FedShrFmr2Amt = fedShrFmr2Amt;
    entities.Ocse34.FedShr2Amt = fedShr2Amt;
    entities.Ocse34.LastTwoDaysColl = lastTwoDaysColl;
    entities.Ocse34.OtherCountryAmt = otherCountryAmt;
    entities.Ocse34.OutsideIvdAmt = outsideIvdAmt;
    entities.Ocse34.FeesRetNeverAmt = feesRetNeverAmt;
    entities.Ocse34.FeesRetOtherAmt = feesRetOtherAmt;
    entities.Ocse34.FeesRetTotalAmt = feesRetTotalAmt;
    entities.Ocse34.OthCntryAmtForw = othCntryAmtForw;
    entities.Ocse34.Passthru4AAmt = passthru4AAmt;
    entities.Ocse34.Passthru4EAmt = passthru4EAmt;
    entities.Ocse34.PassthruFmr4AAmt = passthruFmr4AAmt;
    entities.Ocse34.PassthruFmr4EAmt = passthruFmr4EAmt;
    entities.Ocse34.PassthruTotalAmt = passthruTotalAmt;
    entities.Ocse34.Populated = true;
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
    /// A value of Lda.
    /// </summary>
    [JsonPropertyName("lda")]
    public Common Lda
    {
      get => lda ??= new();
      set => lda = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
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

    private Common lda;
    private Ocse34 ocse34;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
