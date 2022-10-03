// Program: SI_QUICK_AUDIT, ID: 374537067, model: 746.
// Short name: SWE03125
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_QUICK_AUDIT.
/// </para>
/// <para>
/// Audit
/// </para>
/// </summary>
[Serializable]
public partial class SiQuickAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickAudit.
  /// </summary>
  public SiQuickAudit(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // IMPORTANT
    // !!********************************************
    // *    IF IMPORT OR EXPORT VIEWS ARE MODIFIED THEN THE  *
    // *    DB2 STORED PROCEDURE MUST BE UPDATED TO MATCH!!  *
    // *******************************************************
    // ***
    //    10-12-09       A Hockman            Initial development during cq211 
    // project for Quick
    // ***
    try
    {
      CreateQuickAudit();
      export.QuickAudit.Assign(import.QuickAudit);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "RECORD_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "RECORD_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateQuickAudit()
  {
    var systemUserId = import.QuickAudit.SystemUserId;
    var requestTimestamp = import.QuickAudit.RequestTimestamp;
    var requestorId = import.QuickAudit.RequestorId;
    var requestingCaseId = import.QuickAudit.RequestingCaseId;
    var requestingCaseOtherId = import.QuickAudit.RequestingCaseOtherId;
    var systemServerId = import.QuickAudit.SystemServerId;
    var systemResponseCode = import.QuickAudit.SystemResponseCode;
    var dataResponseCode = import.QuickAudit.DataResponseCode;
    var startDate = import.QuickAudit.StartDate;
    var endDate = import.QuickAudit.EndDate;
    var dataRequestType = import.QuickAudit.DataRequestType;
    var providerCaseState = import.QuickAudit.ProviderCaseState;
    var providerCaseOtherId = import.QuickAudit.ProviderCaseOtherId ?? "";
    var requestingCaseState = import.QuickAudit.RequestingCaseState;
    var stateGeneratedId = import.QuickAudit.StateGeneratedId;
    var systemResponseMessage = import.QuickAudit.SystemResponseMessage;
    var dataResponseMessage = import.QuickAudit.DataResponseMessage;

    entities.QuickAudit.Populated = false;
    Update("CreateQuickAudit",
      (db, command) =>
      {
        db.SetString(command, "systemUserId", systemUserId);
        db.SetDateTime(command, "requestTimestamp", requestTimestamp);
        db.SetString(command, "requestorId", requestorId);
        db.SetString(command, "requestingCaseId", requestingCaseId);
        db.SetString(command, "reqCaseOtherId", requestingCaseOtherId);
        db.SetString(command, "systemServerId", systemServerId);
        db.SetString(command, "sysRespCode", systemResponseCode);
        db.SetString(command, "dataRespCode", dataResponseCode);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "dataRequestType", dataRequestType);
        db.SetString(command, "provCaseState", providerCaseState);
        db.SetNullableString(command, "provCaseOthrId", providerCaseOtherId);
        db.SetString(command, "reqCaseState", requestingCaseState);
        db.SetString(command, "stateGenId", stateGeneratedId);
        db.SetString(command, "sysRespMsg", systemResponseMessage);
        db.SetString(command, "dataRespMsg", dataResponseMessage);
      });

    entities.QuickAudit.SystemUserId = systemUserId;
    entities.QuickAudit.RequestTimestamp = requestTimestamp;
    entities.QuickAudit.RequestorId = requestorId;
    entities.QuickAudit.RequestingCaseId = requestingCaseId;
    entities.QuickAudit.RequestingCaseOtherId = requestingCaseOtherId;
    entities.QuickAudit.SystemServerId = systemServerId;
    entities.QuickAudit.SystemResponseCode = systemResponseCode;
    entities.QuickAudit.DataResponseCode = dataResponseCode;
    entities.QuickAudit.StartDate = startDate;
    entities.QuickAudit.EndDate = endDate;
    entities.QuickAudit.DataRequestType = dataRequestType;
    entities.QuickAudit.ProviderCaseState = providerCaseState;
    entities.QuickAudit.ProviderCaseOtherId = providerCaseOtherId;
    entities.QuickAudit.RequestingCaseState = requestingCaseState;
    entities.QuickAudit.StateGeneratedId = stateGeneratedId;
    entities.QuickAudit.SystemResponseMessage = systemResponseMessage;
    entities.QuickAudit.DataResponseMessage = dataResponseMessage;
    entities.QuickAudit.Populated = true;
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
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }
#endregion
}
