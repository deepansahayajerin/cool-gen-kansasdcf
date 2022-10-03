// Program: SI_DELETE_INTERSTATE_PARTICIPANT, ID: 373441312, model: 746.
// Short name: SWE02753
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DELETE_INTERSTATE_PARTICIPANT.
/// </summary>
[Serializable]
public partial class SiDeleteInterstateParticipant: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_INTERSTATE_PARTICIPANT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteInterstateParticipant(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteInterstateParticipant.
  /// </summary>
  public SiDeleteInterstateParticipant(IContext context, Import import,
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
    if (ReadInterstateParticipant())
    {
      DeleteInterstateParticipant();
    }
    else if (ReadInterstateCase())
    {
      // -----------------------------------------------------------
      // Attempting to DELETE and it doesn't exist - not an error
      // -----------------------------------------------------------
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private void DeleteInterstateParticipant()
  {
    Update("DeleteInterstateParticipant",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateParticipant.CcaTransactionDt.GetValueOrDefault());
        db.SetInt32(
          command, "sysGeneratedId",
          entities.InterstateParticipant.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateParticipant.CcaTransSerNum);
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

  private bool ReadInterstateParticipant()
  {
    entities.InterstateParticipant.Populated = false;

    return Read("ReadInterstateParticipant",
      (db, command) =>
      {
        db.SetInt32(
          command, "sysGeneratedId",
          import.InterstateParticipant.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.Populated = true;
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
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
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

    private InterstateParticipant interstateParticipant;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
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

    private InterstateParticipant interstateParticipant;
    private InterstateCase interstateCase;
  }
#endregion
}
