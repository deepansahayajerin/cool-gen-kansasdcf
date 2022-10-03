// Program: FN_B651_PRINT_DISBURSEMENT, ID: 373526636, model: 746.
// Short name: SWE02604
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_PRINT_DISBURSEMENT.
/// </summary>
[Serializable]
public partial class FnB651PrintDisbursement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_PRINT_DISBURSEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651PrintDisbursement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651PrintDisbursement.
  /// </summary>
  public FnB651PrintDisbursement(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ReadObligationType();
    ReadCollectionType();
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (import.DisbursementTransaction.Amount < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + "  Disb Id " + NumberToString
      (import.DisbursementTransaction.SystemGeneratedIdentifier, 7, 9) + "  Amt" +
      local.Sign.Text1 + NumberToString
      ((long)(import.DisbursementTransaction.Amount * 100), 9, 7) + "  " + (
        import.DisbursementTransaction.ReferenceNumber ?? "") + "  Excess URA Ind " +
      (import.DisbursementTransaction.ExcessUraInd ?? "");
    UseCabControlReport();
    local.EabReportSend.RptDetail = "AP " + import.Ap.Number + "  ObID " + NumberToString
      (import.Obligation.SystemGeneratedIdentifier, 13, 3) + "  " + entities
      .ObligationType.Code + "  TrnID " + NumberToString
      (import.ObligationTransaction.SystemGeneratedIdentifier, 7, 9) + "  DDDD " +
      NumberToString(DateToInt(import.DebtDetail.DueDt), 8, 8) + "  Su " + import
      .Supported.Number;
    UseCabControlReport();
    local.EabReportSend.RptDetail = "Coll ID " + NumberToString
      (import.Collection.SystemGeneratedIdentifier, 7, 9) + "  Pgm appld to " +
      import.Collection.ProgramAppliedTo + " Dist pgm st appld to " + (
        import.Collection.DistPgmStateAppldTo ?? "") + "  Appld to cd " + import
      .Collection.AppliedToCode;
    UseCabControlReport();
    local.EabReportSend.RptDetail = "Future " + import
      .Collection.AppliedToFuture + "  Crcpt " + NumberToString
      (import.CashReceipt.SequentialNumber, 7, 9) + "  Coll Type " + Substring
      (entities.CollectionType.Code, CollectionType.Code_MaxLength, 1, 1) + "    Cash/Non " +
      entities.CollectionType.CashNonCashInd + "  Coll Dt " + NumberToString
      (DateToInt(import.DisbursementTransaction.CollectionDate), 8, 8);
    UseCabControlReport();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          import.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private CsePerson ar;
    private CsePerson ap;
    private CsePerson supported;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private ObligationType obligationType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private DebtDetail debtDetail;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public WorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private WorkArea sign;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private ObligationType obligationType;
    private CollectionType collectionType;
  }
#endregion
}
