// Program: FN_B717_PROG_HIERARCHY_4_PERSON, ID: 373350657, model: 746.
// Short name: SWE03021
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_PROG_HIERARCHY_4_PERSON.
/// </summary>
[Serializable]
public partial class FnB717ProgHierarchy4Person: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_PROG_HIERARCHY_4_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717ProgHierarchy4Person(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717ProgHierarchy4Person.
  /// </summary>
  public FnB717ProgHierarchy4Person(IContext context, Import import,
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
    foreach(var item in ReadPersonProgramProgram())
    {
      switch(TrimEnd(entities.Program.Code))
      {
        case "AF":
          local.Af.Flag = "Y";

          goto ReadEach;
        case "FC":
          local.Fc.Flag = "Y";

          break;
        case "NF":
          local.Nf.Flag = "Y";

          break;
        case "NC":
          local.Nc.Flag = "Y";

          break;
        case "NA":
          local.Na.Flag = "Y";

          break;
        case "CC":
          local.Na.Flag = "Y";

          break;
        case "FS":
          local.Na.Flag = "Y";

          break;
        case "MP":
          local.Na.Flag = "Y";

          break;
        case "MS":
          local.Na.Flag = "Y";

          break;
        case "MA":
          local.Na.Flag = "Y";

          break;
        case "MK":
          local.Na.Flag = "Y";

          break;
        case "CI":
          local.Na.Flag = "Y";

          break;
        case "SI":
          local.Na.Flag = "Y";

          break;
        case "AFI":
          local.Afi.Flag = "Y";

          break;
        case "FCI":
          local.Afi.Flag = "Y";

          break;
        case "MAI":
          local.Nai.Flag = "Y";

          break;
        case "NAI":
          local.Nai.Flag = "Y";

          break;
        default:
          break;
      }
    }

ReadEach:

    if (AsChar(local.Af.Flag) == 'Y')
    {
      export.Program.Code = "AF";
      export.Program.SystemGeneratedIdentifier = 1;
    }
    else if (AsChar(local.Fc.Flag) == 'Y')
    {
      export.Program.Code = "FC";
      export.Program.SystemGeneratedIdentifier = 5;
    }
    else if (AsChar(local.Nf.Flag) == 'Y')
    {
      export.Program.Code = "NF";
      export.Program.SystemGeneratedIdentifier = 6;
    }
    else if (AsChar(local.Nc.Flag) == 'Y')
    {
      export.Program.Code = "NC";
      export.Program.SystemGeneratedIdentifier = 7;
    }
    else if (AsChar(local.Na.Flag) == 'Y')
    {
      UseFnB717CheckPriorAf();

      if (AsChar(local.Prior.Flag) == 'N')
      {
        export.Program.Code = "NA";
        export.Program.SystemGeneratedIdentifier = 8;

        return;
      }

      local.DebtSearch.Code = "AF";
      UseFnB717CheckDebtsOwed();

      if (AsChar(local.DebtOwed.Flag) == 'Y')
      {
        export.Program.Code = "NAS";
        export.Program.SystemGeneratedIdentifier = 3;
      }
      else
      {
        export.Program.Code = "NAN";
        export.Program.SystemGeneratedIdentifier = 4;
      }
    }
    else if (AsChar(local.Afi.Flag) == 'Y' || AsChar(local.Nai.Flag) == 'Y')
    {
      local.DebtSearch.Code = "KS";
      UseFnB717CheckDebtsOwed();

      if (AsChar(local.DebtOwed.Flag) == 'Y')
      {
        export.Program.Code = "IKS";
        export.Program.SystemGeneratedIdentifier = 11;
      }
      else if (AsChar(local.Afi.Flag) == 'Y')
      {
        export.Program.Code = "AFI";
        export.Program.SystemGeneratedIdentifier = 10;
      }
      else
      {
        export.Program.Code = "NAI";
        export.Program.SystemGeneratedIdentifier = 9;
      }
    }
    else
    {
      local.DebtSearch.Code = "NA";
      UseFnB717CheckDebtsOwed();

      if (AsChar(local.DebtOwed.Flag) == 'Y')
      {
        UseFnB717CheckPriorAf();

        if (AsChar(local.Prior.Flag) == 'N')
        {
          export.Program.Code = "NA";
          export.Program.SystemGeneratedIdentifier = 8;

          return;
        }

        local.DebtSearch.Code = "AF";
        UseFnB717CheckDebtsOwed();

        if (AsChar(local.DebtOwed.Flag) == 'Y')
        {
          export.Program.Code = "NAS";
          export.Program.SystemGeneratedIdentifier = 3;
        }
        else
        {
          export.Program.Code = "NAN";
          export.Program.SystemGeneratedIdentifier = 4;
        }
      }
      else
      {
        UseFnB717CheckPriorAf();

        if (AsChar(local.Prior.Flag) == 'Y')
        {
          export.Program.Code = "XA";
          export.Program.SystemGeneratedIdentifier = 2;

          return;
        }

        UseFnB717CheckPriorAfi();

        if (AsChar(local.Prior.Flag) == 'Y')
        {
          export.Program.Code = "PAF";
          export.Program.SystemGeneratedIdentifier = 13;

          return;
        }

        UseFnB717CheckForPriorNai();

        if (AsChar(local.Prior.Flag) == 'Y')
        {
          export.Program.Code = "PNA";
          export.Program.SystemGeneratedIdentifier = 12;

          return;
        }

        UseFnB717CheckIncInterst4Pers();

        switch(AsChar(local.IncomingInterstateCase.Flag))
        {
          case 'A':
            export.Program.Code = "PAF";
            export.Program.SystemGeneratedIdentifier = 13;

            break;
          case 'N':
            export.Program.Code = "PNA";
            export.Program.SystemGeneratedIdentifier = 12;

            break;
          default:
            export.Program.Code = "NA";
            export.Program.SystemGeneratedIdentifier = 8;

            break;
        }
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseFnB717CheckDebtsOwed()
  {
    var useImport = new FnB717CheckDebtsOwed.Import();
    var useExport = new FnB717CheckDebtsOwed.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    useImport.DebtSearch.Code = local.DebtSearch.Code;

    Call(FnB717CheckDebtsOwed.Execute, useImport, useExport);

    local.DebtOwed.Flag = useExport.DebtOwed.Flag;
  }

  private void UseFnB717CheckForPriorNai()
  {
    var useImport = new FnB717CheckForPriorNai.Import();
    var useExport = new FnB717CheckForPriorNai.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(FnB717CheckForPriorNai.Execute, useImport, useExport);

    local.Prior.Flag = useExport.PriorAfi.Flag;
  }

  private void UseFnB717CheckIncInterst4Pers()
  {
    var useImport = new FnB717CheckIncInterst4Pers.Import();
    var useExport = new FnB717CheckIncInterst4Pers.Export();

    useImport.CsePerson.Assign(import.CsePerson);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717CheckIncInterst4Pers.Execute, useImport, useExport);

    local.IncomingInterstateCase.Flag = useExport.IncomingInterstate.Flag;
  }

  private void UseFnB717CheckPriorAf()
  {
    var useImport = new FnB717CheckPriorAf.Import();
    var useExport = new FnB717CheckPriorAf.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(FnB717CheckPriorAf.Execute, useImport, useExport);

    local.Prior.Flag = useExport.PriorAf.Flag;
  }

  private void UseFnB717CheckPriorAfi()
  {
    var useImport = new FnB717CheckPriorAfi.Import();
    var useExport = new FnB717CheckPriorAfi.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(FnB717CheckPriorAfi.Execute, useImport, useExport);

    local.Prior.Flag = useExport.PriorAfi.Flag;
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportEndDate.Date.GetValueOrDefault());
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
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private CsePerson csePerson;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private Program program;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DebtSearch.
    /// </summary>
    [JsonPropertyName("debtSearch")]
    public Program DebtSearch
    {
      get => debtSearch ??= new();
      set => debtSearch = value;
    }

    /// <summary>
    /// A value of DebtOwed.
    /// </summary>
    [JsonPropertyName("debtOwed")]
    public Common DebtOwed
    {
      get => debtOwed ??= new();
      set => debtOwed = value;
    }

    /// <summary>
    /// A value of NoActiveProg.
    /// </summary>
    [JsonPropertyName("noActiveProg")]
    public Common NoActiveProg
    {
      get => noActiveProg ??= new();
      set => noActiveProg = value;
    }

    /// <summary>
    /// A value of Prior.
    /// </summary>
    [JsonPropertyName("prior")]
    public Common Prior
    {
      get => prior ??= new();
      set => prior = value;
    }

    /// <summary>
    /// A value of Nai.
    /// </summary>
    [JsonPropertyName("nai")]
    public Common Nai
    {
      get => nai ??= new();
      set => nai = value;
    }

    /// <summary>
    /// A value of Afi.
    /// </summary>
    [JsonPropertyName("afi")]
    public Common Afi
    {
      get => afi ??= new();
      set => afi = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Common Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Nc.
    /// </summary>
    [JsonPropertyName("nc")]
    public Common Nc
    {
      get => nc ??= new();
      set => nc = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public Common Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public Common Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Common Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of IncomingInterstateCase.
    /// </summary>
    [JsonPropertyName("incomingInterstateCase")]
    public Common IncomingInterstateCase
    {
      get => incomingInterstateCase ??= new();
      set => incomingInterstateCase = value;
    }

    private Program debtSearch;
    private Common debtOwed;
    private Common noActiveProg;
    private Common prior;
    private Common nai;
    private Common afi;
    private Common na;
    private Common nc;
    private Common nf;
    private Common fc;
    private Common af;
    private Common incomingInterstateCase;
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

    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
