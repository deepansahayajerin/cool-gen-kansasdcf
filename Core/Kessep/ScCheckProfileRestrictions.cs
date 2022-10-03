// Program: SC_CHECK_PROFILE_RESTRICTIONS, ID: 945106699, model: 746.
// Short name: SWE01663
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SC_CHECK_PROFILE_RESTRICTIONS.
/// </summary>
[Serializable]
public partial class ScCheckProfileRestrictions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CHECK_PROFILE_RESTRICTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCheckProfileRestrictions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCheckProfileRestrictions.
  /// </summary>
  public ScCheckProfileRestrictions(IContext context, Import import,
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
    // ---------------------------  C H A
    // N G E   L O G
    // ----------------------------------
    // Date	  Programmer   Effort	Description
    // --------  ----------   -------  
    // ----------------------------------------------------
    // 09/13/12  GVandy       CQ35548  Initial Development.  Implement FTIE and 
    // FTIR security
    // 				profile restrictions.
    // ------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Security.Command = global.Command;
    local.Transaction.Trancode = global.TranCode;

    if (!ReadTransaction())
    {
      return;
    }

    if (ReadProfile())
    {
      export.Profile.Assign(entities.Profile);
    }
  }

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
        db.SetString(command, "fkCmdValue", local.Security.Command);
        db.SetString(command, "fkTrnTrancode", entities.Transaction.Trancode);
        db.SetString(command, "fkTrnScreenid", entities.Transaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.RestrictionCode1 = db.GetNullableString(reader, 1);
        entities.Profile.RestrictionCode2 = db.GetNullableString(reader, 2);
        entities.Profile.RestrictionCode3 = db.GetNullableString(reader, 3);
        entities.Profile.Populated = true;
      });
  }

  private bool ReadTransaction()
  {
    entities.Transaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "trancode", local.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.Transaction.ScreenId = db.GetString(reader, 0);
        entities.Transaction.Trancode = db.GetString(reader, 1);
        entities.Transaction.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private Profile profile;
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
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

    private DateWorkArea current;
    private Security2 security;
    private Transaction transaction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

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
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    /// <summary>
    /// A value of TransactionCommand.
    /// </summary>
    [JsonPropertyName("transactionCommand")]
    public TransactionCommand TransactionCommand
    {
      get => transactionCommand ??= new();
      set => transactionCommand = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
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

    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private ProfileAuthorization profileAuthorization;
    private TransactionCommand transactionCommand;
    private Command command;
    private Transaction transaction;
  }
#endregion
}
