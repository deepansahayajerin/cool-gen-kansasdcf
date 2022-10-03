// Program: SI_PEPR_VALIDATE_PERS_PGM_PERIOD, ID: 371736654, model: 746.
// Short name: SWE01263
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
/// A program: SI_PEPR_VALIDATE_PERS_PGM_PERIOD.
/// </para>
/// <para>
/// This AB reads all the programs that a person is currently on.  The new 
/// person program cannot be open at the same time as a protected program.  Also
/// if the new person program is NAI, it cannot be open at the same time as
/// AFI, FCI, or MAI.
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprValidatePersPgmPeriod: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_VALIDATE_PERS_PGM_PERIOD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprValidatePersPgmPeriod(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprValidatePersPgmPeriod.
  /// </summary>
  public SiPeprValidatePersPgmPeriod(IContext context, Import import,
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
    // 05/08/96  G. Lofton			Initial Development
    // 04/29/97  JeHoward                      Current date fix.
    // ----------------------------------------------------------
    // **************************************************************
    // 09/06/00   M.Lachowicz    WR # 00188.
    //                           Make changes for validation of JJA
    //                           programs.
    // *************************************************************
    local.Current.Date = Now().Date;
    MovePersonProgram(import.ChPersonProgram, export.Ch);

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadProgram())
    {
      if (Lt(entities.Program.DiscontinueDate, local.Current.Date))
      {
        ExitState = "SI0000_PROGRAM_IS_DISCONTINUED";

        return;
      }

      if (Lt(export.Ch.EffectiveDate, entities.Program.EffectiveDate))
      {
        ExitState = "SI0000_EFF_DT_LT_PROGRAM_EFF_DT";

        return;
      }
    }
    else
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    UseCabSetMaximumDiscontinueDate();

    if (export.Ch.DiscontinueDate == null)
    {
      if (Lt(entities.Program.DiscontinueDate, local.Max.Date))
      {
        export.Ch.DiscontinueDate = entities.Program.DiscontinueDate;
      }
      else
      {
        export.Ch.DiscontinueDate = local.Max.Date;
      }
    }

    // 09/06/00 M.L Start
    // Read all programs not only active. Removed clause
    // 'Desired person_program discontinue_date IS GREATER OR EQUAL TO 
    // local_current date_work_area date'
    // 09/06/00 M.L End
    foreach(var item in ReadPersonProgramProgram())
    {
      // 09/06/00 M.L Start
      if (Equal(import.ChPersonProgram.CreatedTimestamp,
        entities.PersonProgram.CreatedTimestamp) && Equal
        (import.ChProgram.Code, entities.Program.Code))
      {
        continue;
      }

      // Add validation for JJA programs NC, NF and FC.
      if (Equal(import.ChProgram.Code, "NC") || Equal
        (import.ChProgram.Code, "FC") || Equal(import.ChProgram.Code, "NF"))
      {
        if (Equal(entities.Program.Code, "NC") || Equal
          (entities.Program.Code, "FC") || Equal
          (entities.Program.Code, "NF") || Equal(entities.Program.Code, "NA"))
        {
          if (!Lt(export.Ch.EffectiveDate, entities.PersonProgram.EffectiveDate) &&
            !
            Lt(entities.PersonProgram.DiscontinueDate, export.Ch.EffectiveDate) ||
            !
            Lt(export.Ch.DiscontinueDate, entities.PersonProgram.EffectiveDate) &&
            !
            Lt(entities.PersonProgram.DiscontinueDate, export.Ch.DiscontinueDate)
            || !
            Lt(entities.PersonProgram.EffectiveDate, export.Ch.EffectiveDate) &&
            !
            Lt(export.Ch.DiscontinueDate, entities.PersonProgram.DiscontinueDate))
            
          {
            ExitState = "SI0000_INVALID_PERSON_PGM_DATES";

            return;
          }
          else
          {
            continue;
          }
        }
      }

      // 09/06/00 M.L End
      if (Equal(import.ChProgram.Code, 1, 2, "NA"))
      {
        if (!Lt(export.Ch.EffectiveDate, entities.PersonProgram.EffectiveDate) &&
          !
          Lt(entities.PersonProgram.DiscontinueDate, export.Ch.EffectiveDate) ||
          !
          Lt(export.Ch.DiscontinueDate, entities.PersonProgram.EffectiveDate) &&
          !
          Lt(entities.PersonProgram.DiscontinueDate, export.Ch.DiscontinueDate) ||
          !
          Lt(entities.PersonProgram.EffectiveDate, export.Ch.EffectiveDate) && !
          Lt(export.Ch.DiscontinueDate, entities.PersonProgram.DiscontinueDate))
        {
          ExitState = "SI0000_INVALID_PERSON_PGM_DATES";

          return;
        }
      }
      else
      {
        // 09/06/00 M.L Start
        if (Equal(entities.Program.Code, "NC") || Equal
          (entities.Program.Code, "FC") || Equal(entities.Program.Code, "NF"))
        {
          continue;
        }

        if (Equal(import.ChProgram.Code, "NC") || Equal
          (import.ChProgram.Code, "FC") || Equal(import.ChProgram.Code, "NF"))
        {
          continue;
        }

        // 09/06/00 M.L End
        if (CharAt(entities.Program.Code, 3) == 'I')
        {
          if (Equal(entities.Program.Code, "NAI"))
          {
            if (!Lt(export.Ch.EffectiveDate,
              entities.PersonProgram.EffectiveDate) && !
              Lt(entities.PersonProgram.DiscontinueDate, export.Ch.EffectiveDate)
              || !
              Lt(export.Ch.DiscontinueDate, entities.PersonProgram.EffectiveDate)
              && !
              Lt(entities.PersonProgram.DiscontinueDate,
              export.Ch.DiscontinueDate) || !
              Lt(entities.PersonProgram.EffectiveDate, export.Ch.EffectiveDate) &&
              !
              Lt(export.Ch.DiscontinueDate,
              entities.PersonProgram.DiscontinueDate))
            {
              ExitState = "SI0000_INVALID_PERSON_PGM_DATES";

              return;
            }
          }
        }
        else if (!Lt(export.Ch.EffectiveDate,
          entities.PersonProgram.EffectiveDate) && !
          Lt(entities.PersonProgram.DiscontinueDate, export.Ch.EffectiveDate) ||
          !
          Lt(export.Ch.DiscontinueDate, entities.PersonProgram.EffectiveDate) &&
          !
          Lt(entities.PersonProgram.DiscontinueDate, export.Ch.DiscontinueDate) ||
          !
          Lt(entities.PersonProgram.EffectiveDate, export.Ch.EffectiveDate) && !
          Lt(export.Ch.DiscontinueDate, entities.PersonProgram.DiscontinueDate))
        {
          // 09/06/00 M.L Start
          if ((Equal(entities.Program.Code, "AF") || Equal
            (entities.Program.Code, "FS") || Equal
            (entities.Program.Code, "MP") || Equal
            (entities.Program.Code, "MA") || Equal
            (entities.Program.Code, "MS") || Equal
            (entities.Program.Code, "MK") || Equal
            (entities.Program.Code, "CI") || Equal
            (entities.Program.Code, "SI") || Equal
            (entities.Program.Code, "CC")) && (
              Equal(import.ChProgram.Code, "AF") || Equal
            (import.ChProgram.Code, "FS") || Equal
            (import.ChProgram.Code, "MP") || Equal
            (import.ChProgram.Code, "MA") || Equal
            (import.ChProgram.Code, "MS") || Equal
            (import.ChProgram.Code, "MK") || Equal
            (import.ChProgram.Code, "CI") || Equal
            (import.ChProgram.Code, "SI") || Equal
            (import.ChProgram.Code, "CC")))
          {
            continue;
          }

          // 09/06/00 M.L End
          ExitState = "SI0000_INVALID_PERSON_PGM_DATES";

          return;
        }
      }
    }
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.ChCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.Program.Populated = true;
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
        db.SetString(command, "code", import.ChProgram.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.EffectiveDate = db.GetDate(reader, 2);
        entities.Program.DiscontinueDate = db.GetDate(reader, 3);
        entities.Program.Populated = true;
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
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChPersonProgram.
    /// </summary>
    [JsonPropertyName("chPersonProgram")]
    public PersonProgram ChPersonProgram
    {
      get => chPersonProgram ??= new();
      set => chPersonProgram = value;
    }

    /// <summary>
    /// A value of ChProgram.
    /// </summary>
    [JsonPropertyName("chProgram")]
    public Program ChProgram
    {
      get => chProgram ??= new();
      set => chProgram = value;
    }

    private CsePersonsWorkSet chCsePersonsWorkSet;
    private PersonProgram chPersonProgram;
    private Program chProgram;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public PersonProgram Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    private PersonProgram ch;
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

    private DateWorkArea current;
    private PersonProgram personProgram;
    private DateWorkArea max;
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

    private CsePerson csePerson;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
