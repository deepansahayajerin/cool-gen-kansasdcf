// Program: SI_CREATE_OG_CSENET_ENVELOP, ID: 372382228, model: 746.
// Short name: SWE01142
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_OG_CSENET_ENVELOP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the outgoing CSENet Envelop which contains data about the 
/// CSENet transaction; used to uniquely identify the transaction within KESSEP,
/// to indicate its processing status, creation date and time.
/// Set the status to the current procesing status.
/// R - Request
/// S - Sent
/// C - Received
/// P - Processed.	
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetEnvelop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_ENVELOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetEnvelop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetEnvelop.
  /// </summary>
  public SiCreateOgCsenetEnvelop(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // 07/10/95  Sid   Initial Creation.
    // Create the CSENet Envelop and associate it
    // with the CSENet Case.
    // 02/27/2001	M Ramirez	114580
    // Changed process status to S (from R)
    // ---------------------------------------------
    if (ReadInterstateCase())
    {
      try
      {
        CreateCsenetTransactionEnvelop();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        export.Zdel.Assign(entities.CsenetTransactionEnvelop);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSENET_ENVELOP_AE";

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
    else
    {
      ExitState = "CSENET_CASE_NF";
    }
  }

  private void CreateCsenetTransactionEnvelop()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var directionInd = "O";
    var processingStatusCode = "S";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("CreateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "directionInd", directionInd);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTstamp", lastUpdatedTimestamp);
        db.SetNullableString(command, "errorCode", "");
      });

    entities.CsenetTransactionEnvelop.CcaTransactionDt = ccaTransactionDt;
    entities.CsenetTransactionEnvelop.CcaTransSerNum = ccaTransSerNum;
    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.DirectionInd = directionInd;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.CreatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.CreatedTstamp = lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsenetTransactionEnvelop Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsenetTransactionEnvelop Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private CsenetTransactionEnvelop zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
  }
#endregion
}
