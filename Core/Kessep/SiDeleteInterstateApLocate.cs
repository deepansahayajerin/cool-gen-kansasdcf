// Program: SI_DELETE_INTERSTATE_AP_LOCATE, ID: 373441301, model: 746.
// Short name: SWE02750
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DELETE_INTERSTATE_AP_LOCATE.
/// </summary>
[Serializable]
public partial class SiDeleteInterstateApLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_INTERSTATE_AP_LOCATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteInterstateApLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteInterstateApLocate.
  /// </summary>
  public SiDeleteInterstateApLocate(IContext context, Import import,
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
    if (ReadInterstateApLocate())
    {
      DeleteInterstateApLocate();
    }
    else if (ReadInterstateApIdentification())
    {
      // -----------------------------------------------------------
      // Attempting to DELETE and it doesn't exist - not an error
      // -----------------------------------------------------------
    }
    else if (ReadInterstateCase())
    {
      ExitState = "CSENET_AP_ID_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private void DeleteInterstateApLocate()
  {
    Update("DeleteInterstateApLocate",
      (db, command) =>
      {
        db.SetDate(
          command, "cncTransactionDt",
          entities.InterstateApLocate.CncTransactionDt.GetValueOrDefault());
        db.SetInt64(
          command, "cncTransSerlNbr",
          entities.InterstateApLocate.CncTransSerlNbr);
      });
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
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
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateApLocate()
  {
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocate",
      (db, command) =>
      {
        db.SetInt64(
          command, "cncTransSerlNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "cncTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApLocate.ResidentialAddressLine1 =
          db.GetNullableString(reader, 2);
        entities.InterstateApLocate.Populated = true;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
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

    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
  }
#endregion
}
