// Program: CREATE_PROFILE_AUTHORIZATION, ID: 371455300, model: 746.
// Short name: SWE00156
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_PROFILE_AUTHORIZATION.
/// </summary>
[Serializable]
public partial class CreateProfileAuthorization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_PROFILE_AUTHORIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateProfileAuthorization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateProfileAuthorization.
  /// </summary>
  public CreateProfileAuthorization(IContext context, Import import,
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
    if (!ReadProfile())
    {
      ExitState = "SC0015_PROFILE_NF";

      return;
    }

    if (!ReadTransactionCommand())
    {
      ExitState = "SC0018_TRANS_COMMAND_NF";

      return;
    }

    if (AsChar(import.ProfileAuthorization.CaseAuth) == 'Y')
    {
      export.ProfileAuthorization.CaseAuth =
        import.ProfileAuthorization.CaseAuth;
    }
    else
    {
      export.ProfileAuthorization.CaseAuth = "N";
    }

    if (AsChar(import.ProfileAuthorization.LegalActionAuth) == 'Y')
    {
      export.ProfileAuthorization.LegalActionAuth =
        import.ProfileAuthorization.LegalActionAuth;
    }
    else
    {
      export.ProfileAuthorization.LegalActionAuth = "N";
    }

    try
    {
      CreateProfileAuthorization1();
      export.ProfileAuthorization.Assign(entities.ProfileAuthorization);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0020_PROFILE_AUTHORIZATION_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SC0046_PROFILE_AUTHORIZATION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateProfileAuthorization1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingTransactionCommand.Populated);

    var createdTimestamp = Now();
    var activeInd = "Y";
    var createdBy = global.UserId;
    var fkProName = entities.ExistingProfile.Name;
    var fkTrnTrancode = entities.ExistingTransactionCommand.FkTrnTrancode;
    var fkTrnScreenid = entities.ExistingTransactionCommand.FkTrnScreenid;
    var fkCmdValue = entities.ExistingTransactionCommand.FkCmdValue;
    var caseAuth = export.ProfileAuthorization.CaseAuth;
    var legalActionAuth = export.ProfileAuthorization.LegalActionAuth;

    entities.ProfileAuthorization.Populated = false;
    Update("CreateProfileAuthorization",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "activeInd", activeInd);
        db.SetNullableInt32(command, "activeCount", 0);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstam", default(DateTime));
        db.SetString(command, "fkProName", fkProName);
        db.SetString(command, "fkTrnTrancode", fkTrnTrancode);
        db.SetString(command, "fkTrnScreenid", fkTrnScreenid);
        db.SetString(command, "fkCmdValue", fkCmdValue);
        db.SetString(command, "caseAuth", caseAuth);
        db.SetString(command, "legalActionAuth", legalActionAuth);
      });

    entities.ProfileAuthorization.CreatedTimestamp = createdTimestamp;
    entities.ProfileAuthorization.ActiveInd = activeInd;
    entities.ProfileAuthorization.ActiveCount = 0;
    entities.ProfileAuthorization.CreatedBy = createdBy;
    entities.ProfileAuthorization.FkProName = fkProName;
    entities.ProfileAuthorization.FkTrnTrancode = fkTrnTrancode;
    entities.ProfileAuthorization.FkTrnScreenid = fkTrnScreenid;
    entities.ProfileAuthorization.FkCmdValue = fkCmdValue;
    entities.ProfileAuthorization.CaseAuth = caseAuth;
    entities.ProfileAuthorization.LegalActionAuth = legalActionAuth;
    entities.ProfileAuthorization.Populated = true;
  }

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", import.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadTransactionCommand()
  {
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkCmdValue", import.Command.Value);
        db.SetString(command, "fkTrnScreenid", import.Transaction.ScreenId);
        db.SetString(command, "fkTrnTrancode", import.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransactionCommand.Id = db.GetInt32(reader, 0);
        entities.ExistingTransactionCommand.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingTransactionCommand.Populated = true;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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

    private Transaction transaction;
    private Command command;
    private Profile profile;
    private ProfileAuthorization profileAuthorization;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    private ProfileAuthorization profileAuthorization;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

    /// <summary>
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
    }

    /// <summary>
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
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
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
    }

    private Transaction existingTransaction;
    private Command existingCommand;
    private TransactionCommand existingTransactionCommand;
    private ProfileAuthorization profileAuthorization;
    private Profile existingProfile;
  }
#endregion
}
