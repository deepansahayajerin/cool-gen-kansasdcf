// Program: FN_CREATE_OUTBND_CSENET_COLL, ID: 372449467, model: 746.
// Short name: SWE02453
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OUTBND_CSENET_COLL.
/// </summary>
[Serializable]
public partial class FnCreateOutbndCsenetColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OUTBND_CSENET_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateOutbndCsenetColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateOutbndCsenetColl.
  /// </summary>
  public FnCreateOutbndCsenetColl(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------
    // Date		Developer	Request		Desc
    // -------------------------------------------------------------
    // 01/23/2001	P Phinney	I00122005
    // add payment source
    // 03/30/2001	M Ramirez	114580 Seg A
    // Changed process_status to S (from R)
    // -------------------------------------------------------------
    // 04/10/2001	P Phinney	I00116873   Display COL on IREQ
    // -------------------------------------------------------------
    // 09/18/01  T Bobb     PR 00127601 Remove code to get new serial number 
    // when creating
    //  interstate history and changed the action code from
    // an "A" to a "P".
    // -------------------------------------------------------------------------------------
    // I00159319      11/15/02    P.Phinney  - COMPLETELY redid logic because 
    // program was not working correctly
    // -------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.InterstateCase.OtherFipsState = import.InterstateCase.OtherFipsState;
    local.InterstateCase.OtherFipsCounty =
      import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    local.InterstateCase.OtherFipsLocation =
      import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    local.InterstateCase.ActionCode = "P";
    local.InterstateCase.FunctionalTypeCode = "COL";
    local.InterstateCase.TransactionDate = import.Current.Date;
    local.InterstateCase.AttachmentsInd = "N";
    local.InterstateCase.KsCaseId = import.InterstateCase.KsCaseId ?? "";
    local.InterstateCase.InterstateCaseId =
      import.InterstateCase.InterstateCaseId ?? "";
    local.InterstateCase.ActionReasonCode = "CITAX";
    local.InterstateCase.CaseStatus = "O";
    UseSiGetDataInterstateCaseDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.InterstateCase.CollectionDataInd = 1;
    UseSiCreateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseSiCreateOgCsenetEnvelop();

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // I00122005      01/23/01    P.PHINNEY  - add PAYMENT_SOURCE
    MoveInterstateCollection2(import.InterstateCollection,
      local.InterstateCollection);
    local.InterstateCollection.SystemGeneratedSequenceNum = 1;
    local.InterstateCollection.DateOfPosting = import.Current.Date;
    UseSiCreateOgCsenetCollections();

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.CreateInterstateRequestHistory.ActionCode = "P";
    local.CreateInterstateRequestHistory.ActionReasonCode = "CITAX";
    local.CreateInterstateRequestHistory.AttachmentIndicator = "N";
    local.CreateInterstateRequestHistory.CreatedBy = global.UserId;
    local.CreateInterstateRequestHistory.CreatedTimestamp = Now();
    local.CreateInterstateRequestHistory.FunctionalTypeCode = "COL";
    local.CreateInterstateRequestHistory.Note =
      Spaces(InterstateRequestHistory.Note_MaxLength);
    local.CreateInterstateRequestHistory.TransactionDate = Now().Date;
    local.CreateInterstateRequestHistory.TransactionDirectionInd = "O";
    local.CreateInterstateRequestHistory.TransactionSerialNum =
      local.InterstateCase.TransSerialNumber;
    UseFnCreateCsenetOgIntstHist();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCollection1(InterstateCollection source,
    InterstateCollection target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.DateOfCollection = source.DateOfCollection;
    target.DateOfPosting = source.DateOfPosting;
    target.PaymentAmount = source.PaymentAmount;
    target.PaymentSource = source.PaymentSource;
    target.InterstatePaymentMethod = source.InterstatePaymentMethod;
  }

  private static void MoveInterstateCollection2(InterstateCollection source,
    InterstateCollection target)
  {
    target.DateOfCollection = source.DateOfCollection;
    target.PaymentAmount = source.PaymentAmount;
    target.PaymentSource = source.PaymentSource;
    target.InterstatePaymentMethod = source.InterstatePaymentMethod;
  }

  private void UseFnCreateCsenetOgIntstHist()
  {
    var useImport = new FnCreateCsenetOgIntstHist.Import();
    var useExport = new FnCreateCsenetOgIntstHist.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.Assign(
      local.CreateInterstateRequestHistory);

    Call(FnCreateCsenetOgIntstHist.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetCollections()
  {
    var useImport = new SiCreateOgCsenetCollections.Import();
    var useExport = new SiCreateOgCsenetCollections.Export();

    MoveInterstateCollection1(local.InterstateCollection,
      useImport.InterstateCollection);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetCollections.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = import.Current.Date;
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    private InterstateCase interstateCase;
    private InterstateCollection interstateCollection;
    private TextWorkArea userId;
    private DateWorkArea current;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
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
    /// A value of CreateInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("createInterstateRequestHistory")]
    public InterstateRequestHistory CreateInterstateRequestHistory
    {
      get => createInterstateRequestHistory ??= new();
      set => createInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of CreateCommon.
    /// </summary>
    [JsonPropertyName("createCommon")]
    public Common CreateCommon
    {
      get => createCommon ??= new();
      set => createCommon = value;
    }

    private InterstateCollection interstateCollection;
    private InterstateCase interstateCase;
    private InterstateRequestHistory createInterstateRequestHistory;
    private Common createCommon;
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
    /// A value of NewInterstateCase.
    /// </summary>
    [JsonPropertyName("newInterstateCase")]
    public InterstateCase NewInterstateCase
    {
      get => newInterstateCase ??= new();
      set => newInterstateCase = value;
    }

    /// <summary>
    /// A value of NewCsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("newCsenetTransactionEnvelop")]
    public CsenetTransactionEnvelop NewCsenetTransactionEnvelop
    {
      get => newCsenetTransactionEnvelop ??= new();
      set => newCsenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of NewInterstateCollection.
    /// </summary>
    [JsonPropertyName("newInterstateCollection")]
    public InterstateCollection NewInterstateCollection
    {
      get => newInterstateCollection ??= new();
      set => newInterstateCollection = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private InterstateCase newInterstateCase;
    private CsenetTransactionEnvelop newCsenetTransactionEnvelop;
    private InterstateCollection newInterstateCollection;
  }
#endregion
}
