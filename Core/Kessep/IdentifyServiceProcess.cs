// Program: IDENTIFY_SERVICE_PROCESS, ID: 372015477, model: 746.
// Short name: SWE00712
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: IDENTIFY_SERVICE_PROCESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates SERVICE PROCESS and associates it to LEGAL ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class IdentifyServiceProcess: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the IDENTIFY_SERVICE_PROCESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new IdentifyServiceProcess(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of IdentifyServiceProcess.
  /// </summary>
  public IdentifyServiceProcess(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // --------------------------------------------
    if (ReadLegalAction())
    {
      if (ReadServiceProcess())
      {
        local.Last.Identifier = entities.ExistingLast.Identifier;
      }

      try
      {
        CreateServiceProcess();
        export.ServiceProcess.Assign(entities.New1);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SERVICE_PROCESS_AE";

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
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private void CreateServiceProcess()
  {
    var lgaIdentifier = entities.Existing.Identifier;
    var serviceDocumentType = import.ServiceProcess.ServiceDocumentType;
    var serviceRequestDate = import.ServiceProcess.ServiceRequestDate;
    var methodOfService = import.ServiceProcess.MethodOfService;
    var serviceDate = import.ServiceProcess.ServiceDate;
    var returnDate = import.ServiceProcess.ReturnDate;
    var serverName = import.ServiceProcess.ServerName;
    var requestedServee = import.ServiceProcess.RequestedServee;
    var servee = import.ServiceProcess.Servee;
    var serveeRelationship = import.ServiceProcess.ServeeRelationship ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var identifier = local.Last.Identifier + 1;
    var serviceLocation = import.ServiceProcess.ServiceLocation;
    var serviceResult = import.ServiceProcess.ServiceResult ?? "";

    entities.New1.Populated = false;
    Update("CreateServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetString(command, "serviceDocType", serviceDocumentType);
        db.SetDate(command, "serviceRequestDt", serviceRequestDate);
        db.SetString(command, "methodOfService", methodOfService);
        db.SetDate(command, "serviceDate", serviceDate);
        db.SetNullableDate(command, "returnDate", returnDate);
        db.SetString(command, "serverName", serverName);
        db.SetString(command, "requestedServee", requestedServee);
        db.SetString(command, "servee", servee);
        db.SetNullableString(command, "serveeRelationshp", serveeRelationship);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "serviceLocation", serviceLocation);
        db.SetNullableString(command, "serviceResult", serviceResult);
      });

    entities.New1.LgaIdentifier = lgaIdentifier;
    entities.New1.ServiceDocumentType = serviceDocumentType;
    entities.New1.ServiceRequestDate = serviceRequestDate;
    entities.New1.MethodOfService = methodOfService;
    entities.New1.ServiceDate = serviceDate;
    entities.New1.ReturnDate = returnDate;
    entities.New1.ServerName = serverName;
    entities.New1.RequestedServee = requestedServee;
    entities.New1.Servee = servee;
    entities.New1.ServeeRelationship = serveeRelationship;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.Identifier = identifier;
    entities.New1.ServiceLocation = serviceLocation;
    entities.New1.ServiceResult = serviceResult;
    entities.New1.Populated = true;
  }

  private bool ReadLegalAction()
  {
    entities.Existing.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.Identifier = db.GetInt32(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.Existing.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLast.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public ServiceProcess Last
    {
      get => last ??= new();
      set => last = value;
    }

    private ServiceProcess last;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public ServiceProcess ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ServiceProcess New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public LegalAction Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private ServiceProcess existingLast;
    private ServiceProcess new1;
    private LegalAction existing;
  }
#endregion
}
