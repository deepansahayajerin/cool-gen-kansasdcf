// Program: FN_B701_PRINT_ERROR_REPORT, ID: 945110011, model: 746.
// Short name: SWEF701B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B701_PRINT_ERROR_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB701PrintErrorReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B701_PRINT_ERROR_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB701PrintErrorReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB701PrintErrorReport.
  /// </summary>
  public FnB701PrintErrorReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 11/16/2012      DDupree   	Initial Creation
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB701Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.OfficeHeading.RptDetail = "             Office:";
    local.WorkerHeading.RptDetail = "Attorney/Contractor:";
    local.CountyHeading.RptDetail = "             County:";
    local.MaxLine.Count = 51;
    local.LineCount.Count = 1;
    local.FirstRecordRead.Flag = "Y";
    local.NewHeading.Flag = "Y";

    do
    {
      local.External.FileInstruction = "READ";
      UseFnEabB701ReadErrorFile();

      if (Equal(local.External.TextReturnCode, "EF"))
      {
        break;
      }

      if (!Equal(local.External.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      local.EabReportSend.RptDetail = local.Read.RptDetail;

      if (!Equal(local.County.Name, local.PreviousCounty.Name) || !
        Equal(local.Office.Name, local.PreviousOffice.Name) || !
        Equal(local.ServiceProvider.LastName,
        local.PreviousServiceProvider.LastName) && !
        Equal(local.ServiceProvider.FirstName,
        local.PreviousServiceProvider.FirstName) || !
        Equal(local.ServiceProvider.LastName,
        local.PreviousServiceProvider.LastName) || !
        Equal(local.ServiceProvider.FirstName,
        local.PreviousServiceProvider.FirstName))
      {
        local.LineCount.Count = 0;

        // office heading
        if (!Equal(local.Office.Name, local.PreviousOffice.Name))
        {
          if (!IsEmpty(local.Office.Name))
          {
            local.OfficeHeading.RptDetail =
              Substring(local.OfficeHeading.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 20) + local.Office.Name;
          }
          else
          {
            local.OfficeHeading.RptDetail =
              "             Office: No Office Available";
          }
        }

        if (AsChar(local.FirstRecordRead.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "FIRSTS";
          local.FirstRecordRead.Flag = "N";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
            local.EabFileHandling.Status = "";
          }

          ++local.LineCount.Count;
        }
        else
        {
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
            local.EabFileHandling.Status = "";
          }

          ++local.LineCount.Count;
          local.EabFileHandling.Action = "WRITE";

          // date heading
          UseCabBusinessReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
            local.EabFileHandling.Status = "";
          }

          ++local.LineCount.Count;
        }

        // blank line
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport5();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;
        local.EabFileHandling.Action = "WRITE";

        // office heading
        if (!Equal(local.Office.Name, local.PreviousOffice.Name))
        {
          if (!IsEmpty(local.Office.Name))
          {
            local.OfficeHeading.RptDetail =
              Substring(local.OfficeHeading.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 20) + local.Office.Name;
          }
          else
          {
            local.OfficeHeading.RptDetail =
              "             Office: No Office Available";
          }
        }

        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        // attorney heading
        if (!Equal(local.ServiceProvider.LastName,
          local.PreviousServiceProvider.LastName) || !
          Equal(local.ServiceProvider.FirstName,
          local.PreviousServiceProvider.FirstName))
        {
          if (!IsEmpty(local.ServiceProvider.LastName))
          {
            local.WorkerHeading.RptDetail =
              Substring(local.WorkerHeading.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 20) + TrimEnd
              (local.ServiceProvider.LastName) + ", " + TrimEnd
              (local.ServiceProvider.FirstName) + " " + local
              .ServiceProvider.MiddleInitial;
          }
          else
          {
            local.WorkerHeading.RptDetail =
              "Attorney/Contractor: No Attoreny/Contractor Available";
          }
        }

        UseCabBusinessReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;

        // county heading
        if (!Equal(local.County.Name, local.PreviousCounty.Name))
        {
          if (!IsEmpty(local.County.Name))
          {
            local.CountyHeading.RptDetail =
              Substring(local.CountyHeading.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 20) + local.County.Name;
          }
          else
          {
            local.CountyHeading.RptDetail =
              "             County: No County Available";
          }
        }

        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;
        local.EabFileHandling.Action = "WRITE";

        // blank line
        ++local.LineCount.Count;
        UseCabBusinessReport5();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;

        // write the detail now
        UseCabBusinessReport6();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;
        local.PreviousOffice.Name = local.Office.Name;
        local.PreviousServiceProvider.Assign(local.ServiceProvider);
        local.PreviousCounty.Name = local.County.Name;
      }
      else
      {
        if (local.LineCount.Count >= local.MaxLine.Count)
        {
          local.LineCount.Count = 0;

          if (IsEmpty(local.Office.Name) && IsEmpty(local.PreviousOffice.Name))
          {
            local.NewHeading.Flag = "Y";
          }
        }

        if (IsEmpty(local.Office.Name) && IsEmpty
          (local.PreviousOffice.Name) && AsChar(local.NewHeading.Flag) == 'Y')
        {
          local.NewHeading.Flag = "N";

          if (AsChar(local.FirstRecordRead.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "FIRSTS";
            local.FirstRecordRead.Flag = "N";
            local.OfficeHeading.RptDetail =
              "             Office: No Office Available";
            UseCabBusinessReport3();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
          }
          else
          {
            local.EabFileHandling.Action = "NEWPAGE";
            UseCabBusinessReport3();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
            local.EabFileHandling.Action = "WRITE";

            // date
            UseCabBusinessReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
          }

          local.EabFileHandling.Action = "WRITE";

          // blank
          UseCabBusinessReport5();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
            local.EabFileHandling.Status = "";
          }

          ++local.LineCount.Count;
          local.EabFileHandling.Action = "WRITE";

          if (IsEmpty(local.Office.Name) && IsEmpty(local.PreviousOffice.Name))
          {
            local.OfficeHeading.RptDetail =
              "             Office: No Office Available";
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport3();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
          }

          if (IsEmpty(local.ServiceProvider.LastName) && IsEmpty
            (local.PreviousServiceProvider.LastName))
          {
            local.WorkerHeading.RptDetail =
              "Attorney/Contractor: No Attoreny/Contractor Available";
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
          }

          // county
          if (IsEmpty(local.County.Name) && IsEmpty(local.PreviousCounty.Name))
          {
            local.CountyHeading.RptDetail =
              "             County: No County Available";
            UseCabBusinessReport4();
            local.EabFileHandling.Action = "WRITE";

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Program abended because: " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = "";
              local.EabFileHandling.Status = "";
            }

            ++local.LineCount.Count;
          }

          // blank
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport5();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Program abended because: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
            local.EabFileHandling.Status = "";
          }

          ++local.LineCount.Count;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport6();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Program abended because: " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Status = "";
        }

        ++local.LineCount.Count;
      }
    }
    while(!Equal(local.External.TextReturnCode, "EF"));

    local.FirstRecordRead.Flag = "Y";

    // -- Close the control report
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing control report.  Return code = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Close the error report
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
    target.ColHeading3 = source.ColHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RunDate = source.RunDate;
    target.RunTime = source.RunTime;
    target.RptHeading1 = source.RptHeading1;
    target.RptHeading2 = source.RptHeading2;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.DateHeading.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.WorkerHeading.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.OfficeHeading.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.CountyHeading.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport5()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport6()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB701Housekeeping()
  {
    var useImport = new FnB701Housekeeping.Import();
    var useExport = new FnB701Housekeeping.Export();

    Call(FnB701Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    MoveEabReportSend2(useExport.EabReportSend, local.EabReportSend);
    local.DateHeading.RptDetail = useExport.DateHeading.RptDetail;
  }

  private void UseFnEabB701ReadErrorFile()
  {
    var useImport = new FnEabB701ReadErrorFile.Import();
    var useExport = new FnEabB701ReadErrorFile.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.Case1.Number = local.Case1.Number;
    useExport.ServiceProvider.Assign(local.ServiceProvider);
    useExport.Office.Name = local.Office.Name;
    useExport.Region.Name = local.County.Name;
    useExport.EabReportSend.RptDetail = local.Read.RptDetail;
    useExport.External.Assign(local.External);

    Call(FnEabB701ReadErrorFile.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
    local.ServiceProvider.Assign(useExport.ServiceProvider);
    local.Office.Name = useExport.Office.Name;
    local.County.Name = useExport.Region.Name;
    local.Read.RptDetail = useExport.EabReportSend.RptDetail;
    local.External.Assign(useExport.External);
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
    /// <summary>
    /// A value of DateHeading.
    /// </summary>
    [JsonPropertyName("dateHeading")]
    public EabReportSend DateHeading
    {
      get => dateHeading ??= new();
      set => dateHeading = value;
    }

    /// <summary>
    /// A value of MaxLine.
    /// </summary>
    [JsonPropertyName("maxLine")]
    public Common MaxLine
    {
      get => maxLine ??= new();
      set => maxLine = value;
    }

    /// <summary>
    /// A value of LineCount.
    /// </summary>
    [JsonPropertyName("lineCount")]
    public Common LineCount
    {
      get => lineCount ??= new();
      set => lineCount = value;
    }

    /// <summary>
    /// A value of NewHeading.
    /// </summary>
    [JsonPropertyName("newHeading")]
    public Common NewHeading
    {
      get => newHeading ??= new();
      set => newHeading = value;
    }

    /// <summary>
    /// A value of WorkerHeading.
    /// </summary>
    [JsonPropertyName("workerHeading")]
    public EabReportSend WorkerHeading
    {
      get => workerHeading ??= new();
      set => workerHeading = value;
    }

    /// <summary>
    /// A value of OfficeHeading.
    /// </summary>
    [JsonPropertyName("officeHeading")]
    public EabReportSend OfficeHeading
    {
      get => officeHeading ??= new();
      set => officeHeading = value;
    }

    /// <summary>
    /// A value of CountyHeading.
    /// </summary>
    [JsonPropertyName("countyHeading")]
    public EabReportSend CountyHeading
    {
      get => countyHeading ??= new();
      set => countyHeading = value;
    }

    /// <summary>
    /// A value of ReadPrevious.
    /// </summary>
    [JsonPropertyName("readPrevious")]
    public EabReportSend ReadPrevious
    {
      get => readPrevious ??= new();
      set => readPrevious = value;
    }

    /// <summary>
    /// A value of PreviousCounty.
    /// </summary>
    [JsonPropertyName("previousCounty")]
    public CseOrganization PreviousCounty
    {
      get => previousCounty ??= new();
      set => previousCounty = value;
    }

    /// <summary>
    /// A value of PreviousOffice.
    /// </summary>
    [JsonPropertyName("previousOffice")]
    public Office PreviousOffice
    {
      get => previousOffice ??= new();
      set => previousOffice = value;
    }

    /// <summary>
    /// A value of PreviousServiceProvider.
    /// </summary>
    [JsonPropertyName("previousServiceProvider")]
    public ServiceProvider PreviousServiceProvider
    {
      get => previousServiceProvider ??= new();
      set => previousServiceProvider = value;
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

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EabReportSend Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of FirstRecordRead.
    /// </summary>
    [JsonPropertyName("firstRecordRead")]
    public Common FirstRecordRead
    {
      get => firstRecordRead ??= new();
      set => firstRecordRead = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of ReportNumber.
    /// </summary>
    [JsonPropertyName("reportNumber")]
    public Common ReportNumber
    {
      get => reportNumber ??= new();
      set => reportNumber = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private EabReportSend dateHeading;
    private Common maxLine;
    private Common lineCount;
    private Common newHeading;
    private EabReportSend workerHeading;
    private EabReportSend officeHeading;
    private EabReportSend countyHeading;
    private EabReportSend readPrevious;
    private CseOrganization previousCounty;
    private Office previousOffice;
    private ServiceProvider previousServiceProvider;
    private External external;
    private EabReportSend read;
    private CseOrganization county;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common firstRecordRead;
    private Common common;
    private Common reportNumber;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
  }
#endregion
}
