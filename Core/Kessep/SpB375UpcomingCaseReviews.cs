// Program: SP_B375_UPCOMING_CASE_REVIEWS, ID: 374514468, model: 746.
// Short name: SWEP375B
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
/// A program: SP_B375_UPCOMING_CASE_REVIEWS.
/// </para>
/// <para>
/// This report lists cases where a review has never been done and also cases 
/// where the review is overdue .
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB375UpcomingCaseReviews: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B375_UPCOMING_CASE_REVIEWS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB375UpcomingCaseReviews(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB375UpcomingCaseReviews.
  /// </summary>
  public SpB375UpcomingCaseReviews(IContext context, Import import,
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
    // -------------------------------------------------------------------
    //                        Maintenance Log .
    // -------------------------------------------------------------------
    //      Date        Developer           Description
    //   11/10/09      DDupree     Upcoming Case Review based on b303 delinquent
    // review
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.OfficeCount.Count = 0;
    local.CollectionOfficersCount.Count = 0;
    local.ProgramProcessingInfo.Name = global.UserId;
    local.CurrentDateWorkArea.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.ApCaseRole.Type1 = "AP";
    local.Open.Status = "O";
    local.Local51.ActivityControlNumber = 51;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Run.Month = Month(local.ProgramProcessingInfo.ProcessDate);
    local.RangeBeg.Year = Year(local.ProgramProcessingInfo.ProcessDate);
    local.RangeBeg.Day = (int)StringToNumber("01");

    // ***
    // *** Range start date
    // ***
    local.RangeBeg.TextDate = NumberToString(local.RangeBeg.Year, 12, 4) + NumberToString
      (local.Run.Month, 14, 2) + NumberToString(local.RangeBeg.Day, 14, 2);
    local.RangeBeg.Date =
      IntToDate((int)StringToNumber(local.RangeBeg.TextDate));
    local.RangeBeg.Date = AddMonths(local.RangeBeg.Date, 1);
    local.ProcessCountToCommit.Count = 0;
    local.Start.Count = 1;
    local.CurrentCommon.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.ExtractWritten.Count = 0;

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ';')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        if (local.FieldNumber.Count == 1)
        {
          // get parm passed in for months
          local.NumberOfMonthsWorkArea.Text15 = "";

          if (local.CurrentCommon.Count == 1)
          {
            local.NumberOfMonthsWorkArea.Text15 = "18";
            local.NumberOfMonthsCommon.Count =
              (int)StringToNumber(local.NumberOfMonthsWorkArea.Text15);
          }
          else
          {
            local.NumberOfMonthsWorkArea.Text15 =
              Substring(local.ProgramProcessingInfo.ParameterList,
              local.Start.Count, local.CurrentCommon.Count - 1);
            local.NumberOfMonthsCommon.Count =
              (int)StringToNumber(local.NumberOfMonthsWorkArea.Text15);
          }

          local.Start.Count = local.CurrentPosition.Count + 1;
          local.CurrentCommon.Count = 0;

          break;
        }
        else
        {
        }
      }

      ++local.CurrentPosition.Count;
      ++local.CurrentCommon.Count;

      if (local.CurrentPosition.Count >= 240)
      {
        local.NumberOfMonthsCommon.Count = 18;

        break;

        // we do not want to get into an endless loop, if for some reason there 
        // is no
        // delimiter in the parameter then when we will get to the last 
        // character we will escape.
      }
    }
    while(!Equal(global.Command, "COMMAND"));

    // ***
    // *** Range end date
    // ***
    local.RangeBeg.Date =
      AddMonths(local.RangeBeg.Date, -local.NumberOfMonthsCommon.Count);
    local.RangeEnd.Date = AddMonths(local.RangeBeg.Date, 1);
    local.RangeEnd.Date = AddDays(local.RangeEnd.Date, -1);
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.FirstOfMonth.Date = local.EabReportSend.ProcessDate;
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
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************************
    //            Opening the flatfile to write records  .
    // ***************************************************************
    local.FileAction.ActionEntry = "OP";
    UseSpEabB375FileExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Extract File.";

      // ******************
      // *** Write to report
      // ******************
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    foreach(var item in ReadOffice())
    {
      ++local.OfficeCount.Count;

      foreach(var item1 in ReadServiceProvider())
      {
        ++local.CollectionOfficersCount.Count;
        local.SupervisorFound.Flag = "N";

        if (ReadServiceProviderOfficeServiceProvRelationship())
        {
          local.SupervisorFound.Flag = "Y";
        }

        if (AsChar(local.SupervisorFound.Flag) == 'Y')
        {
          local.Ss.LastName = entities.SsServiceProvider.LastName;
          local.Ss.FirstName = entities.SsServiceProvider.FirstName;
          local.Ss.MiddleInitial = entities.SsServiceProvider.MiddleInitial;
          local.Ss.FormattedName = TrimEnd(local.Ss.LastName) + ", " + TrimEnd
            (local.Ss.FirstName) + " " + local.Ss.MiddleInitial;
        }
        else
        {
          local.Ss.LastName = "";
          local.Ss.FirstName = "";
          local.Ss.MiddleInitial = "";
          local.Ss.FormattedName = "";
        }

        // ***********************************************************
        //       Never reviewed and delinquent reviewed cases .
        // ***********************************************************
        foreach(var item2 in ReadCase())
        {
          local.CaseDueForReview.Flag = "N";
          local.MaxReferenceDate.Date = null;
          local.Review.Date = null;
          local.Review.TextDate = "";
          ReadInfrastructure();

          if (!Equal(local.MaxReferenceDate.Date, null))
          {
            if (!Lt(local.MaxReferenceDate.Date, local.RangeBeg.Date) && !
              Lt(local.RangeEnd.Date, local.MaxReferenceDate.Date))
            {
              local.CaseDueForReview.Flag = "Y";
            }
          }
          else
          {
            continue;

            // if there is not infrastructure record then move on to the case
          }

          if (AsChar(local.CaseDueForReview.Flag) == 'Y')
          {
            // ******************************************************
            //         Fill up the local views which will be used to
            //  write to the flat file later
            // ******************************************************
            local.Office.SystemGeneratedId =
              entities.ExistingOffice.SystemGeneratedId;
            local.Office.Name = entities.ExistingOffice.Name;
            local.Co.LastName = entities.CoServiceProvider.LastName;
            local.Co.FirstName = entities.CoServiceProvider.FirstName;
            local.Co.MiddleInitial = entities.CoServiceProvider.MiddleInitial;
            local.Co.FormattedName = TrimEnd(local.Co.LastName) + ", " + TrimEnd
              (local.Co.FirstName) + " " + local.Co.MiddleInitial;
            local.Case1.Number = entities.ExistingCase.Number;

            // Commented the compliance date and now using reference date as 
            // review date
            local.Review.Date = local.MaxReferenceDate.Date;
            local.Review.Date =
              AddMonths(local.Review.Date, local.NumberOfMonthsCommon.Count);
            local.Review.TextDate =
              NumberToString(DateToInt(local.Review.Date), 8);
            local.ApFound.Flag = "N";

            foreach(var item3 in ReadCsePerson())
            {
              local.ApFound.Flag = "Y";
              local.CsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              ExitState = "ACO_NN0000_ALL_OK";
              UseCabReadAdabasPersonBatch();
              local.ApCsePersonsWorkSet.FormattedName =
                TrimEnd(local.ApCsePersonsWorkSet.LastName) + ", " + TrimEnd
                (local.ApCsePersonsWorkSet.FirstName) + " " + local
                .ApCsePersonsWorkSet.MiddleInitial;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.FileAction.ActionEntry = "WR";
                UseSpEabB375FileExtract1();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabReportSend.RptDetail =
                    "Error encountered writing to the Extract File .";
                  ExitState = "SP_ERROR_WRITING_REPORT_FILE";

                  goto ReadEach;
                }

                ++local.ExtractWritten.Count;
              }
              else
              {
                if (AsChar(local.Returned.Type1) == 'A' && Equal
                  (local.Returned.AdabasResponseCd, "0113"))
                {
                  // ***
                  // *** ADABAS not found condition
                  // ***
                  local.NeededToWrite.RptDetail =
                    "Person not found in ADABAS database for: " + local
                    .AdabasInput.Number;
                }
                else
                {
                  // ***
                  // *** Unknown error response returned from ADABAS
                  // ***
                  local.NeededToWrite.RptDetail =
                    "Fatal error in ADABAS for person number: " + local
                    .AdabasInput.Number;
                  local.NeededToWrite.RptDetail =
                    TrimEnd(local.NeededToWrite.RptDetail) + ", Abend Type code: " +
                    local.Returned.Type1 + ", Response code: " + local
                    .Returned.AdabasResponseCd + ", File number: " + local
                    .Returned.AdabasFileNumber + ", File action: " + local
                    .Returned.AdabasFileAction;
                }

                // ****************************
                // *****  Write to Error Report
                // ****************************
                local.EabFileHandling.Action = "WRITE";
                export.NeededToWrite.RptDetail =
                  "Fatal error in ADABAS for person number  : " + local
                  .CsePersonsWorkSet.Number;
                UseCabErrorReport5();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
            }

            if (AsChar(local.ApFound.Flag) == 'N')
            {
              // *********************************************************
              //       Add statements to write to file for no active AP on case.
              // *********************************************************
              local.ApCsePersonsWorkSet.FormattedName =
                "*** No active AP on the case ***";
              local.FileAction.ActionEntry = "WR";
              local.Review.Date = local.Blank.Date;
              UseSpEabB375FileExtract1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabReportSend.RptDetail =
                  "Error encountered writing to the Extract File .";
                ExitState = "SP_ERROR_WRITING_REPORT_FILE";

                goto ReadEach;
              }

              ++local.ExtractWritten.Count;
            }
          }
        }
      }
    }

