// Program: FN_OCSE157_LINE_1, ID: 371092706, model: 746.
// Short name: SWE02917
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_1.
/// </summary>
[Serializable]
public partial class FnOcse157Line1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line1.
  /// </summary>
  public FnOcse157Line1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // If display ind=N, then write following info to verification table
    // - Case # for line 01
    // No additional records written for 01a, 01b and 01c
    // ----------------------------------------------------------------------
    // --------------------------------------------------------------
    // If display ind=Y, then also write the following
    // - Case assignment info
    // - Supp Person
    // - Person Program
    // Additional verification records for line 01a and 01b
    // - Interstate Request Id
    // - Kansas Case Ind
    // - No additional record for line 01c
    // ----------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // ??/??/??  KDoshi			Initial Development
    // 03/09/06  GVandy	WR00230751	Federally mandated changes.
    // 					1) Include foreign and tribal interstate cases in
    // 					   line 1a and 1b.
    // 					2) Add new sublines 1d, 1e, 1f, 1g.
    // 					3) Exclude from line 1 if state has no jurisdiction
    // 					   for the case.
    // 09/01/10  RMathews      CQ21411         Include case assignments with 
    // discontinue date equal
    // 					to report end date
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, exclude cases to tribal 
    // and
    // 					international child support agencies from Lines 1a
    // 					and 1b.
    // ---------------------------------------------------------------------------------------------------
    local.GetMedicaidOnlyProgram.Flag = "Y";
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line1Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line1Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line1Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
      local.Line1ACurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 10));
      local.Line1AFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 54, 10));
      local.Line1ANever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 64, 10));
      local.Line1BCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 74, 10));
      local.Line1BFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 84, 10));
      local.Line1BNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 94, 10));
      local.Line1CNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 104, 10));
      local.Line1DCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 114, 10));
      local.Line1DFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 124, 10));
      local.Line1DNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 134, 10));
      local.Line1ECurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 144, 10));
      local.Line1EFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 154, 10));
      local.Line1ENever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 164, 10));
      local.Line1F.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 174, 10));
      local.Line1G.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 184, 10));
    }

    foreach(var item in ReadCase())
    {
      if (Equal(entities.Case1.Number, local.Prev.Number))
      {
        continue;
      }

      local.Prev.Number = entities.Case1.Number;
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      // CQ21411  Include case assignment with discontinue date equal to report 
      // end date
      if (!ReadCaseAssignment())
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "01";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case assignment is not open.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      if (!IsEmpty(entities.Case1.NoJurisdictionCd))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "01";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-state has no jurisdiction.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      UseFn157GetAssistanceForCase();

      // ------------------------------------------
      // No exit state is set in this CAB.
      // ------------------------------------------
      switch(AsChar(local.AssistanceProgram.Flag))
      {
        case 'C':
          ++local.Line1Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line1Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line1Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      local.AssistanceBasedColumn.Column =
        local.ForCreateOcse157Verification.Column ?? "";
      local.ForCreateOcse157Verification.LineNumber = "01";
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.CaseAsinEffDte =
          entities.CaseAssignment.EffectiveDate;
        local.ForCreateOcse157Verification.CaseAsinEndDte =
          entities.CaseAssignment.DiscontinueDate;
        local.ForCreateOcse157Verification.SuppPersonNumber =
          local.SuppPersForVerification.Number;
        local.ForCreateOcse157Verification.PersonProgCode =
          local.ForVerification.Code;
      }

      UseFnCreateOcse157Verification();
      local.CountedIn1A.Flag = "N";
      local.CountedIn1B.Flag = "N";
      local.CountedIn1D.Flag = "N";
      local.CountedIn1E.Flag = "N";
      local.CountedIn1F.Flag = "N";
      local.CountedIn1G.Flag = "N";

      foreach(var item1 in ReadInterstateRequest())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          if (AsChar(local.CountedIn1A.Flag) == 'N')
          {
            if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
              .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault() && (
                !IsEmpty(entities.InterstateRequest.Country) || !
              IsEmpty(entities.InterstateRequest.TribalAgency)))
            {
              // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, exclude cases 
              // to tribal and
              // international child support agencies from Lines 1a and 1b.
              goto Test1;
            }

            local.CountedIn1A.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line1ACurr.Count;

                break;
              case 'F':
                ++local.Line1AFormer.Count;

                break;
              default:
                ++local.Line1ANever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01a";
              UseFnCreateOcse157Verification();
            }
          }

