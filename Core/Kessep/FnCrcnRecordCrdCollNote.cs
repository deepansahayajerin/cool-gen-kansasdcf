// Program: FN_CRCN_RECORD_CRD_COLL_NOTE, ID: 371769142, model: 746.
// Short name: SWECRCNP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CRCN_RECORD_CRD_COLL_NOTE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrcnRecordCrdCollNote: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRCN_RECORD_CRD_COLL_NOTE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrcnRecordCrdCollNote(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrcnRecordCrdCollNote.
  /// </summary>
  public FnCrcnRecordCrdCollNote(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date		Developer Name		Request #	Description
    // 12/22/95	Holly Kennedy-MTW			SOURCE
    // 02/01/96	Holly Kennedy-MTW			Retrofits
    // 01/03/97	R. Marchman				Add new security/next tran
    // 09/25/98	Sunya Sharp				Make changes per screen assessment signed 9/14/
    // 98.  Changed screen properties on display only fields to remain protected
    // when an error occurs.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // *****
    // Move imports to exports
    // *****
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    export.HiddenCashReceiptSourceType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptSourceType.SystemGeneratedIdentifier;
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.HiddenCollectionType.SequentialIdentifier =
      import.HiddenCollectionType.SequentialIdentifier;

    // *** Moved clear screen logic to only clear the note field and retain all 
    // non-enterable fields.  Sunya Sharp - 9/25/98 ***
    if (Equal(global.Command, "CLEAR"))
    {
      export.CashReceiptDetail.Notes =
        Spaces(CashReceiptDetail.Notes_MaxLength);
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Previous.Notes = import.Previous.Notes;

    // *****
    // Next Tran/Security logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No views to populate
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // No views to populate
      // ****
      UseScCabNextTranGet();

      // *** There is not enough information on the next tran to allow display 
      // and all views required to get information are not enterable.  Send
      // exitstate and error fields.  Sunya Sharp - 9/28/98. ***
      var field1 = GetField(export.CashReceipt, "sequentialNumber");

      field1.Error = true;

      var field2 = GetField(export.CashReceiptDetail, "collectionAmount");

      field2.Error = true;

      var field3 = GetField(export.CashReceiptDetail, "collectionDate");

      field3.Error = true;

      var field4 = GetField(export.CashReceiptDetail, "sequentialIdentifier");

      field4.Error = true;

      ExitState = "FN0000_CASH_RCPT_DTL_ADD_UPDATE";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // *****
    // If the command is add processing is the same as update.  Set the command 
    // to update.
    // *****
    if (Equal(global.Command, "ADD"))
    {
      if (IsEmpty(import.CashReceiptDetail.Notes))
      {
        ExitState = "FN0000_ENTER_DATA_BEFORE_ADD";

        return;
      }

      global.Command = "UPDATE";
      local.Add.Flag = "Y";
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Case of command
    // *****
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "DISPLAY":
        // *****
        // Display Cash Receipt Detail.
        // *****
        // *****
        // If appropriate data is not supplied set exitstate.
        // *****
        if (import.CashReceipt.SequentialNumber == 0 || import
          .CashReceiptDetail.CollectionAmount == 0 || Equal
          (import.CashReceiptDetail.CollectionDate, local.Null1.CollectionDate) ||
          import.CashReceiptDetail.SequentialIdentifier == 0)
        {
          var field1 = GetField(export.CashReceipt, "sequentialNumber");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "collectionAmount");

          field2.Error = true;

          var field3 = GetField(export.CashReceiptDetail, "collectionDate");

          field3.Error = true;

          var field4 =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field4.Error = true;

          ExitState = "FN0000_RET_CALLING_PGM_FOR_DATA";

          return;
        }

        UseFnAbReadCashRecDtlNote();

        // *****
        // Validate Exitstate.
        // *****
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.CashReceiptDetail, "collectionAmount");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "collectionDate");

          field2.Error = true;

          var field3 = GetField(export.CashReceipt, "sequentialNumber");

          field3.Error = true;

          var field4 =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field4.Error = true;

          return;
        }

        // *****
        // Display message if no description is present.
        // *****
        // *** Added exitstate to say successful display.  Sunya Sharp - 9/25/98
        // ***
        if (IsEmpty(export.CashReceiptDetail.Notes))
        {
          ExitState = "FN0000_NO_NOTE_EXISTS";

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "ADD":
        // *****
        // Falls into update logic
        // *****
        break;
      case "UPDATE":
        // *****
        // Determine if note has been changed before allowing update
        // *****
        // *** Added check to see if information is spaces. Sunya Sharp - 9/28/
        // 98 ***
        if (IsEmpty(export.CashReceiptDetail.Notes))
        {
          ExitState = "FN0000_MUST_DISPLAY_NOTE_UPDATE";

          return;
        }

        if (Equal(export.CashReceiptDetail.Notes, export.Previous.Notes))
        {
          ExitState = "INVALID_UPDATE";

          return;
        }

        // *****
        // Update the CRD.
        // *****
        UseUpdateCollectionTest();

        // *****
        // exitstate validation.
        // *****
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          var field1 = GetField(export.CashReceipt, "sequentialNumber");

          field1.Error = true;

          var field2 = GetField(export.CashReceiptDetail, "collectionAmount");

          field2.Error = true;

          var field3 = GetField(export.CashReceiptDetail, "collectionDate");

          field3.Error = true;

          var field4 =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field4.Error = true;

          return;
        }

        if (AsChar(local.Add.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      default:
        // *** Changed exitstate to say invalid command.  This is now valid for 
        // unused PF keys and enter.  Sunya Sharp - 9/28/98. ***
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // *****
    // All processing completed successfully.  Move to previous view.
    // *****
    export.Previous.Notes = export.CashReceiptDetail.Notes;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Notes = source.Notes;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnAbReadCashRecDtlNote()
  {
    var useImport = new FnAbReadCashRecDtlNote.Import();
    var useExport = new FnAbReadCashRecDtlNote.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbReadCashRecDtlNote.Execute, useImport, useExport);

    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseUpdateCollectionTest()
  {
    var useImport = new UpdateCollectionTest.Import();
    var useExport = new UpdateCollectionTest.Export();

    useImport.CollectionType.SequentialIdentifier =
      export.HiddenCollectionType.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptDetail(export.CashReceiptDetail, useImport.CashReceiptDetail);
      

    Call(UpdateCollectionTest.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of HiddenCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenCollectionType")]
    public CollectionType HiddenCollectionType
    {
      get => hiddenCollectionType ??= new();
      set => hiddenCollectionType = value;
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
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private CashReceiptDetail previous;
    private CollectionType hiddenCollectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
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
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of HiddenCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenCollectionType")]
    public CollectionType HiddenCollectionType
    {
      get => hiddenCollectionType ??= new();
      set => hiddenCollectionType = value;
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
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private CashReceiptDetail previous;
    private CollectionType hiddenCollectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public Common Add
    {
      get => add ??= new();
      set => add = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CashReceiptDetail Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common add;
    private CashReceiptDetail null1;
  }
#endregion
}
