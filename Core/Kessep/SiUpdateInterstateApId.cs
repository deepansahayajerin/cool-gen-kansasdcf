// Program: SI_UPDATE_INTERSTATE_AP_ID, ID: 373441309, model: 746.
// Short name: SWE02756
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_INTERSTATE_AP_ID.
/// </summary>
[Serializable]
public partial class SiUpdateInterstateApId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INTERSTATE_AP_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateInterstateApId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateInterstateApId.
  /// </summary>
  public SiUpdateInterstateApId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadInterstateApIdentification())
    {
      try
      {
        UpdateInterstateApIdentification();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_AP_IDENT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTATE_AP_IDENT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else if (ReadInterstateCase())
    {
      ExitState = "CSENET_AP_ID_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
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
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
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
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private void UpdateInterstateApIdentification()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateApIdentification.Populated);

    var aliasSsn2 = import.InterstateApIdentification.AliasSsn2 ?? "";
    var aliasSsn1 = import.InterstateApIdentification.AliasSsn1 ?? "";
    var otherIdInfo = import.InterstateApIdentification.OtherIdInfo ?? "";
    var eyeColor = import.InterstateApIdentification.EyeColor ?? "";
    var hairColor = import.InterstateApIdentification.HairColor ?? "";
    var weight = import.InterstateApIdentification.Weight.GetValueOrDefault();
    var heightIn =
      import.InterstateApIdentification.HeightIn.GetValueOrDefault();
    var heightFt =
      import.InterstateApIdentification.HeightFt.GetValueOrDefault();
    var placeOfBirth = import.InterstateApIdentification.PlaceOfBirth ?? "";
    var ssn = import.InterstateApIdentification.Ssn ?? "";
    var race = import.InterstateApIdentification.Race ?? "";
    var sex = import.InterstateApIdentification.Sex ?? "";
    var dateOfBirth = import.InterstateApIdentification.DateOfBirth;
    var nameSuffix = import.InterstateApIdentification.NameSuffix ?? "";
    var nameFirst = import.InterstateApIdentification.NameFirst;
    var nameLast = import.InterstateApIdentification.NameLast ?? "";
    var middleName = import.InterstateApIdentification.MiddleName ?? "";
    var possiblyDangerous =
      import.InterstateApIdentification.PossiblyDangerous ?? "";
    var maidenName = import.InterstateApIdentification.MaidenName ?? "";
    var mothersMaidenOrFathersName =
      import.InterstateApIdentification.MothersMaidenOrFathersName ?? "";

    entities.InterstateApIdentification.Populated = false;
    Update("UpdateInterstateApIdentification",
      (db, command) =>
      {
        db.SetNullableString(command, "altSsn2", aliasSsn2);
        db.SetNullableString(command, "altSsn1", aliasSsn1);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableString(command, "birthPlaceCity", placeOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableDate(command, "birthDate", dateOfBirth);
        db.SetNullableString(command, "suffix", nameSuffix);
        db.SetString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "possiblyDangerous", possiblyDangerous);
        db.SetNullableString(command, "maidenName", maidenName);
        db.SetNullableString(
          command, "mthMaidOrFathN", mothersMaidenOrFathersName);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateApIdentification.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateApIdentification.CcaTransSerNum);
      });

    entities.InterstateApIdentification.AliasSsn2 = aliasSsn2;
    entities.InterstateApIdentification.AliasSsn1 = aliasSsn1;
    entities.InterstateApIdentification.OtherIdInfo = otherIdInfo;
    entities.InterstateApIdentification.EyeColor = eyeColor;
    entities.InterstateApIdentification.HairColor = hairColor;
    entities.InterstateApIdentification.Weight = weight;
    entities.InterstateApIdentification.HeightIn = heightIn;
    entities.InterstateApIdentification.HeightFt = heightFt;
    entities.InterstateApIdentification.PlaceOfBirth = placeOfBirth;
    entities.InterstateApIdentification.Ssn = ssn;
    entities.InterstateApIdentification.Race = race;
    entities.InterstateApIdentification.Sex = sex;
    entities.InterstateApIdentification.DateOfBirth = dateOfBirth;
    entities.InterstateApIdentification.NameSuffix = nameSuffix;
    entities.InterstateApIdentification.NameFirst = nameFirst;
    entities.InterstateApIdentification.NameLast = nameLast;
    entities.InterstateApIdentification.MiddleName = middleName;
    entities.InterstateApIdentification.PossiblyDangerous = possiblyDangerous;
    entities.InterstateApIdentification.MaidenName = maidenName;
    entities.InterstateApIdentification.MothersMaidenOrFathersName =
      mothersMaidenOrFathersName;
    entities.InterstateApIdentification.Populated = true;
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

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
