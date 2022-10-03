// Program: SI_PEPR_DELETE_PERSON_PROGRAM, ID: 371736655, model: 746.
// Short name: SWE01161
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PEPR_DELETE_PERSON_PROGRAM.
/// </para>
/// <para>
/// This AB updates details of a program assigned to a person
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprDeletePersonProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_DELETE_PERSON_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprDeletePersonProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprDeletePersonProgram.
  /// </summary>
  public SiPeprDeletePersonProgram(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer	Request #	Description
    // 05/06/96  G. Lofton			Initial Development
    // ----------------------------------------------------------
    // **************************************************************
    // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadProgram())
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    if (ReadPersonProgram())
    {
      local.CsePersonAccount.PgmChgEffectiveDate =
        entities.PersonProgram.EffectiveDate;
      DeletePersonProgram();

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
        if (Equal(entities.Program.Code, "AF") || Equal
          (entities.Program.Code, "NF") || Equal
          (entities.Program.Code, "NA") || Equal
          (entities.Program.Code, "NC") || Equal
          (entities.Program.Code, "AFI") || Equal
          (entities.Program.Code, "FC") || Equal
          (entities.Program.Code, "FCI") || Equal
          (entities.Program.Code, "MAI") || Equal
          (entities.Program.Code, "NAI"))
        {
          if (ReadCsePersonAccount())
          {
            if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
              local.Blank.Date) || Lt
              (local.CsePersonAccount.PgmChgEffectiveDate,
              entities.CsePersonAccount.PgmChgEffectiveDate))
            {
              try
              {
                UpdateCsePersonAccount();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
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
            // This date is only updated for Supported Persons, CSE Person Type
            // 'S'
            // **********************************************************
          }
        }
      }
    }
    else
    {
      ExitState = "PERSON_PROGRAM_NF";
    }
  }

  private void DeletePersonProgram()
  {
    Update("DeletePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 8);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetString(command, "code", import.Program.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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

    private DateWorkArea blank;
    private CsePersonAccount csePersonAccount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
