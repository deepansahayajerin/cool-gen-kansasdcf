// Program: FN_B700_OCSE_34_REPORT, ID: 372733961, model: 746.
// Short name: SWEF700B
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
/// A program: FN_B700_OCSE_34_REPORT.
/// </para>
/// <para>
/// The program produces the Child Support Enforcement Program, Quarterly Report
/// of Collections (OCSE-34) report.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB700Ocse34Report: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE_34_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Report(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Report.
  /// </summary>
  public FnB700Ocse34Report(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 15/??/99  SWSRCHF	????????	Initial Development
    // 02/??/02  SWSRKXD	????????	Virtual rewrite
    // 12/05/03  SWSRESS	WR040134	Federally mandated changes to OCSE34 report.
    // 12/03/07  GVandy	CQ295		Federally mandated changes to OCSE34 report.
    // 01/06/09  GVandy	CQ486		Enhance audit trail to determine why part 1 and 
    // part 2 of the
    // 					OCSE34 report do not balance.
    // 10/14/12  GVandy			Emergency fix to expand foreign group view size
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEFB700";
    UseFnB700BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      UseFnOcse157GetRestartLineNbr();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "PR";
      UseOcse157WriteError();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // 12/3/2007 GVandy CQ295...
    // Build a group view of standard numbers associated to outgoing Foreign 
    // Interstate cases.
    // The group view is built here and passed to the appropriate action blocks 
    // to aid in performance.
    local.OutgoingForeign.Index = -1;

    foreach(var item in ReadLegalAction2())
    {
      if (local.OutgoingForeign.Index + 1 >= Local
        .OutgoingForeignGroup.Capacity)
      {
        local.EabReportSend.RptDetail =
          "Group view overflow while reading outoing Foreign interstate cases.";
          
        UseCabErrorReport2();
        ExitState = "FN0000_STANDARD_NO_GROUP_OVERFLO";

        return;
      }

      ++local.OutgoingForeign.Index;
      local.OutgoingForeign.CheckSize();

      local.OutgoingForeign.Update.GlocalOutgoingForeign.StandardNumber =
        entities.LegalAction.StandardNumber;
    }

    // 12/3/2007 GVandy CQ295...
    // Build a group view of standard numbers associated to incoming Foreign 
    // Interstate cases.
    // The group view is built here and passed to the appropriate action blocks 
    // to aid in performance.
    local.IncomingForeign.Index = -1;

    foreach(var item in ReadLegalAction1())
    {
      if (local.IncomingForeign.Index + 1 >= Local
        .IncomingForeignGroup.Capacity)
      {
        local.EabReportSend.RptDetail =
          "Group view overflow while reading incoming Foreign interstate cases.";
          
        UseCabErrorReport2();
        ExitState = "FN0000_STANDARD_NO_GROUP_OVERFLO";

        return;
      }

      ++local.IncomingForeign.Index;
      local.IncomingForeign.CheckSize();

      local.IncomingForeign.Update.GlocalIncomingForeign.StandardNumber =
        entities.LegalAction.StandardNumber;
    }

    if (!Lt("01", local.Restart.LineNumber) && !
      Lt("01", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "01"))
    {
      UseFnB700Ocse34Step1();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("02", local.Restart.LineNumber) && !
      Lt("02", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "02"))
    {
      UseFnB700Ocse34Step2();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    // ******  Logic within step3 handled refund processing.  This logic was 
    // incorporated into step2.  Hence the disabling.
    if (!Lt("03", local.Restart.LineNumber) && !
      Lt("03", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "03"))
    {
      UseFnB700Ocse34Step3();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("04", local.Restart.LineNumber) && !
      Lt("04", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "04"))
    {
      UseFnB700Ocse34Step4();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    // --------------------------------------------------------
    // Step 5 reads URA disb. There aren't any after 10/1/2000.
    // --------------------------------------------------------
    if (!Lt("05", local.Restart.LineNumber) && !
      Lt("05", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "05") && Lt
      (local.ReportEnd.Date, new DateTime(2000, 10, 1)) && !
      Equal(local.ProgramCheckpointRestart.RestartInfo, 4, 6, "PART 2"))
    {
      UseFnB700Ocse34Step51356350890();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("05", local.Restart.LineNumber) && !
      Lt("05", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "05"))
    {
      UseFnB700Ocse34HeldStale();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("06", local.Restart.LineNumber) && !
      Lt("06", local.FromOcse157Verification.LineNumber) && !
      Lt(local.ToOcse157Verification.LineNumber, "06"))
    {
      UseFnB700Ocse34Step6();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    // **************************************************************************
    // **               Check for out of balance situations.
    // **************************************************************************
    if (AsChar(local.Line9OobInd.Flag) == 'Y' || AsChar
      (local.Line9BOobInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "";
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (AsChar(local.Line9OobInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "**** Line 9 out-of-balance detected.  Line 6 - Line 8 <> Line 9a + Line 9b";
          
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.Line9BOobInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "**** Line 9B out-of-balance detected.  Line 9 - Line 9a <> Line 9c + Line 9d";
          
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
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

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveGroup1(FnB700Ocse34HeldStale.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(Local.GroupGroup source,
    FnB700Ocse34HeldStale.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup3(Local.GroupGroup source,
    FnB700Ocse34Step6.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup4(Local.GroupGroup source,
    FnB700Ocse34Step3.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup5(Local.GroupGroup source,
    FnB700Ocse34Step51356350890.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup6(Local.GroupGroup source,
    FnB700Ocse34Step2.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup7(Local.GroupGroup source,
    FnB700Ocse34Step1.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup8(Local.GroupGroup source,
    FnB700Ocse34Step4.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup9(FnB700Ocse34Step6.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup10(FnB700Ocse34Step3.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup11(FnB700Ocse34Step51356350890.Import.
    GroupGroup source, Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup12(FnB700Ocse34Step2.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup13(FnB700Ocse34Step1.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup14(FnB700Ocse34Step4.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveIncomingForeign1(Local.IncomingForeignGroup source,
    FnB700Ocse34HeldStale.Import.IncomingForeignGroup target)
  {
    target.GimportIncomingForeign.StandardNumber =
      source.GlocalIncomingForeign.StandardNumber;
  }

  private static void MoveIncomingForeign2(Local.IncomingForeignGroup source,
    FnB700Ocse34Step4.Import.IncomingForeignGroup target)
  {
    target.GimportIncomingForeign.StandardNumber =
      source.GlocalIncomingForeign.StandardNumber;
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
    target.LineNumber = source.LineNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
  }

  private static void MoveOcse1(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.PreviousUndistribAmount = source.PreviousUndistribAmount;
    target.AdjustmentsAmount = source.AdjustmentsAmount;
    target.UndistributedAmount = source.UndistributedAmount;
    target.IncentivePaymentAmount = source.IncentivePaymentAmount;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReportingPeriodBeginDate = source.ReportingPeriodBeginDate;
    target.ReportingPeriodEndDate = source.ReportingPeriodEndDate;
  }

  private static void MoveOcse2(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.NonIvdCasesAmount = source.NonIvdCasesAmount;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.FmapRate = source.FmapRate;
    target.CseDisbCreditAmt = source.CseDisbCreditAmt;
    target.CseDisbDebitAmt = source.CseDisbDebitAmt;
    target.CseWarrantAmt = source.CseWarrantAmt;
    target.CsePaymentAmt = source.CsePaymentAmt;
    target.CseInterstateAmt = source.CseInterstateAmt;
    target.CseCshRcptDtlSuspAmt = source.CseCshRcptDtlSuspAmt;
    target.CseDisbSuppressAmt = source.CseDisbSuppressAmt;
    target.KpcNon4DIwoCollAmt = source.KpcNon4DIwoCollAmt;
    target.KpcIvdNonIwoCollAmt = source.KpcIvdNonIwoCollAmt;
    target.KpcNonIvdIwoForwCollAmt = source.KpcNonIvdIwoForwCollAmt;
    target.KpcStaleDateAmt = source.KpcStaleDateAmt;
    target.KpcHeldDisbAmt = source.KpcHeldDisbAmt;
    target.UiIvdNonIwoIntAmt = source.UiIvdNonIwoIntAmt;
    target.KpcUiIvdNonIwoNonIntAmt = source.KpcUiIvdNonIwoNonIntAmt;
    target.KpcUiIvdNivdIwoAmt = source.KpcUiIvdNivdIwoAmt;
    target.KpcUiNonIvdIwoAmt = source.KpcUiNonIvdIwoAmt;
    target.KpcNivdIwoLda = source.KpcNivdIwoLda;
    target.FdsoDsbSuppAmt = source.FdsoDsbSuppAmt;
  }

  private static void MoveOcse3(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOcse4(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.FmapRate = source.FmapRate;
    target.KpcNon4DIwoCollAmt = source.KpcNon4DIwoCollAmt;
    target.KpcIvdNonIwoCollAmt = source.KpcIvdNonIwoCollAmt;
    target.UiIvdNonIwoIntAmt = source.UiIvdNonIwoIntAmt;
    target.KpcUiIvdNonIwoNonIntAmt = source.KpcUiIvdNonIwoNonIntAmt;
    target.KpcUiNonIvdIwoAmt = source.KpcUiNonIvdIwoAmt;
  }

  private static void MoveOutgoingForeign1(Local.OutgoingForeignGroup source,
    FnB700Ocse34Step1.Import.OutgoingForeignGroup target)
  {
    target.GlocalOutgoingForeign.StandardNumber =
      source.GlocalOutgoingForeign.StandardNumber;
  }

  private static void MoveOutgoingForeign2(Local.OutgoingForeignGroup source,
    FnB700Ocse34Step2.Import.OutgoingForeignGroup target)
  {
    target.GimportOutgoingForeign.StandardNumber =
      source.GlocalOutgoingForeign.StandardNumber;
  }

  private static void MoveOutgoingForeign3(Local.OutgoingForeignGroup source,
    FnB700Ocse34Step3.Import.OutgoingForeignGroup target)
  {
    target.GimportOutgoingForeign.StandardNumber =
      source.GlocalOutgoingForeign.StandardNumber;
  }

  private static void MoveOutgoingForeign4(Local.OutgoingForeignGroup source,
    FnB700Ocse34Step4.Import.OutgoingForeignGroup target)
  {
    target.GimportOutgoingForeign.StandardNumber =
      source.GlocalOutgoingForeign.StandardNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB700BatchInitialization()
  {
    var useImport = new FnB700BatchInitialization.Import();
    var useExport = new FnB700BatchInitialization.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(FnB700BatchInitialization.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.NonPayReqRptBeginDt, local.NonPayReqRptBegin);
    MoveDateWorkArea(useExport.NonPayReqRptEndDt, local.NonPayReqRptEnd);
    local.ToCashReceipt.SequentialNumber =
      useExport.ToCashReceipt.SequentialNumber;
    local.FromCashReceipt.SequentialNumber =
      useExport.FromCashReceipt.SequentialNumber;
    local.PreviousQuarter.Period = useExport.Prev.Period;
    local.Ocse34.Assign(useExport.Curr);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    MoveDateWorkArea(useExport.RptPrdBeginDt, local.ReportStart);
    MoveDateWorkArea(useExport.RptPrdEndDt, local.ReportEnd);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveOcse157Verification1(useExport.Ocse157Verification, local.Restart);
    MoveOcse157Verification2(useExport.FromOcse157Verification,
      local.FromOcse157Verification);
    MoveOcse157Verification2(useExport.ToOcse157Verification,
      local.ToOcse157Verification);
  }

  private void UseFnB700Ocse34HeldStale()
  {
    var useImport = new FnB700Ocse34HeldStale.Import();
    var useExport = new FnB700Ocse34HeldStale.Export();

    MoveOcse3(local.Ocse34, useImport.Ocse34);
    local.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveDateWorkArea(local.ReportStart, useImport.RptPrdBegin);
    MoveDateWorkArea(local.ReportEnd, useImport.RptPrdEnd);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.WriteAuditDtl.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    local.IncomingForeign.
      CopyTo(useImport.IncomingForeign, MoveIncomingForeign1);

    Call(FnB700Ocse34HeldStale.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup1);
    local.AbortProgram.Flag = useExport.AbortInd.Flag;
  }

  private void UseFnB700Ocse34Step1()
  {
    var useImport = new FnB700Ocse34Step1.Import();
    var useExport = new FnB700Ocse34Step1.Export();

    local.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign1);
    useImport.To.SequentialNumber = local.ToCashReceipt.SequentialNumber;
    useImport.From.SequentialNumber = local.FromCashReceipt.SequentialNumber;
    MoveDateWorkArea(local.ReportEnd, useImport.ReportEnd);
    MoveDateWorkArea(local.ReportStart, useImport.ReportStart);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    local.Group.CopyTo(useImport.Group, MoveGroup7);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    MoveOcse1(local.Ocse34, useImport.Ocse34);

    Call(FnB700Ocse34Step1.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup13);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB700Ocse34Step2()
  {
    var useImport = new FnB700Ocse34Step2.Import();
    var useExport = new FnB700Ocse34Step2.Export();

    local.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign2);
    useImport.To.SequentialNumber = local.ToCashReceipt.SequentialNumber;
    useImport.From.SequentialNumber = local.FromCashReceipt.SequentialNumber;
    MoveDateWorkArea(local.ReportEnd, useImport.ReportEnd);
    MoveDateWorkArea(local.ReportStart, useImport.ReportStart);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    local.Group.CopyTo(useImport.Group, MoveGroup6);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    MoveOcse4(local.Ocse34, useImport.Ocse34);

    Call(FnB700Ocse34Step2.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup12);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB700Ocse34Step3()
  {
    var useImport = new FnB700Ocse34Step3.Import();
    var useExport = new FnB700Ocse34Step3.Export();

    local.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign3);
    useImport.To.SequentialNumber = local.ToCashReceipt.SequentialNumber;
    useImport.From.SequentialNumber = local.FromCashReceipt.SequentialNumber;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    local.Group.CopyTo(useImport.Group, MoveGroup4);
    MoveDateWorkArea(local.ReportStart, useImport.ReportStart);
    MoveDateWorkArea(local.ReportEnd, useImport.ReportEnd);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);
    MoveOcse4(local.Ocse34, useImport.Ocse34);

    Call(FnB700Ocse34Step3.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup10);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB700Ocse34Step4()
  {
    var useImport = new FnB700Ocse34Step4.Import();
    var useExport = new FnB700Ocse34Step4.Export();

    local.IncomingForeign.
      CopyTo(useImport.IncomingForeign, MoveIncomingForeign2);
    local.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign4);
    MoveOcse3(local.Ocse34, useImport.Ocse34);
    local.Group.CopyTo(useImport.Group, MoveGroup8);
    MoveDateWorkArea(local.ReportStart, useImport.RptPrdBegin);
    MoveDateWorkArea(local.ReportEnd, useImport.RptPrdEnd);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo2(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.WriteAuditDtl.Flag = local.DisplayInd.Flag;
    MoveOcse157Verification1(local.Restart, useImport.Ocse157Verification);

    Call(FnB700Ocse34Step4.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup14);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB700Ocse34Step51356350890()
  {
    var useImport = new FnB700Ocse34Step51356350890.Import();
    var useExport = new FnB700Ocse34Step51356350890.Export();

    MoveDateWorkArea(local.ReportEnd, useImport.ReportEnd);
    MoveDateWorkArea(local.ReportStart, useImport.ReportStart);
    local.Group.CopyTo(useImport.Group, MoveGroup5);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.Ocse34.Period = local.Ocse34.Period;

    Call(FnB700Ocse34Step51356350890.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup11);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB700Ocse34Step6()
  {
    var useImport = new FnB700Ocse34Step6.Import();
    var useExport = new FnB700Ocse34Step6.Export();

    MoveOcse2(local.Ocse34, useImport.Ocse34);
    useImport.Prev.Period = local.PreviousQuarter.Period;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    local.Group.CopyTo(useImport.Group, MoveGroup3);

    Call(FnB700Ocse34Step6.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup9);
    local.Line9OobInd.Flag = useExport.Line9OobInd.Flag;
    local.Line9BOobInd.Flag = useExport.Line9BOobInd.Flag;
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

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    useImport.Ocse157Verification.LineNumber = local.ForError.LineNumber;

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction1",
      null,
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      null,
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;

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
    /// <summary>A IncomingForeignGroup group.</summary>
    [Serializable]
    public class IncomingForeignGroup
    {
      /// <summary>
      /// A value of GlocalIncomingForeign.
      /// </summary>
      [JsonPropertyName("glocalIncomingForeign")]
      public LegalAction GlocalIncomingForeign
      {
        get => glocalIncomingForeign ??= new();
        set => glocalIncomingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction glocalIncomingForeign;
    }

    /// <summary>A OutgoingForeignGroup group.</summary>
    [Serializable]
    public class OutgoingForeignGroup
    {
      /// <summary>
      /// A value of GlocalOutgoingForeign.
      /// </summary>
      [JsonPropertyName("glocalOutgoingForeign")]
      public LegalAction GlocalOutgoingForeign
      {
        get => glocalOutgoingForeign ??= new();
        set => glocalOutgoingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction glocalOutgoingForeign;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>
    /// Gets a value of IncomingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<IncomingForeignGroup> IncomingForeign =>
      incomingForeign ??= new(IncomingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("incomingForeign")]
    [Computed]
    public IList<IncomingForeignGroup> IncomingForeign_Json
    {
      get => incomingForeign;
      set => IncomingForeign.Assign(value);
    }

    /// <summary>
    /// Gets a value of OutgoingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<OutgoingForeignGroup> OutgoingForeign =>
      outgoingForeign ??= new(OutgoingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OutgoingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("outgoingForeign")]
    [Computed]
    public IList<OutgoingForeignGroup> OutgoingForeign_Json
    {
      get => outgoingForeign;
      set => OutgoingForeign.Assign(value);
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of PreviousQuarter.
    /// </summary>
    [JsonPropertyName("previousQuarter")]
    public Ocse34 PreviousQuarter
    {
      get => previousQuarter ??= new();
      set => previousQuarter = value;
    }

    /// <summary>
    /// A value of ReportStart.
    /// </summary>
    [JsonPropertyName("reportStart")]
    public DateWorkArea ReportStart
    {
      get => reportStart ??= new();
      set => reportStart = value;
    }

    /// <summary>
    /// A value of ReportEnd.
    /// </summary>
    [JsonPropertyName("reportEnd")]
    public DateWorkArea ReportEnd
    {
      get => reportEnd ??= new();
      set => reportEnd = value;
    }

    /// <summary>
    /// A value of NonPayReqRptBegin.
    /// </summary>
    [JsonPropertyName("nonPayReqRptBegin")]
    public DateWorkArea NonPayReqRptBegin
    {
      get => nonPayReqRptBegin ??= new();
      set => nonPayReqRptBegin = value;
    }

    /// <summary>
    /// A value of NonPayReqRptEnd.
    /// </summary>
    [JsonPropertyName("nonPayReqRptEnd")]
    public DateWorkArea NonPayReqRptEnd
    {
      get => nonPayReqRptEnd ??= new();
      set => nonPayReqRptEnd = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Ocse157Verification Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of FromOcse157Verification.
    /// </summary>
    [JsonPropertyName("fromOcse157Verification")]
    public Ocse157Verification FromOcse157Verification
    {
      get => fromOcse157Verification ??= new();
      set => fromOcse157Verification = value;
    }

    /// <summary>
    /// A value of ToOcse157Verification.
    /// </summary>
    [JsonPropertyName("toOcse157Verification")]
    public Ocse157Verification ToOcse157Verification
    {
      get => toOcse157Verification ??= new();
      set => toOcse157Verification = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    /// <summary>
    /// A value of Line9OobInd.
    /// </summary>
    [JsonPropertyName("line9OobInd")]
    public Common Line9OobInd
    {
      get => line9OobInd ??= new();
      set => line9OobInd = value;
    }

    /// <summary>
    /// A value of Line9BOobInd.
    /// </summary>
    [JsonPropertyName("line9BOobInd")]
    public Common Line9BOobInd
    {
      get => line9BOobInd ??= new();
      set => line9BOobInd = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of ToCashReceipt.
    /// </summary>
    [JsonPropertyName("toCashReceipt")]
    public CashReceipt ToCashReceipt
    {
      get => toCashReceipt ??= new();
      set => toCashReceipt = value;
    }

    /// <summary>
    /// A value of FromCashReceipt.
    /// </summary>
    [JsonPropertyName("fromCashReceipt")]
    public CashReceipt FromCashReceipt
    {
      get => fromCashReceipt ??= new();
      set => fromCashReceipt = value;
    }

    private Array<IncomingForeignGroup> incomingForeign;
    private Array<OutgoingForeignGroup> outgoingForeign;
    private Ocse34 ocse34;
    private Array<GroupGroup> group;
    private EabFileHandling eabFileHandling;
    private Ocse34 previousQuarter;
    private DateWorkArea reportStart;
    private DateWorkArea reportEnd;
    private DateWorkArea nonPayReqRptBegin;
    private DateWorkArea nonPayReqRptEnd;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Common displayInd;
    private Ocse157Verification restart;
    private Ocse157Verification fromOcse157Verification;
    private Ocse157Verification toOcse157Verification;
    private Common abortProgram;
    private Ocse157Verification forError;
    private Common line9OobInd;
    private Common line9BOobInd;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private CashReceipt toCashReceipt;
    private CashReceipt fromCashReceipt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
