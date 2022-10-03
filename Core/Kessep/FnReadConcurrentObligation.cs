// Program: FN_READ_CONCURRENT_OBLIGATION, ID: 372810774, model: 746.
// Short name: SWE00053
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_CONCURRENT_OBLIGATION.
/// </summary>
[Serializable]
public partial class FnReadConcurrentObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CONCURRENT_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadConcurrentObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadConcurrentObligation.
  /// </summary>
  public FnReadConcurrentObligation(IContext context, Import import,
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
    if (ReadObligationObligationTransactionCsePerson2())
    {
      export.JointAndSeveralObligation.SystemGeneratedIdentifier =
        entities.JointAndSeveralObligation.SystemGeneratedIdentifier;
      MoveCsePerson(entities.JointAndSeveralCsePerson,
        export.JointAndSeveralCsePerson);
      MoveObligationTransaction(entities.JointAndSeveralObligationTransaction,
        export.JointAndSeveralObligationTransaction);
    }
    else if (ReadObligationObligationTransactionCsePerson1())
    {
      export.JointAndSeveralObligation.SystemGeneratedIdentifier =
        entities.JointAndSeveralObligation.SystemGeneratedIdentifier;
      MoveCsePerson(entities.JointAndSeveralCsePerson,
        export.JointAndSeveralCsePerson);
      MoveObligationTransaction(entities.JointAndSeveralObligationTransaction,
        export.JointAndSeveralObligationTransaction);
    }
    else
    {
      ExitState = "FN0000_JOINT_SEVERAL_DEBT_NF_RB";
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private bool ReadObligationObligationTransactionCsePerson1()
  {
    entities.JointAndSeveralObligation.Populated = false;
    entities.JointAndSeveralObligationTransaction.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;

    return Read("ReadObligationObligationTransactionCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otySecondId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetString(command, "debtTyp", import.HcOtrnDtAccrlInstrctn.DebtType);
        db.
          SetNullableString(command, "cpaSupType", import.HcCpaSupported.Type1);
          
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralObligation.CpaType = db.GetString(reader, 0);
        entities.JointAndSeveralObligation.CspNumber = db.GetString(reader, 1);
        entities.JointAndSeveralObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralObligationTransaction.DebtType =
          db.GetString(reader, 9);
        entities.JointAndSeveralObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.JointAndSeveralObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralObligationTransaction.OtyType =
          db.GetInt32(reader, 12);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 13);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 14);
        entities.JointAndSeveralObligation.Populated = true;
        entities.JointAndSeveralObligationTransaction.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.JointAndSeveralObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
      });
  }

  private bool ReadObligationObligationTransactionCsePerson2()
  {
    entities.JointAndSeveralObligation.Populated = false;
    entities.JointAndSeveralObligationTransaction.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;

    return Read("ReadObligationObligationTransactionCsePerson2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgFGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyFirstId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaFType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspFNumber", import.CsePerson.Number);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetString(command, "debtTyp", import.HcOtrnDtAccrlInstrctn.DebtType);
        db.
          SetNullableString(command, "cpaSupType", import.HcCpaSupported.Type1);
          
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralObligation.CpaType = db.GetString(reader, 0);
        entities.JointAndSeveralObligation.CspNumber = db.GetString(reader, 1);
        entities.JointAndSeveralObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralObligationTransaction.DebtType =
          db.GetString(reader, 9);
        entities.JointAndSeveralObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.JointAndSeveralObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralObligationTransaction.OtyType =
          db.GetInt32(reader, 12);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 13);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 14);
        entities.JointAndSeveralObligation.Populated = true;
        entities.JointAndSeveralObligationTransaction.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.JointAndSeveralObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
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
    /// A value of HcOtrnDtAccrlInstrctn.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrlInstrctn")]
    public ObligationTransaction HcOtrnDtAccrlInstrctn
    {
      get => hcOtrnDtAccrlInstrctn ??= new();
      set => hcOtrnDtAccrlInstrctn = value;
    }

    /// <summary>
    /// A value of HcCpaSupported.
    /// </summary>
    [JsonPropertyName("hcCpaSupported")]
    public CsePersonAccount HcCpaSupported
    {
      get => hcCpaSupported ??= new();
      set => hcCpaSupported = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public DebtDetail Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private ObligationTransaction hcOtrnDtAccrlInstrctn;
    private CsePersonAccount hcCpaSupported;
    private CsePerson supported;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransaction hcOtrnTDebt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private DebtDetail zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of JointAndSeveralObligationTransaction.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligationTransaction")]
    public ObligationTransaction JointAndSeveralObligationTransaction
    {
      get => jointAndSeveralObligationTransaction ??= new();
      set => jointAndSeveralObligationTransaction = value;
    }

    /// <summary>
    /// A value of JointAndSeveralCsePerson.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePerson")]
    public CsePerson JointAndSeveralCsePerson
    {
      get => jointAndSeveralCsePerson ??= new();
      set => jointAndSeveralCsePerson = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligation.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligation")]
    public Obligation JointAndSeveralObligation
    {
      get => jointAndSeveralObligation ??= new();
      set => jointAndSeveralObligation = value;
    }

    private ObligationTransaction jointAndSeveralObligationTransaction;
    private CsePerson jointAndSeveralCsePerson;
    private Obligation jointAndSeveralObligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligation.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligation")]
    public Obligation JointAndSeveralObligation
    {
      get => jointAndSeveralObligation ??= new();
      set => jointAndSeveralObligation = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligationTransaction.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligationTransaction")]
    public ObligationTransaction JointAndSeveralObligationTransaction
    {
      get => jointAndSeveralObligationTransaction ??= new();
      set => jointAndSeveralObligationTransaction = value;
    }

    /// <summary>
    /// A value of JointAndSeveralCsePerson.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePerson")]
    public CsePerson JointAndSeveralCsePerson
    {
      get => jointAndSeveralCsePerson ??= new();
      set => jointAndSeveralCsePerson = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of JointAndSeveralCsePersonAccount.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePersonAccount")]
    public CsePersonAccount JointAndSeveralCsePersonAccount
    {
      get => jointAndSeveralCsePersonAccount ??= new();
      set => jointAndSeveralCsePersonAccount = value;
    }

    private CsePersonAccount supportedCsePersonAccount;
    private CsePerson supportedCsePerson;
    private Obligation jointAndSeveralObligation;
    private ObligationTransaction jointAndSeveralObligationTransaction;
    private CsePerson jointAndSeveralCsePerson;
    private ObligationRln obligationRln;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CsePersonAccount jointAndSeveralCsePersonAccount;
  }
#endregion
}
