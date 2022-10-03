// Program: EAB_SDSO_INTERFACE_DRVR, ID: 372428128, model: 746.
// Short name: SWEXFE98
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_SDSO_INTERFACE_DRVR.
/// </summary>
[Serializable]
public partial class EabSdsoInterfaceDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_SDSO_INTERFACE_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabSdsoInterfaceDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabSdsoInterfaceDrvr.
  /// </summary>
  public EabSdsoInterfaceDrvr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE98", context, import, export, 0);
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
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "TextLine80",
      "TextReturnCode",
      "NumericReturnCode"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      [Member(Index = 1, Members = new[] { "SourceCreationDate" })]
      public CashReceiptEvent Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      private CashReceiptEvent detail;
    }

    /// <summary>A DetailRecordGroup group.</summary>
    [Serializable]
    public class DetailRecordGroup
    {
      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      [Member(Index = 1, Members = new[] { "SelectChar" })]
      public Common CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      [Member(Index = 2, Members
        = new[] { "ObligorPersonNumber", "CollectionAmount" })]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      private Common collectionType;
      private CashReceiptDetail detail;
    }

    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of DetailTotal.
      /// </summary>
      [JsonPropertyName("detailTotal")]
      [Member(Index = 1, Members
        = new[] { "TotalNonCashTransactionCount", "TotalNonCashAmt" })]
      public CashReceiptEvent DetailTotal
      {
        get => detailTotal ??= new();
        set => detailTotal = value;
      }

      private CashReceiptEvent detailTotal;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "TextLine80",
      "NumericReturnCode",
      "TextReturnCode"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 2, Members = new[] { "Count" })]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    [Member(Index = 3)]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
    }

    /// <summary>
    /// Gets a value of DetailRecord.
    /// </summary>
    [JsonPropertyName("detailRecord")]
    [Member(Index = 4)]
    public DetailRecordGroup DetailRecord
    {
      get => detailRecord ?? (detailRecord = new());
      set => detailRecord = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    [Member(Index = 5)]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    private External external;
    private Common recordType;
    private HeaderRecordGroup headerRecord;
    private DetailRecordGroup detailRecord;
    private TotalRecordGroup totalRecord;
  }
#endregion
}
