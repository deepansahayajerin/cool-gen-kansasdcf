// Program: FN_B643_HARDCODE, ID: 372685126, model: 746.
// Short name: SWE02390
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_HARDCODE.
/// </summary>
[Serializable]
public partial class FnB643Hardcode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_HARDCODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643Hardcode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643Hardcode.
  /// </summary>
  public FnB643Hardcode(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.County.Id = 195;
    export.FcrtRec.SystemGeneratedIdentifier = 2;
    export.FdirPmt.SystemGeneratedIdentifier = 7;

    if (ReadCollectionType())
    {
      export.CollectionType.SequentialIdentifier =
        entities.CollectionType.SequentialIdentifier;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "IWO Collection Type was not found.  Entry in needed in table.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      null,
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FcrtRec.
    /// </summary>
    [JsonPropertyName("fcrtRec")]
    public CashReceiptType FcrtRec
    {
      get => fcrtRec ??= new();
      set => fcrtRec = value;
    }

    /// <summary>
    /// A value of FdirPmt.
    /// </summary>
    [JsonPropertyName("fdirPmt")]
    public CashReceiptType FdirPmt
    {
      get => fdirPmt ??= new();
      set => fdirPmt = value;
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
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public Code County
    {
      get => county ??= new();
      set => county = value;
    }

    private CashReceiptType fcrtRec;
    private CashReceiptType fdirPmt;
    private CollectionType collectionType;
    private Code county;
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

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CollectionType collectionType;
  }
#endregion
}
