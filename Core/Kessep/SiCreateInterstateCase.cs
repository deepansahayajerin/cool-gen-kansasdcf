// Program: SI_CREATE_INTERSTATE_CASE, ID: 372711825, model: 746.
// Short name: SWE02553
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_INTERSTATE_CASE.
/// </summary>
[Serializable]
public partial class SiCreateInterstateCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateCase.
  /// </summary>
  public SiCreateInterstateCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer       Description
    // 03/29/99 W.Campbell      Initial development
    // ----------------------------------------------
    try
    {
      CreateInterstateCase();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_CASE_AE";

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

  private void CreateInterstateCase()
  {
    var localFipsState = import.InterstateCase.LocalFipsState;
    var localFipsCounty =
      import.InterstateCase.LocalFipsCounty.GetValueOrDefault();
    var localFipsLocation =
      import.InterstateCase.LocalFipsLocation.GetValueOrDefault();
    var otherFipsState = import.InterstateCase.OtherFipsState;
    var otherFipsCounty =
      import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    var otherFipsLocation =
      import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    var transSerialNumber = import.InterstateCase.TransSerialNumber;
    var actionCode = import.InterstateCase.ActionCode;
    var functionalTypeCode = import.InterstateCase.FunctionalTypeCode;
    var transactionDate = import.InterstateCase.TransactionDate;
    var ksCaseId = import.InterstateCase.KsCaseId ?? "";
    var interstateCaseId = import.InterstateCase.InterstateCaseId ?? "";
    var actionReasonCode = import.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = import.InterstateCase.ActionResolutionDate;
    var attachmentsInd = import.InterstateCase.AttachmentsInd;
    var caseDataInd = import.InterstateCase.CaseDataInd.GetValueOrDefault();
    var apIdentificationInd =
      import.InterstateCase.ApIdentificationInd.GetValueOrDefault();
    var apLocateDataInd =
      import.InterstateCase.ApLocateDataInd.GetValueOrDefault();
    var participantDataInd =
      import.InterstateCase.ParticipantDataInd.GetValueOrDefault();
    var orderDataInd = import.InterstateCase.OrderDataInd.GetValueOrDefault();
    var collectionDataInd =
      import.InterstateCase.CollectionDataInd.GetValueOrDefault();
    var informationInd =
      import.InterstateCase.InformationInd.GetValueOrDefault();
    var sentDate = import.InterstateCase.SentDate;
    var sentTime = import.InterstateCase.SentTime.GetValueOrDefault();
    var dueDate = import.InterstateCase.DueDate;
    var overdueInd = import.InterstateCase.OverdueInd.GetValueOrDefault();
    var dateReceived = import.InterstateCase.DateReceived;
    var timeReceived = import.InterstateCase.TimeReceived.GetValueOrDefault();
    var attachmentsDueDate = import.InterstateCase.AttachmentsDueDate;
    var interstateFormsPrinted =
      import.InterstateCase.InterstateFormsPrinted ?? "";
    var caseType = import.InterstateCase.CaseType;
    var caseStatus = import.InterstateCase.CaseStatus;
    var paymentMailingAddressLine1 =
      import.InterstateCase.PaymentMailingAddressLine1 ?? "";
    var paymentAddressLine2 = import.InterstateCase.PaymentAddressLine2 ?? "";
    var paymentCity = import.InterstateCase.PaymentCity ?? "";
    var paymentState = import.InterstateCase.PaymentState ?? "";
    var paymentZipCode5 = import.InterstateCase.PaymentZipCode5 ?? "";
    var paymentZipCode4 = import.InterstateCase.PaymentZipCode4 ?? "";
    var contactNameLast = import.InterstateCase.ContactNameLast ?? "";
    var contactNameFirst = import.InterstateCase.ContactNameFirst ?? "";
    var contactNameMiddle = import.InterstateCase.ContactNameMiddle ?? "";
    var contactNameSuffix = import.InterstateCase.ContactNameSuffix ?? "";
    var contactAddressLine1 = import.InterstateCase.ContactAddressLine1;
    var contactAddressLine2 = import.InterstateCase.ContactAddressLine2 ?? "";
    var contactCity = import.InterstateCase.ContactCity ?? "";
    var contactState = import.InterstateCase.ContactState ?? "";
    var contactZipCode5 = import.InterstateCase.ContactZipCode5 ?? "";
    var contactZipCode4 = import.InterstateCase.ContactZipCode4 ?? "";
    var contactPhoneNum =
      import.InterstateCase.ContactPhoneNum.GetValueOrDefault();
    var assnDeactDt = import.InterstateCase.AssnDeactDt;
    var assnDeactInd = import.InterstateCase.AssnDeactInd ?? "";
    var lastDeferDt = import.InterstateCase.LastDeferDt;
    var memo = import.InterstateCase.Memo ?? "";
    var contactPhoneExtension = import.InterstateCase.ContactPhoneExtension ?? ""
      ;
    var contactFaxNumber =
      import.InterstateCase.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      import.InterstateCase.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      import.InterstateCase.ContactInternetAddress ?? "";
    var initiatingDocketNumber =
      import.InterstateCase.InitiatingDocketNumber ?? "";
    var sendPaymentsBankAccount =
      import.InterstateCase.SendPaymentsBankAccount ?? "";
    var sendPaymentsRoutingCode =
      import.InterstateCase.SendPaymentsRoutingCode.GetValueOrDefault();
    var nondisclosureFinding = import.InterstateCase.NondisclosureFinding ?? "";
    var respondingDocketNumber =
      import.InterstateCase.RespondingDocketNumber ?? "";
    var stateWithCej = import.InterstateCase.StateWithCej ?? "";
    var paymentFipsCounty = import.InterstateCase.PaymentFipsCounty ?? "";
    var paymentFipsState = import.InterstateCase.PaymentFipsState ?? "";
    var paymentFipsLocation = import.InterstateCase.PaymentFipsLocation ?? "";
    var contactAreaCode =
      import.InterstateCase.ContactAreaCode.GetValueOrDefault();

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase",
      (db, command) =>
      {
        db.SetInt32(command, "localFipsState", localFipsState);
        db.SetNullableInt32(command, "localFipsCounty", localFipsCounty);
        db.SetNullableInt32(command, "localFipsLocatio", localFipsLocation);
        db.SetInt32(command, "otherFipsState", otherFipsState);
        db.SetNullableInt32(command, "otherFipsCounty", otherFipsCounty);
        db.SetNullableInt32(command, "otherFipsLocatio", otherFipsLocation);
        db.SetInt64(command, "transSerialNbr", transSerialNumber);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "ksCaseId", ksCaseId);
        db.SetNullableString(command, "interstateCaseId", interstateCaseId);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResolution", actionResolutionDate);
        db.SetString(command, "attachmentsInd", attachmentsInd);
        db.SetNullableInt32(command, "caseDataInd", caseDataInd);
        db.SetNullableInt32(command, "apIdentification", apIdentificationInd);
        db.SetNullableInt32(command, "apLocateDataInd", apLocateDataInd);
        db.SetNullableInt32(command, "participantDataI", participantDataInd);
        db.SetNullableInt32(command, "orderDataInd", orderDataInd);
        db.SetNullableInt32(command, "collectionDataIn", collectionDataInd);
        db.SetNullableInt32(command, "informationInd", informationInd);
        db.SetNullableDate(command, "sentDate", sentDate);
        db.SetNullableTimeSpan(command, "sentTime", sentTime);
        db.SetNullableDate(command, "dueDate", dueDate);
        db.SetNullableInt32(command, "overdueInd", overdueInd);
        db.SetNullableDate(command, "dateReceived", dateReceived);
        db.SetNullableTimeSpan(command, "timeReceived", timeReceived);
        db.SetNullableDate(command, "attachmntsDueDte", attachmentsDueDate);
        db.
          SetNullableString(command, "interstateFormsP", interstateFormsPrinted);
          
        db.SetString(command, "caseType", caseType);
        db.SetString(command, "caseStatus", caseStatus);
        db.SetNullableString(
          command, "paymentMailingAd", paymentMailingAddressLine1);
        db.SetNullableString(command, "paymentAddressLi", paymentAddressLine2);
        db.SetNullableString(command, "paymentCity", paymentCity);
        db.SetNullableString(command, "paymentState", paymentState);
        db.SetNullableString(command, "paymentZipCode5", paymentZipCode5);
        db.SetNullableString(command, "paymentZipCode4", paymentZipCode4);
        db.SetNullableString(command, "zdelCpAddrLine1", "");
        db.SetNullableString(command, "zdelCpCity", "");
        db.SetNullableString(command, "zdelCpState", "");
        db.SetNullableString(command, "zdelCpZipCode5", "");
        db.SetNullableString(command, "zdelCpZipCode4", "");
        db.SetNullableString(command, "contactNameLast", contactNameLast);
        db.SetNullableString(command, "contactNameFirst", contactNameFirst);
        db.SetNullableString(command, "contactNameMiddl", contactNameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetString(command, "contactAddrLine1", contactAddressLine1);
        db.SetNullableString(command, "contactAddrLine2", contactAddressLine2);
        db.SetNullableString(command, "contactCity", contactCity);
        db.SetNullableString(command, "contactState", contactState);
        db.SetNullableString(command, "contactZipCode5", contactZipCode5);
        db.SetNullableString(command, "contactZipCode4", contactZipCode4);
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "assnDeactDt", assnDeactDt);
        db.SetNullableString(command, "assnDeactInd", assnDeactInd);
        db.SetNullableDate(command, "lastDeferDt", lastDeferDt);
        db.SetNullableString(command, "memo", memo);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "conFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "conInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "initDocketNum", initiatingDocketNumber);
        db.
          SetNullableString(command, "sendPaymBankAcc", sendPaymentsBankAccount);
          
        db.SetNullableInt64(command, "sendPaymRtCode", sendPaymentsRoutingCode);
        db.
          SetNullableString(command, "nondisclosureFind", nondisclosureFinding);
          
        db.SetNullableString(command, "respDocketNum", respondingDocketNumber);
        db.SetNullableString(command, "stateWithCej", stateWithCej);
        db.SetNullableString(command, "paymFipsCounty", paymentFipsCounty);
        db.SetNullableString(command, "paymentFipsState", paymentFipsState);
        db.SetNullableString(command, "paymFipsLocation", paymentFipsLocation);
        db.SetNullableInt32(command, "contactAreaCode", contactAreaCode);
      });

    entities.InterstateCase.LocalFipsState = localFipsState;
    entities.InterstateCase.LocalFipsCounty = localFipsCounty;
    entities.InterstateCase.LocalFipsLocation = localFipsLocation;
    entities.InterstateCase.OtherFipsState = otherFipsState;
    entities.InterstateCase.OtherFipsCounty = otherFipsCounty;
    entities.InterstateCase.OtherFipsLocation = otherFipsLocation;
    entities.InterstateCase.TransSerialNumber = transSerialNumber;
    entities.InterstateCase.ActionCode = actionCode;
    entities.InterstateCase.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateCase.TransactionDate = transactionDate;
    entities.InterstateCase.KsCaseId = ksCaseId;
    entities.InterstateCase.InterstateCaseId = interstateCaseId;
    entities.InterstateCase.ActionReasonCode = actionReasonCode;
    entities.InterstateCase.ActionResolutionDate = actionResolutionDate;
    entities.InterstateCase.AttachmentsInd = attachmentsInd;
    entities.InterstateCase.CaseDataInd = caseDataInd;
    entities.InterstateCase.ApIdentificationInd = apIdentificationInd;
    entities.InterstateCase.ApLocateDataInd = apLocateDataInd;
    entities.InterstateCase.ParticipantDataInd = participantDataInd;
    entities.InterstateCase.OrderDataInd = orderDataInd;
    entities.InterstateCase.CollectionDataInd = collectionDataInd;
    entities.InterstateCase.InformationInd = informationInd;
    entities.InterstateCase.SentDate = sentDate;
    entities.InterstateCase.SentTime = sentTime;
    entities.InterstateCase.DueDate = dueDate;
    entities.InterstateCase.OverdueInd = overdueInd;
    entities.InterstateCase.DateReceived = dateReceived;
    entities.InterstateCase.TimeReceived = timeReceived;
    entities.InterstateCase.AttachmentsDueDate = attachmentsDueDate;
    entities.InterstateCase.InterstateFormsPrinted = interstateFormsPrinted;
    entities.InterstateCase.CaseType = caseType;
    entities.InterstateCase.CaseStatus = caseStatus;
    entities.InterstateCase.PaymentMailingAddressLine1 =
      paymentMailingAddressLine1;
    entities.InterstateCase.PaymentAddressLine2 = paymentAddressLine2;
    entities.InterstateCase.PaymentCity = paymentCity;
    entities.InterstateCase.PaymentState = paymentState;
    entities.InterstateCase.PaymentZipCode5 = paymentZipCode5;
    entities.InterstateCase.PaymentZipCode4 = paymentZipCode4;
    entities.InterstateCase.ContactNameLast = contactNameLast;
    entities.InterstateCase.ContactNameFirst = contactNameFirst;
    entities.InterstateCase.ContactNameMiddle = contactNameMiddle;
    entities.InterstateCase.ContactNameSuffix = contactNameSuffix;
    entities.InterstateCase.ContactAddressLine1 = contactAddressLine1;
    entities.InterstateCase.ContactAddressLine2 = contactAddressLine2;
    entities.InterstateCase.ContactCity = contactCity;
    entities.InterstateCase.ContactState = contactState;
    entities.InterstateCase.ContactZipCode5 = contactZipCode5;
    entities.InterstateCase.ContactZipCode4 = contactZipCode4;
    entities.InterstateCase.ContactPhoneNum = contactPhoneNum;
    entities.InterstateCase.AssnDeactDt = assnDeactDt;
    entities.InterstateCase.AssnDeactInd = assnDeactInd;
    entities.InterstateCase.LastDeferDt = lastDeferDt;
    entities.InterstateCase.Memo = memo;
    entities.InterstateCase.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateCase.ContactFaxNumber = contactFaxNumber;
    entities.InterstateCase.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateCase.ContactInternetAddress = contactInternetAddress;
    entities.InterstateCase.InitiatingDocketNumber = initiatingDocketNumber;
    entities.InterstateCase.SendPaymentsBankAccount = sendPaymentsBankAccount;
    entities.InterstateCase.SendPaymentsRoutingCode = sendPaymentsRoutingCode;
    entities.InterstateCase.NondisclosureFinding = nondisclosureFinding;
    entities.InterstateCase.RespondingDocketNumber = respondingDocketNumber;
    entities.InterstateCase.StateWithCej = stateWithCej;
    entities.InterstateCase.PaymentFipsCounty = paymentFipsCounty;
    entities.InterstateCase.PaymentFipsState = paymentFipsState;
    entities.InterstateCase.PaymentFipsLocation = paymentFipsLocation;
    entities.InterstateCase.ContactAreaCode = contactAreaCode;
    entities.InterstateCase.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }
#endregion
}
