// Program: FN_DRA_FEE_PROCESS_RECOVERY_OBG, ID: 371343758, model: 746.
// Short name: SWE02030
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DRA_FEE_PROCESS_RECOVERY_OBG.
/// </summary>
[Serializable]
public partial class FnDraFeeProcessRecoveryObg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DRA_FEE_PROCESS_RECOVERY_OBG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDraFeeProcessRecoveryObg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDraFeeProcessRecoveryObg.
  /// </summary>
  public FnDraFeeProcessRecoveryObg(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // Process Recovery obligations for DRA Fee purposes.  For a non-TAF AR/AP 
    // combination, the creation of a Recovery
    // obligation should offset the non_taf_amount in the Disbursement Summary 
    // record by the amount of the Recovery
    // obligation attributable to that AR/AP combination.
    // -----------------------------------------------------------------------------------------------------------------
    local.Stop.Flag = "Y";

    // -- Read the Recovery obligation and debt.
    if (ReadObligationObligationTransaction())
    {
      export.Recovery.Amount = entities.RecoveryObligationTransaction.Amount;
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    local.DisbursementTransaction.ProcessDate =
      Date(entities.RecoveryObligation.CreatedTmst);
    local.RecoveryObCreateDate.Date = local.DisbursementTransaction.ProcessDate;

    // -- Determine if the Recovery obligation was created from a Potential 
    // Recovery payment request.
    if (ReadPaymentRequest())
    {
      // -- Determine the amount of the potential recovery attributable to each 
      // AP.
      foreach(var item in ReadDisbursementTransaction())
      {
        if (!ReadCsePerson())
        {
          continue;
        }

        local.PotentialRecoveryTotal.Amount += entities.Debit.Amount;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (Equal(export.Group.Item.G.Number, entities.Obligor2.Number))
          {
            export.Group.Update.GexportPotentialRecovery.Amount =
              export.Group.Item.GexportPotentialRecovery.Amount + entities
              .Debit.Amount;

            goto ReadEach;
          }
        }

        export.Group.Index = export.Group.Count;
        export.Group.CheckIndex();

        do
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.G.Number = entities.Obligor2.Number;
          export.Group.Update.GexportPotentialRecovery.Amount =
            entities.Debit.Amount;
          export.Group.Next();

          goto ReadEach;

          export.Group.Next();
        }
        while(AsChar(local.Stop.Flag) != 'Y');

ReadEach:
        ;
      }
    }
    else
    {
      // -- There is no potential recovery to identify the recovery amount that 
      // applies to each AP.  Therefore, for each
      // AP for whom non-TAF disbursements have been made in the fiscal year in 
      // which the recovery obligation was created,
      // we will use the non_taf_amount in the disbursement summary record as a 
      // substitute for a potential recovery amount.  The
      // disbursement summary non_taf_amount will then be used to determine how 
      // the recovery amount will be pro-rated among the APs.
      if (Month(local.RecoveryObCreateDate.Date) <= 9)
      {
        local.DisbursementSummary.FiscalYear =
          Year(local.RecoveryObCreateDate.Date);
      }
      else
      {
        local.DisbursementSummary.FiscalYear =
          Year(local.RecoveryObCreateDate.Date) + 1;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadCsePersonDisbursementSummary())
      {
        export.Group.Update.G.Number = entities.Obligor2.Number;
        export.Group.Update.GexportPotentialRecovery.Amount =
          entities.DisbursementSummary.NonTafAmount.GetValueOrDefault();
        local.PotentialRecoveryTotal.Amount += export.Group.Item.
          GexportPotentialRecovery.Amount;
        export.Group.Next();
      }
    }

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      // -- Determine if the AR/AP combo was non-taf on the date that the 
      // Recovery obligation was created.
      UseFnDraFeeTafDetermination();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(export.Group.Item.GexportTafIndicator.Flag) == 'N')
      {
        // -- Prorate the recovery obligation amount based on the percentage of 
        // the potential recovery total allocable to each AP.
        if (local.PotentialRecoveryTotal.Amount == 0)
        {
          local.DisbursementTransaction.Amount = 0;
        }
        else
        {
          local.DisbursementTransaction.Amount =
            Math.Round(
              -entities.RecoveryObligationTransaction.Amount * export
            .Group.Item.GexportPotentialRecovery.Amount /
            local.PotentialRecoveryTotal.Amount, 2,
            MidpointRounding.AwayFromZero);
        }

        export.Group.Update.GexportActualRecovery.Amount =
          local.DisbursementTransaction.Amount;

        // -- Update the disbursement summary non_taf_amount with the recovery 
        // amount that is attributable to this AR/AP combination.
        UseFnDraFeeMaintainDisbSummary();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
  }

  private void UseFnDraFeeMaintainDisbSummary()
  {
    var useImport = new FnDraFeeMaintainDisbSummary.Import();
    var useExport = new FnDraFeeMaintainDisbSummary.Export();

    useImport.Obligor.Number = export.Group.Item.G.Number;
    useImport.DisbursementTransaction.Assign(local.DisbursementTransaction);
    useImport.Obligee.Number = import.Ar.Number;

    Call(FnDraFeeMaintainDisbSummary.Execute, useImport, useExport);
  }

  private void UseFnDraFeeTafDetermination()
  {
    var useImport = new FnDraFeeTafDetermination.Import();
    var useExport = new FnDraFeeTafDetermination.Export();

    useImport.Obligor.Number = export.Group.Item.G.Number;
    useImport.DateWorkArea.Date = local.RecoveryObCreateDate.Date;
    useImport.Obligee.Number = import.Ar.Number;

    Call(FnDraFeeTafDetermination.Execute, useImport, useExport);

    export.Group.Update.GexportTafIndicator.Flag = useExport.TafIndicator.Flag;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.Obligor2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor2.Number = db.GetString(reader, 0);
        entities.Obligor2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonDisbursementSummary()
  {
    return ReadEach("ReadCsePersonDisbursementSummary",
      (db, command) =>
      {
        db.
          SetInt32(command, "fiscalYear", local.DisbursementSummary.FiscalYear);
          
        db.SetString(command, "cspNumberOblgee", import.Ar.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Obligor2.Number = db.GetString(reader, 0);
        entities.DisbursementSummary.CspNumberOblgr = db.GetString(reader, 0);
        entities.DisbursementSummary.FiscalYear = db.GetInt32(reader, 1);
        entities.DisbursementSummary.NonTafAmount =
          db.GetNullableDecimal(reader, 2);
        entities.DisbursementSummary.CspNumberOblgee = db.GetString(reader, 3);
        entities.DisbursementSummary.CpaTypeOblgee = db.GetString(reader, 4);
        entities.DisbursementSummary.CpaTypeOblgr = db.GetString(reader, 5);
        entities.DisbursementSummary.Populated = true;
        entities.Obligor2.Populated = true;
        CheckValid<DisbursementSummary>("CpaTypeOblgee",
          entities.DisbursementSummary.CpaTypeOblgee);
        CheckValid<DisbursementSummary>("CpaTypeOblgr",
          entities.DisbursementSummary.CpaTypeOblgr);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    entities.Debit.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PotentialRecovery.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Amount = db.GetDecimal(reader, 3);
        entities.Debit.PrqGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Debit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);

        return true;
      });
  }

  private bool ReadObligationObligationTransaction()
  {
    entities.RecoveryObligationTransaction.Populated = false;
    entities.RecoveryObligation.Populated = false;

    return Read("ReadObligationObligationTransaction",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.RecoveryObligation.CpaType = db.GetString(reader, 0);
        entities.RecoveryObligationTransaction.CpaType =
          db.GetString(reader, 0);
        entities.RecoveryObligation.CspNumber = db.GetString(reader, 1);
        entities.RecoveryObligationTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.RecoveryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.RecoveryObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.RecoveryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.RecoveryObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.RecoveryObligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.RecoveryObligation.CreatedTmst = db.GetDateTime(reader, 5);
        entities.RecoveryObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.RecoveryObligationTransaction.Type1 = db.GetString(reader, 7);
        entities.RecoveryObligationTransaction.Amount =
          db.GetDecimal(reader, 8);
        entities.RecoveryObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 9);
        entities.RecoveryObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 10);
        entities.RecoveryObligationTransaction.Populated = true;
        entities.RecoveryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.RecoveryObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.RecoveryObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.RecoveryObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.RecoveryObligationTransaction.CpaSupType);
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.RecoveryObligation.Populated);
    entities.PotentialRecovery.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.RecoveryObligation.PrqId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PotentialRecovery.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PotentialRecovery.Type1 = db.GetString(reader, 1);
        entities.PotentialRecovery.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PotentialRecovery.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PotentialRecovery.Type1);
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePerson G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportTafIndicator.
      /// </summary>
      [JsonPropertyName("gexportTafIndicator")]
      public Common GexportTafIndicator
      {
        get => gexportTafIndicator ??= new();
        set => gexportTafIndicator = value;
      }

      /// <summary>
      /// A value of GexportPotentialRecovery.
      /// </summary>
      [JsonPropertyName("gexportPotentialRecovery")]
      public DisbursementTransaction GexportPotentialRecovery
      {
        get => gexportPotentialRecovery ??= new();
        set => gexportPotentialRecovery = value;
      }

      /// <summary>
      /// A value of GexportActualRecovery.
      /// </summary>
      [JsonPropertyName("gexportActualRecovery")]
      public DisbursementTransaction GexportActualRecovery
      {
        get => gexportActualRecovery ??= new();
        set => gexportActualRecovery = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson g;
      private Common gexportTafIndicator;
      private DisbursementTransaction gexportPotentialRecovery;
      private DisbursementTransaction gexportActualRecovery;
    }

    /// <summary>
    /// A value of Recovery.
    /// </summary>
    [JsonPropertyName("recovery")]
    public ObligationTransaction Recovery
    {
      get => recovery ??= new();
      set => recovery = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private ObligationTransaction recovery;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public Common Stop
    {
      get => stop ??= new();
      set => stop = value;
    }

    /// <summary>
    /// A value of DisbursementSummary.
    /// </summary>
    [JsonPropertyName("disbursementSummary")]
    public DisbursementSummary DisbursementSummary
    {
      get => disbursementSummary ??= new();
      set => disbursementSummary = value;
    }

    /// <summary>
    /// A value of RecoveryObCreateDate.
    /// </summary>
    [JsonPropertyName("recoveryObCreateDate")]
    public DateWorkArea RecoveryObCreateDate
    {
      get => recoveryObCreateDate ??= new();
      set => recoveryObCreateDate = value;
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
    /// A value of PotentialRecoveryTotal.
    /// </summary>
    [JsonPropertyName("potentialRecoveryTotal")]
    public DisbursementTransaction PotentialRecoveryTotal
    {
      get => potentialRecoveryTotal ??= new();
      set => potentialRecoveryTotal = value;
    }

    private Common stop;
    private DisbursementSummary disbursementSummary;
    private DateWorkArea recoveryObCreateDate;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementTransaction potentialRecoveryTotal;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbursementSummary.
    /// </summary>
    [JsonPropertyName("disbursementSummary")]
    public DisbursementSummary DisbursementSummary
    {
      get => disbursementSummary ??= new();
      set => disbursementSummary = value;
    }

    /// <summary>
    /// A value of RecoveryObligationTransaction.
    /// </summary>
    [JsonPropertyName("recoveryObligationTransaction")]
    public ObligationTransaction RecoveryObligationTransaction
    {
      get => recoveryObligationTransaction ??= new();
      set => recoveryObligationTransaction = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of PotentialRecovery.
    /// </summary>
    [JsonPropertyName("potentialRecovery")]
    public PaymentRequest PotentialRecovery
    {
      get => potentialRecovery ??= new();
      set => potentialRecovery = value;
    }

    /// <summary>
    /// A value of RecoveryObligationType.
    /// </summary>
    [JsonPropertyName("recoveryObligationType")]
    public ObligationType RecoveryObligationType
    {
      get => recoveryObligationType ??= new();
      set => recoveryObligationType = value;
    }

    /// <summary>
    /// A value of RecoveryObligation.
    /// </summary>
    [JsonPropertyName("recoveryObligation")]
    public Obligation RecoveryObligation
    {
      get => recoveryObligation ??= new();
      set => recoveryObligation = value;
    }

    /// <summary>
    /// A value of RecoveryObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("recoveryObligorCsePersonAccount")]
    public CsePersonAccount RecoveryObligorCsePersonAccount
    {
      get => recoveryObligorCsePersonAccount ??= new();
      set => recoveryObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of RecoveryObligorCsePerson.
    /// </summary>
    [JsonPropertyName("recoveryObligorCsePerson")]
    public CsePerson RecoveryObligorCsePerson
    {
      get => recoveryObligorCsePerson ??= new();
      set => recoveryObligorCsePerson = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementSummary disbursementSummary;
    private ObligationTransaction recoveryObligationTransaction;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private DisbursementTransaction debit;
    private PaymentRequest potentialRecovery;
    private ObligationType recoveryObligationType;
    private Obligation recoveryObligation;
    private CsePersonAccount recoveryObligorCsePersonAccount;
    private CsePerson recoveryObligorCsePerson;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
  }
#endregion
}
