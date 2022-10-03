// Program: FN_CREATE_PAYMENT_REQUEST, ID: 372676267, model: 746.
// Short name: SWE02113
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_PAYMENT_REQUEST.
/// </summary>
[Serializable]
public partial class FnCreatePaymentRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_PAYMENT_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreatePaymentRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreatePaymentRequest.
  /// </summary>
  public FnCreatePaymentRequest(IContext context, Import import, Export export):
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
    // Date	By	IDCR#	Description
    // ---------------------------------------------
    // ??????	??????		Initial code
    // 121097	govind		Made Disb Tran as optional. Payment Request can exist 
    // without a Disb Tran associated with it.
    // 122397	govind		Fixed to set the desig payee from Disb Tran and use the 
    // new version of 'get designated payee'
    // 042599  Fangman       Removed logic to create Elec_fund_tran record.
    // 06/29/99  Fangman     Changed code to only load Designated Payee field on
    // Payment Request when a Designated Payee exists.
    // ---------------------------------------------
    // ***** Check for Designated Payee.  If there is a Designated Payee then we
    // need to disburse using the preferred method of that Designated Payee.
    local.Imported.Type1 = import.PaymentRequest.Type1;
    local.HardcodeProcessed.SystemGeneratedIdentifier = 2;
    local.HardcodeSupport.Classification = "SUP";

    if (ReadCsePerson())
    {
      if (import.DisbursementTransaction.SystemGeneratedIdentifier != 0)
      {
        if (!ReadDisbursementTransaction())
        {
          ExitState = "FN0000_DISB_TRANSACTION_NF";
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Get the designated payee and preferred payment method for Warrants and 
    // EFTs
    // ---------------------------------------------
    if (Equal(import.PaymentRequest.Type1, "WAR") || Equal
      (import.PaymentRequest.Type1, "EFT"))
    {
      if (ReadCsePersonDesigPayeeCsePerson())
      {
        local.DesignatedPayee.Number = entities.Designated.Number;
        local.ForPreferredPaymentMeth.Number = entities.Designated.Number;

        // -----------------------------------------
        // Why is the export CSE Person number being set to the Designated Payee
        // Number & how is it used in the calling program????
        export.CsePerson.Number = local.ForPreferredPaymentMeth.Number;
      }
      else
      {
        local.ForPreferredPaymentMeth.Number = import.Obligee.Number;
      }

      // --------------------------------------
      // N.Engoor  -  02/10/99
      // The READ has been changed to retrieve the currently effective record 
      // for either the obligee or the designated payee.
      // --------------------------------------
      if (ReadPersonPreferredPaymentMethodPaymentMethodType())
      {
        MovePersonPreferredPaymentMethod(entities.PersonPreferredPaymentMethod,
          export.PersonPreferredPaymentMethod);
        local.Imported.Type1 = "EFT";
      }
      else
      {
        // *****  No person_preferred_payment_method was found & the default is 
        // warrant so we can go ahead with the creation of the warrant
        // payment_request.
      }
    }

    for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 10; ++
      local.CreateAttempts.Count)
    {
      try
      {
        CreatePaymentRequest();

        if (import.DisbursementTransaction.SystemGeneratedIdentifier != 0)
        {
          AssociateDisbursementTransaction();
        }

        MovePaymentRequest(entities.PaymentRequest, export.PaymentRequest);
        ExitState = "ACO_NN0000_ALL_OK";

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

            continue;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PYMNT_REQ_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // --------------------------
      // N.Engoor - 03/17/99
      // Depending on whether a potential recovery is to be created or a WAR/EFT
      // /RCP the payment status is set.
      // If the Payment request is RCV (Potential recovery), the status is set 
      // to RCVPOT else it is set to REQ (Requested).
      // ---------------------------
      if (Equal(entities.PaymentRequest.Type1, "RCV"))
      {
        local.HardcodeRequestedOrRcvpot.SystemGeneratedIdentifier = 27;
      }
      else
      {
        local.HardcodeRequestedOrRcvpot.SystemGeneratedIdentifier = 1;
      }

      UseFnCreatePaymentStatusHist();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (import.DisbursementTransaction.SystemGeneratedIdentifier != 0 && !
          Equal(entities.PaymentRequest.Type1, "RCP"))
        {
          UseFnSetDisbTranAsProcessed();
        }
      }
    }
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MovePersonPreferredPaymentMethod(
    PersonPreferredPaymentMethod source, PersonPreferredPaymentMethod target)
  {
    target.AbaRoutingNumber = source.AbaRoutingNumber;
    target.DfiAccountNumber = source.DfiAccountNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.AccountType = source.AccountType;
  }

  private void UseFnCreatePaymentStatusHist()
  {
    var useImport = new FnCreatePaymentStatusHist.Import();
    var useExport = new FnCreatePaymentStatusHist.Export();

    useImport.PaymentRequest.SystemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Requested.SystemGeneratedIdentifier =
      local.HardcodeRequestedOrRcvpot.SystemGeneratedIdentifier;

    Call(FnCreatePaymentStatusHist.Execute, useImport, useExport);
  }

  private void UseFnSetDisbTranAsProcessed()
  {
    var useImport = new FnSetDisbTranAsProcessed.Import();
    var useExport = new FnSetDisbTranAsProcessed.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Processed.SystemGeneratedIdentifier =
      local.HardcodeProcessed.SystemGeneratedIdentifier;
    useImport.DisbursementTransaction.SystemGeneratedIdentifier =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    useImport.Obligee.Number = import.Obligee.Number;

    Call(FnSetDisbTranAsProcessed.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;

    entities.DisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.PrqGeneratedId = prqGeneratedId;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var processDate = local.Initiailized.ProcessDate;
    var amount = import.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo = local.DesignatedPayee.Number;
    var csePersonNumber = entities.Obligee2.Number;
    var imprestFundCode = local.Initiailized.ImprestFundCode ?? "";
    var classification = local.HardcodeSupport.Classification;
    var type1 = local.Imported.Type1;
    var interstateInd = "N";

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
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
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
    entities.PaymentRequest.ImprestFundCode = imprestFundCode;
    entities.PaymentRequest.Classification = classification;
    entities.PaymentRequest.AchFormatCode = "";
    entities.PaymentRequest.Type1 = type1;
    entities.PaymentRequest.PrqRGeneratedId = null;
    entities.PaymentRequest.InterstateInd = interstateInd;
    entities.PaymentRequest.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee2.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayeeCsePerson()
  {
    entities.Designated.Populated = false;
    entities.CsePersonDesigPayee.Populated = false;

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
        entities.Designated.Number = db.GetString(reader, 4);
        entities.Designated.Populated = true;
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligee2.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.DisbursementTransaction.DesignatedPayee =
          db.GetNullableString(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private bool ReadPersonPreferredPaymentMethodPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;
    entities.PersonPreferredPaymentMethod.Populated = false;

    return Read("ReadPersonPreferredPaymentMethodPaymentMethodType",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "discontinueDate", date);
        db.
          SetString(command, "cspPNumber", local.ForPreferredPaymentMeth.Number);
          
      },
      (db, reader) =>
      {
        entities.PersonPreferredPaymentMethod.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.SystemGeneratedIdentifier =
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
        entities.PersonPreferredPaymentMethod.Description =
          db.GetNullableString(reader, 7);
        entities.PersonPreferredPaymentMethod.AccountType =
          db.GetNullableString(reader, 8);
        entities.PaymentMethodType.Populated = true;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private ProgramProcessingInfo programProcessingInfo;
    private PaymentRequest paymentRequest;
    private CsePerson obligee;
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

    private CsePerson csePerson;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of HardcodedPotRcv.
    /// </summary>
    [JsonPropertyName("hardcodedPotRcv")]
    public PaymentStatus HardcodedPotRcv
    {
      get => hardcodedPotRcv ??= new();
      set => hardcodedPotRcv = value;
    }

    /// <summary>
    /// A value of Imported.
    /// </summary>
    [JsonPropertyName("imported")]
    public PaymentRequest Imported
    {
      get => imported ??= new();
      set => imported = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of PayeeForPaymentMethod.
    /// </summary>
    [JsonPropertyName("payeeForPaymentMethod")]
    public CsePerson PayeeForPaymentMethod
    {
      get => payeeForPaymentMethod ??= new();
      set => payeeForPaymentMethod = value;
    }

    /// <summary>
    /// A value of ForPreferredPaymentMeth.
    /// </summary>
    [JsonPropertyName("forPreferredPaymentMeth")]
    public CsePerson ForPreferredPaymentMeth
    {
      get => forPreferredPaymentMeth ??= new();
      set => forPreferredPaymentMeth = value;
    }

    /// <summary>
    /// A value of EftCommon.
    /// </summary>
    [JsonPropertyName("eftCommon")]
    public Common EftCommon
    {
      get => eftCommon ??= new();
      set => eftCommon = value;
    }

    /// <summary>
    /// A value of EftPaymentMethodType.
    /// </summary>
    [JsonPropertyName("eftPaymentMethodType")]
    public PaymentMethodType EftPaymentMethodType
    {
      get => eftPaymentMethodType ??= new();
      set => eftPaymentMethodType = value;
    }

    /// <summary>
    /// A value of HardcodeWarrant.
    /// </summary>
    [JsonPropertyName("hardcodeWarrant")]
    public PaymentMethodType HardcodeWarrant
    {
      get => hardcodeWarrant ??= new();
      set => hardcodeWarrant = value;
    }

    /// <summary>
    /// A value of Initiailized.
    /// </summary>
    [JsonPropertyName("initiailized")]
    public PaymentRequest Initiailized
    {
      get => initiailized ??= new();
      set => initiailized = value;
    }

    /// <summary>
    /// A value of HardcodeSupport.
    /// </summary>
    [JsonPropertyName("hardcodeSupport")]
    public PaymentRequest HardcodeSupport
    {
      get => hardcodeSupport ??= new();
      set => hardcodeSupport = value;
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

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of HardcodeProcessed.
    /// </summary>
    [JsonPropertyName("hardcodeProcessed")]
    public DisbursementStatus HardcodeProcessed
    {
      get => hardcodeProcessed ??= new();
      set => hardcodeProcessed = value;
    }

    /// <summary>
    /// A value of HardcodeRequestedOrRcvpot.
    /// </summary>
    [JsonPropertyName("hardcodeRequestedOrRcvpot")]
    public PaymentStatus HardcodeRequestedOrRcvpot
    {
      get => hardcodeRequestedOrRcvpot ??= new();
      set => hardcodeRequestedOrRcvpot = value;
    }

    /// <summary>
    /// A value of ZdeleteMeZzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdeleteMeZzzzzzzzzzzzzzzzzzzz")]
    public ElectronicFundTransmission ZdeleteMeZzzzzzzzzzzzzzzzzzzz
    {
      get => zdeleteMeZzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdeleteMeZzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of ZdeleteMeBbbbbbbbbbbbbbbbbbbbb.
    /// </summary>
    [JsonPropertyName("zdeleteMeBbbbbbbbbbbbbbbbbbbbb")]
    public DateWorkArea ZdeleteMeBbbbbbbbbbbbbbbbbbbbb
    {
      get => zdeleteMeBbbbbbbbbbbbbbbbbbbbb ??= new();
      set => zdeleteMeBbbbbbbbbbbbbbbbbbbbb = value;
    }

    /// <summary>
    /// A value of ZdeleteMeTtttttttttttttttttttt.
    /// </summary>
    [JsonPropertyName("zdeleteMeTtttttttttttttttttttt")]
    public PaymentMethodType ZdeleteMeTtttttttttttttttttttt
    {
      get => zdeleteMeTtttttttttttttttttttt ??= new();
      set => zdeleteMeTtttttttttttttttttttt = value;
    }

    /// <summary>
    /// A value of ZdeleteMeAaaaaaaaaaaaaaaaaaaaa.
    /// </summary>
    [JsonPropertyName("zdeleteMeAaaaaaaaaaaaaaaaaaaaa")]
    public DateWorkArea ZdeleteMeAaaaaaaaaaaaaaaaaaaaa
    {
      get => zdeleteMeAaaaaaaaaaaaaaaaaaaaa ??= new();
      set => zdeleteMeAaaaaaaaaaaaaaaaaaaaa = value;
    }

    /// <summary>
    /// A value of ZdeleteMeNnnnnnnnnnnnnnnnnnnnn.
    /// </summary>
    [JsonPropertyName("zdeleteMeNnnnnnnnnnnnnnnnnnnnn")]
    public Common ZdeleteMeNnnnnnnnnnnnnnnnnnnnn
    {
      get => zdeleteMeNnnnnnnnnnnnnnnnnnnnn ??= new();
      set => zdeleteMeNnnnnnnnnnnnnnnnnnnnn = value;
    }

    /// <summary>
    /// A value of ZdeleteMeZzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdeleteMeZzzzzzzzzzzzzzzzzzzzz")]
    public ElectronicFundTransmission ZdeleteMeZzzzzzzzzzzzzzzzzzzzz
    {
      get => zdeleteMeZzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdeleteMeZzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of ZdeleteMeVvvvvvvvvvvvvvvvvvvvv.
    /// </summary>
    [JsonPropertyName("zdeleteMeVvvvvvvvvvvvvvvvvvvvv")]
    public ElectronicFundTransmission ZdeleteMeVvvvvvvvvvvvvvvvvvvvv
    {
      get => zdeleteMeVvvvvvvvvvvvvvvvvvvvv ??= new();
      set => zdeleteMeVvvvvvvvvvvvvvvvvvvvv = value;
    }

    /// <summary>
    /// A value of ZdeleteMeXxxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("zdeleteMeXxxxxxxxxxxxxxxxxxxxx")]
    public ElectronicFundTransmission ZdeleteMeXxxxxxxxxxxxxxxxxxxxx
    {
      get => zdeleteMeXxxxxxxxxxxxxxxxxxxxx ??= new();
      set => zdeleteMeXxxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of ZdeleteMeMmmmmmmmmmmmmmmmmmmmm.
    /// </summary>
    [JsonPropertyName("zdeleteMeMmmmmmmmmmmmmmmmmmmmm")]
    public ProgramProcessingInfo ZdeleteMeMmmmmmmmmmmmmmmmmmmmm
    {
      get => zdeleteMeMmmmmmmmmmmmmmmmmmmmm ??= new();
      set => zdeleteMeMmmmmmmmmmmmmmmmmmmmm = value;
    }

    private CsePerson designatedPayee;
    private PaymentStatus hardcodedPotRcv;
    private PaymentRequest imported;
    private DateWorkArea zero;
    private CsePerson payeeForPaymentMethod;
    private CsePerson forPreferredPaymentMeth;
    private Common eftCommon;
    private PaymentMethodType eftPaymentMethodType;
    private PaymentMethodType hardcodeWarrant;
    private PaymentRequest initiailized;
    private PaymentRequest hardcodeSupport;
    private Common createAttempts;
    private Common numberOfUpdates;
    private DisbursementStatus hardcodeProcessed;
    private PaymentStatus hardcodeRequestedOrRcvpot;
    private ElectronicFundTransmission zdeleteMeZzzzzzzzzzzzzzzzzzzz;
    private DateWorkArea zdeleteMeBbbbbbbbbbbbbbbbbbbbb;
    private PaymentMethodType zdeleteMeTtttttttttttttttttttt;
    private DateWorkArea zdeleteMeAaaaaaaaaaaaaaaaaaaaa;
    private Common zdeleteMeNnnnnnnnnnnnnnnnnnnnn;
    private ElectronicFundTransmission zdeleteMeZzzzzzzzzzzzzzzzzzzzz;
    private ElectronicFundTransmission zdeleteMeVvvvvvvvvvvvvvvvvvvvv;
    private ElectronicFundTransmission zdeleteMeXxxxxxxxxxxxxxxxxxxxx;
    private ProgramProcessingInfo zdeleteMeMmmmmmmmmmmmmmmmmmmmm;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    /// <summary>
    /// A value of ZdeleteMeGgggggggggggggggggggg.
    /// </summary>
    [JsonPropertyName("zdeleteMeGgggggggggggggggggggg")]
    public PaymentMethodType ZdeleteMeGgggggggggggggggggggg
    {
      get => zdeleteMeGgggggggggggggggggggg ??= new();
      set => zdeleteMeGgggggggggggggggggggg = value;
    }

    /// <summary>
    /// A value of ZdeleteMeXxxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("zdeleteMeXxxxxxxxxxxxxxxxxxxxx")]
    public CsePerson ZdeleteMeXxxxxxxxxxxxxxxxxxxxx
    {
      get => zdeleteMeXxxxxxxxxxxxxxxxxxxxx ??= new();
      set => zdeleteMeXxxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of Designated.
    /// </summary>
    [JsonPropertyName("designated")]
    public CsePerson Designated
    {
      get => designated ??= new();
      set => designated = value;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private PaymentMethodType paymentMethodType;
    private PaymentMethodType zdeleteMeGgggggggggggggggggggg;
    private CsePerson zdeleteMeXxxxxxxxxxxxxxxxxxxxx;
    private CsePerson designated;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePersonAccount obligee1;
    private CsePerson payee;
    private CsePerson obligee2;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private DisbursementTransaction disbursementTransaction;
    private PaymentRequest paymentRequest;
  }
#endregion
}
