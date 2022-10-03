// Program: FN_UPDATE_EFT_STATUS, ID: 372405213, model: 746.
// Short name: SWE02420
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_EFT_STATUS.
/// </summary>
[Serializable]
public partial class FnUpdateEftStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_EFT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateEftStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateEftStatus.
  /// </summary>
  public FnUpdateEftStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // This action block will update an EFT record that is passed in as a 
    // persistent view.  It will update the status of the EFT record to the
    // status that is passed in.
    if (!export.Persistent.Populated)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Unable to update EFT status because view not populated. " + NumberToString
        (export.Persistent.TransmissionIdentifier, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Unable to update EFT status because view not locked. " + NumberToString
        (export.Persistent.TransmissionIdentifier, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    try
    {
      UpdateElectronicFundTransmission();
      ++export.NbrOfUpdates.Count;

      if (Equal(import.Changes.TransmissionStatusCode, "PENDED"))
      {
        ++export.NbrOfEftsPended.Count;
        export.AmtOfEftsPended.TotalCurrency += export.Persistent.
          TransmittalAmount;
      }
      else
      {
        ++export.NbrOfEftsReceipted.Count;
        export.AmtOfEftsReceipted.TotalCurrency += export.Persistent.
          TransmittalAmount;
      }

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Identification number " + NumberToString
          (export.Persistent.TransmissionIdentifier, 15) + " Updated EFT status with a code of " +
          import.Changes.TransmissionStatusCode;
        UseCabControlReport();
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Identification number " + NumberToString
            (export.Persistent.TransmissionIdentifier, 15) + " Critical Error: Not Unique error setting the EFT row to " +
            import.Changes.TransmissionStatusCode;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****
            // Any error dealing with file handling is a "catastrophic error" 
            // and will result in an abend.
            // *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          break;
        case ErrorCode.PermittedValueViolation:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Identification number " + NumberToString
            (export.Persistent.TransmissionIdentifier, 15) + " Critical Error: Permitted Value error setting the EFT row to " +
            import.Changes.TransmissionStatusCode;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****
            // Any error dealing with file handling is a "catastrophic error" 
            // and will result in an abend.
            // *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UpdateElectronicFundTransmission()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var transmissionStatusCode = import.Changes.TransmissionStatusCode;
    var transmissionProcessDate = import.Changes.TransmissionProcessDate;

    export.Persistent.Populated = false;
    Update("UpdateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.
          SetNullableDate(command, "transProcessDate", transmissionProcessDate);
          
        db.SetString(
          command, "transmissionType", export.Persistent.TransmissionType);
        db.SetInt32(
          command, "transmissionId", export.Persistent.TransmissionIdentifier);
      });

    export.Persistent.LastUpdatedBy = lastUpdatedBy;
    export.Persistent.LastUpdatedTimestamp = lastUpdatedTimestamp;
    export.Persistent.TransmissionStatusCode = transmissionStatusCode;
    export.Persistent.TransmissionProcessDate = transmissionProcessDate;
    export.Persistent.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Changes.
    /// </summary>
    [JsonPropertyName("changes")]
    public ElectronicFundTransmission Changes
    {
      get => changes ??= new();
      set => changes = value;
    }

    /// <summary>
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    private ElectronicFundTransmission changes;
    private Common traceIndicator;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public ElectronicFundTransmission Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of NbrOfEftsReceipted.
    /// </summary>
    [JsonPropertyName("nbrOfEftsReceipted")]
    public Common NbrOfEftsReceipted
    {
      get => nbrOfEftsReceipted ??= new();
      set => nbrOfEftsReceipted = value;
    }

    /// <summary>
    /// A value of AmtOfEftsReceipted.
    /// </summary>
    [JsonPropertyName("amtOfEftsReceipted")]
    public Common AmtOfEftsReceipted
    {
      get => amtOfEftsReceipted ??= new();
      set => amtOfEftsReceipted = value;
    }

    /// <summary>
    /// A value of NbrOfEftsPended.
    /// </summary>
    [JsonPropertyName("nbrOfEftsPended")]
    public Common NbrOfEftsPended
    {
      get => nbrOfEftsPended ??= new();
      set => nbrOfEftsPended = value;
    }

    /// <summary>
    /// A value of AmtOfEftsPended.
    /// </summary>
    [JsonPropertyName("amtOfEftsPended")]
    public Common AmtOfEftsPended
    {
      get => amtOfEftsPended ??= new();
      set => amtOfEftsPended = value;
    }

    /// <summary>
    /// A value of NbrOfUpdates.
    /// </summary>
    [JsonPropertyName("nbrOfUpdates")]
    public Common NbrOfUpdates
    {
      get => nbrOfUpdates ??= new();
      set => nbrOfUpdates = value;
    }

    private ElectronicFundTransmission persistent;
    private Common nbrOfEftsReceipted;
    private Common amtOfEftsReceipted;
    private Common nbrOfEftsPended;
    private Common amtOfEftsPended;
    private Common nbrOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
