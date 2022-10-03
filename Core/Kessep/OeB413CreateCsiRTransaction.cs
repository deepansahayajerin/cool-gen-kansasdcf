// Program: OE_B413_CREATE_CSI_R_TRANSACTION, ID: 373437156, model: 746.
// Short name: SWE01966
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B413_CREATE_CSI_R_TRANSACTION.
/// </summary>
[Serializable]
public partial class OeB413CreateCsiRTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B413_CREATE_CSI_R_TRANSACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB413CreateCsiRTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB413CreateCsiRTransaction.
  /// </summary>
  public OeB413CreateCsiRTransaction(IContext context, Import import,
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
    // *** change log    A Hockman  11/23/04   made change to remove leading 
    // zeroes   WR 040139 for ICR
    local.Current.Date = import.ProgramProcessingInfo.ProcessDate;
    local.InterstateCase.FunctionalTypeCode = "CSI";
    local.InterstateCase.ActionCode = "R";
    local.InterstateCase.ActionReasonCode = "FRINF";
    local.InterstateCase.OtherFipsState =
      (int)StringToNumber(import.FcrProactiveMatchResponse.
        TransmitterStateOrTerrCode);
    local.InterstateCase.OtherFipsCounty = 0;
    local.InterstateCase.OtherFipsLocation = 0;
    local.InterstateCase.InterstateCaseId =
      import.FcrProactiveMatchResponse.MatchedCaseId ?? "";
    UseSiGetDataInterstateCaseDb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.InterstateCase.Memo =
      "This request was triggered by a proactive match.";

    // ***   wr 040139 changing this to stop inserting leading zeroes in front 
    // of the case number.   A Hockman 11/23/04
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
    else
    {
      return;
    }

    UseSiCreateIsRequest();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.InterstateRequestHistory.ActionCode = "R";
    local.InterstateRequestHistory.FunctionalTypeCode = "CSI";
    local.InterstateRequestHistory.TransactionDirectionInd = "O";
    local.InterstateRequestHistory.AttachmentIndicator = "";
    local.InterstateRequestHistory.Note =
      Spaces(InterstateRequestHistory.Note_MaxLength);
    local.InterstateRequestHistory.TransactionSerialNum =
      local.InterstateCase.TransSerialNumber;
    local.InterstateRequestHistory.TransactionDate = Now().Date;
    local.InterstateRequestHistory.CreatedBy =
      import.ProgramProcessingInfo.Name;

    if (AsChar(import.FcrProactiveMatchResponse.MatchedCaseType) == 'F')
    {
      local.InterstateRequestHistory.ActionReasonCode = "FRINF";
    }
    else
    {
      local.InterstateRequestHistory.ActionReasonCode = "FRNNF";
    }

    UseSiCabCreateIsRequestHistory();
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.AssnDeactDt = source.AssnDeactDt;
    target.AssnDeactInd = source.AssnDeactInd;
    target.LastDeferDt = source.LastDeferDt;
    target.Memo = source.Memo;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.InitiatingDocketNumber = source.InitiatingDocketNumber;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
    target.NondisclosureFinding = source.NondisclosureFinding;
    target.RespondingDocketNumber = source.RespondingDocketNumber;
    target.StateWithCej = source.StateWithCej;
    target.PaymentFipsCounty = source.PaymentFipsCounty;
    target.PaymentFipsState = source.PaymentFipsState;
    target.PaymentFipsLocation = source.PaymentFipsLocation;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateCase2(InterstateCase source,
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

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseSiCabCreateIsRequestHistory()
  {
    var useImport = new SiCabCreateIsRequestHistory.Import();
    var useExport = new SiCabCreateIsRequestHistory.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      local.InterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCabCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateIsRequest()
  {
    var useImport = new SiCreateIsRequest.Import();
    var useExport = new SiCreateIsRequest.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Ap.Number = import.CsePerson.Number;
    MoveInterstateCase3(local.InterstateCase, useImport.Incoming);
    useImport.InterstateRequest.Assign(local.InterstateRequest);

    Call(SiCreateIsRequest.Execute, useImport, useExport);

    local.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase4(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    useImport.Case1.Number = import.Case1.Number;
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.Current.Date = local.Current.Date;

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    MoveInterstateCase1(useExport.InterstateCase, local.InterstateCase);
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
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private Case1 case1;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private DateWorkArea current;
  }
#endregion
}
