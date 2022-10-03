// Program: SI_UPDATE_INTERSTATE_RQST_STATUS, ID: 372670732, model: 746.
// Short name: SWE02576
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_INTERSTATE_RQST_STATUS.
/// </summary>
[Serializable]
public partial class SiUpdateInterstateRqstStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INTERSTATE_RQST_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateInterstateRqstStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateInterstateRqstStatus.
  /// </summary>
  public SiUpdateInterstateRqstStatus(IContext context, Import import,
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
    // ------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer		Description
    // 05/24/99  Carl Ott              Initial development
    // ------------------------------------------------------------
    if (ReadInterstateRequest())
    {
      try
      {
        UpdateInterstateRequest();
        ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_REQUEST_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_REQUEST_PV";

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
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 1);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private void UpdateInterstateRequest()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var otherStateCaseStatus = import.InterstateRequest.OtherStateCaseStatus;
    var ksCaseInd = import.InterstateRequest.KsCaseInd;
    var otherStateCaseClosureReason =
      import.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      import.InterstateRequest.OtherStateCaseClosureDate;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.Populated = true;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest interstateRequest;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest interstateRequest;
  }
#endregion
}
