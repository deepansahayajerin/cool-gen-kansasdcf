// Program: FN_CHECK_OBLIGATION_FOR_ACTIVITY, ID: 372086106, model: 746.
// Short name: SWE00321
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
/// A program: FN_CHECK_OBLIGATION_FOR_ACTIVITY.
/// </para>
/// <para>
/// This action block checks an Accruing Obligation for activity.  A flag is set
/// if:
///    - The Obligation create date is not equal to current date
///    - There are Obligation Transactions related to the
///      Obligation that are not Accruing Instructions
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckObligationForActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_OBLIGATION_FOR_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckObligationForActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckObligationForActivity.
  /// </summary>
  public FnCheckObligationForActivity(IContext context, Import import,
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
    // *** 09/02/98  Bud Adams     Moved IF record selection logic into Read 
    // Each
    // 			    actions   ***
    // ***			   Deleted fn-hardcode-debt-distribution;
    // ***			   imported values  ***
    // =================================================
    // 2/9/1999 - B Adams  -  Read properties set
    // =================================================
    export.ActiveObligation.Flag = "N";

    if (!import.P.Populated)
    {
      if (!ReadObligation())
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }
    }

    // : Check for transaction activity
    foreach(var item in ReadObligationTransaction())
    {
      if (!Equal(global.Command, "DELETE"))
      {
        // : Read Obligation Transaction Rln, for a primary relationship to 
        // Obligation Transaction.  If one is found and it is not for the reason
        // of CONCURRENT, the obligation has activity.
        if (ReadObligationTransactionRlnObligationTransactionRlnRsn1())
        {
          export.ActiveObligation.Flag = "Y";

          return;
        }

        // : Repeat the above read, checking the secondary relationship
        if (ReadObligationTransactionRlnObligationTransactionRlnRsn2())
        {
          export.ActiveObligation.Flag = "Y";

          return;

          // *** Removed IF construct and put selection logic in "Where"
        }
      }

      // : Check for collection activity
      // This relationship is one Ob_Tran to many Collections.  So, this is 
      // strictly an existence check.  We don't care how many rows qualify or
      // about the first row that's returned; and a 'when not found' is OK.  Bud
      // Adams - 3/17/99  Happy St. Patrick's Day!!
      if (ReadCollection())
      {
        export.ActiveObligation.Flag = "Y";

        return;
      }
      else
      {
        // ***  Get next row  ***
      }
    }

    // =================================================
    // 11/4/99 - b adams  -  deleted a Read Each of Cash_Receipt_
    //   Detail via unused relationship between it and Obligation.
    //   The only path used from CRD to Oblig is via Collection, and
    //   Collection existence is already checked above.  So, CRD
    //   existence is unnecessary.
    // =================================================
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadObligation()
  {
    import.P.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        import.P.CpaType = db.GetString(reader, 0);
        import.P.CspNumber = db.GetString(reader, 1);
        import.P.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        import.P.DtyGeneratedId = db.GetInt32(reader, 3);
        import.P.Description = db.GetNullableString(reader, 4);
        import.P.PrimarySecondaryCode = db.GetNullableString(reader, 5);
        import.P.CreatedTmst = db.GetDateTime(reader, 6);
        import.P.Populated = true;
        CheckValid<Obligation>("CpaType", import.P.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          import.P.PrimarySecondaryCode);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.P.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId", import.P.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.P.CspNumber);
        db.SetString(command, "cpaType", import.P.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationTransactionRlnObligationTransactionRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.ObligationTransactionRln.Populated = false;

    return Read("ReadObligationTransactionRlnObligationTransactionRlnRsn1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypePrimary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrPType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspPNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetInt32(
          command, "onrGeneratedId",
          import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);
      });
  }

  private bool ReadObligationTransactionRlnObligationTransactionRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.ObligationTransactionRln.Populated = false;

    return Read("ReadObligationTransactionRlnObligationTransactionRlnRsn2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypeSecondary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetInt32(
          command, "onrGeneratedId",
          import.HcOtrrConcurrentObliga.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
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
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public Obligation P
    {
      get => p ??= new();
      set => p = value;
    }

    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private CsePersonAccount hcCpaObligor;
    private CsePerson obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private Obligation p;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveObligation.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    public Common ActiveObligation
    {
      get => activeObligation ??= new();
      set => activeObligation = value;
    }

    private Common activeObligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

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
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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

    private CashReceiptDetail cashReceiptDetail;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private ObligationType obligationType;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
  }
#endregion
}
