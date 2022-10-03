// Program: SI_UPDATE_CSENET_TRANS_ENVELOP, ID: 373440623, model: 746.
// Short name: SWE02755
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_CSENET_TRANS_ENVELOP.
/// </summary>
[Serializable]
public partial class SiUpdateCsenetTransEnvelop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_CSENET_TRANS_ENVELOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateCsenetTransEnvelop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateCsenetTransEnvelop.
  /// </summary>
  public SiUpdateCsenetTransEnvelop(IContext context, Import import,
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
    if (Lt(local.CsenetTransactionEnvelop.LastUpdatedTimestamp,
      import.CsenetTransactionEnvelop.LastUpdatedTimestamp))
    {
      local.CsenetTransactionEnvelop.LastUpdatedTimestamp =
        import.CsenetTransactionEnvelop.LastUpdatedTimestamp;
    }
    else
    {
      local.CsenetTransactionEnvelop.LastUpdatedTimestamp = Now();
    }

    if (!IsEmpty(import.CsenetTransactionEnvelop.LastUpdatedBy))
    {
      local.CsenetTransactionEnvelop.LastUpdatedBy =
        import.CsenetTransactionEnvelop.LastUpdatedBy;
    }
    else
    {
      local.CsenetTransactionEnvelop.LastUpdatedBy = global.UserId;
    }

    if (!IsEmpty(import.CsenetTransactionEnvelop.ProcessingStatusCode))
    {
      local.CsenetTransactionEnvelop.ProcessingStatusCode =
        import.CsenetTransactionEnvelop.ProcessingStatusCode;
    }
    else if (!IsEmpty(import.CsenetTransactionEnvelop.ErrorCode))
    {
      local.CsenetTransactionEnvelop.ProcessingStatusCode = "E";
    }
    else
    {
      local.CsenetTransactionEnvelop.ProcessingStatusCode = "P";
    }

    if (ReadCsenetTransactionEnvelop())
    {
      if (AsChar(local.CsenetTransactionEnvelop.ProcessingStatusCode) != 'E'
        && AsChar(local.CsenetTransactionEnvelop.ProcessingStatusCode) != 'P'
        && AsChar(local.CsenetTransactionEnvelop.ProcessingStatusCode) != 'S')
      {
        ExitState = "CSENET_TRANSACTION_ENVELOP_PV";

        return;
      }

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
    else if (ReadInterstateCase())
    {
      ExitState = "CSENET_TRANSACTION_ENVELOP_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
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

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.CsenetTransactionEnvelop.LastUpdatedBy;
    var lastUpdatedTimestamp =
      local.CsenetTransactionEnvelop.LastUpdatedTimestamp;
    var processingStatusCode =
      local.CsenetTransactionEnvelop.ProcessingStatusCode;
    var errorCode1 = import.CsenetTransactionEnvelop.ErrorCode ?? "";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
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
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.ErrorCode = errorCode1;
    entities.CsenetTransactionEnvelop.Populated = true;
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
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
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
