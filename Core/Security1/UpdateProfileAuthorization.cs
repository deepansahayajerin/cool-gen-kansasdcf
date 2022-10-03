// Program: UPDATE_PROFILE_AUTHORIZATION, ID: 371455617, model: 746.
// Short name: SWE01499
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: UPDATE_PROFILE_AUTHORIZATION.
/// </summary>
[Serializable]
public partial class UpdateProfileAuthorization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PROFILE_AUTHORIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateProfileAuthorization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateProfileAuthorization.
  /// </summary>
  public UpdateProfileAuthorization(IContext context, Import import,
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

    if (ReadProfileAuthorization())
    {
      try
      {
        UpdateProfileAuthorization1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SC0020_PROFILE_AUTHORIZATION_NU";

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
    else
    {
      ExitState = "SC0020_PROFILE_AUTHORIZATION_NF";
    }
  }

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", import.Profile.Name);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.Populated = true;
      });
  }

  private bool ReadProfileAuthorization()
  {
    System.Diagnostics.Debug.Assert(entities.TransactionCommand.Populated);
    entities.ProfileAuthorization.Populated = false;

    return Read("ReadProfileAuthorization",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.Profile.Name);
        db.SetString(
          command, "fkCmdValue", entities.TransactionCommand.FkCmdValue);
        db.SetString(
          command, "fkTrnScreenid", entities.TransactionCommand.FkTrnScreenid);
        db.SetString(
          command, "fkTrnTrancode", entities.TransactionCommand.FkTrnTrancode);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ProfileAuthorization.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ProfileAuthorization.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.ProfileAuthorization.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.ProfileAuthorization.FkProName = db.GetString(reader, 3);
        entities.ProfileAuthorization.FkTrnTrancode = db.GetString(reader, 4);
        entities.ProfileAuthorization.FkTrnScreenid = db.GetString(reader, 5);
        entities.ProfileAuthorization.FkCmdValue = db.GetString(reader, 6);
        entities.ProfileAuthorization.CaseAuth = db.GetString(reader, 7);
        entities.ProfileAuthorization.LegalActionAuth = db.GetString(reader, 8);
        entities.ProfileAuthorization.Populated = true;
      });
  }

  private bool ReadTransactionCommand()
  {
    entities.TransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkCmdValue", import.Command.Value);
        db.SetString(command, "fkTrnScreenid", import.Transaction.ScreenId);
        db.SetString(command, "fkTrnTrancode", import.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.TransactionCommand.Id = db.GetInt32(reader, 0);
        entities.TransactionCommand.FkTrnScreenid = db.GetString(reader, 1);
        entities.TransactionCommand.FkTrnTrancode = db.GetString(reader, 2);
        entities.TransactionCommand.FkCmdValue = db.GetString(reader, 3);
        entities.TransactionCommand.Populated = true;
      });
  }

  private void UpdateProfileAuthorization1()
  {
    System.Diagnostics.Debug.Assert(entities.ProfileAuthorization.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var caseAuth = import.ProfileAuthorization.CaseAuth;
    var legalActionAuth = import.ProfileAuthorization.LegalActionAuth;

    entities.ProfileAuthorization.Populated = false;
    Update("UpdateProfileAuthorization",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstam", lastUpdatedTstamp);
        db.SetString(command, "caseAuth", caseAuth);
        db.SetString(command, "legalActionAuth", legalActionAuth);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProfileAuthorization.CreatedTimestamp.GetValueOrDefault());
        db.SetString(
          command, "fkProName", entities.ProfileAuthorization.FkProName);
        db.SetString(
          command, "fkTrnTrancode",
          entities.ProfileAuthorization.FkTrnTrancode);
        db.SetString(
          command, "fkTrnScreenid",
          entities.ProfileAuthorization.FkTrnScreenid);
        db.SetString(
          command, "fkCmdValue", entities.ProfileAuthorization.FkCmdValue);
      });

    entities.ProfileAuthorization.LastUpdatedBy = lastUpdatedBy;
    entities.ProfileAuthorization.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.ProfileAuthorization.CaseAuth = caseAuth;
    entities.ProfileAuthorization.LegalActionAuth = legalActionAuth;
    entities.ProfileAuthorization.Populated = true;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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

    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    private Profile profile;
    private Command command;
    private Transaction transaction;
    private ProfileAuthorization profileAuthorization;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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

    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    private Profile profile;
    private TransactionCommand transactionCommand;
    private Command command;
    private Transaction transaction;
    private ProfileAuthorization profileAuthorization;
  }
#endregion
}
