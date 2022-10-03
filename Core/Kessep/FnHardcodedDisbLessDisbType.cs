// Program: FN_HARDCODED_DISB_LESS_DISB_TYPE, ID: 372544597, model: 746.
// Short name: SWE02426
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_HARDCODED_DISB_LESS_DISB_TYPE.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will set the hardcoded types, reasons and statuses that 
/// are located in the disbursement management subject area.
/// </para>
/// </summary>
[Serializable]
public partial class FnHardcodedDisbLessDisbType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODED_DISB_LESS_DISB_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodedDisbLessDisbType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodedDisbLessDisbType.
  /// </summary>
  public FnHardcodedDisbLessDisbType(IContext context, Import import,
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
    // Broke the ridiculous fn hardcoded disbursement info action block into two
    // parts. rk 1/21/98
    // ****************************************************************
    // *****
    // Set the CSE Person number for the state so that we will know when the 
    // state is the obligee.
    // Changed the value from 0000000001 to 000000017O.  RK 1/11/98
    // *****
    export.StateObligee.Number = "000000017O";

    // *****
    // Set Disbursement Transaction Types
    // *****
    export.CollectionDisbursementTransaction.Type1 = "C";
    export.PassthruDisbursementTransaction.Type1 = "P";
    export.DisbursementDisbursementTransaction.Type1 = "D";

    // *****
    // Set Recapture Rule Types
    // *****
    export.Obligor.Type1 = "O";
    export.Default1.Type1 = "D";

    // *****
    // Set Disbursement Suppression Status History Types
    // *****
    export.Person.Type1 = "P";
    export.CollectionType.Type1 = "C";
    export.Automatic.Type1 = "A";

    // *****
    // Set Disbursement Transaction System Genererated IDs
    // *****
    export.CollectionDisbursementTransactionType.SystemGeneratedIdentifier = 1;
    export.DisbursementDisbursementTransactionType.SystemGeneratedIdentifier =
      2;
    export.PassthruDisbursementTransactionType.SystemGeneratedIdentifier = 3;

    // *****
    // Set Disbursement Statuses
    // *****
    // ************************
    // Added in Reversed - 4
    // ************************
    export.Released.SystemGeneratedIdentifier = 1;
    export.Processed.SystemGeneratedIdentifier = 2;
    export.Suppressed.SystemGeneratedIdentifier = 3;
    export.Reversed.SystemGeneratedIdentifier = 4;

    // *****
    // Set Payment Statuses
    // *****
    // ****************************************************************
    // Changed the following from old designation to new:
    // stop - 6,7. reisreq - 7,10. reisden - 9,11. reml - 10,9
    // lost - 11,21. can - 12,6. candun - 13,12. outlaw - 14,13.
    // ****************************************************************
    export.Req.SystemGeneratedIdentifier = 1;
    export.Doa.SystemGeneratedIdentifier = 2;
    export.Paid.SystemGeneratedIdentifier = 3;
    export.Ret.SystemGeneratedIdentifier = 4;
    export.Held.SystemGeneratedIdentifier = 5;
    export.Can.SystemGeneratedIdentifier = 6;
    export.Stop.SystemGeneratedIdentifier = 7;
    export.Reis.SystemGeneratedIdentifier = 8;
    export.Reml.SystemGeneratedIdentifier = 9;
    export.Reisreq.SystemGeneratedIdentifier = 10;
    export.Reisden.SystemGeneratedIdentifier = 11;
    export.Candum.SystemGeneratedIdentifier = 12;
    export.Outlaw.SystemGeneratedIdentifier = 13;
    export.Lost.SystemGeneratedIdentifier = 21;

    // *****
    // Set Disbursement(Payment) Method Types
    // *****
    // ****************************************************************
    // Changed the following from old designation to new:
    // warrant - 1,27. Recap - 4,6. off system(formerly central impressed) 6,8.
    // Added Recovery  its a 7, and took out local impressed 7
    // ****************************************************************
    export.Eft.SystemGeneratedIdentifier = 2;
    export.InterfundVoucher.SystemGeneratedIdentifier = 3;
    export.JournalVoucher.SystemGeneratedIdentifier = 5;
    export.Recapture.SystemGeneratedIdentifier = 6;
    export.Recovery.SystemGeneratedIdentifier = 7;
    export.OffSystem.SystemGeneratedIdentifier = 8;
    export.Warrant.SystemGeneratedIdentifier = 27;

    // *****
    // Set Program Types
    // *****
    export.Af.ProgramCode = "AF";
    export.Na.ProgramCode = "NA";
    export.Fc.ProgramCode = "FC";
    export.Nf.ProgramCode = "NF";

    // *****
    // Set Disbursement Transaction Relation Reasons
    // *****
    export.IsRelatedTo.SystemGeneratedIdentifier = 1;

    // *****
    // Set CSE Person Relation Reasons
    // *****
    export.DesignatedPayee.SystemGeneratedIdentifier = 1;

    // *****
    // Set Payment Request classifications
    // *****
    export.Refund.Classification = "REF";
    export.Support.Classification = "SUP";
    export.Advancement.Classification = "ADV";
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public DisbursementType Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public DisbursementType Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public DisbursementType Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public DisbursementType Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of StateObligee.
    /// </summary>
    [JsonPropertyName("stateObligee")]
    public CsePerson StateObligee
    {
      get => stateObligee ??= new();
      set => stateObligee = value;
    }

    /// <summary>
    /// A value of CollectionDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("collectionDisbursementTransaction")]
    public DisbursementTransaction CollectionDisbursementTransaction
    {
      get => collectionDisbursementTransaction ??= new();
      set => collectionDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("passthruDisbursementTransaction")]
    public DisbursementTransaction PassthruDisbursementTransaction
    {
      get => passthruDisbursementTransaction ??= new();
      set => passthruDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementDisbursementTransaction")]
    public DisbursementTransaction DisbursementDisbursementTransaction
    {
      get => disbursementDisbursementTransaction ??= new();
      set => disbursementDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public RecaptureRule Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public RecaptureRule Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public DisbSuppressionStatusHistory Person
    {
      get => person ??= new();
      set => person = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public DisbSuppressionStatusHistory CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public DisbSuppressionStatusHistory Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
    }

    /// <summary>
    /// A value of CollectionDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("collectionDisbursementTransactionType")]
    public DisbursementTransactionType CollectionDisbursementTransactionType
    {
      get => collectionDisbursementTransactionType ??= new();
      set => collectionDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of DisbursementDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementDisbursementTransactionType")]
    public DisbursementTransactionType DisbursementDisbursementTransactionType
    {
      get => disbursementDisbursementTransactionType ??= new();
      set => disbursementDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("passthruDisbursementTransactionType")]
    public DisbursementTransactionType PassthruDisbursementTransactionType
    {
      get => passthruDisbursementTransactionType ??= new();
      set => passthruDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public DisbursementStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Suppressed.
    /// </summary>
    [JsonPropertyName("suppressed")]
    public DisbursementStatus Suppressed
    {
      get => suppressed ??= new();
      set => suppressed = value;
    }

    /// <summary>
    /// A value of Reversed.
    /// </summary>
    [JsonPropertyName("reversed")]
    public DisbursementStatus Reversed
    {
      get => reversed ??= new();
      set => reversed = value;
    }

    /// <summary>
    /// A value of Req.
    /// </summary>
    [JsonPropertyName("req")]
    public PaymentStatus Req
    {
      get => req ??= new();
      set => req = value;
    }

    /// <summary>
    /// A value of Doa.
    /// </summary>
    [JsonPropertyName("doa")]
    public PaymentStatus Doa
    {
      get => doa ??= new();
      set => doa = value;
    }

    /// <summary>
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public PaymentStatus Paid
    {
      get => paid ??= new();
      set => paid = value;
    }

    /// <summary>
    /// A value of Ret.
    /// </summary>
    [JsonPropertyName("ret")]
    public PaymentStatus Ret
    {
      get => ret ??= new();
      set => ret = value;
    }

    /// <summary>
    /// A value of Held.
    /// </summary>
    [JsonPropertyName("held")]
    public PaymentStatus Held
    {
      get => held ??= new();
      set => held = value;
    }

    /// <summary>
    /// A value of Stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public PaymentStatus Stop
    {
      get => stop ??= new();
      set => stop = value;
    }

    /// <summary>
    /// A value of Reisreq.
    /// </summary>
    [JsonPropertyName("reisreq")]
    public PaymentStatus Reisreq
    {
      get => reisreq ??= new();
      set => reisreq = value;
    }

    /// <summary>
    /// A value of Reis.
    /// </summary>
    [JsonPropertyName("reis")]
    public PaymentStatus Reis
    {
      get => reis ??= new();
      set => reis = value;
    }

    /// <summary>
    /// A value of Reisden.
    /// </summary>
    [JsonPropertyName("reisden")]
    public PaymentStatus Reisden
    {
      get => reisden ??= new();
      set => reisden = value;
    }

    /// <summary>
    /// A value of Reml.
    /// </summary>
    [JsonPropertyName("reml")]
    public PaymentStatus Reml
    {
      get => reml ??= new();
      set => reml = value;
    }

    /// <summary>
    /// A value of Lost.
    /// </summary>
    [JsonPropertyName("lost")]
    public PaymentStatus Lost
    {
      get => lost ??= new();
      set => lost = value;
    }

    /// <summary>
    /// A value of Can.
    /// </summary>
    [JsonPropertyName("can")]
    public PaymentStatus Can
    {
      get => can ??= new();
      set => can = value;
    }

    /// <summary>
    /// A value of Candum.
    /// </summary>
    [JsonPropertyName("candum")]
    public PaymentStatus Candum
    {
      get => candum ??= new();
      set => candum = value;
    }

    /// <summary>
    /// A value of Outlaw.
    /// </summary>
    [JsonPropertyName("outlaw")]
    public PaymentStatus Outlaw
    {
      get => outlaw ??= new();
      set => outlaw = value;
    }

    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentMethodType Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public PaymentMethodType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of InterfundVoucher.
    /// </summary>
    [JsonPropertyName("interfundVoucher")]
    public PaymentMethodType InterfundVoucher
    {
      get => interfundVoucher ??= new();
      set => interfundVoucher = value;
    }

    /// <summary>
    /// A value of Recapture.
    /// </summary>
    [JsonPropertyName("recapture")]
    public PaymentMethodType Recapture
    {
      get => recapture ??= new();
      set => recapture = value;
    }

    /// <summary>
    /// A value of Recovery.
    /// </summary>
    [JsonPropertyName("recovery")]
    public PaymentMethodType Recovery
    {
      get => recovery ??= new();
      set => recovery = value;
    }

    /// <summary>
    /// A value of JournalVoucher.
    /// </summary>
    [JsonPropertyName("journalVoucher")]
    public PaymentMethodType JournalVoucher
    {
      get => journalVoucher ??= new();
      set => journalVoucher = value;
    }

    /// <summary>
    /// A value of OffSystem.
    /// </summary>
    [JsonPropertyName("offSystem")]
    public PaymentMethodType OffSystem
    {
      get => offSystem ??= new();
      set => offSystem = value;
    }

    /// <summary>
    /// A value of IsRelatedTo.
    /// </summary>
    [JsonPropertyName("isRelatedTo")]
    public DisbursementTranRlnRsn IsRelatedTo
    {
      get => isRelatedTo ??= new();
      set => isRelatedTo = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonRlnRsn DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public PaymentRequest Refund
    {
      get => refund ??= new();
      set => refund = value;
    }

    /// <summary>
    /// A value of Support.
    /// </summary>
    [JsonPropertyName("support")]
    public PaymentRequest Support
    {
      get => support ??= new();
      set => support = value;
    }

    /// <summary>
    /// A value of Advancement.
    /// </summary>
    [JsonPropertyName("advancement")]
    public PaymentRequest Advancement
    {
      get => advancement ??= new();
      set => advancement = value;
    }

    private DisbursementType af;
    private DisbursementType fc;
    private DisbursementType na;
    private DisbursementType nf;
    private CsePerson stateObligee;
    private DisbursementTransaction collectionDisbursementTransaction;
    private DisbursementTransaction passthruDisbursementTransaction;
    private DisbursementTransaction disbursementDisbursementTransaction;
    private RecaptureRule obligor;
    private RecaptureRule default1;
    private DisbSuppressionStatusHistory person;
    private DisbSuppressionStatusHistory collectionType;
    private DisbSuppressionStatusHistory automatic;
    private DisbursementTransactionType collectionDisbursementTransactionType;
    private DisbursementTransactionType disbursementDisbursementTransactionType;
    private DisbursementTransactionType passthruDisbursementTransactionType;
    private DisbursementStatus released;
    private DisbursementStatus processed;
    private DisbursementStatus suppressed;
    private DisbursementStatus reversed;
    private PaymentStatus req;
    private PaymentStatus doa;
    private PaymentStatus paid;
    private PaymentStatus ret;
    private PaymentStatus held;
    private PaymentStatus stop;
    private PaymentStatus reisreq;
    private PaymentStatus reis;
    private PaymentStatus reisden;
    private PaymentStatus reml;
    private PaymentStatus lost;
    private PaymentStatus can;
    private PaymentStatus candum;
    private PaymentStatus outlaw;
    private PaymentMethodType warrant;
    private PaymentMethodType eft;
    private PaymentMethodType interfundVoucher;
    private PaymentMethodType recapture;
    private PaymentMethodType recovery;
    private PaymentMethodType journalVoucher;
    private PaymentMethodType offSystem;
    private DisbursementTranRlnRsn isRelatedTo;
    private CsePersonRlnRsn designatedPayee;
    private PaymentRequest refund;
    private PaymentRequest support;
    private PaymentRequest advancement;
  }
#endregion
}
