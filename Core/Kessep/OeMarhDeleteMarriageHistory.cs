// Program: OE_MARH_DELETE_MARRIAGE_HISTORY, ID: 371884875, model: 746.
// Short name: SWE00944
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_MARH_DELETE_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block deletes marriage history details for a CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class OeMarhDeleteMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_DELETE_MARRIAGE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhDeleteMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhDeleteMarriageHistory.
  /// </summary>
  public OeMarhDeleteMarriageHistory(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block delets a given marriage history record.
    // PROCESSING:
    // This action block is passed the views of marriage history to be deleted. 
    // It reads and deletes the marriage history record.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	MARRIAGE_HISTORY	- R - D
    // DATABSE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		01-04-1995.
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHG REQ#	DESCRIPTION
    // govind	01-04-95		Initial Coding
    // *********************************************
    if (!ReadCsePerson())
    {
      ExitState = "OE0026_INVALID_CSE_PERSON_NO";

      return;
    }

    if (!ReadMarriageHistory())
    {
      ExitState = "OE0061_NF_MARRIAGE_HISTORY";

      return;
    }

    DeleteMarriageHistory();
  }

  private void DeleteMarriageHistory()
  {
    Update("DeleteMarriageHistory",
      (db, command) =>
      {
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Prime.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadMarriageHistory()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "identifier", import.Existing.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCurrent.Populated = true;
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
    /// A value of Prime.
    /// </summary>
    [JsonPropertyName("prime")]
    public CsePerson Prime
    {
      get => prime ??= new();
      set => prime = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public MarriageHistory Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CsePerson prime;
    private MarriageHistory existing;
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
    /// A value of ExistingPrime.
    /// </summary>
    [JsonPropertyName("existingPrime")]
    public CsePerson ExistingPrime
    {
      get => existingPrime ??= new();
      set => existingPrime = value;
    }

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public MarriageHistory ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    private CsePerson existingPrime;
    private MarriageHistory existingCurrent;
  }
#endregion
}
