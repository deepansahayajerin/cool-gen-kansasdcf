// Program: PEND_COLLECTION, ID: 371770022, model: 746.
// Short name: SWE00981
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: PEND_COLLECTION.
/// </summary>
[Serializable]
public partial class PendCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the PEND_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new PendCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of PendCollection.
  /// </summary>
  public PendCollection(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************
    // 4/29/97		SHERAZ		CHANGE CURRENT_DATE
    // 10/30/98	Sunya Sharp	Added changes per screen assessment.  Only allow for
    // reason code of "RESEARCH".  Display status of "PEND" not "REC".  Allow
    // for cash receipt detail in suspended status to be pended.
    // ******************************************************
    local.Current.Date = Now().Date;
    MoveCashReceiptDetailStatus(import.CashReceiptDetailStatus,
      export.CashReceiptDetailStatus);
    MoveCashReceiptDetailStatHistory(import.CashReceiptDetailStatHistory,
      export.CashReceiptDetailStatHistory);
    UseFnHardcodedCashReceipting();
    UseCabSetMaximumDiscontinueDate();

    // *****
    // Determine the status of the CR before continuing processing
    // *****
    if (!ReadCashReceipt())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    if (ReadCashReceiptDetail())
    {
      if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
      {
        // *** Added logic to default reason code to "RESEARCH" if it is blank 
        // and to error if it is not "RESEARCH".  This is the only valid pend
        // reason code.  Sunya Sharp 10/30/98 ***
        // *** Added logic to allow suspended cash receipt detail to be pended.
        // Sunya Sharp 10/31/98 ***
        // *** Added logic to allow released cash receipt detail to be pended.  
        // Sunya Sharp 05/25/99 ***
        if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .Recorded.SystemGeneratedIdentifier || entities
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .Suspended.SystemGeneratedIdentifier || entities
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .Released.SystemGeneratedIdentifier)
        {
          if (IsEmpty(import.CashReceiptDetailStatHistory.ReasonCodeId))
          {
            MoveCashReceiptDetailStatHistory(import.
              CashReceiptDetailStatHistory,
              export.CashReceiptDetailStatHistory);
            export.CashReceiptDetailStatHistory.ReasonCodeId = "RESEARCH";
            local.ToBeChanged.SystemGeneratedIdentifier =
              local.Pended.SystemGeneratedIdentifier;
          }
          else
          {
            local.Pass.Cdvalue =
              import.CashReceiptDetailStatHistory.ReasonCodeId ?? Spaces(10);
            local.Code.CodeName = "PEND/SUSP REASON";
            UseCabValidateCodeValue();

            if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
            {
              ExitState = "PENDING_REASON_CODE_INVALID";

              return;
            }
            else if (!Equal(import.CashReceiptDetailStatHistory.ReasonCodeId,
              "RESEARCH"))
            {
              ExitState = "PENDING_REASON_CODE_INVALID";

              return;
            }
            else
            {
              MoveCashReceiptDetailStatHistory(import.
                CashReceiptDetailStatHistory,
                export.CashReceiptDetailStatHistory);
              local.ToBeChanged.SystemGeneratedIdentifier =
                local.Pended.SystemGeneratedIdentifier;
            }
          }
        }
        else
        {
          ExitState = "FN0000_COLLECT_STAT_IMPROPER_RB";

          return;
        }
      }
      else
      {
        ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.ToBeChanged.SystemGeneratedIdentifier;
      export.CashReceiptDetailStatus.Code = "PEND";
    }
    else
    {
    }
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.Pass.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.ToBeChanged.SystemGeneratedIdentifier;
    MoveCashReceiptDetailStatHistory(export.CashReceiptDetailStatHistory,
      useImport.New1);

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Released.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.Pended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.Recorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.Suspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public CashReceiptDetailStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of ToBeChanged.
    /// </summary>
    [JsonPropertyName("toBeChanged")]
    public CashReceiptDetailStatus ToBeChanged
    {
      get => toBeChanged ??= new();
      set => toBeChanged = value;
    }

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
    /// A value of ValidCodeIndicator.
    /// </summary>
    [JsonPropertyName("validCodeIndicator")]
    public Common ValidCodeIndicator
    {
      get => validCodeIndicator ??= new();
      set => validCodeIndicator = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Recorded.
    /// </summary>
    [JsonPropertyName("recorded")]
    public CashReceiptDetailStatus Recorded
    {
      get => recorded ??= new();
      set => recorded = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of FoundAStatus.
    /// </summary>
    [JsonPropertyName("foundAStatus")]
    public Common FoundAStatus
    {
      get => foundAStatus ??= new();
      set => foundAStatus = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CodeValue Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private CashReceiptDetailStatus released;
    private CashReceiptDetailStatus toBeChanged;
    private DateWorkArea current;
    private Common validCodeIndicator;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea maximum;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatus recorded;
    private CashReceiptDetailStatus suspended;
    private Common foundAStatus;
    private CodeValue pass;
    private Common returnCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }
#endregion
}
