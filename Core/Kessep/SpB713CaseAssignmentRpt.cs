// Program: SP_B713_CASE_ASSIGNMENT_RPT, ID: 371133423, model: 746.
// Short name: SWEP713B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B713_CASE_ASSIGNMENT_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB713CaseAssignmentRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B713_CASE_ASSIGNMENT_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB713CaseAssignmentRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB713CaseAssignmentRpt.
  /// </summary>
  public SpB713CaseAssignmentRpt(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // resp:  Service Plan
    // This procedure produces a report of open case assignments for the
    // selected office service provider. Sorting by Case Number and
    // AP Name is supported.
    // *****************************************************************
    // Date		Developer	Request #	Description
    // --------------------------------------------------------------------
    // 12/15/00  	Maureen Brown			Initial Development
    // 04/14/2005	M J Quinn	PR 180473	SUMMARIZE statements replaced by READ EACH
    // 						to improve execution speed.
    // 02/11/2010	J. Huss		CQ 15389	Replaced hardcoded job name with job name
    // 						from PPI record
    // 12/03/2010	GVandy  	CQ109 seg B  	Add search field for override 
    // indicator.
    // 10/03/2013	GVandy		CQ41762		Expand report to support 25,000 cases.
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Group.Index = -1;
    local.HardcodedOpen.Status = "O";
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
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

    local.OrderBy.Text10 = Substring(local.Sysin.ParameterList, 20, 10);

    switch(TrimEnd(local.OrderBy.Text10))
    {
      case "CASE":
        break;
      case "AP":
        break;
      default:
        ExitState = "FN0000_SYSIN_PARM_FORMAT_ERR_A";

        return;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    // **** SET status TO "PROCESSING"
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

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // *****************************************************************
    // Extract Filters from Parameter Information
    // *****************************************************************
    // **** IF local job_run parameter_information = SPACES ........
    //      ERROR: Invalid Parm Information!!!!
    //      Update JOB_RUN Error Message & Get Out!!!!
    // : Mandatory Parm Values.
    local.ParmServiceProvider.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 1, 5));

    if (local.ParmServiceProvider.SystemGeneratedId == 0)
    {
      // **** ERROR: Service Provider!!!!
      //      Update JOB_RUN Error Message & Get Out!!!!
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid service provider in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmOffice.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 7, 4));

    if (local.ParmOffice.SystemGeneratedId == 0)
    {
      // **** ERROR:Office!!!!
      //      Update JOB_RUN Error Message & Get Out!!!!
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid office in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmOfficeServiceProvider.RoleCode =
      Substring(entities.ExistingJobRun.ParmInfo, 12, 2);

    if (IsEmpty(local.ParmOfficeServiceProvider.RoleCode))
    {
      // **** ERROR: Service Provider!!!!
      //      Update JOB_RUN Error Message & Get Out!!!!
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid osp role code in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.ParmOfficeServiceProvider.EffectiveDate =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 15, 10));

    if (Equal(local.ParmOfficeServiceProvider.EffectiveDate,
      local.NullDateWorkArea.Date))
    {
      // **** ERROR: Service Provider!!!!
      //      Update JOB_RUN Error Message & Get Out!!!!
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid osp effective date in job_run parameter info.";
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    // : Optional Parm Values.
    local.ParmAp.Text17 = Substring(entities.ExistingJobRun.ParmInfo, 26, 17);
    local.ParmApFi.Text1 = Substring(entities.ExistingJobRun.ParmInfo, 44, 1);
    local.ParmAr.Text17 = Substring(entities.ExistingJobRun.ParmInfo, 46, 17);
    local.ParmArFi.Text1 = Substring(entities.ExistingJobRun.ParmInfo, 64, 1);
    local.ParmCase.Number = Substring(entities.ExistingJobRun.ParmInfo, 66, 10);
    local.ParmProgram.Code = Substring(entities.ExistingJobRun.ParmInfo, 77, 3);
    local.ParmCaseFuncWorkSet.FuncText1 =
      Substring(entities.ExistingJobRun.ParmInfo, 81, 1);
    local.ParmCaseAssignment.OverrideInd =
      Substring(entities.ExistingJobRun.ParmInfo, 83, 1);
    local.ParmTribunal1.Text4 =
      Substring(entities.ExistingJobRun.ParmInfo, 85, 4);

    if (!IsEmpty(local.ParmTribunal1.Text4))
    {
      local.ParmTribunal2.Identifier =
        (int)StringToNumber(local.ParmTribunal1.Text4);
    }

    if (!IsEmpty(local.ParmAp.Text17))
    {
      local.ApNameLength.Count = Length(TrimEnd(local.ParmAp.Text17));
    }

    if (!IsEmpty(local.ParmAr.Text17))
    {
      local.ArNameLength.Count = Length(TrimEnd(local.ParmAr.Text17));
    }

    // *****************************************************************
    // Perform Initial Qualifying Reads
    // *****************************************************************
    if (ReadServiceProvider())
    {
      local.FormattedServiceProvider.FirstName =
        entities.ExistingServiceProvider.FirstName;
      local.FormattedServiceProvider.LastName =
        entities.ExistingServiceProvider.LastName;
      local.FormattedServiceProvider.MiddleInitial =
        entities.ExistingServiceProvider.MiddleInitial;
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

    // *****************************************************************
    // Mainline Process
    // *****************************************************************
    foreach(var item in ReadCaseCaseAssignment())
    {
      // ************************************************
      // Determine the Case Level Function.
      // ************************************************
      local.CaseFuncWorkSet.FuncText1 = "";
      UseSiCabReturnCaseFunction();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsEmpty(local.CaseFuncWorkSet.FuncText1))
        {
          local.CaseFuncWorkSet.FuncText1 = "L";
        }

        if (!IsEmpty(local.ParmCaseFuncWorkSet.FuncText1))
        {
          if (AsChar(local.CaseFuncWorkSet.FuncText1) != AsChar
            (local.ParmCaseFuncWorkSet.FuncText1))
          {
            continue;
          }
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport3();

        continue;
      }

      // : Prepare the AP data.
      if (ReadCsePerson2())
      {
        local.FormattedAp.Number = entities.ExistingKeyOnly.Number;
        UseSiReadCsePersonBatch1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.FormattedAp.Number = entities.ExistingKeyOnly.Number;
          local.FormattedAp.FormattedName = "ERROR: ADABAS N/A: " + local
            .FormattedAp.Number;
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(local.ParmAp.Text17))
        {
          if (!Equal(local.ParmAp.Text17, local.FormattedAp.LastName, 1,
            local.ApNameLength.Count))
          {
            continue;
          }
        }

        if (!IsEmpty(local.ParmApFi.Text1))
        {
          if (AsChar(local.ParmApFi.Text1) != CharAt
            (local.FormattedAp.FirstName, 1))
          {
            continue;
          }
        }
      }
      else if (!IsEmpty(local.ParmAp.Text17))
      {
        continue;
      }
      else
      {
        local.FormattedAp.FormattedName = "ERROR: AP NOT FOUND";
      }

      // : Prepare the AR Data.
      if (ReadCsePerson1())
      {
        local.FormattedAr.Number = entities.ExistingKeyOnly.Number;
        UseSiReadCsePersonBatch2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.FormattedAr.Number = entities.ExistingKeyOnly.Number;
          local.FormattedAr.FormattedName = "ERROR: ADABAS N/A: " + local
            .FormattedAr.Number;
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(local.ParmAr.Text17))
        {
          if (!Equal(local.ParmAr.Text17, local.FormattedAr.LastName, 1,
            local.ArNameLength.Count))
          {
            continue;
          }
        }

        if (!IsEmpty(local.ParmArFi.Text1))
        {
          if (AsChar(local.ParmArFi.Text1) != CharAt
            (local.FormattedAr.FirstName, 1))
          {
            continue;
          }
        }
      }
      else if (!IsEmpty(local.ParmAr.Text17))
      {
        continue;
      }
      else
      {
        local.FormattedAr.FormattedName = "ERROR: AR NOT FOUND";
      }

      // ************************************************
      // Determine the Case Level Program
      // ************************************************
      UseSiReadCaseProgramType();

      if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
      {
        local.Tmp.Code = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (!IsEmpty(local.ParmProgram.Code))
      {
        if (!Equal(local.ParmProgram.Code, local.Tmp.Code))
        {
          continue;
        }
      }

      if (local.ParmTribunal2.Identifier > 0)
      {
        if (!ReadLegalAction())
        {
          continue;
        }
      }

      // ************************************************
      // Prepare the Case Unit count data.
      // ************************************************
      // ***************************************************************************************************
      // M J Quinn  15 April 2005     PR  180473  SUMMARIZE statements replaced 
      // by READ EACH
      // 	                                 to improve execution speed.
      // 
      // ***************************************************************************************************
      local.Cau.Count = 0;

      foreach(var item1 in ReadCaseUnit())
      {
        ++local.Cau.Count;
      }

      if (local.ParmTribunal2.Identifier > 0)
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.Case1.Number = entities.ExistingCase.Number;
        local.Group.Update.Tribunal.Identifier = local.ParmTribunal2.Identifier;
        MoveCsePersonsWorkSet(local.FormattedAp, local.Group.Update.Ap);
        MoveCsePersonsWorkSet(local.FormattedAr, local.Group.Update.Ar);
        MoveCaseAssignment(entities.ExistingCaseAssignment,
          local.Group.Update.CaseAssignment);
        local.Group.Update.Program.Code = local.Tmp.Code;
        local.Group.Update.Func.Text1 = local.CaseFuncWorkSet.FuncText1;
        local.Group.Update.Cau.Count = local.Cau.Count;
      }
      else
      {
        local.TribunalFound.Flag = "";

        foreach(var item1 in ReadTribunal2())
        {
          local.TribunalFound.Flag = "Y";

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number = entities.ExistingCase.Number;
          local.Group.Update.Tribunal.Identifier = entities.Tribunal.Identifier;
          MoveCsePersonsWorkSet(local.FormattedAp, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.FormattedAr, local.Group.Update.Ar);
          MoveCaseAssignment(entities.ExistingCaseAssignment,
            local.Group.Update.CaseAssignment);
          local.Group.Update.Program.Code = local.Tmp.Code;
          local.Group.Update.Func.Text1 = local.CaseFuncWorkSet.FuncText1;
          local.Group.Update.Cau.Count = local.Cau.Count;
        }

        if (IsEmpty(local.TribunalFound.Flag))
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number = entities.ExistingCase.Number;
          local.Group.Update.Tribunal.Identifier = 0;
          MoveCsePersonsWorkSet(local.FormattedAp, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.FormattedAr, local.Group.Update.Ar);
          MoveCaseAssignment(entities.ExistingCaseAssignment,
            local.Group.Update.CaseAssignment);
          local.Group.Update.Program.Code = local.Tmp.Code;
          local.Group.Update.Func.Text1 = local.CaseFuncWorkSet.FuncText1;
          local.Group.Update.Cau.Count = local.Cau.Count;
        }
      }
    }

    local.AsgnCount.Count = 0;

    foreach(var item in ReadCaseAssignment())
    {
      ++local.AsgnCount.Count;
    }

    // *****************************************************************
    // Process Sort
    // *****************************************************************
    if (Equal(local.OrderBy.Text10, "AP"))
    {
      if (local.Group.IsEmpty)
      {
        goto Test;
      }

      do
      {
        local.LoopAgain.Flag = "N";
        local.Group.Index = 0;

        for(var limit = local.Group.Count - 1; local.Group.Index < limit; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          local.ApCompare.FormattedName = local.Group.Item.Ap.FormattedName;

          ++local.Group.Index;
          local.Group.CheckSize();

          if (!Lt(local.Group.Item.Ap.FormattedName,
            local.ApCompare.FormattedName))
          {
            --local.Group.Index;
            local.Group.CheckSize();

            continue;
          }

          // : Switch the table entries around.
          local.B.Bcase.Number = local.Group.Item.Case1.Number;
          local.B.Btribunal.Identifier = local.Group.Item.Tribunal.Identifier;
          MoveCsePersonsWorkSet(local.Group.Item.Ap, local.B.ApB);
          MoveCsePersonsWorkSet(local.Group.Item.Ar, local.B.ArB);
          local.B.BcaseAssignment.Assign(local.Group.Item.CaseAssignment);
          local.B.Bprogram.Code = local.Group.Item.Program.Code;
          local.B.Bfunc.Text1 = local.Group.Item.Func.Text1;
          local.B.Bcau.Count = local.Group.Item.Cau.Count;

          --local.Group.Index;
          local.Group.CheckSize();

          local.A.Acase.Number = local.Group.Item.Case1.Number;
          local.A.Atribunal.Identifier = local.Group.Item.Tribunal.Identifier;
          MoveCsePersonsWorkSet(local.Group.Item.Ap, local.A.ApA);
          MoveCsePersonsWorkSet(local.Group.Item.Ar, local.A.ArA);
          local.A.AcaseAssignment.Assign(local.Group.Item.CaseAssignment);
          local.A.Aprogram.Code = local.Group.Item.Program.Code;
          local.A.Afunc.Text1 = local.Group.Item.Func.Text1;
          local.A.Acau.Count = local.Group.Item.Cau.Count;
          local.Group.Update.Case1.Number = local.B.Bcase.Number;
          local.Group.Update.Tribunal.Identifier = local.B.Btribunal.Identifier;
          MoveCsePersonsWorkSet(local.B.ApB, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.B.ArB, local.Group.Update.Ar);
          local.Group.Update.CaseAssignment.Assign(local.B.BcaseAssignment);
          local.Group.Update.Program.Code = local.B.Bprogram.Code;
          local.Group.Update.Func.Text1 = local.B.Bfunc.Text1;
          local.Group.Update.Cau.Count = local.B.Bcau.Count;

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number = local.A.Acase.Number;
          local.Group.Update.Tribunal.Identifier = local.A.Atribunal.Identifier;
          MoveCsePersonsWorkSet(local.A.ApA, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.A.ArA, local.Group.Update.Ar);
          local.Group.Update.CaseAssignment.Assign(local.A.AcaseAssignment);
          local.Group.Update.Program.Code = local.A.Aprogram.Code;
          local.Group.Update.Func.Text1 = local.A.Afunc.Text1;
          local.Group.Update.Cau.Count = local.A.Acau.Count;

          --local.Group.Index;
          local.Group.CheckSize();

          local.LoopAgain.Flag = "Y";
        }

        local.Group.CheckIndex();
      }
      while(AsChar(local.LoopAgain.Flag) != 'N');
    }

