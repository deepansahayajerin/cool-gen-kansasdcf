// Program: FN_B652_ASSOC_PT_DISB_DEBITS, ID: 372709117, model: 746.
// Short name: SWE02165
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
/// A program: FN_B652_ASSOC_PT_DISB_DEBITS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block:
///   - Creates a warrant or potential recovery if it is not already created.
///   - Associates the disb debits with the warrant or potential recovery 
/// payment request
/// for the given obligee.
/// </para>
/// </summary>
[Serializable]
public partial class FnB652AssocPtDisbDebits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_ASSOC_PT_DISB_DEBITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652AssocPtDisbDebits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652AssocPtDisbDebits.
  /// </summary>
  public FnB652AssocPtDisbDebits(IContext context, Import import, Export export):
    
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
    // This acblk associates the passthru disbursements of the specified month 
    // with the payment request. If the payment request has not been created, it
    // creates one now and associates the passthru disb debits.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 121097	govind		Initial code
    // ---------------------------------------------
    MoveElectronicFundTransmission(import.ElectronicFundTransmission,
      export.ElectronicFundTransmission);
    export.EftCreated.Flag = import.EftCreated.Flag;
    local.Current.Date = Now().Date;
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Zero.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    }

    UseOeGetMonthStartAndEndDate();

    if (AsChar(local.InvalidMonth.Flag) == 'Y')
    {
      ExitState = "OE0000_INVALID_MONTH";

      return;
    }

    local.HardcodedPassthru.SystemGeneratedIdentifier = 71;
    MovePaymentRequest1(import.WarrOrPotRec, export.EftWarrOrPotRec);

    if (!ReadCsePerson())
    {
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";

      return;
    }

    if (!ReadObligee())
    {
      ExitState = "FN0000_OBLIGEE_NF";

      return;
    }

    if (import.WarrOrPotRec.SystemGeneratedIdentifier == 0)
    {
      // --- The Payment Request has not been created yet. Create one now.
      UseFnCreatePaymentRequest();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (!ReadPaymentRequest())
    {
      ExitState = "FN0000_PAYMENT_REQUEST_NF";

      return;
    }

    // ---------------------------------------------
    // Read each Disb Tran and associate it with the Payment Request.
    // Note that the Collection Date in Passthru Disb Debit record contains the 
    // last date of the passthru month.
    // ---------------------------------------------
    foreach(var item in ReadDisbursementTransaction())
    {
      try
      {
        UpdateDisbursementTransaction();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_TRAN_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        UpdatePaymentRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PAYMENT_REQUEST_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PAYMENT_REQUEST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    export.EftWarrOrPotRec.Assign(entities.WarrEftOrPotRec);
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.TraceNumber = source.TraceNumber;
  }

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private void UseFnCreatePaymentRequest()
  {
    var useImport = new FnCreatePaymentRequest.Import();
    var useExport = new FnCreatePaymentRequest.Export();

    useImport.Obligee.Number = entities.Obligee2.Number;
    MovePaymentRequest2(import.WarrOrPotRec, useImport.PaymentRequest);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(FnCreatePaymentRequest.Execute, useImport, useExport);

    local.DesigPayeeForEft.Number = useExport.CsePerson.Number;
    MovePaymentRequest1(useExport.PaymentRequest, export.EftWarrOrPotRec);
    export.Eft.Assign(useExport.PersonPreferredPaymentMethod);
  }

  private void UseOeGetMonthStartAndEndDate()
  {
    var useImport = new OeGetMonthStartAndEndDate.Import();
    var useExport = new OeGetMonthStartAndEndDate.Export();

    useImport.DateWorkArea.YearMonth = import.PassthruMonth.YearMonth;

    Call(OeGetMonthStartAndEndDate.Execute, useImport, useExport);

    local.InvalidMonth.Flag = useExport.InvalidMonth.Flag;
    local.PassthruMonthEndDate.Date = useExport.MonthEndDate.Date;
    local.PassthruMonthStartDate.Date = useExport.MonthStartDate.Date;
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

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.PassThruDebit2.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
        db.SetDate(
          command, "date1",
          local.PassthruMonthStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          local.PassthruMonthEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate", local.Zero.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "dbtGeneratedId",
          local.HardcodedPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PassThruDebit2.CpaType = db.GetString(reader, 0);
        entities.PassThruDebit2.CspNumber = db.GetString(reader, 1);
        entities.PassThruDebit2.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PassThruDebit2.Type1 = db.GetString(reader, 3);
        entities.PassThruDebit2.Amount = db.GetDecimal(reader, 4);
        entities.PassThruDebit2.ProcessDate = db.GetNullableDate(reader, 5);
        entities.PassThruDebit2.CashNonCashInd = db.GetString(reader, 6);
        entities.PassThruDebit2.RecapturedInd = db.GetString(reader, 7);
        entities.PassThruDebit2.CollectionDate = db.GetNullableDate(reader, 8);
        entities.PassThruDebit2.DbtGeneratedId = db.GetNullableInt32(reader, 9);
        entities.PassThruDebit2.PrqGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.PassThruDebit2.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.PassThruDebit2.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.PassThruDebit2.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.PassThruDebit2.CashNonCashInd);

        return true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee1.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligee2.Number);
      },
      (db, reader) =>
      {
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.Obligee1.Type1 = db.GetString(reader, 1);
        entities.Obligee1.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee1.Type1);
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.WarrEftOrPotRec.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.EftWarrOrPotRec.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrEftOrPotRec.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrEftOrPotRec.ProcessDate = db.GetDate(reader, 1);
        entities.WarrEftOrPotRec.Amount = db.GetDecimal(reader, 2);
        entities.WarrEftOrPotRec.CreatedBy = db.GetString(reader, 3);
        entities.WarrEftOrPotRec.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.WarrEftOrPotRec.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.WarrEftOrPotRec.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.WarrEftOrPotRec.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.WarrEftOrPotRec.Classification = db.GetString(reader, 8);
        entities.WarrEftOrPotRec.Type1 = db.GetString(reader, 9);
        entities.WarrEftOrPotRec.PrqRGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.WarrEftOrPotRec.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.WarrEftOrPotRec.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.WarrEftOrPotRec.Type1);
      });
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.PassThruDebit2.Populated);

    var processDate = local.ProgramProcessingInfo.ProcessDate;
    var prqGeneratedId = entities.WarrEftOrPotRec.SystemGeneratedIdentifier;

    entities.PassThruDebit2.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(command, "cpaType", entities.PassThruDebit2.CpaType);
        db.SetString(command, "cspNumber", entities.PassThruDebit2.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.PassThruDebit2.SystemGeneratedIdentifier);
      });

    entities.PassThruDebit2.ProcessDate = processDate;
    entities.PassThruDebit2.PrqGeneratedId = prqGeneratedId;
    entities.PassThruDebit2.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    var amount =
      entities.WarrEftOrPotRec.Amount + entities.PassThruDebit2.Amount;

    entities.WarrEftOrPotRec.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.WarrEftOrPotRec.SystemGeneratedIdentifier);
      });

    entities.WarrEftOrPotRec.Amount = amount;
    entities.WarrEftOrPotRec.Populated = true;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftCreated.
    /// </summary>
    [JsonPropertyName("eftCreated")]
    public Common EftCreated
    {
      get => eftCreated ??= new();
      set => eftCreated = value;
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
    /// A value of WarrOrPotRec.
    /// </summary>
    [JsonPropertyName("warrOrPotRec")]
    public PaymentRequest WarrOrPotRec
    {
      get => warrOrPotRec ??= new();
      set => warrOrPotRec = value;
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

    /// <summary>
    /// A value of PassthruMonth.
    /// </summary>
    [JsonPropertyName("passthruMonth")]
    public DateWorkArea PassthruMonth
    {
      get => passthruMonth ??= new();
      set => passthruMonth = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private Common eftCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private PaymentRequest warrOrPotRec;
    private CsePerson obligee;
    private DateWorkArea passthruMonth;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public PersonPreferredPaymentMethod Eft
    {
      get => eft ??= new();
      set => eft = value;
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
    /// A value of EftCreated.
    /// </summary>
    [JsonPropertyName("eftCreated")]
    public Common EftCreated
    {
      get => eftCreated ??= new();
      set => eftCreated = value;
    }

    /// <summary>
    /// A value of EftWarrOrPotRec.
    /// </summary>
    [JsonPropertyName("eftWarrOrPotRec")]
    public PaymentRequest EftWarrOrPotRec
    {
      get => eftWarrOrPotRec ??= new();
      set => eftWarrOrPotRec = value;
    }

    private PersonPreferredPaymentMethod eft;
    private ElectronicFundTransmission electronicFundTransmission;
    private Common eftCreated;
    private PaymentRequest eftWarrOrPotRec;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForEftElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("forEftElectronicFundTransmission")]
    public ElectronicFundTransmission ForEftElectronicFundTransmission
    {
      get => forEftElectronicFundTransmission ??= new();
      set => forEftElectronicFundTransmission = value;
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
    /// A value of DesigPayeeForEft.
    /// </summary>
    [JsonPropertyName("desigPayeeForEft")]
    public CsePerson DesigPayeeForEft
    {
      get => desigPayeeForEft ??= new();
      set => desigPayeeForEft = value;
    }

    /// <summary>
    /// A value of ForEftPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("forEftPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod ForEftPersonPreferredPaymentMethod
    {
      get => forEftPersonPreferredPaymentMethod ??= new();
      set => forEftPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of TempWarrOrPotRecAmunt.
    /// </summary>
    [JsonPropertyName("tempWarrOrPotRecAmunt")]
    public PaymentRequest TempWarrOrPotRecAmunt
    {
      get => tempWarrOrPotRecAmunt ??= new();
      set => tempWarrOrPotRecAmunt = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of InvalidMonth.
    /// </summary>
    [JsonPropertyName("invalidMonth")]
    public Common InvalidMonth
    {
      get => invalidMonth ??= new();
      set => invalidMonth = value;
    }

    /// <summary>
    /// A value of PassthruMonthEndDate.
    /// </summary>
    [JsonPropertyName("passthruMonthEndDate")]
    public DateWorkArea PassthruMonthEndDate
    {
      get => passthruMonthEndDate ??= new();
      set => passthruMonthEndDate = value;
    }

    /// <summary>
    /// A value of PassthruMonthStartDate.
    /// </summary>
    [JsonPropertyName("passthruMonthStartDate")]
    public DateWorkArea PassthruMonthStartDate
    {
      get => passthruMonthStartDate ??= new();
      set => passthruMonthStartDate = value;
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
    /// A value of HardcodedPassthru.
    /// </summary>
    [JsonPropertyName("hardcodedPassthru")]
    public DisbursementType HardcodedPassthru
    {
      get => hardcodedPassthru ??= new();
      set => hardcodedPassthru = value;
    }

    /// <summary>
    /// A value of HardcodedDebit.
    /// </summary>
    [JsonPropertyName("hardcodedDebit")]
    public DisbursementTransaction HardcodedDebit
    {
      get => hardcodedDebit ??= new();
      set => hardcodedDebit = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursed.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursed")]
    public DisbursementTranRlnRsn HardcodeDisbursed
    {
      get => hardcodeDisbursed ??= new();
      set => hardcodeDisbursed = value;
    }

    /// <summary>
    /// A value of HardcodedSuppressed.
    /// </summary>
    [JsonPropertyName("hardcodedSuppressed")]
    public DisbursementStatus HardcodedSuppressed
    {
      get => hardcodedSuppressed ??= new();
      set => hardcodedSuppressed = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public DisbursementStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    private ElectronicFundTransmission forEftElectronicFundTransmission;
    private Common backoffInd;
    private CsePerson desigPayeeForEft;
    private PersonPreferredPaymentMethod forEftPersonPreferredPaymentMethod;
    private PaymentRequest tempWarrOrPotRecAmunt;
    private DateWorkArea current;
    private DateWorkArea zero;
    private Common invalidMonth;
    private DateWorkArea passthruMonthEndDate;
    private DateWorkArea passthruMonthStartDate;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementType hardcodedPassthru;
    private DisbursementTransaction hardcodedDebit;
    private DisbursementTranRlnRsn hardcodeDisbursed;
    private DisbursementStatus hardcodedSuppressed;
    private DisbursementStatus hardcodedReleased;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PassthruDebit1.
    /// </summary>
    [JsonPropertyName("passthruDebit1")]
    public DisbursementType PassthruDebit1
    {
      get => passthruDebit1 ??= new();
      set => passthruDebit1 = value;
    }

    /// <summary>
    /// A value of PassThruDebit2.
    /// </summary>
    [JsonPropertyName("passThruDebit2")]
    public DisbursementTransaction PassThruDebit2
    {
      get => passThruDebit2 ??= new();
      set => passThruDebit2 = value;
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
    /// A value of WarrEftOrPotRec.
    /// </summary>
    [JsonPropertyName("warrEftOrPotRec")]
    public PaymentRequest WarrEftOrPotRec
    {
      get => warrEftOrPotRec ??= new();
      set => warrEftOrPotRec = value;
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

    private ElectronicFundTransmission electronicFundTransmission;
    private DisbursementType passthruDebit1;
    private DisbursementTransaction passThruDebit2;
    private CsePersonAccount obligee1;
    private PaymentRequest warrEftOrPotRec;
    private CsePerson obligee2;
  }
#endregion
}
