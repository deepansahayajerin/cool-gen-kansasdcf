// Program: RECORD_SERVICE_ATTEMPT_RESULT, ID: 372015474, model: 746.
// Short name: SWE01054
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RECORD_SERVICE_ATTEMPT_RESULT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates SERVICE PROCESS.
/// </para>
/// </summary>
[Serializable]
public partial class RecordServiceAttemptResult: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RECORD_SERVICE_ATTEMPT_RESULT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RecordServiceAttemptResult(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RecordServiceAttemptResult.
  /// </summary>
  public RecordServiceAttemptResult(IContext context, Import import,
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
    if (ReadServiceProcess())
    {
      try
      {
        UpdateServiceProcess();
        export.ServiceProcess.Assign(entities.ServiceProcess);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SERVICE_PROCESS_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "SERVICE_PROCESS_NF";
    }
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "identifier", import.ServiceProcess.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceDocumentType = db.GetString(reader, 1);
        entities.ServiceProcess.ServiceRequestDate = db.GetDate(reader, 2);
        entities.ServiceProcess.MethodOfService = db.GetString(reader, 3);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 4);
        entities.ServiceProcess.ReturnDate = db.GetNullableDate(reader, 5);
        entities.ServiceProcess.ServerName = db.GetString(reader, 6);
        entities.ServiceProcess.RequestedServee = db.GetString(reader, 7);
        entities.ServiceProcess.Servee = db.GetString(reader, 8);
        entities.ServiceProcess.ServeeRelationship =
          db.GetNullableString(reader, 9);
        entities.ServiceProcess.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.ServiceProcess.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.ServiceProcess.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 13);
        entities.ServiceProcess.ServiceLocation = db.GetString(reader, 14);
        entities.ServiceProcess.ServiceResult =
          db.GetNullableString(reader, 15);
        entities.ServiceProcess.Populated = true;
      });
  }

  private void UpdateServiceProcess()
  {
    System.Diagnostics.Debug.Assert(entities.ServiceProcess.Populated);

    var serviceDocumentType = import.ServiceProcess.ServiceDocumentType;
    var serviceRequestDate = import.ServiceProcess.ServiceRequestDate;
    var methodOfService = import.ServiceProcess.MethodOfService;
    var serviceDate = import.ServiceProcess.ServiceDate;
    var returnDate = import.ServiceProcess.ReturnDate;
    var serverName = import.ServiceProcess.ServerName;
    var requestedServee = import.ServiceProcess.RequestedServee;
    var servee = import.ServiceProcess.Servee;
    var serveeRelationship = import.ServiceProcess.ServeeRelationship ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var serviceLocation = import.ServiceProcess.ServiceLocation;
    var serviceResult = import.ServiceProcess.ServiceResult ?? "";

    entities.ServiceProcess.Populated = false;
    Update("UpdateServiceProcess",
      (db, command) =>
      {
        db.SetString(command, "serviceDocType", serviceDocumentType);
        db.SetDate(command, "serviceRequestDt", serviceRequestDate);
        db.SetString(command, "methodOfService", methodOfService);
        db.SetDate(command, "serviceDate", serviceDate);
        db.SetNullableDate(command, "returnDate", returnDate);
        db.SetString(command, "serverName", serverName);
        db.SetString(command, "requestedServee", requestedServee);
        db.SetString(command, "servee", servee);
        db.SetNullableString(command, "serveeRelationshp", serveeRelationship);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "serviceLocation", serviceLocation);
        db.SetNullableString(command, "serviceResult", serviceResult);
        db.SetInt32(
          command, "lgaIdentifier", entities.ServiceProcess.LgaIdentifier);
        db.SetInt32(command, "identifier", entities.ServiceProcess.Identifier);
      });

    entities.ServiceProcess.ServiceDocumentType = serviceDocumentType;
    entities.ServiceProcess.ServiceRequestDate = serviceRequestDate;
    entities.ServiceProcess.MethodOfService = methodOfService;
    entities.ServiceProcess.ServiceDate = serviceDate;
    entities.ServiceProcess.ReturnDate = returnDate;
    entities.ServiceProcess.ServerName = serverName;
    entities.ServiceProcess.RequestedServee = requestedServee;
    entities.ServiceProcess.Servee = servee;
    entities.ServiceProcess.ServeeRelationship = serveeRelationship;
    entities.ServiceProcess.LastUpdatedBy = lastUpdatedBy;
    entities.ServiceProcess.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.ServiceProcess.ServiceLocation = serviceLocation;
    entities.ServiceProcess.ServiceResult = serviceResult;
    entities.ServiceProcess.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    private LegalAction legalAction;
    private ServiceProcess serviceProcess;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    private ServiceProcess serviceProcess;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    private LegalAction legalAction;
    private ServiceProcess serviceProcess;
  }
#endregion
}
