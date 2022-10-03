// Program: SI_BATCH_START_NA_PRSN_PGM, ID: 371787557, model: 746.
// Short name: SWE01564
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_BATCH_START_NA_PRSN_PGM.
/// </summary>
[Serializable]
public partial class SiBatchStartNaPrsnPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BATCH_START_NA_PRSN_PGM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBatchStartNaPrsnPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBatchStartNaPrsnPgm.
  /// </summary>
  public SiBatchStartNaPrsnPgm(IContext context, Import import, Export export):
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
    //   Date    Developer       Request #       Description
    // 07/10/96  G. Lofton                       Initial Development
    // ----------------------------------------------------------
    // ****************************************************************
    // 11/24/1998  C. Ott   Problem Report # 43002
    // Rollover to 'NA' was not occurring if the case closure date was in the 
    // future.  Changed a READ statement to check whether Person Program
    // discontinue date is greater or equal to the import effective date rather
    // than the processing date.
    // ****************************************************************
    // **************************************************************
    // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    export.CntlTotPersPgmCreates.Count = import.CntlTotPersPgmCreates.Count;
    UseCabSetMaximumDiscontinueDate();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ****************************************************************
    // Before a NA program can be started check to verify no other
    // programs are in effect.
    // ****************************************************************
    // ****************************************************************
    // 11/24/1998  C. Ott   Problem Report # 43002
    // Rollover to 'NA' was not occurring if the case closure date was in the 
    // future.  Changed the following READ statement to check whether Person
    // Program discontinue date is greater or equal to the import effective date
    // rather than the processing date.
    // ****************************************************************
    // ****************************************************************
    // 12/15/1999  C. Ott   Changed the READ statement to check whether Person 
    // Program discontinue date is equal to high value date.
    // ****************************************************************
    if (ReadPersonProgram())
    {
      return;
    }

    if (!ReadProgram())
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    if (!Equal(import.PersonProgram.DiscontinueDate, local.Blank.Date))
    {
      local.PersonProgram.DiscontinueDate =
        import.PersonProgram.DiscontinueDate;
    }
    else
    {
      local.PersonProgram.DiscontinueDate = local.Max.Date;
    }

    try
    {
      CreatePersonProgram();

      // **************************************************************
      // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
      // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
      // when a program change is made.
      // *************************************************************
      if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
      {
        if (ReadCsePersonAccount())
        {
          if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
            local.Blank.Date) || Lt
            (import.PersonProgram.EffectiveDate,
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
          // This date is only updated for Supported Persons, CSE Person Type '
          // S'
          // **********************************************************
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PERSON_PROGRAM_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PERSON_PROGRAM_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    ++import.ExpCheckpointNumbUpdates.Count;
    ++export.CntlTotPersPgmCreates.Count;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void CreatePersonProgram()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = import.PersonProgram.EffectiveDate;
    var assignedDate = import.PersonProgram.AssignedDate;
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var changedInd = import.PersonProgram.ChangedInd ?? "";
    var changeDate = local.Blank.Date;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.PersonProgram.Populated = false;
    Update("CreatePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "closureReason", "");
        db.SetNullableDate(command, "assignedDate", assignedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", createdTimestamp);
        db.SetNullableString(command, "changedInd", changedInd);
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", default(DateTime));
      });

    entities.PersonProgram.CspNumber = cspNumber;
    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.AssignedDate = assignedDate;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.CreatedBy = createdBy;
    entities.PersonProgram.CreatedTimestamp = createdTimestamp;
    entities.PersonProgram.LastUpdatedBy = createdBy;
    entities.PersonProgram.LastUpdatdTstamp = createdTimestamp;
    entities.PersonProgram.ChangedInd = changedInd;
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.PersonProgram.Populated = true;
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
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 4);
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

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      null,
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = import.PersonProgram.EffectiveDate;

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
    /// A value of ExpCheckpointNumbUpdates.
    /// </summary>
    [JsonPropertyName("expCheckpointNumbUpdates")]
    public Common ExpCheckpointNumbUpdates
    {
      get => expCheckpointNumbUpdates ??= new();
      set => expCheckpointNumbUpdates = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of CntlTotPersPgmCreates.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreates")]
    public Common CntlTotPersPgmCreates
    {
      get => cntlTotPersPgmCreates ??= new();
      set => cntlTotPersPgmCreates = value;
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

    private Common expCheckpointNumbUpdates;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PersonProgram personProgram;
    private Common cntlTotPersPgmCreates;
    private Common recomputeDistribution;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CntlTotPersPgmCreates.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreates")]
    public Common CntlTotPersPgmCreates
    {
      get => cntlTotPersPgmCreates ??= new();
      set => cntlTotPersPgmCreates = value;
    }

    private Common cntlTotPersPgmCreates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private DateWorkArea max;
    private PersonProgram personProgram;
    private DateWorkArea blank;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