ReadEach:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.FileAction.ActionEntry = "CL";
    UseSpEabB375FileExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Extract File.";

      // ********
      // *** Write to Error Report
      // ********
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "Records written to the Extract File: " + NumberToString
      (local.ExtractWritten.Count, 11, 5);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report ( total number of offices read ) .";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
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

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

  private void UseCabErrorReport5()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.ApCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.Returned.Assign(useExport.AbendData);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpEabB375FileExtract1()
  {
    var useImport = new SpEabB375FileExtract.Import();
    var useExport = new SpEabB375FileExtract.Export();

    useImport.Review.TextDate = local.Review.TextDate;
    useImport.Ap.FormattedName = local.ApCsePersonsWorkSet.FormattedName;
    useImport.Office.Name = local.Office.Name;
    useImport.Ss.FormattedName = local.Ss.FormattedName;
    useImport.Co.FormattedName = local.Co.FormattedName;
    useImport.Case1.Number = local.Case1.Number;
    useImport.Common.ActionEntry = local.FileAction.ActionEntry;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabB375FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabB375FileExtract2()
  {
    var useImport = new SpEabB375FileExtract.Import();
    var useExport = new SpEabB375FileExtract.Export();

    useImport.Common.ActionEntry = local.FileAction.ActionEntry;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabB375FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(command, "status", local.Open.Status ?? "");
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CoServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingCase.ClosureReason = db.GetNullableString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingCase.Status = db.GetNullableString(reader, 2);
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "type", local.ApCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.FirstOfMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetNullableString(command, "caseNumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        local.MaxReferenceDate.Date = db.GetDate(reader, 0);
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingOffice.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.ExistingOffice.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProvider()
  {
    entities.CoServiceProvider.Populated = false;

    return ReadEach("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CoServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.CoServiceProvider.UserId = db.GetString(reader, 1);
        entities.CoServiceProvider.LastName = db.GetString(reader, 2);
        entities.CoServiceProvider.FirstName = db.GetString(reader, 3);
        entities.CoServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.CoServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvRelationship()
  {
    entities.SsServiceProvider.Populated = false;
    entities.SsCo.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CoServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.SsServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.SsServiceProvider.LastName = db.GetString(reader, 1);
        entities.SsServiceProvider.FirstName = db.GetString(reader, 2);
        entities.SsServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.SsCo.OspEffectiveDate = db.GetDate(reader, 4);
        entities.SsCo.OspRoleCode = db.GetString(reader, 5);
        entities.SsCo.OffGeneratedId = db.GetInt32(reader, 6);
        entities.SsCo.SpdGeneratedId = db.GetInt32(reader, 7);
        entities.SsCo.OspREffectiveDt = db.GetDate(reader, 8);
        entities.SsCo.OspRRoleCode = db.GetString(reader, 9);
        entities.SsCo.OffRGeneratedId = db.GetInt32(reader, 10);
        entities.SsCo.SpdRGeneratedId = db.GetInt32(reader, 11);
        entities.SsCo.CreatedBy = db.GetString(reader, 12);
        entities.SsCo.CreatedDtstamp = db.GetDateTime(reader, 13);
        entities.SsServiceProvider.Populated = true;
        entities.SsCo.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumberOfMonthsCommon.
    /// </summary>
    [JsonPropertyName("numberOfMonthsCommon")]
    public Common NumberOfMonthsCommon
    {
      get => numberOfMonthsCommon ??= new();
      set => numberOfMonthsCommon = value;
    }

    /// <summary>
    /// A value of ApFound.
    /// </summary>
    [JsonPropertyName("apFound")]
    public Common ApFound
    {
      get => apFound ??= new();
      set => apFound = value;
    }

    /// <summary>
    /// A value of TextOfficeNumber.
    /// </summary>
    [JsonPropertyName("textOfficeNumber")]
    public TextWorkArea TextOfficeNumber
    {
      get => textOfficeNumber ??= new();
      set => textOfficeNumber = value;
    }

    /// <summary>
    /// A value of TextReviewDate.
    /// </summary>
    [JsonPropertyName("textReviewDate")]
    public DateWorkArea TextReviewDate
    {
      get => textReviewDate ??= new();
      set => textReviewDate = value;
    }

    /// <summary>
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of SupervisorFound.
    /// </summary>
    [JsonPropertyName("supervisorFound")]
    public Common SupervisorFound
    {
      get => supervisorFound ??= new();
      set => supervisorFound = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of CollectionOfficersCount.
    /// </summary>
    [JsonPropertyName("collectionOfficersCount")]
    public Common CollectionOfficersCount
    {
      get => collectionOfficersCount ??= new();
      set => collectionOfficersCount = value;
    }

    /// <summary>
    /// A value of OfficeCount.
    /// </summary>
    [JsonPropertyName("officeCount")]
    public Common OfficeCount
    {
      get => officeCount ??= new();
      set => officeCount = value;
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of CaseDueForReview.
    /// </summary>
    [JsonPropertyName("caseDueForReview")]
    public Common CaseDueForReview
    {
      get => caseDueForReview ??= new();
      set => caseDueForReview = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public AbendData Returned
    {
      get => returned ??= new();
      set => returned = value;
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Case1 Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Local51.
    /// </summary>
    [JsonPropertyName("local51")]
    public MonitoredActivity Local51
    {
      get => local51 ??= new();
      set => local51 = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of RestartServiceProvider.
    /// </summary>
    [JsonPropertyName("restartServiceProvider")]
    public ServiceProvider RestartServiceProvider
    {
      get => restartServiceProvider ??= new();
      set => restartServiceProvider = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
    }

    /// <summary>
    /// A value of MaxReferenceDate.
    /// </summary>
    [JsonPropertyName("maxReferenceDate")]
    public DateWorkArea MaxReferenceDate
    {
      get => maxReferenceDate ??= new();
      set => maxReferenceDate = value;
    }

    /// <summary>
    /// A value of Run.
    /// </summary>
    [JsonPropertyName("run")]
    public DateWorkArea Run
    {
      get => run ??= new();
      set => run = value;
    }

    /// <summary>
    /// A value of RangeBeg.
    /// </summary>
    [JsonPropertyName("rangeBeg")]
    public DateWorkArea RangeBeg
    {
      get => rangeBeg ??= new();
      set => rangeBeg = value;
    }

    /// <summary>
    /// A value of RangeEnd.
    /// </summary>
    [JsonPropertyName("rangeEnd")]
    public DateWorkArea RangeEnd
    {
      get => rangeEnd ??= new();
      set => rangeEnd = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of CurrentCommon.
    /// </summary>
    [JsonPropertyName("currentCommon")]
    public Common CurrentCommon
    {
      get => currentCommon ??= new();
      set => currentCommon = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of NumberOfMonthsWorkArea.
    /// </summary>
    [JsonPropertyName("numberOfMonthsWorkArea")]
    public WorkArea NumberOfMonthsWorkArea
    {
      get => numberOfMonthsWorkArea ??= new();
      set => numberOfMonthsWorkArea = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Case1 Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of AdabasInput.
    /// </summary>
    [JsonPropertyName("adabasInput")]
    public CsePersonsWorkSet AdabasInput
    {
      get => adabasInput ??= new();
      set => adabasInput = value;
    }

    /// <summary>
    /// A value of ExtractWritten.
    /// </summary>
    [JsonPropertyName("extractWritten")]
    public Common ExtractWritten
    {
      get => extractWritten ??= new();
      set => extractWritten = value;
    }

    private Common numberOfMonthsCommon;
    private Common apFound;
    private TextWorkArea textOfficeNumber;
    private DateWorkArea textReviewDate;
    private DateWorkArea firstOfMonth;
    private Common supervisorFound;
    private DateWorkArea blank;
    private Common collectionOfficersCount;
    private Common officeCount;
    private Common delinOrNever;
    private DateWorkArea review;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Office office;
    private CsePersonsWorkSet ss;
    private CsePersonsWorkSet co;
    private Case1 case1;
    private Common fileAction;
    private Common caseDueForReview;
    private AbendData returned;
    private DateWorkArea max;
    private DateWorkArea currentDateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CaseRole apCaseRole;
    private Case1 open;
    private MonitoredActivity local51;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common fileOpened;
    private EabReportSend neededToWrite;
    private ExitStateWorkArea exitStateWorkArea;
    private Case1 restartCase;
    private ServiceProvider restartServiceProvider;
    private Office restartOffice;
    private TextWorkArea textWorkArea;
    private Common processCountToCommit;
    private DateWorkArea maxReferenceDate;
    private DateWorkArea run;
    private DateWorkArea rangeBeg;
    private DateWorkArea rangeEnd;
    private Common start;
    private Common currentCommon;
    private Common currentPosition;
    private Common fieldNumber;
    private TextWorkArea postion;
    private WorkArea workArea;
    private WorkArea numberOfMonthsWorkArea;
    private Case1 work;
    private CsePersonsWorkSet adabasInput;
    private Common extractWritten;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of SsServiceProvider.
    /// </summary>
    [JsonPropertyName("ssServiceProvider")]
    public ServiceProvider SsServiceProvider
    {
      get => ssServiceProvider ??= new();
      set => ssServiceProvider = value;
    }

    /// <summary>
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
    }

    /// <summary>
    /// A value of SsCo.
    /// </summary>
    [JsonPropertyName("ssCo")]
    public OfficeServiceProvRelationship SsCo
    {
      get => ssCo ??= new();
      set => ssCo = value;
    }

    /// <summary>
    /// A value of SsOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ssOfficeServiceProvider")]
    public OfficeServiceProvider SsOfficeServiceProvider
    {
      get => ssOfficeServiceProvider ??= new();
      set => ssOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("coOfficeServiceProvider")]
    public OfficeServiceProvider CoOfficeServiceProvider
    {
      get => coOfficeServiceProvider ??= new();
      set => coOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredActivity.
    /// </summary>
    [JsonPropertyName("existingMonitoredActivity")]
    public MonitoredActivity ExistingMonitoredActivity
    {
      get => existingMonitoredActivity ??= new();
      set => existingMonitoredActivity = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
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

    private CaseAssignment existingCaseAssignment;
    private ServiceProvider ssServiceProvider;
    private ServiceProvider coServiceProvider;
    private OfficeServiceProvRelationship ssCo;
    private OfficeServiceProvider ssOfficeServiceProvider;
    private OfficeServiceProvider coOfficeServiceProvider;
    private Office existingOffice;
    private CaseRole caseRole;
    private Case1 existingCase;
    private MonitoredActivity existingMonitoredActivity;
    private Infrastructure infrastructure;
    private CsePersonAccount obligor;
    private CsePerson existingCsePerson;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
