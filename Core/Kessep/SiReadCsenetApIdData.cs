// Program: SI_READ_CSENET_AP_ID_DATA, ID: 372513213, model: 746.
// Short name: SWE01212
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_CSENET_AP_ID_DATA.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAB performs the retrieval functions that populate the CSENet AP Id 
/// Data screen.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsenetApIdData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_AP_ID_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetApIdData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetApIdData.
  /// </summary>
  public SiReadCsenetApIdData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // * This PAB performs the READ's that populate*
    // * the CSENet AP Id screen.                  *
    // *********************************************
    if (ReadInterstateApIdentificationInterstateCase())
    {
      export.InterstateApIdentification.Assign(
        entities.InterstateApIdentification);
      export.InterstateCase.Assign(entities.InterstateCase);
    }
    else
    {
      ExitState = "CSENET_AP_ID_NF";
    }
  }

  private bool ReadInterstateApIdentificationInterstateCase()
  {
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateApIdentificationInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.PossiblyDangerous =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.MaidenName =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.MothersMaidenOrFathersName =
          db.GetNullableString(reader, 21);
        entities.InterstateCase.ActionCode = db.GetString(reader, 22);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 23);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 24);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 25);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 26);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 27);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 28);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 29);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 30);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 31);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 32);
        entities.InterstateCase.CaseType = db.GetString(reader, 33);
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateCase.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
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

    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
  }
#endregion
}
