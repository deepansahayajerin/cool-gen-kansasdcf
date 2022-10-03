// Program: SI_RESEND_CSENET_TRANSACTION, ID: 373440324, model: 746.
// Short name: SWE01568
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RESEND_CSENET_TRANSACTION.
/// </summary>
[Serializable]
public partial class SiResendCsenetTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RESEND_CSENET_TRANSACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiResendCsenetTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiResendCsenetTransaction.
  /// </summary>
  public SiResendCsenetTransaction(IContext context, Import import,
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
    // *******************************************************************
    // *
    // 
    // *
    // * C H A N G E   L O G
    // 
    // *
    // * ===================
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    //     Date   Name      PR#  Reason
    //     ----   ----      ---  ------
    //   3-13-02   MCA    Initial Creation
    // *******************************************************************
    // *******************************************************************
    // Set the Error code to IREQ so the Resend Errored batch will pick it up.  
    // We set it to  "IREQ" so the batch will pick up these every night, it will
    // be hardcoded to look for all "IREQ"s.  The Others will be triggered by
    // an entry in the PPI Record.
    // The user will be fixing the data outside the interstate case.  The resend
    // errored batch will pick up the data that the user fixed and re-build or
    // update the csenet transaction.
    // *******************************************************************
    if (ReadCsenetTransactionEnvelop())
    {
      try
      {
        UpdateCsenetTransactionEnvelop();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSENET_TRANSACTION_ENVELOP_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSENET_TRANSACTION_ENVELOP_PV";

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
      ExitState = "CSENET_TRANSACTION_ENVELOP_NF";
    }
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum",
          import.InterstateRequestHistory.TransactionSerialNum);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateRequestHistory.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ErrorCode =
          db.GetNullableString(reader, 5);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var errorCode1 = "IREQ";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "errorCode", errorCode1);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.CsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.CsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.ErrorCode = errorCode1;
    entities.CsenetTransactionEnvelop.Populated = true;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
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
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
  }
#endregion
}
