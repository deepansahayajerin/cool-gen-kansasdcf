// Program: SI_CREATE_PA_PARTICIPANT_ADDRESS, ID: 371789372, model: 746.
// Short name: SWE01143
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_PA_PARTICIPANT_ADDRESS.
/// </summary>
[Serializable]
public partial class SiCreatePaParticipantAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_PA_PARTICIPANT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreatePaParticipantAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreatePaParticipantAddress.
  /// </summary>
  public SiCreatePaParticipantAddress(IContext context, Import import,
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
    if (ReadPaReferralParticipantPaReferral())
    {
      try
      {
        CreatePaParticipantAddress();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PA_PARTICIPANT_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PA_PARTICIPANT_ADDRESS_PV";

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
      ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
    }
  }

  private void CreatePaParticipantAddress()
  {
    System.Diagnostics.Debug.Assert(entities.PaReferralParticipant.Populated);

    var createdTimestamp = import.PaParticipantAddress.CreatedTimestamp;
    var type1 = import.PaParticipantAddress.Type1 ?? "";
    var street1 = import.PaParticipantAddress.Street1 ?? "";
    var street2 = import.PaParticipantAddress.Street2 ?? "";
    var city = import.PaParticipantAddress.City ?? "";
    var state = import.PaParticipantAddress.State ?? "";
    var zip = import.PaParticipantAddress.Zip ?? "";
    var zip4 = import.PaParticipantAddress.Zip4 ?? "";
    var zip3 = import.PaParticipantAddress.Zip3 ?? "";
    var createdBy = import.PaParticipantAddress.CreatedBy ?? "";
    var lastUpdatedBy = import.PaParticipantAddress.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.PaParticipantAddress.LastUpdatedTimestamp;
    var identifier = import.PaParticipantAddress.Identifier;
    var prpIdentifier = entities.PaReferralParticipant.Identifier;
    var pafType = entities.PaReferralParticipant.PafType;
    var preNumber = entities.PaReferralParticipant.PreNumber;
    var pafTstamp = entities.PaReferralParticipant.PafTstamp;

    entities.PaParticipantAddress.Populated = false;
    Update("CreatePaParticipantAddress",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zip", zip);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "prpIdentifier", prpIdentifier);
        db.SetString(command, "pafType", pafType);
        db.SetString(command, "preNumber", preNumber);
        db.SetDateTime(command, "pafTstamp", pafTstamp);
      });

    entities.PaParticipantAddress.CreatedTimestamp = createdTimestamp;
    entities.PaParticipantAddress.Type1 = type1;
    entities.PaParticipantAddress.Street1 = street1;
    entities.PaParticipantAddress.Street2 = street2;
    entities.PaParticipantAddress.City = city;
    entities.PaParticipantAddress.State = state;
    entities.PaParticipantAddress.Zip = zip;
    entities.PaParticipantAddress.Zip4 = zip4;
    entities.PaParticipantAddress.Zip3 = zip3;
    entities.PaParticipantAddress.CreatedBy = createdBy;
    entities.PaParticipantAddress.LastUpdatedBy = lastUpdatedBy;
    entities.PaParticipantAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaParticipantAddress.Identifier = identifier;
    entities.PaParticipantAddress.PrpIdentifier = prpIdentifier;
    entities.PaParticipantAddress.PafType = pafType;
    entities.PaParticipantAddress.PreNumber = preNumber;
    entities.PaParticipantAddress.PafTstamp = pafTstamp;
    entities.PaParticipantAddress.Populated = true;
  }

  private bool ReadPaReferralParticipantPaReferral()
  {
    entities.PaReferral.Populated = false;
    entities.PaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipantPaReferral",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.PaReferralParticipant.Identifier);
        db.SetString(command, "preNumber", import.PaReferral.Number);
        db.SetString(command, "pafType", import.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferralParticipant.Identifier = db.GetInt32(reader, 0);
        entities.PaReferralParticipant.PreNumber = db.GetString(reader, 1);
        entities.PaReferral.Number = db.GetString(reader, 1);
        entities.PaReferralParticipant.PafType = db.GetString(reader, 2);
        entities.PaReferral.Type1 = db.GetString(reader, 2);
        entities.PaReferralParticipant.PafTstamp = db.GetDateTime(reader, 3);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaReferral.Populated = true;
        entities.PaReferralParticipant.Populated = true;
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
    /// A value of PaParticipantAddress.
    /// </summary>
    [JsonPropertyName("paParticipantAddress")]
    public PaParticipantAddress PaParticipantAddress
    {
      get => paParticipantAddress ??= new();
      set => paParticipantAddress = value;
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

    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
    }

    private PaParticipantAddress paParticipantAddress;
    private PaReferral paReferral;
    private PaReferralParticipant paReferralParticipant;
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
    /// A value of PaParticipantAddress.
    /// </summary>
    [JsonPropertyName("paParticipantAddress")]
    public PaParticipantAddress PaParticipantAddress
    {
      get => paParticipantAddress ??= new();
      set => paParticipantAddress = value;
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

    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
    }

    private PaParticipantAddress paParticipantAddress;
    private PaReferral paReferral;
    private PaReferralParticipant paReferralParticipant;
  }
#endregion
}
