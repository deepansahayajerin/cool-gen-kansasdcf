// Program: SI_READ_CSENET_AP_LOCATE, ID: 372542410, model: 746.
// Short name: SWE01213
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_CSENET_AP_LOCATE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsenetApLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_AP_LOCATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetApLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetApLocate.
  /// </summary>
  public SiReadCsenetApLocate(IContext context, Import import, Export export):
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
      export.InterstateApLocate.Assign(entities.InterstateApLocate);
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
        entities.InterstateApLocate.HomePhoneNumber =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApLocate.WorkPhoneNumber =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApLocate.DriversLicState =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.DriversLicenseNum =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.Alias1FirstName =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.Alias1MiddleName =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.Alias1LastName =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.Alias1Suffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.Alias2FirstName =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.Alias2MiddleName =
          db.GetNullableString(reader, 17);
        entities.InterstateApLocate.Alias2LastName =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.Alias2Suffix =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.Alias3FirstName =
          db.GetNullableString(reader, 20);
        entities.InterstateApLocate.Alias3MiddleName =
          db.GetNullableString(reader, 21);
        entities.InterstateApLocate.Alias3LastName =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.Alias3Suffix =
          db.GetNullableString(reader, 23);
        entities.InterstateApLocate.CurrentSpouseFirstName =
          db.GetNullableString(reader, 24);
        entities.InterstateApLocate.CurrentSpouseMiddleName =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.CurrentSpouseLastName =
          db.GetNullableString(reader, 26);
        entities.InterstateApLocate.CurrentSpouseSuffix =
          db.GetNullableString(reader, 27);
        entities.InterstateApLocate.Occupation =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.WageQtr = db.GetNullableInt32(reader, 29);
        entities.InterstateApLocate.WageYear = db.GetNullableInt32(reader, 30);
        entities.InterstateApLocate.WageAmount =
          db.GetNullableDecimal(reader, 31);
        entities.InterstateApLocate.InsuranceCarrierName =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.InsurancePolicyNum =
          db.GetNullableString(reader, 33);
        entities.InterstateApLocate.LastEmployerName =
          db.GetNullableString(reader, 34);
        entities.InterstateApLocate.LastEmployerDate =
          db.GetNullableDate(reader, 35);
        entities.InterstateApLocate.WorkAreaCode =
          db.GetNullableInt32(reader, 36);
        entities.InterstateApLocate.HomeAreaCode =
          db.GetNullableInt32(reader, 37);
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
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    private InterstateApLocate interstateApLocate;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private InterstateApLocate interstateApLocate;
  }
#endregion
}
