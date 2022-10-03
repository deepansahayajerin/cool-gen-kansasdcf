// Program: OE_DETERMINE_ADC_PGM_PARTICIPATN, ID: 371802162, model: 746.
// Short name: SWE00210
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
/// A program: OE_DETERMINE_ADC_PGM_PARTICIPATN.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Thie common action block determines if a given CSE Person or any member of 
/// the given case participates in an ADC program.
/// </para>
/// </summary>
[Serializable]
public partial class OeDetermineAdcPgmParticipatn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DETERMINE_ADC_PGM_PARTICIPATN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDetermineAdcPgmParticipatn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDetermineAdcPgmParticipatn.
  /// </summary>
  public OeDetermineAdcPgmParticipatn(IContext context, Import import,
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
    if (import.ProgramProcessingInfo.ProcessDate != null)
    {
      local.Current.Date = import.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    // **********************************************************
    // This program should call the CAB 'SI_READ_CASE_PROGRAM_TYPE'
    // to get the right program type.
    // **********************************************************
    if (!IsEmpty(import.Case1.Number))
    {
      UseSiReadCaseProgramType2();

      if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
      {
        return;
      }

      if (Equal(local.Program.Code, "NA") || Equal(local.Program.Code, "NAI"))
      {
        export.CaseInAdcProgram.SelectChar = "N";
      }
      else
      {
        export.CaseInAdcProgram.SelectChar = "Y";
      }
    }
    else
    {
      foreach(var item in ReadCase())
      {
        UseSiReadCaseProgramType1();

        if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
        {
          foreach(var item1 in ReadCaseRolePersonProgramProgram())
          {
            ExitState = "ACO_NN0000_ALL_OK";

            switch(TrimEnd(entities.Program.Code))
            {
              case "NA":
                export.CaseInAdcProgram.SelectChar = "N";

                break;
              case "NAI":
                export.CaseInAdcProgram.SelectChar = "N";

                break;
              default:
                export.CaseInAdcProgram.SelectChar = "Y";

                return;
            }
          }
        }
        else if (Equal(local.Program.Code, "NA") || Equal
          (local.Program.Code, "NAI"))
        {
          export.CaseInAdcProgram.SelectChar = "N";
        }
        else
        {
          export.CaseInAdcProgram.SelectChar = "Y";

          return;
        }
      }
    }
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseSiReadCaseProgramType1()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Existing.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCaseProgramType2()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRolePersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRolePersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Existing.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 4);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 5);
        entities.PersonProgram.CspNumber = db.GetString(reader, 6);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 7);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Program.Code = db.GetString(reader, 11);
        entities.Program.EffectiveDate = db.GetDate(reader, 12);
        entities.Program.DiscontinueDate = db.GetDate(reader, 13);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Case1 case1;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseInAdcProgram.
    /// </summary>
    [JsonPropertyName("caseInAdcProgram")]
    public Common CaseInAdcProgram
    {
      get => caseInAdcProgram ??= new();
      set => caseInAdcProgram = value;
    }

    /// <summary>
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
    }

    private Common caseInAdcProgram;
    private Common medProgExists;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private DateWorkArea current;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingApCaseRole.
    /// </summary>
    [JsonPropertyName("existingApCaseRole")]
    public CaseRole ExistingApCaseRole
    {
      get => existingApCaseRole ??= new();
      set => existingApCaseRole = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingApCsePerson.
    /// </summary>
    [JsonPropertyName("existingApCsePerson")]
    public CsePerson ExistingApCsePerson
    {
      get => existingApCsePerson ??= new();
      set => existingApCsePerson = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private CaseRole existingApCaseRole;
    private Case1 existing;
    private CsePerson existingApCsePerson;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}
