// Program: SI_CHECK_AP_CURR_HIST, ID: 372497574, model: 746.
// Short name: SWE01115
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHECK_AP_CURR_HIST.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This common action block will read the CSENET_AP_LOCATE, 
/// CSENET_AP_LOCATE_ADDRESS, and CSENET_AP_ID entity to set two indicators
/// identifying whether or not attributes for the AP current or history screen
/// are populated.
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckApCurrHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_AP_CURR_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckApCurrHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckApCurrHist.
  /// </summary>
  public SiCheckApCurrHist(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadInterstateApLocate())
    {
      // ---------------------------------------------
      // The following If Clause is checking to see if
      // AP History Screen Attributes are present on
      // the CSENET_AP_LOCATE entity.  If any
      // attribute is present, the AP_History_Ind will
      // be set to "Y".
      // ---------------------------------------------
      if (!IsEmpty(entities.InterstateApLocate.Alias1FirstName) || !
        IsEmpty(entities.InterstateApLocate.Alias1LastName) || !
        IsEmpty(entities.InterstateApLocate.Alias1MiddleName) || !
        IsEmpty(entities.InterstateApLocate.Alias2FirstName) || !
        IsEmpty(entities.InterstateApLocate.Alias2LastName) || !
        IsEmpty(entities.InterstateApLocate.Alias2MiddleName) || !
        IsEmpty(entities.InterstateApLocate.Alias3FirstName) || !
        IsEmpty(entities.InterstateApLocate.Alias3LastName) || !
        IsEmpty(entities.InterstateApLocate.Alias3MiddleName) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerName) || entities
        .InterstateApLocate.LastEmployerDate != null || !
        IsEmpty(entities.InterstateApLocate.LastEmployerAddressLine1) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerAddressLine2) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerCity) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerState) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerZipCode4) || !
        IsEmpty(entities.InterstateApLocate.LastEmployerZipCode5) || entities
        .InterstateApLocate.LastMailAddressDate != null || !
        IsEmpty(entities.InterstateApLocate.LastMailAddressLine1) || !
        IsEmpty(entities.InterstateApLocate.LastMailAddressLine2) || !
        IsEmpty(entities.InterstateApLocate.LastMailCity) || !
        IsEmpty(entities.InterstateApLocate.LastMailZipCode4) || !
        IsEmpty(entities.InterstateApLocate.LastMailZipCode5) || entities
        .InterstateApLocate.LastResAddressDate != null || !
        IsEmpty(entities.InterstateApLocate.LastResAddressLine1) || !
        IsEmpty(entities.InterstateApLocate.LastResAddressLine2) || !
        IsEmpty(entities.InterstateApLocate.LastResCity) || !
        IsEmpty(entities.InterstateApLocate.LastResState) || !
        IsEmpty(entities.InterstateApLocate.LastResZipCode4) || !
        IsEmpty(entities.InterstateApLocate.LastResZipCode5))
      {
        export.ApHistoryInd.Flag = "Y";

        // ---------------------------------------------
        // The following If Clause is checking to see if
        // AP Current Screen Attributes are present on
        // the CSENET_AP_LOCATE entity.  If any
        // attribute is present, the AP_Current_Ind will
        // be set to "Y".
        // ---------------------------------------------
        if (!IsEmpty(entities.InterstateApLocate.CurrentSpouseFirstName) || !
          IsEmpty(entities.InterstateApLocate.CurrentSpouseLastName) || !
          IsEmpty(entities.InterstateApLocate.CurrentSpouseMiddleName) || !
          IsEmpty(entities.InterstateApLocate.DriversLicenseNum) || !
          IsEmpty(entities.InterstateApLocate.DriversLicState) || !
          IsEmpty(entities.InterstateApLocate.EmployerConfirmedInd) || entities
          .InterstateApLocate.EmployerEffectiveDate != null || Lt
          (0, entities.InterstateApLocate.EmployerEin) || entities
          .InterstateApLocate.EmployerEndDate != null || !
          IsEmpty(entities.InterstateApLocate.EmployerName) || Lt
          (0, entities.InterstateApLocate.EmployerPhoneNum) || Lt
          (0, entities.InterstateApLocate.HomePhoneNumber) || !
          IsEmpty(entities.InterstateApLocate.InsuranceCarrierName) || !
          IsEmpty(entities.InterstateApLocate.InsurancePolicyNum) || !
          IsEmpty(entities.InterstateApLocate.Occupation) || Lt
          (0, entities.InterstateApLocate.WageAmount) || Lt
          (0, entities.InterstateApLocate.WageQtr) || Lt
          (0, entities.InterstateApLocate.WageYear) || Lt
          (0, entities.InterstateApLocate.WorkPhoneNumber) || !
          IsEmpty(entities.InterstateApLocate.MailingAddressConfirmedInd) || entities
          .InterstateApLocate.MailingAddressEffectiveDate != null || entities
          .InterstateApLocate.ResidentialAddressEndDate != null || !
          IsEmpty(entities.InterstateApLocate.ResidentialAddressLine1) || !
          IsEmpty(entities.InterstateApLocate.ResidentialAddressLine2) || !
          IsEmpty(entities.InterstateApLocate.MailingCity) || !
          IsEmpty(entities.InterstateApLocate.MailingState) || !
          IsEmpty(entities.InterstateApLocate.MailingZipCode4) || !
          IsEmpty(entities.InterstateApLocate.MailingZipCode5) || !
          IsEmpty(entities.InterstateApLocate.ResidentialAddressConfirmInd) || entities
          .InterstateApLocate.ResidentialAddressEffectvDate != null || entities
          .InterstateApLocate.ResidentialAddressEndDate != null || !
          IsEmpty(entities.InterstateApLocate.ResidentialAddressLine1) || !
          IsEmpty(entities.InterstateApLocate.ResidentialAddressLine2) || !
          IsEmpty(entities.InterstateApLocate.ResidentialCity) || !
          IsEmpty(entities.InterstateApLocate.ResidentialState) || !
          IsEmpty(entities.InterstateApLocate.ResidentialZipCode4) || !
          IsEmpty(entities.InterstateApLocate.ResidentialZipCode5) || !
          IsEmpty(entities.InterstateApLocate.EmployerAddressLine1) || !
          IsEmpty(entities.InterstateApLocate.EmployerAddressLine2) || !
          IsEmpty(entities.InterstateApLocate.EmployerCity) || !
          IsEmpty(entities.InterstateApLocate.EmployerState) || !
          IsEmpty(entities.InterstateApLocate.EmployerZipCode5) || !
          IsEmpty(entities.InterstateApLocate.EmployerZipCode4))
        {
          export.ApCurrentInd.Flag = "Y";
        }
      }
    }
    else
    {
      ExitState = "CSENET_AP_LOCATE_NF";
    }
  }

  private bool ReadInterstateApLocate()
  {
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocate",
      (db, command) =>
      {
        db.SetInt64(
          command, "cncTransSerlNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "cncTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApLocate.EmployerEin =
          db.GetNullableInt32(reader, 2);
        entities.InterstateApLocate.EmployerName =
          db.GetNullableString(reader, 3);
        entities.InterstateApLocate.EmployerPhoneNum =
          db.GetNullableInt32(reader, 4);
        entities.InterstateApLocate.EmployerEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateApLocate.EmployerEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateApLocate.EmployerConfirmedInd =
          db.GetNullableString(reader, 7);
        entities.InterstateApLocate.ResidentialAddressLine1 =
          db.GetNullableString(reader, 8);
        entities.InterstateApLocate.ResidentialAddressLine2 =
          db.GetNullableString(reader, 9);
        entities.InterstateApLocate.ResidentialCity =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.ResidentialState =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.ResidentialZipCode5 =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.ResidentialZipCode4 =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.MailingAddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.MailingAddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.MailingCity =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.MailingState =
          db.GetNullableString(reader, 17);
        entities.InterstateApLocate.MailingZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.MailingZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.ResidentialAddressEffectvDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateApLocate.ResidentialAddressEndDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateApLocate.ResidentialAddressConfirmInd =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.MailingAddressEffectiveDate =
          db.GetNullableDate(reader, 23);
        entities.InterstateApLocate.MailingAddressEndDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateApLocate.MailingAddressConfirmedInd =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.HomePhoneNumber =
          db.GetNullableInt32(reader, 26);
        entities.InterstateApLocate.WorkPhoneNumber =
          db.GetNullableInt32(reader, 27);
        entities.InterstateApLocate.DriversLicState =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.DriversLicenseNum =
          db.GetNullableString(reader, 29);
        entities.InterstateApLocate.Alias1FirstName =
          db.GetNullableString(reader, 30);
        entities.InterstateApLocate.Alias1MiddleName =
          db.GetNullableString(reader, 31);
        entities.InterstateApLocate.Alias1LastName =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.Alias2FirstName =
          db.GetNullableString(reader, 33);
        entities.InterstateApLocate.Alias2MiddleName =
          db.GetNullableString(reader, 34);
        entities.InterstateApLocate.Alias2LastName =
          db.GetNullableString(reader, 35);
        entities.InterstateApLocate.Alias3FirstName =
          db.GetNullableString(reader, 36);
        entities.InterstateApLocate.Alias3MiddleName =
          db.GetNullableString(reader, 37);
        entities.InterstateApLocate.Alias3LastName =
          db.GetNullableString(reader, 38);
        entities.InterstateApLocate.CurrentSpouseFirstName =
          db.GetNullableString(reader, 39);
        entities.InterstateApLocate.CurrentSpouseMiddleName =
          db.GetNullableString(reader, 40);
        entities.InterstateApLocate.CurrentSpouseLastName =
          db.GetNullableString(reader, 41);
        entities.InterstateApLocate.Occupation =
          db.GetNullableString(reader, 42);
        entities.InterstateApLocate.EmployerAddressLine1 =
          db.GetNullableString(reader, 43);
        entities.InterstateApLocate.EmployerAddressLine2 =
          db.GetNullableString(reader, 44);
        entities.InterstateApLocate.EmployerCity =
          db.GetNullableString(reader, 45);
        entities.InterstateApLocate.EmployerState =
          db.GetNullableString(reader, 46);
        entities.InterstateApLocate.EmployerZipCode5 =
          db.GetNullableString(reader, 47);
        entities.InterstateApLocate.EmployerZipCode4 =
          db.GetNullableString(reader, 48);
        entities.InterstateApLocate.WageQtr = db.GetNullableInt32(reader, 49);
        entities.InterstateApLocate.WageYear = db.GetNullableInt32(reader, 50);
        entities.InterstateApLocate.WageAmount =
          db.GetNullableDecimal(reader, 51);
        entities.InterstateApLocate.InsuranceCarrierName =
          db.GetNullableString(reader, 52);
        entities.InterstateApLocate.InsurancePolicyNum =
          db.GetNullableString(reader, 53);
        entities.InterstateApLocate.LastResAddressLine1 =
          db.GetNullableString(reader, 54);
        entities.InterstateApLocate.LastResAddressLine2 =
          db.GetNullableString(reader, 55);
        entities.InterstateApLocate.LastResCity =
          db.GetNullableString(reader, 56);
        entities.InterstateApLocate.LastResState =
          db.GetNullableString(reader, 57);
        entities.InterstateApLocate.LastResZipCode5 =
          db.GetNullableString(reader, 58);
        entities.InterstateApLocate.LastResZipCode4 =
          db.GetNullableString(reader, 59);
        entities.InterstateApLocate.LastResAddressDate =
          db.GetNullableDate(reader, 60);
        entities.InterstateApLocate.LastMailAddressLine1 =
          db.GetNullableString(reader, 61);
        entities.InterstateApLocate.LastMailAddressLine2 =
          db.GetNullableString(reader, 62);
        entities.InterstateApLocate.LastMailCity =
          db.GetNullableString(reader, 63);
        entities.InterstateApLocate.LastMailState =
          db.GetNullableString(reader, 64);
        entities.InterstateApLocate.LastMailZipCode5 =
          db.GetNullableString(reader, 65);
        entities.InterstateApLocate.LastMailZipCode4 =
          db.GetNullableString(reader, 66);
        entities.InterstateApLocate.LastMailAddressDate =
          db.GetNullableDate(reader, 67);
        entities.InterstateApLocate.LastEmployerName =
          db.GetNullableString(reader, 68);
        entities.InterstateApLocate.LastEmployerDate =
          db.GetNullableDate(reader, 69);
        entities.InterstateApLocate.LastEmployerAddressLine1 =
          db.GetNullableString(reader, 70);
        entities.InterstateApLocate.LastEmployerAddressLine2 =
          db.GetNullableString(reader, 71);
        entities.InterstateApLocate.LastEmployerCity =
          db.GetNullableString(reader, 72);
        entities.InterstateApLocate.LastEmployerState =
          db.GetNullableString(reader, 73);
        entities.InterstateApLocate.LastEmployerZipCode5 =
          db.GetNullableString(reader, 74);
        entities.InterstateApLocate.LastEmployerZipCode4 =
          db.GetNullableString(reader, 75);
        entities.InterstateApLocate.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ApCurrentInd.
    /// </summary>
    [JsonPropertyName("apCurrentInd")]
    public Common ApCurrentInd
    {
      get => apCurrentInd ??= new();
      set => apCurrentInd = value;
    }

    /// <summary>
    /// A value of ApHistoryInd.
    /// </summary>
    [JsonPropertyName("apHistoryInd")]
    public Common ApHistoryInd
    {
      get => apHistoryInd ??= new();
      set => apHistoryInd = value;
    }

    private Common apCurrentInd;
    private Common apHistoryInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
  }
#endregion
}
