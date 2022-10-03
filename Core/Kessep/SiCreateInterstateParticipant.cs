// Program: SI_CREATE_INTERSTATE_PARTICIPANT, ID: 372383574, model: 746.
// Short name: SWE01613
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_INTERSTATE_PARTICIPANT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the entity type that contains Interstate participant.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateInterstateParticipant: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_PARTICIPANT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateParticipant(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateParticipant.
  /// </summary>
  public SiCreateInterstateParticipant(IContext context, Import import,
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
    // ***************************************************************
    // 2/17/1999    Carl Ott    IDCR # 501, new attributes added.
    // **************************************************************
    export.InterstateParticipant.Assign(import.InterstateParticipant);

    if (!ReadInterstateCase())
    {
      ExitState = "CSENET_CASE_NF";

      return;
    }

    ReadInterstateParticipant();
    export.InterstateParticipant.SystemGeneratedSequenceNum =
      entities.InterstateParticipant.SystemGeneratedSequenceNum + 1;

    try
    {
      CreateInterstateParticipant();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateParticipant()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var systemGeneratedSequenceNum =
      export.InterstateParticipant.SystemGeneratedSequenceNum;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var nameLast = export.InterstateParticipant.NameLast ?? "";
    var nameFirst = export.InterstateParticipant.NameFirst ?? "";
    var nameMiddle = export.InterstateParticipant.NameMiddle ?? "";
    var nameSuffix = export.InterstateParticipant.NameSuffix ?? "";
    var dateOfBirth = export.InterstateParticipant.DateOfBirth;
    var ssn = export.InterstateParticipant.Ssn ?? "";
    var sex = export.InterstateParticipant.Sex ?? "";
    var race = export.InterstateParticipant.Race ?? "";
    var relationship = export.InterstateParticipant.Relationship ?? "";
    var status = export.InterstateParticipant.Status ?? "";
    var dependentRelationCp =
      export.InterstateParticipant.DependentRelationCp ?? "";
    var addressLine1 = export.InterstateParticipant.AddressLine1 ?? "";
    var addressLine2 = export.InterstateParticipant.AddressLine2 ?? "";
    var city = export.InterstateParticipant.City ?? "";
    var state = export.InterstateParticipant.State ?? "";
    var zipCode5 = export.InterstateParticipant.ZipCode5 ?? "";
    var zipCode4 = export.InterstateParticipant.ZipCode4 ?? "";
    var employerAddressLine1 =
      export.InterstateParticipant.EmployerAddressLine1 ?? "";
    var employerAddressLine2 =
      export.InterstateParticipant.EmployerAddressLine2 ?? "";
    var employerCity = export.InterstateParticipant.EmployerCity ?? "";
    var employerState = export.InterstateParticipant.EmployerState ?? "";
    var employerZipCode5 = export.InterstateParticipant.EmployerZipCode5 ?? "";
    var employerZipCode4 = export.InterstateParticipant.EmployerZipCode4 ?? "";
    var employerName = export.InterstateParticipant.EmployerName ?? "";
    var employerEin =
      export.InterstateParticipant.EmployerEin.GetValueOrDefault();
    var addressVerifiedDate = export.InterstateParticipant.AddressVerifiedDate;
    var employerVerifiedDate =
      export.InterstateParticipant.EmployerVerifiedDate;
    var workPhone = export.InterstateParticipant.WorkPhone ?? "";
    var workAreaCode = export.InterstateParticipant.WorkAreaCode ?? "";
    var placeOfBirth = export.InterstateParticipant.PlaceOfBirth ?? "";
    var childStateOfResidence =
      export.InterstateParticipant.ChildStateOfResidence ?? "";
    var childPaternityStatus =
      export.InterstateParticipant.ChildPaternityStatus ?? "";
    var employerConfirmedInd =
      export.InterstateParticipant.EmployerConfirmedInd ?? "";
    var addressConfirmedInd =
      export.InterstateParticipant.AddressConfirmedInd ?? "";

    entities.InterstateParticipant.Populated = false;
    Update("CreateInterstateParticipant",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
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
      });

    entities.InterstateParticipant.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateParticipant.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateParticipant.CcaTransSerNum = ccaTransSerNum;
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
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
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
    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    private InterstateParticipant interstateParticipant;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    private InterstateCase interstateCase;
    private InterstateParticipant interstateParticipant;
  }
#endregion
}