Test:

    // *****************************************************************
    // Format Report and Add to the Report Repository
    // *****************************************************************
    // : Format Header Lines and add to REPORT_DATA.
    local.Header.Index = 0;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";

    // 02/11/2010	JHuss	Replaced hardcoded job name with job name from PPI 
    // record
    local.Header.Update.Header1.LineText = local.Job.Name + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 58) + "STATE OF KANSAS" +
      Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 41) + "PAGE:     1";
      
    local.TextMm.Text2 =
      NumberToString(Month(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextDd.Text2 =
      NumberToString(Day(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextYyyy.Text4 =
      NumberToString(Year(entities.ExistingJobRun.StartTimestamp), 12, 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    local.Header.Index = 1;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText = "RUN DATE: " + local
      .TextDate.Text10 + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 36) + "DEPARTMENT FOR CHILDREN AND FAMILIES";
      

    local.Header.Index = 2;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      62) + "KAECSES - CHILD SUPPORT";

    local.Header.Index = 3;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "01";

    switch(TrimEnd(local.OrderBy.Text10))
    {
      case "CASE":
        local.Header.Update.Header1.LineText =
          Substring(local.NullReportData.LineText,
          ReportData.LineText_MaxLength, 1, 62) + "CASE ASSIGNMENT BY CASE";

        break;
      case "AP":
        local.Header.Update.Header1.LineText =
          Substring(local.NullReportData.LineText,
          ReportData.LineText_MaxLength, 1, 63) + "CASE ASSIGNMENT BY AP";

        break;
      default:
        break;
    }

    local.Header.Index = 4;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      9) + "FILTERS:  OFFICE:           " + NumberToString
      (entities.ExistingOffice.SystemGeneratedId, 12, 4) + "  " + entities
      .ExistingOffice.Name + "                                   TOTAL ASSIGNMENTS: " +
      NumberToString(local.AsgnCount.Count, 11, 5);

    local.Header.Index = 5;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      19) + "SERVICE PROVIDER: " + NumberToString
      (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " " + local
      .FormattedServiceProvider.FormattedName;

    local.Header.Index = 6;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      19) + "ORDER BY: " + local.OrderBy.Text10;

    local.Header.Index = 7;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";

    if (IsEmpty(local.ParmAp.Text17))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "SEARCH BY EXACT AP: N/A";
    }
    else if (IsEmpty(local.ParmApFi.Text1))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "SEARCH BY EXACT AP: " + TrimEnd(local.ParmAp.Text17) + " " + " ";
        
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "SEARCH BY EXACT AP: " + TrimEnd(local.ParmAp.Text17) + ", " +
        local.ParmApFi.Text1;
    }

    local.Header.Index = 8;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmAr.Text17))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          EXACT AR: N/A";
    }
    else if (IsEmpty(local.ParmArFi.Text1))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          EXACT AR: " + TrimEnd(local.ParmAr.Text17) + " " + " ";
        
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          EXACT AR: " + TrimEnd(local.ParmAr.Text17) + ", " +
        local.ParmArFi.Text1;
    }

    // @@@ New below...
    local.Header.Index = 9;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmTribunal1.Text4) || local
      .ParmTribunal2.Identifier == 0)
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          TRIBUNAL: N/A";
    }
    else
    {
      if (ReadTribunal1())
      {
        local.TribunalName.Text30 = entities.Tribunal.Name;
      }

      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          TRIBUNAL: " + local.ParmTribunal1.Text4 + " " + local
        .TribunalName.Text30;
    }

    // @@@ New above...
    local.Header.Index = 10;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmProgram.Code))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          PROGRAM:  N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          PROGRAM:  " + local.ParmProgram.Code;
    }

    local.Header.Index = 11;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmCaseFuncWorkSet.FuncText1))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          FUNCTION: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          FUNCTION: " + local
        .ParmCaseFuncWorkSet.FuncText1 + " " + local.ParmArFi.Text1;
    }

    local.Header.Index = 12;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmCaseAssignment.OverrideInd))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          OVERRIDE: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "          OVERRIDE: " + local.ParmCaseAssignment.OverrideInd;
    }

    local.Header.Index = 13;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "02";
    local.Header.Update.Header1.LineText =
      "SEQ   CASE         AP-NAME                           AR-NAME                               F     PGM    CAU     TRIB   O    EFF-DATE";
      

    local.Header.Index = 14;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      "====================================================================================================================================";
      

    for(local.Header.Index = 0; local.Header.Index < local.Header.Count; ++
      local.Header.Index)
    {
      if (!local.Header.CheckSize())
      {
        break;
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
    }

    local.Header.CheckIndex();

    if (local.Group.IsEmpty)
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "No Data Retrieved for the report.";
      UseCabErrorReport3();
    }
    else
    {
      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        // : Format text date for the case assignment effective date.
        local.TextMm.Text2 =
          NumberToString(
            DateToInt(entities.ExistingCaseAssignment.EffectiveDate), 12, 2);
        local.TextDd.Text2 =
          NumberToString(
            DateToInt(entities.ExistingCaseAssignment.EffectiveDate), 14, 2);
        local.TextYyyy.Text4 =
          NumberToString(
            DateToInt(entities.ExistingCaseAssignment.EffectiveDate), 8, 4);
        local.TextDate.Text10 = local.TextMm.Text2 + "/" + local
          .TextDd.Text2 + "/" + local.TextYyyy.Text4;

        if (local.Group.Item.Tribunal.Identifier == 0)
        {
          local.TempTribunal.Text4 = "";
        }
        else
        {
          local.TempTribunal.Text4 =
            NumberToString(local.Group.Item.Tribunal.Identifier, 12, 4);
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

      local.Group.CheckIndex();
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.ExistingJobRun.OutputType, "ONLINE"))
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

  private static void MoveCaseAssignment(CaseAssignment source,
    CaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
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

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    local.CaseFuncWorkSet.FuncText1 = useExport.CaseFuncWorkSet.FuncText1;
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

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Tmp.Code = useExport.Program.Code;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.FormattedAp.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.FormattedAp.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.FormattedAr.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.FormattedAr.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.Group.Index + 1;
    var firstPageOnlyInd = "N";
    var lineText = Substring(NumberToString(local.Group.Index + 1, 11, 15) + " " +
      local.Group.Item.Case1.Number + "   " + Substring
      (local.Group.Item.Ap.FormattedName,
      CsePersonsWorkSet.FormattedName_MaxLength, 1, 37) + " " + Substring
      (local.Group.Item.Ar.FormattedName,
      CsePersonsWorkSet.FormattedName_MaxLength, 1, 37) + " " + " " + " " + " " +
      " " + local.Group.Item.Func.Text1 + "     " + local
      .Group.Item.Program.Code + "   " + NumberToString
      (local.Group.Item.Cau.Count, 11, 5) + "    " + local
      .TempTribunal.Text4 + "   " + local
      .Group.Item.CaseAssignment.OverrideInd + "  " + local
      .TextDate.Text10 + " ", 1, 132);
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
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

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = "";
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private void CreateReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "H";
    var sequenceNumber = local.Header.Index + 1;
    var firstPageOnlyInd = local.Header.Item.Header1.FirstPageOnlyInd ?? "";
    var lineControl = local.Header.Item.Header1.LineControl;
    var lineText = local.Header.Item.Header1.LineText;
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData2",
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

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.
          SetNullableString(command, "status", local.HardcodedOpen.Status ?? "");
          
      },
      (db, reader) =>
      {
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 7);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 9);
        entities.ExistingCaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.
          SetNullableString(command, "status", local.HardcodedOpen.Status ?? "");
          
        db.SetString(command, "numb", local.ParmCase.Number);
        db.SetString(
          command, "overrideInd", local.ParmCaseAssignment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 2);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 3);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 7);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 8);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 9);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 10);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 1);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 3);
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingKeyOnly.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingKeyOnly.Type1 = db.GetString(reader, 1);
        entities.ExistingKeyOnly.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingKeyOnly.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingKeyOnly.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingKeyOnly.Type1 = db.GetString(reader, 1);
        entities.ExistingKeyOnly.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingKeyOnly.Type1);
      });
  }

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetInt32(command, "systemGenId", local.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 3);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 4);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.JobName = db.GetString(reader, 6);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 7);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableInt32(command, "trbId", local.ParmTribunal2.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "officeId", local.ParmOffice.SystemGeneratedId);
        db.SetString(
          command, "roleCode", local.ParmOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          local.ParmOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOffice.Name = db.GetString(reader, 5);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          local.ParmServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.ParmTribunal2.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunal2()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "PROCESSING";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "WAIT";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      public CsePersonsWorkSet Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>
      /// A value of Ar.
      /// </summary>
      [JsonPropertyName("ar")]
      public CsePersonsWorkSet Ar
      {
        get => ar ??= new();
        set => ar = value;
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

      /// <summary>
      /// A value of Cau.
      /// </summary>
      [JsonPropertyName("cau")]
      public Common Cau
      {
        get => cau ??= new();
        set => cau = value;
      }

      /// <summary>
      /// A value of Func.
      /// </summary>
      [JsonPropertyName("func")]
      public TextWorkArea Func
      {
        get => func ??= new();
        set => func = value;
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
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25000;

      private Case1 case1;
      private CsePersonsWorkSet ap;
      private CsePersonsWorkSet ar;
      private CaseAssignment caseAssignment;
      private Common cau;
      private TextWorkArea func;
      private Program program;
      private Tribunal tribunal;
    }

    /// <summary>A AGroup group.</summary>
    [Serializable]
    public class AGroup
    {
      /// <summary>
      /// A value of Acase.
      /// </summary>
      [JsonPropertyName("acase")]
      public Case1 Acase
      {
        get => acase ??= new();
        set => acase = value;
      }

      /// <summary>
      /// A value of ApA.
      /// </summary>
      [JsonPropertyName("apA")]
      public CsePersonsWorkSet ApA
      {
        get => apA ??= new();
        set => apA = value;
      }

      /// <summary>
      /// A value of ArA.
      /// </summary>
      [JsonPropertyName("arA")]
      public CsePersonsWorkSet ArA
      {
        get => arA ??= new();
        set => arA = value;
      }

      /// <summary>
      /// A value of AcaseAssignment.
      /// </summary>
      [JsonPropertyName("acaseAssignment")]
      public CaseAssignment AcaseAssignment
      {
        get => acaseAssignment ??= new();
        set => acaseAssignment = value;
      }

      /// <summary>
      /// A value of Acau.
      /// </summary>
      [JsonPropertyName("acau")]
      public Common Acau
      {
        get => acau ??= new();
        set => acau = value;
      }

      /// <summary>
      /// A value of Afunc.
      /// </summary>
      [JsonPropertyName("afunc")]
      public TextWorkArea Afunc
      {
        get => afunc ??= new();
        set => afunc = value;
      }

      /// <summary>
      /// A value of Aprogram.
      /// </summary>
      [JsonPropertyName("aprogram")]
      public Program Aprogram
      {
        get => aprogram ??= new();
        set => aprogram = value;
      }

      /// <summary>
      /// A value of Atribunal.
      /// </summary>
      [JsonPropertyName("atribunal")]
      public Tribunal Atribunal
      {
        get => atribunal ??= new();
        set => atribunal = value;
      }

      private Case1 acase;
      private CsePersonsWorkSet apA;
      private CsePersonsWorkSet arA;
      private CaseAssignment acaseAssignment;
      private Common acau;
      private TextWorkArea afunc;
      private Program aprogram;
      private Tribunal atribunal;
    }

    /// <summary>A BGroup group.</summary>
    [Serializable]
    public class BGroup
    {
      /// <summary>
      /// A value of Bcase.
      /// </summary>
      [JsonPropertyName("bcase")]
      public Case1 Bcase
      {
        get => bcase ??= new();
        set => bcase = value;
      }

      /// <summary>
      /// A value of ApB.
      /// </summary>
      [JsonPropertyName("apB")]
      public CsePersonsWorkSet ApB
      {
        get => apB ??= new();
        set => apB = value;
      }

      /// <summary>
      /// A value of ArB.
      /// </summary>
      [JsonPropertyName("arB")]
      public CsePersonsWorkSet ArB
      {
        get => arB ??= new();
        set => arB = value;
      }

      /// <summary>
      /// A value of BcaseAssignment.
      /// </summary>
      [JsonPropertyName("bcaseAssignment")]
      public CaseAssignment BcaseAssignment
      {
        get => bcaseAssignment ??= new();
        set => bcaseAssignment = value;
      }

      /// <summary>
      /// A value of Bcau.
      /// </summary>
      [JsonPropertyName("bcau")]
      public Common Bcau
      {
        get => bcau ??= new();
        set => bcau = value;
      }

      /// <summary>
      /// A value of Bfunc.
      /// </summary>
      [JsonPropertyName("bfunc")]
      public TextWorkArea Bfunc
      {
        get => bfunc ??= new();
        set => bfunc = value;
      }

      /// <summary>
      /// A value of Bprogram.
      /// </summary>
      [JsonPropertyName("bprogram")]
      public Program Bprogram
      {
        get => bprogram ??= new();
        set => bprogram = value;
      }

      /// <summary>
      /// A value of Btribunal.
      /// </summary>
      [JsonPropertyName("btribunal")]
      public Tribunal Btribunal
      {
        get => btribunal ??= new();
        set => btribunal = value;
      }

      private Case1 bcase;
      private CsePersonsWorkSet apB;
      private CsePersonsWorkSet arB;
      private CaseAssignment bcaseAssignment;
      private Common bcau;
      private TextWorkArea bfunc;
      private Program bprogram;
      private Tribunal btribunal;
    }

    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ReportData header1;
    }

    /// <summary>
    /// A value of TempTribunal.
    /// </summary>
    [JsonPropertyName("tempTribunal")]
    public WorkArea TempTribunal
    {
      get => tempTribunal ??= new();
      set => tempTribunal = value;
    }

    /// <summary>
    /// A value of TribunalName.
    /// </summary>
    [JsonPropertyName("tribunalName")]
    public TextWorkArea TribunalName
    {
      get => tribunalName ??= new();
      set => tribunalName = value;
    }

    /// <summary>
    /// A value of TribunalFound.
    /// </summary>
    [JsonPropertyName("tribunalFound")]
    public Common TribunalFound
    {
      get => tribunalFound ??= new();
      set => tribunalFound = value;
    }

    /// <summary>
    /// A value of Cau.
    /// </summary>
    [JsonPropertyName("cau")]
    public Common Cau
    {
      get => cau ??= new();
      set => cau = value;
    }

    /// <summary>
    /// A value of ParmTribunal1.
    /// </summary>
    [JsonPropertyName("parmTribunal1")]
    public WorkArea ParmTribunal1
    {
      get => parmTribunal1 ??= new();
      set => parmTribunal1 = value;
    }

    /// <summary>
    /// A value of ParmTribunal2.
    /// </summary>
    [JsonPropertyName("parmTribunal2")]
    public Tribunal ParmTribunal2
    {
      get => parmTribunal2 ??= new();
      set => parmTribunal2 = value;
    }

    /// <summary>
    /// A value of ParmCaseAssignment.
    /// </summary>
    [JsonPropertyName("parmCaseAssignment")]
    public CaseAssignment ParmCaseAssignment
    {
      get => parmCaseAssignment ??= new();
      set => parmCaseAssignment = value;
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
    /// A value of OrderBy.
    /// </summary>
    [JsonPropertyName("orderBy")]
    public TextWorkArea OrderBy
    {
      get => orderBy ??= new();
      set => orderBy = value;
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
    /// A value of ParmApFi.
    /// </summary>
    [JsonPropertyName("parmApFi")]
    public TextWorkArea ParmApFi
    {
      get => parmApFi ??= new();
      set => parmApFi = value;
    }

    /// <summary>
    /// A value of ParmArFi.
    /// </summary>
    [JsonPropertyName("parmArFi")]
    public TextWorkArea ParmArFi
    {
      get => parmArFi ??= new();
      set => parmArFi = value;
    }

    /// <summary>
    /// A value of ParmAr.
    /// </summary>
    [JsonPropertyName("parmAr")]
    public WorkArea ParmAr
    {
      get => parmAr ??= new();
      set => parmAr = value;
    }

    /// <summary>
    /// A value of ParmAp.
    /// </summary>
    [JsonPropertyName("parmAp")]
    public WorkArea ParmAp
    {
      get => parmAp ??= new();
      set => parmAp = value;
    }

    /// <summary>
    /// A value of ParmProgram.
    /// </summary>
    [JsonPropertyName("parmProgram")]
    public Program ParmProgram
    {
      get => parmProgram ??= new();
      set => parmProgram = value;
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
    /// A value of ParmCaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("parmCaseFuncWorkSet")]
    public CaseFuncWorkSet ParmCaseFuncWorkSet
    {
      get => parmCaseFuncWorkSet ??= new();
      set => parmCaseFuncWorkSet = value;
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
    /// A value of FormattedAp.
    /// </summary>
    [JsonPropertyName("formattedAp")]
    public CsePersonsWorkSet FormattedAp
    {
      get => formattedAp ??= new();
      set => formattedAp = value;
    }

    /// <summary>
    /// A value of FormattedAr.
    /// </summary>
    [JsonPropertyName("formattedAr")]
    public CsePersonsWorkSet FormattedAr
    {
      get => formattedAr ??= new();
      set => formattedAr = value;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApNameLength.
    /// </summary>
    [JsonPropertyName("apNameLength")]
    public Common ApNameLength
    {
      get => apNameLength ??= new();
      set => apNameLength = value;
    }

    /// <summary>
    /// A value of ArNameLength.
    /// </summary>
    [JsonPropertyName("arNameLength")]
    public Common ArNameLength
    {
      get => arNameLength ??= new();
      set => arNameLength = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Program Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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
    /// A value of ApCompare.
    /// </summary>
    [JsonPropertyName("apCompare")]
    public CsePersonsWorkSet ApCompare
    {
      get => apCompare ??= new();
      set => apCompare = value;
    }

    /// <summary>
    /// A value of LoopAgain.
    /// </summary>
    [JsonPropertyName("loopAgain")]
    public Common LoopAgain
    {
      get => loopAgain ??= new();
      set => loopAgain = value;
    }

    /// <summary>
    /// Gets a value of A.
    /// </summary>
    [JsonPropertyName("a")]
    public AGroup A
    {
      get => a ?? (a = new());
      set => a = value;
    }

    /// <summary>
    /// Gets a value of B.
    /// </summary>
    [JsonPropertyName("b")]
    public BGroup B
    {
      get => b ?? (b = new());
      set => b = value;
    }

    /// <summary>
    /// A value of HardcodedOpen.
    /// </summary>
    [JsonPropertyName("hardcodedOpen")]
    public Case1 HardcodedOpen
    {
      get => hardcodedOpen ??= new();
      set => hardcodedOpen = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
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
    /// A value of ParmLegalReferral.
    /// </summary>
    [JsonPropertyName("parmLegalReferral")]
    public LegalReferral ParmLegalReferral
    {
      get => parmLegalReferral ??= new();
      set => parmLegalReferral = value;
    }

    /// <summary>
    /// A value of ParmReferralReason.
    /// </summary>
    [JsonPropertyName("parmReferralReason")]
    public TextWorkArea ParmReferralReason
    {
      get => parmReferralReason ??= new();
      set => parmReferralReason = value;
    }

    /// <summary>
    /// A value of ParmShowOnly.
    /// </summary>
    [JsonPropertyName("parmShowOnly")]
    public TextWorkArea ParmShowOnly
    {
      get => parmShowOnly ??= new();
      set => parmShowOnly = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
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

    private WorkArea tempTribunal;
    private TextWorkArea tribunalName;
    private Common tribunalFound;
    private Common cau;
    private WorkArea parmTribunal1;
    private Tribunal parmTribunal2;
    private CaseAssignment parmCaseAssignment;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea orderBy;
    private ServiceProvider parmServiceProvider;
    private TextWorkArea parmApFi;
    private TextWorkArea parmArFi;
    private WorkArea parmAr;
    private WorkArea parmAp;
    private Program parmProgram;
    private Case1 parmCase;
    private OfficeServiceProvider parmOfficeServiceProvider;
    private Office parmOffice;
    private CaseFuncWorkSet parmCaseFuncWorkSet;
    private Common asgnCount;
    private CsePersonsWorkSet formattedServiceProvider;
    private CsePersonsWorkSet formattedAp;
    private CsePersonsWorkSet formattedAr;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Common apNameLength;
    private Common arNameLength;
    private Program tmp;
    private External external;
    private Array<GroupGroup> group;
    private CsePersonsWorkSet apCompare;
    private Common loopAgain;
    private AGroup a;
    private BGroup b;
    private Case1 hardcodedOpen;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Program program;
    private LegalReferral parmLegalReferral;
    private TextWorkArea parmReferralReason;
    private TextWorkArea parmShowOnly;
    private Array<HeaderGroup> header;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
    }

    /// <summary>
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public CsePerson ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
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
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ReportData New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Job existingJob;
    private JobRun existingJobRun;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private CsePerson existingKeyOnly;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
    private CaseUnit existingCaseUnit;
    private ReportData new1;
  }
#endregion
}
