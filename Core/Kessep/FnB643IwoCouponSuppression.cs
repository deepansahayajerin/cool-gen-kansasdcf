// Program: FN_B643_IWO_COUPON_SUPPRESSION, ID: 372870500, model: 746.
// Short name: SWE02477
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_IWO_COUPON_SUPPRESSION.
/// </summary>
[Serializable]
public partial class FnB643IwoCouponSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_IWO_COUPON_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643IwoCouponSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643IwoCouponSuppression.
  /// </summary>
  public FnB643IwoCouponSuppression(IContext context, Import import,
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
    export.CpnsSuppressedByDebt.Count = import.CpnsSuppressedByDebt.Count;

    if (ReadLegalActionIncomeSource())
    {
      export.SuppressCouponsByDebt.Flag = "Y";
      ++export.CpnsSuppressedByDebt.Count;
    }
  }

  private bool ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
        db.SetDate(
          command, "effectiveDt", import.EndCoupon.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 5);
        entities.LegalActionIncomeSource.Populated = true;
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
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public CollectionType Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of StmtBegin.
    /// </summary>
    [JsonPropertyName("stmtBegin")]
    public DateWorkArea StmtBegin
    {
      get => stmtBegin ??= new();
      set => stmtBegin = value;
    }

    /// <summary>
    /// A value of StmtEnd.
    /// </summary>
    [JsonPropertyName("stmtEnd")]
    public DateWorkArea StmtEnd
    {
      get => stmtEnd ??= new();
      set => stmtEnd = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of EndCoupon.
    /// </summary>
    [JsonPropertyName("endCoupon")]
    public DateWorkArea EndCoupon
    {
      get => endCoupon ??= new();
      set => endCoupon = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
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

    private CollectionType iwo;
    private DateWorkArea stmtBegin;
    private DateWorkArea stmtEnd;
    private Common cpnsSuppressedByDebt;
    private DateWorkArea endCoupon;
    private CsePerson client;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SuppressCouponsByDebt.
    /// </summary>
    [JsonPropertyName("suppressCouponsByDebt")]
    public Common SuppressCouponsByDebt
    {
      get => suppressCouponsByDebt ??= new();
      set => suppressCouponsByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    private Common suppressCouponsByDebt;
    private Common cpnsSuppressedByDebt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IwoCollections.
    /// </summary>
    [JsonPropertyName("iwoCollections")]
    public Common IwoCollections
    {
      get => iwoCollections ??= new();
      set => iwoCollections = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private Common iwoCollections;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    private IncomeSource incomeSource;
    private LegalActionIncomeSource legalActionIncomeSource;
  }
#endregion
}
