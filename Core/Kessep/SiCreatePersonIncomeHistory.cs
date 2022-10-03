// Program: SI_CREATE_PERSON_INCOME_HISTORY, ID: 371766295, model: 746.
// Short name: SWE01147
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_PERSON_INCOME_HISTORY.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCreatePersonIncomeHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_PERSON_INCOME_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreatePersonIncomeHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreatePersonIncomeHistory.
  /// </summary>
  public SiCreatePersonIncomeHistory(IContext context, Import import,
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
    // -------------------------------------
    // 03/20/99 W.Campbell       Added local view
    //                           for CURRENT timestamp and
    //                           logic to use it for two set
    //                           statements inside the
    //                           create.
    // -------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    local.Current.Timestamp = Now();

    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    // 07/12/99  Changed property of READ (Select Only)
    if (ReadCsePerson())
    {
      // 07/12/99  Changed property of READ (Select Only)
      if (ReadIncomeSource())
      {
        do
        {
          // -------------------------------------
          // ASSOCIATION TO CSE PERSON IS REDUNDANT.
          // REMOVE STATEMENT ONCE DB CHANGE IS IMPLEMENTED.
          // -------------------------------------
          try
          {
            CreatePersonIncomeHistory();
            ExitState = "ACO_NN0000_ALL_OK";
            import.PersonIncomeHistory.Assign(entities.PersonIncomeHistory);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSON_INCOME_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PERSON_INCOME_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        while(!IsExitState("ACO_NN0000_ALL_OK"));
      }
      else
      {
        ExitState = "INCOME_SOURCE_NF";
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void CreatePersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var cspNumber = entities.CsePerson.Number;
    var isrIdentifier = entities.IncomeSource.Identifier;
    var identifier = local.Current.Timestamp;
    var incomeEffDt = import.PersonIncomeHistory.IncomeEffDt;
    var incomeAmt = import.PersonIncomeHistory.IncomeAmt.GetValueOrDefault();
    var freq = import.PersonIncomeHistory.Freq ?? "";
    var workerId = global.UserId;
    var verifiedDt = import.PersonIncomeHistory.VerifiedDt;
    var cspINumber = entities.IncomeSource.CspINumber;
    var militaryBaqAllotment =
      import.PersonIncomeHistory.MilitaryBaqAllotment.GetValueOrDefault();

    entities.PersonIncomeHistory.Populated = false;
    Update("CreatePersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetDateTime(command, "identifier", identifier);
        db.SetNullableDate(command, "incomeEffDt", incomeEffDt);
        db.SetNullableDecimal(command, "incomeAmt", incomeAmt);
        db.SetNullableString(command, "freq", freq);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDt", verifiedDt);
        db.SetNullableDecimal(
          command, "checkEarned", GetImplicitValue<PersonIncomeHistory,
          decimal?>("CheckEarned").GetValueOrDefault());
        db.SetNullableString(
          command, "checkEarnedFreq", GetImplicitValue<PersonIncomeHistory,
          string>("CheckEarnedFrequency"));
        db.SetNullableDecimal(
          command, "checkUnearned", GetImplicitValue<PersonIncomeHistory,
          decimal?>("CheckUnearned").GetValueOrDefault());
        db.SetNullableString(
          command, "checkUnearnFreq", GetImplicitValue<PersonIncomeHistory,
          string>("CheckUnearnedFrequency"));
        db.SetNullableDate(command, "checkPayDate", default(DateTime));
        db.SetNullableDecimal(
          command, "chkDeferredComp", GetImplicitValue<PersonIncomeHistory,
          decimal?>("CheckDeferredCompensation").GetValueOrDefault());
        db.SetNullableString(command, "paymentType", "");
        db.SetNullableDecimal(command, "checkMonthlyAmt", 0M);
        db.SetString(command, "createdBy", workerId);
        db.SetDateTime(command, "createdTimestamp", identifier);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "cspINumber", cspINumber);
        db.SetNullableDecimal(command, "baqAllotment", militaryBaqAllotment);
      });

    entities.PersonIncomeHistory.CspNumber = cspNumber;
    entities.PersonIncomeHistory.IsrIdentifier = isrIdentifier;
    entities.PersonIncomeHistory.Identifier = identifier;
    entities.PersonIncomeHistory.IncomeEffDt = incomeEffDt;
    entities.PersonIncomeHistory.IncomeAmt = incomeAmt;
    entities.PersonIncomeHistory.Freq = freq;
    entities.PersonIncomeHistory.WorkerId = workerId;
    entities.PersonIncomeHistory.VerifiedDt = verifiedDt;
    entities.PersonIncomeHistory.PaymentType = "";
    entities.PersonIncomeHistory.CreatedBy = workerId;
    entities.PersonIncomeHistory.CreatedTimestamp = identifier;
    entities.PersonIncomeHistory.CspINumber = cspINumber;
    entities.PersonIncomeHistory.MilitaryBaqAllotment = militaryBaqAllotment;
    entities.PersonIncomeHistory.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
      });
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
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
