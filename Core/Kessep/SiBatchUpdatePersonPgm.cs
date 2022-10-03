// Program: SI_BATCH_UPDATE_PERSON_PGM, ID: 371787555, model: 746.
// Short name: SWE01556
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_BATCH_UPDATE_PERSON_PGM.
/// </para>
/// <para>
/// This AB updates details of a program assigned to a person
/// </para>
/// </summary>
[Serializable]
public partial class SiBatchUpdatePersonPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BATCH_UPDATE_PERSON_PGM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBatchUpdatePersonPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBatchUpdatePersonPgm.
  /// </summary>
  public SiBatchUpdatePersonPgm(IContext context, Import import, Export export):
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
    // 06/24/96  G. Lofton                       Initial Development
    // ----------------------------------------------------------
    // **************************************************************
    // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    // **************************************************************
    // 12/03/99   C. Ott   Problem # 78289, Change READ to READ EACH in order to
    // handle program that may be closed already and update MED TYPE.
    // *************************************************************
    // ****************************************************************
    // 12/07/99  C. Ott   PR # 82109, Program end date should not change if 
    // program is already closed.
    // ***************************************************************
    // ****************************************************************
    // 01/28/00  C. Ott   PR # 85941.  Delete records when effective date and 
    // end date are equal.
    // ***************************************************************
    export.CntlTotPersPgmCloses.Count = import.CntlTotPersPgmCloses.Count;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadProgram())
    {
      MoveProgram2(entities.Program, export.Program);
    }
    else
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    foreach(var item in ReadPersonProgram())
    {
      // ************************************************************
      // 09/07/00 A Doty - Correct a problem with setting the trigger date.
      // ************************************************************
      local.Tmp.DiscontinueDate = entities.PersonProgram.DiscontinueDate;

      if (!Lt(local.Blank.DiscontinueDate, import.PersonProgram.DiscontinueDate))
        
      {
        local.PersonProgram.DiscontinueDate =
          entities.PersonProgram.DiscontinueDate;
      }
      else if (Lt(entities.PersonProgram.DiscontinueDate, local.Max.Date))
      {
        // **************************************************************
        // Program is already closed, set discontinue date to existing
        // program discontinue date.
        // ***************************************************************
        local.PersonProgram.DiscontinueDate =
          entities.PersonProgram.DiscontinueDate;
      }
      else if (Lt(import.PersonProgram.DiscontinueDate,
        entities.PersonProgram.EffectiveDate))
      {
        if (Equal(entities.PersonProgram.DiscontinueDate, local.Max.Date))
        {
          local.PersonProgram.DiscontinueDate =
            entities.PersonProgram.DiscontinueDate;
        }
        else
        {
          local.PersonProgram.DiscontinueDate =
            AddDays(entities.PersonProgram.EffectiveDate, 1);
        }
      }
      else if (Lt(entities.PersonProgram.DiscontinueDate,
        import.PersonProgram.DiscontinueDate))
      {
        // ****************************************************************
        // 12/07/99  C. Ott   PR # 82109, Program end date should not change if 
        // program is already closed.
        // ***************************************************************
        if (Lt(local.Blank.DiscontinueDate,
          entities.PersonProgram.DiscontinueDate))
        {
          local.PersonProgram.DiscontinueDate =
            entities.PersonProgram.DiscontinueDate;
        }
        else
        {
          local.PersonProgram.DiscontinueDate =
            import.PersonProgram.DiscontinueDate;
        }
      }
      else
      {
        local.PersonProgram.DiscontinueDate =
          import.PersonProgram.DiscontinueDate;
      }

      local.PersonProgram.MedTypeDiscontinueDate =
        entities.PersonProgram.MedTypeDiscontinueDate;

      if (!IsEmpty(entities.PersonProgram.MedType))
      {
        if ((
          Equal(entities.PersonProgram.MedTypeDiscontinueDate, local.Max.Date) ||
          Equal
          (entities.PersonProgram.MedTypeDiscontinueDate,
          local.Blank.MedTypeDiscontinueDate)) && Lt
          (local.Blank.MedTypeDiscontinueDate,
          import.PersonProgram.MedTypeDiscontinueDate))
        {
          local.PersonProgram.MedTypeDiscontinueDate =
            import.PersonProgram.MedTypeDiscontinueDate;
        }
      }

      if (Equal(entities.PersonProgram.EffectiveDate,
        local.PersonProgram.DiscontinueDate))
      {
        if (Equal(import.PersonProgram.ClosureReason, "IE"))
        {
          export.AeCaseDenied.Flag = "Y";
        }

        UseSiPeprDeletePersonProgram();
        MovePersonProgram(import.PersonProgram, export.PersonProgram);
      }
      else
      {
        try
        {
          UpdatePersonProgram();
          export.PersonProgram.Assign(entities.PersonProgram);
          ++export.CntlTotPersPgmCloses.Count;

          // **************************************************************
          // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
          // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
          // distribution when a program change is made.
          // *************************************************************
          if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
          {
            // ************************************************************
            // 09/07/00 A Doty - Correct a problem with setting the trigger 
            // date.
            // ************************************************************
            if (!Equal(local.Tmp.DiscontinueDate,
              local.PersonProgram.DiscontinueDate))
            {
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
                  // ************************************************************
                  // 09/07/00 A Doty - Correct a problem with setting the 
                  // trigger date.
                  // ************************************************************
                  if (Lt(local.PersonProgram.DiscontinueDate,
                    local.Tmp.DiscontinueDate))
                  {
                    local.Tmp.DiscontinueDate =
                      local.PersonProgram.DiscontinueDate;
                  }

                  local.CsePersonAccount.PgmChgEffectiveDate =
                    AddDays(local.Tmp.DiscontinueDate, 1);

                  if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                    local.Blank.DiscontinueDate) || Lt
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
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PERSON_PROGRAM_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PERSON_PROGRAM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveProgram1(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgram2(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiPeprDeletePersonProgram()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    useImport.PersonProgram.Assign(entities.PersonProgram);
    MoveProgram1(entities.Program, useImport.Program);
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = import.RecomputeDistribution.Flag;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
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

  private IEnumerable<bool> ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 10);
        entities.PersonProgram.Populated = true;

        return true;
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

  private void UpdatePersonProgram()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var closureReason = import.PersonProgram.ClosureReason ?? "";
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate = local.PersonProgram.MedTypeDiscontinueDate;

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "closureReason", closureReason);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });

    entities.PersonProgram.ClosureReason = closureReason;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
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
    /// A value of CntlTotPersPgmCloses.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCloses")]
    public Common CntlTotPersPgmCloses
    {
      get => cntlTotPersPgmCloses ??= new();
      set => cntlTotPersPgmCloses = value;
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

    private Common cntlTotPersPgmCloses;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of CntlTotPersPgmCloses.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCloses")]
    public Common CntlTotPersPgmCloses
    {
      get => cntlTotPersPgmCloses ??= new();
      set => cntlTotPersPgmCloses = value;
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
    /// A value of AeCaseDenied.
    /// </summary>
    [JsonPropertyName("aeCaseDenied")]
    public Common AeCaseDenied
    {
      get => aeCaseDenied ??= new();
      set => aeCaseDenied = value;
    }

    private PersonProgram personProgram;
    private Common cntlTotPersPgmCloses;
    private Program program;
    private Common aeCaseDenied;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public PersonProgram Blank
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public PersonProgram Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    private CsePersonAccount csePersonAccount;
    private DateWorkArea max;
    private PersonProgram blank;
    private PersonProgram personProgram;
    private PersonProgram tmp;
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
