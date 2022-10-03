// Program: SI_CREATE_PA_REFERRAL_PARTICIPNT, ID: 371789371, model: 746.
// Short name: SWE01145
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_PA_REFERRAL_PARTICIPNT.
/// </summary>
[Serializable]
public partial class SiCreatePaReferralParticipnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_PA_REFERRAL_PARTICIPNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreatePaReferralParticipnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreatePaReferralParticipnt.
  /// </summary>
  public SiCreatePaReferralParticipnt(IContext context, Import import,
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
    // **************************************************
    // Date    Developer    Description
    // 06/96	J Howard     Initial development
    // **************************************************
    if (ReadPaReferral())
    {
      try
      {
        CreatePaReferralParticipant();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PA_REFERRAL_PARTICIPANT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PA_REFERRAL_PARTICIPANT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PA_REFERRAL_NF";
    }
  }

  private void CreatePaReferralParticipant()
  {
    var identifier = import.PaReferralParticipant.Identifier;
    var createdTimestamp = import.PaReferralParticipant.CreatedTimestamp;
    var absenceCode = import.PaReferralParticipant.AbsenceCode ?? "";
    var relationship = import.PaReferralParticipant.Relationship ?? "";
    var sex = import.PaReferralParticipant.Sex ?? "";
    var dob = import.PaReferralParticipant.Dob;
    var lastName = import.PaReferralParticipant.LastName ?? "";
    var firstName = import.PaReferralParticipant.FirstName ?? "";
    var mi = import.PaReferralParticipant.Mi ?? "";
    var ssn = import.PaReferralParticipant.Ssn ?? "";
    var personNumber = import.PaReferralParticipant.PersonNumber ?? "";
    var insurInd = import.PaReferralParticipant.InsurInd ?? "";
    var patEstInd = import.PaReferralParticipant.PatEstInd ?? "";
    var beneInd = import.PaReferralParticipant.BeneInd ?? "";
    var createdBy = import.PaReferralParticipant.CreatedBy ?? "";
    var lastUpdatedBy = import.PaReferralParticipant.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.PaReferralParticipant.LastUpdatedTimestamp;
    var preNumber = entities.PaReferral.Number;
    var goodCauseStatus = import.PaReferralParticipant.GoodCauseStatus ?? "";
    var pafType = entities.PaReferral.Type1;
    var pafTstamp = entities.PaReferral.CreatedTimestamp;
    var role = import.PaReferralParticipant.Role ?? "";

    entities.PaReferralParticipant.Populated = false;
    Update("CreatePaReferralParticipant",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "absenceCode", absenceCode);
        db.SetNullableString(command, "relationship", relationship);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableDate(command, "dob", dob);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", mi);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "personNum", personNumber);
        db.SetNullableString(command, "insurInd", insurInd);
        db.SetNullableString(command, "patEstInd", patEstInd);
        db.SetNullableString(command, "beneInd", beneInd);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "preNumber", preNumber);
        db.SetNullableString(command, "goodCauseStatus", goodCauseStatus);
        db.SetString(command, "pafType", pafType);
        db.SetDateTime(command, "pafTstamp", pafTstamp);
        db.SetNullableString(command, "role", role);
      });

    entities.PaReferralParticipant.Identifier = identifier;
    entities.PaReferralParticipant.CreatedTimestamp = createdTimestamp;
    entities.PaReferralParticipant.AbsenceCode = absenceCode;
    entities.PaReferralParticipant.Relationship = relationship;
    entities.PaReferralParticipant.Sex = sex;
    entities.PaReferralParticipant.Dob = dob;
    entities.PaReferralParticipant.LastName = lastName;
    entities.PaReferralParticipant.FirstName = firstName;
    entities.PaReferralParticipant.Mi = mi;
    entities.PaReferralParticipant.Ssn = ssn;
    entities.PaReferralParticipant.PersonNumber = personNumber;
    entities.PaReferralParticipant.InsurInd = insurInd;
    entities.PaReferralParticipant.PatEstInd = patEstInd;
    entities.PaReferralParticipant.BeneInd = beneInd;
    entities.PaReferralParticipant.CreatedBy = createdBy;
    entities.PaReferralParticipant.LastUpdatedBy = lastUpdatedBy;
    entities.PaReferralParticipant.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaReferralParticipant.PreNumber = preNumber;
    entities.PaReferralParticipant.GoodCauseStatus = goodCauseStatus;
    entities.PaReferralParticipant.PafType = pafType;
    entities.PaReferralParticipant.PafTstamp = pafTstamp;
    entities.PaReferralParticipant.Role = role;
    entities.PaReferralParticipant.Populated = true;
  }

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

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
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.Type1 = db.GetString(reader, 1);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaReferral.Populated = true;
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
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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

    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
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
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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

    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
  }
#endregion
}
