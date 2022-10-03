// Program: SI_UPDATE_PERSON_INCOME_HISTORY, ID: 371766294, model: 746.
// Short name: SWE01256
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_PERSON_INCOME_HISTORY.
/// </summary>
[Serializable]
public partial class SiUpdatePersonIncomeHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_PERSON_INCOME_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdatePersonIncomeHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdatePersonIncomeHistory.
  /// </summary>
  public SiUpdatePersonIncomeHistory(IContext context, Import import,
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
    // ??/??/??  ????????              Initial Development
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    // 07/12/99  Changed property of READ (Select Only)
    if (ReadPersonIncomeHistory())
    {
      try
      {
        UpdatePersonIncomeHistory();
        import.PersonIncomeHistory.Assign(entities.PersonIncomeHistory);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PERSON_INCOME_HISTORY_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PERSON_INCOME_PV";

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
      ExitState = "PERSON_INCOME_HISTORY_NF";
    }
  }

  private bool ReadPersonIncomeHistory()
  {
    entities.PersonIncomeHistory.Populated = false;

    return Read("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.PersonIncomeHistory.Identifier.GetValueOrDefault());
        db.SetDateTime(
          command, "isrIdentifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonIncomeHistory.CspNumber = db.GetString(reader, 0);
        entities.PersonIncomeHistory.IsrIdentifier = db.GetDateTime(reader, 1);
        entities.PersonIncomeHistory.Identifier = db.GetDateTime(reader, 2);
        entities.PersonIncomeHistory.IncomeEffDt =
          db.GetNullableDate(reader, 3);
        entities.PersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 4);
        entities.PersonIncomeHistory.Freq = db.GetNullableString(reader, 5);
        entities.PersonIncomeHistory.WorkerId = db.GetNullableString(reader, 6);
        entities.PersonIncomeHistory.VerifiedDt = db.GetNullableDate(reader, 7);
        entities.PersonIncomeHistory.PaymentType =
          db.GetNullableString(reader, 8);
        entities.PersonIncomeHistory.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.PersonIncomeHistory.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.PersonIncomeHistory.CspINumber = db.GetString(reader, 11);
        entities.PersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 12);
        entities.PersonIncomeHistory.Populated = true;
      });
  }

  private void UpdatePersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PersonIncomeHistory.Populated);

    var incomeEffDt = import.PersonIncomeHistory.IncomeEffDt;
    var incomeAmt = import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault();
    var freq = import.PersonIncomeHistory.Freq ?? "";
    var workerId = global.UserId;
    var verifiedDt = import.PersonIncomeHistory.VerifiedDt;
    var lastUpdatedTimestamp = Now();
    var militaryBaqAllotment =
      import.PersonIncomeHistory.MilitaryBaqAllotment.GetValueOrDefault();

    entities.PersonIncomeHistory.Populated = false;
    Update("UpdatePersonIncomeHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "incomeEffDt", incomeEffDt);
        db.SetNullableDecimal(command, "incomeAmt", incomeAmt);
        db.SetNullableString(command, "freq", freq);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDt", verifiedDt);
        db.SetNullableString(command, "lastUpdatedBy", workerId);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDecimal(command, "baqAllotment", militaryBaqAllotment);
        db.SetString(
          command, "cspNumber", entities.PersonIncomeHistory.CspNumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.PersonIncomeHistory.IsrIdentifier.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          entities.PersonIncomeHistory.Identifier.GetValueOrDefault());
        db.SetString(
          command, "cspINumber", entities.PersonIncomeHistory.CspINumber);
      });

    entities.PersonIncomeHistory.IncomeEffDt = incomeEffDt;
    entities.PersonIncomeHistory.IncomeAmt = incomeAmt;
    entities.PersonIncomeHistory.Freq = freq;
    entities.PersonIncomeHistory.WorkerId = workerId;
    entities.PersonIncomeHistory.VerifiedDt = verifiedDt;
    entities.PersonIncomeHistory.LastUpdatedBy = workerId;
    entities.PersonIncomeHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PersonIncomeHistory.MilitaryBaqAllotment = militaryBaqAllotment;
    entities.PersonIncomeHistory.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private PersonIncomeHistory personIncomeHistory;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private PersonIncomeHistory personIncomeHistory;
  }
#endregion
}
