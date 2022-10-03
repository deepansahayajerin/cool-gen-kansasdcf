// Program: SI_ICAS_REGISTER_DUPLICATE_CASE, ID: 372741410, model: 746.
// Short name: SWE02592
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ICAS_REGISTER_DUPLICATE_CASE.
/// </para>
/// <para>
/// This CAB creates an acknowledgement (similar to REGI) and updates the 
/// interstate case entity, setting the Duplicate Case Indicator flag to 'Y',
/// and deactivates the referral.
/// </para>
/// </summary>
[Serializable]
public partial class SiIcasRegisterDuplicateCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ICAS_REGISTER_DUPLICATE_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIcasRegisterDuplicateCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIcasRegisterDuplicateCase.
  /// </summary>
  public SiIcasRegisterDuplicateCase(IContext context, Import import,
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
    // ****************************************************************************
    // 05/12/99 C Scroggins This CAB creates the Interstate case 
    // acknowledgement, sets the
    //          duplicate case indicator for the existing Kansas Case, and 
    // deactivates the
    //          referral.
    // 06/22/00 C Scroggins PR# 82996 Modified CAB to NOT automatically update 
    // the duplicate
    //          case indicator.
    // ****************************************************************************
    export.CsePerson.Number = import.CsePerson.Number;
    local.Ap.Number = import.CsePerson.Number;
    MoveInterstateCase1(import.InterstateCase, export.InterstateCase);
    MoveInterstateCase3(export.InterstateCase, export.From);
    local.IncIcInvolvement.Flag = "";
    local.OgIcInvolvement.Flag = "";
    local.InterstateInvolvement.Count = 0;

    foreach(var item in ReadInterstateCaseCsenetTransactionEnvelop())
    {
      if (AsChar(entities.CsenetTransactionEnvelop.DirectionInd) == 'I')
      {
        local.IncIcInvolvement.Flag = "Y";
        ++local.InterstateInvolvement.Count;
      }

      if (AsChar(entities.CsenetTransactionEnvelop.DirectionInd) == 'O')
      {
        local.OgIcInvolvement.Flag = "Y";
      }
    }

    if (ReadCase())
    {
      local.Case1.DuplicateCaseIndicator =
        entities.Case1.DuplicateCaseIndicator;

      try
      {
        UpdateCase();
        export.Case1.Assign(entities.Case1);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_REGISTER_UNSCUCCESSFUL";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_REGISTER_UNSCUCCESSFUL";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadInterstateCase())
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
            ExitState = "SI0000_REGISTER_UNSCUCCESSFUL";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_REGISTER_UNSCUCCESSFUL";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Case1.Number = "";
      export.NotFromReferral.Flag = "";
      UseEabRollbackCics();

      return;
    }

    UseSiCreateIcIsReqFrmReferral();

    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Case1.Number = "";
      export.NotFromReferral.Flag = "";
      UseEabRollbackCics();

      return;
    }

    UseSiUpdateReferralDeactStatus();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Case1.Number = "";
      export.NotFromReferral.Flag = "";
      UseEabRollbackCics();

      return;
    }

    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.InterstateCase);
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
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
    target.DateReceived = source.DateReceived;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ZdelCpAddrLine1 = source.ZdelCpAddrLine1;
    target.ZdelCpAddrLine2 = source.ZdelCpAddrLine2;
    target.ZdelCpCity = source.ZdelCpCity;
    target.ZdelCpState = source.ZdelCpState;
    target.ZdelCpZipCode5 = source.ZdelCpZipCode5;
    target.ZdelCpZipCode4 = source.ZdelCpZipCode4;
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
    target.AssnDeactInd = source.AssnDeactInd;
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
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.CaseType = source.CaseType;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.AssnDeactInd = source.AssnDeactInd;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSiCreateIcIsReqFrmReferral()
  {
    var useImport = new SiCreateIcIsReqFrmReferral.Import();
    var useExport = new SiCreateIcIsReqFrmReferral.Export();

    useImport.Ap.Number = local.Ap.Number;
    useImport.NewlyCreated.Number = export.Case1.Number;
    MoveInterstateCase2(export.From, useImport.InterstateCase);

    Call(SiCreateIcIsReqFrmReferral.Execute, useImport, useExport);
  }

  private void UseSiUpdateReferralDeactStatus()
  {
    var useImport = new SiUpdateReferralDeactStatus.Import();
    var useExport = new SiUpdateReferralDeactStatus.Export();

    MoveInterstateCase2(export.From, useImport.InterstateCase);

    Call(SiUpdateReferralDeactStatus.Execute, useImport, useExport);

    MoveInterstateCase4(useExport.InterstateCase, export.InterstateCase);
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
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.ManagementArea = db.GetNullableString(reader, 1);
        entities.Case1.ManagementRegion = db.GetNullableString(reader, 2);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 3);
        entities.Case1.LocateInd = db.GetNullableString(reader, 4);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 6);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 7);
        entities.Case1.StationId = db.GetNullableString(reader, 8);
        entities.Case1.ApplicantLastName = db.GetNullableString(reader, 9);
        entities.Case1.ApplicantFirstName = db.GetNullableString(reader, 10);
        entities.Case1.ApplicantMiddleInitial =
          db.GetNullableString(reader, 11);
        entities.Case1.ApplicationSentDate = db.GetNullableDate(reader, 12);
        entities.Case1.ApplicationRequestDate = db.GetNullableDate(reader, 13);
        entities.Case1.ApplicationReturnDate = db.GetNullableDate(reader, 14);
        entities.Case1.DeniedRequestDate = db.GetNullableDate(reader, 15);
        entities.Case1.DeniedRequestCode = db.GetNullableString(reader, 16);
        entities.Case1.DeniedRequestReason = db.GetNullableString(reader, 17);
        entities.Case1.Status = db.GetNullableString(reader, 18);
        entities.Case1.KsFipsCode = db.GetNullableString(reader, 19);
        entities.Case1.ValidApplicationReceivedDate =
          db.GetNullableDate(reader, 20);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 21);
        entities.Case1.Potential = db.GetNullableString(reader, 22);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 23);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 24);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 26);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 27);
        entities.Case1.CreatedBy = db.GetString(reader, 28);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 29);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 30);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 31);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 32);
        entities.Case1.PaRefCreatedTimestamp = db.GetDateTime(reader, 33);
        entities.Case1.PaReferralNumber = db.GetString(reader, 34);
        entities.Case1.PaReferralType = db.GetString(reader, 35);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 36);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 37);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 38);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 39);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 40);
        entities.Case1.Note = db.GetNullableString(reader, 41);
        entities.Case1.Populated = true;
        CheckValid<Case1>("Potential", entities.Case1.Potential);
      });
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
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 6);
        entities.InterstateCase.ActionCode = db.GetString(reader, 7);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 10);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 14);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 15);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 22);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 23);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 24);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 26);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.InterstateCase.CaseType = db.GetString(reader, 30);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 34);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 35);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ZdelCpAddrLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.ZdelCpAddrLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.ZdelCpCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.ZdelCpState = db.GetNullableString(reader, 41);
        entities.InterstateCase.ZdelCpZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.ZdelCpZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 59);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 60);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 61);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 65);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 67);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 68);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 69);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 70);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 72);
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "number", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 6);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 6);
        entities.InterstateCase.ActionCode = db.GetString(reader, 7);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 9);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 10);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 14);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 15);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 22);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 23);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 24);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 26);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.InterstateCase.CaseType = db.GetString(reader, 30);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 34);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 35);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ZdelCpAddrLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.ZdelCpAddrLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.ZdelCpCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.ZdelCpState = db.GetNullableString(reader, 41);
        entities.InterstateCase.ZdelCpZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.ZdelCpZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 59);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 60);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 61);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 65);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 67);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 68);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 69);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 70);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 72);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 73);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 74);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 75);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 76);
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 77);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 78);
        entities.CsenetTransactionEnvelop.Populated = true;
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private void UpdateCase()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var duplicateCaseIndicator = local.Case1.DuplicateCaseIndicator ?? "";

    entities.Case1.Populated = false;
    Update("UpdateCase",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableString(command, "dupCaseIndicator", duplicateCaseIndicator);
          
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.DuplicateCaseIndicator = duplicateCaseIndicator;
    entities.Case1.Populated = true;
  }

  private void UpdateInterstateCase()
  {
    var ksCaseId = import.Case1.Number;

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(command, "ksCaseId", ksCaseId);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.KsCaseId = ksCaseId;
    entities.InterstateCase.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsePerson csePerson;
    private Case1 case1;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public InterstateCase From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of NotFromReferral.
    /// </summary>
    [JsonPropertyName("notFromReferral")]
    public Common NotFromReferral
    {
      get => notFromReferral ??= new();
      set => notFromReferral = value;
    }

    private CsePerson csePerson;
    private InterstateCase interstateCase;
    private Case1 case1;
    private InterstateCase from;
    private Common notFromReferral;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterstateInvolvement.
    /// </summary>
    [JsonPropertyName("interstateInvolvement")]
    public Common InterstateInvolvement
    {
      get => interstateInvolvement ??= new();
      set => interstateInvolvement = value;
    }

    /// <summary>
    /// A value of OgIcInvolvement.
    /// </summary>
    [JsonPropertyName("ogIcInvolvement")]
    public Common OgIcInvolvement
    {
      get => ogIcInvolvement ??= new();
      set => ogIcInvolvement = value;
    }

    /// <summary>
    /// A value of IncIcInvolvement.
    /// </summary>
    [JsonPropertyName("incIcInvolvement")]
    public Common IncIcInvolvement
    {
      get => incIcInvolvement ??= new();
      set => incIcInvolvement = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Common interstateInvolvement;
    private Common ogIcInvolvement;
    private Common incIcInvolvement;
    private Case1 case1;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private Case1 case1;
  }
#endregion
}
