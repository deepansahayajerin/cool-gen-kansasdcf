// Program: SI_PEPR_CREATE_PERSON_PROGRAM, ID: 371727807, model: 746.
// Short name: SWE01148
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PEPR_CREATE_PERSON_PROGRAM.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will create the association between a person and a 
/// program.
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprCreatePersonProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_CREATE_PERSON_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprCreatePersonProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprCreatePersonProgram.
  /// </summary>
  public SiPeprCreatePersonProgram(IContext context, Import import,
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
    //   Date      Developer       Request #       Description
    // 09/20/95  Helen Sharland    Initial Development
    // 09/17/97  Sid Chowdhary	    Setting the classification code for FN.
    // 01/27/97  Sid		    Edits to make sure that only one open
    // 			    program exists for each program type.
    // ----------------------------------------------------------
    // **************************************************************
    // 9/22/1999   C. Ott    Added logic to set Person Program Change Indicator 
    // for distribution.
    // **************************************************************
    // **************************************************************
    // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    // **************************************************************
    // 09/13/01  M. Lachowicz    PR 127465      Set created by to
    //                                          
    // userid from import view.
    // *************************************************************
    local.PersonProgram.DiscontinueDate = import.PersonProgram.DiscontinueDate;
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 09/13/01 M. Lachowicz Start
    if (IsEmpty(import.Security.Userid))
    {
      local.Security.Userid = global.UserId;
    }
    else
    {
      local.Security.Userid = import.Security.Userid;
    }

    // 09/13/01 M. Lachowicz End
    if (Equal(import.PersonProgram.DiscontinueDate, null))
    {
      local.PersonProgram.DiscontinueDate = local.Max.Date;
    }

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

    if (Equal(import.PersonProgram.DiscontinueDate, null))
    {
      // ------------------------------------------------------
      // Check if an Open Person Program already exists.
      // ------------------------------------------------------
      if (ReadPersonProgram1())
      {
        // ------------------------------------------------------
        // An Open Person Program already exists for this person and program.
        // ------------------------------------------------------
        return;
      }
    }

    // ***************************************************************
    // Determine whether there are any currently active Person Programs.  If no 
    // other programs active, set flag to update CSE PERSON ACCOUNT for
    // distribution.
    // **************************************************************
    local.UpdateCsePersonAccount.Flag = "Y";

    if (ReadPersonProgram2())
    {
      local.UpdateCsePersonAccount.Flag = "N";
    }

    try
    {
      CreatePersonProgram();

      if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
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
          (entities.Program.Code, "NAI") || AsChar
          (local.UpdateCsePersonAccount.Flag) == 'Y')
        {
          if (ReadCsePersonAccount())
          {
            local.CsePersonAccount.PgmChgEffectiveDate =
              import.PersonProgram.EffectiveDate;

            if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
              local.Zero.Date) || Lt
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
            // This date is only updated for Supported Persons, CSE Person Type
            // 'S'
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
          ExitState = "PERSON_PROGRAM_AE";

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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreatePersonProgram()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = import.PersonProgram.EffectiveDate;
    var closureReason = import.PersonProgram.ClosureReason ?? "";
    var assignedDate = local.Current.Date;
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var createdBy = local.Security.Userid;
    var createdTimestamp = Now();
    var changedInd = local.PersonProgram.ChangedInd ?? "";
    var changeDate = local.Zero.Date;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.PersonProgram.Populated = false;
    Update("CreatePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "closureReason", closureReason);
        db.SetNullableDate(command, "assignedDate", assignedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", createdTimestamp);
        db.SetNullableString(command, "changedInd", changedInd);
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", null);
        db.SetNullableString(command, "medType", "");
      });

    entities.PersonProgram.CspNumber = cspNumber;
    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.Status = "";
    entities.PersonProgram.ClosureReason = closureReason;
    entities.PersonProgram.AssignedDate = assignedDate;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.CreatedBy = createdBy;
    entities.PersonProgram.CreatedTimestamp = createdTimestamp;
    entities.PersonProgram.LastUpdatedBy = createdBy;
    entities.PersonProgram.LastUpdatdTstamp = createdTimestamp;
    entities.PersonProgram.ChangedInd = changedInd;
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.PersonProgram.MedTypeDiscontinueDate = null;
    entities.PersonProgram.MedType = "";
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

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    private Security2 security;
    private PersonProgram personProgram;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Program program;
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    /// <summary>
    /// A value of UpdateCsePersonAccount.
    /// </summary>
    [JsonPropertyName("updateCsePersonAccount")]
    public Common UpdateCsePersonAccount
    {
      get => updateCsePersonAccount ??= new();
      set => updateCsePersonAccount = value;
    }

    private Security2 security;
    private DateWorkArea zero;
    private DateWorkArea current;
    private PersonProgram personProgram;
    private DateWorkArea max;
    private CsePersonAccount csePersonAccount;
    private Common updateCsePersonAccount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private PersonProgram personProgram;
    private Program program;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
