// Program: SI_CAB_UPDATE_IS_REQUEST, ID: 373465507, model: 746.
// Short name: SWE02799
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_UPDATE_IS_REQUEST.
/// </summary>
[Serializable]
public partial class SiCabUpdateIsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_UPDATE_IS_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabUpdateIsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabUpdateIsRequest.
  /// </summary>
  public SiCabUpdateIsRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************UPDATES AND CHANGES TO 
    // PROGRAM
    // **********************************************
    // 6-6-08    cq1415     Anita Hockman   changes made to help fix resend 
    // issues.      when resend is done the data is read from CASE table and
    // when  the update is done the update  is don to fix the reason the trans
    // didn't go out in the first place, it makes changes to the REQUEST table
    // so when the resend is done the data in CASE is not updated so the new
    // data doesn't go out.    CASE is really supposed to be a history of
    // transactions but since the first trans errored and didn't go out Jolene
    // felt it was ok to 'update' history to get the transaction resent.
    // ******************************************************************************************************
    local.InterstateRequest.Assign(import.InterstateRequest);

    if (Lt(local.Null1.Timestamp, import.InterstateRequest.LastUpdatedTimestamp))
      
    {
      local.InterstateRequest.LastUpdatedTimestamp =
        import.InterstateRequest.LastUpdatedTimestamp;
    }
    else
    {
      local.InterstateRequest.LastUpdatedTimestamp = Now();
    }

    if (!IsEmpty(import.InterstateRequest.LastUpdatedBy))
    {
      local.InterstateRequest.LastUpdatedBy =
        import.InterstateRequest.LastUpdatedBy;
    }
    else
    {
      local.InterstateRequest.LastUpdatedBy = global.UserId;
    }

    switch(AsChar(import.InterstateRequest.OtherStateCaseStatus))
    {
      case 'O':
        local.InterstateRequest.OtherStateCaseClosureReason = "";

        break;
      case 'C':
        local.InterstateRequest.OtherStateCaseClosureReason =
          import.InterstateRequest.OtherStateCaseClosureReason ?? "";

        break;
      default:
        ExitState = "INTERSTATE_REQUEST_PV";

        return;
    }

    if (ReadInterstateRequest())
    {
      // ***  cq1415   Anita Hockman   resend issues
      if (AsChar(import.OtherStateCaseIdChg.Flag) == 'Y')
      {
        foreach(var item in ReadInterstateRequestHistory())
        {
          foreach(var item1 in ReadInterstateCase())
          {
            try
            {
              UpdateInterstateCase();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "INTERSTATE_CASE_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "INTERSTATE_CASE_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }

      try
      {
        UpdateInterstateRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_REQUEST_NU";

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

  private IEnumerable<bool> ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr",
          entities.InterstateRequestHistory.TransactionSerialNum);
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 2);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 3);
        entities.InterstateCase.Populated = true;

        return true;
      });
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
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 2);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 4);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 5);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 7);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 8);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 2);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private void UpdateInterstateCase()
  {
    var interstateCaseId = local.InterstateRequest.OtherStateCaseId ?? "";

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(command, "interstateCaseId", interstateCaseId);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.InterstateCaseId = interstateCaseId;
    entities.InterstateCase.Populated = true;
  }

  private void UpdateInterstateRequest()
  {
    var otherStateCaseId = local.InterstateRequest.OtherStateCaseId ?? "";
    var lastUpdatedBy = local.InterstateRequest.LastUpdatedBy;
    var lastUpdatedTimestamp = local.InterstateRequest.LastUpdatedTimestamp;
    var otherStateCaseStatus = local.InterstateRequest.OtherStateCaseStatus;
    var caseType = local.InterstateRequest.CaseType ?? "";
    var ksCaseInd = local.InterstateRequest.KsCaseInd;
    var otherStateCaseClosureReason =
      local.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate =
      local.InterstateRequest.OtherStateCaseClosureDate;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
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
  protected readonly Local local = new();
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
    /// A value of OtherStateCaseIdChg.
    /// </summary>
    [JsonPropertyName("otherStateCaseIdChg")]
    public Common OtherStateCaseIdChg
    {
      get => otherStateCaseIdChg ??= new();
      set => otherStateCaseIdChg = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private Common otherStateCaseIdChg;
    private InterstateRequest interstateRequest;
    private Common previous;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private Common previous;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private DateWorkArea null1;
    private InterstateRequest interstateRequest;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
  }
#endregion
}
