// Program: EAB_ACCESS_INBOUND_EFT_FILE, ID: 372402819, model: 746.
// Short name: SWEXFE97
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_ACCESS_INBOUND_EFT_FILE.
/// </summary>
[Serializable]
public partial class EabAccessInboundEftFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ACCESS_INBOUND_EFT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabAccessInboundEftFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabAccessInboundEftFile.
  /// </summary>
  public EabAccessInboundEftFile(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE97", context, import, export, 0);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of EftHeaderRecord.
    /// </summary>
    [JsonPropertyName("eftHeaderRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date", "Time" })]
      
    public DateWorkArea EftHeaderRecord
    {
      get => eftHeaderRecord ??= new();
      set => eftHeaderRecord = value;
    }

    /// <summary>
    /// A value of EftDetailRecord.
    /// </summary>
    [JsonPropertyName("eftDetailRecord")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "PayDate",
      "TransmittalAmount",
      "ApSsn",
      "MedicalSupportId",
      "ApName",
      "FipsCode",
      "EmploymentTerminationId",
      "SequenceNumber",
      "ReceivingDfiAccountNumber",
      "TransactionCode",
      "CaseId",
      "CompanyName",
      "OriginatingDfiIdentification",
      "CompanyIdentificationIcd",
      "CompanyIdentificationNumber",
      "CompanyDescriptiveDate",
      "EffectiveEntryDate",
      "ReceivingCompanyName",
      "TraceNumber",
      "ApplicationIdentifier",
      "CollectionAmount",
      "CompanyEntryDescription"
    })]
    public ElectronicFundTransmission EftDetailRecord
    {
      get => eftDetailRecord ??= new();
      set => eftDetailRecord = value;
    }

    /// <summary>
    /// A value of EftTrailerRecord.
    /// </summary>
    [JsonPropertyName("eftTrailerRecord")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "Count", "TotalCurrency" })]
    public Common EftTrailerRecord
    {
      get => eftTrailerRecord ??= new();
      set => eftTrailerRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend error;
    private DateWorkArea eftHeaderRecord;
    private ElectronicFundTransmission eftDetailRecord;
    private Common eftTrailerRecord;
  }
#endregion
}
