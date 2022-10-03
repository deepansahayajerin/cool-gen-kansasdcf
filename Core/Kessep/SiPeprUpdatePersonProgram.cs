// Program: SI_PEPR_UPDATE_PERSON_PROGRAM, ID: 371736657, model: 746.
// Short name: SWE01257
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PEPR_UPDATE_PERSON_PROGRAM.
/// </para>
/// <para>
/// This AB updates details of a program assigned to a person
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprUpdatePersonProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_UPDATE_PERSON_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprUpdatePersonProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprUpdatePersonProgram.
  /// </summary>
  public SiPeprUpdatePersonProgram(IContext context, Import import,
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
    // ****************************************************************
    // 9/23/1999  C. Ott    Problem # 74221.  Set changed indicator and change 
    // date when a program closes.
    // ***************************************************************
    // **************************************************************
    // 10/01/99   C. Ott     Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // Rolled back changes made for PR # 74221
    // *************************************************************
    // **************************************************************
    // 09/06/00   M.Lachowicz    WR # 00188.
    //                           Update Person Program Effective
    //                           date to value from import view.
    // *************************************************************
    if (ReadPersonProgram())
    {
      if (Equal(import.PersonProgram.EffectiveDate, null))
      {
        local.PersonProgram.EffectiveDate =
          entities.PersonProgram.EffectiveDate;
      }
      else
      {
        local.PersonProgram.EffectiveDate = import.PersonProgram.EffectiveDate;
      }

      if (Equal(import.PersonProgram.DiscontinueDate, null))
      {
        local.PersonProgram.DiscontinueDate =
          entities.PersonProgram.DiscontinueDate;
      }
      else
      {
        local.PersonProgram.DiscontinueDate =
          import.PersonProgram.DiscontinueDate;
      }

      // 09/06/00 M.L Start
      if (!Equal(local.PersonProgram.EffectiveDate,
        entities.PersonProgram.EffectiveDate))
      {
        local.ChangedDate.Date = local.PersonProgram.EffectiveDate;
      }
      else
      {
        local.ChangedDate.Date = local.PersonProgram.DiscontinueDate;
      }

      // 09/06/00 M.L End
      try
      {
        UpdatePersonProgram();

        if (AsChar(import.RecomputeDistribution.Flag) == 'N')
        {
        }
        else
        {
          // **************************************************************
          // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
          // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
          // distribution when a program change is made.
          // *************************************************************
          if (Equal(import.Program.Code, "AF") || Equal
            (import.Program.Code, "NF") || Equal(import.Program.Code, "NA") || Equal
            (import.Program.Code, "NC") || Equal
            (import.Program.Code, "AFI") || Equal
            (import.Program.Code, "FC") || Equal
            (import.Program.Code, "FCI") || Equal
            (import.Program.Code, "MAI") || Equal(import.Program.Code, "NAI"))
          {
            if (ReadCsePersonAccount())
            {
              // 09/06/00 M.L Start
              local.CsePersonAccount.PgmChgEffectiveDate =
                local.ChangedDate.Date;

              // 09/06/00 M.L End
              if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                local.Blank.Date) || Lt
                (local.CsePersonAccount.PgmChgEffectiveDate,
                entities.CsePersonAccount.PgmChgEffectiveDate))
              {
                try
                {
                  UpdateCsePersonAccount();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            else
            {
              // ***********************************************************
              // This date is only updated for Supported Persons, CSE Person 
              // Type 'S'
              // **********************************************************
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PERSON_PROGRAM_NU";

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
      ExitState = "PERSON_PROGRAM_NF";
    }
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonAccount.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "code", import.Program.Code);
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.PersonProgram.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.Populated = true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.CsePersonAccount.PgmChgEffectiveDate;

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "type", entities.CsePersonAccount.Type1);
      });

    entities.CsePersonAccount.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAccount.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonAccount.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.CsePersonAccount.Populated = true;
  }

  private void UpdatePersonProgram()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var effectiveDate = local.PersonProgram.EffectiveDate;
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var changedInd = local.PersonProgram.ChangedInd ?? "";
    var changeDate = local.PersonProgram.ChangeDate;

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "changedInd", changedInd);
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });

    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.ChangedInd = changedInd;
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.Populated = true;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

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
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    private PersonProgram personProgram;
    private Program program;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common recomputeDistribution;
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
    /// A value of ChangedDate.
    /// </summary>
    [JsonPropertyName("changedDate")]
    public DateWorkArea ChangedDate
    {
      get => changedDate ??= new();
      set => changedDate = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private DateWorkArea changedDate;
    private DateWorkArea blank;
    private PersonProgram personProgram;
    private CsePersonAccount csePersonAccount;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
