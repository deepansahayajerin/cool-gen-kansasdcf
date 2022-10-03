// Program: SI_PERS_PGM_CREATE_CSENET_TRAN, ID: 372712732, model: 746.
// Short name: SWE02575
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_PERS_PGM_CREATE_CSENET_TRAN.
/// </summary>
[Serializable]
public partial class SiPersPgmCreateCsenetTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PERS_PGM_CREATE_CSENET_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPersPgmCreateCsenetTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPersPgmCreateCsenetTran.
  /// </summary>
  public SiPersPgmCreateCsenetTran(IContext context, Import import,
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
    // ****************************************************************
    // 12/06/1999   C. Ott   PR # 73241, prevent multiple CSENet transactions 
    // from being sent to the same state on same day for the same program.
    // ****************************************************************
    // **************************************************************
    // 01/14/00  C. Ott   PR # 85296.  Check whether other state is ready to 
    // receive transactions through CSENet.
    // **************************************************************
    // **************************************************************
    // 03/28/01  M. Lachowicz PR # 112832.  Add UR clause for READ EACH 
    // INTERSATE_CASE to avoid duplicate transactions.
    // **************************************************************
    // **************************************************************
    // 04/9/01  M. Ashworth PR # 131908.  Add quilification to read interstate 
    // case so it will only read for outgoing intersate cases.  (kansas case ind
    // = Y) This also will solve the problem for PR 132667.
    // **************************************************************
    // **************************************************************
    // 07/10/06  G Vandy   WR # 230751.  Skip CSENet processing for Foreign and 
    // Tribal IV-D agencies.
    // **************************************************************
    // *************************************************************
    // 07/31/07  G. Pan   PR218020 Mapped two entities for calling
    //                    si_get_payment_mailing_address
    // *************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Null1.Date = null;

    if (ReadCsePerson())
    {
      // **************************************************************
      // 04/9/01  M. Ashworth PR # 131908.  Add quilification to read interstate
      // case so it will only read for outgoing intersate cases.  (kansas case
      // ind = Y) This also will solve the problem for PR 132667.
      // **************************************************************
      foreach(var item in ReadInterstateRequestCaseCaseRole())
      {
        // **************************************************************
        // On an Incoming interstate case, do not send an interstate transaction
        // to the other state notifying them of the case type cghange on their
        // case.
        // This would occur when the PEPR screen (Tran code = SR2M) updates the 
        // incoming case type.
        // **************************************************************
        if (Equal(global.TranCode, "SR2M") && AsChar
          (entities.InterstateRequest.KsCaseInd) != 'Y')
        {
          continue;
        }

        // **************************************************************
        // 01/14/00  C. Ott   PR # 85296.  Check whether other state is ready to
        // receive transactions through CSENet.
        // **************************************************************
        if (entities.InterstateRequest.OtherStateFips == 0)
        {
          // --  Don't send a CSENet transaction if the other state fips code is
          // zero (i.e. it is a tribal or foreign interstate request)
          local.SendCsenetTransaction.Flag = "N";
        }
        else
        {
          local.SendCsenetTransaction.Flag = "Y";
          ReadFips();
          local.CsenetStateTable.StateCode = entities.Fips.StateAbbreviation;
          UseSiReadCsenetStateTable();

          if (AsChar(local.CsenetStateTable.CsenetReadyInd) != 'Y' || AsChar
            (local.CsenetStateTable.RecStateInd) != 'Y')
          {
            // **************************************************************
            // Other state is not ready to receive transactions through CSENet.
            // **************************************************************
            local.SendCsenetTransaction.Flag = "N";
          }
        }

        local.Case1.Assign(entities.Case1);
        local.InterstateCase.ActionCode = "P";
        local.InterstateCase.ActionReasonCode = "GSTYP";
        local.InterstateCase.FunctionalTypeCode = "MSC";
        local.InterstateCase.AttachmentsInd = "";

        // ****************************************************************
        // 12/06/1999   C. Ott   PR # 73241, prevent multiple CSENet 
        // transactions from being sent to the same state on same day for the
        // same program.
        // ****************************************************************
        foreach(var item1 in ReadInterstateRequestHistory())
        {
          // 03/28/01 M.L Start
          foreach(var item2 in ReadInterstateCase())
          {
            goto ReadEach;
          }

          // 03/28/01 M.L End
        }

        UseSiReadCaseProgramType();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // **************************************************************
          // 4-9-02 MCA PR 132667 Do not allow update of interstate request and 
          // further IO if there is an error finding the case program type.  The
          // original problem was the update was setting the interstate request
          // case type to spaces
          // **************************************************************
          return;
        }

        if (Equal(local.Program.Code, entities.InterstateRequest.CaseType))
        {
          // **************************************************************
          // No change in Case Type for this Interstate Request.  No Case Type 
          // change transaction should be sent.
          // **************************************************************
          continue;
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
              ExitState = "INTERSTATE_REQUEST_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INTERSTATE_REQUEST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (AsChar(local.SendCsenetTransaction.Flag) != 'Y')
        {
          // *************************************************************
          // Other state is not ready to receive CSENet transaction.
          // *************************************************************
          continue;
        }

        UseSiGenCsenetTransactSerialNo();

        try
        {
          CreateInterstateRequestHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_INTERSTAT_REQ_HIST_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_INTERSTAT_REQ_HIST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // ***      Set KS as the Local FIPS.       ***
        local.InterstateCase.LocalFipsState = 20;
        local.InterstateCase.LocalFipsCounty = 0;
        local.InterstateCase.LocalFipsLocation = 0;

        // ***      Set the other State FIPS.        ***
        local.InterstateCase.OtherFipsState =
          entities.InterstateRequest.OtherStateFips;
        local.InterstateCase.OtherFipsCounty = 0;
        local.InterstateCase.OtherFipsLocation = 0;
        local.InterstateCase.KsCaseId = local.Case1.Number;
        local.InterstateCase.InterstateCaseId =
          entities.InterstateRequest.OtherStateCaseId;

        // ----------------------------------
        // Set attributes for Interstate Case
        // ----------------------------------
        local.InterstateCase.CaseDataInd = 1;
        local.InterstateCase.InformationInd = 0;
        local.InterstateCase.OrderDataInd = 0;
        local.InterstateCase.ApIdentificationInd = 0;
        local.InterstateCase.ApLocateDataInd = 0;
        local.InterstateCase.CollectionDataInd = 0;
        local.InterstateCase.ParticipantDataInd = 0;
        local.InterstateCase.AssnDeactDt = local.Null1.Date;
        local.InterstateCase.AttachmentsDueDate = local.Null1.Date;
        local.InterstateCase.DateReceived = local.Null1.Date;
        local.InterstateCase.DueDate = local.Null1.Date;
        local.InterstateCase.LastDeferDt = local.Null1.Date;
        local.InterstateCase.SentDate = local.Null1.Date;
        local.InterstateCase.ActionResolutionDate = local.Null1.Date;
        local.InterstateCase.CaseType = local.Program.Code;
        local.InterstateCase.CaseStatus = local.Case1.Status ?? Spaces(1);
        local.InterstateCase.AssnDeactInd = "";
        local.InterstateCase.InterstateFormsPrinted = "";

        if (ReadLegalAction())
        {
          UseSiGetPaymentMailingAddress();
        }

        if (ReadCaseAssignment())
        {
          if (ReadServiceProviderOfficeServiceProviderOffice())
          {
            local.InterstateCase.ContactNameFirst =
              entities.ServiceProvider.FirstName;
            local.InterstateCase.ContactNameLast =
              entities.ServiceProvider.LastName;
            local.InterstateCase.ContactNameMiddle =
              entities.ServiceProvider.MiddleInitial;
            local.InterstateCase.ContactAreaCode =
              entities.OfficeServiceProvider.WorkPhoneAreaCode;
            local.InterstateCase.ContactPhoneNum =
              entities.OfficeServiceProvider.WorkPhoneNumber;
            local.InterstateCase.ContactPhoneExtension =
              entities.OfficeServiceProvider.WorkPhoneExtension;
            local.InterstateCase.ContactFaxAreaCode =
              entities.OfficeServiceProvider.WorkFaxAreaCode;
            local.InterstateCase.ContactFaxNumber =
              entities.OfficeServiceProvider.WorkFaxNumber;

            if (ReadServiceProviderAddress())
            {
              local.InterstateCase.ContactAddressLine1 =
                entities.ServiceProviderAddress.Street1;
              local.InterstateCase.ContactAddressLine2 =
                entities.ServiceProviderAddress.Street2;
              local.InterstateCase.ContactCity =
                entities.ServiceProviderAddress.City;
              local.InterstateCase.ContactState =
                entities.ServiceProviderAddress.StateProvince;
              local.InterstateCase.ContactZipCode5 =
                entities.ServiceProviderAddress.Zip;
              local.InterstateCase.ContactZipCode4 =
                entities.ServiceProviderAddress.Zip4;
            }
            else if (ReadOfficeAddress())
            {
              local.InterstateCase.ContactAddressLine1 =
                entities.OfficeAddress.Street1;
              local.InterstateCase.ContactAddressLine2 =
                entities.OfficeAddress.Street2;
              local.InterstateCase.ContactCity = entities.OfficeAddress.City;
              local.InterstateCase.ContactState =
                entities.OfficeAddress.StateProvince;
              local.InterstateCase.ContactZipCode5 = entities.OfficeAddress.Zip;
              local.InterstateCase.ContactZipCode4 =
                entities.OfficeAddress.Zip4;
            }
          }
        }

        // *****************************************************************
        // State Treasurer's office confirms the following:
        // ABA routing number for UMB:  101000695
        // Sub account number at UMB:  9870969688
        // ****************************************************************
        local.InterstateCase.SendPaymentsRoutingCode = 101000695;
        local.InterstateCase.SendPaymentsBankAccount = "9870969688";

        try
        {
          CreateInterstateCase();
          UseSiCreateOgCsenetEnvelop();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
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

ReadEach:
        ;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase1(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiGetPaymentMailingAddress()
  {
    var useImport = new SiGetPaymentMailingAddress.Import();
    var useExport = new SiGetPaymentMailingAddress.Export();

    useImport.Ap.Number = entities.CsePerson.Number;
    useImport.Case1.Number = local.Case1.Number;
    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;

    Call(SiGetPaymentMailingAddress.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void CreateInterstateCase()
  {
    var localFipsState = local.InterstateCase.LocalFipsState;
    var localFipsCounty =
      local.InterstateCase.LocalFipsCounty.GetValueOrDefault();
    var localFipsLocation =
      local.InterstateCase.LocalFipsLocation.GetValueOrDefault();
    var otherFipsState = local.InterstateCase.OtherFipsState;
    var otherFipsCounty =
      local.InterstateCase.OtherFipsCounty.GetValueOrDefault();
    var otherFipsLocation =
      local.InterstateCase.OtherFipsLocation.GetValueOrDefault();
    var transSerialNumber = local.InterstateCase.TransSerialNumber;
    var actionCode = local.InterstateCase.ActionCode;
    var functionalTypeCode = local.InterstateCase.FunctionalTypeCode;
    var transactionDate = local.InterstateCase.TransactionDate;
    var ksCaseId = local.InterstateCase.KsCaseId ?? "";
    var interstateCaseId = local.InterstateCase.InterstateCaseId ?? "";
    var actionReasonCode = local.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = local.InterstateCase.ActionResolutionDate;
    var attachmentsInd = local.InterstateCase.AttachmentsInd;
    var caseDataInd = local.InterstateCase.CaseDataInd.GetValueOrDefault();
    var apIdentificationInd =
      local.InterstateCase.ApIdentificationInd.GetValueOrDefault();
    var apLocateDataInd =
      local.InterstateCase.ApLocateDataInd.GetValueOrDefault();
    var participantDataInd =
      local.InterstateCase.ParticipantDataInd.GetValueOrDefault();
    var orderDataInd = local.InterstateCase.OrderDataInd.GetValueOrDefault();
    var collectionDataInd =
      local.InterstateCase.CollectionDataInd.GetValueOrDefault();
    var informationInd =
      local.InterstateCase.InformationInd.GetValueOrDefault();
    var sentDate = local.InterstateCase.SentDate;
    var sentTime = local.InterstateCase.SentTime.GetValueOrDefault();
    var dueDate = local.InterstateCase.DueDate;
    var overdueInd = local.InterstateCase.OverdueInd.GetValueOrDefault();
    var dateReceived = local.InterstateCase.DateReceived;
    var timeReceived = local.InterstateCase.TimeReceived.GetValueOrDefault();
    var attachmentsDueDate = local.InterstateCase.AttachmentsDueDate;
    var interstateFormsPrinted =
      local.InterstateCase.InterstateFormsPrinted ?? "";
    var caseType = local.InterstateCase.CaseType;
    var caseStatus = local.InterstateCase.CaseStatus;
    var paymentMailingAddressLine1 =
      local.InterstateCase.PaymentMailingAddressLine1 ?? "";
    var paymentAddressLine2 = local.InterstateCase.PaymentAddressLine2 ?? "";
    var paymentCity = local.InterstateCase.PaymentCity ?? "";
    var paymentState = local.InterstateCase.PaymentState ?? "";
    var paymentZipCode5 = local.InterstateCase.PaymentZipCode5 ?? "";
    var paymentZipCode4 = local.InterstateCase.PaymentZipCode4 ?? "";
    var contactNameLast = local.InterstateCase.ContactNameLast ?? "";
    var contactNameFirst = local.InterstateCase.ContactNameFirst ?? "";
    var contactNameMiddle = local.InterstateCase.ContactNameMiddle ?? "";
    var contactNameSuffix = local.InterstateCase.ContactNameSuffix ?? "";
    var contactAddressLine1 = local.InterstateCase.ContactAddressLine1;
    var contactAddressLine2 = local.InterstateCase.ContactAddressLine2 ?? "";
    var contactCity = local.InterstateCase.ContactCity ?? "";
    var contactState = local.InterstateCase.ContactState ?? "";
    var contactZipCode5 = local.InterstateCase.ContactZipCode5 ?? "";
    var contactZipCode4 = local.InterstateCase.ContactZipCode4 ?? "";
    var contactPhoneNum =
      local.InterstateCase.ContactPhoneNum.GetValueOrDefault();
    var assnDeactDt = local.InterstateCase.AssnDeactDt;
    var assnDeactInd = local.InterstateCase.AssnDeactInd ?? "";
    var lastDeferDt = local.InterstateCase.LastDeferDt;
    var memo = local.InterstateCase.Memo ?? "";
    var contactPhoneExtension = local.InterstateCase.ContactPhoneExtension ?? ""
      ;
    var contactFaxNumber =
      local.InterstateCase.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      local.InterstateCase.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      local.InterstateCase.ContactInternetAddress ?? "";
    var initiatingDocketNumber =
      local.InterstateCase.InitiatingDocketNumber ?? "";
    var sendPaymentsBankAccount =
      local.InterstateCase.SendPaymentsBankAccount ?? "";
    var sendPaymentsRoutingCode =
      local.InterstateCase.SendPaymentsRoutingCode.GetValueOrDefault();
    var nondisclosureFinding = local.InterstateCase.NondisclosureFinding ?? "";
    var respondingDocketNumber =
      local.InterstateCase.RespondingDocketNumber ?? "";
    var stateWithCej = local.InterstateCase.StateWithCej ?? "";
    var paymentFipsCounty = local.InterstateCase.PaymentFipsCounty ?? "";
    var paymentFipsState = local.InterstateCase.PaymentFipsState ?? "";
    var paymentFipsLocation = local.InterstateCase.PaymentFipsLocation ?? "";
    var contactAreaCode =
      local.InterstateCase.ContactAreaCode.GetValueOrDefault();

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

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var transactionDirectionInd = "O";
    var transactionSerialNum = local.InterstateCase.TransSerialNumber;
    var actionCode = local.InterstateCase.ActionCode;
    var functionalTypeCode = local.InterstateCase.FunctionalTypeCode;
    var transactionDate = local.Current.Date;
    var actionReasonCode = local.InterstateCase.ActionReasonCode ?? "";
    var actionResolutionDate = local.Null1.Date;
    var attachmentIndicator = local.InterstateCase.AttachmentsInd;
    var note = Spaces(400);

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

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetInt32(command, "state", entities.InterstateRequest.OtherStateFips);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase()
  {
    entities.UncommitedRead.Populated = false;

    return ReadEach("ReadInterstateCase",
      (db, command) =>
      {
        db.SetString(command, "actionCode", local.InterstateCase.ActionCode);
        db.SetNullableString(
          command, "actionReasonCode",
          local.InterstateCase.ActionReasonCode ?? "");
        db.SetDate(
          command, "transactionDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "caseType", import.Program.Code);
      },
      (db, reader) =>
      {
        entities.UncommitedRead.TransSerialNumber = db.GetInt64(reader, 0);
        entities.UncommitedRead.ActionCode = db.GetString(reader, 1);
        entities.UncommitedRead.TransactionDate = db.GetDate(reader, 2);
        entities.UncommitedRead.ActionReasonCode =
          db.GetNullableString(reader, 3);
        entities.UncommitedRead.CaseType = db.GetString(reader, 4);
        entities.UncommitedRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
        entities.Case1.Number = db.GetString(reader, 12);
        entities.CaseRole.CasNumber = db.GetString(reader, 12);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 13);
        entities.Case1.Status = db.GetNullableString(reader, 14);
        entities.CaseRole.CspNumber = db.GetString(reader, 15);
        entities.CaseRole.Type1 = db.GetString(reader, 16);
        entities.CaseRole.Identifier = db.GetInt32(reader, 17);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 18);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 19);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
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
        db.SetString(command, "actionCode", local.InterstateCase.ActionCode);
        db.SetNullableString(
          command, "actionReasonCode",
          local.InterstateCase.ActionReasonCode ?? "");
        db.SetDate(
          command, "transactionDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 8);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 9);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 12);
        entities.Office.Name = db.GetString(reader, 13);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 14);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private void UpdateInterstateRequest()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var caseType = local.Program.Code;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "caseType", caseType);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.CaseType = caseType;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private Program program;
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
    /// A value of SendCsenetTransaction.
    /// </summary>
    [JsonPropertyName("sendCsenetTransaction")]
    public Common SendCsenetTransaction
    {
      get => sendCsenetTransaction ??= new();
      set => sendCsenetTransaction = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private Common sendCsenetTransaction;
    private CsenetStateTable csenetStateTable;
    private Program program;
    private Case1 case1;
    private DateWorkArea null1;
    private DateWorkArea current;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of UncommitedRead.
    /// </summary>
    [JsonPropertyName("uncommitedRead")]
    public InterstateCase UncommitedRead
    {
      get => uncommitedRead ??= new();
      set => uncommitedRead = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private InterstateCase uncommitedRead;
    private Fips fips;
    private CaseRole caseRole;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private InterstateCase interstateCase;
    private CaseRole absentParent;
    private CsePerson csePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeAddress officeAddress;
  }
#endregion
}
