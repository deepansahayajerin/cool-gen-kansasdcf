// Program: SP_B712_LEGAL_REF_ASGNMNT_RPT, ID: 371143873, model: 746.
// Short name: SWEP712B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B712_LEGAL_REF_ASGNMNT_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB712LegalRefAsgnmntRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B712_LEGAL_REF_ASGNMNT_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB712LegalRefAsgnmntRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB712LegalRefAsgnmntRpt.
  /// </summary>
  public SpB712LegalRefAsgnmntRpt(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 12/07/2009  Raj S              CQ9076      Changeed local group view 
    // size from 10000*
    // *
    // 
    // to 30000 to handle counties has more than*
    // *
    // 
    // 10000 cases.                             *
    // *
    // 
    // *
    // ***************************************************************************************
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 12/11/00  Alan Doty              Initial Development
    // --------------------------------------------------------------------
    // November 15, 2001, M. Brown, PR# 132127: Fixed problem where same date 
    // was showing on each line.
    // (wrong view was being used to populate the attribute on the list).
    // November 15, 2001, M. Brown, PR# 132125: Increased the size of the group 
    // view to 8000. January 29, 2002 changed group view size to 10000 per CSE
    // request on PR#132125. L Bachura at M. Brown's request.
    // November 15, 2001, M. Brown, PR# 136213: Added total assignments to top 
    // of rpt.
    // : Also changed to not include closed,  withdrawn or rejected statuses.
    // ***************************************************************************************************
    // M J Quinn  15 April 2005     PR  180473  SUMMARIZE statement replaced by 
    // READ EACH
    // to improve execution speed.
    // ***************************************************************************************************
    // 12/03/2010 SWSRGAV	  CQ109      Add parm for override indicator.
    // *****************************************************************
    // resp:  Service Plan
    // This procedure produces a report of open case assignments for the
    // selected office service provider. Sorting by Case Number and
    // AP Name is supported.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.DebugMode.Flag = "N";

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
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
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
    try
    {
      UpdateJobRun6();
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
    if (IsEmpty(entities.ExistingJobRun.ParmInfo))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    // : Mandatory Parm Values.
    local.ParmServiceProvider.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 1, 5));

    if (local.ParmServiceProvider.SystemGeneratedId == 0)
    {
      try
      {
        UpdateJobRun5();

        // : Perform a DB2 Commit to Free Up the JOB_RUN row.
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_SYSIN_PARM_ERROR_A";

          return;
        }
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

      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ParmOffice.SystemGeneratedId =
      (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 54, 5));

    if (local.ParmOffice.SystemGeneratedId == 0)
    {
      try
      {
        UpdateJobRun4();

        // : Perform a DB2 Commit to Free Up the JOB_RUN row.
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_SYSIN_PARM_ERROR_A";

          return;
        }
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

      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ParmOfficeServiceProvider.EffectiveDate =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 60, 10));

    if (Equal(local.ParmOfficeServiceProvider.EffectiveDate,
      local.NullDateWorkArea.Date))
    {
      try
      {
        UpdateJobRun2();

        // : Perform a DB2 Commit to Free Up the JOB_RUN row.
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_SYSIN_PARM_ERROR_A";

          return;
        }
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

      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ParmOfficeServiceProvider.RoleCode =
      Substring(entities.ExistingJobRun.ParmInfo, 71, 2);

    if (local.ParmOffice.SystemGeneratedId == 0)
    {
      try
      {
        UpdateJobRun3();

        // : Perform a DB2 Commit to Free Up the JOB_RUN row.
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_SYSIN_PARM_ERROR_A";

          return;
        }
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

      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // : Optional Parm Values.
    local.ParmAp.Text10 = Substring(entities.ExistingJobRun.ParmInfo, 7, 10);
    local.ParmApFi.Text1 = Substring(entities.ExistingJobRun.ParmInfo, 18, 1);
    local.ParmCase.Number = Substring(entities.ExistingJobRun.ParmInfo, 20, 10);
    local.ParmLegalReferral.ReferralDate =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 31, 10));
    local.ParmLegalReferral.Status =
      Substring(entities.ExistingJobRun.ParmInfo, 42, 1);
    local.ParmReferralReason.Text4 =
      Substring(entities.ExistingJobRun.ParmInfo, 44, 3);
    local.ParmProgram.Code = Substring(entities.ExistingJobRun.ParmInfo, 48, 3);
    local.ParmShowOnly.Text1 =
      Substring(entities.ExistingJobRun.ParmInfo, 52, 1);
    local.ParmLegalReferralAssignment.OverrideInd =
      Substring(entities.ExistingJobRun.ParmInfo, 74, 1);

    if (!IsEmpty(local.ParmAp.Text10))
    {
      local.ApNameLength.Count = Length(TrimEnd(local.ParmAp.Text10));
    }

    if (IsEmpty(local.ParmShowOnly.Text1))
    {
      local.ParmShowOnly.Text1 = "N";
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
    local.Group.Index = -1;

    foreach(var item in ReadLegalReferralLegalReferralAssignmentCase())
    {
      if (!Equal(local.ParmLegalReferral.ReferralDate,
        local.NullDateWorkArea.Date))
      {
        if (!Equal(local.ParmLegalReferral.ReferralDate,
          entities.ExistingLegalReferral.ReferralDate))
        {
          continue;
        }
      }

      if (!IsEmpty(local.ParmLegalReferral.Status))
      {
        if (AsChar(entities.ExistingLegalReferral.Status) != AsChar
          (local.ParmLegalReferral.Status))
        {
          continue;
        }
      }
      else
      {
        // : November, 2001, Maureen Brown, PR# 132613. Changed to not include 
        // closed,
        //  withdrawn or rejected statuses.
        if (AsChar(entities.ExistingLegalReferral.Status) == 'O' || AsChar
          (entities.ExistingLegalReferral.Status) == 'S')
        {
        }
        else
        {
          continue;
        }
      }

      if (!IsEmpty(local.ParmReferralReason.Text4))
      {
        if (Equal(local.ParmReferralReason.Text4,
          entities.ExistingLegalReferral.ReferralReason1) || Equal
          (local.ParmReferralReason.Text4,
          entities.ExistingLegalReferral.ReferralReason2) || Equal
          (local.ParmReferralReason.Text4,
          entities.ExistingLegalReferral.ReferralReason3) || Equal
          (local.ParmReferralReason.Text4,
          entities.ExistingLegalReferral.ReferralReason4))
        {
          // : Continue Processing.
        }
        else
        {
          continue;
        }
      }

      // : If Show Arrears Only Cases is True, then skip all Zero Balance 
      // Obligors.
      if (AsChar(local.ParmShowOnly.Text1) == 'Y')
      {
        local.ArrearsBalance.Count = 0;

        // ***************************************************************************************************
        // M J Quinn  15 April 2005     PR  180473  SUMMARIZE
        // statement replaced by READ EACH
        // to improve execution
        // speed.
        // ***************************************************************************************************
        foreach(var item1 in ReadDebtDetail())
        {
          ++local.ArrearsBalance.Count;
        }

        if (local.ArrearsBalance.Count == 0)
        {
          continue;
        }
      }

      // : Prepare the AP data.
      local.FormattedAp.Assign(local.NullCsePersonsWorkSet);

      if (ReadCsePerson1())
      {
        local.FormattedAp.Number = entities.ExistingKeyOnly.Number;

        if (AsChar(local.DebugMode.Flag) == 'N')
        {
          UseSiReadCsePersonBatch1();
        }
        else
        {
          local.FormattedAp.FormattedName = "DEBUG MODE: AP NAME NOT AVAILABLE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.FormattedAp.FormattedName = "ERROR: AP NOT FOUND ON ADABAS";
          ExitState = "ACO_NN0000_ALL_OK";

          goto Read;
        }

        if (!IsEmpty(local.ParmAp.Text10))
        {
          if (!Equal(local.ParmAp.Text10, local.FormattedAp.LastName, 1,
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
      else
      {
        local.FormattedAp.FormattedName = "ERROR: AP NOT FOUND";
      }

Read:

      // : Prepare the AR Data.
      MoveCsePersonsWorkSet(local.NullCsePersonsWorkSet, local.FormattedAr);

      if (ReadCsePerson2())
      {
        local.FormattedAr.Number = entities.ExistingKeyOnly.Number;

        if (AsChar(local.DebugMode.Flag) == 'N')
        {
          UseSiReadCsePersonBatch2();
        }
        else
        {
          local.FormattedAr.FormattedName = "DEBUG MODE: AP NAME NOT AVAILABLE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.FormattedAr.FormattedName = "ERROR: AR NOT FOUND ON ADABAS";
          ExitState = "ACO_NN0000_ALL_OK";
        }
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

      ++local.Group.Index;
      local.Group.CheckSize();

      local.Group.Update.Case1.Number = entities.ExistingCase.Number;
      MoveCsePersonsWorkSet(local.FormattedAp, local.Group.Update.Ap);
      MoveCsePersonsWorkSet(local.FormattedAr, local.Group.Update.Ar);
      local.Group.Update.LegalReferral.Assign(entities.ExistingLegalReferral);
      local.Group.Update.Program.Code = local.Tmp.Code;
      local.Group.Update.LegalReferralAssignment.OverrideInd =
        entities.ExistingLegalReferralAssignment.OverrideInd;

      if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
      {
        break;
      }
    }

    // : November, 2001, Maureen Brown, PR# 132613.
    local.NumberOfCases.Count = local.Group.Index + 1;

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

          local.Ap1Compare.FormattedName = local.Group.Item.Ap.FormattedName;

          ++local.Group.Index;
          local.Group.CheckSize();

          if (!Lt(local.Group.Item.Ap.FormattedName,
            local.Ap1Compare.FormattedName))
          {
            --local.Group.Index;
            local.Group.CheckSize();

            continue;
          }

          local.Grp2Case.Number = local.Group.Item.Case1.Number;
          MoveCsePersonsWorkSet(local.Group.Item.Ap, local.GrpAp2);
          MoveCsePersonsWorkSet(local.Group.Item.Ar, local.GrpAr2);
          local.Grp2LegalReferral.Assign(local.Group.Item.LegalReferral);
          local.Grp2Program.Code = local.Group.Item.Program.Code;
          local.Grp2LegalReferralAssignment.OverrideInd =
            local.Group.Item.LegalReferralAssignment.OverrideInd;

          --local.Group.Index;
          local.Group.CheckSize();

          local.Grp1Case.Number = local.Group.Item.Case1.Number;
          MoveCsePersonsWorkSet(local.Group.Item.Ap, local.GrpAp1);
          MoveCsePersonsWorkSet(local.Group.Item.Ar, local.GrpAr1);
          local.Grp1LegalReferral.Assign(local.Group.Item.LegalReferral);
          local.Grp1Program.Code = local.Group.Item.Program.Code;
          local.Grp1LegalReferralAssignment.OverrideInd =
            local.Group.Item.LegalReferralAssignment.OverrideInd;
          local.Group.Update.Case1.Number = local.Grp2Case.Number;
          MoveCsePersonsWorkSet(local.GrpAp2, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.GrpAr2, local.Group.Update.Ar);
          local.Group.Update.LegalReferral.Assign(local.Grp2LegalReferral);
          local.Group.Update.Program.Code = local.Grp2Program.Code;
          local.Group.Update.LegalReferralAssignment.OverrideInd =
            local.Grp2LegalReferralAssignment.OverrideInd;

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number = local.Grp1Case.Number;
          MoveCsePersonsWorkSet(local.GrpAp1, local.Group.Update.Ap);
          MoveCsePersonsWorkSet(local.GrpAr1, local.Group.Update.Ar);
          local.Group.Update.LegalReferral.Assign(local.Grp1LegalReferral);
          local.Group.Update.Program.Code = local.Grp1Program.Code;
          local.Group.Update.LegalReferralAssignment.OverrideInd =
            local.Grp1LegalReferralAssignment.OverrideInd;

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
    local.Header.Update.Header1.LineText = entities.ExistingJob.Name + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 58) + "STATE OF KANSAS" +
      Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 40) + "PAGE:     1";
      

    local.Header.Index = 1;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.TextMm.Text2 =
      NumberToString(Month(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextDd.Text2 =
      NumberToString(Day(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextYyyy.Text4 =
      NumberToString(Year(entities.ExistingJobRun.StartTimestamp), 12, 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;
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
          ReportData.LineText_MaxLength, 1, 57) + "LEGAL REFERRAL ASSIGNMENT BY CASE";
          

        break;
      case "AP":
        local.Header.Update.Header1.LineText =
          Substring(local.NullReportData.LineText,
          ReportData.LineText_MaxLength, 1, 58) + "LEGAL REFERRAL ASSIGNMENT BY AP";
          

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
      (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " " + entities
      .ExistingOffice.Name;
    local.Header.Update.Header1.LineText =
      Substring(local.Header.Item.Header1.LineText,
      ReportData.LineText_MaxLength, 1, 72) + "                                    TOTAL ASSIGNMENTS: " +
      NumberToString(local.NumberOfCases.Count, 11, 5);

    local.Header.Index = 5;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      19) + "SERVICE PROVIDER: " + NumberToString
      (entities.ExistingServiceProvider.SystemGeneratedId, 12, 4) + " " + local
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

    if (IsEmpty(local.ParmAp.Text10))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "SEARCH BY AP:       N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "SEARCH BY AP:       " + local.ParmAp.Text10 + ", " + local
        .ParmApFi.Text1;
    }

    local.Header.Index = 8;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (Equal(local.ParmLegalReferral.ReferralDate, local.NullDateWorkArea.Date))
      
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "DATE:     N/A";
    }
    else
    {
      local.TextMm.Text2 =
        NumberToString(Month(local.ParmLegalReferral.ReferralDate), 14, 2);
      local.TextDd.Text2 =
        NumberToString(Day(local.ParmLegalReferral.ReferralDate), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(local.ParmLegalReferral.ReferralDate), 12, 4);
      local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
        + local.TextYyyy.Text4;
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "DATE:     " + local.TextDate.Text10;
    }

    local.Header.Index = 9;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmLegalReferral.Status))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "STATUS:   N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "STATUS:   " + (local.ParmLegalReferral.Status ?? "");
    }

    local.Header.Index = 10;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmReferralReason.Text4))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "RR:       N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "RR:       " + local.ParmReferralReason.Text4;
    }

    local.Header.Index = 11;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmProgram.Code))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "PROGRAM:  N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "PROGRAM:  " + local.ParmProgram.Code;
    }

    local.Header.Index = 12;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmLegalReferralAssignment.OverrideInd))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "OVERRIDE: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 29) + "OVERRIDE: " + local.ParmLegalReferralAssignment.OverrideInd;
    }

    local.Header.Index = 13;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";

    if (IsEmpty(local.ParmCase.Number))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "STARTING CASE: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "STARTING CASE: " + local.ParmCase.Number;
    }

    local.Header.Index = 14;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      19) + "SHOW ONLY WITH ARREARS OWED: " + local.ParmShowOnly.Text1;

    local.Header.Index = 15;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "02";
    local.Header.Update.Header1.LineText =
      "SEQ   CASE       AP-NAME                           AR-NAME                           REF-DATE   S RR1  RR2  RR3  RR4  OVR  PGM";
      

    local.Header.Index = 16;
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

    local.Header.CheckIndex();

    // : Format Detail Lines and add to REPORT_DATA.
    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      // November 15, 2001, M. Brown, PR# 132127
      local.TextMm.Text2 =
        NumberToString(Month(local.Group.Item.LegalReferral.ReferralDate), 14, 2);
        
      local.TextDd.Text2 =
        NumberToString(Day(local.Group.Item.LegalReferral.ReferralDate), 14, 2);
        
      local.TextYyyy.Text4 =
        NumberToString(Year(local.Group.Item.LegalReferral.ReferralDate), 12, 4);
        
      local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
        + local.TextYyyy.Text4;

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

    local.Group.CheckIndex();

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
        UpdateJobRun7();
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

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.FormattedAr);
  }

  private void CreateReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.Group.Count + 1;
    var firstPageOnlyInd = "N";
    var lineControl = "02";
    var lineText = "****  END OF REPORT ****";
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData1",
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

  private void CreateReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.Group.Index + 1;
    var firstPageOnlyInd = "N";
    var lineText =
      Substring(NumberToString(local.Group.Index + 1, 11, 5) + " " + local
      .Group.Item.Case1.Number + " " + local.Group.Item.Ap.FormattedName + " " +
      local.Group.Item.Ar.FormattedName + " " + local.TextDate.Text10 + " " + (
        local.Group.Item.LegalReferral.Status ?? "") + " " + local
      .Group.Item.LegalReferral.ReferralReason1 + " " + local
      .Group.Item.LegalReferral.ReferralReason2 + " " + local
      .Group.Item.LegalReferral.ReferralReason3 + " " + local
      .Group.Item.LegalReferral.ReferralReason4 + "  " + local
      .Group.Item.LegalReferralAssignment.OverrideInd + "   " + local
      .Group.Item.Program.Code, 1, 132);
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
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

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = "";
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private void CreateReportData3()
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
    Update("CreateReportData3",
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
        entities.ExistingKeyOnly.Populated = true;
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
        entities.ExistingKeyOnly.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);

        return true;
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

  private IEnumerable<bool> ReadLegalReferralLegalReferralAssignmentCase()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingLegalReferralAssignment.Populated = false;
    entities.ExistingCase.Populated = false;
    entities.ExistingLegalReferral.Populated = false;

    return ReadEach("ReadLegalReferralLegalReferralAssignmentCase",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          local.ParmLegalReferralAssignment.OverrideInd);
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
      },
      (db, reader) =>
      {
        entities.ExistingLegalReferral.CasNumber = db.GetString(reader, 0);
        entities.ExistingLegalReferralAssignment.CasNo =
          db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingLegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.ExistingLegalReferral.Status = db.GetNullableString(reader, 2);
        entities.ExistingLegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.ExistingLegalReferral.ReferralReason1 =
          db.GetString(reader, 4);
        entities.ExistingLegalReferral.ReferralReason2 =
          db.GetString(reader, 5);
        entities.ExistingLegalReferral.ReferralReason3 =
          db.GetString(reader, 6);
        entities.ExistingLegalReferral.ReferralReason4 =
          db.GetString(reader, 7);
        entities.ExistingLegalReferralAssignment.OverrideInd =
          db.GetString(reader, 8);
        entities.ExistingLegalReferralAssignment.EffectiveDate =
          db.GetDate(reader, 9);
        entities.ExistingLegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingLegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ExistingLegalReferralAssignment.SpdId =
          db.GetInt32(reader, 12);
        entities.ExistingLegalReferralAssignment.OffId =
          db.GetInt32(reader, 13);
        entities.ExistingLegalReferralAssignment.OspCode =
          db.GetString(reader, 14);
        entities.ExistingLegalReferralAssignment.OspDate =
          db.GetDate(reader, 15);
        entities.ExistingLegalReferralAssignment.Populated = true;
        entities.ExistingCase.Populated = true;
        entities.ExistingLegalReferral.Populated = true;

        return true;
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
        db.SetDate(
          command, "effectiveDate",
          local.ParmOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", local.ParmOfficeServiceProvider.RoleCode);
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

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var endTimestamp = Now();
    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.EndTimestamp = endTimestamp;
    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "ERROR";
    var errorMsg =
      "ERROR: Invalid Parm Value for Office Service Provider Effective Date";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "ERROR";
    var errorMsg = "ERROR: Invalid Parm Value for Office Service Provider Role";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "ERROR";
    var errorMsg = "ERROR: Invalid Parm Value for Office";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun4",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun5()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "ERROR";
    var errorMsg = "ERROR: Invalid Parm Value for Service Provider";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun5",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun6()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "PROCESSING";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun6",
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

  private void UpdateJobRun7()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "WAIT";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun7",
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
      /// A value of LegalReferralAssignment.
      /// </summary>
      [JsonPropertyName("legalReferralAssignment")]
      public LegalReferralAssignment LegalReferralAssignment
      {
        get => legalReferralAssignment ??= new();
        set => legalReferralAssignment = value;
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
      /// A value of LegalReferral.
      /// </summary>
      [JsonPropertyName("legalReferral")]
      public LegalReferral LegalReferral
      {
        get => legalReferral ??= new();
        set => legalReferral = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30000;

      private LegalReferralAssignment legalReferralAssignment;
      private Case1 case1;
      private CsePersonsWorkSet ap;
      private CsePersonsWorkSet ar;
      private LegalReferral legalReferral;
      private Program program;
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
    /// A value of ParmLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("parmLegalReferralAssignment")]
    public LegalReferralAssignment ParmLegalReferralAssignment
    {
      get => parmLegalReferralAssignment ??= new();
      set => parmLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Ap1Compare.
    /// </summary>
    [JsonPropertyName("ap1Compare")]
    public CsePersonsWorkSet Ap1Compare
    {
      get => ap1Compare ??= new();
      set => ap1Compare = value;
    }

    /// <summary>
    /// A value of LoopEnd.
    /// </summary>
    [JsonPropertyName("loopEnd")]
    public Common LoopEnd
    {
      get => loopEnd ??= new();
      set => loopEnd = value;
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
    /// A value of Grp1LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("grp1LegalReferralAssignment")]
    public LegalReferralAssignment Grp1LegalReferralAssignment
    {
      get => grp1LegalReferralAssignment ??= new();
      set => grp1LegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Grp1Case.
    /// </summary>
    [JsonPropertyName("grp1Case")]
    public Case1 Grp1Case
    {
      get => grp1Case ??= new();
      set => grp1Case = value;
    }

    /// <summary>
    /// A value of GrpAp1.
    /// </summary>
    [JsonPropertyName("grpAp1")]
    public CsePersonsWorkSet GrpAp1
    {
      get => grpAp1 ??= new();
      set => grpAp1 = value;
    }

    /// <summary>
    /// A value of GrpAr1.
    /// </summary>
    [JsonPropertyName("grpAr1")]
    public CsePersonsWorkSet GrpAr1
    {
      get => grpAr1 ??= new();
      set => grpAr1 = value;
    }

    /// <summary>
    /// A value of Grp1LegalReferral.
    /// </summary>
    [JsonPropertyName("grp1LegalReferral")]
    public LegalReferral Grp1LegalReferral
    {
      get => grp1LegalReferral ??= new();
      set => grp1LegalReferral = value;
    }

    /// <summary>
    /// A value of Grp1Program.
    /// </summary>
    [JsonPropertyName("grp1Program")]
    public Program Grp1Program
    {
      get => grp1Program ??= new();
      set => grp1Program = value;
    }

    /// <summary>
    /// A value of Grp2LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("grp2LegalReferralAssignment")]
    public LegalReferralAssignment Grp2LegalReferralAssignment
    {
      get => grp2LegalReferralAssignment ??= new();
      set => grp2LegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Grp2Case.
    /// </summary>
    [JsonPropertyName("grp2Case")]
    public Case1 Grp2Case
    {
      get => grp2Case ??= new();
      set => grp2Case = value;
    }

    /// <summary>
    /// A value of GrpAp2.
    /// </summary>
    [JsonPropertyName("grpAp2")]
    public CsePersonsWorkSet GrpAp2
    {
      get => grpAp2 ??= new();
      set => grpAp2 = value;
    }

    /// <summary>
    /// A value of GrpAr2.
    /// </summary>
    [JsonPropertyName("grpAr2")]
    public CsePersonsWorkSet GrpAr2
    {
      get => grpAr2 ??= new();
      set => grpAr2 = value;
    }

    /// <summary>
    /// A value of Grp2LegalReferral.
    /// </summary>
    [JsonPropertyName("grp2LegalReferral")]
    public LegalReferral Grp2LegalReferral
    {
      get => grp2LegalReferral ??= new();
      set => grp2LegalReferral = value;
    }

    /// <summary>
    /// A value of Grp2Program.
    /// </summary>
    [JsonPropertyName("grp2Program")]
    public Program Grp2Program
    {
      get => grp2Program ??= new();
      set => grp2Program = value;
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
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
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
    /// A value of ParmOffice.
    /// </summary>
    [JsonPropertyName("parmOffice")]
    public Office ParmOffice
    {
      get => parmOffice ??= new();
      set => parmOffice = value;
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
    /// A value of ParmAp.
    /// </summary>
    [JsonPropertyName("parmAp")]
    public TextWorkArea ParmAp
    {
      get => parmAp ??= new();
      set => parmAp = value;
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
    /// A value of ParmProgram.
    /// </summary>
    [JsonPropertyName("parmProgram")]
    public Program ParmProgram
    {
      get => parmProgram ??= new();
      set => parmProgram = value;
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
    /// A value of ParmCase.
    /// </summary>
    [JsonPropertyName("parmCase")]
    public Case1 ParmCase
    {
      get => parmCase ??= new();
      set => parmCase = value;
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
    /// A value of DebugMode.
    /// </summary>
    [JsonPropertyName("debugMode")]
    public Common DebugMode
    {
      get => debugMode ??= new();
      set => debugMode = value;
    }

    /// <summary>
    /// A value of ArrearsBalance.
    /// </summary>
    [JsonPropertyName("arrearsBalance")]
    public Common ArrearsBalance
    {
      get => arrearsBalance ??= new();
      set => arrearsBalance = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NumberOfCases.
    /// </summary>
    [JsonPropertyName("numberOfCases")]
    public Common NumberOfCases
    {
      get => numberOfCases ??= new();
      set => numberOfCases = value;
    }

    private LegalReferralAssignment parmLegalReferralAssignment;
    private CsePersonsWorkSet ap1Compare;
    private Common loopEnd;
    private Common loopAgain;
    private Array<GroupGroup> group;
    private LegalReferralAssignment grp1LegalReferralAssignment;
    private Case1 grp1Case;
    private CsePersonsWorkSet grpAp1;
    private CsePersonsWorkSet grpAr1;
    private LegalReferral grp1LegalReferral;
    private Program grp1Program;
    private LegalReferralAssignment grp2LegalReferralAssignment;
    private Case1 grp2Case;
    private CsePersonsWorkSet grpAp2;
    private CsePersonsWorkSet grpAr2;
    private LegalReferral grp2LegalReferral;
    private Program grp2Program;
    private WorkArea textDate;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private ReportData nullReportData;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea orderBy;
    private ServiceProvider parmServiceProvider;
    private Office parmOffice;
    private OfficeServiceProvider parmOfficeServiceProvider;
    private TextWorkArea parmAp;
    private TextWorkArea parmApFi;
    private LegalReferral parmLegalReferral;
    private TextWorkArea parmReferralReason;
    private Program parmProgram;
    private TextWorkArea parmShowOnly;
    private Case1 parmCase;
    private CsePersonsWorkSet formattedServiceProvider;
    private CsePersonsWorkSet formattedAp;
    private CsePersonsWorkSet formattedAr;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Common apNameLength;
    private Program tmp;
    private External external;
    private Array<HeaderGroup> header;
    private Common debugMode;
    private Common arrearsBalance;
    private DateWorkArea initialized;
    private Common numberOfCases;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingSearchByAp.
    /// </summary>
    [JsonPropertyName("existingSearchByAp")]
    public CsePerson ExistingSearchByAp
    {
      get => existingSearchByAp ??= new();
      set => existingSearchByAp = value;
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
    /// A value of ExistingLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("existingLegalReferralAssignment")]
    public LegalReferralAssignment ExistingLegalReferralAssignment
    {
      get => existingLegalReferralAssignment ??= new();
      set => existingLegalReferralAssignment = value;
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
    /// A value of ExistingLegalReferral.
    /// </summary>
    [JsonPropertyName("existingLegalReferral")]
    public LegalReferral ExistingLegalReferral
    {
      get => existingLegalReferral ??= new();
      set => existingLegalReferral = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ReportData New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    private CsePerson existingSearchByAp;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private LegalReferralAssignment existingLegalReferralAssignment;
    private CsePerson existingKeyOnly;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private LegalReferral existingLegalReferral;
    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData new1;
    private DebtDetail existingDebtDetail;
    private ObligationTransaction existingDebt;
    private Obligation existingObligation;
    private CsePersonAccount existingObligor;
    private CsePerson keyOnly;
  }
#endregion
}
