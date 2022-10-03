// Program: SI_READ_PA_REFERRAL_2, ID: 371760611, model: 746.
// Short name: SWE01230
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_PA_REFERRAL_2.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAB performs the I/O functions that actually retrieve data to populate 
/// the PA Referral (Screen2)
/// </para>
/// </summary>
[Serializable]
public partial class SiReadPaReferral2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_PA_REFERRAL_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadPaReferral2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadPaReferral2.
  /// </summary>
  public SiReadPaReferral2(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //      M A I N T E N A N C E   L O G
    //  Date    Developer   Description
    // 3-01-95  J.W. Hays   Initial development
    // 8-29-95  Ken Evans   Continue development
    // ---------------------------------------------
    // 06/29/99 M.Lachowicz Change property of READ
    //                      (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 06/29/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadPaReferral())
    {
      export.PaReferral.Assign(entities.ExistingPaReferral);
    }
    else
    {
      ExitState = "PA_REFERRAL_NF";

      return;
    }

    // 06/29/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadPaParticipantAddress())
    {
      export.Employer.Assign(entities.ExistingPaParticipantAddress);
    }
  }

  private bool ReadPaParticipantAddress()
  {
    entities.ExistingPaParticipantAddress.Populated = false;

    return Read("ReadPaParticipantAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.ExistingPaReferral.Number);
        db.SetString(command, "pafType", entities.ExistingPaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingPaParticipantAddress.Type1 =
          db.GetNullableString(reader, 0);
        entities.ExistingPaParticipantAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.ExistingPaParticipantAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingPaParticipantAddress.City =
          db.GetNullableString(reader, 3);
        entities.ExistingPaParticipantAddress.State =
          db.GetNullableString(reader, 4);
        entities.ExistingPaParticipantAddress.Zip =
          db.GetNullableString(reader, 5);
        entities.ExistingPaParticipantAddress.Zip4 =
          db.GetNullableString(reader, 6);
        entities.ExistingPaParticipantAddress.Identifier =
          db.GetInt32(reader, 7);
        entities.ExistingPaParticipantAddress.PrpIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingPaParticipantAddress.PafType = db.GetString(reader, 9);
        entities.ExistingPaParticipantAddress.PreNumber =
          db.GetString(reader, 10);
        entities.ExistingPaParticipantAddress.PafTstamp =
          db.GetDateTime(reader, 11);
        entities.ExistingPaParticipantAddress.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.ExistingPaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetString(command, "type", import.PaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.Populated = true;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    private PaReferral paReferral;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public PaParticipantAddress Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    private PaParticipantAddress employer;
    private PaReferral paReferral;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPaReferralParticipant.
    /// </summary>
    [JsonPropertyName("existingPaReferralParticipant")]
    public PaReferralParticipant ExistingPaReferralParticipant
    {
      get => existingPaReferralParticipant ??= new();
      set => existingPaReferralParticipant = value;
    }

    /// <summary>
    /// A value of ExistingPaParticipantAddress.
    /// </summary>
    [JsonPropertyName("existingPaParticipantAddress")]
    public PaParticipantAddress ExistingPaParticipantAddress
    {
      get => existingPaParticipantAddress ??= new();
      set => existingPaParticipantAddress = value;
    }

    /// <summary>
    /// A value of ExistingPaReferral.
    /// </summary>
    [JsonPropertyName("existingPaReferral")]
    public PaReferral ExistingPaReferral
    {
      get => existingPaReferral ??= new();
      set => existingPaReferral = value;
    }

    private PaReferralParticipant existingPaReferralParticipant;
    private PaParticipantAddress existingPaParticipantAddress;
    private PaReferral existingPaReferral;
  }
#endregion
}
