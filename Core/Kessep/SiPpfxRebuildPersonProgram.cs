// Program: SI_PPFX_REBUILD_PERSON_PROGRAM, ID: 372919355, model: 746.
// Short name: SWE02802
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_PPFX_REBUILD_PERSON_PROGRAM.
/// </summary>
[Serializable]
public partial class SiPpfxRebuildPersonProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PPFX_REBUILD_PERSON_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPpfxRebuildPersonProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPpfxRebuildPersonProgram.
  /// </summary>
  public SiPpfxRebuildPersonProgram(IContext context, Import import,
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
    //   Date      Developer    Request #          Description
    // 10/19/1999  C. Ott       PR # 73777         Initial Development
    // ----------------------------------------------------------
    // **************************************************************
    // 11/15/99   C. Ott   Added ELSE IF condition to prevent 'CC' program 
    // history from being deleted.  This was added because the current load
    // history process does not retrieve KSC history.  This will be removed when
    // the load history process is corrected.
    // **************************************************************
    // **************************************************************
    // 08/15/00   M. Lachowicz Do not close NA program
    // if exists AF with open WT or EM.  PR #88801.
    // **************************************************************
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    local.Current.Date = Now().Date;

    // 08/15/00 M.L Start
    UseCabSetMaximumDiscontinueDate();

    // 08/15/00 M.L End
    foreach(var item in ReadPersonProgramProgram())
    {
      if (entities.Program.SystemGeneratedIdentifier == 12 || entities
        .Program.SystemGeneratedIdentifier == 13 || entities
        .Program.SystemGeneratedIdentifier == 14 || entities
        .Program.SystemGeneratedIdentifier == 16 || entities
        .Program.SystemGeneratedIdentifier == 17 || entities
        .Program.SystemGeneratedIdentifier == 18)
      {
        // 08/15/00 M.L Start
        if (entities.Program.SystemGeneratedIdentifier == 12 && AsChar
          (local.OpenNaProgram.Flag) != 'Y')
        {
          MovePersonProgram(entities.PersonProgram, local.ExistingNa);
          local.OpenNaProgram.Flag = "Y";
        }

        // 08/15/00 M.L End
        continue;
      }
      else if (entities.Program.SystemGeneratedIdentifier == 5 && Lt
        (entities.PersonProgram.DiscontinueDate, local.Current.Date))
      {
        continue;
      }
      else
      {
        UseSiPeprDeletePersonProgram();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }
    }

    // ---------------------------------------------
    // Check to see if the CSE person is known to the CSE system.
    // Non-case related CSE person (CSE flag = 'N')
    // Case related CSE person (CSE flag = 'Y')
    // Unknown CSE Person (CSE flag = spaces)
    // If known to CSE no action is necessary.
    // If not known (CSE flag = spaces) change in ADABAS
    // to a case related CSE person.
    // If Non-case related this is an error.
    // ---------------------------------------------
    local.ErrOnAdabasUnavail.Flag = "Y";
    UseCabReadAdabasPerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.Cse.Flag) == 'Y')
    {
      // ---------------------------------------------
      // CSE person is known to the CSE system.
      // ---------------------------------------------
    }
    else
    {
      // ---------------------------------------------
      // CSE person is either unknown in ADABAS or is NON-case related.
      // Update the ADABAS record with the unique key
      // set to spaces to indicate that the person is now
      // a CASE related person.
      // ---------------------------------------------
      local.CsePersonsWorkSet.UniqueKey = "";
      UseSiAltsCabUpdateAlias();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabRollbackCics();

        return;
      }
    }

    // -------------------------------------------------------
    // 	Retreive the person program history from ADABAS
    // -------------------------------------------------------
    UseSiRegiCopyAdabasPersonPgms();

    if (IsExitState("CHILD_PERSON_PROG_NF"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // 08/15/00 M.L Start
    local.AllOpenedPrograms.Flag = "Y";

    foreach(var item in ReadPersonProgram6())
    {
      local.AllOpenedPrograms.Flag = "N";
      ++local.AllOpenedPrograms.Count;
    }

    if (local.AllOpenedPrograms.Count > 1)
    {
      if (ReadPersonProgram2())
      {
        DeletePersonProgram1();
      }

      return;
    }

    if (local.AllOpenedPrograms.Count == 1)
    {
      if (ReadPersonProgram3())
      {
        if (ReadPersonProgram4())
        {
          if (Equal(entities.OpenNa.EffectiveDate,
            AddDays(entities.PersonProgram.DiscontinueDate, 1)))
          {
          }
          else
          {
            local.New1.EffectiveDate =
              AddDays(entities.PersonProgram.DiscontinueDate, 1);
            DeletePersonProgram2();
            local.Program.Code = "NA";
            UseSiPeprCreatePersonProgram();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
          }
        }
      }

      return;
    }

    local.WtEmPrograms.Flag = "N";

    if (ReadPersonProgram5())
    {
      local.WtEmPrograms.Flag = "Y";
      local.New1.EffectiveDate =
        AddDays(entities.PersonProgram.DiscontinueDate, 1);
    }

    if (AsChar(local.WtEmPrograms.Flag) == 'N')
    {
      return;
    }

    if (ReadPersonProgram1())
    {
      DeletePersonProgram1();
    }

    local.Program.Code = "NA";
    UseSiPeprCreatePersonProgram();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabRollbackCics();
    }

    // 08/15/00 M.L End
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavail.Flag;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.Cse.Flag = useExport.Cse.Flag;
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiPeprCreatePersonProgram()
  {
    var useImport = new SiPeprCreatePersonProgram.Import();
    var useExport = new SiPeprCreatePersonProgram.Export();

    useImport.PersonProgram.EffectiveDate = local.New1.EffectiveDate;
    useImport.Program.Code = local.Program.Code;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiPeprCreatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprDeletePersonProgram()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    MoveProgram(entities.Program, useImport.Program);
    useImport.PersonProgram.Assign(entities.PersonProgram);
    useImport.RecomputeDistribution.Flag = import.RecomputeDistribution.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiRegiCopyAdabasPersonPgms()
  {
    var useImport = new SiRegiCopyAdabasPersonPgms.Import();
    var useExport = new SiRegiCopyAdabasPersonPgms.Export();

    useImport.RecomputeDistribution.Flag = import.RecomputeDistribution.Flag;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CseReferralReceived.Date = local.Current.Date;

    Call(SiRegiCopyAdabasPersonPgms.Execute, useImport, useExport);
  }

  private void DeletePersonProgram1()
  {
    Update("DeletePersonProgram1",
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

  private void DeletePersonProgram2()
  {
    Update("DeletePersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.OpenNa.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OpenNa.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "prgGeneratedId", entities.OpenNa.PrgGeneratedId);
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ExistingNa.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "medTypeDiscDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram4()
  {
    entities.OpenNa.Populated = false;

    return Read("ReadPersonProgram4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.OpenNa.CspNumber = db.GetString(reader, 0);
        entities.OpenNa.EffectiveDate = db.GetDate(reader, 1);
        entities.OpenNa.ClosureReason = db.GetNullableString(reader, 2);
        entities.OpenNa.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.OpenNa.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.OpenNa.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.OpenNa.MedTypeDiscontinueDate = db.GetNullableDate(reader, 6);
        entities.OpenNa.MedType = db.GetNullableString(reader, 7);
        entities.OpenNa.Populated = true;
      });
  }

  private bool ReadPersonProgram5()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "medTypeDiscDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram6()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 7);
        entities.Program.Code = db.GetString(reader, 8);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
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
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
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

    private Common recomputeDistribution;
    private CsePerson csePerson;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PersonProgram New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of EffectiveDate.
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    public DateWorkArea EffectiveDate
    {
      get => effectiveDate ??= new();
      set => effectiveDate = value;
    }

    /// <summary>
    /// A value of WtEmPrograms.
    /// </summary>
    [JsonPropertyName("wtEmPrograms")]
    public Common WtEmPrograms
    {
      get => wtEmPrograms ??= new();
      set => wtEmPrograms = value;
    }

    /// <summary>
    /// A value of AllOpenedPrograms.
    /// </summary>
    [JsonPropertyName("allOpenedPrograms")]
    public Common AllOpenedPrograms
    {
      get => allOpenedPrograms ??= new();
      set => allOpenedPrograms = value;
    }

    /// <summary>
    /// A value of OpenNaProgram.
    /// </summary>
    [JsonPropertyName("openNaProgram")]
    public Common OpenNaProgram
    {
      get => openNaProgram ??= new();
      set => openNaProgram = value;
    }

    /// <summary>
    /// A value of ExistingNa.
    /// </summary>
    [JsonPropertyName("existingNa")]
    public PersonProgram ExistingNa
    {
      get => existingNa ??= new();
      set => existingNa = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of ErrOnAdabasUnavail.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavail")]
    public Common ErrOnAdabasUnavail
    {
      get => errOnAdabasUnavail ??= new();
      set => errOnAdabasUnavail = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private PersonProgram new1;
    private Program program;
    private DateWorkArea effectiveDate;
    private Common wtEmPrograms;
    private Common allOpenedPrograms;
    private Common openNaProgram;
    private PersonProgram existingNa;
    private DateWorkArea maxDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errOnAdabasUnavail;
    private Common cse;
    private AbendData abendData;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OpenNa.
    /// </summary>
    [JsonPropertyName("openNa")]
    public PersonProgram OpenNa
    {
      get => openNa ??= new();
      set => openNa = value;
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private PersonProgram openNa;
    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
  }
#endregion
}
