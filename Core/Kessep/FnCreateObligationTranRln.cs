// Program: FN_CREATE_OBLIGATION_TRAN_RLN, ID: 372086316, model: 746.
// Short name: SWE00378
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_OBLIGATION_TRAN_RLN.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates the entity Obligation Transaction Rln, for the 
/// reason of CONCURRENT.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateObligationTranRln: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OBLIGATION_TRAN_RLN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateObligationTranRln(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateObligationTranRln.
  /// </summary>
  public FnCreateObligationTranRln(IContext context, Import import,
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
    // ================================================
    // 10/13/98 - Bud Adams  -  deleted Use fn-hardcode-debt-distribution
    // and imported the 2 attributes and Current_Timestamp
    // 5/18/99 - bud adams  -  6 Reads had the exception logic
    //   removed and had Notes saying that an error would cause
    //   an abort.  Restored the "when not found" along with exit
    //   states
    // 11/12/99 - b adams  -  Removed Read actions of persistent
    //   views and all references to them.  The needed data is
    //   imported; no need to have the 'intent' locks that come along
    //   with persistent views.
    // ================================================
    if (!ReadObligationTransaction1())
    {
      ExitState = "FN0000_OBLIG_TRANS_NF_RB";

      return;
    }

    if (!ReadObligationTransaction2())
    {
      ExitState = "FN0000_CONCRNT_OBLIG_TRANS_NF_RB";

      return;
    }

    // Read the obligation transaction rln rsn to be associated
    // to the new obligation transaction rln.
    if (ReadObligationTransactionRlnRsn())
    {
      // ** OK **
    }
    else
    {
      ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF_RB";

      return;
    }

    // Create Obligation Txn Rln.  Use a loop in case duplicate identifier is 
    // generated
    for(local.CreateCounter.Count = 1; local.CreateCounter.Count <= 5; ++
      local.CreateCounter.Count)
    {
      try
      {
        CreateObligationTransactionRln();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.CreateCounter.Count < 5)
            {
            }
            else
            {
              // This condition has been encountered 5 times - abort
              ExitState = "FN0000_OBLIG_TRANS_RLN_AE_RB";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIG_TRANS_RLN_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    System.Diagnostics.Debug.Assert(import.Pconcurrent.Populated);

    var onrGeneratedId =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    var otrType = import.Pconcurrent.Type1;
    var otrGeneratedId = import.Pconcurrent.SystemGeneratedIdentifier;
    var cpaType = import.Pconcurrent.CpaType;
    var cspNumber = import.Pconcurrent.CspNumber;
    var obgGeneratedId = import.Pconcurrent.ObgGeneratedId;
    var otrPType = import.P.Type1;
    var otrPGeneratedId = import.P.SystemGeneratedIdentifier;
    var cpaPType = import.P.CpaType;
    var cspPNumber = import.P.CspNumber;
    var obgPGeneratedId = import.P.ObgGeneratedId;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otyTypePrimary = import.P.OtyType;
    var otyTypeSecondary = import.Pconcurrent.OtyType;

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.ObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", "");
      });

    entities.ObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.ObligationTransactionRln.OtrType = otrType;
    entities.ObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.ObligationTransactionRln.CpaType = cpaType;
    entities.ObligationTransactionRln.CspNumber = cspNumber;
    entities.ObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransactionRln.OtrPType = otrPType;
    entities.ObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.ObligationTransactionRln.CpaPType = cpaPType;
    entities.ObligationTransactionRln.CspPNumber = cspPNumber;
    entities.ObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.ObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransactionRln.CreatedBy = createdBy;
    entities.ObligationTransactionRln.CreatedTmst = createdTmst;
    entities.ObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.ObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.ObligationTransactionRln.Description = "";
    entities.ObligationTransactionRln.Populated = true;
  }

  private bool ReadObligationTransaction1()
  {
    import.P.Populated = false;

    return Read("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        import.P.ObgGeneratedId = db.GetInt32(reader, 0);
        import.P.CspNumber = db.GetString(reader, 1);
        import.P.CpaType = db.GetString(reader, 2);
        import.P.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        import.P.Type1 = db.GetString(reader, 4);
        import.P.Amount = db.GetDecimal(reader, 5);
        import.P.CreatedBy = db.GetString(reader, 6);
        import.P.CreatedTmst = db.GetDateTime(reader, 7);
        import.P.DebtType = db.GetString(reader, 8);
        import.P.CspSupNumber = db.GetNullableString(reader, 9);
        import.P.CpaSupType = db.GetNullableString(reader, 10);
        import.P.OtyType = db.GetInt32(reader, 11);
        import.P.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", import.P.CpaType);
        CheckValid<ObligationTransaction>("Type1", import.P.Type1);
        CheckValid<ObligationTransaction>("DebtType", import.P.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", import.P.CpaSupType);
      });
  }

  private bool ReadObligationTransaction2()
  {
    import.Pconcurrent.Populated = false;

    return Read("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.ConcurrentObligationTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.ConcurrentCsePerson.Number);
      },
      (db, reader) =>
      {
        import.Pconcurrent.ObgGeneratedId = db.GetInt32(reader, 0);
        import.Pconcurrent.CspNumber = db.GetString(reader, 1);
        import.Pconcurrent.CpaType = db.GetString(reader, 2);
        import.Pconcurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        import.Pconcurrent.Type1 = db.GetString(reader, 4);
        import.Pconcurrent.Amount = db.GetDecimal(reader, 5);
        import.Pconcurrent.CreatedBy = db.GetString(reader, 6);
        import.Pconcurrent.CreatedTmst = db.GetDateTime(reader, 7);
        import.Pconcurrent.DebtType = db.GetString(reader, 8);
        import.Pconcurrent.CspSupNumber = db.GetNullableString(reader, 9);
        import.Pconcurrent.CpaSupType = db.GetNullableString(reader, 10);
        import.Pconcurrent.OtyType = db.GetInt32(reader, 11);
        import.Pconcurrent.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", import.Pconcurrent.CpaType);
          
        CheckValid<ObligationTransaction>("Type1", import.Pconcurrent.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          import.Pconcurrent.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          import.Pconcurrent.CpaSupType);
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.OtrrConcurrentObligatio.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of OtrrConcurrentObligatio.
    /// </summary>
    [JsonPropertyName("otrrConcurrentObligatio")]
    public ObligationTransactionRlnRsn OtrrConcurrentObligatio
    {
      get => otrrConcurrentObligatio ??= new();
      set => otrrConcurrentObligatio = value;
    }

    /// <summary>
    /// A value of CpaObligor.
    /// </summary>
    [JsonPropertyName("cpaObligor")]
    public CsePersonAccount CpaObligor
    {
      get => cpaObligor ??= new();
      set => cpaObligor = value;
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
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of ConcurrentObligationTransaction.
    /// </summary>
    [JsonPropertyName("concurrentObligationTransaction")]
    public ObligationTransaction ConcurrentObligationTransaction
    {
      get => concurrentObligationTransaction ??= new();
      set => concurrentObligationTransaction = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public ObligationTransaction P
    {
      get => p ??= new();
      set => p = value;
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
    /// A value of Pconcurrent.
    /// </summary>
    [JsonPropertyName("pconcurrent")]
    public ObligationTransaction Pconcurrent
    {
      get => pconcurrent ??= new();
      set => pconcurrent = value;
    }

    private DateWorkArea current;
    private ObligationTransactionRlnRsn otrrConcurrentObligatio;
    private CsePersonAccount cpaObligor;
    private CsePerson csePerson;
    private CsePerson concurrentCsePerson;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private ObligationTransaction concurrentObligationTransaction;
    private ObligationTransaction p;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction pconcurrent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CreateCounter.
    /// </summary>
    [JsonPropertyName("createCounter")]
    public Common CreateCounter
    {
      get => createCounter ??= new();
      set => createCounter = value;
    }

    private Common createCounter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
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
    /// A value of ConcurrentObligor.
    /// </summary>
    [JsonPropertyName("concurrentObligor")]
    public CsePersonAccount ConcurrentObligor
    {
      get => concurrentObligor ??= new();
      set => concurrentObligor = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private Obligation concurrentObligation;
    private Obligation obligation;
    private CsePerson concurrentCsePerson;
    private CsePerson csePerson;
    private CsePersonAccount concurrentObligor;
    private CsePersonAccount obligor;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransactionRln obligationTransactionRln;
  }
#endregion
}
