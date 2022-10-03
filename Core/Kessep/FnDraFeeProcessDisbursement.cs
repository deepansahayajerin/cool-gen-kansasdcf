// Program: FN_DRA_FEE_PROCESS_DISBURSEMENT, ID: 371343759, model: 746.
// Short name: SWE02029
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DRA_FEE_PROCESS_DISBURSEMENT.
/// </summary>
[Serializable]
public partial class FnDraFeeProcessDisbursement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DRA_FEE_PROCESS_DISBURSEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDraFeeProcessDisbursement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDraFeeProcessDisbursement.
  /// </summary>
  public FnDraFeeProcessDisbursement(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 03/17/2008  GVandy	CQ296		Initial Coding
    // -----------------------------------------------------------------------------------------------------------------
    export.Obligee.Number = import.Obligee.Number;
    MoveDisbursementTransaction(import.Debit, export.Debit);
    export.DisbCountedForDraFee.Flag = "N";

    // -- Read the obligee.
    if (ReadCsePersonObligee())
    {
      export.Obligee.Number = entities.Obligee2.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // -- Read one of the debit disbursement for the reference number on the 
    // specified date.  Any debit disbursement
    // record on this date will suffice.  We are only reading this to ultimately
    // find the associated obligor.
    if (!ReadDisbursementTransaction1())
    {
      ExitState = "FN0000_DISB_TRANS_NF";

      return;
    }

    // -- Find the obligor on the associated credit disbursement(s).
    if (ReadObligorCsePerson())
    {
      export.Obligor.Number = entities.Obligor1.Number;
    }
    else
    {
      ExitState = "FN0000_OBLIGOR_NF";

      return;
    }

    // -- Determine the net amount of this disbursement, excluding cost recovery
    // fees.
    ReadDisbursementTransaction2();

    if (AsChar(entities.Obligee2.Type1) != 'C')
    {
      // -- No additional processing required if the if the obligee is an 
      // organization.
      return;
    }

    if (export.Debit.Amount <= 0)
    {
      if (ReadPaymentRequest())
      {
        // -- If the debit disbursements are tied to a warrant then even though 
        // the amount is negative we will want to include it.
        // The scenario is that this particular disbursement debit offset an 
        // amount from another disbursement debit, possibly
        // from a different AP, and the result was a warrant reduced by the 
        // amount of this particular disbursement debit.
        // In this case a potential recovery will not be created because we have
        // essentially recaptured the amount by reducing the warrant amount.
        // -- Continue.
      }
      else
      {
        // -- The disbursement did not result in a positive amount and is not 
        // associated to a warrant.  No additional processing required.
        // (i.e. If the net is zero it is likely an adjustment that did not 
        // result in any change to the disbursement.
        // If the net is less than zero then the adjustment did/will result in a
        // potential recovery.  We'll apply
        // the Recovery amount to the disbursement summary record(s) when a 
        // Recovery obligation is created on OREC.)
        return;
      }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Determine whether the obligor/obligee combination met the definition of a
    // non-TAF case on the date of the
    // disbursement.  Details regarding determination of non-TAF case are 
    // included at in the called action block.
    // ------------------------------------------------------------------------------------------------------------
    local.DateWorkArea.Date = import.Debit.ProcessDate;
    UseFnDraFeeTafDetermination();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(export.TafIndicator.Flag) == 'Y')
    {
      return;
    }

    // -- Add the disbursement amount to the disbursement summary record for 
    // this obligee/obligor combo.
    UseFnDraFeeMaintainDisbSummary();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -- If we make it this far we successfully applied the net debit 
    // disbursement amount to the AR/AP combo Disbursement_Summary
    // non_taf_amount.
    export.DisbCountedForDraFee.Flag = "Y";
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.Type1 = source.Type1;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseFnDraFeeMaintainDisbSummary()
  {
    var useImport = new FnDraFeeMaintainDisbSummary.Import();
    var useExport = new FnDraFeeMaintainDisbSummary.Export();

    useImport.DisbursementTransaction.Assign(export.Debit);
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.Obligee.Number = entities.Obligee2.Number;

    Call(FnDraFeeMaintainDisbSummary.Execute, useImport, useExport);
  }

  private void UseFnDraFeeTafDetermination()
  {
    var useImport = new FnDraFeeTafDetermination.Import();
    var useExport = new FnDraFeeTafDetermination.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.Obligee.Number = entities.Obligee2.Number;

    Call(FnDraFeeTafDetermination.Execute, useImport, useExport);

    export.TafIndicator.Flag = useExport.TafIndicator.Flag;
  }

  private bool ReadCsePersonObligee()
  {
    entities.Obligee1.Populated = false;
    entities.Obligee2.Populated = false;

    return Read("ReadCsePersonObligee",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee1.Type1 = db.GetString(reader, 2);
        entities.Obligee1.Populated = true;
        entities.Obligee2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligee2.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligee1.Type1);
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.Debit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "referenceNumber", import.Debit.ReferenceNumber ?? "");
        db.SetNullableDate(
          command, "processDate", import.Debit.ProcessDate.GetValueOrDefault());
          
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Debit.PrqGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.Debit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    export.Debit.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate", import.Debit.ProcessDate.GetValueOrDefault());
          
        db.SetNullableString(
          command, "referenceNumber", import.Debit.ReferenceNumber ?? "");
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
      },
      (db, reader) =>
      {
        export.Debit.Amount = db.GetDecimal(reader, 0);
        export.Debit.Populated = true;
      });
  }

  private bool ReadObligorCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.Obligor1.Populated = false;
    entities.Obligor2.Populated = false;

    return Read("ReadObligorCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor2.CspNumber = db.GetString(reader, 0);
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor2.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Populated = true;
        entities.Obligor2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor2.Type1);
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableString(
          command, "referenceNumber", import.Debit.ReferenceNumber ?? "");
        db.SetNullableDate(
          command, "processDate", import.Debit.ProcessDate.GetValueOrDefault());
          
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Type1 = db.GetString(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    private CsePerson obligee;
    private DisbursementTransaction debit;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TafIndicator.
    /// </summary>
    [JsonPropertyName("tafIndicator")]
    public Common TafIndicator
    {
      get => tafIndicator ??= new();
      set => tafIndicator = value;
    }

    /// <summary>
    /// A value of DisbCountedForDraFee.
    /// </summary>
    [JsonPropertyName("disbCountedForDraFee")]
    public Common DisbCountedForDraFee
    {
      get => disbCountedForDraFee ??= new();
      set => disbCountedForDraFee = value;
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private Common tafIndicator;
    private Common disbCountedForDraFee;
    private DisbursementTransaction debit;
    private CsePerson obligor;
    private CsePerson obligee;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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

    private PaymentRequest paymentRequest;
    private Collection collection;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private DisbursementTransaction credit;
    private DisbursementType disbursementType;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction debit;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
  }
#endregion
}
