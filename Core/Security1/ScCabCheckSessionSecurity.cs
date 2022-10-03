// Program: SC_CAB_CHECK_SESSION_SECURITY, ID: 371337706, model: 746.
// Short name: SWE00554
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_CHECK_SESSION_SECURITY.
/// </para>
/// <para>
/// Use the USERID from the import view be the basis of security authorization.
/// Create a record in the user session log table if the user violates the
/// security.
/// </para>
/// </summary>
[Serializable]
public partial class ScCabCheckSessionSecurity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_CHECK_SESSION_SECURITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabCheckSessionSecurity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabCheckSessionSecurity.
  /// </summary>
  public ScCabCheckSessionSecurity(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    // MAINTENANCE LOG:
    // Developed By Arun Mathias on 02/27/2008
    // Description   :  This Action block is called from the CICS program "
    // SWEXGC05"
    //                  and "SWEXGC05" is called from the cool:gen security exit
    // i.e. TIRCSECR
    //                  Tony Pierce 3/2010
    //                  Added "BATCHAPPS" and "DISTAPPS" to the list of profiles
    // for which security is bypassed.
    // 		 Raj 05/10/2018
    // 		 Included "SAHELPDESK" with SRE5 transaction to the bypassed profile 
    // list.
    //                  SRE5 transaciton allows user to maintain DB2 SYNC client
    // information,
    //                  information includes Primary/Secondary Indicator & 
    // preferred ID.
    // ---------------------------------------------------------------------------------------
    export.SecurityBypassFlg.Flag = "N";
    local.Current.Date = Now().Date;

    if (ReadProfile())
    {
      // ************************************************************************************
      // *** Developers, Operations and Prog/Oper profiles are excluded from 
      // being violators.
      // 3/2010 -- Tony Pierce -- Added "BATCHAPPS" and "DISTAPPS" to 
      // qualification.
      // 5/10/2018 -- Raj -- Added Profile "SAHELPDESK" with SRE5 transaction 
      // code to the
      //                     security bypass List.
      // ************************************************************************************
      if (Equal(entities.Profile.Name, "DEVELOPERS") || Equal
        (entities.Profile.Name, "OPERATIONS") || Equal
        (entities.Profile.Name, "PROG/OPER") || Equal
        (entities.Profile.Name, "BATCHAPPS") || Equal
        (entities.Profile.Name, "DISTAPPS") || Equal
        (entities.Profile.Name, "SAHELPDESK") && Equal
        (import.Transaction.Trancode, "SRE5"))
      {
        export.SecurityBypassFlg.Flag = "Y";
      }

      local.Profile.Name = entities.Profile.Name;
    }
    else
    {
      local.Profile.Name = "";
    }

    if (AsChar(export.SecurityBypassFlg.Flag) == 'N')
    {
      // Log the User in the table for violation
      try
      {
        CreateUserSessionLog();
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
  }

  private void CreateUserSessionLog()
  {
    var userId = import.ServiceProvider.UserId;
    var createdTstamp = Now();
    var systemName = "KAECSES-CSE";
    var tranId = import.Transaction.Trancode;
    var profileName = local.Profile.Name;

    entities.UserSessionLog.Populated = false;
    Update("CreateUserSessionLog",
      (db, command) =>
      {
        db.SetString(command, "userId", userId);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "systemName", systemName);
        db.SetString(command, "tranId", tranId);
        db.SetNullableString(command, "profileName", profileName);
        db.SetNullableString(command, "violationMessage", "");
      });

    entities.UserSessionLog.UserId = userId;
    entities.UserSessionLog.CreatedTstamp = createdTstamp;
    entities.UserSessionLog.SystemName = systemName;
    entities.UserSessionLog.TranId = tranId;
    entities.UserSessionLog.ProfileName = profileName;
    entities.UserSessionLog.Populated = true;
  }

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
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
