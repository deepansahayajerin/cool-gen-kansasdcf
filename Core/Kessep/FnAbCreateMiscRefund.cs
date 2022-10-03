// Program: FN_AB_CREATE_MISC_REFUND, ID: 372312320, model: 746.
// Short name: SWE00241
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_CREATE_MISC_REFUND.
/// </summary>
[Serializable]
public partial class FnAbCreateMiscRefund: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_CREATE_MISC_REFUND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbCreateMiscRefund(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbCreateMiscRefund.
  /// </summary>
  public FnAbCreateMiscRefund(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 01/29/95  Holly Kennedy-MTW			Source
    // ------------------------------------------------------------------------
    // 	
    MoveReceiptRefund(import.ReceiptRefund, export.ReceiptRefund);

    // *****
    // Add Receipt Refund
    // *****
    try
    {
      CreateReceiptRefund();

      // *****
      // Add and associate Address
      // *****
      try
      {
        CreateCashReceiptDetailAddress();

        // *****
        // Continue processing
        // *****
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_ROLLBACK_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_ROLLBACK_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // *** Add logic to move cse person to the local payment request person 
      // number if person.  If source code, need to determine the organization
      // cse person number to set the local payment request person number.  I am
      // moving the source type code values to a local and then using them in
      // the person read to prevent an unnecessary join.  This number will be
      // used when creating the payment request.  Sunya Sharp 3/17/1999 ***
      // *****
      // Associate CSE Person if entered
      // *****
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        if (ReadCsePerson1())
        {
          AssociateCsePerson();
          local.PaymentRequest.CsePersonNumber = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "CSE_PERSON_NF_RB";

          return;
        }
      }

      // *****
      // Associate Source if entered
      // *****
      if (!IsEmpty(import.CashReceiptSourceType.Code))
      {
        if (ReadCashReceiptSourceType())
        {
          local.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);

          if (ReadCsePerson2())
          {
            local.PaymentRequest.CsePersonNumber = entities.SourceType.Number;
          }
          else
          {
            UseEabRollbackCics();
            ExitState = "FN0000_NO_ADD_REFUND_NO_ORG_NUMB";
            export.ReceiptRefund.RequestDate = null;

            return;
          }

          AssociateCashReceiptSourceType();
        }
        else
        {
          ExitState = "FN0000_ROLLBACK_RB";

          return;
        }
      }

      // *****
      // Add the associated Payment Request and associate
      // *****
      try
      {
        CreatePaymentRequest();

        // *****
        // Add Payment Status
        // *****
        if (!ReadPaymentStatus())
        {
          ExitState = "FN0000_PYMNT_STAT_NF_RB";

          return;
        }

        local.Attempts.Count = 0;

        while(local.Attempts.Count < 10)
        {
          try
          {
            CreatePaymentStatusHistory();

            break;
          }
          catch(Exception e2)
          {
            switch(GetErrorCode(e2))
            {
              case ErrorCode.AlreadyExists:
                ++local.Attempts.Count;
                ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      export.ReceiptRefund.Assign(entities.ReceiptRefund);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ROLLBACK_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_ROLLBACK_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveReceiptRefund(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void AssociateCashReceiptSourceType()
  {
    var cstAIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;

    entities.ReceiptRefund.Populated = false;
    Update("AssociateCashReceiptSourceType",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cstAIdentifier", cstAIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.CstAIdentifier = cstAIdentifier;
    entities.ReceiptRefund.Populated = true;
  }

  private void AssociateCsePerson()
  {
    var cspNumber = entities.CsePerson.Number;

    entities.ReceiptRefund.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.CspNumber = cspNumber;
    entities.ReceiptRefund.Populated = true;
  }

  private void CreateCashReceiptDetailAddress()
  {
    var systemGeneratedIdentifier = Now();
    var street1 = import.CashReceiptDetailAddress.Street1;
    var street2 = import.CashReceiptDetailAddress.Street2 ?? "";
    var city = import.CashReceiptDetailAddress.City;
    var state = import.CashReceiptDetailAddress.State;
    var zipCode5 = import.CashReceiptDetailAddress.ZipCode5;
    var zipCode4 = import.CashReceiptDetailAddress.ZipCode4 ?? "";
    var zipCode3 = import.CashReceiptDetailAddress.ZipCode3 ?? "";

    entities.ReceiptRefund.Populated = false;
    entities.CashReceiptDetailAddress.Populated = false;
    Update("CreateCashReceiptDetailAddress#1",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
      });

    Update("CreateCashReceiptDetailAddress#3",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "cdaIdentifier", systemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.ReceiptRefund.CdaIdentifier = systemGeneratedIdentifier;
    entities.ReceiptRefund.Populated = true;
    entities.CashReceiptDetailAddress.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var processDate = new DateTime(1, 1, 1);
    var amount = import.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var csePersonNumber = local.PaymentRequest.CsePersonNumber ?? "";
    var classification = import.PaymentRequest.Classification;
    var type1 = import.PaymentRequest.Type1;
    var rctRTstamp = entities.ReceiptRefund.CreatedTimestamp;

    CheckValid<PaymentRequest>("Type1", type1);
    entities.PaymentRequest.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "dpCsePerNum", "");
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", "");
        db.SetString(command, "classification", classification);
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
        db.SetNullableDateTime(command, "rctRTstamp", rctRTstamp);
      });

    entities.PaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.CreatedBy = createdBy;
    entities.PaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.PaymentRequest.DesignatedPayeeCsePersonNo = "";
    entities.PaymentRequest.CsePersonNumber = csePersonNumber;
    entities.PaymentRequest.Classification = classification;
    entities.PaymentRequest.Number = "";
    entities.PaymentRequest.Type1 = type1;
    entities.PaymentRequest.RctRTstamp = rctRTstamp;
    entities.PaymentRequest.PrqRGeneratedId = null;
    entities.PaymentRequest.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var effectiveDate = Now().Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = "MISCELLANEOUS REFUND";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreateReceiptRefund()
  {
    var createdTimestamp = Now();
    var reasonCode = import.ReceiptRefund.ReasonCode;
    var taxid = import.ReceiptRefund.Taxid ?? "";
    var payeeName = import.ReceiptRefund.PayeeName ?? "";
    var amount = import.ReceiptRefund.Amount;
    var requestDate = import.ReceiptRefund.RequestDate;
    var createdBy = global.UserId;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";

    entities.ReceiptRefund.Populated = false;
    Update("CreateReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "taxid", taxid);
        db.SetNullableString(command, "payeeName", payeeName);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableInt32(command, "offsetTaxYear", 0);
        db.SetDate(command, "requestDate", requestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "offsetClosed", "");
        db.SetNullableDate(command, "dateTransmitted", default(DateTime));
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.ReceiptRefund.CreatedTimestamp = createdTimestamp;
    entities.ReceiptRefund.ReasonCode = reasonCode;
    entities.ReceiptRefund.Taxid = taxid;
    entities.ReceiptRefund.PayeeName = payeeName;
    entities.ReceiptRefund.Amount = amount;
    entities.ReceiptRefund.RequestDate = requestDate;
    entities.ReceiptRefund.CreatedBy = createdBy;
    entities.ReceiptRefund.CspNumber = null;
    entities.ReceiptRefund.CstAIdentifier = null;
    entities.ReceiptRefund.CdaIdentifier = null;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.Populated = true;
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 2);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.SourceType.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          local.CashReceiptSourceType.County.GetValueOrDefault());
        db.SetInt32(
          command, "location",
          local.CashReceiptSourceType.Location.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.SourceType.Number = db.GetString(reader, 0);
        entities.SourceType.Type1 = db.GetString(reader, 1);
        entities.SourceType.Populated = true;
        CheckValid<CsePerson>("Type1", entities.SourceType.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Populated = true;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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

    private PaymentRequest paymentRequest;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Set.
    /// </summary>
    [JsonPropertyName("set")]
    public CashReceipt Set
    {
      get => set ??= new();
      set => set = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    private PaymentRequest paymentRequest;
    private CashReceipt set;
    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public PaymentRequest Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CashReceiptType Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of Attempts.
    /// </summary>
    [JsonPropertyName("attempts")]
    public Common Attempts
    {
      get => attempts ??= new();
      set => attempts = value;
    }

    /// <summary>
    /// A value of Recorded.
    /// </summary>
    [JsonPropertyName("recorded")]
    public CashReceiptStatus Recorded
    {
      get => recorded ??= new();
      set => recorded = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private PaymentRequest next;
    private CashReceiptType check;
    private Common attempts;
    private CashReceiptStatus recorded;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SourceType.
    /// </summary>
    [JsonPropertyName("sourceType")]
    public CsePerson SourceType
    {
      get => sourceType ??= new();
      set => sourceType = value;
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
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentRequest Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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

    private CsePerson sourceType;
    private Fips fips;
    private PaymentRequest warrant;
    private CashReceiptType cashReceiptType;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private CsePerson csePerson;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptSourceType cashReceiptSourceType;
  }
#endregion
}
