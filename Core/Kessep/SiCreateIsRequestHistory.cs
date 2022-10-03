// Program: SI_CREATE_IS_REQUEST_HISTORY, ID: 372381436, model: 746.
// Short name: SWE01137
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_IS_REQUEST_HISTORY.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the Interstate Request History which contains the history 
/// of all interstate csenet transactions for both inbound or outbound request.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateIsRequestHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_IS_REQUEST_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateIsRequestHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateIsRequestHistory.
  /// </summary>
  public SiCreateIsRequestHistory(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *************************************************************
    // 04/29/97	SHERAZ MALIK	CHANGE CURRENT_DATE
    // *************************************************************
    // 06/23/99  M. Lachowicz     Change property of READ
    //                            (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    // 02/18/09  AHockman  cq962       stop duplicates from being created
    //                                 
    // on outgoing MSC P GSUPD
    // transactions
    //                                  
    // on court order keychanges.
    // -------------------------------------------------------------------------------
    // 06/22/10  R.Mathews  CQ19787  Moved read for duplicate interstate
    // request history records for csenet back into 
    // update_legal_action_court_case
    // -------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.InterstateRequestHistory.Assign(import.InterstateRequestHistory);
    export.InterstateRequestHistory.Assign(import.InterstateRequestHistory);

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadAbsentParentCsePerson())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (import.InterstateRequest.IntHGeneratedId != 0)
    {
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadInterstateRequest2())
      {
        // 06/22/10  R.Mathews  Moved following read for duplicate csenet 
        // transactions into calling program
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }
    else if (!ReadInterstateRequest1())
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    // ---------------------------------------------
    // For an outgoing referral,
    // ie. direction_ind = "O",
    // The transaction serial # and the transaction
    // date will be blanks.
    // ---------------------------------------------
    if (AsChar(import.InterstateRequestHistory.TransactionDirectionInd) == 'O'
      || IsEmpty(import.InterstateRequestHistory.TransactionDirectionInd))
    {
      local.InterstateRequestHistory.TransactionSerialNum = 0;
    }

    // ---------------------------------------------
    // For an incomming referral,
    // ie. direction_ind = "I",
    // The CSENet_Case DB will populate most of the
    // Interstate Req History details.
    // ---------------------------------------------
    if (AsChar(import.InterstateRequestHistory.TransactionDirectionInd) == 'I')
    {
      local.InterstateRequestHistory.TransactionSerialNum =
        import.InterstateCase.TransSerialNumber;
      local.InterstateRequestHistory.ActionCode =
        import.InterstateCase.ActionCode;
      local.InterstateRequestHistory.FunctionalTypeCode =
        import.InterstateCase.FunctionalTypeCode;
      local.InterstateRequestHistory.ActionReasonCode =
        import.InterstateCase.ActionReasonCode ?? "";
      local.InterstateRequestHistory.TransactionDate =
        import.InterstateCase.TransactionDate;
      local.InterstateRequestHistory.ActionResolutionDate =
        import.InterstateCase.ActionResolutionDate;
      local.InterstateRequestHistory.AttachmentIndicator =
        import.InterstateCase.AttachmentsInd;
    }

    // ****************************************************************
    // WR 10501
    // Tom Bobb  06/27/01
    // If called from OINR or IIMC set the created by to
    // the calling program name, otherwise set it to userid.
    // ****************************************************************
    if (Equal(import.InterstateRequestHistory.CreatedBy, "SWEIOINR") || Equal
      (import.InterstateRequestHistory.CreatedBy, "SWEIIIMC"))
    {
      local.InterstateRequestHistory.CreatedBy =
        import.InterstateRequestHistory.CreatedBy ?? "";
    }
    else
    {
      local.InterstateRequestHistory.CreatedBy = global.UserId;
    }

    try
    {
      CreateInterstateRequestHistory();
      export.InterstateRequestHistory.Assign(entities.InterstateRequestHistory);
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTAT_REQ_HIST_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTAT_REQ_HIST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = Now();
    var createdBy = local.InterstateRequestHistory.CreatedBy ?? "";
    var transactionDirectionInd =
      local.InterstateRequestHistory.TransactionDirectionInd;
    var transactionSerialNum =
      local.InterstateRequestHistory.TransactionSerialNum;
    var actionCode = local.InterstateRequestHistory.ActionCode;
    var functionalTypeCode = local.InterstateRequestHistory.FunctionalTypeCode;
    var transactionDate = local.InterstateRequestHistory.TransactionDate;
    var actionReasonCode = local.InterstateRequestHistory.ActionReasonCode ?? ""
      ;
    var actionResolutionDate =
      local.InterstateRequestHistory.ActionResolutionDate;
    var attachmentIndicator =
      local.InterstateRequestHistory.AttachmentIndicator ?? "";
    var note = local.InterstateRequestHistory.Note ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("CreateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetString(command, "transactionDirect", transactionDirectionInd);
        db.SetInt64(command, "transactionSerial", transactionSerialNum);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResDte", actionResolutionDate);
        db.SetNullableString(command, "attachmentIndicat", attachmentIndicator);
        db.SetNullableString(command, "note", note);
      });

    entities.InterstateRequestHistory.IntGeneratedId = intGeneratedId;
    entities.InterstateRequestHistory.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.TransactionDirectionInd =
      transactionDirectionInd;
    entities.InterstateRequestHistory.TransactionSerialNum =
      transactionSerialNum;
    entities.InterstateRequestHistory.ActionCode = actionCode;
    entities.InterstateRequestHistory.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateRequestHistory.TransactionDate = transactionDate;
    entities.InterstateRequestHistory.ActionReasonCode = actionReasonCode;
    entities.InterstateRequestHistory.ActionResolutionDate =
      actionResolutionDate;
    entities.InterstateRequestHistory.AttachmentIndicator = attachmentIndicator;
    entities.InterstateRequestHistory.Note = note;
    entities.InterstateRequestHistory.Populated = true;
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
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
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private Case1 case1;
    private CsePerson ap;
    private InterstateRequest interstateRequest;
    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson ap;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
  }
#endregion
}
