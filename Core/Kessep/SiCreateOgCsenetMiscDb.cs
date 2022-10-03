// Program: SI_CREATE_OG_CSENET_MISC_DB, ID: 372382225, model: 746.
// Short name: SWE01612
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_OG_CSENET_MISC_DB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// THIS CAB CREATES THE INTERSTATE_INFORMATION_DB AND ASSOCIATES IT WITH THE 
/// INTERSTATE_CASE_DB. THE INFORMATION LINES 1-3 ARE SET TO THE CORRESPONDING
/// INTERSTATE_HISTORY NOTE.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetMiscDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_MISC_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetMiscDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetMiscDb.
  /// </summary>
  public SiCreateOgCsenetMiscDb(IContext context, Import import, Export export):
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
    if (ReadInterstateRequest())
    {
      if (!ReadInterstateRequestHistory())
      {
        ExitState = "INTERSTATE_REQUEST_HISTORY_NF";

        return;
      }
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (!ReadInterstateCase())
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    export.InterstateMiscellaneous.StatusChangeCode = import.Case1.Status ?? Spaces
      (1);
    local.Length.Count = Length(entities.InterstateRequestHistory.Note);

    if (local.Length.Count < 80)
    {
      export.InterstateMiscellaneous.InformationTextLine1 =
        TrimEnd(entities.InterstateRequestHistory.Note);
    }
    else if (local.Length.Count < 160)
    {
      export.InterstateMiscellaneous.InformationTextLine1 =
        Substring(entities.InterstateRequestHistory.Note, 1, 80);
      export.InterstateMiscellaneous.InformationTextLine2 =
        Substring(entities.InterstateRequestHistory.Note, 81,
        local.Length.Count - 80);
    }
    else if (local.Length.Count < 240)
    {
      export.InterstateMiscellaneous.InformationTextLine1 =
        Substring(entities.InterstateRequestHistory.Note, 1, 80);
      export.InterstateMiscellaneous.InformationTextLine2 =
        Substring(entities.InterstateRequestHistory.Note, 81, 80);
      export.InterstateMiscellaneous.InformationTextLine3 =
        Substring(entities.InterstateRequestHistory.Note, 161,
        local.Length.Count - 80);
    }
    else if (local.Length.Count < 320)
    {
      export.InterstateMiscellaneous.InformationTextLine1 =
        Substring(entities.InterstateRequestHistory.Note, 1, 80);
      export.InterstateMiscellaneous.InformationTextLine2 =
        Substring(entities.InterstateRequestHistory.Note, 81, 80);
      export.InterstateMiscellaneous.InformationTextLine3 =
        Substring(entities.InterstateRequestHistory.Note, 161, 80);
      export.InterstateMiscellaneous.InformationTextLine4 =
        Substring(entities.InterstateRequestHistory.Note, 241,
        local.Length.Count - 80);
    }
    else
    {
      export.InterstateMiscellaneous.InformationTextLine1 =
        Substring(entities.InterstateRequestHistory.Note, 1, 80);
      export.InterstateMiscellaneous.InformationTextLine2 =
        Substring(entities.InterstateRequestHistory.Note, 81, 80);
      export.InterstateMiscellaneous.InformationTextLine3 =
        Substring(entities.InterstateRequestHistory.Note, 161, 80);
      export.InterstateMiscellaneous.InformationTextLine4 =
        Substring(entities.InterstateRequestHistory.Note, 241, 80);
      export.InterstateMiscellaneous.InformationTextLine5 =
        Substring(entities.InterstateRequestHistory.Note, 321,
        local.Length.Count - 80);
    }

    try
    {
      CreateInterstateMiscellaneous();
      export.InterstateMiscellaneous.Assign(entities.InterstateMiscellaneous);
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

  private void CreateInterstateMiscellaneous()
  {
    var statusChangeCode = export.InterstateMiscellaneous.StatusChangeCode;
    var newCaseId = export.InterstateMiscellaneous.NewCaseId ?? "";
    var informationTextLine1 =
      export.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      export.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      export.InterstateMiscellaneous.InformationTextLine3 ?? "";
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var informationTextLine4 =
      export.InterstateMiscellaneous.InformationTextLine4 ?? "";
    var informationTextLine5 =
      export.InterstateMiscellaneous.InformationTextLine5 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("CreateInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetString(command, "statusChangeCode", statusChangeCode);
        db.SetNullableString(command, "newCaseId", newCaseId);
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetNullableString(command, "infoTextLine4", informationTextLine4);
        db.SetNullableString(command, "infoTextLine5", informationTextLine5);
      });

    entities.InterstateMiscellaneous.StatusChangeCode = statusChangeCode;
    entities.InterstateMiscellaneous.NewCaseId = newCaseId;
    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateMiscellaneous.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateMiscellaneous.InformationTextLine4 =
      informationTextLine4;
    entities.InterstateMiscellaneous.InformationTextLine5 =
      informationTextLine5;
    entities.InterstateMiscellaneous.Populated = true;
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
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          import.InterstateRequestHistory.CreatedTimestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 2);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 3);
        entities.InterstateRequestHistory.Populated = true;
      });
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private InterstateMiscellaneous interstateMiscellaneous;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    private Common length;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
    private InterstateMiscellaneous interstateMiscellaneous;
  }
#endregion
}
