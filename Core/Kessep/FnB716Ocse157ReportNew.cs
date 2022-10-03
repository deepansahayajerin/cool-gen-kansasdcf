// Program: FN_B716_OCSE157_REPORT_NEW, ID: 371092610, model: 746.
// Short name: SWEF716B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B716_OCSE157_REPORT_NEW.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB716Ocse157ReportNew: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B716_OCSE157_REPORT_NEW program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB716Ocse157ReportNew(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB716Ocse157ReportNew.
  /// </summary>
  public FnB716Ocse157ReportNew(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 05/??/01  KDoshi			Initial Development
    // 09/17/01				View map abort flag on one of the USE statements.
    // 09/19/01				Add Case Universe logic.
    // 10/30/01				Split lines 5 and 7.
    // 03/09/06  GVandy	WR00230751	Federally mandated changes.
    // 					The following new lines were added: 3, 8, 33, 34, 35, 36.
    // 					The following lines where changed: 1, 2, 5, 7, 9a, 18,
    // 					19, 20, 21, 22, 23, 24, 29, 37, 38
    // 02/04/20  GVandy	CQ66220		Federally mandated changes for FY 2022.
    // 					The following lines were changed: 1a, 1b, 2a, 2b, 19,
    // 					20, 25, 27, 29, 33, and 34.
    // 					Report labels were changed in SRC15203 for the
    // 					following lines: 2c, 18a, 22, 23, 25, 27, 33, 34, 35.
    // ---------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEFB716";
    UseFnOcse157BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      UseFnOcse157GetRestartLineNbr();
    }

    if (!Lt("01", local.Restart.LineNumber) && !
      Lt("01", local.From.LineNumber) && !Lt(local.To.LineNumber, "01"))
    {
      UseFnOcse157Line1();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("02", local.Restart.LineNumber) && !
      Lt("02", local.From.LineNumber) && !Lt(local.To.LineNumber, "02"))
    {
      UseFnOcse157Line2();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("03", local.Restart.LineNumber) && !
      Lt("03", local.From.LineNumber) && !Lt(local.To.LineNumber, "03"))
    {
      UseFnOcse157Line3();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("04", local.Restart.LineNumber) && !
      Lt("04", local.From.LineNumber) && !Lt(local.To.LineNumber, "04"))
    {
      UseFnOcse157Line4();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("05", local.Restart.LineNumber) && !
      Lt("05", local.From.LineNumber) && !Lt(local.To.LineNumber, "05"))
    {
      UseFnOcse157Line5();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("06", local.Restart.LineNumber) && !
      Lt("06", local.From.LineNumber) && !Lt(local.To.LineNumber, "06"))
    {
      UseFnOcse157Line6();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("07", local.Restart.LineNumber) && !
      Lt("07", local.From.LineNumber) && !Lt(local.To.LineNumber, "07"))
    {
      UseFnOcse157Line7();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("08", local.Restart.LineNumber) && !
      Lt("08", local.From.LineNumber) && !Lt(local.To.LineNumber, "08"))
    {
      UseFnOcse157Line8();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("09", local.Restart.LineNumber) && !
      Lt("09", local.From.LineNumber) && !Lt(local.To.LineNumber, "09"))
    {
      UseFnOcse157Line9A();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("12", local.Restart.LineNumber) && !
      Lt("12", local.From.LineNumber) && !Lt(local.To.LineNumber, "12"))
    {
      UseFnOcse157Line12();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("13", local.Restart.LineNumber) && !
      Lt("13", local.From.LineNumber) && !Lt(local.To.LineNumber, "13"))
    {
      UseFnOcse157Line13();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("14", local.Restart.LineNumber) && !
      Lt("14", local.From.LineNumber) && !Lt(local.To.LineNumber, "14"))
    {
      UseFnOcse157Line14();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("16", local.Restart.LineNumber) && !
      Lt("16", local.From.LineNumber) && !Lt(local.To.LineNumber, "16"))
    {
      UseFnOcse157Line16();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("17", local.Restart.LineNumber) && !
      Lt("17", local.From.LineNumber) && !Lt(local.To.LineNumber, "17"))
    {
      UseFnOcse157Line17();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("18", local.Restart.LineNumber) && !
      Lt("18", local.From.LineNumber) && !Lt(local.To.LineNumber, "18"))
    {
      UseFnOcse157Line18();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("19", local.Restart.LineNumber) && !
      Lt("19", local.From.LineNumber) && !Lt(local.To.LineNumber, "19"))
    {
      UseFnOcse157Line19();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("20", local.Restart.LineNumber) && !
      Lt("20", local.From.LineNumber) && !Lt(local.To.LineNumber, "20"))
    {
      UseFnOcse157Line20();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("21", local.Restart.LineNumber) && !
      Lt("21", local.From.LineNumber) && !Lt(local.To.LineNumber, "21"))
    {
      UseFnOcse157Line212223();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("24", local.Restart.LineNumber) && !
      Lt("24", local.From.LineNumber) && !Lt(local.To.LineNumber, "24"))
    {
      UseFnOcse157Line24();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("25", local.Restart.LineNumber) && !
      Lt("25", local.From.LineNumber) && !Lt(local.To.LineNumber, "25"))
    {
      UseFnOcse157Line25();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("26", local.Restart.LineNumber) && !
      Lt("26", local.From.LineNumber) && !Lt(local.To.LineNumber, "26"))
    {
      UseFnOcse157Line26();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("27", local.Restart.LineNumber) && !
      Lt("27", local.From.LineNumber) && !Lt(local.To.LineNumber, "27"))
    {
      UseFnOcse157Line27();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("28", local.Restart.LineNumber) && !
      Lt("28", local.From.LineNumber) && !Lt(local.To.LineNumber, "28"))
    {
      UseFnOcse157Line28();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("29", local.Restart.LineNumber) && !
      Lt("29", local.From.LineNumber) && !Lt(local.To.LineNumber, "29"))
    {
      UseFnOcse157Line29();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("33", local.Restart.LineNumber) && !
      Lt("33", local.From.LineNumber) && !Lt(local.To.LineNumber, "33"))
    {
      UseFnOcse157Line3334();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("35", local.Restart.LineNumber) && !
      Lt("35", local.From.LineNumber) && !Lt(local.To.LineNumber, "35"))
    {
      UseFnOcse157Line35();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("36", local.Restart.LineNumber) && !
      Lt("36", local.From.LineNumber) && !Lt(local.To.LineNumber, "36"))
    {
      UseFnOcse157Line36();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("37", local.Restart.LineNumber) && !
      Lt("37", local.From.LineNumber) && !Lt(local.To.LineNumber, "37"))
    {
      UseFnOcse157Line37();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("38", local.Restart.LineNumber) && !
      Lt("38", local.From.LineNumber) && !Lt(local.To.LineNumber, "38"))
    {
      UseFnOcse157Line38New();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (AsChar(local.CaseUniverse.Flag) == 'Y' && !
      Lt("888", local.Restart.LineNumber))
    {
      UseFnOcse157CaseUniverse();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (AsChar(local.GetCaseCountPerProg.Flag) == 'Y')
    {
      UseFnGetCaseCountsPerPersProg();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    // --------------------------------------------------
    // Processing complete!!!!!
    // Take checkpoint.
    // --------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = " " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "PR";
      local.ForError.CaseNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    UseFnOcse157CloseFiles();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseFnGetCaseCountsPerPersProg()
  {
    var useImport = new FnGetCaseCountsPerPersProg.Import();
    var useExport = new FnGetCaseCountsPerPersProg.Export();

    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnGetCaseCountsPerPersProg.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157BatchInitialization()
  {
    var useImport = new FnOcse157BatchInitialization.Import();
    var useExport = new FnOcse157BatchInitialization.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(FnOcse157BatchInitialization.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.CalendarYearStart, local.CalendarYrStartDate);
    MoveDateWorkArea(useExport.CalendarYearEnd, local.CalendarYearEndDate);
    local.From.Assign(useExport.From);
    local.To.Assign(useExport.To);
    MoveOcse157Verification1(useExport.Ocse157Verification, local.Restart);
    MoveDateWorkArea(useExport.ReportEndDate, local.ReportEndDate);
    MoveDateWorkArea(useExport.ReportStartDate, local.ReportStartDate);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.GetCaseCountPerProg.Flag = useExport.GetCaseCountPerPepr.Flag;
    local.CaseUniverse.Flag = useExport.CaseUniverse.Flag;
    local.Line8B.Number = useExport.Line8B.Number;
    local.Cq66220EffectiveFy.FiscalYear =
      useExport.Cq66220EffectiveFy.FiscalYear;
  }

  private void UseFnOcse157CaseUniverse()
  {
    var useImport = new FnOcse157CaseUniverse.Import();
    var useExport = new FnOcse157CaseUniverse.Export();

    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnOcse157CaseUniverse.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157CloseFiles()
  {
    var useImport = new FnOcse157CloseFiles.Import();
    var useExport = new FnOcse157CloseFiles.Export();

    Call(FnOcse157CloseFiles.Execute, useImport, useExport);
  }

  private void UseFnOcse157GetRestartLineNbr()
  {
    var useImport = new FnOcse157GetRestartLineNbr.Import();
    var useExport = new FnOcse157GetRestartLineNbr.Export();

    MoveProgramCheckpointRestart(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(FnOcse157GetRestartLineNbr.Execute, useImport, useExport);

    local.Restart.LineNumber = useExport.Restart.LineNumber;
  }

  private void UseFnOcse157Line1()
  {
    var useImport = new FnOcse157Line1.Import();
    var useExport = new FnOcse157Line1.Export();

    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line1.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line12()
  {
    var useImport = new FnOcse157Line12.Import();
    var useExport = new FnOcse157Line12.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;

    Call(FnOcse157Line12.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line13()
  {
    var useImport = new FnOcse157Line13.Import();
    var useExport = new FnOcse157Line13.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line13.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line14()
  {
    var useImport = new FnOcse157Line14.Import();
    var useExport = new FnOcse157Line14.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line14.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line16()
  {
    var useImport = new FnOcse157Line16.Import();
    var useExport = new FnOcse157Line16.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line16.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line17()
  {
    var useImport = new FnOcse157Line17.Import();
    var useExport = new FnOcse157Line17.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line17.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line18()
  {
    var useImport = new FnOcse157Line18.Import();
    var useExport = new FnOcse157Line18.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line18.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line19()
  {
    var useImport = new FnOcse157Line19.Import();
    var useExport = new FnOcse157Line19.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line19.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line2()
  {
    var useImport = new FnOcse157Line2.Import();
    var useExport = new FnOcse157Line2.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line2.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line20()
  {
    var useImport = new FnOcse157Line20.Import();
    var useExport = new FnOcse157Line20.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line20.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line212223()
  {
    var useImport = new FnOcse157Line212223.Import();
    var useExport = new FnOcse157Line212223.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line212223.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line24()
  {
    var useImport = new FnOcse157Line24.Import();
    var useExport = new FnOcse157Line24.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line24.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line25()
  {
    var useImport = new FnOcse157Line25.Import();
    var useExport = new FnOcse157Line25.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line25.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line26()
  {
    var useImport = new FnOcse157Line26.Import();
    var useExport = new FnOcse157Line26.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line26.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line27()
  {
    var useImport = new FnOcse157Line27.Import();
    var useExport = new FnOcse157Line27.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line27.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line28()
  {
    var useImport = new FnOcse157Line28.Import();
    var useExport = new FnOcse157Line28.Export();

    MoveOcse157Verification2(local.To, useImport.To);
    MoveOcse157Verification2(local.From, useImport.From);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line28.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line29()
  {
    var useImport = new FnOcse157Line29.Import();
    var useExport = new FnOcse157Line29.Export();

    MoveOcse157Verification2(local.To, useImport.To);
    MoveOcse157Verification2(local.From, useImport.From);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line29.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line3()
  {
    var useImport = new FnOcse157Line3.Import();
    var useExport = new FnOcse157Line3.Export();

    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.From.CaseNumber = local.From.CaseNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line3.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line3334()
  {
    var useImport = new FnOcse157Line3334.Import();
    var useExport = new FnOcse157Line3334.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;

    Call(FnOcse157Line3334.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line35()
  {
    var useImport = new FnOcse157Line35.Import();
    var useExport = new FnOcse157Line35.Export();

    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.From.CaseNumber = local.From.CaseNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line35.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line36()
  {
    var useImport = new FnOcse157Line36.Import();
    var useExport = new FnOcse157Line36.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line36.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line37()
  {
    var useImport = new FnOcse157Line37.Import();
    var useExport = new FnOcse157Line37.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line37.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line38New()
  {
    var useImport = new FnOcse157Line38New.Import();
    var useExport = new FnOcse157Line38New.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    useImport.From.CaseNumber = local.From.CaseNumber;
    useImport.To.CaseNumber = local.To.CaseNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line38New.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line4()
  {
    var useImport = new FnOcse157Line4.Import();
    var useExport = new FnOcse157Line4.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line4.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line5()
  {
    var useImport = new FnOcse157Line5.Import();
    var useExport = new FnOcse157Line5.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;

    Call(FnOcse157Line5.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line6()
  {
    var useImport = new FnOcse157Line6.Import();
    var useExport = new FnOcse157Line6.Export();

    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;

    Call(FnOcse157Line6.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line7()
  {
    var useImport = new FnOcse157Line7.Import();
    var useExport = new FnOcse157Line7.Export();

    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line7.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line8()
  {
    var useImport = new FnOcse157Line8.Import();
    var useExport = new FnOcse157Line8.Export();

    useImport.Line8B.Number = local.Line8B.Number;
    MoveDateWorkArea(local.CalendarYrStartDate, useImport.CalendarYearStartDte);
    MoveDateWorkArea(local.CalendarYearEndDate, useImport.CalendarYearEndDate);
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line8.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157Line9A()
  {
    var useImport = new FnOcse157Line9A.Import();
    var useExport = new FnOcse157Line9A.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    MoveDateWorkArea(local.CalendarYrStartDate, useImport.CalendarYearStartDte);
    MoveDateWorkArea(local.CalendarYearEndDate, useImport.CalendarYearEndDate);
    useImport.From.SuppPersonNumber = local.From.SuppPersonNumber;
    useImport.To.SuppPersonNumber = local.To.SuppPersonNumber;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;

    Call(FnOcse157Line9A.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    /// <summary>
    /// A value of Line8B.
    /// </summary>
    [JsonPropertyName("line8B")]
    public Ocse157Data Line8B
    {
      get => line8B ??= new();
      set => line8B = value;
    }

    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    [JsonPropertyName("previousYear")]
    public Ocse157Verification PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of CaseUniverse.
    /// </summary>
    [JsonPropertyName("caseUniverse")]
    public Common CaseUniverse
    {
      get => caseUniverse ??= new();
      set => caseUniverse = value;
    }

    /// <summary>
    /// A value of GetCaseCountPerProg.
    /// </summary>
    [JsonPropertyName("getCaseCountPerProg")]
    public Common GetCaseCountPerProg
    {
      get => getCaseCountPerProg ??= new();
      set => getCaseCountPerProg = value;
    }

    /// <summary>
    /// A value of CalendarYrStartDate.
    /// </summary>
    [JsonPropertyName("calendarYrStartDate")]
    public DateWorkArea CalendarYrStartDate
    {
      get => calendarYrStartDate ??= new();
      set => calendarYrStartDate = value;
    }

    /// <summary>
    /// A value of CalendarYearEndDate.
    /// </summary>
    [JsonPropertyName("calendarYearEndDate")]
    public DateWorkArea CalendarYearEndDate
    {
      get => calendarYearEndDate ??= new();
      set => calendarYearEndDate = value;
    }

    /// <summary>
    /// A value of AbortProgram.
    /// </summary>
    [JsonPropertyName("abortProgram")]
    public Common AbortProgram
    {
      get => abortProgram ??= new();
      set => abortProgram = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Ocse157Verification From
    {
      get => from ??= new();
      set => from = value;
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

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Ocse157Verification Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of TestRun.
    /// </summary>
    [JsonPropertyName("testRun")]
    public Common TestRun
    {
      get => testRun ??= new();
      set => testRun = value;
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

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Ocse157Verification cq66220EffectiveFy;
    private Ocse157Data line8B;
    private Ocse157Verification previousYear;
    private Common caseUniverse;
    private Common getCaseCountPerProg;
    private DateWorkArea calendarYrStartDate;
    private DateWorkArea calendarYearEndDate;
    private Common abortProgram;
    private Ocse157Verification to;
    private Ocse157Verification from;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification restart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling status;
    private EabReportSend eabReportSend;
    private Common testRun;
    private Common displayInd;
    private Ocse157Verification forError;
  }
#endregion
}
