// Program: SP_B376_DELIN_REV_RPT_PRNT, ID: 371318056, model: 746.
// Short name: SWEP376B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_B376_DELIN_REV_RPT_PRNT.
/// </para>
/// <para>
/// This report lists cases where a review has never been done and also cases 
/// where the review is overdue . This will be  sorted by  Office number ,
/// Supervisor , and Collection officer .
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB376DelinRevRptPrnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B376_DELIN_REV_RPT_PRNT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB376DelinRevRptPrnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB376DelinRevRptPrnt.
  /// </summary>
  public SpB376DelinRevRptPrnt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    //                        Maintenance Log
    // -------------------------------------------------------------------
    //      Date        Developer       Description
    //   06/12/01      Madhu Kumar      Delinquent review reports
    //                                  
    // print
    //   05/06/08      Arun Mathias     CQ#4325 Report Format changes
    //                                  
    // requested by Sana
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    local.Current.Date = Now().Date;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.FileAction.ActionEntry = "OP";
    UseEabSpB376FileExtractRead2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the read extract file";

      // **************************
      //     Write to Error Report
      // **************************
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.HeaderRecord.Flag = "Y";
    export.EabFileHandling.Action = "OPEN";
    export.NeededToOpen.RptHeading3 = "";
    export.NeededToOpen.NumberOfColHeadings = 2;

    // *** CQ#4325 Changes Begin Here ***
    export.NeededToOpen.ColHeading1 =
      "                     Delinquent Case Review Report";
    export.NeededToOpen.ColHeading2 =
      "                             Run date " + NumberToString
      (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "/" + NumberToString
      (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "/" + NumberToString
      (Year(local.ProgramProcessingInfo.ProcessDate), 12, 4);

    // *** CQ#4325 Changes End Here ***
    export.NeededToOpen.BlankLineAfterHeading = "Y";
    export.NeededToOpen.BlankLineAfterColHead = "Y";
    export.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabBusinessReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.CurrentNeverRvdCaseCnt.Count = 0;
    local.CurrentDelnRevdCaseCnt.Count = 0;
    local.TotalDelnReviewedCases.Count = 0;
    local.TotalNeverReviewedCases.Count = 0;
    local.PrevCo.LastName = "";
    local.PrevCo.FirstName = "";
    local.PrevCo.MiddleInitial = "";
    local.PrevOffcTextNumber.Text4 = "";

    // *** CQ#4325 Added first time flag
    local.FirstTime.Flag = "Y";

    do
    {
      local.FileAction.ActionEntry = "RD";
      UseEabSpB376FileExtractRead1();

      if (Equal(local.EabFileHandling.Status, "NOK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered reading the extract file";

        // ********
        // *** Write to Error Report
        // ********
        UseCabErrorReport4();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else if (Equal(local.EabFileHandling.Status, "C"))
      {
        break;
      }

      // ***CQ#4325 Changes Begin Here ***
      if (IsEmpty(local.Ss.LastName) && IsEmpty(local.Ss.FirstName) && IsEmpty
        (local.Ss.MiddleInitial))
      {
        local.Ss.FirstName = "N/A";
        local.Ss.LastName = "N/A";
        local.Ss.MiddleInitial = "";
      }

      // ***CQ#4325 Changes End   Here ***
      if (!Equal(local.OfficeTextNumber.Text4, local.PrevOffcTextNumber.Text4) ||
        !Equal(local.Co.LastName, local.PrevCo.LastName) || !
        Equal(local.Co.FirstName, local.PrevCo.FirstName) || AsChar
        (local.Co.MiddleInitial) != AsChar(local.PrevCo.MiddleInitial) || !
        Equal(local.Ss.LastName, local.PrevSs.LastName) || !
        Equal(local.Ss.FirstName, local.PrevSs.FirstName) || AsChar
        (local.Ss.MiddleInitial) != AsChar(local.PrevSs.MiddleInitial))
      {
        if (local.CurrentNeverRvdCaseCnt.Count > 0 && local
          .CurrentDelnRevdCaseCnt.Count == 0)
        {
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            NumberToString(local.CurrentNeverRvdCaseCnt.Count, 15) + " Cases Never Reviewed";
            
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // ********************************************************
          //              Insert a blank line
          // ********************************************************
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "";
          UseCabBusinessReport2();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (local.CurrentDelnRevdCaseCnt.Count > 0)
        {
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            NumberToString(local.CurrentDelnRevdCaseCnt.Count, 15) + " Cases with Delinquent Reviews ";
            
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // ********************************************************
          //              Insert a blank line
          // ********************************************************
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "";
          UseCabBusinessReport2();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      if (!Equal(local.OfficeTextNumber.Text4, local.PrevOffcTextNumber.Text4))
      {
        // ***CQ#4325 Changes Begin Here ***
        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";
        }
        else
        {
          // ********************************************************
          //              Start on a New Page
          // ********************************************************
          export.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport2();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // ***CQ#4325 Changes End   Here ***
        // ********************************************************
        //              Insert a blank line
        // ********************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "";
        UseCabBusinessReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // ********************************************************
        //          Write the office number and name
        // ********************************************************
        export.EabFileHandling.Action = "WRITE";

        // *** CQ#4325 Changes Begin Here ***
        export.NeededToWrite.RptDetail = "Office Nbr:  " + TrimEnd
          (local.OfficeTextNumber.Text4) + "             Office Name:  " + TrimEnd
          (local.Office.Name);

        // *** CQ#4325 Changes End   Here ***
        UseCabBusinessReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        export.NeededToWrite.RptDetail = "";
        local.PrevOffcTextNumber.Text4 = local.OfficeTextNumber.Text4;

        // ********************************************************
        //              Insert a blank line
        // ********************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "";
        UseCabBusinessReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // ***CQ#4325 Changes Begin Here ***
        local.PrevSs.LastName = "";
        local.PrevSs.FirstName = "";
        local.PrevSs.MiddleInitial = "";
        local.PrevCo.LastName = "";
        local.PrevCo.FirstName = "";
        local.PrevCo.MiddleInitial = "";

        // ***CQ#4325 Changes End   Here ***
      }

      if (!Equal(local.Ss.LastName, local.PrevSs.LastName) || !
        Equal(local.Ss.FirstName, local.PrevSs.FirstName) || AsChar
        (local.Ss.MiddleInitial) != AsChar(local.PrevSs.MiddleInitial))
      {
        // ***CQ#4325 Changes Begin Here ***
        if (Equal(local.Ss.LastName, "N/A") && Equal
          (local.Ss.FirstName, "N/A") && IsEmpty(local.Ss.MiddleInitial))
        {
          export.NeededToWrite.RptDetail = "N/A";
        }
        else
        {
          export.NeededToWrite.RptDetail = TrimEnd(local.Ss.LastName) + ", " + TrimEnd
            (local.Ss.FirstName) + " " + TrimEnd(local.Ss.MiddleInitial);
        }

        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "Supervisor:  " + TrimEnd
          (export.NeededToWrite.RptDetail);
        UseCabBusinessReport2();
        export.NeededToWrite.RptDetail = "";

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // ***CQ#4325 Changes End   Here ***
        local.PrevSs.LastName = local.Ss.LastName;
        local.PrevSs.FirstName = local.Ss.FirstName;
        local.PrevSs.MiddleInitial = local.Ss.MiddleInitial;
      }

      if (!Equal(local.Co.LastName, local.PrevCo.LastName) || !
        Equal(local.Co.FirstName, local.PrevCo.FirstName) || AsChar
        (local.Co.MiddleInitial) != AsChar(local.PrevCo.MiddleInitial))
      {
        // ********************************************************
        //              Insert a blank line
        // ********************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "";
        UseCabBusinessReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // *********************************************************
        //     Write the name of Collection officer in the report
        // *********************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = TrimEnd(local.Co.LastName) + ", " + TrimEnd
          (local.Co.FirstName) + " " + TrimEnd(local.Co.MiddleInitial);
        export.NeededToWrite.RptDetail = "Collection Officer:   " + TrimEnd
          (export.NeededToWrite.RptDetail);
        UseCabBusinessReport2();
        export.NeededToWrite.RptDetail = "";

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.PrevCo.LastName = local.Co.LastName;
        local.PrevCo.FirstName = local.Co.FirstName;
        local.PrevCo.MiddleInitial = local.Co.MiddleInitial;
        local.CurrentNeverRvdCaseCnt.Count = 0;
        local.CurrentDelnRevdCaseCnt.Count = 0;

        // ********************************************************
        //              Insert a blank line
        // ********************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "";
        UseCabBusinessReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.DelinOrNever.Flag) == 'N')
      {
        ++local.CurrentNeverRvdCaseCnt.Count;
        ++local.TotalNeverReviewedCases.Count;

        if (local.CurrentNeverRvdCaseCnt.Count == 1)
        {
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "Never Reviewed";
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "Case Open Date" + "    " + "Case Number" +
            "       " + "AP Name";
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.CaseOpenDate.TextDate =
          NumberToString(DateToInt(local.Case1.CseOpenDate), 8, 8);
        export.NeededToWrite.RptDetail =
          Substring(local.CaseOpenDate.TextDate,
          DateWorkArea.TextDate_MaxLength, 5, 2) + "/" + Substring
          (local.CaseOpenDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) +
          "/" + Substring
          (local.CaseOpenDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
        export.NeededToWrite.RptDetail =
          Substring(export.NeededToWrite.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 18) + TrimEnd
          (local.Case1.Number);

        if (IsEmpty(local.Ap.LastName) && IsEmpty(local.Ap.FirstName) && IsEmpty
          (local.Ap.MiddleInitial))
        {
          export.NeededToWrite.RptDetail =
            TrimEnd(export.NeededToWrite.RptDetail) + "        " + "No AP found ";
            
        }
        else
        {
          export.NeededToWrite.RptDetail =
            TrimEnd(export.NeededToWrite.RptDetail) + "        " + TrimEnd
            (local.Ap.LastName) + ", " + TrimEnd(local.Ap.FirstName) + " " + TrimEnd
            (local.Ap.MiddleInitial);
        }

        export.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport2();
        export.NeededToWrite.RptDetail = "";

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.DelinOrNever.Flag) == 'D')
      {
        ++local.CurrentDelnRevdCaseCnt.Count;
        ++local.TotalDelnReviewedCases.Count;

        if (local.CurrentNeverRvdCaseCnt.Count > 0 && local
          .CurrentDelnRevdCaseCnt.Count == 1)
        {
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            NumberToString(local.CurrentNeverRvdCaseCnt.Count, 15) + "  Cases Never Reviewed ";
            
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "";
          UseCabBusinessReport2();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (local.CurrentDelnRevdCaseCnt.Count == 1)
        {
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "Delinquent Reviews";
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail = "Review Date" + "       " + "Case Number" +
            "       " + "AP Name";
          UseCabBusinessReport2();
          export.NeededToWrite.RptDetail = "";

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail =
          Substring(local.TextDelinDate.TextDate,
          DateWorkArea.TextDate_MaxLength, 5, 2) + "/" + Substring
          (local.TextDelinDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) +
          "/" + Substring
          (local.TextDelinDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
          
        export.NeededToWrite.RptDetail =
          Substring(export.NeededToWrite.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 18) + TrimEnd
          (local.Case1.Number);

        if (IsEmpty(local.Ap.LastName) && IsEmpty(local.Ap.FirstName) && IsEmpty
          (local.Ap.MiddleInitial))
        {
          export.NeededToWrite.RptDetail =
            TrimEnd(export.NeededToWrite.RptDetail) + "        " + "No AP found ";
            
        }
        else
        {
          export.NeededToWrite.RptDetail =
            TrimEnd(export.NeededToWrite.RptDetail) + "        " + TrimEnd
            (local.Ap.LastName) + ", " + TrimEnd(local.Ap.FirstName) + " " + TrimEnd
            (local.Ap.MiddleInitial);
        }

        UseCabBusinessReport2();
        export.NeededToWrite.RptDetail = "";

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }
    while(!Equal(global.Command, "PERFECT"));

    if (local.CurrentNeverRvdCaseCnt.Count > 0 && local
      .CurrentDelnRevdCaseCnt.Count == 0)
    {
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        NumberToString(local.CurrentNeverRvdCaseCnt.Count, 15) + " Cases Never Reviewed";
        
      UseCabBusinessReport2();
      export.NeededToWrite.RptDetail = "";

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ********************************************************
      //              Insert a blank line
      // ********************************************************
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (local.CurrentDelnRevdCaseCnt.Count > 0)
    {
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        NumberToString(local.CurrentDelnRevdCaseCnt.Count, 15) + " Cases with Delinquent Reviews ";
        
      UseCabBusinessReport2();
      export.NeededToWrite.RptDetail = "";

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ********************************************************
      //              Insert a blank line
      // ********************************************************
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Delinquent Case Records Written  :  " + NumberToString
      (local.TotalDelnReviewedCases.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Delinquent Case Records)";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Never Reviewed Case Records Written  :  " + NumberToString
      (local.TotalNeverReviewedCases.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Never Reviewed Case Records Written)";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.FileAction.ActionEntry = "CL";
    UseEabSpB376FileExtractRead2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the read extract file";

      // **************************
      //     Write to Error Report
      // **************************
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    export.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport3();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.CseOpenDate = source.CseOpenDate;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToOpen.Assign(export.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabSpB376FileExtractRead1()
  {
    var useImport = new EabSpB376FileExtractRead.Import();
    var useExport = new EabSpB376FileExtractRead.Export();

    useImport.Common.ActionEntry = local.FileAction.ActionEntry;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    MoveOffice(local.Office, useExport.Office);
    useExport.Ss.Assign(local.Ss);
    useExport.Co.Assign(local.Co);
    useExport.Review.Date = local.Review.Date;
    useExport.Ap.Assign(local.Ap);
    MoveCase1(local.Case1, useExport.Case1);
    useExport.DelinquentOrNever.Flag = local.DelinOrNever.Flag;
    useExport.OfficeNumber.Text4 = local.OfficeTextNumber.Text4;
    useExport.DelinDateText.TextDate = local.TextDelinDate.TextDate;

    Call(EabSpB376FileExtractRead.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveOffice(useExport.Office, local.Office);
    local.Ss.Assign(useExport.Ss);
    local.Co.Assign(useExport.Co);
    local.Review.Date = useExport.Review.Date;
    local.Ap.Assign(useExport.Ap);
    MoveCase1(useExport.Case1, local.Case1);
    local.DelinOrNever.Flag = useExport.DelinquentOrNever.Flag;
    local.OfficeTextNumber.Text4 = useExport.OfficeNumber.Text4;
    local.TextDelinDate.TextDate = useExport.DelinDateText.TextDate;
  }

  private void UseEabSpB376FileExtractRead2()
  {
    var useImport = new EabSpB376FileExtractRead.Import();
    var useExport = new EabSpB376FileExtractRead.Export();

    useImport.Common.ActionEntry = local.FileAction.ActionEntry;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabSpB376FileExtractRead.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of ExternalFidmTrailer.
    /// </summary>
    [JsonPropertyName("externalFidmTrailer")]
    public ExternalFidmTrailer ExternalFidmTrailer
    {
      get => externalFidmTrailer ??= new();
      set => externalFidmTrailer = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ExternalFidmTrailer externalFidmTrailer;
    private External external;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseOpenDate.
    /// </summary>
    [JsonPropertyName("caseOpenDate")]
    public DateWorkArea CaseOpenDate
    {
      get => caseOpenDate ??= new();
      set => caseOpenDate = value;
    }

    /// <summary>
    /// A value of PrevSs.
    /// </summary>
    [JsonPropertyName("prevSs")]
    public CsePersonsWorkSet PrevSs
    {
      get => prevSs ??= new();
      set => prevSs = value;
    }

    /// <summary>
    /// A value of PrevOffcTextNumber.
    /// </summary>
    [JsonPropertyName("prevOffcTextNumber")]
    public TextWorkArea PrevOffcTextNumber
    {
      get => prevOffcTextNumber ??= new();
      set => prevOffcTextNumber = value;
    }

    /// <summary>
    /// A value of TextDelinDate.
    /// </summary>
    [JsonPropertyName("textDelinDate")]
    public DateWorkArea TextDelinDate
    {
      get => textDelinDate ??= new();
      set => textDelinDate = value;
    }

    /// <summary>
    /// A value of OfficeTextNumber.
    /// </summary>
    [JsonPropertyName("officeTextNumber")]
    public TextWorkArea OfficeTextNumber
    {
      get => officeTextNumber ??= new();
      set => officeTextNumber = value;
    }

    /// <summary>
    /// A value of CurrentDelnRevdCaseCnt.
    /// </summary>
    [JsonPropertyName("currentDelnRevdCaseCnt")]
    public Common CurrentDelnRevdCaseCnt
    {
      get => currentDelnRevdCaseCnt ??= new();
      set => currentDelnRevdCaseCnt = value;
    }

    /// <summary>
    /// A value of CurrentNeverRvdCaseCnt.
    /// </summary>
    [JsonPropertyName("currentNeverRvdCaseCnt")]
    public Common CurrentNeverRvdCaseCnt
    {
      get => currentNeverRvdCaseCnt ??= new();
      set => currentNeverRvdCaseCnt = value;
    }

    /// <summary>
    /// A value of PrevCo.
    /// </summary>
    [JsonPropertyName("prevCo")]
    public CsePersonsWorkSet PrevCo
    {
      get => prevCo ??= new();
      set => prevCo = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Office Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of TotalDelnReviewedCases.
    /// </summary>
    [JsonPropertyName("totalDelnReviewedCases")]
    public Common TotalDelnReviewedCases
    {
      get => totalDelnReviewedCases ??= new();
      set => totalDelnReviewedCases = value;
    }

    /// <summary>
    /// A value of TotalNeverReviewedCases.
    /// </summary>
    [JsonPropertyName("totalNeverReviewedCases")]
    public Common TotalNeverReviewedCases
    {
      get => totalNeverReviewedCases ??= new();
      set => totalNeverReviewedCases = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public Common HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of DelinOrNever.
    /// </summary>
    [JsonPropertyName("delinOrNever")]
    public Common DelinOrNever
    {
      get => delinOrNever ??= new();
      set => delinOrNever = value;
    }

    /// <summary>
    /// A value of Review.
    /// </summary>
    [JsonPropertyName("review")]
    public DateWorkArea Review
    {
      get => review ??= new();
      set => review = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Ss.
    /// </summary>
    [JsonPropertyName("ss")]
    public CsePersonsWorkSet Ss
    {
      get => ss ??= new();
      set => ss = value;
    }

    /// <summary>
    /// A value of Co.
    /// </summary>
    [JsonPropertyName("co")]
    public CsePersonsWorkSet Co
    {
      get => co ??= new();
      set => co = value;
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
    /// A value of FileAction.
    /// </summary>
    [JsonPropertyName("fileAction")]
    public Common FileAction
    {
      get => fileAction ??= new();
      set => fileAction = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    private DateWorkArea caseOpenDate;
    private CsePersonsWorkSet prevSs;
    private TextWorkArea prevOffcTextNumber;
    private DateWorkArea textDelinDate;
    private TextWorkArea officeTextNumber;
    private Common currentDelnRevdCaseCnt;
    private Common currentNeverRvdCaseCnt;
    private CsePersonsWorkSet prevCo;
    private Office prev;
    private Common totalDelnReviewedCases;
    private Common totalNeverReviewedCases;
    private DateWorkArea max;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common record;
    private AdministrativeActCertification administrativeActCertification;
    private Common headerRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common fileOpened;
    private Common delinOrNever;
    private DateWorkArea review;
    private CsePersonsWorkSet ap;
    private Office office;
    private CsePersonsWorkSet ss;
    private CsePersonsWorkSet co;
    private Case1 case1;
    private Common fileAction;
    private EabReportSend neededToWrite;
    private Common firstTime;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
