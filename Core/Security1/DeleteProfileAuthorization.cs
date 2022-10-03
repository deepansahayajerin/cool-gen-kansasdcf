// Program: DELETE_PROFILE_AUTHORIZATION, ID: 371452499, model: 746.
// Short name: SWE00193
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_PROFILE_AUTHORIZATION.
/// </summary>
[Serializable]
public partial class DeleteProfileAuthorization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_PROFILE_AUTHORIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteProfileAuthorization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteProfileAuthorization.
  /// </summary>
  public DeleteProfileAuthorization(IContext context, Import import,
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

    foreach(var item in ReadProfileAuthorization())
    {
      if (IsEmpty(import.Command.Value))
      {
        // if no command passed in, this means that the user wants to remove the
        // transaction from the profile, so delete all occurances of profile
        // authorization.
      }
      else
      {
        // if the command is passed in, the user wants to only remove the 
        // command from the profile.
        if (!ReadCommand())
        {
          ExitState = "SC0012_COMMAND_NF";

          return;
        }

        if (Equal(entities.ExistingCommand.Value, import.Command.Value))
        {
        }
        else
        {
          continue;
        }
      }

      DeleteProfileAuthorization1();
    }
  }

  private void DeleteProfileAuthorization1()
  {
    Update("DeleteProfileAuthorization",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingProfileAuthorization.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(
          command, "fkProName",
          entities.ExistingProfileAuthorization.FkProName);
        db.SetString(
          command, "fkTrnTrancode",
          entities.ExistingProfileAuthorization.FkTrnTrancode);
        db.SetString(
          command, "fkTrnScreenid",
          entities.ExistingProfileAuthorization.FkTrnScreenid);
        db.SetString(
          command, "fkCmdValue",
          entities.ExistingProfileAuthorization.FkCmdValue);
      });
  }

  private bool ReadCommand()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingProfileAuthorization.Populated);
    entities.ExistingCommand.Populated = false;

    return Read("ReadCommand",
      (db, command) =>
      {
        db.SetString(
          command, "cmdValue",
          entities.ExistingProfileAuthorization.FkCmdValue);
      },
      (db, reader) =>
      {
        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Populated = true;
      });
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

  private IEnumerable<bool> ReadProfileAuthorization()
  {
    entities.ExistingProfileAuthorization.Populated = false;

    return ReadEach("ReadProfileAuthorization",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "fkTrnTrancode", import.Transaction.Trancode);
        db.SetString(command, "fkTrnScreenid", import.Transaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProfileAuthorization.FkProName =
          db.GetString(reader, 1);
        entities.ExistingProfileAuthorization.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingProfileAuthorization.FkTrnScreenid =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.FkCmdValue =
          db.GetString(reader, 4);
        entities.ExistingProfileAuthorization.Populated = true;

        return true;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    private Command command;
    private Profile profile;
    private Transaction transaction;
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
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
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
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

    /// <summary>
    /// A value of ExistingProfileAuthorization.
    /// </summary>
    [JsonPropertyName("existingProfileAuthorization")]
    public ProfileAuthorization ExistingProfileAuthorization
    {
      get => existingProfileAuthorization ??= new();
      set => existingProfileAuthorization = value;
    }

    private Command existingCommand;
    private Profile existingProfile;
    private TransactionCommand existingTransactionCommand;
    private Transaction existingTransaction;
    private ProfileAuthorization existingProfileAuthorization;
  }
#endregion
}
