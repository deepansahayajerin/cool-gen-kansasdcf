// Program: EAB_ACCESS_OUTBOUND_EFT_FILE, ID: 372400443, model: 746.
// Short name: SWEXFE96
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_ACCESS_OUTBOUND_EFT_FILE.
/// </summary>
[Serializable]
public partial class EabAccessOutboundEftFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ACCESS_OUTBOUND_EFT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabAccessOutboundEftFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabAccessOutboundEftFile.
  /// </summary>
  public EabAccessOutboundEftFile(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE96", context, import, export, 0);
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

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp",
      "PayDate",
      "TransmittalAmount",
      "ApSsn",
      "MedicalSupportId",
      "ApName",
      "FipsCode",
      "EmploymentTerminationId",
      "SequenceNumber",
      "ReceivingDfiIdentification",
      "DfiAccountNumber",
      "TransactionCode",
      "SettlementDate",
      "CaseId",
      "TransmissionStatusCode",
      "CompanyName",
      "OriginatingDfiIdentification",
      "ReceivingEntityName",
      "TransmissionType",
      "TransmissionIdentifier",
      "TransmissionProcessDate",
      "FileCreationDate",
      "FileCreationTime",
      "CompanyIdentificationIcd",
      "CompanyIdentificationNumber",
      "CompanyDescriptiveDate",
      "EffectiveEntryDate",
      "ReceivingCompanyName",
      "TraceNumber",
      "ApplicationIdentifier",
      "CollectionAmount",
      "VendorNumber",
      "CheckDigit",
      "ReceivingDfiAccountNumber",
      "CompanyEntryDescription"
    })]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private EabFileHandling eabFileHandling;
    private ElectronicFundTransmission electronicFundTransmission;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend error;
  }
#endregion
}
