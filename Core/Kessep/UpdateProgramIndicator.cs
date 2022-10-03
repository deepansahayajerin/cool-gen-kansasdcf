// Program: UPDATE_PROGRAM_INDICATOR, ID: 371745834, model: 746.
// Short name: SWE01501
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_PROGRAM_INDICATOR.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// This is used to update program indicator.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateProgramIndicator: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PROGRAM_INDICATOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateProgramIndicator(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateProgramIndicator.
  /// </summary>
  public UpdateProgramIndicator(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.HighDate.Date = new DateTime(2099, 12, 31);
    local.CurrDate.Date = Now().Date;
    export.ProgramIndicator.Assign(import.ProgramIndicator);

    if (ReadProgramIndicatorProgram())
    {
      // **** If program effective is current date or in the future            *
      // ***
      // ****      then overlay existing Prgm_ind rec with new data            *
      // ***
      // **** else if program end date in 
      // past
      // 
      // ****
      // ****       then Add 2 new records
      // 
      // ****
      // ****           one with
      // 
      // ****
      // ****              Effective date = program end + 1 day and            *
      // ***
      // ****              End date = current date -1 and                      *
      // ***
      // ****              child_support_retention code = spaces               *
      // ***
      // ****              iv-d fee = spaces
      // 
      // ****
      // ****          second with
      // 
      // ****
      // ****              Effective date = current date and                   *
      // ***
      // ****              End date = Program end date                         *
      // ***
      // ****              child_support_retention code = export prgm_ind      *
      // ***
      // ****              iv-d fee = export prgm_ind                          *
      // ***
      // ****       else
      // 
      // ****
      // ****       then Add 1 new record and update 1 record                  *
      // ***
      // ****          update existing 
      // record by
      // 
      // ****
      // ****              End date = current date -1                          *
      // ***
      // ****          Add record with
      // 
      // ****
      // ****              Effective date = current date and                   *
      // ***
      // ****              End date = Program end date                         *
      // ***
      // ****              child_support_retention code = export prgm_ind      *
      // ***
      // ****              iv-d fee = export prgm_ind                          *
      // ***
      local.DateOnly.Flag = "Y";

      if (AsChar(entities.ProgramIndicator.IvDFeeIndicator) != AsChar
        (import.ProgramIndicator.IvDFeeIndicator))
      {
        local.DateOnly.Flag = "N";
      }
      else if (!Equal(entities.ProgramIndicator.ChildSupportRetentionCode,
        import.ProgramIndicator.ChildSupportRetentionCode))
      {
        local.DateOnly.Flag = "N";
      }

      if (!Lt(entities.Program.EffectiveDate, local.CurrDate.Date))
      {
        // **** If effective date in the future just correct the existing record
        // ****
        try
        {
          CreateProgramIndicator3();

          if (ReadProgramIndicatorProgram())
          {
            DeleteProgramIndicator();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (ReadProgramIndicatorProgram())
              {
                try
                {
                  UpdateProgramIndicator1();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "PROGRAM_INDICATOR_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "PROGRAM_INDICATOR_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_INDICATOR_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (!Equal(import.Program.DiscontinueDate,
        entities.Program.DiscontinueDate))
      {
        // ****  End date has been changed ****
        if (Lt(entities.Program.DiscontinueDate,
          AddDays(local.CurrDate.Date, -1)))
        {
          // **** ADD A BLANK HISTORY REC TO INDICATE A PERIOD OF IN-ACTIVE TIME
          // ****
          local.ProgramIndicator.EffectiveDate =
            AddDays(entities.ProgramIndicator.DiscontinueDate, 1);
          local.ProgramIndicator.DiscontinueDate =
            AddDays(local.CurrDate.Date, -1);
          local.ProgramIndicator.ChildSupportRetentionCode = "";
          local.ProgramIndicator.IvDFeeIndicator = "";

          try
          {
            CreateProgramIndicator2();
            local.ProgramIndicator.Assign(import.ProgramIndicator);
            local.ProgramIndicator.EffectiveDate = local.CurrDate.Date;
            local.ProgramIndicator.DiscontinueDate =
              import.Program.DiscontinueDate;

            try
            {
              CreateProgramIndicator2();
              export.ProgramIndicator.Assign(local.ProgramIndicator);
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (Equal(entities.Program.DiscontinueDate,
          AddDays(local.CurrDate.Date, -1)))
        {
          if (AsChar(local.DateOnly.Flag) == 'Y')
          {
            // ****  IF END DATE ONLY CHANGE JUST UPDATE END DATE ON THE CURRENT
            // RECORD ****
            try
            {
              UpdateProgramIndicator1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

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
            local.ProgramIndicator.Assign(import.ProgramIndicator);
            local.ProgramIndicator.EffectiveDate = local.CurrDate.Date;
            local.ProgramIndicator.DiscontinueDate =
              import.Program.DiscontinueDate;

            try
            {
              CreateProgramIndicator2();
              export.ProgramIndicator.Assign(local.ProgramIndicator);
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

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
          // ****  End date GE CURRENT DATE ****
          // ****  End date current prgm ind and create new prgm inf ****
          if (AsChar(local.DateOnly.Flag) == 'Y')
          {
            // **** JUST CHANGE THE END DATE  ****
            try
            {
              UpdateProgramIndicator1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else if (Equal(entities.ProgramIndicator.EffectiveDate,
            local.CurrDate.Date))
          {
            try
            {
              UpdateProgramIndicator1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

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
            local.ProgramIndicator.DiscontinueDate =
              AddDays(local.CurrDate.Date, -1);

            try
            {
              UpdateProgramIndicator3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            local.ProgramIndicator.Assign(import.ProgramIndicator);
            local.ProgramIndicator.EffectiveDate = local.CurrDate.Date;
            local.ProgramIndicator.DiscontinueDate =
              import.Program.DiscontinueDate;

            try
            {
              CreateProgramIndicator1();
              export.ProgramIndicator.Assign(local.ProgramIndicator);
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PROGRAM_INDICATOR_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PROGRAM_INDICATOR_PV";

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
      else if (Equal(entities.Program.EffectiveDate,
        import.Program.EffectiveDate) && Equal
        (entities.Program.DiscontinueDate, import.Program.DiscontinueDate))
      {
        // **** IV-D-FEE OR CHILD-SUPPORT_RETENTION CHANGE ONLY   ****
        local.ProgramIndicator.Assign(import.ProgramIndicator);
        local.ProgramIndicator.DiscontinueDate =
          AddDays(local.CurrDate.Date, -1);

        if (Equal(entities.ProgramIndicator.EffectiveDate, local.CurrDate.Date))
        {
          // **** UPDATE THE  IV-D-FEE OR CHILD-SUPPORT_RETENTION CHANGE ONLY   
          // ****
          try
          {
            UpdateProgramIndicator1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (Equal(entities.ProgramIndicator.EffectiveDate,
          AddDays(local.CurrDate.Date, -1)))
        {
          local.ProgramIndicator.Assign(entities.ProgramIndicator);
          local.ProgramIndicator.DiscontinueDate =
            entities.ProgramIndicator.EffectiveDate;

          try
          {
            UpdateProgramIndicator2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.ProgramIndicator.Assign(import.ProgramIndicator);
          local.ProgramIndicator.EffectiveDate = local.CurrDate.Date;
          local.ProgramIndicator.DiscontinueDate =
            import.Program.DiscontinueDate;

          try
          {
            CreateProgramIndicator2();
            export.ProgramIndicator.Assign(entities.ProgramIndicator);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

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
          local.ProgramIndicator.Assign(entities.ProgramIndicator);
          local.ProgramIndicator.DiscontinueDate =
            AddDays(local.CurrDate.Date, -1);

          try
          {
            UpdateProgramIndicator4();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.ProgramIndicator.Assign(import.ProgramIndicator);
          local.ProgramIndicator.EffectiveDate = local.CurrDate.Date;
          local.ProgramIndicator.DiscontinueDate =
            import.Program.DiscontinueDate;

          try
          {
            CreateProgramIndicator1();
            export.ProgramIndicator.Assign(local.ProgramIndicator);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PROGRAM_INDICATOR_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_INDICATOR_PV";

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
  }

  private void CreateProgramIndicator1()
  {
    var childSupportRetentionCode =
      import.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = import.ProgramIndicator.IvDFeeIndicator;
    var effectiveDate = local.ProgramIndicator.EffectiveDate;
    var discontinueDate = local.ProgramIndicator.DiscontinueDate;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.ProgramIndicator.Populated = false;
    Update("CreateProgramIndicator1",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.EffectiveDate = effectiveDate;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.PrgGeneratedId = prgGeneratedId;
    entities.ProgramIndicator.Populated = true;
  }

  private void CreateProgramIndicator2()
  {
    var childSupportRetentionCode =
      local.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = local.ProgramIndicator.IvDFeeIndicator;
    var effectiveDate = local.ProgramIndicator.EffectiveDate;
    var discontinueDate = local.ProgramIndicator.DiscontinueDate;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.ProgramIndicator.Populated = false;
    Update("CreateProgramIndicator2",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.EffectiveDate = effectiveDate;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.PrgGeneratedId = prgGeneratedId;
    entities.ProgramIndicator.Populated = true;
  }

  private void CreateProgramIndicator3()
  {
    var childSupportRetentionCode =
      import.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = import.ProgramIndicator.IvDFeeIndicator;
    var effectiveDate = import.Program.EffectiveDate;
    var discontinueDate = import.Program.DiscontinueDate;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.ProgramIndicator.Populated = false;
    Update("CreateProgramIndicator3",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.EffectiveDate = effectiveDate;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.PrgGeneratedId = prgGeneratedId;
    entities.ProgramIndicator.Populated = true;
  }

  private void DeleteProgramIndicator()
  {
    Update("DeleteProgramIndicator",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });
  }

  private bool ReadProgramIndicatorProgram()
  {
    entities.Program.Populated = false;
    entities.ProgramIndicator.Populated = false;

    return Read("ReadProgramIndicatorProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "programId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ProgramIndicator.ChildSupportRetentionCode =
          db.GetString(reader, 0);
        entities.ProgramIndicator.IvDFeeIndicator = db.GetString(reader, 1);
        entities.ProgramIndicator.EffectiveDate = db.GetDate(reader, 2);
        entities.ProgramIndicator.DiscontinueDate = db.GetDate(reader, 3);
        entities.ProgramIndicator.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.EffectiveDate = db.GetDate(reader, 5);
        entities.Program.DiscontinueDate = db.GetDate(reader, 6);
        entities.Program.Populated = true;
        entities.ProgramIndicator.Populated = true;
      });
  }

  private void UpdateProgramIndicator1()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramIndicator.Populated);

    var childSupportRetentionCode =
      import.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = import.ProgramIndicator.IvDFeeIndicator;
    var discontinueDate = import.Program.DiscontinueDate;

    entities.ProgramIndicator.Populated = false;
    Update("UpdateProgramIndicator1",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.Populated = true;
  }

  private void UpdateProgramIndicator2()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramIndicator.Populated);

    var childSupportRetentionCode =
      local.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = local.ProgramIndicator.IvDFeeIndicator;
    var discontinueDate = local.ProgramIndicator.DiscontinueDate;

    entities.ProgramIndicator.Populated = false;
    Update("UpdateProgramIndicator2",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.Populated = true;
  }

  private void UpdateProgramIndicator3()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramIndicator.Populated);

    var discontinueDate = local.ProgramIndicator.DiscontinueDate;

    entities.ProgramIndicator.Populated = false;
    Update("UpdateProgramIndicator3",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });

    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.Populated = true;
  }

  private void UpdateProgramIndicator4()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramIndicator.Populated);

    var childSupportRetentionCode =
      local.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = local.ProgramIndicator.IvDFeeIndicator;
    var discontinueDate = local.ProgramIndicator.DiscontinueDate;

    entities.ProgramIndicator.Populated = false;
    Update("UpdateProgramIndicator4",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(
          command, "effectiveDate",
          entities.ProgramIndicator.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.ProgramIndicator.PrgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.Populated = true;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    private Program program;
    private ProgramIndicator programIndicator;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    private ProgramIndicator programIndicator;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateOnly.
    /// </summary>
    [JsonPropertyName("dateOnly")]
    public Common DateOnly
    {
      get => dateOnly ??= new();
      set => dateOnly = value;
    }

    /// <summary>
    /// A value of CurrDate.
    /// </summary>
    [JsonPropertyName("currDate")]
    public DateWorkArea CurrDate
    {
      get => currDate ??= new();
      set => currDate = value;
    }

    /// <summary>
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    private Common dateOnly;
    private DateWorkArea currDate;
    private DateWorkArea highDate;
    private ProgramIndicator programIndicator;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
    }

    private Program program;
    private ProgramIndicator programIndicator;
  }
#endregion
}
