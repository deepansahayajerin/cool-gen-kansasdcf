// Program: EAB_ACCESS_DOA_EFT_RETURN_FILE, ID: 372401737, model: 746.
// Short name: SWEXFE95
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_ACCESS_DOA_EFT_RETURN_FILE.
/// </summary>
[Serializable]
public partial class EabAccessDoaEftReturnFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ACCESS_DOA_EFT_RETURN_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabAccessDoaEftReturnFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabAccessDoaEftReturnFile.
  /// </summary>
  public EabAccessDoaEftReturnFile(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXFE95", context, import, export, EabOptions.Hpvp);
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
    /// <summary>A DoaEftReturnRecordGroup group.</summary>
    [Serializable]
    public class DoaEftReturnRecordGroup
    {
      /// <summary>
      /// A value of Totals.
      /// </summary>
      [JsonPropertyName("totals")]
      [Member(Index = 1, AccessFields = false, Members
        = new[] { "Count", "TotalCurrency" })]
      public Common Totals
      {
        get => totals ??= new();
        set => totals = value;
      }

      /// <summary>
      /// A value of SettlementDate.
      /// </summary>
      [JsonPropertyName("settlementDate")]
      [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
      public DateWorkArea SettlementDate
      {
        get => settlementDate ??= new();
        set => settlementDate = value;
      }

      private Common totals;
      private DateWorkArea settlementDate;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// Gets a value of DoaEftReturnRecord.
    /// </summary>
    [JsonPropertyName("doaEftReturnRecord")]
    [Member(Index = 3)]
    public DoaEftReturnRecordGroup DoaEftReturnRecord
    {
      get => doaEftReturnRecord ?? (doaEftReturnRecord = new());
      set => doaEftReturnRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend error;
    private DoaEftReturnRecordGroup doaEftReturnRecord;
  }
#endregion
}
