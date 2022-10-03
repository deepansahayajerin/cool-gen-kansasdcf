// Program: SECURITY_CAB_GENERATOR, ID: 374508518, model: 746.
// Short name: SECURITY
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: SECURITY_CAB_GENERATOR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SecurityCabGenerator: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SECURITY_CAB_GENERATOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SecurityCabGenerator(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SecurityCabGenerator.
  /// </summary>
  public SecurityCabGenerator(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseCreateCommand();
    UseCreateProfile();
    UseCreateProfileAuthorization();
    UseCreateServProvProfile();
    UseCreateTransactions();
    UseCreateTransactionCommands();
    UseDeleteCommand();
    UseDeleteProfile();
    UseDeleteProfileAuthorization();
    UseDeleteServProvProfile();
    UseDeleteTransactions();
    UseDeleteTransactionCommands();
    UseUpdateCommand();
    UseUpdateProfile();
    UseUpdateProfileAuthorization();
    UseUpdateTransactions();
    UseScEabValidateTopSecret();
    UseUpdateServProvProfile();
  }

  private void UseCreateCommand()
  {
    var useImport = new CreateCommand.Import();
    var useExport = new CreateCommand.Export();

    Call(CreateCommand.Execute, useImport, useExport);
  }

  private void UseCreateProfile()
  {
    var useImport = new CreateProfile.Import();
    var useExport = new CreateProfile.Export();

    Call(CreateProfile.Execute, useImport, useExport);
  }

  private void UseCreateProfileAuthorization()
  {
    var useImport = new CreateProfileAuthorization.Import();
    var useExport = new CreateProfileAuthorization.Export();

    Call(CreateProfileAuthorization.Execute, useImport, useExport);
  }

  private void UseCreateServProvProfile()
  {
    var useImport = new CreateServProvProfile.Import();
    var useExport = new CreateServProvProfile.Export();

    Call(CreateServProvProfile.Execute, useImport, useExport);
  }

  private void UseCreateTransactionCommands()
  {
    var useImport = new CreateTransactionCommands.Import();
    var useExport = new CreateTransactionCommands.Export();

    Call(CreateTransactionCommands.Execute, useImport, useExport);
  }

  private void UseCreateTransactions()
  {
    var useImport = new CreateTransactions.Import();
    var useExport = new CreateTransactions.Export();

    Call(CreateTransactions.Execute, useImport, useExport);
  }

  private void UseDeleteCommand()
  {
    var useImport = new DeleteCommand.Import();
    var useExport = new DeleteCommand.Export();

    Call(DeleteCommand.Execute, useImport, useExport);
  }

  private void UseDeleteProfile()
  {
    var useImport = new DeleteProfile.Import();
    var useExport = new DeleteProfile.Export();

    Call(DeleteProfile.Execute, useImport, useExport);
  }

  private void UseDeleteProfileAuthorization()
  {
    var useImport = new DeleteProfileAuthorization.Import();
    var useExport = new DeleteProfileAuthorization.Export();

    Call(DeleteProfileAuthorization.Execute, useImport, useExport);
  }

  private void UseDeleteServProvProfile()
  {
    var useImport = new DeleteServProvProfile.Import();
    var useExport = new DeleteServProvProfile.Export();

    Call(DeleteServProvProfile.Execute, useImport, useExport);
  }

  private void UseDeleteTransactionCommands()
  {
    var useImport = new DeleteTransactionCommands.Import();
    var useExport = new DeleteTransactionCommands.Export();

    Call(DeleteTransactionCommands.Execute, useImport, useExport);
  }

  private void UseDeleteTransactions()
  {
    var useImport = new DeleteTransactions.Import();
    var useExport = new DeleteTransactions.Export();

    Call(DeleteTransactions.Execute, useImport, useExport);
  }

  private void UseScEabValidateTopSecret()
  {
    var useImport = new ScEabValidateTopSecret.Import();
    var useExport = new ScEabValidateTopSecret.Export();

    Call(ScEabValidateTopSecret.Execute, useImport, useExport);
  }

  private void UseUpdateCommand()
  {
    var useImport = new UpdateCommand.Import();
    var useExport = new UpdateCommand.Export();

    Call(UpdateCommand.Execute, useImport, useExport);
  }

  private void UseUpdateProfile()
  {
    var useImport = new UpdateProfile.Import();
    var useExport = new UpdateProfile.Export();

    Call(UpdateProfile.Execute, useImport, useExport);
  }

  private void UseUpdateProfileAuthorization()
  {
    var useImport = new UpdateProfileAuthorization.Import();
    var useExport = new UpdateProfileAuthorization.Export();

    Call(UpdateProfileAuthorization.Execute, useImport, useExport);
  }

  private void UseUpdateServProvProfile()
  {
    var useImport = new UpdateServProvProfile.Import();
    var useExport = new UpdateServProvProfile.Export();

    Call(UpdateServProvProfile.Execute, useImport, useExport);
  }

  private void UseUpdateTransactions()
  {
    var useImport = new UpdateTransactions.Import();
    var useExport = new UpdateTransactions.Export();

    Call(UpdateTransactions.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
  }
#endregion
}
