// Program: FN_GET_CASE_COUNTS_PER_PERS_PROG, ID: 371110004, model: 746.
// Short name: SWE02951
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GET_CASE_COUNTS_PER_PERS_PROG.
/// </summary>
[Serializable]
public partial class FnGetCaseCountsPerPersProg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_CASE_COUNTS_PER_PERS_PROG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetCaseCountsPerPersProg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetCaseCountsPerPersProg.
  /// </summary>
  public FnGetCaseCountsPerPersProg(IContext context, Import import,
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
    // --------------------------------------------------
    // Initial Version - 08/2001
    // CAB takes periodic COMMITs but no checkpoints.
    // --------------------------------------------------
    // --------------------------------------------------
    // 8/13/2001
    // Set restart line # to 999 for the dummy checkpoint.
    // --------------------------------------------------
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "PGM";

    // --------------------------------------------------
    // Take a dummy checkpoint here so if job is cancelled,
    // restart will start at this CAB and not duplicate prev lines.
    // --------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "999" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "PGM";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";

        return;
      }
    }

    // --------------------------------------------------
    // Read Open cases as of report end date..
    // --------------------------------------------------
    foreach(var item in ReadCase())
    {
      if (Equal(entities.Case1.Number, local.Prev.Number))
      {
        continue;
      }

      local.Prev.Number = entities.Case1.Number;
      UseFnGetPersonProgramForCase();

      switch(local.ProgramType.Count)
      {
        case 1:
          ++local.Local1.Count;

          break;
        case 2:
          ++local.Local2.Count;

          break;
        case 3:
          ++local.Local3.Count;

          break;
        case 4:
          ++local.Local4.Count;

          break;
        case 5:
          ++local.Local5.Count;

          break;
        case 6:
          ++local.Local6.Count;

          break;
        case 7:
          ++local.Local7.Count;

          break;
        case 8:
          ++local.Local8.Count;

          break;
        case 9:
          ++local.Local9.Count;

          break;
        default:
          break;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.Column =
          NumberToString(local.ProgramType.Count, 1);
        UseFnCreateOcse157Verification();
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() && AsChar
        (import.DisplayInd.Flag) != 'Y')
      {
        local.CommitCnt.Count = 0;
        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";
          local.ForError.LineNumber = "PGM";
          local.ForError.CaseNumber = entities.Ocse157Verification.CaseNumber;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // --------------------------------------------------
    // Processing complete. Create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "PGM";
    local.ForCreateOcse157Data.Column = "1";
    local.ForCreateOcse157Data.Number = local.Local1.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "2";
    local.ForCreateOcse157Data.Number = local.Local2.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "3";
    local.ForCreateOcse157Data.Number = local.Local3.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "4";
    local.ForCreateOcse157Data.Number = local.Local4.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "5";
    local.ForCreateOcse157Data.Number = local.Local5.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "6";
    local.ForCreateOcse157Data.Number = local.Local6.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "7";
    local.ForCreateOcse157Data.Number = local.Local7.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "8";
    local.ForCreateOcse157Data.Number = local.Local8.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "9";
    local.ForCreateOcse157Data.Number = local.Local9.Count;
    UseFnCreateOcse157Data();

    // --------------------------------------------------
    // Now, we are REALLY done. Take final checkpoint!
    // --------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = " " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "PGM";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveOcse157Verification(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnCreateOcse157Data()
  {
    var useImport = new FnCreateOcse157Data.Import();
    var useExport = new FnCreateOcse157Data.Export();

    useImport.Ocse157Data.Assign(local.ForCreateOcse157Data);

    Call(FnCreateOcse157Data.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseFnGetPersonProgramForCase()
  {
    var useImport = new FnGetPersonProgramForCase.Import();
    var useExport = new FnGetPersonProgramForCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(FnGetPersonProgramForCase.Execute, useImport, useExport);

    local.ProgramType.Count = useExport.ProgramType.Count;
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    useImport.Ocse157Verification.Assign(local.ForError);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    private DateWorkArea reportEndDate;
    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common displayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Local8.
    /// </summary>
    [JsonPropertyName("local8")]
    public Common Local8
    {
      get => local8 ??= new();
      set => local8 = value;
    }

    /// <summary>
    /// A value of Local9.
    /// </summary>
    [JsonPropertyName("local9")]
    public Common Local9
    {
      get => local9 ??= new();
      set => local9 = value;
    }

    /// <summary>
    /// A value of Local6.
    /// </summary>
    [JsonPropertyName("local6")]
    public Common Local6
    {
      get => local6 ??= new();
      set => local6 = value;
    }

    /// <summary>
    /// A value of Local7.
    /// </summary>
    [JsonPropertyName("local7")]
    public Common Local7
    {
      get => local7 ??= new();
      set => local7 = value;
    }

    /// <summary>
    /// A value of Local5.
    /// </summary>
    [JsonPropertyName("local5")]
    public Common Local5
    {
      get => local5 ??= new();
      set => local5 = value;
    }

    /// <summary>
    /// A value of Local4.
    /// </summary>
    [JsonPropertyName("local4")]
    public Common Local4
    {
      get => local4 ??= new();
      set => local4 = value;
    }

    /// <summary>
    /// A value of Local3.
    /// </summary>
    [JsonPropertyName("local3")]
    public Common Local3
    {
      get => local3 ??= new();
      set => local3 = value;
    }

    /// <summary>
    /// A value of Local2.
    /// </summary>
    [JsonPropertyName("local2")]
    public Common Local2
    {
      get => local2 ??= new();
      set => local2 = value;
    }

    /// <summary>
    /// A value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public Common Local1
    {
      get => local1 ??= new();
      set => local1 = value;
    }

    /// <summary>
    /// A value of ProgramType.
    /// </summary>
    [JsonPropertyName("programType")]
    public Common ProgramType
    {
      get => programType ??= new();
      set => programType = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Minimum.
    /// </summary>
    [JsonPropertyName("minimum")]
    public Case1 Minimum
    {
      get => minimum ??= new();
      set => minimum = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Verification.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Verification")]
    public Ocse157Verification ForCreateOcse157Verification
    {
      get => forCreateOcse157Verification ??= new();
      set => forCreateOcse157Verification = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private Common local8;
    private Common local9;
    private Common local6;
    private Common local7;
    private Common local5;
    private Common local4;
    private Common local3;
    private Common local2;
    private Common local1;
    private Common programType;
    private Ocse157Data forCreateOcse157Data;
    private Ocse157Data max;
    private Case1 minimum;
    private Common commitCnt;
    private External forCommit;
    private Ocse157Verification forError;
    private Case1 restart;
    private Case1 prev;
    private Ocse157Verification forCreateOcse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private Ocse157Data ocse157Data;
    private Ocse157Verification ocse157Verification;
    private Case1 case1;
    private CaseAssignment caseAssignment;
  }
#endregion
}
