// Program: FN_CREATE_WAR_OR_EFT_PMT_REQ, ID: 372673988, model: 746.
// Short name: SWE02588
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_WAR_OR_EFT_PMT_REQ.
/// </summary>
[Serializable]
public partial class FnCreateWarOrEftPmtReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_WAR_OR_EFT_PMT_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateWarOrEftPmtReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateWarOrEftPmtReq.
  /// </summary>
  public FnCreateWarOrEftPmtReq(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	DEscription
    // 7/10/99    Fangman         Changed logic to use the new 
    // interstate_payment_address account_number_dfi to determine if the payment
    // should be an EFT or a Warrant.
    // 7/13/99   Fangman           Changed logic to use all three new 
    // interstate_payment_address EFT attributes to determine if the payment
    // should be an EFT or a Warrant.
    // 8/11/99   Fangman           Changed logic to read a interstate payment 
    // address with a type of 'PY'.
    // 02/12/01 Fangman      WR 284  Don't issue Interstate EFTs.
    // ---------------------------------------------------------------------------------------
    if (AsChar(import.PaymentRequest.InterstateInd) == 'Y')
    {
      local.PaymentRequest.Type1 = "WAR";
    }
    else
    {
      if (ReadCsePersonDesigPayeeCsePerson())
      {
        export.DesignatedPayee.Number = entities.DesignatedPayee.Number;
        local.DesignatedPayee.Number = entities.DesignatedPayee.Number;
        local.ForPpm.Number = entities.DesignatedPayee.Number;
      }
      else
      {
        export.DesignatedPayee.Number = local.DesignatedPayee.Number;
        local.ForPpm.Number = import.Obligee.Number;
      }

      local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

      if (ReadPersonPreferredPaymentMethod())
      {
        export.PersonPreferredPaymentMethod.Assign(
          entities.PersonPreferredPaymentMethod);
        local.PaymentRequest.Type1 = "EFT";
      }
      else
      {
        local.PaymentRequest.Type1 = "WAR";

        // *****  No person_preferred_payment_method was found & the default is 
        // warrant.
      }
    }

    for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 20; ++
      local.CreateAttempts.Count)
    {
      try
      {
        CreatePaymentRequest();
        export.PaymentRequest.SystemGeneratedIdentifier =
          entities.PaymentRequest.SystemGeneratedIdentifier;
        ExitState = "ACO_NN0000_ALL_OK";

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PAYMENT_REQUEST_AE";

            continue;
          case ErrorCode.PermittedValueViolation:
            export.EabReportSend.RptDetail =
              "Permitted Value error creating the payment request for Obligee " +
              import.Obligee.Number + " with a type of " + local
              .PaymentRequest.Type1 + " Disb ID " + NumberToString
              (import.Debit.SystemGeneratedIdentifier, 15);

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (IsExitState("FN0000_PAYMENT_REQUEST_AE"))
    {
      export.EabReportSend.RptDetail =
        "Already Exists error creating the payment request for Obligee " + import
        .Obligee.Number + " with a type of " + local.PaymentRequest.Type1 + " Disb ID " +
        NumberToString(import.Debit.SystemGeneratedIdentifier, 15);
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var processDate = local.Initialized.Date;
    var amount = 0M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo = local.DesignatedPayee.Number;
    var csePersonNumber = import.Obligee.Number;
    var classification = "SUP";
    var type1 = local.PaymentRequest.Type1;
    var interstateInd = import.PaymentRequest.InterstateInd ?? "";

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
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", "");
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "interstateInd", interstateInd);
      });

    entities.PaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.CreatedBy = createdBy;
    entities.PaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.PaymentRequest.DesignatedPayeeCsePersonNo =
      designatedPayeeCsePersonNo;
    entities.PaymentRequest.CsePersonNumber = csePersonNumber;
    entities.PaymentRequest.ImprestFundCode = "";
    entities.PaymentRequest.Classification = classification;
    entities.PaymentRequest.Type1 = type1;
    entities.PaymentRequest.PrqRGeneratedId = null;
    entities.PaymentRequest.InterstateInd = interstateInd;
    entities.PaymentRequest.Populated = true;
  }

  private bool ReadCsePersonDesigPayeeCsePerson()
  {
    entities.CsePersonDesigPayee.Populated = false;
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePersonDesigPayeeCsePerson",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", import.Obligee.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 4);
        entities.DesignatedPayee.Number = db.GetString(reader, 4);
        entities.CsePersonDesigPayee.Populated = true;
        entities.DesignatedPayee.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethod()
  {
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetString(command, "cspPNumber", local.ForPpm.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PersonPreferredPaymentMethod.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PersonPreferredPaymentMethod.AbaRoutingNumber =
          db.GetNullableInt64(reader, 2);
        entities.PersonPreferredPaymentMethod.DfiAccountNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPreferredPaymentMethod.EffectiveDate =
          db.GetDate(reader, 4);
        entities.PersonPreferredPaymentMethod.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.PersonPreferredPaymentMethod.CspPNumber =
          db.GetString(reader, 6);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 7);
        entities.PersonPreferredPaymentMethod.Populated = true;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    private CsePerson obligee;
    private PaymentRequest paymentRequest;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementTransaction debit;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private CsePerson designatedPayee;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentRequest paymentRequest;
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
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of ForPpm.
    /// </summary>
    [JsonPropertyName("forPpm")]
    public CsePerson ForPpm
    {
      get => forPpm ??= new();
      set => forPpm = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    private DateWorkArea maximum;
    private CsePerson designatedPayee;
    private CsePerson forPpm;
    private PaymentRequest paymentRequest;
    private DateWorkArea initialized;
    private Common createAttempts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    private PaymentRequest paymentRequest;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePerson designatedPayee;
    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private DisbursementTransaction disbursementTransaction;
    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
  }
#endregion
}
