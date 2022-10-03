// Program: SI_UPDATE_INTERSTATE_PARTICIPANT, ID: 373441271, model: 746.
// Short name: SWE02747
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_INTERSTATE_PARTICIPANT.
/// </summary>
[Serializable]
public partial class SiUpdateInterstateParticipant: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INTERSTATE_PARTICIPANT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateInterstateParticipant(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateInterstateParticipant.
  /// </summary>
  public SiUpdateInterstateParticipant(IContext context, Import import,
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
    if (ReadInterstateParticipant())
    {
      try
      {
        UpdateInterstateParticipant();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_PART_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTATE_PART_PV";

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
      ExitState = "SI0000_INTERSTATE_PART_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
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

  private bool ReadInterstateParticipant()
  {
    entities.InterstateParticipant.Populated = false;

    return Read("ReadInterstateParticipant",
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
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;
      });
  }

  private void UpdateInterstateParticipant()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateParticipant.Populated);

    var nameLast = import.InterstateParticipant.NameLast ?? "";
    var nameFirst = import.InterstateParticipant.NameFirst ?? "";
    var nameMiddle = import.InterstateParticipant.NameMiddle ?? "";
    var nameSuffix = import.InterstateParticipant.NameSuffix ?? "";
    var dateOfBirth = import.InterstateParticipant.DateOfBirth;
    var ssn = import.InterstateParticipant.Ssn ?? "";
    var sex = import.InterstateParticipant.Sex ?? "";
    var race = import.InterstateParticipant.Race ?? "";
    var relationship = import.InterstateParticipant.Relationship ?? "";
    var status = import.InterstateParticipant.Status ?? "";
    var dependentRelationCp =
      import.InterstateParticipant.DependentRelationCp ?? "";
    var addressLine1 = import.InterstateParticipant.AddressLine1 ?? "";
    var addressLine2 = import.InterstateParticipant.AddressLine2 ?? "";
    var city = import.InterstateParticipant.City ?? "";
    var state = import.InterstateParticipant.State ?? "";
    var zipCode5 = import.InterstateParticipant.ZipCode5 ?? "";
    var zipCode4 = import.InterstateParticipant.ZipCode4 ?? "";
    var employerAddressLine1 =
      import.InterstateParticipant.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      import.InterstateParticipant.EmployerAddressLine2 ?? "";
    var employerCity = import.InterstateParticipant.EmployerCity ?? "";
    var employerState = import.InterstateParticipant.EmployerState ?? "";
    var employerZipCode5 = import.InterstateParticipant.EmployerZipCode5 ?? "";
    var employerZipCode4 = import.InterstateParticipant.EmployerZipCode4 ?? "";
    var employerName = import.InterstateParticipant.EmployerName ?? "";
    var employerEin =
      import.InterstateParticipant.EmployerEin.GetValueOrDefault();
    var addressVerifiedDate = import.InterstateParticipant.AddressVerifiedDate;
    var employerVerifiedDate =
      import.InterstateParticipant.EmployerVerifiedDate;
    var workPhone = import.InterstateParticipant.WorkPhone ?? "";
    var workAreaCode = import.InterstateParticipant.WorkAreaCode ?? "";
    var placeOfBirth = import.InterstateParticipant.PlaceOfBirth ?? "";
    var childStateOfResidence =
      import.InterstateParticipant.ChildStateOfResidence ?? "";
    var childPaternityStatus =
      import.InterstateParticipant.ChildPaternityStatus ?? "";
    var employerConfirmedInd =
      import.InterstateParticipant.EmployerConfirmedInd ?? "";
    var addressConfirmedInd =
      import.InterstateParticipant.AddressConfirmedInd ?? "";

    entities.InterstateParticipant.Populated = false;
    Update("UpdateInterstateParticipant",
      (db, command) =>
      {
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameSuffix", nameSuffix);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "relationship", relationship);
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "dependentRelation", dependentRelationCp);
        db.SetNullableString(command, "addressLine1", addressLine1);
        db.SetNullableString(command, "addressLine2", addressLine2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "empAddressLine1", employerAddressLine1);
        db.SetNullableString(command, "empAddressLine2", employerAddressLine2);
        db.SetNullableString(command, "employerCity", employerCity);
        db.SetNullableString(command, "employerState", employerState);
        db.SetNullableString(command, "empZipCode5", employerZipCode5);
        db.SetNullableString(command, "empZipCode4", employerZipCode4);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableInt32(command, "employerEin", employerEin);
        db.SetNullableDate(command, "addrVerifiedDate", addressVerifiedDate);
        db.SetNullableDate(command, "empVerifiedDate", employerVerifiedDate);
        db.SetNullableString(command, "workPhone", workPhone);
        db.SetNullableString(command, "workAreaCode", workAreaCode);
        db.SetNullableString(command, "placeOfBirth", placeOfBirth);
        db.SetNullableString(command, "childStateOfRes", childStateOfResidence);
        db.SetNullableString(command, "childPaterStatus", childPaternityStatus);
        db.SetNullableString(command, "empConfirmedInd", employerConfirmedInd);
        db.SetNullableString(command, "addrConfirmedInd", addressConfirmedInd);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateParticipant.CcaTransactionDt.GetValueOrDefault());
        db.SetInt32(
          command, "sysGeneratedId",
          entities.InterstateParticipant.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateParticipant.CcaTransSerNum);
      });

    entities.InterstateParticipant.NameLast = nameLast;
    entities.InterstateParticipant.NameFirst = nameFirst;
    entities.InterstateParticipant.NameMiddle = nameMiddle;
    entities.InterstateParticipant.NameSuffix = nameSuffix;
    entities.InterstateParticipant.DateOfBirth = dateOfBirth;
    entities.InterstateParticipant.Ssn = ssn;
    entities.InterstateParticipant.Sex = sex;
    entities.InterstateParticipant.Race = race;
    entities.InterstateParticipant.Relationship = relationship;
    entities.InterstateParticipant.Status = status;
    entities.InterstateParticipant.DependentRelationCp = dependentRelationCp;
    entities.InterstateParticipant.AddressLine1 = addressLine1;
    entities.InterstateParticipant.AddressLine2 = addressLine2;
    entities.InterstateParticipant.City = city;
    entities.InterstateParticipant.State = state;
    entities.InterstateParticipant.ZipCode5 = zipCode5;
    entities.InterstateParticipant.ZipCode4 = zipCode4;
    entities.InterstateParticipant.EmployerAddressLine1 = employerAddressLine1;
    entities.InterstateParticipant.EmployerAddressLine2 = employerAddressLine2;
    entities.InterstateParticipant.EmployerCity = employerCity;
    entities.InterstateParticipant.EmployerState = employerState;
    entities.InterstateParticipant.EmployerZipCode5 = employerZipCode5;
    entities.InterstateParticipant.EmployerZipCode4 = employerZipCode4;
    entities.InterstateParticipant.EmployerName = employerName;
    entities.InterstateParticipant.EmployerEin = employerEin;
    entities.InterstateParticipant.AddressVerifiedDate = addressVerifiedDate;
    entities.InterstateParticipant.EmployerVerifiedDate = employerVerifiedDate;
    entities.InterstateParticipant.WorkPhone = workPhone;
    entities.InterstateParticipant.WorkAreaCode = workAreaCode;
    entities.InterstateParticipant.PlaceOfBirth = placeOfBirth;
    entities.InterstateParticipant.ChildStateOfResidence =
      childStateOfResidence;
    entities.InterstateParticipant.ChildPaternityStatus = childPaternityStatus;
    entities.InterstateParticipant.EmployerConfirmedInd = employerConfirmedInd;
    entities.InterstateParticipant.AddressConfirmedInd = addressConfirmedInd;
    entities.InterstateParticipant.Populated = true;
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
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
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

    private InterstateParticipant interstateParticipant;
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
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
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

    private InterstateParticipant interstateParticipant;
    private InterstateCase interstateCase;
  }
#endregion
}