Test1:

          if (AsChar(local.CountedIn1D.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.TribalAgency))
          {
            local.CountedIn1D.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line1DCurr.Count;

                break;
              case 'F':
                ++local.Line1DFormer.Count;

                break;
              default:
                ++local.Line1DNever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01d";
              UseFnCreateOcse157Verification();
            }
          }

          if (AsChar(local.CountedIn1F.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.Country))
          {
            local.CountedIn1F.Flag = "Y";
            ++local.Line1F.Count;

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column = "a";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01f";
              UseFnCreateOcse157Verification();
            }
          }
        }
        else if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
        {
          if (AsChar(local.CountedIn1B.Flag) == 'N')
          {
            if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
              .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault() && (
                !IsEmpty(entities.InterstateRequest.Country) || !
              IsEmpty(entities.InterstateRequest.TribalAgency)))
            {
              // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, exclude cases 
              // to tribal and
              // international child support agencies from Lines 1a and 1b.
              goto Test2;
            }

            local.CountedIn1B.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line1BCurr.Count;

                break;
              case 'F':
                ++local.Line1BFormer.Count;

                break;
              default:
                ++local.Line1BNever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01b";
              UseFnCreateOcse157Verification();
            }
          }

Test2:

          if (AsChar(local.CountedIn1E.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.TribalAgency))
          {
            local.CountedIn1E.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line1ECurr.Count;

                break;
              case 'F':
                ++local.Line1EFormer.Count;

                break;
              default:
                ++local.Line1ENever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01e";
              UseFnCreateOcse157Verification();
            }
          }

          if (AsChar(local.CountedIn1G.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.Country))
          {
            local.CountedIn1G.Flag = "Y";
            ++local.Line1G.Count;

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column = "a";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "01g";
              UseFnCreateOcse157Verification();
            }
          }
        }
      }

      if (AsChar(local.AssistanceProgram.Flag) == 'M')
      {
        ++local.Line1CNever.Count;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.Column =
            local.AssistanceBasedColumn.Column ?? "";
          local.ForCreateOcse157Verification.LineNumber = "01c";
          UseFnCreateOcse157Verification();
        }
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "01 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1Never.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1ACurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1AFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1ANever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1BCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1BFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1BNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1CNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1DCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1DFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1DNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1ECurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1EFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1ENever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1F.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line1G.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "01";
          local.ForError.CaseNumber = entities.Case1.Number;
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
    // Processing complete for line 1.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "01";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line1Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line1Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1Never.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01a";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line1ACurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line1AFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1ANever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01b";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line1BCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line1BFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1BNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01c";
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1CNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01d";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line1DCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line1DFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1DNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01e";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line1ECurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line1EFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line1ENever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01f";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line1F.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "01g";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line1G.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "02 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "01";
      local.ForError.CaseNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
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

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
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

  private void UseFn157GetAssistanceForCase()
  {
    var useImport = new Fn157GetAssistanceForCase.Import();
    var useExport = new Fn157GetAssistanceForCase.Export();

    useImport.Case1.Assign(entities.Case1);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.GetMedicaidOnlyProgram.Flag = local.GetMedicaidOnlyProgram.Flag;

    Call(Fn157GetAssistanceForCase.Execute, useImport, useExport);

    local.ForVerification.Code = useExport.Program.Code;
    local.SuppPersForVerification.Number = useExport.CsePerson.Number;
    local.AssistanceProgram.Flag = useExport.AssistanceProgram.Flag;
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

    MoveOcse157Verification1(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
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
        db.SetString(command, "numb", local.Restart.Number);
        db.SetNullableString(
          command, "caseNumber1", import.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", import.To.CaseNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "othStateClsDte",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 6);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 7);
        entities.InterstateRequest.Populated = true;

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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private Ocse157Verification to;
    private Ocse157Verification from;
    private Common displayInd;
    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportEndDate;
    private Ocse157Verification cq66220EffectiveFy;
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
    /// A value of AssistanceBasedColumn.
    /// </summary>
    [JsonPropertyName("assistanceBasedColumn")]
    public Ocse157Verification AssistanceBasedColumn
    {
      get => assistanceBasedColumn ??= new();
      set => assistanceBasedColumn = value;
    }

    /// <summary>
    /// A value of CountedIn1F.
    /// </summary>
    [JsonPropertyName("countedIn1F")]
    public Common CountedIn1F
    {
      get => countedIn1F ??= new();
      set => countedIn1F = value;
    }

    /// <summary>
    /// A value of CountedIn1G.
    /// </summary>
    [JsonPropertyName("countedIn1G")]
    public Common CountedIn1G
    {
      get => countedIn1G ??= new();
      set => countedIn1G = value;
    }

    /// <summary>
    /// A value of CountedIn1D.
    /// </summary>
    [JsonPropertyName("countedIn1D")]
    public Common CountedIn1D
    {
      get => countedIn1D ??= new();
      set => countedIn1D = value;
    }

    /// <summary>
    /// A value of CountedIn1E.
    /// </summary>
    [JsonPropertyName("countedIn1E")]
    public Common CountedIn1E
    {
      get => countedIn1E ??= new();
      set => countedIn1E = value;
    }

    /// <summary>
    /// A value of CountedIn1B.
    /// </summary>
    [JsonPropertyName("countedIn1B")]
    public Common CountedIn1B
    {
      get => countedIn1B ??= new();
      set => countedIn1B = value;
    }

    /// <summary>
    /// A value of CountedIn1A.
    /// </summary>
    [JsonPropertyName("countedIn1A")]
    public Common CountedIn1A
    {
      get => countedIn1A ??= new();
      set => countedIn1A = value;
    }

    /// <summary>
    /// A value of Line1F.
    /// </summary>
    [JsonPropertyName("line1F")]
    public Common Line1F
    {
      get => line1F ??= new();
      set => line1F = value;
    }

    /// <summary>
    /// A value of Line1G.
    /// </summary>
    [JsonPropertyName("line1G")]
    public Common Line1G
    {
      get => line1G ??= new();
      set => line1G = value;
    }

    /// <summary>
    /// A value of Line1EFormer.
    /// </summary>
    [JsonPropertyName("line1EFormer")]
    public Common Line1EFormer
    {
      get => line1EFormer ??= new();
      set => line1EFormer = value;
    }

    /// <summary>
    /// A value of Line1ECurr.
    /// </summary>
    [JsonPropertyName("line1ECurr")]
    public Common Line1ECurr
    {
      get => line1ECurr ??= new();
      set => line1ECurr = value;
    }

    /// <summary>
    /// A value of Line1ENever.
    /// </summary>
    [JsonPropertyName("line1ENever")]
    public Common Line1ENever
    {
      get => line1ENever ??= new();
      set => line1ENever = value;
    }

    /// <summary>
    /// A value of Line1DFormer.
    /// </summary>
    [JsonPropertyName("line1DFormer")]
    public Common Line1DFormer
    {
      get => line1DFormer ??= new();
      set => line1DFormer = value;
    }

    /// <summary>
    /// A value of Line1DCurr.
    /// </summary>
    [JsonPropertyName("line1DCurr")]
    public Common Line1DCurr
    {
      get => line1DCurr ??= new();
      set => line1DCurr = value;
    }

    /// <summary>
    /// A value of Line1DNever.
    /// </summary>
    [JsonPropertyName("line1DNever")]
    public Common Line1DNever
    {
      get => line1DNever ??= new();
      set => line1DNever = value;
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
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of ForVerification.
    /// </summary>
    [JsonPropertyName("forVerification")]
    public Program ForVerification
    {
      get => forVerification ??= new();
      set => forVerification = value;
    }

    /// <summary>
    /// A value of SuppPersForVerification.
    /// </summary>
    [JsonPropertyName("suppPersForVerification")]
    public CsePerson SuppPersForVerification
    {
      get => suppPersForVerification ??= new();
      set => suppPersForVerification = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Line1CNever.
    /// </summary>
    [JsonPropertyName("line1CNever")]
    public Common Line1CNever
    {
      get => line1CNever ??= new();
      set => line1CNever = value;
    }

    /// <summary>
    /// A value of GetMedicaidOnlyProgram.
    /// </summary>
    [JsonPropertyName("getMedicaidOnlyProgram")]
    public Common GetMedicaidOnlyProgram
    {
      get => getMedicaidOnlyProgram ??= new();
      set => getMedicaidOnlyProgram = value;
    }

    /// <summary>
    /// A value of Line1BFormer.
    /// </summary>
    [JsonPropertyName("line1BFormer")]
    public Common Line1BFormer
    {
      get => line1BFormer ??= new();
      set => line1BFormer = value;
    }

    /// <summary>
    /// A value of Line1BCurr.
    /// </summary>
    [JsonPropertyName("line1BCurr")]
    public Common Line1BCurr
    {
      get => line1BCurr ??= new();
      set => line1BCurr = value;
    }

    /// <summary>
    /// A value of Line1BNever.
    /// </summary>
    [JsonPropertyName("line1BNever")]
    public Common Line1BNever
    {
      get => line1BNever ??= new();
      set => line1BNever = value;
    }

    /// <summary>
    /// A value of Line1AFormer.
    /// </summary>
    [JsonPropertyName("line1AFormer")]
    public Common Line1AFormer
    {
      get => line1AFormer ??= new();
      set => line1AFormer = value;
    }

    /// <summary>
    /// A value of Line1ACurr.
    /// </summary>
    [JsonPropertyName("line1ACurr")]
    public Common Line1ACurr
    {
      get => line1ACurr ??= new();
      set => line1ACurr = value;
    }

    /// <summary>
    /// A value of Line1ANever.
    /// </summary>
    [JsonPropertyName("line1ANever")]
    public Common Line1ANever
    {
      get => line1ANever ??= new();
      set => line1ANever = value;
    }

    /// <summary>
    /// A value of Line1Former.
    /// </summary>
    [JsonPropertyName("line1Former")]
    public Common Line1Former
    {
      get => line1Former ??= new();
      set => line1Former = value;
    }

    /// <summary>
    /// A value of Line1Curr.
    /// </summary>
    [JsonPropertyName("line1Curr")]
    public Common Line1Curr
    {
      get => line1Curr ??= new();
      set => line1Curr = value;
    }

    /// <summary>
    /// A value of Line1Never.
    /// </summary>
    [JsonPropertyName("line1Never")]
    public Common Line1Never
    {
      get => line1Never ??= new();
      set => line1Never = value;
    }

    /// <summary>
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    private Ocse157Verification assistanceBasedColumn;
    private Common countedIn1F;
    private Common countedIn1G;
    private Common countedIn1D;
    private Common countedIn1E;
    private Common countedIn1B;
    private Common countedIn1A;
    private Common line1F;
    private Common line1G;
    private Common line1EFormer;
    private Common line1ECurr;
    private Common line1ENever;
    private Common line1DFormer;
    private Common line1DCurr;
    private Common line1DNever;
    private Ocse157Verification forError;
    private Ocse157Data forCreateOcse157Data;
    private Program forVerification;
    private CsePerson suppPersForVerification;
    private Ocse157Verification null1;
    private Ocse157Verification forCreateOcse157Verification;
    private Case1 restart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common line1CNever;
    private Common getMedicaidOnlyProgram;
    private Common line1BFormer;
    private Common line1BCurr;
    private Common line1BNever;
    private Common line1AFormer;
    private Common line1ACurr;
    private Common line1ANever;
    private Common line1Former;
    private Common line1Curr;
    private Common line1Never;
    private Common assistanceProgram;
    private Common commitCnt;
    private External forCommit;
    private Case1 prev;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    private InterstateRequest interstateRequest;
    private CaseAssignment caseAssignment;
    private Case1 case1;
  }
#endregion
}
