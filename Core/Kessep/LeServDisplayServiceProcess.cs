// Program: LE_SERV_DISPLAY_SERVICE_PROCESS, ID: 372015476, model: 746.
// Short name: SWE00818
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_SERV_DISPLAY_SERVICE_PROCESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads SERVICE_PROCESS and populates export view for 
/// display.
/// </para>
/// </summary>
[Serializable]
public partial class LeServDisplayServiceProcess: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SERV_DISPLAY_SERVICE_PROCESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeServDisplayServiceProcess(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeServDisplayServiceProcess.
  /// </summary>
  public LeServDisplayServiceProcess(IContext context, Import import,
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
    local.ServiceProcessFound.Flag = "N";

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        foreach(var item in ReadServiceProcess5())
        {
          if (!Equal(import.ServiceProcess.ServiceRequestDate,
            local.InitialisedToSpaces.ServiceRequestDate) && !
            Equal(entities.ExistingServiceProcess.ServiceRequestDate,
            import.ServiceProcess.ServiceRequestDate))
          {
            continue;
          }

          export.ServiceProcess.Assign(entities.ExistingServiceProcess);
          local.ServiceProcessFound.Flag = "Y";

          break;
        }

        if (AsChar(local.ServiceProcessFound.Flag) == 'N')
        {
          if (Equal(import.ServiceProcess.ServiceRequestDate,
            local.InitialisedToSpaces.ServiceRequestDate))
          {
            ExitState = "LE0000_NO_SERVICE_PROC_2_DISPLAY";
          }
          else
          {
            ExitState = "SERVICE_PROCESS_NF";
          }

          return;
        }

        break;
      case "PREV":
        if (ReadServiceProcess2())
        {
          export.ServiceProcess.Assign(entities.ExistingServiceProcess);
          local.ServiceProcessFound.Flag = "Y";
        }

        if (AsChar(local.ServiceProcessFound.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        break;
      case "NEXT":
        if (ReadServiceProcess1())
        {
          export.ServiceProcess.Assign(entities.ExistingServiceProcess);
          local.ServiceProcessFound.Flag = "Y";
        }

        if (AsChar(local.ServiceProcessFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_SERVICE_PROC_2_DISPLAY";

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (AsChar(local.ServiceProcessFound.Flag) == 'Y')
    {
      foreach(var item in ReadServiceProcess4())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }

      foreach(var item in ReadServiceProcess3())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }
    }
  }

  private bool ReadServiceProcess1()
  {
    entities.ExistingServiceProcess.Populated = false;

    return Read("ReadServiceProcess1",
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
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 11);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 12);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 13);
        entities.ExistingServiceProcess.Populated = true;
      });
  }

  private bool ReadServiceProcess2()
  {
    entities.ExistingServiceProcess.Populated = false;

    return Read("ReadServiceProcess2",
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
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 11);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 12);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 13);
        entities.ExistingServiceProcess.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProcess3()
  {
    entities.ExistingServiceProcess.Populated = false;

    return ReadEach("ReadServiceProcess3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "identifier", export.ServiceProcess.Identifier);
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
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 11);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 12);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 13);
        entities.ExistingServiceProcess.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProcess4()
  {
    entities.ExistingServiceProcess.Populated = false;

    return ReadEach("ReadServiceProcess4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "identifier", export.ServiceProcess.Identifier);
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
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 11);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 12);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 13);
        entities.ExistingServiceProcess.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProcess5()
  {
    entities.ExistingServiceProcess.Populated = false;

    return ReadEach("ReadServiceProcess5",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
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
        entities.ExistingServiceProcess.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.ExistingServiceProcess.Identifier = db.GetInt32(reader, 11);
        entities.ExistingServiceProcess.ServiceLocation =
          db.GetString(reader, 12);
        entities.ExistingServiceProcess.ServiceResult =
          db.GetNullableString(reader, 13);
        entities.ExistingServiceProcess.Populated = true;

        return true;
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

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
    private Common userAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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

    private ScrollingAttributes scrollingAttributes;
    private ServiceProcess serviceProcess;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public ServiceProcess InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    /// <summary>
    /// A value of ServiceProcessFound.
    /// </summary>
    [JsonPropertyName("serviceProcessFound")]
    public Common ServiceProcessFound
    {
      get => serviceProcessFound ??= new();
      set => serviceProcessFound = value;
    }

    private ServiceProcess initialisedToSpaces;
    private Common serviceProcessFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingServiceProcess.
    /// </summary>
    [JsonPropertyName("existingServiceProcess")]
    public ServiceProcess ExistingServiceProcess
    {
      get => existingServiceProcess ??= new();
      set => existingServiceProcess = value;
    }

    private LegalAction existingLegalAction;
    private ServiceProcess existingServiceProcess;
  }
#endregion
}
