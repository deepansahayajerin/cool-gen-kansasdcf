// Program: SP_B731_LEGAL_ACTION_ASSGNMT_RPT, ID: 945041255, model: 746.
// Short name: SWEB731P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B731_LEGAL_ACTION_ASSGNMT_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB731LegalActionAssgnmtRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B731_LEGAL_ACTION_ASSGNMT_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB731LegalActionAssgnmtRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB731LegalActionAssgnmtRpt.
  /// </summary>
  public SpB731LegalActionAssgnmtRpt(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------
    // This procedure produces a report of open legal action assignments for the
    // selected office service provider. Sorting by Case Number and
    // Standard Number is supported.
    // ----------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ---------	--------------	
    // -------------	
    // ----------------------------------
    // 12/15/10  	GVandy		CQ109		Initial Development
    // ----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // ----------------------------------------------------------------------------------
    // Open the error report
    // ----------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();

    // ----------------------------------------------------------------------------------
    // Get the SYSIN Parm Values
    // ----------------------------------------------------------------------------------
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.Sysin.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    // ----------------------------------------------------------------------------------
    // Find the JOB and JOB RUN entries
    // ----------------------------------------------------------------------------------
    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.Sysin.ParameterList ?? Spaces(132);
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      local.NeededToWrite.RptDetail = "Job Run Not Found";
      UseCabErrorReport2();
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    // ----------------------------------------------------------------------------------
    // Determine how the report should be sorted.
    // ----------------------------------------------------------------------------------
    local.SortBy.Text4 = Substring(local.Sysin.ParameterList, 20, 4);

    switch(TrimEnd(local.SortBy.Text4))
    {
      case "CASE":
        break;
      case "STD#":
        break;
      default:
        local.NeededToWrite.RptDetail = "Invalid SORT BY value : " + local
          .SortBy.Text4;
        UseCabErrorReport2();
        ExitState = "CO0000_JOB_RUN_NF_AB";

        return;
    }

    // ----------------------------------------------------------------------------------
    // Update the Status of the Report in JOB_RUN.  Set status TO "PROCESSING"
    // ----------------------------------------------------------------------------------
    try
    {
      UpdateJobRun2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_JOB_RUN_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ----------------------------------------------------------------------------------
    // Perform a DB2 Commit to Free Up the JOB_RUN row.
    // ----------------------------------------------------------------------------------
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // ----------------------------------------------------------------------------------
    // Extract Mandatory Parameter Values
    // ----------------------------------------------------------------------------------
    local.ParmOffice.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.JobRun.ParmInfo, 1, 4));

    if (local.ParmOffice.SystemGeneratedId == 0)
    {
      // -- ERROR: No Office provided
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid office in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmServiceProvider.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.JobRun.ParmInfo, 6, 5));

    if (local.ParmServiceProvider.SystemGeneratedId == 0)
    {
      // -- ERROR: No Service Provider provided.
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid service provider in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmOfficeServiceProvider.RoleCode =
      Substring(entities.JobRun.ParmInfo, 12, 2);

    if (IsEmpty(local.ParmOfficeServiceProvider.RoleCode))
    {
      // -- ERROR: No Office Service Provider Role Code provided
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid osp role code in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmOfficeServiceProvider.EffectiveDate =
      StringToDate(Substring(entities.JobRun.ParmInfo, 15, 10));

    if (Equal(local.ParmOfficeServiceProvider.EffectiveDate,
      local.NullDateWorkArea.Date))
    {
      // -- ERROR: Office Service Provider Effective Date provided
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid osp effective date in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    // ----------------------------------------------------------------------------------
    // Extract Optional Parameter Values
    // ----------------------------------------------------------------------------------
    local.ParmLegalAction.StandardNumber =
      Substring(entities.JobRun.ParmInfo, 26, 12);
    local.ParmTribunal.Identifier =
      (int)StringToNumber(Substring(entities.JobRun.ParmInfo, 39, 9));
    local.ParmLegalAction.CreatedTstamp =
      Timestamp(Substring(entities.JobRun.ParmInfo, 49, 10));
    local.ParmLegalAction.Classification =
      Substring(entities.JobRun.ParmInfo, 60, 1);
    local.ParmLegalAction.ActionTaken =
      Substring(entities.JobRun.ParmInfo, 62, 30);
    local.ParmLegalAction.FiledDate =
      StringToDate(Substring(entities.JobRun.ParmInfo, 93, 10));
    local.ParmCase.Number = Substring(entities.JobRun.ParmInfo, 104, 10);
    local.ParmLegalActionAssigment.OverrideInd =
      Substring(entities.JobRun.ParmInfo, 115, 1);

    // ----------------------------------------------------------------------------------
    // Perform Initial Qualifying Reads
    // ----------------------------------------------------------------------------------
    if (ReadServiceProvider())
    {
      local.FormattedServiceProvider.FirstName =
        entities.ServiceProvider.FirstName;
      local.FormattedServiceProvider.LastName =
        entities.ServiceProvider.LastName;
      local.FormattedServiceProvider.MiddleInitial =
        entities.ServiceProvider.MiddleInitial;
      UseSiFormatCsePersonName();
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (!ReadOfficeServiceProviderOffice())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    if (local.ParmTribunal.Identifier != 0)
    {
      if (!ReadTribunal2())
      {
        ExitState = "TRIBUNAL_NF";

        return;
      }

      if (!ReadFips())
      {
        if (!ReadFipsTribAddress())
        {
          ExitState = "FIPS_NF";

          return;
        }
      }
    }

    // ----------------------------------------------------------------------------------
    // Retrieve Data and Build the Report
    // ----------------------------------------------------------------------------------
    // -- Format Header Lines and add to REPORT_DATA.
    for(local.Common.Count = 1; local.Common.Count <= 17; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.ReportData.FirstPageOnlyInd = "N";
          local.ReportData.LineControl = "";
          local.ReportData.LineText = local.Job.Name + Substring
            (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 58)
            + "STATE OF KANSAS" + Substring
            (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 41)
            + "PAGE:     1";

          break;
        case 2:
          local.TextMm.Text2 =
            NumberToString(Month(entities.JobRun.StartTimestamp), 14, 2);
          local.TextDd.Text2 =
            NumberToString(Day(entities.JobRun.StartTimestamp), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(Year(entities.JobRun.StartTimestamp), 12, 4);
          local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;
          local.ReportData.FirstPageOnlyInd = "N";
          local.ReportData.LineControl = "";
          local.ReportData.LineText = "RUN DATE: " + local.TextDate.Text10 + Substring
            (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 36)
            + "DEPARTMENT FOR CHILDREN AND FAMILIES";

          break;
        case 3:
          local.ReportData.FirstPageOnlyInd = "N";
          local.ReportData.LineControl = "";
          local.ReportData.LineText =
            Substring(local.NullReportData.LineText,
            ReportData.LineText_MaxLength, 1, 62) + "KAECSES - CHILD SUPPORT";

          break;
        case 4:
          local.ReportData.FirstPageOnlyInd = "N";
          local.ReportData.LineControl = "01";

          if (Equal(local.SortBy.Text4, "CASE"))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 58) + "LEGAL ACTION ASSIGNMENT BY CASE";
              
          }
          else if (Equal(local.SortBy.Text4, "STD#"))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 53) + "LEGAL ACTION ASSIGNMENT BY STANDARD NUMBER";
              
          }

          break;
        case 5:
          // -- This line is written at the end of the procedure since we need 
          // the total assignment count to place here.
          continue;
        case 6:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "01";
          local.ReportData.LineText =
            Substring(local.NullReportData.LineText,
            ReportData.LineText_MaxLength, 1, 4) + "OFFICE:  " + NumberToString
            (entities.Office.SystemGeneratedId, 12, 4) + "  " + entities
            .Office.Name + "  SERVICE PROVIDER: " + NumberToString
            (entities.ServiceProvider.SystemGeneratedId, 12, 4) + "  " + local
            .FormattedServiceProvider.FormattedName;

          break;
        case 7:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "01";

          if (Equal(local.SortBy.Text4, "CASE"))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "ORDER BY: CASE NUMBER";
          }
          else if (Equal(local.SortBy.Text4, "STD#"))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "ORDER BY: STANDARD NUMBER";
              
          }

          break;
        case 8:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "01";

          if (IsEmpty(local.ParmCase.Number))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "FILTERS:  CASE NO:      N/A";
              
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "FILTERS:  CASE NO:      " +
              local.ParmCase.Number;
          }

          break;
        case 9:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";

          if (IsEmpty(local.ParmLegalAction.StandardNumber))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "STANDARD NO:  N/A";
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "STANDARD NO:  " + (
                local.ParmLegalAction.StandardNumber ?? "");
          }

          break;
        case 10:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";

          if (local.ParmTribunal.Identifier == 0)
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "TRIB ST/CO:   N/A";
          }
          else if (entities.Fips.Populated)
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "TRIB ST/CO:   " + entities
              .Fips.StateAbbreviation + " " + entities.Fips.CountyAbbreviation;
          }
          else if (entities.FipsTribAddress.Populated)
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "TRIB ST/CO:   " + entities
              .FipsTribAddress.Country;
          }

          break;
        case 11:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";

          if (IsEmpty(local.ParmLegalAction.Classification))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "CLASS:        N/A";
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "CLASS:        " + local
              .ParmLegalAction.Classification;
          }

          break;
        case 12:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";

          if (IsEmpty(local.ParmLegalAction.ActionTaken))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "ACTION TAKEN: N/A";
          }
          else
          {
            UseLeGetActionTakenDescription2();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "ACTION TAKEN: " + local
              .ActionTaken.Text25;
          }

          break;
        case 13:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";

          if (IsEmpty(local.ParmLegalActionAssigment.OverrideInd))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "OVERRIDE:     N/A";
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "OVERRIDE:     " + local
              .ParmLegalActionAssigment.OverrideInd;
          }

          break;
        case 14:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "01";
          local.TextMm.Text2 =
            NumberToString(Month(local.ParmLegalAction.CreatedTstamp), 14, 2);
          local.TextDd.Text2 =
            NumberToString(Day(local.ParmLegalAction.CreatedTstamp), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(Year(local.ParmLegalAction.CreatedTstamp), 12, 4);
          local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;

          if (Equal(local.ParmLegalAction.CreatedTstamp,
            local.NullDateWorkArea.Timestamp))
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "STARTING: CREATED DATE: N/A";
              
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 4) + "STARTING: CREATED DATE: " +
              local.TextDate.Text10;
          }

          break;
        case 15:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";
          local.TextMm.Text2 =
            NumberToString(Month(local.ParmLegalAction.FiledDate), 14, 2);
          local.TextDd.Text2 =
            NumberToString(Day(local.ParmLegalAction.FiledDate), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(Year(local.ParmLegalAction.FiledDate), 12, 4);
          local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;

          if (Equal(local.ParmLegalAction.FiledDate, local.NullDateWorkArea.Date))
            
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "FILED DATE:   N/A";
          }
          else
          {
            local.ReportData.LineText =
              Substring(local.NullReportData.LineText,
              ReportData.LineText_MaxLength, 1, 14) + "FILED DATE:   " + local
              .TextDate.Text10;
          }

          break;
        case 16:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "02";
          local.ReportData.LineText =
            "SEQ   CASE        STANDARD NUMBER      CLASS  ACTION TAKEN               OVR  CREATED     FILED";
            

          break;
        case 17:
          local.ReportData.FirstPageOnlyInd = "Y";
          local.ReportData.LineControl = "";
          local.ReportData.LineText =
            "====================================================================================================================================";
            

          break;
        default:
          break;
      }

      try
      {
        CreateReportData6();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_REPORT_DATA_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (Equal(local.SortBy.Text4, "CASE"))
    {
      if (IsEmpty(local.ParmCase.Number))
      {
        if (IsEmpty(local.ParmLegalAction.StandardNumber))
        {
          // -----------------------------------------------------------------------------
          // -- No case number or standard number entered as search criteria.
          // -----------------------------------------------------------------------------
          // -- Write all the legal actions to the report that are not 
          // associated to a cse case.
          //    These should be listed before legal actions with case numbers.
          foreach(var item in ReadLegalActionLegalActionAssigment2())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            // -- Count the total number of assignments that meet the filter 
            // criteria.
            ++local.AsgnCount.Count;

            foreach(var item1 in ReadCase1())
            {
              goto ReadEach1;
            }

            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            ++local.SequenceNumber.Count;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            try
            {
              CreateReportData4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CO0000_REPORT_DATA_AE_AB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CO0000_REPORT_DATA_PV_AB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

ReadEach1:
            ;
          }

          // -- Write all the legal actions to the report that are associated to
          // a cse case.
          foreach(var item in ReadLegalActionLegalActionAssigmentCase3())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            ++local.SequenceNumber.Count;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            try
            {
              CreateReportData1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CO0000_REPORT_DATA_AE_AB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CO0000_REPORT_DATA_PV_AB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        else
        {
          // -----------------------------------------------------------------------------
          // -- Standard number entered as search criteria.  Case number was not
          // entered.
          // -----------------------------------------------------------------------------
          // -- Write all the legal actions to the report that are not 
          // associated to a cse case.
          //    These should be listed before legal actions with case numbers.
          foreach(var item in ReadLegalActionLegalActionAssigment1())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            // -- Count the total number of assignments that meet the filter 
            // criteria.
            ++local.AsgnCount.Count;

            foreach(var item1 in ReadCase1())
            {
              goto ReadEach2;
            }

            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            ++local.SequenceNumber.Count;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            try
            {
              CreateReportData2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CO0000_REPORT_DATA_AE_AB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CO0000_REPORT_DATA_PV_AB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

ReadEach2:
            ;
          }

          // -- Write all the legal actions to the report that are associated to
          // a cse case.
          foreach(var item in ReadLegalActionLegalActionAssigmentCase2())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            ++local.SequenceNumber.Count;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            try
            {
              CreateReportData3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CO0000_REPORT_DATA_AE_AB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CO0000_REPORT_DATA_PV_AB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
      else
      {
        // -----------------------------------------------------------------------------
        // -- Case number entered as search criteria.
        // -----------------------------------------------------------------------------
        // -- Write all the legal actions to the report that are associated to a
        // cse case.
        foreach(var item in ReadLegalActionLegalActionAssigmentCase1())
        {
          if (local.ParmTribunal.Identifier != 0)
          {
            if (!ReadTribunal1())
            {
              continue;
            }
          }

          // -- Count the total number of assignments that meet the filter 
          // criteria.
          ++local.AsgnCount.Count;
          UseLeGetActionTakenDescription1();
          local.ActionTaken.Text25 = local.CodeValue.Description;
          ++local.SequenceNumber.Count;
          local.TextMm.Text2 =
            NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
          local.TextDd.Text2 =
            NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
          local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;

          if (Equal(entities.LegalAction.FiledDate, local.NullDateWorkArea.Date))
            
          {
            local.TextDate2.Text10 = "";
          }
          else
          {
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
            local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;
          }

          try
          {
            CreateReportData3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CO0000_REPORT_DATA_AE_AB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CO0000_REPORT_DATA_PV_AB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else if (Equal(local.SortBy.Text4, "STD#"))
    {
      if (IsEmpty(local.ParmCase.Number))
      {
        if (IsEmpty(local.ParmLegalAction.StandardNumber))
        {
          // -----------------------------------------------------------------------------
          // -- No case number or standard number entered as search criteria.
          // -----------------------------------------------------------------------------
          // -- Write all the legal actions to the report that are not 
          // associated to a cse case.
          //    These should be listed before legal actions with case numbers.
          foreach(var item in ReadLegalActionLegalActionAssigment2())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            // -- Count the total number of assignments that meet the filter 
            // criteria.
            ++local.AsgnCount.Count;
            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            local.CaseFound.Flag = "N";

            foreach(var item1 in ReadCase2())
            {
              ++local.SequenceNumber.Count;
              local.CaseFound.Flag = "Y";

              try
              {
                CreateReportData3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CO0000_REPORT_DATA_AE_AB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CO0000_REPORT_DATA_PV_AB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            if (AsChar(local.CaseFound.Flag) == 'N')
            {
              ++local.SequenceNumber.Count;

              try
              {
                CreateReportData2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CO0000_REPORT_DATA_AE_AB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CO0000_REPORT_DATA_PV_AB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        else
        {
          // -----------------------------------------------------------------------------
          // -- Standard number entered as search criteria.  Case number was not
          // entered.
          // -----------------------------------------------------------------------------
          foreach(var item in ReadLegalActionLegalActionAssigment1())
          {
            if (local.ParmTribunal.Identifier != 0)
            {
              if (!ReadTribunal1())
              {
                continue;
              }
            }

            // -- Count the total number of assignments that meet the filter 
            // criteria.
            ++local.AsgnCount.Count;
            UseLeGetActionTakenDescription1();
            local.ActionTaken.Text25 = local.CodeValue.Description;
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
            local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (Equal(entities.LegalAction.FiledDate,
              local.NullDateWorkArea.Date))
            {
              local.TextDate2.Text10 = "";
            }
            else
            {
              local.TextMm.Text2 =
                NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
              local.TextDd.Text2 =
                NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
              local.TextYyyy.Text4 =
                NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
              local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
                .TextDd.Text2 + "/" + local.TextYyyy.Text4;
            }

            local.CaseFound.Flag = "N";

            foreach(var item1 in ReadCase2())
            {
              ++local.SequenceNumber.Count;
              local.CaseFound.Flag = "Y";

              try
              {
                CreateReportData1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CO0000_REPORT_DATA_AE_AB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CO0000_REPORT_DATA_PV_AB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            if (AsChar(local.CaseFound.Flag) == 'N')
            {
              ++local.SequenceNumber.Count;

              try
              {
                CreateReportData4();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CO0000_REPORT_DATA_AE_AB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CO0000_REPORT_DATA_PV_AB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
      }
      else
      {
        // -----------------------------------------------------------------------------
        // -- Case number entered as search criteria.
        // -----------------------------------------------------------------------------
        // -- Write all the legal actions to the report that are associated to a
        // cse case.
        foreach(var item in ReadLegalActionLegalActionAssigmentCase4())
        {
          if (local.ParmTribunal.Identifier != 0)
          {
            if (!ReadTribunal1())
            {
              continue;
            }
          }

          // -- Count the total number of assignments that meet the filter 
          // criteria.
          ++local.AsgnCount.Count;
          UseLeGetActionTakenDescription1();
          local.ActionTaken.Text25 = local.CodeValue.Description;
          ++local.SequenceNumber.Count;
          local.TextMm.Text2 =
            NumberToString(Month(entities.LegalAction.CreatedTstamp), 14, 2);
          local.TextDd.Text2 =
            NumberToString(Day(entities.LegalAction.CreatedTstamp), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(Year(entities.LegalAction.CreatedTstamp), 12, 4);
          local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;

          if (Equal(entities.LegalAction.FiledDate, local.NullDateWorkArea.Date))
            
          {
            local.TextDate2.Text10 = "";
          }
          else
          {
            local.TextMm.Text2 =
              NumberToString(Month(entities.LegalAction.FiledDate), 14, 2);
            local.TextDd.Text2 =
              NumberToString(Day(entities.LegalAction.FiledDate), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(Year(entities.LegalAction.FiledDate), 12, 4);
            local.TextDate2.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;
          }

          try
          {
            CreateReportData3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CO0000_REPORT_DATA_AE_AB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CO0000_REPORT_DATA_PV_AB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    if (local.AsgnCount.Count == 0)
    {
      try
      {
        CreateReportData5();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_REPORT_DATA_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "No Data Retrieved for the report.";
      UseCabErrorReport3();
    }

    // ----------------------------------------------------------------------------------
    // -- Write the total number of assignments in the header record.
    // ----------------------------------------------------------------------------------
    local.ReportData.SequenceNumber = 5;
    local.ReportData.FirstPageOnlyInd = "Y";
    local.ReportData.LineControl = "01";
    local.ReportData.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      4) + "TOTAL ASSIGNMENTS: " + NumberToString
      (local.AsgnCount.Count, 10, 6);

    try
    {
      CreateReportData7();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_REPORT_DATA_AE_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_REPORT_DATA_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ----------------------------------------------------------------------------------
    // Update the Status of the Report in JOB_RUN.
    // ----------------------------------------------------------------------------------
    if (Equal(entities.JobRun.OutputType, "ONLINE"))
    {
      try
      {
        UpdateJobRun1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      try
      {
        UpdateJobRun3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeGetActionTakenDescription1()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = entities.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    local.CodeValue.Description = useExport.CodeValue.Description;
  }

  private void UseLeGetActionTakenDescription2()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = local.ParmLegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    local.CodeValue.Description = useExport.CodeValue.Description;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.FormattedServiceProvider);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.FormattedServiceProvider.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void CreateReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.SequenceNumber.Count;
    var firstPageOnlyInd = "N";
    var lineText =
      Substring(NumberToString(local.SequenceNumber.Count, 12, 4) + "  " + entities
      .Case1.Number + "  " + Substring
      (entities.LegalAction.StandardNumber,
      LegalAction.StandardNumber_MaxLength, 1, 20) + "   " + entities
      .LegalAction.Classification + "    " + local.ActionTaken.Text25 + "   " +
      entities.LegalActionAssigment.OverrideInd + "   " + local
      .TextDate.Text10 + "  " + local.TextDate2.Text10, 1, 132);
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData1",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", "");
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = "";
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.SequenceNumber.Count;
    var firstPageOnlyInd = "N";
    var lineText =
      Substring(NumberToString(local.SequenceNumber.Count, 12, 4) + "  " + "          " +
      "  " + Substring
      (entities.LegalAction.StandardNumber,
      LegalAction.StandardNumber_MaxLength, 1, 20) + "   " + entities
      .LegalAction.Classification + "    " + local.ActionTaken.Text25 + "   " +
      entities.LegalActionAssigment.OverrideInd + "   " + local
      .TextDate.Text10 + "  " + local.TextDate2.Text10, 1, 132);
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData2",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", "");
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = "";
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData3()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.SequenceNumber.Count;
    var firstPageOnlyInd = "N";
    var lineText =
      Substring(NumberToString(local.SequenceNumber.Count, 12, 4) + "  " + entities
      .Case1.Number + "  " + Substring
      (entities.LegalAction.StandardNumber,
      LegalAction.StandardNumber_MaxLength, 1, 20) + "   " + entities
      .LegalAction.Classification + "    " + local.ActionTaken.Text25 + "   " +
      entities.LegalActionAssigment.OverrideInd + "   " + local
      .TextDate.Text10 + "  " + local.TextDate2.Text10, 1, 132);
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData3",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", "");
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = "";
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData4()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.SequenceNumber.Count;
    var firstPageOnlyInd = "N";
    var lineText =
      Substring(NumberToString(local.SequenceNumber.Count, 12, 4) + "  " + "          " +
      "  " + Substring
      (entities.LegalAction.StandardNumber,
      LegalAction.StandardNumber_MaxLength, 1, 20) + "   " + entities
      .LegalAction.Classification + "    " + local.ActionTaken.Text25 + "   " +
      entities.LegalActionAssigment.OverrideInd + "   " + local
      .TextDate.Text10 + "  " + local.TextDate2.Text10, 1, 132);
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData4",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", "");
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = "";
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData5()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = 1;
    var firstPageOnlyInd = "N";
    var lineControl = "01";
    var lineText = "No Data Found for Search Criteria.";
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData5",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = lineControl;
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData6()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "H";
    var sequenceNumber = local.Common.Count;
    var firstPageOnlyInd = local.ReportData.FirstPageOnlyInd ?? "";
    var lineControl = local.ReportData.LineControl;
    var lineText = local.ReportData.LineText;
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData6",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = lineControl;
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private void CreateReportData7()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "H";
    var sequenceNumber = local.ReportData.SequenceNumber;
    var firstPageOnlyInd = local.ReportData.FirstPageOnlyInd ?? "";
    var lineControl = local.ReportData.LineControl;
    var lineText = local.ReportData.LineText;
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData7",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = lineControl;
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
  }

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.State = db.GetString(reader, 1);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 2);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadJob()
  {
    entities.Job.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.Job.Name = db.GetString(reader, 0);
        entities.Job.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.JobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.Job.Name);
        db.SetInt32(command, "systemGenId", local.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.JobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.JobRun.EndTimestamp = db.GetNullableDateTime(reader, 1);
        entities.JobRun.Status = db.GetString(reader, 2);
        entities.JobRun.OutputType = db.GetString(reader, 3);
        entities.JobRun.ErrorMsg = db.GetNullableString(reader, 4);
        entities.JobRun.ParmInfo = db.GetNullableString(reader, 5);
        entities.JobRun.JobName = db.GetString(reader, 6);
        entities.JobRun.SystemGenId = db.GetInt32(reader, 7);
        entities.JobRun.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigment1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigment1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.ParmLegalAction.StandardNumber ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigment2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigment2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigmentCase1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigmentCase1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNumber", local.ParmLegalAction.StandardNumber ?? ""
          );
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
        db.SetString(command, "casNumber", local.ParmCase.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.Case1.Number = db.GetString(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigmentCase2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigmentCase2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.ParmLegalAction.StandardNumber ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.Case1.Number = db.GetString(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigmentCase3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigmentCase3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.Case1.Number = db.GetString(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigmentCase4()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigmentCase4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNumber", local.ParmLegalAction.StandardNumber ?? ""
          );
        db.SetDateTime(
          command, "createdTstamp",
          local.ParmLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", local.ParmLegalAction.Classification);
        db.SetString(command, "actionTaken", local.ParmLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          local.ParmLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", local.ParmLegalActionAssigment.OverrideInd);
        db.SetString(command, "casNumber", local.ParmCase.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 11);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 13);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.Case1.Number = db.GetString(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "officeId", local.ParmOffice.SystemGeneratedId);
        db.SetString(
          command, "roleCode", local.ParmOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          local.ParmOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.Name = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          local.ParmServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier1",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetInt32(command, "identifier2", local.ParmTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.ParmTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var status = "COMPLETE";

    entities.JobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.JobRun.JobName);
        db.SetInt32(command, "systemGenId", entities.JobRun.SystemGenId);
      });

    entities.JobRun.Status = status;
    entities.JobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var status = "PROCESSING";

    entities.JobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.JobRun.JobName);
        db.SetInt32(command, "systemGenId", entities.JobRun.SystemGenId);
      });

    entities.JobRun.Status = status;
    entities.JobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var status = "WAIT";

    entities.JobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.JobRun.JobName);
        db.SetInt32(command, "systemGenId", entities.JobRun.SystemGenId);
      });

    entities.JobRun.Status = status;
    entities.JobRun.Populated = true;
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
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public SpTextWorkArea SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public WorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of TextDate2.
    /// </summary>
    [JsonPropertyName("textDate2")]
    public WorkArea TextDate2
    {
      get => textDate2 ??= new();
      set => textDate2 = value;
    }

    /// <summary>
    /// A value of SequenceNumber.
    /// </summary>
    [JsonPropertyName("sequenceNumber")]
    public Common SequenceNumber
    {
      get => sequenceNumber ??= new();
      set => sequenceNumber = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of ParmLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("parmLegalActionAssigment")]
    public LegalActionAssigment ParmLegalActionAssigment
    {
      get => parmLegalActionAssigment ??= new();
      set => parmLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of ParmTribunal.
    /// </summary>
    [JsonPropertyName("parmTribunal")]
    public Tribunal ParmTribunal
    {
      get => parmTribunal ??= new();
      set => parmTribunal = value;
    }

    /// <summary>
    /// A value of ParmLegalAction.
    /// </summary>
    [JsonPropertyName("parmLegalAction")]
    public LegalAction ParmLegalAction
    {
      get => parmLegalAction ??= new();
      set => parmLegalAction = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Sysin.
    /// </summary>
    [JsonPropertyName("sysin")]
    public ProgramProcessingInfo Sysin
    {
      get => sysin ??= new();
      set => sysin = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
    }

    /// <summary>
    /// A value of ParmServiceProvider.
    /// </summary>
    [JsonPropertyName("parmServiceProvider")]
    public ServiceProvider ParmServiceProvider
    {
      get => parmServiceProvider ??= new();
      set => parmServiceProvider = value;
    }

    /// <summary>
    /// A value of ParmCase.
    /// </summary>
    [JsonPropertyName("parmCase")]
    public Case1 ParmCase
    {
      get => parmCase ??= new();
      set => parmCase = value;
    }

    /// <summary>
    /// A value of ParmOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("parmOfficeServiceProvider")]
    public OfficeServiceProvider ParmOfficeServiceProvider
    {
      get => parmOfficeServiceProvider ??= new();
      set => parmOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ParmOffice.
    /// </summary>
    [JsonPropertyName("parmOffice")]
    public Office ParmOffice
    {
      get => parmOffice ??= new();
      set => parmOffice = value;
    }

    /// <summary>
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
    }

    /// <summary>
    /// A value of FormattedServiceProvider.
    /// </summary>
    [JsonPropertyName("formattedServiceProvider")]
    public CsePersonsWorkSet FormattedServiceProvider
    {
      get => formattedServiceProvider ??= new();
      set => formattedServiceProvider = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public WorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
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

    private SpTextWorkArea sortBy;
    private WorkArea actionTaken;
    private CodeValue codeValue;
    private WorkArea textDate2;
    private Common sequenceNumber;
    private ReportData reportData;
    private Common common;
    private Common caseFound;
    private LegalActionAssigment parmLegalActionAssigment;
    private Tribunal parmTribunal;
    private LegalAction parmLegalAction;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private ServiceProvider parmServiceProvider;
    private Case1 parmCase;
    private OfficeServiceProvider parmOfficeServiceProvider;
    private Office parmOffice;
    private Common asgnCount;
    private CsePersonsWorkSet formattedServiceProvider;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private External external;
    private ReportData nullReportData;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private WorkArea textDate;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

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
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private Case1 case1;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private Job job;
    private JobRun jobRun;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CaseRole caseRole;
    private ReportData reportData;
  }
#endregion
}
