// Program: FN_NOTIFY_AE_OF_PAYMENT, ID: 372727828, model: 746.
// Short name: IMP42054
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_NOTIFY_AE_OF_PAYMENT.
/// </summary>
[Serializable]
public partial class FnNotifyAeOfPayment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NOTIFY_AE_OF_PAYMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNotifyAeOfPayment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNotifyAeOfPayment.
  /// </summary>
  public FnNotifyAeOfPayment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    do
    {
      try
      {
        CreateInterfaceIncomeNotification();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.RetryCounter.Count;

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_INTERFACE_INCOME_NOTIF_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(local.RetryCounter.Count <= 10);

    ExitState = "FN0000_INTERFACE_INCOME_NOTIF_AE";
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateInterfaceIncomeNotification()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var supportedCsePersonNumber =
      import.InterfaceIncomeNotification.SupportedCsePersonNumber;
    var obligorCsePersonNumber =
      import.InterfaceIncomeNotification.ObligorCsePersonNumber;
    var caseNumber = import.InterfaceIncomeNotification.CaseNumber;
    var collectionDate = import.InterfaceIncomeNotification.CollectionDate;
    var collectionAmount = import.InterfaceIncomeNotification.CollectionAmount;
    var personProgram = import.InterfaceIncomeNotification.PersonProgram;
    var programAppliedTo = import.InterfaceIncomeNotification.ProgramAppliedTo;
    var appliedToCode = import.InterfaceIncomeNotification.AppliedToCode;
    var distributionDate = import.InterfaceIncomeNotification.DistributionDate;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.InterfaceIncomeNotification.ProcessDate;

    entities.InterfaceIncomeNotification.Populated = false;
    Update("CreateInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetInt32(command, "intrfcIncNtfId", systemGeneratedIdentifier);
        db.SetString(command, "suppCspNumber", supportedCsePersonNumber);
        db.SetString(command, "obligorCspNumber", obligorCsePersonNumber);
        db.SetString(command, "caseNumb", caseNumber);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetString(command, "personProgram", personProgram);
        db.SetString(command, "programAppliedTo", programAppliedTo);
        db.SetString(command, "appliedToCode", appliedToCode);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDate(command, "processDt", processDate);
      });

    entities.InterfaceIncomeNotification.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterfaceIncomeNotification.SupportedCsePersonNumber =
      supportedCsePersonNumber;
    entities.InterfaceIncomeNotification.ObligorCsePersonNumber =
      obligorCsePersonNumber;
    entities.InterfaceIncomeNotification.CaseNumber = caseNumber;
    entities.InterfaceIncomeNotification.CollectionDate = collectionDate;
    entities.InterfaceIncomeNotification.CollectionAmount = collectionAmount;
    entities.InterfaceIncomeNotification.PersonProgram = personProgram;
    entities.InterfaceIncomeNotification.ProgramAppliedTo = programAppliedTo;
    entities.InterfaceIncomeNotification.AppliedToCode = appliedToCode;
    entities.InterfaceIncomeNotification.DistributionDate = distributionDate;
    entities.InterfaceIncomeNotification.CreatedTimestamp = createdTimestamp;
    entities.InterfaceIncomeNotification.CreatedBy = createdBy;
    entities.InterfaceIncomeNotification.ProcessDate = processDate;
    entities.InterfaceIncomeNotification.Populated = true;
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
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
    }

    private InterfaceIncomeNotification interfaceIncomeNotification;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RetryCounter.
    /// </summary>
    [JsonPropertyName("retryCounter")]
    public Common RetryCounter
    {
      get => retryCounter ??= new();
      set => retryCounter = value;
    }

    private Common retryCounter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
    }

    private InterfaceIncomeNotification interfaceIncomeNotification;
  }
#endregion
}
