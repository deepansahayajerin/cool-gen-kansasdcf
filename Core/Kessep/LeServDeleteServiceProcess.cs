// Program: LE_SERV_DELETE_SERVICE_PROCESS, ID: 372015473, model: 746.
// Short name: SWE00817
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_SERV_DELETE_SERVICE_PROCESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes Service Process
/// </para>
/// </summary>
[Serializable]
public partial class LeServDeleteServiceProcess: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SERV_DELETE_SERVICE_PROCESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeServDeleteServiceProcess(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeServDeleteServiceProcess.
  /// </summary>
  public LeServDeleteServiceProcess(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date		Developer          Description
    // 11/18/98	R.Jean  	   Removed LEGAL ACTION read
    // -------------------------------------------------------------------
    if (ReadServiceProcess())
    {
      DeleteServiceProcess();
    }
    else
    {
      ExitState = "SERVICE_PROCESS_NF";
    }
  }

  private void DeleteServiceProcess()
  {
    Update("DeleteServiceProcess",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier",
          entities.ExistingServiceProcess.LgaIdentifier);
        db.SetInt32(
          command, "identifier", entities.ExistingServiceProcess.Identifier);
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ExistingServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "identifier", import.ServiceProcess.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingServiceProcess.ServiceDocumentType =
          db.GetString(reader, 1);
        entities.ExistingServiceProcess.ServiceRequestDate =
          db.GetDate(reader, 2);
        entities.ExistingServiceProcess.MethodOfService =
          db.GetString(reader, 3);
        entities.ExistingServiceProcess.ServiceDate = db.GetDate(reader, 4);
        entities.ExistingServiceProcess.ReturnDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingServiceProcess.ServerName = db.GetString(reader, 6);
        entities.ExistingServiceProcess.RequestedServee =
          db.GetString(reader, 7);
        entities.ExistingServiceProcess.Servee = db.GetString(reader, 8);
        entities.ExistingServiceProcess.ServeeRelationship =
          db.GetNullableString(reader, 9);
        entities.ExistingServiceProcess.CreatedBy = db.GetString(reader, 10);
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 11);
        entities.ExistingServiceProcess.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.ExistingServiceProcess.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 14);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 15);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 16);
        entities.ExistingServiceProcess.Populated = true;
      });
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
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProcess.
    /// </summary>
    [JsonPropertyName("existingServiceProcess")]
    public ServiceProcess ExistingServiceProcess
    {
      get => existingServiceProcess ??= new();
      set => existingServiceProcess = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private ServiceProcess existingServiceProcess;
    private LegalAction existingLegalAction;
  }
#endregion
}
