// Program: SC_CAB_CHECK_DIST_APPS_SECURITY, ID: 374546029, model: 746.
// Short name: SWE00573
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_CHECK_DIST_APPS_SECURITY.
/// </para>
/// <para>
/// Use the USERID from the import view be the basis of security authorization.
/// Create a record in the user session log table if the user violates the
/// security.
/// </para>
/// </summary>
[Serializable]
public partial class ScCabCheckDistAppsSecurity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_CHECK_DIST_APPS_SECURITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabCheckDistAppsSecurity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabCheckDistAppsSecurity.
  /// </summary>
  public ScCabCheckDistAppsSecurity(IContext context, Import import,
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
    export.SecurityBypassFlg.Flag = "N";
    local.Current.Date = Now().Date;

    foreach(var item in ReadProfile())
    {
      local.Profile.Name = entities.Profile.Name;

      if (Equal(entities.Profile.Name, "DEVELOPERS") || Equal
        (entities.Profile.Name, "OPERATIONS") || Equal
        (entities.Profile.Name, "PROG/OPER") || Equal
        (entities.Profile.Name, "BATCHAPPS") || Equal
        (entities.Profile.Name, "DISTAPPS"))
      {
        export.SecurityBypassFlg.Flag = "Y";

        break;
      }
    }

    if (IsEmpty(local.Profile.Name))
    {
      export.SecurityBypassFlg.Flag = "E";
    }

    if (AsChar(export.SecurityBypassFlg.Flag) == 'N')
    {
      // Log the User in the table for violation
      try
      {
        CreateUserSessionLog1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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
    else if (AsChar(export.SecurityBypassFlg.Flag) == 'E')
    {
      // Log the User as not being in any profile.
      try
      {
        CreateUserSessionLog2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    // Commit the session log entry.
    UseEabCicsCommit();
  }

  private void UseEabCicsCommit()
  {
    var useImport = new EabCicsCommit.Import();
    var useExport = new EabCicsCommit.Export();

    Call(EabCicsCommit.Execute, useImport, useExport);
  }

  private void CreateUserSessionLog1()
  {
    var userId = import.ServiceProvider.UserId;
    var createdTstamp = Now();
    var systemName = "KAECSES-CSE";
    var tranId = import.Transaction.Trancode;
    var profileName = local.Profile.Name;
    var violationMessage = "User id not included in an authorized profile.";

    entities.UserSessionLog.Populated = false;
    Update("CreateUserSessionLog1",
      (db, command) =>
      {
        db.SetString(command, "userId", userId);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "systemName", systemName);
        db.SetString(command, "tranId", tranId);
        db.SetNullableString(command, "profileName", profileName);
        db.SetNullableString(command, "violationMessage", violationMessage);
      });

    entities.UserSessionLog.UserId = userId;
    entities.UserSessionLog.CreatedTstamp = createdTstamp;
    entities.UserSessionLog.SystemName = systemName;
    entities.UserSessionLog.TranId = tranId;
    entities.UserSessionLog.ProfileName = profileName;
    entities.UserSessionLog.ViolationMessage = violationMessage;
    entities.UserSessionLog.Populated = true;
  }

  private void CreateUserSessionLog2()
  {
    var userId = import.ServiceProvider.UserId;
    var createdTstamp = Now();
    var systemName = "KAECSES-CSE";
    var tranId = import.Transaction.Trancode;
    var profileName = "NONE";
    var violationMessage = "User id is not included in any profile.";

    entities.UserSessionLog.Populated = false;
    Update("CreateUserSessionLog2",
      (db, command) =>
      {
        db.SetString(command, "userId", userId);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "systemName", systemName);
        db.SetString(command, "tranId", tranId);
        db.SetNullableString(command, "profileName", profileName);
        db.SetNullableString(command, "violationMessage", violationMessage);
      });

    entities.UserSessionLog.UserId = userId;
    entities.UserSessionLog.CreatedTstamp = createdTstamp;
    entities.UserSessionLog.SystemName = systemName;
    entities.UserSessionLog.TranId = tranId;
    entities.UserSessionLog.ProfileName = profileName;
    entities.UserSessionLog.ViolationMessage = violationMessage;
    entities.UserSessionLog.Populated = true;
  }

  private IEnumerable<bool> ReadProfile()
  {
    entities.Profile.Populated = false;

    return ReadEach("ReadProfile",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.Populated = true;

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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    private ServiceProvider serviceProvider;
    private Transaction transaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SecurityBypassFlg.
    /// </summary>
    [JsonPropertyName("securityBypassFlg")]
    public Common SecurityBypassFlg
    {
      get => securityBypassFlg ??= new();
      set => securityBypassFlg = value;
    }

    private Common securityBypassFlg;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private DateWorkArea current;
    private Profile profile;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of UserSessionLog.
    /// </summary>
    [JsonPropertyName("userSessionLog")]
    public UserSessionLog UserSessionLog
    {
      get => userSessionLog ??= new();
      set => userSessionLog = value;
    }

    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
    private UserSessionLog userSessionLog;
  }
#endregion
}
