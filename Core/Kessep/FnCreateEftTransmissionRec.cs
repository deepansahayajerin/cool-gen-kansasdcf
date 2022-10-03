// Program: FN_CREATE_EFT_TRANSMISSION_REC, ID: 372673981, model: 746.
// Short name: SWE02586
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_EFT_TRANSMISSION_REC.
/// </summary>
[Serializable]
public partial class FnCreateEftTransmissionRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_EFT_TRANSMISSION_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateEftTransmissionRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateEftTransmissionRec.
  /// </summary>
  public FnCreateEftTransmissionRec(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block creates the EFT transmission record that goes with an 
    // EFT Payment Request.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	   By	IDCR #	Description
    // 04/18/99    Fangman                Initial code
    // 01/04/00    Fangman  82289    Change default of Employment Termination 
    // Indicator from 'Y' to blank.
    // 04/12/00    Fangman  93146    Added code to "automatically" cancel 
    // warrants and deny recoveries if the amount is less than one dollar.
    // ---------------------------------------------
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    export.EabReportSend.RptDetail = "";

    if (!IsEmpty(import.DesignatedPayee.Number))
    {
      local.Payee.Number = import.DesignatedPayee.Number;
    }
    else
    {
      local.Payee.Number = import.Obligee.Number;
    }

    UseSiReadCsePersonBatch1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.EabReportSend.RptDetail =
        "Error: Name not found for CSE Person Number (Payee or DP) " + local
        .Payee.Number + " Debit Disbursement ID " + NumberToString
        (import.FirstDisbForPmtReq.SystemGeneratedIdentifier, 15);

      return;
    }

    local.ForCreate.VendorNumber = local.Payee.Ssn;

    if (AsChar(import.Persistent.InterstateInd) == 'Y')
    {
      local.ForCreate.PayDate = import.FirstDisbForPmtReq.CollectionDate;

      // For the first disbursement get the Credit Disb, Collection, AP CSE 
      // Person SSN, AP CSE Person Name, FIPS CODE of Cash Receipt Source Code,
      // Standard Court Order Number.
      if (ReadDisbursementTransaction())
      {
        // We now have the credit disbursement.
      }
      else
      {
        export.EabReportSend.RptDetail =
          "Error: Credit Disbursement_Transaction not found for payee " + import
          .Obligee.Number + " Debit Disbursement ID " + NumberToString
          (import.FirstDisbForPmtReq.SystemGeneratedIdentifier, 15);

        return;
      }

      if (ReadCollectionObligationTransaction())
      {
        // We now have the collection and the debt.
      }
      else
      {
        return;
      }

      if (!ReadCashReceiptDetailCollectionType())
      {
        export.EabReportSend.RptDetail =
          "Error: Cash Receipt Detail and Collection Type not found for payee " +
          import.Obligee.Number + " Collection ID " + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);

        return;
      }

      if (!ReadCsePerson())
      {
        export.EabReportSend.RptDetail =
          "Error: Obligor not found for Payee " + import.Obligee.Number + " debt obligation_transaction " +
          NumberToString(entities.Debt.SystemGeneratedIdentifier, 15);

        return;
      }

      local.Ap.Number = entities.Obligor1.Number;
      UseSiReadCsePersonBatch2();

      // -------------------------------------------------------------------------------
      // Check for return code.
      // -------------------------------------------------------------------------------
      local.ForCreate.ApSsn = (int)StringToNumber(local.Ap.Ssn);
      local.ForCreate.MedicalSupportId = "N";

      if (ReadPersonalHealthInsurance())
      {
        local.ForCreate.MedicalSupportId = "Y";
      }

      local.ForCreate.ApName =
        Substring(local.Ap.LastName, CsePersonsWorkSet.LastName_MaxLength, 1, 7) +
        Substring
        (local.Ap.FirstName, CsePersonsWorkSet.FirstName_MaxLength, 1, 3);

      if (ReadCashReceiptSourceType2())
      {
        local.ForCreate.FipsCode =
          entities.CashReceiptSourceType.State.GetValueOrDefault() * 100000 + entities
          .CashReceiptSourceType.County.GetValueOrDefault() * 100 + entities
          .CashReceiptSourceType.Location.GetValueOrDefault();
      }
      else
      {
        export.EabReportSend.RptDetail =
          "Error: Cash Receipt Source Type not found for payee " + import
          .Obligee.Number + " Collection ID " + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);

        return;
      }

      local.ForCreate.EmploymentTerminationId = "";

      if (ReadIncomeSource())
      {
        if (Lt(entities.IncomeSource.EndDt, local.Maximum.Date))
        {
          local.ForCreate.EmploymentTerminationId = "Y";
        }
      }

      local.ForCreate.CaseId = import.InterstateRequest.OtherStateCaseId ?? Spaces
        (20);
      local.ForCreate.CollectionAmount = 0;

      if (AsChar(import.BackoffInd.Flag) == 'Y')
      {
        local.ForCreate.CollectionAmount = import.Persistent.Amount + import
          .TotalCrFeesForPayee.TotalCurrency * ((import.Persistent.Amount + import
          .TotalCrFeesForPayee.TotalCurrency) / import
          .TotalDisbForPayee.TotalCurrency);
      }
      else
      {
        local.ForCreate.CollectionAmount = import.Persistent.Amount + import
          .TotalCrFeesForPayee.TotalCurrency * (import.Persistent.Amount / (
            import.Persistent.Amount + import.Recapture.Amount));
      }

      if (local.ForCreate.CollectionAmount.GetValueOrDefault() == import
        .Persistent.Amount)
      {
        // No cost recovery fee taken.
        switch(TrimEnd(entities.CollectionType.Code))
        {
          case "S":
            // SDSO
            local.ForCreate.ApplicationIdentifier = "IT";

            break;
          case "I":
            // Unemployment
            local.ForCreate.ApplicationIdentifier = "II";

            break;
          default:
            local.ForCreate.ApplicationIdentifier = "IO";

            break;
        }
      }
      else
      {
        // Cost recovery fee taken.
        switch(TrimEnd(entities.CollectionType.Code))
        {
          case "S":
            // SDSO
            local.ForCreate.ApplicationIdentifier = "RT";

            break;
          case "I":
            // Unemployment
            local.ForCreate.ApplicationIdentifier = "RI";

            break;
          default:
            local.ForCreate.ApplicationIdentifier = "RO";

            break;
        }
      }

      // Get the EFT information and the Company Name (from the state FIPS code
      // ).
      if (ReadInterstateRequest())
      {
        if (ReadInterstatePaymentAddress())
        {
          local.ForCreate.ReceivingDfiAccountNumber =
            entities.InterstatePaymentAddress.AccountNumberDfi;
          local.WorkArea.Text15 =
            NumberToString(entities.InterstatePaymentAddress.RoutingNumberAba.
              GetValueOrDefault(), 15);
          local.ForCreate.ReceivingDfiIdentification =
            (int?)StringToNumber(Substring(
              local.WorkArea.Text15, WorkArea.Text15_MaxLength, 7, 8));
          local.ForCreate.CheckDigit =
            (int?)StringToNumber(Substring(
              local.WorkArea.Text15, WorkArea.Text15_MaxLength, 15, 1));

          if (AsChar(entities.InterstatePaymentAddress.AccountType) == 'S')
          {
            local.ForCreate.TransactionCode = "32";
          }
          else
          {
            local.ForCreate.TransactionCode = "22";
          }
        }
        else
        {
          export.EabReportSend.RptDetail =
            "Error: Interstate Payment Address not found for payee " + import
            .Obligee.Number + " Disb ID " + NumberToString
            (entities.FirstDisbForPmtReq.SystemGeneratedIdentifier, 15);

          return;
        }
      }
      else
      {
        export.EabReportSend.RptDetail =
          "Error: Interstate Request not found for payee " + import
          .Obligee.Number + " Disb ID " + NumberToString
          (entities.FirstDisbForPmtReq.SystemGeneratedIdentifier, 15);

        return;
      }

      // Set this to the name of the State.
      if (!IsEmpty(entities.InterstatePaymentAddress.FipsState))
      {
        local.CashReceiptSourceType.State =
          (int?)StringToNumber(entities.InterstatePaymentAddress.FipsState);

        if (ReadCashReceiptSourceType1())
        {
          local.ForCreate.ReceivingCompanyName =
            entities.CashReceiptSourceType.Name;
        }
        else
        {
          export.EabReportSend.RptDetail =
            "Error: Cash Receipt Source Type not found for Interstate state fips code of " +
            entities.InterstatePaymentAddress.FipsState + " for payee " + import
            .Obligee.Number + " Disb ID " + NumberToString
            (entities.FirstDisbForPmtReq.SystemGeneratedIdentifier, 15);

          return;
        }
      }
      else
      {
        local.ForCreate.ReceivingCompanyName = "State Unknown -No FIPS";
      }
    }
    else
    {
      // This is not an interstate EFT so the fields in the addendum record do 
      // not need to be populated.
      MoveElectronicFundTransmission(local.
        InitializedElectronicFundTransmission, local.ForCreate);
      local.ForCreate.ReceivingDfiAccountNumber =
        import.PersonPreferredPaymentMethod.DfiAccountNumber ?? "";
      local.WorkArea.Text15 =
        NumberToString(import.PersonPreferredPaymentMethod.AbaRoutingNumber.
          GetValueOrDefault(), 15);
      local.ForCreate.ReceivingDfiIdentification =
        (int?)StringToNumber(Substring(
          local.WorkArea.Text15, WorkArea.Text15_MaxLength, 7, 8));
      local.ForCreate.CheckDigit =
        (int?)StringToNumber(Substring(
          local.WorkArea.Text15, WorkArea.Text15_MaxLength, 15, 1));

      if (AsChar(import.PersonPreferredPaymentMethod.AccountType) == 'S')
      {
        local.ForCreate.TransactionCode = "32";
      }
      else
      {
        local.ForCreate.TransactionCode = "22";
      }

      local.ForCreate.ReceivingCompanyName = TrimEnd(local.Payee.LastName) + ", " +
        TrimEnd(local.Payee.FirstName) + " " + local.Payee.MiddleInitial;
    }

    if (!Equal(global.UserId, "SWEFB653") && import.Persistent.Amount < 1M)
    {
      local.ForCreate.TransmissionStatusCode = "PROCESSED";
      local.ForCreate.TransmissionProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.ForCreate.TransmissionStatusCode = "REQUESTED";
      local.ForCreate.TransmissionProcessDate =
        local.InitializedDateWorkArea.Date;
    }

    try
    {
      CreateElectronicFundTransmission();
      ++import.ExportNext.TransmissionIdentifier;
      import.ExportNext.TraceNumber =
        import.ExportNext.TraceNumber.GetValueOrDefault() + 1;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.EabReportSend.RptDetail =
            "Error: Elec Fund Tran already exists for payee " + entities
            .ElectronicFundTransmission.ApName + " Payment Request ID " + NumberToString
            (import.Persistent.SystemGeneratedIdentifier, 15);

          break;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error: Elec Fund Tran permitted value error for payee " + entities
            .ElectronicFundTransmission.ApName + " Payment Request ID " + NumberToString
            (import.Persistent.SystemGeneratedIdentifier, 15);

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.PayDate = source.PayDate;
    target.ApSsn = source.ApSsn;
    target.MedicalSupportId = source.MedicalSupportId;
    target.ApName = source.ApName;
    target.FipsCode = source.FipsCode;
    target.EmploymentTerminationId = source.EmploymentTerminationId;
    target.CaseId = source.CaseId;
    target.ApplicationIdentifier = source.ApplicationIdentifier;
    target.CollectionAmount = source.CollectionAmount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Payee.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Payee.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Ap.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateElectronicFundTransmission()
  {
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var payDate = local.ForCreate.PayDate;
    var transmittalAmount = import.Persistent.Amount;
    var apSsn = local.ForCreate.ApSsn;
    var medicalSupportId = local.ForCreate.MedicalSupportId;
    var apName = local.ForCreate.ApName;
    var fipsCode = local.ForCreate.FipsCode.GetValueOrDefault();
    var employmentTerminationId = local.ForCreate.EmploymentTerminationId ?? "";
    var receivingDfiIdentification =
      local.ForCreate.ReceivingDfiIdentification.GetValueOrDefault();
    var transactionCode = local.ForCreate.TransactionCode;
    var caseId = local.ForCreate.CaseId;
    var transmissionStatusCode = local.ForCreate.TransmissionStatusCode;
    var transmissionType = "O";
    var transmissionIdentifier = import.ExportNext.TransmissionIdentifier;
    var transmissionProcessDate = local.ForCreate.TransmissionProcessDate;
    var prqGeneratedId = import.Persistent.SystemGeneratedIdentifier;
    var receivingCompanyName = local.ForCreate.ReceivingCompanyName ?? "";
    var traceNumber = import.ExportNext.TraceNumber.GetValueOrDefault();
    var applicationIdentifier = local.ForCreate.ApplicationIdentifier ?? "";
    var collectionAmount = local.ForCreate.CollectionAmount.GetValueOrDefault();
    var vendorNumber = local.ForCreate.VendorNumber ?? "";
    var checkDigit = local.ForCreate.CheckDigit.GetValueOrDefault();
    var receivingDfiAccountNumber =
      local.ForCreate.ReceivingDfiAccountNumber ?? "";
    var companyEntryDescription = "KS2017702";

    entities.ElectronicFundTransmission.Populated = false;
    Update("CreateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDate(command, "payDate", payDate);
        db.SetDecimal(command, "transmittalAmount", transmittalAmount);
        db.SetInt32(command, "apSsn", apSsn);
        db.SetString(command, "medicalSupportId", medicalSupportId);
        db.SetString(command, "apName", apName);
        db.SetNullableInt32(command, "fipsCode", fipsCode);
        db.SetNullableString(
          command, "employmentTermId", employmentTerminationId);
        db.SetNullableInt32(command, "zdelAdendaSqNum", 0);
        db.SetNullableInt32(command, "sequenceNumber", 0);
        db.SetNullableInt32(
          command, "receivingDfiIden", receivingDfiIdentification);
        db.SetNullableString(command, "dfiAcctNumber", "");
        db.SetString(command, "transactionCode", transactionCode);
        db.SetNullableDate(command, "settlementDate", default(DateTime));
        db.SetString(command, "caseId", caseId);
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.SetNullableString(command, "companyName", "");
        db.SetNullableInt32(command, "origDfiIdent", 0);
        db.SetNullableString(command, "recvEntityName", "");
        db.SetString(command, "transmissionType", transmissionType);
        db.SetInt32(command, "transmissionId", transmissionIdentifier);
        db.
          SetNullableDate(command, "transProcessDate", transmissionProcessDate);
          
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetNullableTimeSpan(command, "fileCreationTime", TimeSpan.Zero);
        db.SetNullableString(command, "companyIdentIcd", "");
        db.SetNullableString(command, "companyIdentNum", "");
        db.SetNullableDate(command, "companyDescDate", null);
        db.SetNullableString(command, "recvCompanyName", receivingCompanyName);
        db.SetNullableInt64(command, "traceNumber", traceNumber);
        db.
          SetNullableString(command, "applicationIdent", applicationIdentifier);
          
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "vendorNumber", vendorNumber);
        db.SetNullableInt32(command, "checkDigit", checkDigit);
        db.SetNullableString(
          command, "recvDfiAcctNum", receivingDfiAccountNumber);
        db.SetNullableString(
          command, "companyEntryDesc", companyEntryDescription);
      });

    entities.ElectronicFundTransmission.CreatedBy = createdBy;
    entities.ElectronicFundTransmission.CreatedTimestamp = createdTimestamp;
    entities.ElectronicFundTransmission.LastUpdatedBy = createdBy;
    entities.ElectronicFundTransmission.LastUpdatedTimestamp = createdTimestamp;
    entities.ElectronicFundTransmission.PayDate = payDate;
    entities.ElectronicFundTransmission.TransmittalAmount = transmittalAmount;
    entities.ElectronicFundTransmission.ApSsn = apSsn;
    entities.ElectronicFundTransmission.MedicalSupportId = medicalSupportId;
    entities.ElectronicFundTransmission.ApName = apName;
    entities.ElectronicFundTransmission.FipsCode = fipsCode;
    entities.ElectronicFundTransmission.EmploymentTerminationId =
      employmentTerminationId;
    entities.ElectronicFundTransmission.ZdelAddendaSequenceNumber = 0;
    entities.ElectronicFundTransmission.ReceivingDfiIdentification =
      receivingDfiIdentification;
    entities.ElectronicFundTransmission.TransactionCode = transactionCode;
    entities.ElectronicFundTransmission.CaseId = caseId;
    entities.ElectronicFundTransmission.TransmissionStatusCode =
      transmissionStatusCode;
    entities.ElectronicFundTransmission.TransmissionType = transmissionType;
    entities.ElectronicFundTransmission.TransmissionIdentifier =
      transmissionIdentifier;
    entities.ElectronicFundTransmission.TransmissionProcessDate =
      transmissionProcessDate;
    entities.ElectronicFundTransmission.PrqGeneratedId = prqGeneratedId;
    entities.ElectronicFundTransmission.CompanyDescriptiveDate = null;
    entities.ElectronicFundTransmission.ReceivingCompanyName =
      receivingCompanyName;
    entities.ElectronicFundTransmission.TraceNumber = traceNumber;
    entities.ElectronicFundTransmission.ApplicationIdentifier =
      applicationIdentifier;
    entities.ElectronicFundTransmission.CollectionAmount = collectionAmount;
    entities.ElectronicFundTransmission.VendorNumber = vendorNumber;
    entities.ElectronicFundTransmission.CheckDigit = checkDigit;
    entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
      receivingDfiAccountNumber;
    entities.ElectronicFundTransmission.CompanyEntryDescription =
      companyEntryDescription;
    entities.ElectronicFundTransmission.Populated = true;
  }

  private bool ReadCashReceiptDetailCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 4);
        entities.CollectionType.Code = db.GetString(reader, 5);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCollectionObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.Debt.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionObligationTransaction",
      (db, command) =>
      {
        db.
          SetInt32(command, "collId", entities.Credit.ColId.GetValueOrDefault());
          
        db.
          SetInt32(command, "otyId", entities.Credit.OtyId.GetValueOrDefault());
          
        db.
          SetInt32(command, "obgId", entities.Credit.ObgId.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.Credit.CspNumberDisb ?? "");
        db.SetString(command, "cpaType", entities.Credit.CpaTypeDisb ?? "");
        db.
          SetInt32(command, "otrId", entities.Credit.OtrId.GetValueOrDefault());
          
        db.SetString(command, "otrType", entities.Credit.OtrTypeDisb ?? "");
        db.SetInt32(
          command, "crtType", entities.Credit.CrtId.GetValueOrDefault());
        db.
          SetInt32(command, "cstId", entities.Credit.CstId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crvId", entities.Credit.CrvId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Debt.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Debt.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Debt.Type1 = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Debt.OtyType = db.GetInt32(reader, 10);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 11);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 12);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 13);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 14);
        entities.Debt.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Obligor1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          import.FirstDisbForPmtReq.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 4);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 5);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 6);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 7);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 8);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 9);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 10);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 11);
        entities.Credit.CstId = db.GetNullableInt32(reader, 12);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 13);
        entities.Credit.ColId = db.GetNullableInt32(reader, 14);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.Obligor1.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 3);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 4);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 5);
        entities.InterstatePaymentAddress.AccountType =
          db.GetNullableString(reader, 6);
        entities.InterstatePaymentAddress.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          import.FirstDisbForPmtReq.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.Populated = true;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public PaymentRequest Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
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
    /// A value of FirstDisbForPmtReq.
    /// </summary>
    [JsonPropertyName("firstDisbForPmtReq")]
    public DisbursementTransaction FirstDisbForPmtReq
    {
      get => firstDisbForPmtReq ??= new();
      set => firstDisbForPmtReq = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of ExportNext.
    /// </summary>
    [JsonPropertyName("exportNext")]
    public ElectronicFundTransmission ExportNext
    {
      get => exportNext ??= new();
      set => exportNext = value;
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
    /// A value of BackoffInd.
    /// </summary>
    [JsonPropertyName("backoffInd")]
    public Common BackoffInd
    {
      get => backoffInd ??= new();
      set => backoffInd = value;
    }

    /// <summary>
    /// A value of TotalDisbForPayee.
    /// </summary>
    [JsonPropertyName("totalDisbForPayee")]
    public Common TotalDisbForPayee
    {
      get => totalDisbForPayee ??= new();
      set => totalDisbForPayee = value;
    }

    /// <summary>
    /// A value of TotalCrFeesForPayee.
    /// </summary>
    [JsonPropertyName("totalCrFeesForPayee")]
    public Common TotalCrFeesForPayee
    {
      get => totalCrFeesForPayee ??= new();
      set => totalCrFeesForPayee = value;
    }

    /// <summary>
    /// A value of Recapture.
    /// </summary>
    [JsonPropertyName("recapture")]
    public PaymentRequest Recapture
    {
      get => recapture ??= new();
      set => recapture = value;
    }

    private CsePerson obligee;
    private PaymentRequest persistent;
    private CsePerson designatedPayee;
    private InterstateRequest interstateRequest;
    private DisbursementTransaction firstDisbForPmtReq;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private ElectronicFundTransmission exportNext;
    private ProgramProcessingInfo programProcessingInfo;
    private Common backoffInd;
    private Common totalDisbForPayee;
    private Common totalCrFeesForPayee;
    private PaymentRequest recapture;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public ElectronicFundTransmission ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of InterstateInd.
    /// </summary>
    [JsonPropertyName("interstateInd")]
    public Common InterstateInd
    {
      get => interstateInd ??= new();
      set => interstateInd = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of InitializedElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("initializedElectronicFundTransmission")]
    public ElectronicFundTransmission InitializedElectronicFundTransmission
    {
      get => initializedElectronicFundTransmission ??= new();
      set => initializedElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of NextEftId.
    /// </summary>
    [JsonPropertyName("nextEftId")]
    public ElectronicFundTransmission NextEftId
    {
      get => nextEftId ??= new();
      set => nextEftId = value;
    }

    /// <summary>
    /// A value of Localzzzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("localzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public EabFileHandling Localzzzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => localzzzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => localzzzzzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    private DateWorkArea maximum;
    private CashReceiptSourceType cashReceiptSourceType;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet ap;
    private ElectronicFundTransmission forCreate;
    private Common interstateInd;
    private DateWorkArea initializedDateWorkArea;
    private WorkArea workArea;
    private ElectronicFundTransmission initializedElectronicFundTransmission;
    private ElectronicFundTransmission nextEftId;
    private EabFileHandling localzzzzzzzzzzzzzzzzzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FirstDisbForPmtReq.
    /// </summary>
    [JsonPropertyName("firstDisbForPmtReq")]
    public DisbursementTransaction FirstDisbForPmtReq
    {
      get => firstDisbForPmtReq ??= new();
      set => firstDisbForPmtReq = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    private DisbursementTransaction firstDisbForPmtReq;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateRequest interstateRequest;
    private IncomeSource incomeSource;
    private PersonalHealthInsurance personalHealthInsurance;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private Collection collection;
    private DisbursementTransaction credit;
    private ElectronicFundTransmission electronicFundTransmission;
    private DisbursementTransactionRln disbursementTransactionRln;
  }
#endregion
}
