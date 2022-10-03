// Program: FN_B715_OCSE_157_WITH_VERIFICATN, ID: 372927663, model: 746.
// Short name: SWEF715B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B715_OCSE_157_WITH_VERIFICATN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB715Ocse157WithVerificatn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B715_OCSE_157_WITH_VERIFICATN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB715Ocse157WithVerificatn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB715Ocse157WithVerificatn.
  /// </summary>
  public FnB715Ocse157WithVerificatn(IContext context, Import import,
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
    // *******************************************************************
    // *
    // *                       M A I N T E N A N C E    L O G
    // *
    // *******************************************************************
    // *
    // *   Date   Developer    PR#     Reason/Modification
    // * -------- ---------    ---     -------------------
    // * 10/27/99 SWSRCHF   H00077482  New program
    // *
    // 
    // same as program SWEFB710, but
    // *
    // 
    // writes verification data to
    // *
    // 
    // sequential files for lines
    // *
    // 
    // 1, 2, 16, 24, 25, 28 and 29
    // *
    // * 12/15/99 SWSRCHF   H00082840  Added check for emancipation date
    // *
    // 
    // to lines 4, 5, 6, 7 and 16
    // *
    // 
    // (new Federal requirement)
    // *
    // * 04/06/00 E. Parker  WR 160-E	Changed logic to use new CSE
    // *				Person attributes for Paternity
    // *				and B.O.W.    Fixed
    // *				Emancipation logic to
    // *				recognize default date
    // *				of '0001-01-01'.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *** set max DATE to '2099-12-31'
    local.Max.Date = new DateTime(2099, 12, 31);
    local.NotFound.Flag = "Y";

    // ***
    // *** get the process date from Program Processing Info,
    // *** where NAME is SWEFB710
    // ***
    if (ReadProgramProcessingInfo())
    {
      local.NotFound.Flag = "N";
    }

    // ****** Fiscal Year START date
    // ***
    // *** set to October 1st of the previous year
    local.Temp.Year =
      Year(AddYears(entities.ProgramProcessingInfo.ProcessDate, -1));
    local.Temp.Month = 10;
    local.Temp.Day = 1;
    local.Temp.TextDate = NumberToString(local.Temp.Year, 12, 4) + NumberToString
      (local.Temp.Month, 14, 2) + NumberToString(local.Temp.Day, 14, 2);
    local.WorkCommon.Count = (int)StringToNumber(local.Temp.TextDate);
    local.FiscalYearStart.Date = IntToDate(local.WorkCommon.Count);

    // ****** Fiscal Year END date
    // ***
    // *** set to September 30th of the processing year
    local.Temp.Year = Year(entities.ProgramProcessingInfo.ProcessDate);
    local.Temp.Month = 9;
    local.Temp.Day = 30;
    local.Temp.TextDate = NumberToString(local.Temp.Year, 12, 4) + NumberToString
      (local.Temp.Month, 14, 2) + NumberToString(local.Temp.Day, 14, 2);
    local.WorkCommon.Count = (int)StringToNumber(local.Temp.TextDate);
    local.FiscalYearEnd.Date = IntToDate(local.WorkCommon.Count);

    // ***
    // *** OPEN the Error Report
    // ***
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = entities.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProgramName = entities.ProgramProcessingInfo.Name;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (AsChar(local.NotFound.Flag) == 'Y')
    {
      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }

      return;
    }

    // ***
    // *** OPEN the Case Verification Extract file
    // ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabCaseVerificationExtract2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error opening extract file
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ****
    //  ***
    // *** get each Case (OPEN) and Case Role (CHILD) combination
    //  ***
    //   ****
    // ==> lines 1, 1a or 1b <==
    foreach(var item in ReadCase4())
    {
      local.InterstateFound.Flag = "N";

      // *** Determine if Case is Interstate
      // ***
      // *** get each Interstate Request for current Case
      // ***
      if (ReadInterstateRequest())
      {
        local.InterstateFound.Flag = "Y";
      }

      local.WorkCase.Number = entities.Case1.Number;
      UseDetermineTypeOfAssistance();

      // ***
      // *** Initialize the Case Verification Extract file layout
      // ***
      local.CaseVerificationExtract.Assign(local.InitCaseVerificationExtract);

      // ***
      // *** Move CASE info to the Case Verification Extract file layout
      // ***
      local.CaseVerificationExtract.Cnumber = entities.Case1.Number;
      local.CaseVerificationExtract.CopenDate = entities.Case1.CseOpenDate;
      local.CaseVerificationExtract.Cstatus = entities.Case1.Status ?? Spaces
        (1);

      if (AsChar(local.Current.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 1 Current";

        // ***
        // *** get Case Role, CSE Person, Person Program and Program info
        // ***
        UseCabGetCurrentProgramInfo();
      }

      if (AsChar(local.Former.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 1 Former";

        // ***
        // *** get Case Role, CSE Person, Person Program and Program info
        // ***
        UseCabGetFormerProgramInfo();
      }

      if (AsChar(local.Never.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 1 Never";
      }

      if (AsChar(local.Current.Flag) == 'Y')
      {
        // *** Cases Open at the End of the Fiscal Year
        // *** (line 1)
        // ***
        // *** CURRENT ASSISTANCE
        ++local.Local1Current.Count;

        if (AsChar(local.InterstateFound.Flag) == 'Y')
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
          {
            // *** Interstate Cases Received From Another State
            // *** Open at the End of the Fiscal Year
            // *** (line 1b)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local1BCurrent.Count;

            goto Test1;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // *** Interstate Cases Initiated in This State
            // *** Open at the End of the Fiscal Year
            // *** (line 1a)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local1ACurrent.Count;
          }
        }
      }

Test1:

      if (AsChar(local.Former.Flag) == 'Y')
      {
        // *** Cases Open at the End of the Fiscal Year
        // *** (line 1)
        // ***
        // *** FORMER ASSISTANCE
        ++local.Local1Former.Count;

        if (AsChar(local.InterstateFound.Flag) == 'Y')
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
          {
            // *** Interstate Cases Received From Another State
            // *** Open at the End of the Fiscal Year
            // *** (line 1b)
            // ***
            // *** FORMER ASSISTANCE
            ++local.Local1BFormer.Count;

            goto Test2;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // *** Interstate Cases Initiated in This State
            // *** Open at the End of the Fiscal Year
            // *** (line 1a)
            // ***
            // *** FORMER ASSISTANCE
            ++local.Local1AFormer.Count;
          }
        }
      }

Test2:

      if (AsChar(local.Never.Flag) == 'Y')
      {
        // *** Cases Open at the End of the Fiscal Year
        // *** (line 1)
        // ***
        // *** NEVER ASSISTANCE
        ++local.Local1Never.Count;

        if (AsChar(local.InterstateFound.Flag) == 'Y')
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
          {
            // *** Interstate Cases Received From Another State
            // *** Open at the End of the Fiscal Year
            // *** (line 1b)
            // ***
            // *** NEVER ASSISTANCE
            ++local.Local1BNever.Count;

            goto Test3;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // *** Interstate Cases Initiated in This State
            // *** Open at the End of the Fiscal Year
            // *** (line 1a)
            // ***
            // *** NEVER ASSISTANCE
            ++local.Local1ANever.Count;
          }
        }
      }

Test3:

      // ***
      // *** Write a record to the Case Verification Extract file
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      UseEabCaseVerificationExtract1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error Writing to the Case Verification Extract file";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";
        }

        return;
      }
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case (OPEN)
    //  ***
    //   ****
    // ==> lines 1c and 2d <==
    foreach(var item in ReadCase4())
    {
      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRoleCsePerson2())
      {
        // ***
        // *** get each Person Program and Program for current CSE Person
        // ***
        foreach(var item2 in ReadPersonProgramProgram11())
        {
          goto ReadEach1;
        }

        goto ReadEach2;

ReadEach1:
        ;
      }

      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRoleCsePerson2())
      {
        // ***
        // *** get each Person Program and Program for current CSE Person
        // ***
        foreach(var item2 in ReadPersonProgramProgram6())
        {
          goto ReadEach2;
        }
      }

      // *** Medicaid Only Cases
      // *** Open at the End of the Fiscal Year
      // *** (line 1c)
      // ***
      // *** NEVER ASSISTANCE
      ++local.Local1CNever.Count;
      local.Found.Flag = "N";

      // ***
      // *** get each Case Role for current Case
      // ***
      foreach(var item1 in ReadCaseRole4())
      {
        // ***
        // *** get each Legal Action Case Role for current Case Role
        // ***
        foreach(var item2 in ReadLegalActionCaseRole1())
        {
          // ***
          // *** get Legal Action for current Legal Action Case Role
          // ***
          if (ReadLegalAction2())
          {
            if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") && !
              Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
              Equal(entities.LegalAction.ActionTaken, "JEF") && !
              Equal(entities.LegalAction.ActionTaken, "JENF") && !
              Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
              Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
              Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
              Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
              Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
              Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
            {
              continue;
            }

            // ***
            // *** get each Legal Action Detail for current Legal Action
            // ***
            foreach(var item3 in ReadLegalActionDetail2())
            {
              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                if (!Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") &&
                  !Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
                {
                  // *** discard
                  continue;
                }
              }

              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                // *** Medicaid Only Cases With Orders
                // *** Open at the End of the Fiscal Year
                // *** (line 2d)
                // ***
                // *** NEVER ASSISTANCE
                ++local.Local2DNever.Count;

                goto ReadEach2;
              }
              else
              {
                // ***
                // *** get Obligation Type for current Legal Action Detail
                // ***
                if (ReadObligationType2())
                {
                  if (Equal(entities.ObligationType.Code, "AJ") || Equal
                    (entities.ObligationType.Code, "CRCH") || Equal
                    (entities.ObligationType.Code, "CS") || Equal
                    (entities.ObligationType.Code, "MC") || Equal
                    (entities.ObligationType.Code, "MJ") || Equal
                    (entities.ObligationType.Code, "MS") || Equal
                    (entities.ObligationType.Code, "SAJ") || Equal
                    (entities.ObligationType.Code, "SP"))
                  {
                    // *** Medicaid Only Cases With Orders
                    // *** Open at the End of the Fiscal Year
                    // *** (line 2d)
                    // ***
                    // *** NEVER ASSISTANCE
                    ++local.Local2DNever.Count;

                    goto ReadEach2;
                  }
                }
                else
                {
                  ExitState = "OBLIGATION_TYPE_NF";

                  // ***
                  // *** WRITE to the Error Report
                  // ***
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Obligation Type not found for Legal Action Detail " + NumberToString
                    (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                    NumberToString(entities.LegalAction.Identifier, 15) + NumberToString
                    (entities.LegalActionDetail.Number, 15);
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  goto ReadEach2;
                }
              }
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";

            // *** extract Year from timestamp
            local.DateWorkAttributes.NumericalYear =
              Year(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Month from timestamp
            local.DateWorkAttributes.NumericalMonth =
              Month(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Day from timestamp
            local.DateWorkAttributes.NumericalDay =
              Day(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build date
            local.DateWorkAttributes.TextDate10Char =
              NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalMonth, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalDay, 15);

            // *** extract Hours from timestamp
            local.TimeWorkAttributes.NumericalHours =
              Hour(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Minutes from timestamp
            local.TimeWorkAttributes.NumericalMinutes =
              Minute(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Seconds from timestamp
            local.TimeWorkAttributes.NumericalSeconds =
              Second(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Microseconds from timestamp
            local.TimeWorkAttributes.NumericalMicroseconds =
              Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build time
            local.TimeWorkAttributes.TextTime15Char =
              NumberToString(local.TimeWorkAttributes.NumericalHours, 15) + "."
              + NumberToString
              (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalMicroseconds, 15);

            // *** build message
            local.NeededToWrite.RptDetail =
              "Legal Action not found for Legal Action case Role with timestamp " +
              local.DateWorkAttributes.TextDate10Char + "-" + local
              .TimeWorkAttributes.TextTime15Char;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach2;
          }
        }
      }

ReadEach2:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case (OPEN) and Case Role (CHILD) combination
    //  ***
    //   ****
    // ==> lines 2, 2a and 2b <==
    foreach(var item in ReadCase4())
    {
      local.InterstateFound.Flag = "N";

      // *** Determine if Case is Interstate
      // ***
      // *** get each Interstate Request for current Case
      // ***
      if (ReadInterstateRequest())
      {
        local.InterstateFound.Flag = "Y";
      }

      local.WorkCase.Number = entities.Case1.Number;
      UseDetermineTypeOfAssistance();

      // ***
      // *** Initialize the Case Verification Extract file layout
      // ***
      local.CaseVerificationExtract.Assign(local.InitCaseVerificationExtract);

      // ***
      // *** Move CASE info to the Case Verification Extract file layout
      // ***
      local.CaseVerificationExtract.Cnumber = entities.Case1.Number;
      local.CaseVerificationExtract.CopenDate = entities.Case1.CseOpenDate;
      local.CaseVerificationExtract.Cstatus = entities.Case1.Status ?? Spaces
        (1);

      if (AsChar(local.Current.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 2 Current";

        // ***
        // *** get Case Role, CSE Person, Person Program and Program info
        // ***
        UseCabGetCurrentProgramInfo();
      }

      if (AsChar(local.Former.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 2 Former";

        // ***
        // *** get Case Role, CSE Person, Person Program and Program info
        // ***
        UseCabGetFormerProgramInfo();
      }

      if (AsChar(local.Never.Flag) == 'Y')
      {
        local.CaseVerificationExtract.DetailsForLine = "Line 2 Never";
      }

      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRole5())
      {
        // *** Determine Cases With Support Orders Established
        // ***
        // *** get each Legal Action Case Role for current Case Role
        // ***
        foreach(var item2 in ReadLegalActionCaseRole2())
        {
          // ***
          // *** get Legal Action for current Legal Action Case Role
          // ***
          if (ReadLegalAction2())
          {
            if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") && !
              Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
              Equal(entities.LegalAction.ActionTaken, "JEF") && !
              Equal(entities.LegalAction.ActionTaken, "JENF") && !
              Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
              Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
              Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
              Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
              Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
              Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
            {
              continue;
            }

            // ***
            // *** get each Legal Action Details for current Legal Action, where
            // *** Legal Action Detail DETAIL_TYPE is 'F' or 'N'
            // ***
            foreach(var item3 in ReadLegalActionDetail2())
            {
              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                if (!Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") &&
                  !Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
                {
                  // *** discard
                  continue;
                }
              }

              // ***
              // *** get Legal Action Case Role, Legal Action and Legal Action 
              // Detail info
              // ***
              local.CaseVerificationExtract.LacrCreatedTstamp =
                entities.LegalActionCaseRole.CreatedTstamp;
              local.CaseVerificationExtract.LaIdentifier =
                entities.LegalAction.Identifier;
              local.CaseVerificationExtract.LaActionTaken =
                entities.LegalAction.ActionTaken;
              local.CaseVerificationExtract.LaCreatedTstamp =
                entities.LegalAction.CreatedTstamp;
              local.CaseVerificationExtract.LadNumber =
                entities.LegalActionDetail.Number;
              local.CaseVerificationExtract.LadDetailType =
                entities.LegalActionDetail.DetailType;
              local.CaseVerificationExtract.LadCreatedTstamp =
                entities.LegalActionDetail.CreatedTstamp;
              local.CaseVerificationExtract.LadNonFinOblgType =
                entities.LegalActionDetail.NonFinOblgType ?? Spaces(4);

              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                if (AsChar(local.Current.Flag) == 'Y')
                {
                  // *** Cases Open at the End of the Fiscal Year With
                  // *** Support Orders Established
                  // *** (line 2)
                  // ***
                  // *** CURRENT ASSISTANCE
                  ++local.Local2Current.Count;

                  if (AsChar(local.InterstateFound.Flag) == 'Y')
                  {
                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                    {
                      // *** Interstate Cases Received From Another State
                      // *** With Support Orders at the End of the Fiscal Year
                      // *** (line 2b)
                      // ***
                      // *** CURRENT ASSISTANCE
                      ++local.Local2BCurrent.Count;

                      goto Test4;
                    }

                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                    {
                      // *** Interstate Cases Initiated in This State
                      // *** With Support Orders at the End of the Fiscal Year
                      // *** (line 2a)
                      // ***
                      // *** CURRENT ASSISTANCE
                      ++local.Local2ACurrent.Count;
                    }
                  }
                }

Test4:

                if (AsChar(local.Former.Flag) == 'Y')
                {
                  // *** Cases Open at the End of the Fiscal Year With
                  // *** Support Orders Established
                  // *** (line 2)
                  // ***
                  // *** FORMER ASSISTANCE
                  ++local.Local2Former.Count;

                  if (AsChar(local.InterstateFound.Flag) == 'Y')
                  {
                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                    {
                      // *** Interstate Cases Received From Another State
                      // *** With Support Orders at the End of the Fiscal Year
                      // *** (line 2b)
                      // ***
                      // *** FORMER ASSISTANCE
                      ++local.Local2BFormer.Count;

                      goto Test5;
                    }

                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                    {
                      // *** Interstate Cases Initiated in This State With 
                      // Support Orders
                      // *** at the End of the Fiscal Year
                      // *** (line 2a)
                      // ***
                      // *** FORMER ASSISTANCE
                      ++local.Local2AFormer.Count;
                    }
                  }
                }

Test5:

                if (AsChar(local.Never.Flag) == 'Y')
                {
                  // *** Cases Open at the End of the Fiscal Year With
                  // *** Support Orders Established
                  // *** (line 2)
                  // ***
                  // *** NEVER ASSISTANCE
                  ++local.Local2Never.Count;

                  if (AsChar(local.InterstateFound.Flag) == 'Y')
                  {
                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                    {
                      // *** Interstate Cases Received From Another State
                      // *** With Support Orders at the End of the Fiscal Year
                      // *** (line 2b)
                      // ***
                      // *** NEVER ASSISTANCE
                      ++local.Local2BNever.Count;

                      goto Test6;
                    }

                    if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                    {
                      // *** Interstate Cases Initiated in This State With 
                      // Support Orders
                      // *** at the End of the Fiscal Year
                      // *** (line 2a)
                      // ***
                      // *** NEVER ASSISTANCE
                      ++local.Local2ANever.Count;
                    }
                  }
                }

Test6:

                // ***
                // *** Write a record to the Case Verification Extract file
                // ***
                local.ReportParms.Parm1 = "GR";
                local.ReportParms.Parm2 = "";
                UseEabCaseVerificationExtract1();

                if (!IsEmpty(local.ReportParms.Parm1))
                {
                  ExitState = "ERROR_WRITING_TO_FILE_AB";

                  // ***
                  // *** WRITE to the Error Report
                  // ***
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Error Writing to the Case Verification Extract file";
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ERROR_WRITING_TO_REPORT_AB";
                  }

                  return;
                }

                goto ReadEach3;
              }

              // ***
              // *** get Obligation Type for current Obligation, where
              // *** Obligation Type CODE is 'AJ', 'CRCH', 'CS', 'MC', 'MJ', '
              // MS',
              // *** 'SAJ' or 'SP'
              // ***
              if (ReadObligationType2())
              {
                if (Equal(entities.ObligationType.Code, "AJ") || Equal
                  (entities.ObligationType.Code, "CRCH") || Equal
                  (entities.ObligationType.Code, "CS") || Equal
                  (entities.ObligationType.Code, "MC") || Equal
                  (entities.ObligationType.Code, "MJ") || Equal
                  (entities.ObligationType.Code, "MS") || Equal
                  (entities.ObligationType.Code, "SAJ") || Equal
                  (entities.ObligationType.Code, "SP"))
                {
                  // ***
                  // *** get Obligation Type info
                  // ***
                  local.CaseVerificationExtract.OtCode =
                    entities.ObligationType.Code;

                  if (AsChar(local.Current.Flag) == 'Y')
                  {
                    // *** Cases Open at the End of the Fiscal Year With
                    // *** Support Orders Established
                    // *** (line 2)
                    // ***
                    // *** CURRENT ASSISTANCE
                    ++local.Local2Current.Count;

                    if (AsChar(local.InterstateFound.Flag) == 'Y')
                    {
                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                      {
                        if (Equal(entities.ObligationType.Code, "CRCH"))
                        {
                          goto Test7;
                        }

                        // *** Interstate Cases Received From Another State
                        // *** With Support Orders at the End of the Fiscal Year
                        // *** (line 2b)
                        // ***
                        // *** CURRENT ASSISTANCE
                        ++local.Local2BCurrent.Count;

                        goto Test7;
                      }

                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                      {
                        // *** Interstate Cases Initiated in This State With 
                        // Support Orders
                        // *** at the End of the Fiscal Year
                        // *** (line 2a)
                        // ***
                        // *** CURRENT ASSISTANCE
                        ++local.Local2ACurrent.Count;
                      }
                    }
                  }

Test7:

                  if (AsChar(local.Former.Flag) == 'Y')
                  {
                    // *** Cases Open at the End of the Fiscal Year With
                    // *** Support Orders Established
                    // *** (line 2)
                    // ***
                    // *** FORMER ASSISTANCE
                    ++local.Local2Former.Count;

                    if (AsChar(local.InterstateFound.Flag) == 'Y')
                    {
                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                      {
                        if (Equal(entities.ObligationType.Code, "CRCH"))
                        {
                          goto Test8;
                        }

                        // *** Interstate Cases Received From Another State
                        // *** With Support Orders at the End of the Fiscal Year
                        // *** (line 2b)
                        // ***
                        // *** FORMER ASSISTANCE
                        ++local.Local2BFormer.Count;

                        goto Test8;
                      }

                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                      {
                        // *** Interstate Cases Initiated in This State With 
                        // Support Orders
                        // *** at the End of the Fiscal Year
                        // *** (line 2a)
                        // ***
                        // *** FORMER ASSISTANCE
                        ++local.Local2AFormer.Count;
                      }
                    }
                  }

Test8:

                  if (AsChar(local.Never.Flag) == 'Y')
                  {
                    // *** Cases Open at the End of the Fiscal Year With
                    // *** Support Orders Established
                    // *** (line 2)
                    // ***
                    // *** NEVER ASSISTANCE
                    ++local.Local2Never.Count;

                    if (AsChar(local.InterstateFound.Flag) == 'Y')
                    {
                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                      {
                        if (Equal(entities.ObligationType.Code, "CRCH"))
                        {
                          goto Test9;
                        }

                        // *** Interstate Cases Received From Another State
                        // *** With Support Orders at the End of the Fiscal Year
                        // *** (line 2b)
                        // ***
                        // *** NEVER ASSISTANCE
                        ++local.Local2BNever.Count;

                        goto Test9;
                      }

                      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                      {
                        // *** Interstate Cases Initiated in This State With 
                        // Support Orders
                        // *** at the End of the Fiscal Year
                        // *** (line 2a)
                        // ***
                        // *** NEVER ASSISTANCE
                        ++local.Local2ANever.Count;
                      }
                    }
                  }

Test9:

                  // ***
                  // *** Write a record to the Case Verification Extract file
                  // ***
                  local.ReportParms.Parm1 = "GR";
                  local.ReportParms.Parm2 = "";
                  UseEabCaseVerificationExtract1();

                  if (!IsEmpty(local.ReportParms.Parm1))
                  {
                    ExitState = "ERROR_WRITING_TO_FILE_AB";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Error Writing to the Case Verification Extract file";
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ERROR_WRITING_TO_REPORT_AB";
                    }

                    return;
                  }

                  goto ReadEach3;
                }

                continue;
              }
              else
              {
                ExitState = "OBLIGATION_TYPE_NF";

                // ***
                // *** WRITE to the Error Report
                // ***
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Obligation Type not found for Legal Action Detail " + NumberToString
                  (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                  NumberToString(entities.LegalAction.Identifier, 15) + NumberToString
                  (entities.LegalActionDetail.Number, 15);
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                goto ReadEach3;
              }
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";

            // *** extract Year from timestamp
            local.DateWorkAttributes.NumericalYear =
              Year(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Month from timestamp
            local.DateWorkAttributes.NumericalMonth =
              Month(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Day from timestamp
            local.DateWorkAttributes.NumericalDay =
              Day(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build date
            local.DateWorkAttributes.TextDate10Char =
              NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalMonth, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalDay, 15);

            // *** extract Hours from timestamp
            local.TimeWorkAttributes.NumericalHours =
              Hour(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Minutes from timestamp
            local.TimeWorkAttributes.NumericalMinutes =
              Minute(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Seconds from timestamp
            local.TimeWorkAttributes.NumericalSeconds =
              Second(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Microseconds from timestamp
            local.TimeWorkAttributes.NumericalMicroseconds =
              Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build time
            local.TimeWorkAttributes.TextTime15Char =
              NumberToString(local.TimeWorkAttributes.NumericalHours, 15) + "."
              + NumberToString
              (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalMicroseconds, 15);

            // *** build message
            local.NeededToWrite.RptDetail =
              "Legal Action not found for Legal Action case Role with timestamp " +
              local.DateWorkAttributes.TextDate10Char + "-" + local
              .TimeWorkAttributes.TextTime15Char;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach3;
          }
        }
      }

ReadEach3:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case (OPEN) and Case Role (CHILD) combination
    //  ***
    //   ****
    // ==> lines 2c <==
    foreach(var item in ReadCase4())
    {
      local.WorkCase.Number = entities.Case1.Number;
      UseDetermineTypeOfAssistance();

      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRole5())
      {
        // *** Determine Cases With Support Orders Established
        // ***
        // *** get each Legal Action Case Role for current Case Role
        // ***
        foreach(var item2 in ReadLegalActionCaseRole2())
        {
          // ***
          // *** get Legal Action for current Legal Action Case Role
          // ***
          if (ReadLegalAction2())
          {
            if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") && !
              Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
              Equal(entities.LegalAction.ActionTaken, "JEF") && !
              Equal(entities.LegalAction.ActionTaken, "JENF") && !
              Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
              Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
              Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
              Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
              Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
              Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
              Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
              Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
              Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
            {
              continue;
            }

            local.FendDatedFound.Flag = "N";
            local.NhicUmFound.Flag = "N";

            // ***
            // *** get each Legal Action Details for current Legal Action, where
            // *** Legal Action Detail DETAIL_TYPE is 'F' or 'N'
            // ***
            foreach(var item3 in ReadLegalActionDetail2())
            {
              switch(AsChar(entities.LegalActionDetail.DetailType))
              {
                case 'F':
                  // *** keep
                  if (!Lt(local.FiscalYearEnd.Date,
                    entities.LegalActionDetail.EndDate))
                  {
                    local.FendDatedFound.Flag = "Y";

                    if (AsChar(local.NhicUmFound.Flag) != 'Y')
                    {
                      continue;
                    }
                    else
                    {
                      goto ReadEach4;
                    }
                  }
                  else
                  {
                    goto ReadEach5;
                  }

                  break;
                case 'N':
                  // *** keep
                  switch(TrimEnd(entities.LegalActionDetail.NonFinOblgType))
                  {
                    case "HIC":
                      // *** keep
                      local.NhicUmFound.Flag = "Y";

                      if (AsChar(local.FendDatedFound.Flag) != 'Y')
                      {
                        continue;
                      }

                      break;
                    case "UM":
                      // *** keep
                      local.NhicUmFound.Flag = "Y";

                      if (AsChar(local.FendDatedFound.Flag) != 'Y')
                      {
                        continue;
                      }

                      break;
                    default:
                      // *** discard
                      continue;
                  }

                  break;
                default:
                  break;
              }
            }

ReadEach4:

            if (AsChar(local.Current.Flag) == 'Y')
            {
              if (AsChar(local.FendDatedFound.Flag) == 'Y' && AsChar
                (local.NhicUmFound.Flag) == 'Y' || AsChar
                (local.FendDatedFound.Flag) == 'N' && AsChar
                (local.NhicUmFound.Flag) == 'Y')
              {
                // *** Cases With Orders for Zero Cash Support
                // *** Open at the End of the Fiscal Year
                // *** (line 2c)
                // ***
                // *** CURRENT ASSISTANCE
                ++local.Local2CCurrent.Count;

                goto ReadEach5;
              }
            }

            if (AsChar(local.Former.Flag) == 'Y')
            {
              if (AsChar(local.FendDatedFound.Flag) == 'Y' && AsChar
                (local.NhicUmFound.Flag) == 'Y' || AsChar
                (local.FendDatedFound.Flag) == 'N' && AsChar
                (local.NhicUmFound.Flag) == 'Y')
              {
                // *** Cases With Orders for Zero Cash Support
                // *** Open at the End of the Fiscal Year
                // *** (line 2c)
                // ***
                // *** FORMER ASSISTANCE
                ++local.Local2CFormer.Count;

                goto ReadEach5;
              }
            }

            if (AsChar(local.Never.Flag) == 'Y')
            {
              if (AsChar(local.FendDatedFound.Flag) == 'Y' && AsChar
                (local.NhicUmFound.Flag) == 'Y' || AsChar
                (local.FendDatedFound.Flag) == 'N' && AsChar
                (local.NhicUmFound.Flag) == 'Y')
              {
                // *** Cases With Orders for Zero Cash Support
                // *** Open at the End of the Fiscal Year
                // *** (line 2c)
                // ***
                // *** NEVER ASSISTANCE
                ++local.Local2CNever.Count;

                goto ReadEach5;
              }
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";

            // *** extract Year from timestamp
            local.DateWorkAttributes.NumericalYear =
              Year(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Month from timestamp
            local.DateWorkAttributes.NumericalMonth =
              Month(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Day from timestamp
            local.DateWorkAttributes.NumericalDay =
              Day(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build date
            local.DateWorkAttributes.TextDate10Char =
              NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalMonth, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalDay, 15);

            // *** extract Hours from timestamp
            local.TimeWorkAttributes.NumericalHours =
              Hour(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Minutes from timestamp
            local.TimeWorkAttributes.NumericalMinutes =
              Minute(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Seconds from timestamp
            local.TimeWorkAttributes.NumericalSeconds =
              Second(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Microseconds from timestamp
            local.TimeWorkAttributes.NumericalMicroseconds =
              Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build time
            local.TimeWorkAttributes.TextTime15Char =
              NumberToString(local.TimeWorkAttributes.NumericalHours, 15) + "."
              + NumberToString
              (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalMicroseconds, 15);

            // *** build message
            local.NeededToWrite.RptDetail =
              "Legal Action not found for Legal Action case Role with timestamp " +
              local.DateWorkAttributes.TextDate10Char + "-" + local
              .TimeWorkAttributes.TextTime15Char;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach5;
          }
        }
      }

ReadEach5:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    local.PrevCsePerson.Number = "";

    // ****
    //  ***
    // *** get each CSE Person (CLIENT) and Case Role (CHILD)
    //  ***
    //   ****
    // ==> lines 4 and 5 <==
    foreach(var item in ReadCsePersonCaseRole())
    {
      if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
      {
        // *** discard
        continue;
      }

      // ***
      // *** get Case for current Case Role
      // ***
      if (ReadCase2())
      {
        // *** is the Case CLOSED??
        if (AsChar(entities.Case1.Status) == 'C')
        {
          if (!Lt(local.FiscalYearEnd.Date, entities.Case1.StatusDate))
          {
            continue;
          }
        }
      }
      else
      {
        ExitState = "CASE_NF";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Case not found for Case Role Identifier " + NumberToString
          (entities.CaseRole.Identifier, 15) + "Case Role Type " + entities
          .CaseRole.Type1;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

      // *** Number of Children in Cases Open at the End of the Fiscal Year
      // *** (line 4)
      // ***
      // *** TOTAL
      ++local.Local4Total.Count;

      // *** Was the child born out of wedlock??
      if (AsChar(entities.CsePerson.BornOutOfWedlock) == 'Y')
      {
        // *** Children in IV-D Cases Open at the End of the Fiscal Year
        // *** Who were Born Out-of_Wedlock
        // *** (line 5)
        // ***
        // *** TOTAL
        ++local.Local5Total.Count;
      }

      local.PrevCsePerson.Number = entities.CsePerson.Number;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case
    //  ***
    //   ****
    // ==> lines 6, 7, 13 and 16 <==
    local.PrevCsePerson.Number = "";

    foreach(var item in ReadCsePersonCaseRole())
    {
      if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
      {
        // *** discard
        continue;
      }

      if (ReadCase1())
      {
        if (AsChar(entities.Case1.Status) == 'C')
        {
          if (Lt(entities.Case1.StatusDate, local.FiscalYearStart.Date))
          {
            // *** discard
            continue;
          }
        }

        if (AsChar(entities.CsePerson.PaternityEstablishedIndicator) == 'Y')
        {
          if (AsChar(entities.CsePerson.CseToEstblPaternity) == 'Y' && Lt
            (local.FiscalYearStart.Date, entities.CsePerson.DatePaternEstab) &&
            !Lt(local.FiscalYearEnd.Date, entities.CsePerson.DatePaternEstab))
          {
            local.WorkCase.Number = entities.Case1.Number;
            UseDetermineTypeOfAssistance();

            // ***
            // *** Initialize the Case Verification Extract file layout
            // ***
            local.CaseVerificationExtract.Assign(
              local.InitCaseVerificationExtract);

            // ***
            // *** Move CASE info to the Case Verification Extract file layout
            // ***
            local.CaseVerificationExtract.Cnumber = entities.Case1.Number;
            local.CaseVerificationExtract.CopenDate =
              entities.Case1.CseOpenDate;
            local.CaseVerificationExtract.Cstatus = entities.Case1.Status ?? Spaces
              (1);
            local.CaseVerificationExtract.CstatusDate =
              entities.Case1.StatusDate;

            if (AsChar(local.Current.Flag) == 'Y')
            {
              local.CaseVerificationExtract.DetailsForLine = "Line 16 Current";

              // ***
              // *** get Case Role, CSE Person, Person Program and Program info
              // ***
              UseCabGetCurrentProgramInfo();
            }

            if (AsChar(local.Former.Flag) == 'Y')
            {
              local.CaseVerificationExtract.DetailsForLine = "Line 16 Former";

              // ***
              // *** get Case Role, CSE Person, Person Program and Program info
              // ***
              UseCabGetFormerProgramInfo();
            }

            if (AsChar(local.Never.Flag) == 'Y')
            {
              local.CaseVerificationExtract.DetailsForLine = "Line 16 Never";
            }

            if (AsChar(local.Current.Flag) == 'Y')
            {
              // *** Children in the IV-D Caseload for Whom Paternity Was
              // *** Established or Acknowledged During the Fiscal Year
              // *** (line 16)
              // ***
              // *** CURRENT ASSISTANCE
              ++local.Local16Current.Count;
            }

            if (AsChar(local.Former.Flag) == 'Y')
            {
              // *** Children in the IV-D Caseload for Whom Paternity Was
              // *** Established or Acknowledged During the Fiscal Year
              // *** (line 16)
              // ***
              // *** FORMER ASSISTANCE
              ++local.Local16Former.Count;
            }

            if (AsChar(local.Never.Flag) == 'Y')
            {
              // *** Children in the IV-D Caseload for Whom Paternity Was
              // *** Established or Acknowledged During the Fiscal Year
              // *** (line 16)
              // ***
              // *** NEVER ASSISTANCE
              ++local.Local16Never.Count;
            }

            // ***
            // *** Write a record to the Case Verification Extract file
            // ***
            local.ReportParms.Parm1 = "GR";
            local.ReportParms.Parm2 = "";
            UseEabCaseVerificationExtract1();

            if (!IsEmpty(local.ReportParms.Parm1))
            {
              ExitState = "ERROR_WRITING_TO_FILE_AB";

              // ***
              // *** WRITE to the Error Report
              // ***
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Error Writing to the Case Verification Extract file";
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_REPORT_AB";
              }

              return;
            }
          }

          if (AsChar(entities.CsePerson.BornOutOfWedlock) == 'Y')
          {
            // *** Children in IV-D Cases Open During or at the End of the
            // *** Fiscal Year With Paternity Established or Acknowledged
            // *** (line 6)
            // ***
            // *** TOTAL
            ++local.Local6Total.Count;
          }
          else
          {
            if (AsChar(entities.Case1.Status) == 'O' || AsChar
              (entities.Case1.Status) == 'C' && Lt
              (local.FiscalYearEnd.Date, entities.Case1.StatusDate))
            {
              // *** Children in the IV-D Cases at the End of the Fiscal Year 
              // with
              // *** Paternity Resolved
              // *** (line 7)
              // ***
              // *** TOTAL
              ++local.Local7Total.Count;
            }
          }
        }
        else if (AsChar(entities.Case1.Status) == 'O' || AsChar
          (entities.Case1.Status) == 'C' && Lt
          (local.FiscalYearEnd.Date, entities.Case1.StatusDate))
        {
          local.WorkCase.Number = entities.Case1.Number;
          UseDetermineTypeOfAssistance();

          if (AsChar(entities.Case1.Status) == 'O' || AsChar
            (entities.Case1.Status) == 'C' && Lt
            (local.FiscalYearEnd.Date, entities.Case1.StatusDate))
          {
            if (AsChar(local.Current.Flag) == 'Y')
            {
              // *** Children Requiring Paternity Determination Services in 
              // Cases
              // *** Open at the End of the Fiscal Year
              // *** (line 13)
              // ***
              // *** CURRENT ASSISTANCE
              ++local.Local13Current.Count;

              goto Read;
            }

            if (AsChar(local.Former.Flag) == 'Y')
            {
              // *** Children Requiring Paternity Determination Services in 
              // Cases
              // *** Open at the End of the Fiscal Year
              // *** (line 13)
              // ***
              // *** FORMER ASSISTANCE
              ++local.Local13Former.Count;

              goto Read;
            }

            if (AsChar(local.Never.Flag) == 'Y')
            {
              // *** Children Requiring Paternity Determination Services in 
              // Cases
              // *** Open at the End of the Fiscal Year
              // *** (line 13)
              // ***
              // *** NEVER ASSISTANCE
              ++local.Local13Never.Count;
            }
          }
        }
      }
      else
      {
        ExitState = "CASE_NF";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Case not found for Case Role Identifier " + NumberToString
          (entities.CaseRole.Identifier, 15) + "Case Role Type " + entities
          .CaseRole.Type1;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

Read:

      local.PrevCsePerson.Number = entities.CsePerson.Number;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case
    //  ***
    //   ****
    // ==> line 12 <==
    foreach(var item in ReadCase4())
    {
      local.WorkCase.Number = entities.Case1.Number;
      UseDetermineTypeOfAssistance();
      local.ApCommon.Count = 0;

      // ***
      // *** get each "AP" Case Role for current Case
      // ***
      foreach(var item1 in ReadCaseRole1())
      {
        // *** count all "AP"s for the CASE that have not been End Dated
        ++local.ApCommon.Count;
      }

      if (local.ApCommon.Count == 0 || local.ApCommon.Count > 1)
      {
        // *** No "AP" on CASE or more than one "AP" not End Dated on CASE
        if (AsChar(local.Current.Flag) == 'Y')
        {
          ++local.Local12Current.Count;
        }

        if (AsChar(local.Former.Flag) == 'Y')
        {
          ++local.Local12Former.Count;
        }

        if (AsChar(local.Never.Flag) == 'Y')
        {
          ++local.Local12Never.Count;
        }

        continue;
      }

      local.Waived.Count = 0;
      local.Kids.Count = 0;

      // ***
      // *** get each "CH" Case Role for current Case
      // ***
      foreach(var item1 in ReadCaseRole4())
      {
        ++local.Kids.Count;

        if (AsChar(entities.CaseRole.ArWaivedInsurance) == 'Y')
        {
          ++local.Waived.Count;
        }
      }

      if (local.Kids.Count == local.Waived.Count)
      {
        continue;
      }

      // ***
      // *** get each "CH" Case Role for current Case,
      // *** where the AR_WAIVED_INSURANCE not = "Y"
      // ***
      foreach(var item1 in ReadCaseRole2())
      {
        // ***
        // *** get each Legal Action Case Role for current Case Role
        // ***
        foreach(var item2 in ReadLegalActionCaseRole1())
        {
          // ***
          // *** get Legal Action for current Legal Action Case Role
          // ***
          if (ReadLegalAction1())
          {
            // ***
            // *** get each Legal Action Detail for current Legal Action
            // ***
            foreach(var item3 in ReadLegalActionDetail3())
            {
              if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC"))
              {
                goto ReadEach6;
              }
            }

            if (AsChar(local.Current.Flag) == 'Y')
            {
              ++local.Local12Current.Count;
            }

            if (AsChar(local.Former.Flag) == 'Y')
            {
              ++local.Local12Former.Count;
            }

            if (AsChar(local.Never.Flag) == 'Y')
            {
              ++local.Local12Never.Count;
            }

            goto ReadEach7;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";

            // *** extract Year from timestamp
            local.DateWorkAttributes.NumericalYear =
              Year(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Month from timestamp
            local.DateWorkAttributes.NumericalMonth =
              Month(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Day from timestamp
            local.DateWorkAttributes.NumericalDay =
              Day(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build date
            local.DateWorkAttributes.TextDate10Char =
              NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalMonth, 15) + "-"
              + NumberToString(local.DateWorkAttributes.NumericalDay, 15);

            // *** extract Hours from timestamp
            local.TimeWorkAttributes.NumericalHours =
              Hour(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Minutes from timestamp
            local.TimeWorkAttributes.NumericalMinutes =
              Minute(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Seconds from timestamp
            local.TimeWorkAttributes.NumericalSeconds =
              Second(entities.LegalActionCaseRole.CreatedTstamp);

            // *** extract Microseconds from timestamp
            local.TimeWorkAttributes.NumericalMicroseconds =
              Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

            // *** build time
            local.TimeWorkAttributes.TextTime15Char =
              NumberToString(local.TimeWorkAttributes.NumericalHours, 15) + "."
              + NumberToString
              (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
              (local.TimeWorkAttributes.NumericalMicroseconds, 15);

            // *** build message
            local.NeededToWrite.RptDetail =
              "Legal Action not found for Legal Action case Role with timestamp " +
              local.DateWorkAttributes.TextDate10Char + "-" + local
              .TimeWorkAttributes.TextTime15Char;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach7;
          }
        }

ReadEach6:
        ;
      }

ReadEach7:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case (CLOSED)
    //  ***
    //   ****
    // ==> line 14 <==
    foreach(var item in ReadCase3())
    {
      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRoleCsePerson3())
      {
        foreach(var item2 in ReadPersonProgramProgram5())
        {
          // ***
          // *** get each Collection, Obligation Type (CODE = "CS"),
          // *** Obligation Transaction (TYPE = "DE") and
          // *** CSE Person Account (TYPE = "S") combination for
          // *** current CSE Person
          // ***
          foreach(var item3 in ReadCollectionObligationTypeObligationTransaction())
            
          {
            // *** is the Collection (Collection_Dt 'YYYYMM') =
            // *** Case (Status_Date 'YYYYMM')??
            if (Month(entities.Collection.CollectionDt) == Month
              (entities.Case1.StatusDate) && Year
              (entities.Collection.CollectionDt) == Year
              (entities.Case1.StatusDate))
            {
              // *** Title IV-A Cases Closed During the Fiscal Year Where a
              // *** Child Support Payment Was Received
              // *** (line 14)
              // ***
              // *** TOTAL
              ++local.Local14Total.Count;

              goto ReadEach8;
            }
          }
        }
      }

ReadEach8:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //   ***
    //  *** get each Case (OPEN) and STATUS_DATE < fiscal year END DATE
    // ***                      OR
    //  *** Case (CLOSED) and STATUS_DATE in fiscal year
    //   ***
    //    ****
    // ==> line 17 <==
    foreach(var item in ReadCase5())
    {
      // *** is the Case CLOSED and STATUS DATE prior to reporting period??
      if (AsChar(entities.Case1.Status) == 'C' && (
        Lt(entities.Case1.StatusDate, local.FiscalYearStart.Date) || Lt
        (local.FiscalYearEnd.Date, entities.Case1.StatusDate)))
      {
        // *** discard
        continue;
      }

      if (AsChar(entities.Case1.Status) == 'O')
      {
        // ***
        // *** get each Case Role (CHILD) for current Case
        // ***
        foreach(var item1 in ReadCaseRoleCsePerson2())
        {
          // ***
          // *** get each Person Program/Program combination for current
          // *** CSE Person
          // ***
          foreach(var item2 in ReadPersonProgramProgram10())
          {
            // *** is Program CODE 'AF', 'AFI', 'FC' or 'FCI'??
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "AFI") || Equal
              (entities.Program.Code, "FC") || Equal
              (entities.Program.Code, "FCI"))
            {
              // ***
              // *** get each Case Role (CHILD) for current Case
              // ***
              foreach(var item3 in ReadCaseRole3())
              {
                // ***
                // *** get each Legal Action Case Role for current Case Role
                // ***
                foreach(var item4 in ReadLegalActionCaseRole1())
                {
                  // ***
                  // *** get Legal Action for current Legal Action Case Role
                  // ***
                  if (ReadLegalAction2())
                  {
                    if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") &&
                      !Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "JEF") && !
                      Equal(entities.LegalAction.ActionTaken, "JENF") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
                      Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
                      Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
                      Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
                      Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
                      Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
                    {
                      continue;
                    }
                  }
                  else
                  {
                    ExitState = "LEGAL_ACTION_NF";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";

                    // *** extract Year from timestamp
                    local.DateWorkAttributes.NumericalYear =
                      Year(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Month from timestamp
                    local.DateWorkAttributes.NumericalMonth =
                      Month(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Day from timestamp
                    local.DateWorkAttributes.NumericalDay =
                      Day(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build date
                    local.DateWorkAttributes.TextDate10Char =
                      NumberToString(local.DateWorkAttributes.NumericalYear, 15) +
                      "-" + NumberToString
                      (local.DateWorkAttributes.NumericalMonth, 15) + "-" + NumberToString
                      (local.DateWorkAttributes.NumericalDay, 15);

                    // *** extract Hours from timestamp
                    local.TimeWorkAttributes.NumericalHours =
                      Hour(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Minutes from timestamp
                    local.TimeWorkAttributes.NumericalMinutes =
                      Minute(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Seconds from timestamp
                    local.TimeWorkAttributes.NumericalSeconds =
                      Second(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Microseconds from timestamp
                    local.TimeWorkAttributes.NumericalMicroseconds =
                      Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build time
                    local.TimeWorkAttributes.TextTime15Char =
                      NumberToString(local.TimeWorkAttributes.NumericalHours, 15)
                      + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMicroseconds, 15);

                    // *** build message
                    local.NeededToWrite.RptDetail =
                      "Legal Action not found for Legal Action case Role with timestamp " +
                      local.DateWorkAttributes.TextDate10Char + "-" + local
                      .TimeWorkAttributes.TextTime15Char;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    goto ReadEach9;
                  }

                  // ***
                  // *** get each Legal Action Detail for current Legal Action
                  // ***
                  foreach(var item5 in ReadLegalActionDetail1())
                  {
                    if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
                    {
                      // ***
                      // *** get Obligation Type for current Legal Action Detail
                      // ***
                      if (ReadObligationType2())
                      {
                        if (Equal(entities.ObligationType.Code, "AJ") || Equal
                          (entities.ObligationType.Code, "CRCH") || Equal
                          (entities.ObligationType.Code, "CS") || Equal
                          (entities.ObligationType.Code, "MC") || Equal
                          (entities.ObligationType.Code, "MS") || Equal
                          (entities.ObligationType.Code, "SAJ") || Equal
                          (entities.ObligationType.Code, "SP"))
                        {
                          // *** Cases with Orders Established During the Fisacl
                          // Year
                          // *** (line 17)
                          // ***
                          // *** CURRENT ASSISTANCE
                          ++local.Local17Current.Count;

                          goto ReadEach9;
                        }
                      }
                      else
                      {
                        ExitState = "OBLIGATION_TYPE_NF";

                        // ***
                        // *** WRITE to the Error Report
                        // ***
                        local.EabFileHandling.Action = "WRITE";
                        local.NeededToWrite.RptDetail =
                          "Obligation Type not found for Legal Action Detail " +
                          NumberToString
                          (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                          NumberToString
                          (entities.LegalAction.Identifier, 15) + NumberToString
                          (entities.LegalActionDetail.Number, 15);
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        goto ReadEach9;
                      }
                    }
                    else if (Equal(entities.LegalActionDetail.NonFinOblgType,
                      "HIC") || Equal
                      (entities.LegalActionDetail.NonFinOblgType, "UM"))
                    {
                      // *** Cases with Orders Established During the Fisacl 
                      // Year
                      // *** (line 17)
                      // ***
                      // *** CURRENT ASSISTANCE
                      ++local.Local17Current.Count;

                      goto ReadEach9;
                    }
                  }
                }
              }

              goto ReadEach9;
            }
          }

          // ***
          // *** get each Person Program for current CSE Person, where
          // *** Program CODE is 'AF', 'AFI', 'FC' or 'FCI'
          // ***
          foreach(var item2 in ReadPersonProgramProgram6())
          {
            // *** is Program CODE 'AF', 'AFI', 'FC' or 'FCI'??
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "AFI") || Equal
              (entities.Program.Code, "FC") || Equal
              (entities.Program.Code, "FCI"))
            {
              foreach(var item3 in ReadCaseRole3())
              {
                // ***
                // *** get each Legal Action Case Role for current Case Role
                // ***
                foreach(var item4 in ReadLegalActionCaseRole1())
                {
                  // ***
                  // *** get Legal Action for current Legal Action Case Role
                  // ***
                  if (ReadLegalAction2())
                  {
                    if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") &&
                      !Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "JEF") && !
                      Equal(entities.LegalAction.ActionTaken, "JENF") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
                      Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
                      Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
                      Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
                      Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
                      Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
                    {
                      continue;
                    }
                  }
                  else
                  {
                    ExitState = "LEGAL_ACTION_NF";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";

                    // *** extract Year from timestamp
                    local.DateWorkAttributes.NumericalYear =
                      Year(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Month from timestamp
                    local.DateWorkAttributes.NumericalMonth =
                      Month(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Day from timestamp
                    local.DateWorkAttributes.NumericalDay =
                      Day(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build date
                    local.DateWorkAttributes.TextDate10Char =
                      NumberToString(local.DateWorkAttributes.NumericalYear, 15) +
                      "-" + NumberToString
                      (local.DateWorkAttributes.NumericalMonth, 15) + "-" + NumberToString
                      (local.DateWorkAttributes.NumericalDay, 15);

                    // *** extract Hours from timestamp
                    local.TimeWorkAttributes.NumericalHours =
                      Hour(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Minutes from timestamp
                    local.TimeWorkAttributes.NumericalMinutes =
                      Minute(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Seconds from timestamp
                    local.TimeWorkAttributes.NumericalSeconds =
                      Second(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Microseconds from timestamp
                    local.TimeWorkAttributes.NumericalMicroseconds =
                      Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build time
                    local.TimeWorkAttributes.TextTime15Char =
                      NumberToString(local.TimeWorkAttributes.NumericalHours, 15)
                      + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMicroseconds, 15);

                    // *** build message
                    local.NeededToWrite.RptDetail =
                      "Legal Action not found for Legal Action case Role with timestamp " +
                      local.DateWorkAttributes.TextDate10Char + "-" + local
                      .TimeWorkAttributes.TextTime15Char;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    goto ReadEach9;
                  }

                  // ***
                  // *** get Legal Action Detail for current Legal Action
                  // ***
                  foreach(var item5 in ReadLegalActionDetail2())
                  {
                    if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
                    {
                      // ***
                      // *** get Obligation Type for current Legal Action Detail
                      // ***
                      if (ReadObligationType2())
                      {
                        if (Equal(entities.ObligationType.Code, "AJ") || Equal
                          (entities.ObligationType.Code, "CRCH") || Equal
                          (entities.ObligationType.Code, "CS") || Equal
                          (entities.ObligationType.Code, "MC") || Equal
                          (entities.ObligationType.Code, "MS") || Equal
                          (entities.ObligationType.Code, "SAJ") || Equal
                          (entities.ObligationType.Code, "SP"))
                        {
                          // *** Cases with Orders Established During the Fisacl
                          // Year
                          // *** (line 17)
                          // ***
                          // *** FORMER ASSISTANCE
                          ++local.Local17Former.Count;

                          goto ReadEach9;
                        }
                      }
                      else
                      {
                        ExitState = "OBLIGATION_TYPE_NF";

                        // ***
                        // *** WRITE to the Error Report
                        // ***
                        local.EabFileHandling.Action = "WRITE";
                        local.NeededToWrite.RptDetail =
                          "Obligation Type not found for Legal Action Detail " +
                          NumberToString
                          (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                          NumberToString
                          (entities.LegalAction.Identifier, 15) + NumberToString
                          (entities.LegalActionDetail.Number, 15);
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        goto ReadEach9;
                      }
                    }
                    else if (Equal(entities.LegalActionDetail.NonFinOblgType,
                      "HIC") || Equal
                      (entities.LegalActionDetail.NonFinOblgType, "UM"))
                    {
                      // *** Cases with Orders Established During the Fisacl 
                      // Year
                      // *** (line 17)
                      // ***
                      // *** FORMER ASSISTANCE
                      ++local.Local17Former.Count;

                      goto ReadEach9;
                    }
                  }
                }
              }

              goto ReadEach9;
            }
          }

          foreach(var item2 in ReadCaseRole3())
          {
            // ***
            // *** get each Legal Action Case Role for current Case Role
            // ***
            foreach(var item3 in ReadLegalActionCaseRole1())
            {
              // ***
              // *** get Legal Action for current Legal Action Case Role
              // ***
              if (ReadLegalAction2())
              {
                if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") && !
                  Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "JEF") && !
                  Equal(entities.LegalAction.ActionTaken, "JENF") && !
                  Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
                  Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
                  Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
                  Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
                  Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
                  Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
                  Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
                  Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
                {
                  continue;
                }
              }
              else
              {
                ExitState = "LEGAL_ACTION_NF";

                // ***
                // *** WRITE to the Error Report
                // ***
                local.EabFileHandling.Action = "WRITE";

                // *** extract Year from timestamp
                local.DateWorkAttributes.NumericalYear =
                  Year(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Month from timestamp
                local.DateWorkAttributes.NumericalMonth =
                  Month(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Day from timestamp
                local.DateWorkAttributes.NumericalDay =
                  Day(entities.LegalActionCaseRole.CreatedTstamp);

                // *** build date
                local.DateWorkAttributes.TextDate10Char =
                  NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
                  + NumberToString
                  (local.DateWorkAttributes.NumericalMonth, 15) + "-" + NumberToString
                  (local.DateWorkAttributes.NumericalDay, 15);

                // *** extract Hours from timestamp
                local.TimeWorkAttributes.NumericalHours =
                  Hour(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Minutes from timestamp
                local.TimeWorkAttributes.NumericalMinutes =
                  Minute(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Seconds from timestamp
                local.TimeWorkAttributes.NumericalSeconds =
                  Second(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Microseconds from timestamp
                local.TimeWorkAttributes.NumericalMicroseconds =
                  Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

                // *** build time
                local.TimeWorkAttributes.TextTime15Char =
                  NumberToString(local.TimeWorkAttributes.NumericalHours, 15) +
                  "." + NumberToString
                  (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
                  (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
                  (local.TimeWorkAttributes.NumericalMicroseconds, 15);

                // *** build message
                local.NeededToWrite.RptDetail =
                  "Legal Action not found for Legal Action case Role with timestamp " +
                  local.DateWorkAttributes.TextDate10Char + "-" + local
                  .TimeWorkAttributes.TextTime15Char;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                goto ReadEach9;
              }

              // ***
              // *** get each Legal Action Detail for current Legal Action
              // ***
              foreach(var item4 in ReadLegalActionDetail1())
              {
                if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
                {
                  // ***
                  // *** get Obligation Type for current Legal Action Detail
                  // ***
                  if (ReadObligationType2())
                  {
                    if (Equal(entities.ObligationType.Code, "AJ") || Equal
                      (entities.ObligationType.Code, "CRCH") || Equal
                      (entities.ObligationType.Code, "CS") || Equal
                      (entities.ObligationType.Code, "MC") || Equal
                      (entities.ObligationType.Code, "MS") || Equal
                      (entities.ObligationType.Code, "SAJ") || Equal
                      (entities.ObligationType.Code, "SP"))
                    {
                      // *** Cases with Orders Established During the Fisacl 
                      // Year
                      // *** (line 17)
                      // ***
                      // *** NEVER ASSISTANCE
                      ++local.Local17Never.Count;

                      goto ReadEach9;
                    }
                  }
                  else
                  {
                    ExitState = "OBLIGATION_TYPE_NF";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Obligation Type not found for Legal Action Detail " + NumberToString
                      (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                      NumberToString(entities.LegalAction.Identifier, 15) + NumberToString
                      (entities.LegalActionDetail.Number, 15);
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    goto ReadEach9;
                  }
                }
                else if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") ||
                  Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
                {
                  // *** Cases with Orders Established During the Fisacl Year
                  // *** (line 17)
                  // ***
                  // *** NEVER ASSISTANCE
                  ++local.Local17Never.Count;

                  goto ReadEach9;
                }
              }
            }
          }

          goto ReadEach9;
        }
      }

      if (AsChar(entities.Case1.Status) == 'C')
      {
        // ***
        // *** get each Case Role (CHILD) for current Case
        // ***
        foreach(var item1 in ReadCaseRoleCsePerson3())
        {
          // ***
          // *** get each Person Program for current CSE Person
          // ***
          foreach(var item2 in ReadPersonProgramProgram12())
          {
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "AFI") || Equal
              (entities.Program.Code, "FC") || Equal
              (entities.Program.Code, "FCI"))
            {
              // ***
              // *** get each Case Role (CHILD) for current Case
              // ***
              foreach(var item3 in ReadCaseRole3())
              {
                // ***
                // *** get each Legal Action Case Role for current Case Role
                // ***
                foreach(var item4 in ReadLegalActionCaseRole1())
                {
                  // ***
                  // *** get Legal Action for current Legal Action Case Role
                  // ***
                  if (ReadLegalAction2())
                  {
                    if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") &&
                      !Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "JEF") && !
                      Equal(entities.LegalAction.ActionTaken, "JENF") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
                      Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
                      Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
                      Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
                      Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
                      Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
                      Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
                      Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
                      Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
                    {
                      continue;
                    }
                  }
                  else
                  {
                    ExitState = "LEGAL_ACTION_NF";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";

                    // *** extract Year from timestamp
                    local.DateWorkAttributes.NumericalYear =
                      Year(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Month from timestamp
                    local.DateWorkAttributes.NumericalMonth =
                      Month(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Day from timestamp
                    local.DateWorkAttributes.NumericalDay =
                      Day(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build date
                    local.DateWorkAttributes.TextDate10Char =
                      NumberToString(local.DateWorkAttributes.NumericalYear, 15) +
                      "-" + NumberToString
                      (local.DateWorkAttributes.NumericalMonth, 15) + "-" + NumberToString
                      (local.DateWorkAttributes.NumericalDay, 15);

                    // *** extract Hours from timestamp
                    local.TimeWorkAttributes.NumericalHours =
                      Hour(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Minutes from timestamp
                    local.TimeWorkAttributes.NumericalMinutes =
                      Minute(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Seconds from timestamp
                    local.TimeWorkAttributes.NumericalSeconds =
                      Second(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** extract Microseconds from timestamp
                    local.TimeWorkAttributes.NumericalMicroseconds =
                      Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

                    // *** build time
                    local.TimeWorkAttributes.TextTime15Char =
                      NumberToString(local.TimeWorkAttributes.NumericalHours, 15)
                      + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
                      (local.TimeWorkAttributes.NumericalMicroseconds, 15);

                    // *** build message
                    local.NeededToWrite.RptDetail =
                      "Legal Action not found for Legal Action case Role with timestamp " +
                      local.DateWorkAttributes.TextDate10Char + "-" + local
                      .TimeWorkAttributes.TextTime15Char;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    goto ReadEach9;
                  }

                  // ***
                  // *** get Legal Action Detail for current Legal Action
                  // ***
                  foreach(var item5 in ReadLegalActionDetail2())
                  {
                    if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
                    {
                      // ***
                      // *** get Obligation Type for current Legal Action Detail
                      // ***
                      if (ReadObligationType2())
                      {
                        if (Equal(entities.ObligationType.Code, "AJ") || Equal
                          (entities.ObligationType.Code, "CRCH") || Equal
                          (entities.ObligationType.Code, "CS") || Equal
                          (entities.ObligationType.Code, "MC") || Equal
                          (entities.ObligationType.Code, "MS") || Equal
                          (entities.ObligationType.Code, "SAJ") || Equal
                          (entities.ObligationType.Code, "SP"))
                        {
                          // *** Cases with Orders Established During the Fisacl
                          // Year
                          // *** (line 17)
                          // ***
                          // *** FORMER ASSISTANCE
                          ++local.Local17Former.Count;

                          goto ReadEach9;
                        }
                      }
                      else
                      {
                        ExitState = "OBLIGATION_TYPE_NF";

                        // ***
                        // *** WRITE to the Error Report
                        // ***
                        local.EabFileHandling.Action = "WRITE";
                        local.NeededToWrite.RptDetail =
                          "Obligation Type not found for Legal Action Detail " +
                          NumberToString
                          (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                          NumberToString
                          (entities.LegalAction.Identifier, 15) + NumberToString
                          (entities.LegalActionDetail.Number, 15);
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        goto ReadEach9;
                      }
                    }
                    else if (Equal(entities.LegalActionDetail.NonFinOblgType,
                      "HIC") || Equal
                      (entities.LegalActionDetail.NonFinOblgType, "UM"))
                    {
                      // *** Cases with Orders Established During the Fisacl 
                      // Year
                      // *** (line 17)
                      // ***
                      // *** FORMER ASSISTANCE
                      ++local.Local17Former.Count;

                      goto ReadEach9;
                    }
                  }
                }
              }

              goto ReadEach9;
            }
          }

          // ***
          // *** get each Case Role (CHILD) for current Case
          // ***
          foreach(var item2 in ReadCaseRole3())
          {
            // ***
            // *** get each Legal Action Case Role for current Case Role
            // ***
            foreach(var item3 in ReadLegalActionCaseRole1())
            {
              // ***
              // *** get Legal Action for current Legal Action Case Role
              // ***
              if (ReadLegalAction2())
              {
                if (!Equal(entities.LegalAction.ActionTaken, "DEFJPATJ") && !
                  Equal(entities.LegalAction.ActionTaken, "DFLTSUPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "INTERSTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "JEF") && !
                  Equal(entities.LegalAction.ActionTaken, "JENF") && !
                  Equal(entities.LegalAction.ActionTaken, "MEDEXPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "MEDSUPJ") && !
                  Equal(entities.LegalAction.ActionTaken, "MODSUPPO") && !
                  Equal(entities.LegalAction.ActionTaken, "PATERNJ") && !
                  Equal(entities.LegalAction.ActionTaken, "PATMEDJ") && !
                  Equal(entities.LegalAction.ActionTaken, "PATONLYJ") && !
                  Equal(entities.LegalAction.ActionTaken, "QUALMEDO") && !
                  Equal(entities.LegalAction.ActionTaken, "SUPPORTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOLPATTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOLSUPTJ") && !
                  Equal(entities.LegalAction.ActionTaken, "VOL718B") && !
                  Equal(entities.LegalAction.ActionTaken, "718BDEFJ") && !
                  Equal(entities.LegalAction.ActionTaken, "718BJERJ"))
                {
                  continue;
                }
              }
              else
              {
                ExitState = "LEGAL_ACTION_NF";

                // ***
                // *** WRITE to the Error Report
                // ***
                local.EabFileHandling.Action = "WRITE";

                // *** extract Year from timestamp
                local.DateWorkAttributes.NumericalYear =
                  Year(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Month from timestamp
                local.DateWorkAttributes.NumericalMonth =
                  Month(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Day from timestamp
                local.DateWorkAttributes.NumericalDay =
                  Day(entities.LegalActionCaseRole.CreatedTstamp);

                // *** build date
                local.DateWorkAttributes.TextDate10Char =
                  NumberToString(local.DateWorkAttributes.NumericalYear, 15) + "-"
                  + NumberToString
                  (local.DateWorkAttributes.NumericalMonth, 15) + "-" + NumberToString
                  (local.DateWorkAttributes.NumericalDay, 15);

                // *** extract Hours from timestamp
                local.TimeWorkAttributes.NumericalHours =
                  Hour(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Minutes from timestamp
                local.TimeWorkAttributes.NumericalMinutes =
                  Minute(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Seconds from timestamp
                local.TimeWorkAttributes.NumericalSeconds =
                  Second(entities.LegalActionCaseRole.CreatedTstamp);

                // *** extract Microseconds from timestamp
                local.TimeWorkAttributes.NumericalMicroseconds =
                  Microsecond(entities.LegalActionCaseRole.CreatedTstamp);

                // *** build time
                local.TimeWorkAttributes.TextTime15Char =
                  NumberToString(local.TimeWorkAttributes.NumericalHours, 15) +
                  "." + NumberToString
                  (local.TimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
                  (local.TimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
                  (local.TimeWorkAttributes.NumericalMicroseconds, 15);

                // *** build message
                local.NeededToWrite.RptDetail =
                  "Legal Action not found for Legal Action case Role with timestamp " +
                  local.DateWorkAttributes.TextDate10Char + "-" + local
                  .TimeWorkAttributes.TextTime15Char;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                goto ReadEach9;
              }

              // ***
              // *** get each Legal Action Detail for current Legal Action
              // ***
              foreach(var item4 in ReadLegalActionDetail1())
              {
                if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
                {
                  // ***
                  // *** get Obligation Type for current Legal Action Detail
                  // ***
                  if (ReadObligationType2())
                  {
                    if (Equal(entities.ObligationType.Code, "AJ") || Equal
                      (entities.ObligationType.Code, "CRCH") || Equal
                      (entities.ObligationType.Code, "CS") || Equal
                      (entities.ObligationType.Code, "MC") || Equal
                      (entities.ObligationType.Code, "MS") || Equal
                      (entities.ObligationType.Code, "SAJ") || Equal
                      (entities.ObligationType.Code, "SP"))
                    {
                      // *** Cases with Orders Established During the Fisacl 
                      // Year
                      // *** (line 17)
                      // ***
                      // *** NEVER ASSISTANCE
                      ++local.Local17Never.Count;

                      goto ReadEach9;
                    }
                  }
                  else
                  {
                    ExitState = "OBLIGATION_TYPE_NF";

                    // ***
                    // *** WRITE to the Error Report
                    // ***
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Obligation Type not found for Legal Action Detail " + NumberToString
                      (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                      NumberToString(entities.LegalAction.Identifier, 15) + NumberToString
                      (entities.LegalActionDetail.Number, 15);
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    goto ReadEach9;
                  }
                }
                else if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") ||
                  Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
                {
                  // *** Cases with Orders Established During the Fisacl Year
                  // *** (line 17)
                  // ***
                  // *** NEVER ASSISTANCE
                  ++local.Local17Never.Count;

                  goto ReadEach9;
                }
              }
            }
          }

          goto ReadEach9;
        }
      }

ReadEach9:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //  ***
    // *** get each Case
    //  ***
    //   ****
    // ==> lines 18, 18a, 19 and 20 <==
    foreach(var item in ReadCase5())
    {
      local.Discard.Flag = "N";

      switch(AsChar(entities.Case1.Status))
      {
        case 'C':
          if (!Lt(entities.Case1.StatusDate, local.FiscalYearStart.Date) && !
            Lt(local.FiscalYearEnd.Date, entities.Case1.StatusDate))
          {
            // *** keep
            break;
          }

          // *** discard
          local.Discard.Flag = "Y";

          break;
        case 'O':
          // *** keep
          break;
        default:
          // *** discard
          local.Discard.Flag = "Y";

          break;
      }

      // *** was the DISCARD flag set??
      if (AsChar(local.Discard.Flag) == 'Y')
      {
        continue;
      }

      local.InterstateFound.Flag = "N";

      // ***
      // *** get each Interstate Request for current Case
      // ***
      if (ReadInterstateRequest())
      {
        local.InterstateFound.Flag = "Y";
      }

      local.WorkCase.Number = entities.Case1.Number;
      UseDetermineTypeOfAssistance();

      if (AsChar(local.Current.Flag) == 'Y')
      {
        if (AsChar(local.InterstateFound.Flag) == 'Y')
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
          {
            // *** Cases Received from Another State During the Fiscal Year
            // *** (line 20)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local20Current.Count;

            goto Test10;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // *** Cases Sent to Another State During the Fiscal Year
            // *** (line 19)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local19Current.Count;
          }
        }

Test10:

        // ***
        // *** get each Case Role (CHILD) for current Case
        // ***
        foreach(var item1 in ReadCaseRoleCsePerson1())
        {
          // ***
          // *** get each Collection for current Case
          // ***
          foreach(var item2 in ReadCollectionCsePersonAccountObligationTransaction())
            
          {
            // *** Cases With Collections During the Fiscal Year
            // *** (line 18)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local18Current.Count;

            if (Equal(entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI"))
            {
              if (AsChar(local.InterstateFound.Flag) == 'Y')
              {
                if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                {
                  // *** Interstate Cases Initiated in Another State With 
                  // Collections
                  // *** During the Fiscal Year
                  // *** (line 18a)
                  // ***
                  // *** CURRENT ASSISTANCE
                  ++local.Local18ACurrent.Count;
                }
              }
            }

            local.Current.Flag = "N";

            goto ReadEach10;
          }
        }
      }

      local.Former.Flag = "N";
      local.Never.Flag = "N";

      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRoleCsePerson3())
      {
        if (AsChar(entities.Case1.Status) == 'O' && !
          Equal(entities.CaseRole.EndDate, local.Max.Date))
        {
          // *** discard
          continue;
        }

        local.Former.Flag = "N";

        if (AsChar(entities.Case1.Status) == 'O')
        {
          // ***
          // *** get each Person Program/Program combination for current
          // *** CSE Person
          // ***
          if (ReadPersonProgramProgram1())
          {
            local.Former.Flag = "Y";

            break;
          }
        }
        else
        {
          // ***
          // *** get each Person Program/Program combination for current
          // *** CSE Person
          // ***
          if (ReadPersonProgramProgram2())
          {
            local.Former.Flag = "Y";

            break;
          }
        }
      }

      if (AsChar(local.Former.Flag) == 'Y')
      {
        if (AsChar(local.InterstateFound.Flag) == 'Y')
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
          {
            // *** Cases Received From Another State During the Fiscal Year
            // *** (line 20)
            // ***
            // *** FORMER ASSISTANCE
            ++local.Local20Former.Count;

            goto Test11;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // *** Cases Sent to Another State During the Fiscal Year
            // *** (line 19)
            // ***
            // *** FORMER ASSISTANCE
            ++local.Local19Former.Count;
          }
        }

Test11:

        // ***
        // *** get each Case Role (CHILD) for current Case
        // ***
        foreach(var item1 in ReadCaseRoleCsePerson1())
        {
          // ***
          // *** get each Collection for current Case
          // ***
          foreach(var item2 in ReadCollectionObligationTransactionCsePersonAccount2())
            
          {
            if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI") || Equal
              (entities.Collection.ProgramAppliedTo, "NA") || Equal
              (entities.Collection.ProgramAppliedTo, "NAI"))
            {
              // *** Cases With Collections During the Fiscal Year
              // *** (line 18)
              // ***
              // *** FORMER ASSISTANCE
              ++local.Local18Former.Count;

              if (Equal(entities.Collection.ProgramAppliedTo, "AFI") || Equal
                (entities.Collection.ProgramAppliedTo, "FCI") || Equal
                (entities.Collection.ProgramAppliedTo, "NAI"))
              {
                if (AsChar(local.InterstateFound.Flag) == 'Y')
                {
                  if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
                  {
                    // *** Interstate Cases Initiated in Another State With 
                    // Collections
                    // *** During the Fiscal Year
                    // *** (line 18a)
                    // ***
                    // *** FORMER ASSISTANCE
                    ++local.Local18AFormer.Count;
                  }
                }
              }

              local.Former.Flag = "N";

              goto ReadEach10;
            }
          }
        }
      }

      if (AsChar(local.InterstateFound.Flag) == 'Y')
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
        {
          // *** Cases Received from Another State During the Fiscal Year
          // *** (line 20)
          // ***
          // *** NEVER ASSISTANCE
          ++local.Local20Never.Count;

          goto Test12;
        }

        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          // *** Cases Sent to Another State During the Fiscal Year
          // *** (line 19)
          // ***
          // *** NEVER ASSISTANCE
          ++local.Local19Never.Count;
        }
      }

Test12:

      // ***
      // *** get each Case Role (CHILD) for current Case
      // ***
      foreach(var item1 in ReadCaseRoleCsePerson1())
      {
        // ***
        // *** get each Collection for current Case
        // ***
        foreach(var item2 in ReadCollectionObligationTransactionCsePersonAccount1())
          
        {
          // *** Cases With Collections During the Fiscal Year
          // *** (line 18)
          // ***
          // *** NEVER ASSISTANCE
          ++local.Local18Never.Count;

          if (Equal(entities.Collection.ProgramAppliedTo, "NAI"))
          {
            if (AsChar(local.InterstateFound.Flag) == 'Y')
            {
              if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
              {
                // *** Interstate Cases Initiated in Another State With 
                // Collections
                // *** During the Fiscal Year
                // *** (line 18a)
                // ***
                // *** NEVER ASSISTANCE
                ++local.Local18ANever.Count;
              }
            }
          }

          goto ReadEach10;
        }
      }

ReadEach10:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    local.PrevCase.Number = "";

    // ****
    //  ***
    // *** get each Case (OPEN)/Case Role (CHILD) combination
    //  ***
    //   ****
    // ==> lines 21, 22 and 23 <==
    foreach(var item in ReadCaseCaseRole2())
    {
      if (Equal(entities.Case1.Number, local.PrevCase.Number))
      {
        continue;
      }

      local.PrevCase.Number = entities.Case1.Number;

      // *** Line 21
      // ***
      // *** get each Legal Action case Role/Legal Action/
      // *** Legal Action Detail combination for current Case Role
      // ***
      foreach(var item1 in ReadLegalActionCaseRoleLegalActionLegalActionDetail3())
        
      {
        switch(AsChar(entities.LegalActionDetail.DetailType))
        {
          case 'F':
            // ***
            // *** get each Obligation/Obligation Type combination
            // ***
            if (ReadObligationType2())
            {
              if (Equal(entities.ObligationType.Code, "MC") || Equal
                (entities.ObligationType.Code, "MJ") || Equal
                (entities.ObligationType.Code, "MS"))
              {
                // *** Cases Open at the End of the Fiscal Year
                // *** Where Medical Support is Ordered
                // *** (line 21)
                // ***
                // *** TOTAL
                ++local.Local21Total.Count;

                goto ReadEach11;
              }
            }
            else
            {
              ExitState = "OBLIGATION_TYPE_NF";

              // ***
              // *** WRITE to the Error Report
              // ***
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Obligation Type not found for Legal Action Detail " + NumberToString
                (entities.LegalActionDetail.Number, 15) + " for Legal Action " +
                NumberToString(entities.LegalAction.Identifier, 15) + NumberToString
                (entities.LegalActionDetail.Number, 15);
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              goto ReadEach13;
            }

            break;
          case 'N':
            if (Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") || Equal
              (entities.LegalActionDetail.NonFinOblgType, "UM"))
            {
              // *** Cases Open at the End of the Fiscal Year
              // *** Where Medical Support is Ordered
              // *** (line 21)
              // ***
              // *** TOTAL
              ++local.Local21Total.Count;

              goto ReadEach11;
            }

            break;
          default:
            break;
        }
      }

ReadEach11:

      // *** Line 22
      // ***
      // *** get each Legal Action case Role/Legal Action/
      // *** Legal Action Detail combination for current Case Role
      // ***
      if (ReadLegalActionCaseRoleLegalActionLegalActionDetail1())
      {
        // *** Cases Open at the End of the Fiscal Year
        // *** Where Health Insurance is Ordered
        // *** (line 22)
        // ***
        // *** TOTAL
        ++local.Local22Total.Count;
      }

      // *** Line 23
      // ***
      // *** get each Legal Action case Role/Legal Action/
      // *** Legal Action Detail combination for current Case Role
      // ***
      foreach(var item1 in ReadLegalActionCaseRoleLegalActionLegalActionDetail2())
        
      {
        // ***
        // *** get each Legal action Person for Legal Action and
        // *** current Legal Action Detail
        // ***
        foreach(var item2 in ReadLegalActionPerson())
        {
          if (!ReadCsePerson2())
          {
            ExitState = "CSE_PERSON_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "CSE Person not found for Case Role Identifier " + NumberToString
              (entities.CaseRole.Identifier, 15) + ", Case Role Type " + entities
              .CaseRole.Type1;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              goto ReadEach14;
            }

            goto ReadEach12;
          }

          foreach(var item3 in ReadHealthInsuranceCoverage())
          {
            if (Equal(entities.HealthInsuranceCoverage.PolicyExpirationDate,
              local.Max.Date))
            {
              // *** Cases Open at the End of the Fiscal Year
              // *** Where Health Insurance is Provided as Ordered
              // *** (line 23)
              // ***
              // *** TOTAL
              ++local.Local23Total.Count;

              goto ReadEach12;
            }
          }
        }

ReadEach12:
        ;
      }

ReadEach13:
      ;
    }

ReadEach14:

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    local.PrevCase.Number = "";

    // ****
    //  ***
    // *** get each Case/Case Role/CSE Person combination
    //  ***
    //   ****
    // ==> line 28 <==
    foreach(var item in ReadCaseCaseRoleCsePerson1())
    {
      // ***
      // *** get each Debt Detail/Obligation Transaction (DEBT) for
      // *** current CSE Person
      // ***
      foreach(var item1 in ReadDebtDetailObligationTransaction2())
      {
        if (entities.DebtDetail.BalanceDueAmt > 0 || Lt
          (0, entities.DebtDetail.InterestBalanceDueAmt))
        {
          // ***
          // *** get Obligation Type for current Obligation Transaction
          // ***
          if (ReadObligationTypeObligation())
          {
            // *** is it Fees or Recovery??
            if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
              (entities.ObligationType.Classification) == 'R')
            {
              // *** discard
              continue;
            }
          }
          else
          {
            ExitState = "OBLIGATION_TYPE_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Obligation Type not found for Obligation " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15);
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            goto ReadEach15;
          }

          if (Equal(entities.Case1.Number, local.PrevCase.Number))
          {
            // *** discard
            goto ReadEach15;
          }

          local.PrevCase.Number = entities.Case1.Number;

          // *** Cases with Arrears Due During the Fiscal Year
          // *** (line 28)
          // ***
          // *** TOTAL
          local.Local28Total.Count = (int)((long)local.Local28Total.Count + 1);
          local.CaseVerificationExtract.
            Assign(local.InitCaseVerificationExtract);
          local.CaseVerificationExtract.DetailsForLine = "Line 28 Total";
          local.CaseVerificationExtract.Cnumber = entities.Case1.Number;
          local.CaseVerificationExtract.CopenDate = entities.Case1.CseOpenDate;
          local.CaseVerificationExtract.Cstatus = entities.Case1.Status ?? Spaces
            (1);
          local.CaseVerificationExtract.CrIdentifier =
            entities.CaseRole.Identifier;
          local.CaseVerificationExtract.CrEndDate = entities.CaseRole.EndDate;
          local.CaseVerificationExtract.CrStartDate =
            entities.CaseRole.StartDate;
          local.CaseVerificationExtract.CrType = entities.CaseRole.Type1;
          local.CaseVerificationExtract.CpNumber = entities.CsePerson.Number;
          local.CaseVerificationExtract.CpType = entities.CsePerson.Type1;
          local.CaseVerificationExtract.DdBalanceDueAmt =
            entities.DebtDetail.BalanceDueAmt;
          local.CaseVerificationExtract.DdInterestBalanceDueAmt =
            entities.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          local.CaseVerificationExtract.DdDueDt = entities.DebtDetail.DueDt;
          local.CaseVerificationExtract.DdCreatedTmst =
            entities.DebtDetail.CreatedTmst;
          local.CaseVerificationExtract.OtCode = entities.ObligationType.Code;
          local.CaseVerificationExtract.OtClassification =
            entities.ObligationType.Classification;

          // ***
          // *** Write a record to the Case Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCaseVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Case Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }
      }

ReadEach15:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    local.PrevCase.Number = "";

    // ****
    //  ***
    // *** get each Case/Case Role/CSE Person combination
    //  ***
    //   ****
    // ==> line 29 <==
    foreach(var item in ReadCaseCaseRoleCsePerson1())
    {
      // ***
      // *** get each Collection/Obligation Transaction combination for
      // *** current CSE Person
      // ***
      foreach(var item1 in ReadCollectionObligationTransaction2())
      {
        if (Equal(entities.Case1.Number, local.PrevCase.Number))
        {
          // *** discard
          goto ReadEach16;
        }

        local.PrevCase.Number = entities.Case1.Number;

        // *** Cases Paying Toward Arrearages During Fiscal Year
        // *** (line 29)
        // ***
        // *** TOTAL
        local.Local29Total.Count = (int)((long)local.Local29Total.Count + 1);
        local.CaseVerificationExtract.Assign(local.InitCaseVerificationExtract);
        local.CaseVerificationExtract.DetailsForLine = "Line 29 Total";
        local.CaseVerificationExtract.Cnumber = entities.Case1.Number;
        local.CaseVerificationExtract.CopenDate = entities.Case1.CseOpenDate;
        local.CaseVerificationExtract.Cstatus = entities.Case1.Status ?? Spaces
          (1);
        local.CaseVerificationExtract.CrIdentifier =
          entities.CaseRole.Identifier;
        local.CaseVerificationExtract.CrEndDate = entities.CaseRole.EndDate;
        local.CaseVerificationExtract.CrStartDate = entities.CaseRole.StartDate;
        local.CaseVerificationExtract.CrType = entities.CaseRole.Type1;
        local.CaseVerificationExtract.CpNumber = entities.CsePerson.Number;
        local.CaseVerificationExtract.CpType = entities.CsePerson.Type1;
        local.CaseVerificationExtract.CollAdjustedInd =
          entities.Collection.AdjustedInd ?? Spaces(1);
        local.CaseVerificationExtract.CollAppliedToCode =
          entities.Collection.AppliedToCode;
        local.CaseVerificationExtract.CollConcurrentInd =
          entities.Collection.ConcurrentInd;
        local.CaseVerificationExtract.CollCreatedTmst =
          entities.Collection.CreatedTmst;
        local.CaseVerificationExtract.CollIdentifier =
          entities.Collection.SystemGeneratedIdentifier;
        local.CaseVerificationExtract.ObTranCreatedTmst =
          entities.Debt.CreatedTmst;
        local.CaseVerificationExtract.ObTranIdentifier =
          entities.Debt.SystemGeneratedIdentifier;
        local.CaseVerificationExtract.ObTranType = entities.Debt.Type1;

        // ***
        // *** Write a record to the Case Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCaseVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Case Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }
      }

ReadEach16:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ****
    //   ***
    //  *** get each Case/Case Role (APPLICANT RECIPIENT) combination,
    // *** where Case CSE_OPEN_DATE <= fiscal year END DATE and
    //  *** Case Role END_DATE = '2099-12-31'
    //   ***
    //    ****
    // ==> lines 38 and 39 <==
    foreach(var item in ReadCaseCaseRole1())
    {
      switch(AsChar(entities.Case1.Status))
      {
        case 'C':
          if (!Lt(entities.Case1.StatusDate, local.FiscalYearStart.Date) && !
            Lt(local.FiscalYearEnd.Date, entities.Case1.StatusDate))
          {
            // ***
            // *** get each Good Cause for current Case Role
            // ***
            if (ReadGoodCause())
            {
              // *** Cases Open During the Fiscal Year With Good Cause
              // *** Determinations
              // *** (line 39)
              // ***
              // *** CURRENT ASSISTANCE
              ++local.Local39Current.Count;
            }
          }

          break;
        case 'O':
          // ***
          // *** get each Non Cooperation for current Case Role
          // ***
          if (ReadNonCooperation())
          {
            // *** Cases at the End of the Fiscal Year in Which There is a
            // *** Determination of Noncooperation
            // *** (line 38)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local38Current.Count;
          }

          // ***
          // *** get each Good Cause for current Case Role
          // ***
          if (ReadGoodCause())
          {
            // *** Cases Open During the Fiscal Year With Good Cause
            // *** Determinations
            // *** (line 39)
            // ***
            // *** CURRENT ASSISTANCE
            ++local.Local39Current.Count;
          }

          break;
        default:
          break;
      }
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ***
    // *** CLOSE the Case Verification Extract file
    // ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabCaseVerificationExtract2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error opening extract file
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // ***
    // *** OPEN the Cash Verification Extract file
    // ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabCashVerificationExtract2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error opening extract file
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ****
    //  ***
    // *** get each Debt Detail/Obligation Transaction/Obligation Type
    // *** combination
    //  ***
    //   ****
    // ==> line 24 <==
    foreach(var item in ReadDebtDetailObligationTransactionObligationType())
    {
      local.Discard.Flag = "N";

      if (Year(entities.DebtDetail.DueDt) < Year
        (entities.DebtDetail.CreatedTmst))
      {
        // *** discard
        local.Discard.Flag = "Y";
      }

      if (Year(entities.DebtDetail.DueDt) == Year
        (entities.DebtDetail.CreatedTmst) && Month
        (entities.DebtDetail.DueDt) < Month(entities.DebtDetail.CreatedTmst))
      {
        // *** discard
        local.Discard.Flag = "Y";
      }

      // *** has the DISCARD flag been set??
      if (AsChar(local.Discard.Flag) == 'Y')
      {
        continue;
      }

      // ***
      // *** get CSE Person Account (SUPPORTED) for the
      // *** current Obligation Transaction (DEBT)
      // ***
      if (ReadCsePersonAccount())
      {
        // ***
        // *** get CSE Person for current CSE Person Account
        // ***
        if (!ReadCsePerson1())
        {
          ExitState = "CSE_PERSON_NF";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "CSE Person not found for Case Role Identifier " + NumberToString
            (entities.CaseRole.Identifier, 15) + ", Case Role Type " + entities
            .CaseRole.Type1;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          continue;
        }
      }
      else
      {
        ExitState = "CSE_PERSON_ACCOUNT_NF";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "CSE Person Account not found for Obligation Transaction " + NumberToString
          (entities.Debt.SystemGeneratedIdentifier, 15) + ", Obligation Transaction Type " +
          entities.Debt.Type1;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

      local.CashVerificationExtract.Assign(local.InitCashVerificationExtract);
      local.CashVerificationExtract.DdDueDt = entities.DebtDetail.DueDt;
      local.CashVerificationExtract.DdCreatedTmst =
        entities.DebtDetail.CreatedTmst;
      local.CashVerificationExtract.ObTranIdentifier =
        entities.Debt.SystemGeneratedIdentifier;
      local.CashVerificationExtract.ObTranType = entities.Debt.Type1;
      local.CashVerificationExtract.ObTranAmount = entities.Debt.Amount;
      local.CashVerificationExtract.ObTranCreatedBy = entities.Debt.CreatedBy;
      local.CashVerificationExtract.ObTranCreatedTmst =
        entities.Debt.CreatedTmst;
      local.CashVerificationExtract.ObTypeCode = entities.ObligationType.Code;
      local.CashVerificationExtract.ObTypeClassification =
        entities.ObligationType.Classification;
      local.CashVerificationExtract.CpaType = entities.CsePersonAccount.Type1;
      local.CashVerificationExtract.CpaCreatedTmst =
        entities.CsePersonAccount.CreatedTmst;
      local.CashVerificationExtract.CpType = entities.CsePerson.Type1;
      local.CashVerificationExtract.CpCreatedTimestamp =
        entities.CsePerson.CreatedTimestamp;
      local.CashVerificationExtract.CpNumber = entities.CsePerson.Number;

      // ***
      // *** get each Person Program/Program combination, where
      // *** Program CODE is "AF", "AFI", "FC" or "FCI"  and
      // *** meets the date criteria
      // ***
      foreach(var item1 in ReadPersonProgramProgram4())
      {
        // *** Total Amount of Current Support Due for Fiscal Year
        // *** (line 24)
        // ***
        // *** CURRENT ASSISTANCE
        local.Tally24B.TotalCurrency += entities.Debt.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 24 Current";
        local.CashVerificationExtract.PpCreatedTimestamp =
          entities.PersonProgram.CreatedTimestamp;
        local.CashVerificationExtract.PpDiscDate =
          entities.PersonProgram.DiscontinueDate;
        local.CashVerificationExtract.PpEffDate =
          entities.PersonProgram.EffectiveDate;
        local.CashVerificationExtract.Pcode = entities.Program.Code;
        local.CashVerificationExtract.PdiscDate =
          entities.Program.DiscontinueDate;
        local.CashVerificationExtract.PeffDate = entities.Program.EffectiveDate;

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }

        // ***
        // *** get each Debt Adjustment/Obligation Transaction Rln combination
        // *** for current Debt
        // ***
        foreach(var item2 in ReadObligationTransactionRlnObligationTransaction())
          
        {
          if (AsChar(entities.Debt.DebtAdjustmentType) == 'D')
          {
            // *** Total Amount of Current Support Due for Fiscal Year
            // *** (line 24)
            // ***
            // *** CURRENT ASSISTANCE
            local.Tally24B.TotalCurrency -= entities.DebtAdjustment.Amount;
          }
          else
          {
            // *** Total Amount of Current Support Due for Fiscal Year
            // *** (line 24)
            // ***
            // *** CURRENT ASSISTANCE
            local.Tally24B.TotalCurrency += entities.DebtAdjustment.Amount;
          }

          local.CashVerificationExtract.DetailsForLine = "Line 24 Current";
          local.CashVerificationExtract.ObTranType =
            entities.DebtAdjustment.Type1;
          local.CashVerificationExtract.ObTranDebtAdjustmentDt =
            entities.DebtAdjustment.DebtAdjustmentDt;
          local.CashVerificationExtract.ObTranDebtAdjustmentType =
            entities.DebtAdjustment.DebtAdjustmentType;
          local.CashVerificationExtract.ObTranAmount =
            entities.DebtAdjustment.Amount;
          local.CashVerificationExtract.ObTranIdentifier =
            entities.DebtAdjustment.SystemGeneratedIdentifier;
          local.CashVerificationExtract.ObTranCreatedBy =
            entities.DebtAdjustment.CreatedBy;
          local.CashVerificationExtract.ObTranCreatedTmst =
            entities.DebtAdjustment.CreatedTmst;

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }

        goto ReadEach17;
      }

      // ***
      // *** get each Person Program/Program combination, where
      // *** Program CODE is "AF", "AFI", "FC" or "FCI"  and
      // *** meets the date criteria
      // ***
      foreach(var item1 in ReadPersonProgramProgram9())
      {
        // *** Total Amount of Current Support Due for Fiscal Year
        // *** (line 24)
        // ***
        // *** FORMER ASSISTANCE
        local.Tally24C.TotalCurrency += entities.Debt.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 24 Former";
        local.CashVerificationExtract.PpCreatedTimestamp =
          entities.PersonProgram.CreatedTimestamp;
        local.CashVerificationExtract.PpDiscDate =
          entities.PersonProgram.DiscontinueDate;
        local.CashVerificationExtract.PpEffDate =
          entities.PersonProgram.EffectiveDate;
        local.CashVerificationExtract.Pcode = entities.Program.Code;
        local.CashVerificationExtract.PdiscDate =
          entities.Program.DiscontinueDate;
        local.CashVerificationExtract.PeffDate = entities.Program.EffectiveDate;

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }

        // ***
        // *** get each Debt Adjustment/Obligation Transaction Rln combination
        // *** for current Debt
        // ***
        foreach(var item2 in ReadObligationTransactionRlnObligationTransaction())
          
        {
          if (AsChar(entities.Debt.DebtAdjustmentType) == 'D')
          {
            // *** Total Amount of Current Support Due for Fiscal Year
            // *** (line 24)
            // ***
            // *** FORMER ASSISTANCE
            local.Tally24C.TotalCurrency -= entities.DebtAdjustment.Amount;
          }
          else
          {
            // *** Total Amount of Current Support Due for Fiscal Year
            // *** (line 24)
            // ***
            // *** FORMER ASSISTANCE
            local.Tally24C.TotalCurrency += entities.DebtAdjustment.Amount;
          }

          local.CashVerificationExtract.DetailsForLine = "Line 24 Former";
          local.CashVerificationExtract.ObTranType =
            entities.DebtAdjustment.Type1;
          local.CashVerificationExtract.ObTranDebtAdjustmentDt =
            entities.DebtAdjustment.DebtAdjustmentDt;
          local.CashVerificationExtract.ObTranDebtAdjustmentType =
            entities.DebtAdjustment.DebtAdjustmentType;
          local.CashVerificationExtract.ObTranAmount =
            entities.DebtAdjustment.Amount;
          local.CashVerificationExtract.ObTranIdentifier =
            entities.DebtAdjustment.SystemGeneratedIdentifier;
          local.CashVerificationExtract.ObTranCreatedBy =
            entities.DebtAdjustment.CreatedBy;
          local.CashVerificationExtract.ObTranCreatedTmst =
            entities.DebtAdjustment.CreatedTmst;

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }

        goto ReadEach17;
      }

      // *** Total Amount of Current Support Due for Fiscal Year
      // *** (line 24)
      // ***
      // *** NEVER ASSISTANCE
      local.Tally24D.TotalCurrency += entities.Debt.Amount;
      local.CashVerificationExtract.DetailsForLine = "Line 24 Never";

      // ***
      // *** Write a record to the Cash Verification Extract file
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      UseEabCashVerificationExtract1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error Writing to the Cash Verification Extract file";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";
        }

        return;
      }

      // ***
      // *** get each Debt Adjustment/Obligation Transaction Rln combination
      // *** for current Debt
      // ***
      foreach(var item1 in ReadObligationTransactionRlnObligationTransaction())
      {
        if (AsChar(entities.Debt.DebtAdjustmentType) == 'D')
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** NEVER ASSISTANCE
          local.Tally24D.TotalCurrency -= entities.DebtAdjustment.Amount;
        }
        else
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** NEVER ASSISTANCE
          local.Tally24D.TotalCurrency += entities.DebtAdjustment.Amount;
        }

        local.CashVerificationExtract.DetailsForLine = "Line 24 Never";
        local.CashVerificationExtract.ObTranType =
          entities.DebtAdjustment.Type1;
        local.CashVerificationExtract.ObTranDebtAdjustmentDt =
          entities.DebtAdjustment.DebtAdjustmentDt;
        local.CashVerificationExtract.ObTranDebtAdjustmentType =
          entities.DebtAdjustment.DebtAdjustmentType;
        local.CashVerificationExtract.ObTranAmount =
          entities.DebtAdjustment.Amount;
        local.CashVerificationExtract.ObTranIdentifier =
          entities.DebtAdjustment.SystemGeneratedIdentifier;
        local.CashVerificationExtract.ObTranCreatedBy =
          entities.DebtAdjustment.CreatedBy;
        local.CashVerificationExtract.ObTranCreatedTmst =
          entities.DebtAdjustment.CreatedTmst;

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }
      }

ReadEach17:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    UseFnHardcodedDebtDistribution();
    UseFnHardcodedCashReceipting();

    // ****
    //  ***
    // *** get each Collection/Cash receipt Type combination
    //  ***
    //   ****
    // ==> lines 24 and 25 <==
    foreach(var item in ReadCollectionCashReceiptType())
    {
      local.CashVerificationExtract.Assign(local.InitCashVerificationExtract);
      local.CashVerificationExtract.CollAdjustedInd =
        entities.Collection.AdjustedInd ?? Spaces(1);
      local.CashVerificationExtract.CollAmount = entities.Collection.Amount;
      local.CashVerificationExtract.CollAppliedToCode =
        entities.Collection.AppliedToCode;
      local.CashVerificationExtract.CollConcurrentInd =
        entities.Collection.ConcurrentInd;
      local.CashVerificationExtract.CollCreatedTmst =
        entities.Collection.CreatedTmst;
      local.CashVerificationExtract.CollIdentifier =
        entities.Collection.SystemGeneratedIdentifier;
      local.CashVerificationExtract.CollProgramAppliedTo =
        entities.Collection.ProgramAppliedTo;
      local.CashVerificationExtract.CrtIdentifier =
        entities.CashReceiptType.SystemGeneratedIdentifier;

      if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
        (entities.Collection.ProgramAppliedTo, "AFI") || Equal
        (entities.Collection.ProgramAppliedTo, "FC") || Equal
        (entities.Collection.ProgramAppliedTo, "FCI"))
      {
        // *** Total Amount of Support Distributed as Current Support During
        // *** the Fiscal Year
        // *** (line 25)
        // ***
        // *** CURRENT ASSISTANCE
        local.Tally25B.TotalCurrency += entities.Collection.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 25 Current";

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }

        if (AsChar(entities.Collection.AppliedToCode) == 'G')
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** CURRENT ASSISTANCE
          local.Tally24B.TotalCurrency += entities.Collection.Amount;
          local.CashVerificationExtract.DetailsForLine = "Line 24 Current";

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }

        // ***
        // *** get Obligation Type/Obligation Transaction comibination
        // *** for current Collection
        // ***
        if (ReadObligationTypeObligationTransaction())
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** CURRENT ASSISTANCE
          local.Tally24B.TotalCurrency += entities.Collection.Amount;
          local.CashVerificationExtract.DetailsForLine = "Line 24 Current";
          local.CashVerificationExtract.ObTypeCode =
            entities.ObligationType.Code;
          local.CashVerificationExtract.ObTranCreatedBy =
            entities.Debt.CreatedBy;
          local.CashVerificationExtract.ObTranCreatedTmst =
            entities.Debt.CreatedTmst;
          local.CashVerificationExtract.ObTranIdentifier =
            entities.Debt.SystemGeneratedIdentifier;
          local.CashVerificationExtract.ObTranType = entities.Debt.Type1;

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }
        else
        {
          // ***
          // *** NO voluntary Obligation Type for this Collection
          // ***
        }

        continue;
      }

      // ***
      // *** get Obligation Transaction (DEBT) for current Collection
      // ***
      if (ReadObligationTransaction())
      {
        // ***
        // *** get CSE Person Account (SUPPORTED) for
        // *** current Obligation Transaction (DEBT)
        // ***
        if (ReadCsePersonAccount())
        {
          // ***
          // *** get CSE Person for current CSE Person Account (SUPPORTED)
          // ***
          if (!ReadCsePerson1())
          {
            ExitState = "CSE_PERSON_NF";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "CSE Person not found for Case Role Identifier " + NumberToString
              (entities.CaseRole.Identifier, 15) + ", Case Role Type " + entities
              .CaseRole.Type1;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;
          }
        }
        else
        {
          ExitState = "CSE_PERSON_ACCOUNT_NF";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "CSE Person Account not found for Obligation Transaction " + NumberToString
            (entities.Debt.SystemGeneratedIdentifier, 15) + ", Obligation Transaction Type " +
            entities.Debt.Type1;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          continue;
        }
      }
      else
      {
        ExitState = "OBLIGATION_TRANSACTION_NF";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Obligation Transaction not found for Collection " + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

      local.CashVerificationExtract.ObTranCreatedTmst =
        entities.Debt.CreatedTmst;
      local.CashVerificationExtract.ObTranIdentifier =
        entities.Debt.SystemGeneratedIdentifier;
      local.CashVerificationExtract.CpaCreatedTmst =
        entities.CsePersonAccount.CreatedTmst;
      local.CashVerificationExtract.CpaType = entities.CsePersonAccount.Type1;
      local.CashVerificationExtract.CpCreatedTimestamp =
        entities.CsePerson.CreatedTimestamp;
      local.CashVerificationExtract.CpNumber = entities.CsePerson.Number;
      local.CashVerificationExtract.CpType = entities.CsePerson.Type1;

      // ***
      // *** get each Person Program/Program combination for
      // *** current CSE Person that meets the date criteria
      // ***
      foreach(var item1 in ReadPersonProgramProgram8())
      {
        if (AsChar(entities.Collection.AppliedToCode) == 'G')
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** FORMER ASSISTANCE
          local.Tally24C.TotalCurrency += entities.Collection.Amount;
          local.CashVerificationExtract.DetailsForLine = "Line 24 Former";
          local.CashVerificationExtract.PpCreatedTimestamp =
            entities.PersonProgram.CreatedTimestamp;
          local.CashVerificationExtract.PpDiscDate =
            entities.PersonProgram.DiscontinueDate;
          local.CashVerificationExtract.PpEffDate =
            entities.PersonProgram.EffectiveDate;
          local.CashVerificationExtract.Pcode = entities.Program.Code;
          local.CashVerificationExtract.PdiscDate =
            entities.Program.DiscontinueDate;
          local.CashVerificationExtract.PeffDate =
            entities.Program.EffectiveDate;

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }

        // ***
        // *** get Obligation Type for current Obligation Transaction
        // ***
        if (ReadObligationType1())
        {
          // *** Total Amount of Current Support Due for Fiscal Year
          // *** (line 24)
          // ***
          // *** FORMER ASSISTANCE
          local.Tally24C.TotalCurrency += entities.Collection.Amount;
          local.CashVerificationExtract.DetailsForLine = "Line 24 Former";
          local.CashVerificationExtract.ObTypeCode =
            entities.ObligationType.Code;

          // ***
          // *** Write a record to the Cash Verification Extract file
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          UseEabCashVerificationExtract1();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            // ***
            // *** WRITE to the Error Report
            // ***
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error Writing to the Cash Verification Extract file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ERROR_WRITING_TO_REPORT_AB";
            }

            return;
          }
        }
        else
        {
          // ***
          // *** NO voluntary Obligation Type for this Collection
          // ***
        }

        // *** Total Amount of Support Distributed as Current Support During
        // *** the Fiscal Year
        // *** (line 25)
        // ***
        // *** FORMER ASSISTANCE
        local.Tally25C.TotalCurrency += entities.Collection.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 25 Former";

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }

        goto ReadEach18;
      }

      if (AsChar(entities.Collection.AppliedToCode) == 'G')
      {
        // *** Total Amount of Current Support Due for Fiscal Year
        // *** (line 24)
        // ***
        // *** NEVER ASSISTANCE
        local.Tally24D.TotalCurrency += entities.Collection.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 24 Never";

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }
      }

      // ***
      // *** get Obligation Type for current Obligation Transaction
      // ***
      if (ReadObligationType1())
      {
        // *** Total Amount of Current Support Due for Fiscal Year
        // *** (line 24)
        // ***
        // *** NEVER ASSISTANCE
        local.Tally24D.TotalCurrency += entities.Collection.Amount;
        local.CashVerificationExtract.DetailsForLine = "Line 24 Never";
        local.CashVerificationExtract.ObTypeCode = entities.ObligationType.Code;

        // ***
        // *** Write a record to the Cash Verification Extract file
        // ***
        local.ReportParms.Parm1 = "GR";
        local.ReportParms.Parm2 = "";
        UseEabCashVerificationExtract1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error Writing to the Cash Verification Extract file";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";
          }

          return;
        }
      }
      else
      {
        // ***
        // *** NO voluntary Obligation Type for this Collection
        // ***
      }

      // *** Total Amount of Support Distributed as Current Support During
      // *** the Fiscal Year
      // *** (line 25)
      // ***
      // *** NEVER ASSISTANCE
      local.Tally25D.TotalCurrency += entities.Collection.Amount;
      local.CashVerificationExtract.DetailsForLine = "Line 25 Never";

      // ***
      // *** Write a record to the Cash Verification Extract file
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      UseEabCashVerificationExtract1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error Writing to the Cash Verification Extract file";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";
        }

        return;
      }

ReadEach18:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // *** Round amounts for line 24
    local.Local24Current.Count =
      (int)Math.Round(
        local.Tally24B.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local24Former.Count =
      (int)Math.Round(
        local.Tally24C.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local24Never.Count =
      (int)Math.Round(
        local.Tally24D.TotalCurrency, MidpointRounding.AwayFromZero);

    // *** Round amounts for line 25
    local.Local25Current.Count =
      (int)Math.Round(
        local.Tally25B.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local25Former.Count =
      (int)Math.Round(
        local.Tally25C.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local25Never.Count =
      (int)Math.Round(
        local.Tally25D.TotalCurrency, MidpointRounding.AwayFromZero);
    local.PrevCsePerson.Number = "";

    // ****
    //  ***
    // *** get each Case/Case Role/CSE Person combination
    //  ***
    //   ****
    // ==> line 26 <==
    foreach(var item in ReadCaseCaseRoleCsePerson2())
    {
      if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
      {
        // *** discard
        continue;
      }

      local.PrevCsePerson.Number = entities.CsePerson.Number;

      // ***
      // *** get each Debt Detail/Obligation Transaction (DEBT) for
      // *** current CSE Person
      // ***
      foreach(var item1 in ReadDebtDetailObligationTransaction1())
      {
        // ***
        // *** get Obligation Type for current Obligation Transaction
        // ***
        if (ReadObligationTypeObligation())
        {
          // *** is it Fees or Recovery??
          if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
            (entities.ObligationType.Classification) == 'R')
          {
            // *** discard
            continue;
          }
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Obligation Type not found for Obligation " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto ReadEach20;
        }

        // ***
        // *** get each Person Program/Program combination for
        // *** current CSE Person that meets the date criteria
        // ***
        foreach(var item2 in ReadPersonProgramProgram3())
        {
          // *** Total Amount of Arrearages Due for all Fiscal Years
          // *** (line 26)
          // ***
          // *** CURRENT ASSISTANCE
          if (entities.DebtDetail.BalanceDueAmt > 0)
          {
            local.Tally26B.TotalCurrency += entities.DebtDetail.BalanceDueAmt;
          }

          if (Lt(0, entities.DebtDetail.InterestBalanceDueAmt))
          {
            local.Tally26B.TotalCurrency += entities.DebtDetail.
              InterestBalanceDueAmt.GetValueOrDefault();
          }

          goto ReadEach19;
        }

        // ***
        // *** get each Person Program/Program combination for
        // *** current CSE Person that meets the date criteria
        // ***
        foreach(var item2 in ReadPersonProgramProgram7())
        {
          // *** Total Amount of Arrearages Due for all Fiscal Years
          // *** (line 26)
          // ***
          // *** FORMER ASSISTANCE
          if (entities.DebtDetail.BalanceDueAmt > 0)
          {
            local.Tally26C.TotalCurrency += entities.DebtDetail.BalanceDueAmt;
          }

          if (Lt(0, entities.DebtDetail.InterestBalanceDueAmt))
          {
            local.Tally26C.TotalCurrency += entities.DebtDetail.
              InterestBalanceDueAmt.GetValueOrDefault();
          }

          goto ReadEach19;
        }

        // *** Total Amount of Arrearages Due for all Fiscal Years
        // *** (line 26)
        // ***
        // *** NEVER ASSISTANCE
        if (entities.DebtDetail.BalanceDueAmt > 0)
        {
          local.Tally26D.TotalCurrency += entities.DebtDetail.BalanceDueAmt;
        }

        if (Lt(0, entities.DebtDetail.InterestBalanceDueAmt))
        {
          local.Tally26D.TotalCurrency += entities.DebtDetail.
            InterestBalanceDueAmt.GetValueOrDefault();
        }

ReadEach19:
        ;
      }

ReadEach20:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // *** Round amounts for line 26
    local.Local26Current.Count =
      (int)Math.Round(
        local.Tally26B.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local26Former.Count =
      (int)Math.Round(
        local.Tally26C.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local26Never.Count =
      (int)Math.Round(
        local.Tally26D.TotalCurrency, MidpointRounding.AwayFromZero);
    local.PrevCsePerson.Number = "";

    // ****
    //  ***
    // *** get each Case/Case Role/CSE Person combination
    //  ***
    //   ****
    // ==> line 27 <==
    foreach(var item in ReadCaseCaseRoleCsePerson2())
    {
      if (Equal(entities.CsePerson.Number, local.PrevCsePerson.Number))
      {
        // *** discard
        local.Discard.Flag = "Y";

        continue;
      }

      local.PrevCsePerson.Number = entities.CsePerson.Number;

      // ***
      // *** get each Collection/Obligation Transaction combination for
      // *** current CSE Person
      // ***
      foreach(var item1 in ReadCollectionObligationTransaction1())
      {
        // ***
        // *** get Obligation Type for current Obligation Transaction
        // ***
        if (ReadObligationTypeObligation())
        {
          if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
            (entities.ObligationType.Classification) == 'R')
          {
            // *** discard
            continue;
          }
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";

          // ***
          // *** WRITE to the Error Report
          // ***
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Obligation Type not found for Obligation " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto ReadEach22;
        }

        if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
          (entities.Collection.ProgramAppliedTo, "AFI") || Equal
          (entities.Collection.ProgramAppliedTo, "FC") || Equal
          (entities.Collection.ProgramAppliedTo, "FCI"))
        {
          // *** Total Amount of Support Distributed as Arrears During the
          // *** Fiscal Year
          // *** (line 27)
          // ***
          // *** CURRENT ASSISTANCE
          local.Tally27B.TotalCurrency += entities.Collection.Amount;

          continue;
        }

        // ***
        // *** get each Person Program/Program combination for current
        // *** CSE Person that meets rhe date criteria
        // ***
        foreach(var item2 in ReadPersonProgramProgram8())
        {
          // *** Total Amount of Support Distributed as Arrears During the
          // *** Fiscal Year
          // *** (line 27)
          // ***
          // *** FORMER ASSISTANCE
          local.Tally27C.TotalCurrency += entities.Collection.Amount;

          goto ReadEach21;
        }

        // *** Total Amount of Support Distributed as Arrears During the
        // *** Fiscal Year
        // *** (line 27)
        // ***
        // *** NEVER ASSISTANCE
        local.Tally27D.TotalCurrency += entities.Collection.Amount;

ReadEach21:
        ;
      }

ReadEach22:
      ;
    }

    // ***
    // *** perform a COMMIT
    // ***
    UseExtToDoACommit();

    if (local.Commit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // *** Round amounts for line 27
    local.Local27Current.Count =
      (int)Math.Round(
        local.Tally27B.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local27Former.Count =
      (int)Math.Round(
        local.Tally27C.TotalCurrency, MidpointRounding.AwayFromZero);
    local.Local27Never.Count =
      (int)Math.Round(
        local.Tally27D.TotalCurrency, MidpointRounding.AwayFromZero);

    //      *********  **  ** * **  **  *********
    //     *****                             *****
    //    ***
    // 
    // ***
    //   *****   Produce the OCSE-157 report   *****
    // ********   using Report Composer by  ********
    //   *****     CANAM Software Labs, Inc.   *****
    //    ***
    // 
    // ***
    //     *****                             *****
    //      *********  **  ** * **  **  *********
    // ***
    //  ***
    // *** Open OCSE-157 report
    //  ***
    //   ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabOcse157Report2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error opening report
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-157 report (page 1)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "MAIN";
    UseEabOcse157Report1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error writing to report
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-157 report (page 2)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "P2";
    UseEabOcse157Report1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error writing to report
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-157 report (page 3)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "P3";
    UseEabOcse157Report1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error writing to report
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    // ***
    // *** CLOSE the Cash Verification Extract file
    // ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabCashVerificationExtract2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error opening extract file
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    //   ***
    //  ***
    // *** Close OCSE-157 report
    //  ***
    //   ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabOcse157Report2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** Error closing report
      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabGetCurrentProgramInfo()
  {
    var useImport = new CabGetCurrentProgramInfo.Import();
    var useExport = new CabGetCurrentProgramInfo.Export();

    useImport.CaseVerificationExtract.Assign(local.CaseVerificationExtract);
    useImport.Max.Date = local.Max.Date;

    Call(CabGetCurrentProgramInfo.Execute, useImport, useExport);

    local.CaseVerificationExtract.Assign(useExport.CaseVerificationExtract);
  }

  private void UseCabGetFormerProgramInfo()
  {
    var useImport = new CabGetFormerProgramInfo.Import();
    var useExport = new CabGetFormerProgramInfo.Export();

    useImport.CaseVerificationExtract.Assign(local.CaseVerificationExtract);
    useImport.Max.Date = local.Max.Date;

    Call(CabGetFormerProgramInfo.Execute, useImport, useExport);

    local.CaseVerificationExtract.Assign(useExport.CaseVerificationExtract);
  }

  private void UseDetermineTypeOfAssistance()
  {
    var useImport = new DetermineTypeOfAssistance.Import();
    var useExport = new DetermineTypeOfAssistance.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.Case1.Number = local.WorkCase.Number;

    Call(DetermineTypeOfAssistance.Execute, useImport, useExport);

    local.NeededToWrite.RptDetail = useExport.NeededToWrite.RptDetail;
    local.ErrorFound.Flag = useExport.ErrorFound.Flag;
    local.Current.Flag = useExport.Current.Flag;
    local.Former.Flag = useExport.Former.Flag;
    local.Never.Flag = useExport.Never.Flag;
  }

  private void UseEabCaseVerificationExtract1()
  {
    var useImport = new EabCaseVerificationExtract.Import();
    var useExport = new EabCaseVerificationExtract.Export();

    useImport.CaseVerificationExtract.Assign(local.CaseVerificationExtract);
    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCaseVerificationExtract.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCaseVerificationExtract2()
  {
    var useImport = new EabCaseVerificationExtract.Import();
    var useExport = new EabCaseVerificationExtract.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCaseVerificationExtract.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCashVerificationExtract1()
  {
    var useImport = new EabCashVerificationExtract.Import();
    var useExport = new EabCashVerificationExtract.Export();

    useImport.CashVerificationExtract.Assign(local.CashVerificationExtract);
    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCashVerificationExtract.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCashVerificationExtract2()
  {
    var useImport = new EabCashVerificationExtract.Import();
    var useExport = new EabCashVerificationExtract.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCashVerificationExtract.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabOcse157Report1()
  {
    var useImport = new EabOcse157Report.Import();
    var useExport = new EabOcse157Report.Export();

    useImport.Import1Current.Count = local.Local1Current.Count;
    useImport.Import1BCurrent.Count = local.Local1BCurrent.Count;
    useImport.Import1ACurrent.Count = local.Local1ACurrent.Count;
    useImport.Import1Former.Count = local.Local1Former.Count;
    useImport.Import1BFormer.Count = local.Local1BFormer.Count;
    useImport.Import1AFormer.Count = local.Local1AFormer.Count;
    useImport.Import1Never.Count = local.Local1Never.Count;
    useImport.Import1BNever.Count = local.Local1BNever.Count;
    useImport.Import1ANever.Count = local.Local1ANever.Count;
    useImport.Import1CNever.Count = local.Local1CNever.Count;
    useImport.Import2DNever.Count = local.Local2DNever.Count;
    useImport.Import2Current.Count = local.Local2Current.Count;
    useImport.Import2BCurrent.Count = local.Local2BCurrent.Count;
    useImport.Import2ACurrent.Count = local.Local2ACurrent.Count;
    useImport.Import2Former.Count = local.Local2Former.Count;
    useImport.Import2BFormer.Count = local.Local2BFormer.Count;
    useImport.Import2AFormer.Count = local.Local2AFormer.Count;
    useImport.Import2Never.Count = local.Local2Never.Count;
    useImport.Import2BNever.Count = local.Local2BNever.Count;
    useImport.Import2ANever.Count = local.Local2ANever.Count;
    useImport.Import2CCurrent.Count = local.Local2CCurrent.Count;
    useImport.Import2CFormer.Count = local.Local2CFormer.Count;
    useImport.Import2CNever.Count = local.Local2CNever.Count;
    useImport.Import4Total.Count = local.Local4Total.Count;
    useImport.Import5Total.Count = local.Local5Total.Count;
    useImport.Import6Total.Count = local.Local6Total.Count;
    useImport.Import7Total.Count = local.Local7Total.Count;
    useImport.Import16Current.Count = local.Local16Current.Count;
    useImport.Import16Former.Count = local.Local16Former.Count;
    useImport.Import16Never.Count = local.Local16Never.Count;
    useImport.Import13Current.Count = local.Local13Current.Count;
    useImport.Import13Former.Count = local.Local13Former.Count;
    useImport.Import13Never.Count = local.Local13Never.Count;
    useImport.Import12Current.Count = local.Local12Current.Count;
    useImport.Import12Former.Count = local.Local12Former.Count;
    useImport.Import12Never.Count = local.Local12Never.Count;
    useImport.Import14Total.Count = local.Local14Total.Count;
    useImport.Import17Current.Count = local.Local17Current.Count;
    useImport.Import17Former.Count = local.Local17Former.Count;
    useImport.Import17Never.Count = local.Local17Never.Count;
    useImport.Import20Current.Count = local.Local20Current.Count;
    useImport.Import19Current.Count = local.Local19Current.Count;
    useImport.Import18Current.Count = local.Local18Current.Count;
    useImport.Import18ACurrent.Count = local.Local18ACurrent.Count;
    useImport.Import20Former.Count = local.Local20Former.Count;
    useImport.Import19Former.Count = local.Local19Former.Count;
    useImport.Import18Former.Count = local.Local18Former.Count;
    useImport.Import18AFormer.Count = local.Local18AFormer.Count;
    useImport.Import20Never.Count = local.Local20Never.Count;
    useImport.Import19Never.Count = local.Local19Never.Count;
    useImport.Import18Never.Count = local.Local18Never.Count;
    useImport.Import18ANever.Count = local.Local18ANever.Count;
    useImport.Import21Total.Count = local.Local21Total.Count;
    useImport.Import22Total.Count = local.Local22Total.Count;
    useImport.Import23Total.Count = local.Local23Total.Count;
    useImport.Import24Current.Count = local.Local24Current.Count;
    useImport.Import24Former.Count = local.Local24Former.Count;
    useImport.Import24Never.Count = local.Local24Never.Count;
    useImport.Import25Current.Count = local.Local25Current.Count;
    useImport.Import25Former.Count = local.Local25Former.Count;
    useImport.Import25Never.Count = local.Local25Never.Count;
    useImport.Import26Current.Count = local.Local26Current.Count;
    useImport.Import26Former.Count = local.Local26Former.Count;
    useImport.Import26Never.Count = local.Local26Never.Count;
    useImport.Import27Current.Count = local.Local27Current.Count;
    useImport.Import27Former.Count = local.Local27Former.Count;
    useImport.Import27Never.Count = local.Local27Never.Count;
    useImport.Import28Total.Count = local.Local28Total.Count;
    useImport.Import29Total.Count = local.Local29Total.Count;
    useImport.Import39Current.Count = local.Local39Current.Count;
    useImport.Import38Current.Count = local.Local38Current.Count;
    useImport.ReportParms.Assign(local.ReportParms);
    useImport.Import3Current.Count = local.Local3Current.Count;
    useImport.Import3Former.Count = local.Local3Former.Count;
    useImport.Import3Never.Count = local.Local3Never.Count;
    useImport.Import8Total.Count = local.Local8Total.Count;
    useImport.Import9Total.Count = local.Local9Total.Count;
    useImport.Import10Total.Count = local.Local10Total.Count;
    useImport.Import30Total.Count = local.Local30Total.Count;
    useImport.Import31Total.Count = local.Local31Total.Count;
    useImport.Import32Total.Count = local.Local32Total.Count;
    useImport.Import40Total.Count = local.Local40Total.Count;
    useImport.Import41Total.Count = local.Local41Total.Count;
    useImport.Import42Total.Count = local.Local42Total.Count;
    useExport.ReportParms.Assign(local.ReportParms);

    Call(EabOcse157Report.Execute, useImport, useExport);

    local.ReportParms.Assign(useExport.ReportParms);
  }

  private void UseEabOcse157Report2()
  {
    var useImport = new EabOcse157Report.Import();
    var useExport = new EabOcse157Report.Export();

    useImport.ReportParms.Assign(local.ReportParms);
    useExport.ReportParms.Assign(local.ReportParms);

    Call(EabOcse157Report.Execute, useImport, useExport);

    local.ReportParms.Assign(useExport.ReportParms);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Commit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Commit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedFcrtRec.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedVoluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
  }

  private bool ReadCase1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CasNumber);
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CasNumber);
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase4()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase5()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole1()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole2()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "cseOpenDate", local.FiscalYearEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson1()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson1",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 13);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 14);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 15);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 16);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson2()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson2",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 13);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 14);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 15);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 16);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.FiscalYearEnd.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.FiscalYearStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole4()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole5()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    entities.KeyOnly.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.FiscalYearEnd.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.FiscalYearStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.KeyOnly.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.KeyOnly.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.KeyOnly.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.KeyOnly.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 7);
        entities.KeyOnly.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCashReceiptType()
  {
    entities.Collection.Populated = false;
    entities.CashReceiptType.Populated = false;

    return ReadEach("ReadCollectionCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtType1",
          local.HardcodedFcrtRec.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtType2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        entities.CashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonAccountObligationTransaction()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCollectionCsePersonAccountObligationTransaction",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 19);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 19);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 20);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 20);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 21);
        entities.Debt.Amount = db.GetDecimal(reader, 22);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 23);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 24);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 25);
        entities.Debt.CreatedBy = db.GetString(reader, 26);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Debt.DebtType = db.GetString(reader, 28);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTransaction1()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadCollectionObligationTransaction1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.Debt.Amount = db.GetDecimal(reader, 19);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 20);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 21);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 22);
        entities.Debt.CreatedBy = db.GetString(reader, 23);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Debt.DebtType = db.GetString(reader, 25);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 26);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 27);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTransaction2()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadCollectionObligationTransaction2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.Debt.Amount = db.GetDecimal(reader, 19);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 20);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 21);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 22);
        entities.Debt.CreatedBy = db.GetString(reader, 23);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Debt.DebtType = db.GetString(reader, 25);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 26);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 27);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTransactionCsePersonAccount1()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCollectionObligationTransactionCsePersonAccount1",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.Debt.Amount = db.GetDecimal(reader, 19);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 20);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 21);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 22);
        entities.Debt.CreatedBy = db.GetString(reader, 23);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Debt.DebtType = db.GetString(reader, 25);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 26);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 26);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 27);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 27);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 28);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTransactionCsePersonAccount2()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCollectionObligationTransactionCsePersonAccount2",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.Debt.Amount = db.GetDecimal(reader, 19);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 20);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 21);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 22);
        entities.Debt.CreatedBy = db.GetString(reader, 23);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Debt.DebtType = db.GetString(reader, 25);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 26);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 26);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 27);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 27);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 28);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeObligationTransaction()
  {
    entities.ObligationType.Populated = false;
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCollectionObligationTypeObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.AppliedToFuture = db.GetString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.Debt.Amount = db.GetDecimal(reader, 22);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 23);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 24);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 25);
        entities.Debt.CreatedBy = db.GetString(reader, 26);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Debt.DebtType = db.GetString(reader, 28);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 29);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 29);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 30);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 30);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 31);
        entities.ObligationType.Populated = true;
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 3);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 4);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 6);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionPerson.Populated);
    entities.Supported.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.
          SetString(command, "numb", entities.LegalActionPerson.CspNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.Supported.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", entities.Debt.CpaSupType ?? "");
        db.SetString(command, "cspNumber", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 2);
        entities.CsePersonAccount.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "emancipationDt1",
          local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "emancipationDt2",
          local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 3);
        entities.CsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 4);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 6);
        entities.CaseRole.CasNumber = db.GetString(reader, 7);
        entities.CaseRole.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Identifier = db.GetInt32(reader, 9);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 12);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 13);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationTransaction1()
  {
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationTransaction1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetDate(
          command, "dueDt", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 10);
        entities.Debt.Amount = db.GetDecimal(reader, 11);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 12);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 13);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 14);
        entities.Debt.CreatedBy = db.GetString(reader, 15);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Debt.DebtType = db.GetString(reader, 17);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 18);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 19);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationTransaction2()
  {
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationTransaction2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 10);
        entities.Debt.Amount = db.GetDecimal(reader, 11);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 12);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 13);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 14);
        entities.Debt.CreatedBy = db.GetString(reader, 15);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Debt.DebtType = db.GetString(reader, 17);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 18);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 19);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationTransactionObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationTransactionObligationType",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 10);
        entities.Debt.Amount = db.GetDecimal(reader, 11);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 12);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 13);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 14);
        entities.Debt.CreatedBy = db.GetString(reader, 15);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Debt.DebtType = db.GetString(reader, 17);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 18);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 19);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetNullableDate(
          command, "effectiveDate",
          local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 0);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.GoodCause.CasNumber = db.GetString(reader, 2);
        entities.GoodCause.CspNumber = db.GetString(reader, 3);
        entities.GoodCause.CroType = db.GetString(reader, 4);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 5);
        entities.GoodCause.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.Supported.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.
          SetInt32(command, "legalActionId", entities.LegalActionCaseRole.LgaId);
          
        db.
          SetNullableDate(command, "endDt", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.
          SetInt32(command, "legalActionId", entities.LegalActionCaseRole.LgaId);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionCaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionCaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionCaseRoleLegalActionLegalActionDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionCaseRoleLegalActionLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 11);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 12);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 13);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 16);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 17);
        entities.LegalAction.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionCaseRoleLegalActionLegalActionDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionCaseRoleLegalActionLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 11);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 12);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 13);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 16);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 17);
        entities.LegalAction.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionCaseRoleLegalActionLegalActionDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionCaseRoleLegalActionLegalActionDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 11);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 12);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 13);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 16);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 17);
        entities.LegalAction.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail3()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.
          SetNullableDate(command, "endDt", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetNullableDate(
          command, "effectiveDate",
          local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 0);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.NonCooperation.CasNumber = db.GetString(reader, 2);
        entities.NonCooperation.CspNumber = db.GetString(reader, 3);
        entities.NonCooperation.CroType = db.GetString(reader, 4);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 5);
        entities.NonCooperation.Populated = true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Debt.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 6);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 7);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.Debt.CreatedBy = db.GetString(reader, 9);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Debt.DebtType = db.GetString(reader, 11);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 12);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 13);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Debt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRlnObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;
    entities.ObligationTransactionRln.Populated = false;

    return ReadEach("ReadObligationTransactionRlnObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetDate(
          command, "date1", local.FiscalYearStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.FiscalYearEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 13);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 13);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 14);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 15);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 16);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 17);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 18);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 19);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 20);
        entities.DebtAdjustment.Populated = true;
        entities.ObligationTransactionRln.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId1", entities.Debt.OtyType);
        db.SetInt32(
          command, "debtTypId2",
          local.HardcodedVoluntary.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadObligationTypeObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationTypeObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
      });
  }

  private bool ReadObligationTypeObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadObligationTypeObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
        db.SetInt32(
          command, "debtTypId",
          local.HardcodedVoluntary.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.CspNumber = db.GetString(reader, 4);
        entities.Debt.CpaType = db.GetString(reader, 5);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.Debt.Type1 = db.GetString(reader, 7);
        entities.Debt.Amount = db.GetDecimal(reader, 8);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 9);
        entities.Debt.DebtAdjustmentType = db.GetString(reader, 10);
        entities.Debt.DebtAdjustmentDt = db.GetDate(reader, 11);
        entities.Debt.CreatedBy = db.GetString(reader, 12);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Debt.DebtType = db.GetString(reader, 14);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 16);
        entities.Debt.OtyType = db.GetInt32(reader, 17);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate1",
          entities.Case1.CseOpenDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate2", local.Max.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram10()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram10",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram11()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram11",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram12()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram12",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          entities.Case1.CseOpenDate.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          entities.Case1.CseOpenDate.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.FiscalYearEnd.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram4()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DebtDetail.DueDt.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram5()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          entities.Case1.CseOpenDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          entities.Case1.StatusDate.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram6()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.KeyOnly.Number);
        db.SetNullableDate(
          command, "discontinueDate1",
          entities.Case1.CseOpenDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate2", local.Max.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram7()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram7",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.FiscalYearEnd.Date.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram8()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram8",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.Collection.CollectionDt.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram9()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram9",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DebtDetail.DueDt.GetValueOrDefault());
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
        entities.Program.EffectiveDate = db.GetDate(reader, 6);
        entities.Program.DiscontinueDate = db.GetDate(reader, 7);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", global.UserId);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of InitCashVerificationExtract.
    /// </summary>
    [JsonPropertyName("initCashVerificationExtract")]
    public CashVerificationExtract InitCashVerificationExtract
    {
      get => initCashVerificationExtract ??= new();
      set => initCashVerificationExtract = value;
    }

    /// <summary>
    /// A value of InitCaseVerificationExtract.
    /// </summary>
    [JsonPropertyName("initCaseVerificationExtract")]
    public CaseVerificationExtract InitCaseVerificationExtract
    {
      get => initCaseVerificationExtract ??= new();
      set => initCaseVerificationExtract = value;
    }

    /// <summary>
    /// A value of CashVerificationExtract.
    /// </summary>
    [JsonPropertyName("cashVerificationExtract")]
    public CashVerificationExtract CashVerificationExtract
    {
      get => cashVerificationExtract ??= new();
      set => cashVerificationExtract = value;
    }

    /// <summary>
    /// A value of CaseVerificationExtract.
    /// </summary>
    [JsonPropertyName("caseVerificationExtract")]
    public CaseVerificationExtract CaseVerificationExtract
    {
      get => caseVerificationExtract ??= new();
      set => caseVerificationExtract = value;
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
    /// A value of NotFound.
    /// </summary>
    [JsonPropertyName("notFound")]
    public Common NotFound
    {
      get => notFound ??= new();
      set => notFound = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of WorkCommon.
    /// </summary>
    [JsonPropertyName("workCommon")]
    public Common WorkCommon
    {
      get => workCommon ??= new();
      set => workCommon = value;
    }

    /// <summary>
    /// A value of FiscalYearStart.
    /// </summary>
    [JsonPropertyName("fiscalYearStart")]
    public DateWorkArea FiscalYearStart
    {
      get => fiscalYearStart ??= new();
      set => fiscalYearStart = value;
    }

    /// <summary>
    /// A value of FiscalYearEnd.
    /// </summary>
    [JsonPropertyName("fiscalYearEnd")]
    public DateWorkArea FiscalYearEnd
    {
      get => fiscalYearEnd ??= new();
      set => fiscalYearEnd = value;
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
    /// A value of InterstateFound.
    /// </summary>
    [JsonPropertyName("interstateFound")]
    public Common InterstateFound
    {
      get => interstateFound ??= new();
      set => interstateFound = value;
    }

    /// <summary>
    /// A value of WorkCase.
    /// </summary>
    [JsonPropertyName("workCase")]
    public Case1 WorkCase
    {
      get => workCase ??= new();
      set => workCase = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Former.
    /// </summary>
    [JsonPropertyName("former")]
    public Common Former
    {
      get => former ??= new();
      set => former = value;
    }

    /// <summary>
    /// A value of Never.
    /// </summary>
    [JsonPropertyName("never")]
    public Common Never
    {
      get => never ??= new();
      set => never = value;
    }

    /// <summary>
    /// A value of Local1Current.
    /// </summary>
    [JsonPropertyName("local1Current")]
    public Common Local1Current
    {
      get => local1Current ??= new();
      set => local1Current = value;
    }

    /// <summary>
    /// A value of Local1BCurrent.
    /// </summary>
    [JsonPropertyName("local1BCurrent")]
    public Common Local1BCurrent
    {
      get => local1BCurrent ??= new();
      set => local1BCurrent = value;
    }

    /// <summary>
    /// A value of Local1ACurrent.
    /// </summary>
    [JsonPropertyName("local1ACurrent")]
    public Common Local1ACurrent
    {
      get => local1ACurrent ??= new();
      set => local1ACurrent = value;
    }

    /// <summary>
    /// A value of Local1Former.
    /// </summary>
    [JsonPropertyName("local1Former")]
    public Common Local1Former
    {
      get => local1Former ??= new();
      set => local1Former = value;
    }

    /// <summary>
    /// A value of Local1BFormer.
    /// </summary>
    [JsonPropertyName("local1BFormer")]
    public Common Local1BFormer
    {
      get => local1BFormer ??= new();
      set => local1BFormer = value;
    }

    /// <summary>
    /// A value of Local1AFormer.
    /// </summary>
    [JsonPropertyName("local1AFormer")]
    public Common Local1AFormer
    {
      get => local1AFormer ??= new();
      set => local1AFormer = value;
    }

    /// <summary>
    /// A value of Local1Never.
    /// </summary>
    [JsonPropertyName("local1Never")]
    public Common Local1Never
    {
      get => local1Never ??= new();
      set => local1Never = value;
    }

    /// <summary>
    /// A value of Local1BNever.
    /// </summary>
    [JsonPropertyName("local1BNever")]
    public Common Local1BNever
    {
      get => local1BNever ??= new();
      set => local1BNever = value;
    }

    /// <summary>
    /// A value of Local1ANever.
    /// </summary>
    [JsonPropertyName("local1ANever")]
    public Common Local1ANever
    {
      get => local1ANever ??= new();
      set => local1ANever = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public External Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of Local1CNever.
    /// </summary>
    [JsonPropertyName("local1CNever")]
    public Common Local1CNever
    {
      get => local1CNever ??= new();
      set => local1CNever = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    /// <summary>
    /// A value of TimeWorkAttributes.
    /// </summary>
    [JsonPropertyName("timeWorkAttributes")]
    public TimeWorkAttributes TimeWorkAttributes
    {
      get => timeWorkAttributes ??= new();
      set => timeWorkAttributes = value;
    }

    /// <summary>
    /// A value of Local2DNever.
    /// </summary>
    [JsonPropertyName("local2DNever")]
    public Common Local2DNever
    {
      get => local2DNever ??= new();
      set => local2DNever = value;
    }

    /// <summary>
    /// A value of Local2Current.
    /// </summary>
    [JsonPropertyName("local2Current")]
    public Common Local2Current
    {
      get => local2Current ??= new();
      set => local2Current = value;
    }

    /// <summary>
    /// A value of Local2BCurrent.
    /// </summary>
    [JsonPropertyName("local2BCurrent")]
    public Common Local2BCurrent
    {
      get => local2BCurrent ??= new();
      set => local2BCurrent = value;
    }

    /// <summary>
    /// A value of Local2ACurrent.
    /// </summary>
    [JsonPropertyName("local2ACurrent")]
    public Common Local2ACurrent
    {
      get => local2ACurrent ??= new();
      set => local2ACurrent = value;
    }

    /// <summary>
    /// A value of Local2Former.
    /// </summary>
    [JsonPropertyName("local2Former")]
    public Common Local2Former
    {
      get => local2Former ??= new();
      set => local2Former = value;
    }

    /// <summary>
    /// A value of Local2BFormer.
    /// </summary>
    [JsonPropertyName("local2BFormer")]
    public Common Local2BFormer
    {
      get => local2BFormer ??= new();
      set => local2BFormer = value;
    }

    /// <summary>
    /// A value of Local2AFormer.
    /// </summary>
    [JsonPropertyName("local2AFormer")]
    public Common Local2AFormer
    {
      get => local2AFormer ??= new();
      set => local2AFormer = value;
    }

    /// <summary>
    /// A value of Local2Never.
    /// </summary>
    [JsonPropertyName("local2Never")]
    public Common Local2Never
    {
      get => local2Never ??= new();
      set => local2Never = value;
    }

    /// <summary>
    /// A value of Local2BNever.
    /// </summary>
    [JsonPropertyName("local2BNever")]
    public Common Local2BNever
    {
      get => local2BNever ??= new();
      set => local2BNever = value;
    }

    /// <summary>
    /// A value of Local2ANever.
    /// </summary>
    [JsonPropertyName("local2ANever")]
    public Common Local2ANever
    {
      get => local2ANever ??= new();
      set => local2ANever = value;
    }

    /// <summary>
    /// A value of FendDatedFound.
    /// </summary>
    [JsonPropertyName("fendDatedFound")]
    public Common FendDatedFound
    {
      get => fendDatedFound ??= new();
      set => fendDatedFound = value;
    }

    /// <summary>
    /// A value of NhicUmFound.
    /// </summary>
    [JsonPropertyName("nhicUmFound")]
    public Common NhicUmFound
    {
      get => nhicUmFound ??= new();
      set => nhicUmFound = value;
    }

    /// <summary>
    /// A value of Local2CCurrent.
    /// </summary>
    [JsonPropertyName("local2CCurrent")]
    public Common Local2CCurrent
    {
      get => local2CCurrent ??= new();
      set => local2CCurrent = value;
    }

    /// <summary>
    /// A value of Local2CFormer.
    /// </summary>
    [JsonPropertyName("local2CFormer")]
    public Common Local2CFormer
    {
      get => local2CFormer ??= new();
      set => local2CFormer = value;
    }

    /// <summary>
    /// A value of Local2CNever.
    /// </summary>
    [JsonPropertyName("local2CNever")]
    public Common Local2CNever
    {
      get => local2CNever ??= new();
      set => local2CNever = value;
    }

    /// <summary>
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
    }

    /// <summary>
    /// A value of Local4Total.
    /// </summary>
    [JsonPropertyName("local4Total")]
    public Common Local4Total
    {
      get => local4Total ??= new();
      set => local4Total = value;
    }

    /// <summary>
    /// A value of Local5Total.
    /// </summary>
    [JsonPropertyName("local5Total")]
    public Common Local5Total
    {
      get => local5Total ??= new();
      set => local5Total = value;
    }

    /// <summary>
    /// A value of Discard.
    /// </summary>
    [JsonPropertyName("discard")]
    public Common Discard
    {
      get => discard ??= new();
      set => discard = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of FaWork.
    /// </summary>
    [JsonPropertyName("faWork")]
    public Common FaWork
    {
      get => faWork ??= new();
      set => faWork = value;
    }

    /// <summary>
    /// A value of Fa.
    /// </summary>
    [JsonPropertyName("fa")]
    public CsePerson Fa
    {
      get => fa ??= new();
      set => fa = value;
    }

    /// <summary>
    /// A value of Local6Total.
    /// </summary>
    [JsonPropertyName("local6Total")]
    public Common Local6Total
    {
      get => local6Total ??= new();
      set => local6Total = value;
    }

    /// <summary>
    /// A value of Local7Total.
    /// </summary>
    [JsonPropertyName("local7Total")]
    public Common Local7Total
    {
      get => local7Total ??= new();
      set => local7Total = value;
    }

    /// <summary>
    /// A value of Local16Current.
    /// </summary>
    [JsonPropertyName("local16Current")]
    public Common Local16Current
    {
      get => local16Current ??= new();
      set => local16Current = value;
    }

    /// <summary>
    /// A value of Local16Former.
    /// </summary>
    [JsonPropertyName("local16Former")]
    public Common Local16Former
    {
      get => local16Former ??= new();
      set => local16Former = value;
    }

    /// <summary>
    /// A value of Local16Never.
    /// </summary>
    [JsonPropertyName("local16Never")]
    public Common Local16Never
    {
      get => local16Never ??= new();
      set => local16Never = value;
    }

    /// <summary>
    /// A value of Local13Current.
    /// </summary>
    [JsonPropertyName("local13Current")]
    public Common Local13Current
    {
      get => local13Current ??= new();
      set => local13Current = value;
    }

    /// <summary>
    /// A value of Local13Former.
    /// </summary>
    [JsonPropertyName("local13Former")]
    public Common Local13Former
    {
      get => local13Former ??= new();
      set => local13Former = value;
    }

    /// <summary>
    /// A value of Local13Never.
    /// </summary>
    [JsonPropertyName("local13Never")]
    public Common Local13Never
    {
      get => local13Never ??= new();
      set => local13Never = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of Local12Current.
    /// </summary>
    [JsonPropertyName("local12Current")]
    public Common Local12Current
    {
      get => local12Current ??= new();
      set => local12Current = value;
    }

    /// <summary>
    /// A value of Local12Former.
    /// </summary>
    [JsonPropertyName("local12Former")]
    public Common Local12Former
    {
      get => local12Former ??= new();
      set => local12Former = value;
    }

    /// <summary>
    /// A value of Local12Never.
    /// </summary>
    [JsonPropertyName("local12Never")]
    public Common Local12Never
    {
      get => local12Never ??= new();
      set => local12Never = value;
    }

    /// <summary>
    /// A value of Waived.
    /// </summary>
    [JsonPropertyName("waived")]
    public Common Waived
    {
      get => waived ??= new();
      set => waived = value;
    }

    /// <summary>
    /// A value of Kids.
    /// </summary>
    [JsonPropertyName("kids")]
    public Common Kids
    {
      get => kids ??= new();
      set => kids = value;
    }

    /// <summary>
    /// A value of FnotFinObType.
    /// </summary>
    [JsonPropertyName("fnotFinObType")]
    public Common FnotFinObType
    {
      get => fnotFinObType ??= new();
      set => fnotFinObType = value;
    }

    /// <summary>
    /// A value of NnotHic.
    /// </summary>
    [JsonPropertyName("nnotHic")]
    public Common NnotHic
    {
      get => nnotHic ??= new();
      set => nnotHic = value;
    }

    /// <summary>
    /// A value of Local14Total.
    /// </summary>
    [JsonPropertyName("local14Total")]
    public Common Local14Total
    {
      get => local14Total ??= new();
      set => local14Total = value;
    }

    /// <summary>
    /// A value of Local17Current.
    /// </summary>
    [JsonPropertyName("local17Current")]
    public Common Local17Current
    {
      get => local17Current ??= new();
      set => local17Current = value;
    }

    /// <summary>
    /// A value of Local17Former.
    /// </summary>
    [JsonPropertyName("local17Former")]
    public Common Local17Former
    {
      get => local17Former ??= new();
      set => local17Former = value;
    }

    /// <summary>
    /// A value of Local17Never.
    /// </summary>
    [JsonPropertyName("local17Never")]
    public Common Local17Never
    {
      get => local17Never ??= new();
      set => local17Never = value;
    }

    /// <summary>
    /// A value of Local20Current.
    /// </summary>
    [JsonPropertyName("local20Current")]
    public Common Local20Current
    {
      get => local20Current ??= new();
      set => local20Current = value;
    }

    /// <summary>
    /// A value of Local19Current.
    /// </summary>
    [JsonPropertyName("local19Current")]
    public Common Local19Current
    {
      get => local19Current ??= new();
      set => local19Current = value;
    }

    /// <summary>
    /// A value of Local18Current.
    /// </summary>
    [JsonPropertyName("local18Current")]
    public Common Local18Current
    {
      get => local18Current ??= new();
      set => local18Current = value;
    }

    /// <summary>
    /// A value of Local18ACurrent.
    /// </summary>
    [JsonPropertyName("local18ACurrent")]
    public Common Local18ACurrent
    {
      get => local18ACurrent ??= new();
      set => local18ACurrent = value;
    }

    /// <summary>
    /// A value of Local20Former.
    /// </summary>
    [JsonPropertyName("local20Former")]
    public Common Local20Former
    {
      get => local20Former ??= new();
      set => local20Former = value;
    }

    /// <summary>
    /// A value of Local19Former.
    /// </summary>
    [JsonPropertyName("local19Former")]
    public Common Local19Former
    {
      get => local19Former ??= new();
      set => local19Former = value;
    }

    /// <summary>
    /// A value of Local18Former.
    /// </summary>
    [JsonPropertyName("local18Former")]
    public Common Local18Former
    {
      get => local18Former ??= new();
      set => local18Former = value;
    }

    /// <summary>
    /// A value of Local18AFormer.
    /// </summary>
    [JsonPropertyName("local18AFormer")]
    public Common Local18AFormer
    {
      get => local18AFormer ??= new();
      set => local18AFormer = value;
    }

    /// <summary>
    /// A value of Local20Never.
    /// </summary>
    [JsonPropertyName("local20Never")]
    public Common Local20Never
    {
      get => local20Never ??= new();
      set => local20Never = value;
    }

    /// <summary>
    /// A value of Local19Never.
    /// </summary>
    [JsonPropertyName("local19Never")]
    public Common Local19Never
    {
      get => local19Never ??= new();
      set => local19Never = value;
    }

    /// <summary>
    /// A value of Local18Never.
    /// </summary>
    [JsonPropertyName("local18Never")]
    public Common Local18Never
    {
      get => local18Never ??= new();
      set => local18Never = value;
    }

    /// <summary>
    /// A value of Local18ANever.
    /// </summary>
    [JsonPropertyName("local18ANever")]
    public Common Local18ANever
    {
      get => local18ANever ??= new();
      set => local18ANever = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of Local21Total.
    /// </summary>
    [JsonPropertyName("local21Total")]
    public Common Local21Total
    {
      get => local21Total ??= new();
      set => local21Total = value;
    }

    /// <summary>
    /// A value of Local22Total.
    /// </summary>
    [JsonPropertyName("local22Total")]
    public Common Local22Total
    {
      get => local22Total ??= new();
      set => local22Total = value;
    }

    /// <summary>
    /// A value of Local23Total.
    /// </summary>
    [JsonPropertyName("local23Total")]
    public Common Local23Total
    {
      get => local23Total ??= new();
      set => local23Total = value;
    }

    /// <summary>
    /// A value of Tally24B.
    /// </summary>
    [JsonPropertyName("tally24B")]
    public Common Tally24B
    {
      get => tally24B ??= new();
      set => tally24B = value;
    }

    /// <summary>
    /// A value of Tally24C.
    /// </summary>
    [JsonPropertyName("tally24C")]
    public Common Tally24C
    {
      get => tally24C ??= new();
      set => tally24C = value;
    }

    /// <summary>
    /// A value of Tally24D.
    /// </summary>
    [JsonPropertyName("tally24D")]
    public Common Tally24D
    {
      get => tally24D ??= new();
      set => tally24D = value;
    }

    /// <summary>
    /// A value of Local24Current.
    /// </summary>
    [JsonPropertyName("local24Current")]
    public Common Local24Current
    {
      get => local24Current ??= new();
      set => local24Current = value;
    }

    /// <summary>
    /// A value of Local24Former.
    /// </summary>
    [JsonPropertyName("local24Former")]
    public Common Local24Former
    {
      get => local24Former ??= new();
      set => local24Former = value;
    }

    /// <summary>
    /// A value of Local24Never.
    /// </summary>
    [JsonPropertyName("local24Never")]
    public Common Local24Never
    {
      get => local24Never ??= new();
      set => local24Never = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFcrtRec.
    /// </summary>
    [JsonPropertyName("hardcodedFcrtRec")]
    public CashReceiptType HardcodedFcrtRec
    {
      get => hardcodedFcrtRec ??= new();
      set => hardcodedFcrtRec = value;
    }

    /// <summary>
    /// A value of Tally25B.
    /// </summary>
    [JsonPropertyName("tally25B")]
    public Common Tally25B
    {
      get => tally25B ??= new();
      set => tally25B = value;
    }

    /// <summary>
    /// A value of Tally25C.
    /// </summary>
    [JsonPropertyName("tally25C")]
    public Common Tally25C
    {
      get => tally25C ??= new();
      set => tally25C = value;
    }

    /// <summary>
    /// A value of Tally25D.
    /// </summary>
    [JsonPropertyName("tally25D")]
    public Common Tally25D
    {
      get => tally25D ??= new();
      set => tally25D = value;
    }

    /// <summary>
    /// A value of Local25Current.
    /// </summary>
    [JsonPropertyName("local25Current")]
    public Common Local25Current
    {
      get => local25Current ??= new();
      set => local25Current = value;
    }

    /// <summary>
    /// A value of Local25Former.
    /// </summary>
    [JsonPropertyName("local25Former")]
    public Common Local25Former
    {
      get => local25Former ??= new();
      set => local25Former = value;
    }

    /// <summary>
    /// A value of Local25Never.
    /// </summary>
    [JsonPropertyName("local25Never")]
    public Common Local25Never
    {
      get => local25Never ??= new();
      set => local25Never = value;
    }

    /// <summary>
    /// A value of Tally26B.
    /// </summary>
    [JsonPropertyName("tally26B")]
    public Common Tally26B
    {
      get => tally26B ??= new();
      set => tally26B = value;
    }

    /// <summary>
    /// A value of Tally26C.
    /// </summary>
    [JsonPropertyName("tally26C")]
    public Common Tally26C
    {
      get => tally26C ??= new();
      set => tally26C = value;
    }

    /// <summary>
    /// A value of Tally26D.
    /// </summary>
    [JsonPropertyName("tally26D")]
    public Common Tally26D
    {
      get => tally26D ??= new();
      set => tally26D = value;
    }

    /// <summary>
    /// A value of Local26Current.
    /// </summary>
    [JsonPropertyName("local26Current")]
    public Common Local26Current
    {
      get => local26Current ??= new();
      set => local26Current = value;
    }

    /// <summary>
    /// A value of Local26Former.
    /// </summary>
    [JsonPropertyName("local26Former")]
    public Common Local26Former
    {
      get => local26Former ??= new();
      set => local26Former = value;
    }

    /// <summary>
    /// A value of Local26Never.
    /// </summary>
    [JsonPropertyName("local26Never")]
    public Common Local26Never
    {
      get => local26Never ??= new();
      set => local26Never = value;
    }

    /// <summary>
    /// A value of Tally27B.
    /// </summary>
    [JsonPropertyName("tally27B")]
    public Common Tally27B
    {
      get => tally27B ??= new();
      set => tally27B = value;
    }

    /// <summary>
    /// A value of Tally27C.
    /// </summary>
    [JsonPropertyName("tally27C")]
    public Common Tally27C
    {
      get => tally27C ??= new();
      set => tally27C = value;
    }

    /// <summary>
    /// A value of Tally27D.
    /// </summary>
    [JsonPropertyName("tally27D")]
    public Common Tally27D
    {
      get => tally27D ??= new();
      set => tally27D = value;
    }

    /// <summary>
    /// A value of Local27Current.
    /// </summary>
    [JsonPropertyName("local27Current")]
    public Common Local27Current
    {
      get => local27Current ??= new();
      set => local27Current = value;
    }

    /// <summary>
    /// A value of Local27Former.
    /// </summary>
    [JsonPropertyName("local27Former")]
    public Common Local27Former
    {
      get => local27Former ??= new();
      set => local27Former = value;
    }

    /// <summary>
    /// A value of Local27Never.
    /// </summary>
    [JsonPropertyName("local27Never")]
    public Common Local27Never
    {
      get => local27Never ??= new();
      set => local27Never = value;
    }

    /// <summary>
    /// A value of Local28Total.
    /// </summary>
    [JsonPropertyName("local28Total")]
    public Common Local28Total
    {
      get => local28Total ??= new();
      set => local28Total = value;
    }

    /// <summary>
    /// A value of Local29Total.
    /// </summary>
    [JsonPropertyName("local29Total")]
    public Common Local29Total
    {
      get => local29Total ??= new();
      set => local29Total = value;
    }

    /// <summary>
    /// A value of Local39Current.
    /// </summary>
    [JsonPropertyName("local39Current")]
    public Common Local39Current
    {
      get => local39Current ??= new();
      set => local39Current = value;
    }

    /// <summary>
    /// A value of Local38Current.
    /// </summary>
    [JsonPropertyName("local38Current")]
    public Common Local38Current
    {
      get => local38Current ??= new();
      set => local38Current = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of Local3Current.
    /// </summary>
    [JsonPropertyName("local3Current")]
    public Common Local3Current
    {
      get => local3Current ??= new();
      set => local3Current = value;
    }

    /// <summary>
    /// A value of Local3Former.
    /// </summary>
    [JsonPropertyName("local3Former")]
    public Common Local3Former
    {
      get => local3Former ??= new();
      set => local3Former = value;
    }

    /// <summary>
    /// A value of Local3Never.
    /// </summary>
    [JsonPropertyName("local3Never")]
    public Common Local3Never
    {
      get => local3Never ??= new();
      set => local3Never = value;
    }

    /// <summary>
    /// A value of Local8Total.
    /// </summary>
    [JsonPropertyName("local8Total")]
    public Common Local8Total
    {
      get => local8Total ??= new();
      set => local8Total = value;
    }

    /// <summary>
    /// A value of Local9Total.
    /// </summary>
    [JsonPropertyName("local9Total")]
    public Common Local9Total
    {
      get => local9Total ??= new();
      set => local9Total = value;
    }

    /// <summary>
    /// A value of Local10Total.
    /// </summary>
    [JsonPropertyName("local10Total")]
    public Common Local10Total
    {
      get => local10Total ??= new();
      set => local10Total = value;
    }

    /// <summary>
    /// A value of Local30Total.
    /// </summary>
    [JsonPropertyName("local30Total")]
    public Common Local30Total
    {
      get => local30Total ??= new();
      set => local30Total = value;
    }

    /// <summary>
    /// A value of Local31Total.
    /// </summary>
    [JsonPropertyName("local31Total")]
    public Common Local31Total
    {
      get => local31Total ??= new();
      set => local31Total = value;
    }

    /// <summary>
    /// A value of Local32Total.
    /// </summary>
    [JsonPropertyName("local32Total")]
    public Common Local32Total
    {
      get => local32Total ??= new();
      set => local32Total = value;
    }

    /// <summary>
    /// A value of Local40Total.
    /// </summary>
    [JsonPropertyName("local40Total")]
    public Common Local40Total
    {
      get => local40Total ??= new();
      set => local40Total = value;
    }

    /// <summary>
    /// A value of Local41Total.
    /// </summary>
    [JsonPropertyName("local41Total")]
    public Common Local41Total
    {
      get => local41Total ??= new();
      set => local41Total = value;
    }

    /// <summary>
    /// A value of Local42Total.
    /// </summary>
    [JsonPropertyName("local42Total")]
    public Common Local42Total
    {
      get => local42Total ??= new();
      set => local42Total = value;
    }

    private DateWorkArea initialized;
    private CashVerificationExtract initCashVerificationExtract;
    private CaseVerificationExtract initCaseVerificationExtract;
    private CashVerificationExtract cashVerificationExtract;
    private CaseVerificationExtract caseVerificationExtract;
    private DateWorkArea max;
    private Common notFound;
    private DateWorkArea temp;
    private Common workCommon;
    private DateWorkArea fiscalYearStart;
    private DateWorkArea fiscalYearEnd;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private Common interstateFound;
    private Case1 workCase;
    private Common errorFound;
    private Common current;
    private Common former;
    private Common never;
    private Common local1Current;
    private Common local1BCurrent;
    private Common local1ACurrent;
    private Common local1Former;
    private Common local1BFormer;
    private Common local1AFormer;
    private Common local1Never;
    private Common local1BNever;
    private Common local1ANever;
    private External commit;
    private Common local1CNever;
    private Common found;
    private DateWorkAttributes dateWorkAttributes;
    private TimeWorkAttributes timeWorkAttributes;
    private Common local2DNever;
    private Common local2Current;
    private Common local2BCurrent;
    private Common local2ACurrent;
    private Common local2Former;
    private Common local2BFormer;
    private Common local2AFormer;
    private Common local2Never;
    private Common local2BNever;
    private Common local2ANever;
    private Common fendDatedFound;
    private Common nhicUmFound;
    private Common local2CCurrent;
    private Common local2CFormer;
    private Common local2CNever;
    private CsePerson prevCsePerson;
    private Common local4Total;
    private Common local5Total;
    private Common discard;
    private CsePerson apCsePerson;
    private Common faWork;
    private CsePerson fa;
    private Common local6Total;
    private Common local7Total;
    private Common local16Current;
    private Common local16Former;
    private Common local16Never;
    private Common local13Current;
    private Common local13Former;
    private Common local13Never;
    private Common apCommon;
    private Common local12Current;
    private Common local12Former;
    private Common local12Never;
    private Common waived;
    private Common kids;
    private Common fnotFinObType;
    private Common nnotHic;
    private Common local14Total;
    private Common local17Current;
    private Common local17Former;
    private Common local17Never;
    private Common local20Current;
    private Common local19Current;
    private Common local18Current;
    private Common local18ACurrent;
    private Common local20Former;
    private Common local19Former;
    private Common local18Former;
    private Common local18AFormer;
    private Common local20Never;
    private Common local19Never;
    private Common local18Never;
    private Common local18ANever;
    private Case1 prevCase;
    private Common local21Total;
    private Common local22Total;
    private Common local23Total;
    private Common tally24B;
    private Common tally24C;
    private Common tally24D;
    private Common local24Current;
    private Common local24Former;
    private Common local24Never;
    private ObligationType hardcodedVoluntary;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptType hardcodedFcrtRec;
    private Common tally25B;
    private Common tally25C;
    private Common tally25D;
    private Common local25Current;
    private Common local25Former;
    private Common local25Never;
    private Common tally26B;
    private Common tally26C;
    private Common tally26D;
    private Common local26Current;
    private Common local26Former;
    private Common local26Never;
    private Common tally27B;
    private Common tally27C;
    private Common tally27D;
    private Common local27Current;
    private Common local27Former;
    private Common local27Never;
    private Common local28Total;
    private Common local29Total;
    private Common local39Current;
    private Common local38Current;
    private ReportParms reportParms;
    private Common local3Current;
    private Common local3Former;
    private Common local3Never;
    private Common local8Total;
    private Common local9Total;
    private Common local10Total;
    private Common local30Total;
    private Common local31Total;
    private Common local32Total;
    private Common local40Total;
    private Common local41Total;
    private Common local42Total;
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

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private PersonProgram personProgram;
    private Program program;
    private CsePerson keyOnly;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Collection collection;
    private ObligationTransaction debt;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePerson supported;
    private LegalActionPerson legalActionPerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private DebtDetail debtDetail;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private GoodCause goodCause;
    private NonCooperation nonCooperation;
  }
#endregion
}
