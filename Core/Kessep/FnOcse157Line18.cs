// Program: FN_OCSE157_LINE_18, ID: 371092716, model: 746.
// Short name: SWE02922
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_18.
/// </summary>
[Serializable]
public partial class FnOcse157Line18: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_18 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line18(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line18.
  /// </summary>
  public FnOcse157Line18(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------
    // 7/23
    // -Read all Collections, not just 'Current' (conf again on 7/24)
    // -Add display info.
    // -------------------------------------------------------------
    // --------------------------------------------------------------
    // 7/26
    // Skip Collections created during FY that are a result of
    // adjustment to a collection created in previous FY.
    // The adjustment date must be within FY.
    // --------------------------------------------------------------
    // --------------------------------------------------------------
    // 7/26
    // Include Collections created during FY but adjusted 'after'
    // the FY end.
    // --------------------------------------------------------------
    // --------------------------------------------------------------
    // 08/30/2001
    // Include CSENet collections.
    // --------------------------------------------------------------
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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "18 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line18Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line18Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line18Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
      local.Line18ACurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 10));
      local.Line18AFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 54, 10));
      local.Line18ANever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 64, 10));
    }

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      // -----------------------------------------------------------
      // Case Assignment must be open at some point during the FY.
      // -----------------------------------------------------------
      if (!ReadCaseAssignment())
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "18";
          local.ForCreateOcse157Verification.Comment = "Skipped-Case not Open.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // ----------------------------------------------------------------------
      // First loop checks for FCI, AFI and NAI programs only to see if
      // conditions are met for line 18a.
      // If Case is counted in 18a, then automatically include it in 18.
      // Second loop opens the search to all programs for line 18.
      // -----------------------------------------------------------------------
      local.CountInLine18.Flag = "N";
      local.CountInLine18A.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH and AP/AR combos
      // - active or not.
      // -----------------------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // ---------------------------------------------------------------
        // Using AP/CH or AP/AR combination, read for
        // non-concurrent, non-adjusted Collection created during FY.
        // For 18a, pgm_appld_to must be AFI, FCI or NAI.
        // ---------------------------------------------------------------
        foreach(var item2 in ReadCollectionObligationType())
        {
          // ---------------------------------------------
          // Exclude Collections adjusted before FY end.
          // Include Collections adjusted after FY end.
          // --------------------------------------------
          if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
            Lt(import.ReportEndDate.Date,
            entities.Collection.CollectionAdjustmentDt))
          {
            continue;
          }

          // ------------------------------
          // Exclude MJ AF/FC
          // ------------------------------
          if (Equal(entities.ObligationType.Code, "MJ") && (
            Equal(entities.Collection.ProgramAppliedTo, "AFI") || Equal
            (entities.Collection.ProgramAppliedTo, "FCI")))
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CollectionAmount =
                entities.Collection.Amount;
              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.LineNumber = "18";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Coll applied to MJ AF.";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          // -----------------------------------------------------------
          // Skip direct payments via REIP (Cash Receipt Type = 2 or 7)
          // -----------------------------------------------------------
          if (!ReadCashReceiptTypeCashReceiptDetail())
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CollectionAmount =
                entities.Collection.Amount;
              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.LineNumber = "18";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Coll for REIP payment";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          // -----------------------------------------------------------
          // 7/26
          // Skip Collections created during FY that are a result of
          // adjustment to a collection created in previous FY.
          // The adjustment date must be with FY.
          // -----------------------------------------------------------
          if (ReadCollection())
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CollectionAmount =
                entities.Collection.Amount;
              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.LineNumber = "18";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Result of Adjusted Coll";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }
          else
          {
            // --------------------------------------
            // This is good.
            // --------------------------------------
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              entities.Collection.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionAmount =
              entities.Collection.Amount;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChOrArCsePerson.Number;
            local.ForCreateOcse157Verification.CollectionDte =
              entities.Collection.CollectionDt;
            local.ForCreateOcse157Verification.CollCreatedDte =
              Date(entities.Collection.CreatedTmst);
            local.ForCreateOcse157Verification.ObTypeSgi =
              entities.ObligationType.SystemGeneratedIdentifier;
          }

          local.CountInLine18A.Flag = "Y";

          goto ReadEach;
        }
      }

ReadEach:

      if (AsChar(local.CountInLine18A.Flag) == 'N')
      {
        // ----------------------------------------------------------------------
        // Read all valid AP/CH and AP/AR combos - active or not.
        // -----------------------------------------------------------------------
        foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // ---------------------------------------------------------------
          // Using AP/CH or AP/AR combination, read for
          // non-concurrent, non-adjusted Collection created during FY.
          // ---------------------------------------------------------------
          foreach(var item2 in ReadCollectionObligationTypeDebt())
          {
            // ---------------------------------------------
            // Exclude Collections adjusted before FY end.
            // Include Collections adjusted after FY end.
            // --------------------------------------------
            if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
              Lt(import.ReportEndDate.Date,
              entities.Collection.CollectionAdjustmentDt))
            {
              continue;
            }

            // ------------------------------
            // Exclude MJ AF/FC
            // ------------------------------
            if (Equal(entities.ObligationType.Code, "MJ") && (
              Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI")))
            {
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.CollectionAmount =
                  entities.Collection.Amount;
                local.ForCreateOcse157Verification.CollectionSgi =
                  entities.Collection.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CollectionDte =
                  entities.Collection.CollectionDt;
                local.ForCreateOcse157Verification.CollCreatedDte =
                  Date(entities.Collection.CreatedTmst);
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CaseNumber =
                  entities.Case1.Number;
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.ChOrArCsePerson.Number;
                local.ForCreateOcse157Verification.LineNumber = "18";
                local.ForCreateOcse157Verification.Comment =
                  "Skipped-Coll applied to MJ AF.";
                UseFnCreateOcse157Verification();
                MoveOcse157Verification2(local.Null1,
                  local.ForCreateOcse157Verification);
              }

              continue;
            }

            // -----------------------------------------------------------
            // Skip direct payments via REIP (Cash Receipt Type = 2 or 7)
            // -----------------------------------------------------------
            if (!ReadCashReceiptTypeCashReceiptDetail())
            {
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.CollectionAmount =
                  entities.Collection.Amount;
                local.ForCreateOcse157Verification.CollectionSgi =
                  entities.Collection.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CollectionDte =
                  entities.Collection.CollectionDt;
                local.ForCreateOcse157Verification.CollCreatedDte =
                  Date(entities.Collection.CreatedTmst);
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CaseNumber =
                  entities.Case1.Number;
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.ChOrArCsePerson.Number;
                local.ForCreateOcse157Verification.LineNumber = "18";
                local.ForCreateOcse157Verification.Comment =
                  "Skipped-Coll for REIP payment";
                UseFnCreateOcse157Verification();
                MoveOcse157Verification2(local.Null1,
                  local.ForCreateOcse157Verification);
              }

              continue;
            }

            // -----------------------------------------------------------
            // 7/26
            // Skip Collections created during FY that are a result of
            // adjustment to a collection created in previous FY.
            // The adjustment date must be with FY.
            // -----------------------------------------------------------
            if (ReadCollection())
            {
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.CollectionAmount =
                  entities.Collection.Amount;
                local.ForCreateOcse157Verification.CollectionSgi =
                  entities.Collection.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CollectionDte =
                  entities.Collection.CollectionDt;
                local.ForCreateOcse157Verification.CollCreatedDte =
                  Date(entities.Collection.CreatedTmst);
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CaseNumber =
                  entities.Case1.Number;
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.ChOrArCsePerson.Number;
                local.ForCreateOcse157Verification.LineNumber = "18";
                local.ForCreateOcse157Verification.Comment =
                  "Skipped-Result of Adjusted Coll";
                UseFnCreateOcse157Verification();
                MoveOcse157Verification2(local.Null1,
                  local.ForCreateOcse157Verification);
              }

              continue;
            }
            else
            {
              // --------------------------------------
              // This is good.
              // --------------------------------------
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionAmount =
                entities.Collection.Amount;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
            }

            local.CountInLine18.Flag = "Y";

            goto Test;
          }
        }
      }

Test:

      if (AsChar(local.CountInLine18.Flag) == 'N' && AsChar
        (local.CountInLine18A.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "18";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Colls do not meet criteria.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // -----------------------------------------------------------
      // All conditions are satisifed. Count Case.
      // -----------------------------------------------------------
      UseFn157GetAssistanceForCase();

      // ------------------------------------------
      // No exit state is set in this CAB.
      // ------------------------------------------
      switch(AsChar(local.AssistanceProgram.Flag))
      {
        case 'C':
          ++local.Line18Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line18Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line18Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.LineNumber = "18";
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.CaseAsinEffDte =
          entities.CaseAssignment.EffectiveDate;
        local.ForCreateOcse157Verification.CaseAsinEndDte =
          entities.CaseAssignment.DiscontinueDate;
        local.ForCreateOcse157Verification.PersonProgCode =
          local.ForVerification.Code;
        UseFnCreateOcse157Verification();
      }

      if (AsChar(local.CountInLine18A.Flag) == 'Y')
      {
        switch(AsChar(local.AssistanceProgram.Flag))
        {
          case 'C':
            ++local.Line18ACurr.Count;

            break;
          case 'F':
            ++local.Line18AFormer.Count;

            break;
          default:
            ++local.Line18ANever.Count;

            break;
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.LineNumber = "18a";
          UseFnCreateOcse157Verification();
        }
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "18 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18Never.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18ACurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18AFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line18ANever.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "18";
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
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "18";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line18Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line18Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line18Never.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "18a";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line18ACurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line18AFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line18ANever.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "19 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "18";
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
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
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
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

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.Case1.Assign(entities.Case1);

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
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 2);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 3);
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
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
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

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChOrArCaseRole.Populated = false;
    entities.ChOrArCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChOrArCsePerson.Number = db.GetString(reader, 8);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.ChOrArCaseRole.ArWaivedInsurance =
          db.GetNullableString(reader, 13);
        entities.ChOrArCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 14);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChOrArCaseRole.Populated = true;
        entities.ChOrArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptTypeCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptTypeCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjusted.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Adjusted.AppliedToCode = db.GetString(reader, 1);
        entities.Adjusted.CollectionDt = db.GetDate(reader, 2);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Adjusted.ConcurrentInd = db.GetString(reader, 4);
        entities.Adjusted.CrtType = db.GetInt32(reader, 5);
        entities.Adjusted.CstId = db.GetInt32(reader, 6);
        entities.Adjusted.CrvId = db.GetInt32(reader, 7);
        entities.Adjusted.CrdId = db.GetInt32(reader, 8);
        entities.Adjusted.ObgId = db.GetInt32(reader, 9);
        entities.Adjusted.CspNumber = db.GetString(reader, 10);
        entities.Adjusted.CpaType = db.GetString(reader, 11);
        entities.Adjusted.OtrId = db.GetInt32(reader, 12);
        entities.Adjusted.OtrType = db.GetString(reader, 13);
        entities.Adjusted.OtyId = db.GetInt32(reader, 14);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Adjusted.Amount = db.GetDecimal(reader, 17);
        entities.Adjusted.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Adjusted.Populated = true;
        CheckValid<Collection>("AppliedToCode", entities.Adjusted.AppliedToCode);
          
        CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd", entities.Adjusted.ConcurrentInd);
          
        CheckValid<Collection>("CpaType", entities.Adjusted.CpaType);
        CheckValid<Collection>("OtrType", entities.Adjusted.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Adjusted.ProgramAppliedTo);
      });
  }

  private IEnumerable<bool> ReadCollectionObligationType()
  {
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCollectionObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
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
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeDebt()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCollectionObligationTypeDebt",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
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
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 22);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 23);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

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
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
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

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Common displayInd;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CountInLine18.
    /// </summary>
    [JsonPropertyName("countInLine18")]
    public Common CountInLine18
    {
      get => countInLine18 ??= new();
      set => countInLine18 = value;
    }

    /// <summary>
    /// A value of CountInLine18A.
    /// </summary>
    [JsonPropertyName("countInLine18A")]
    public Common CountInLine18A
    {
      get => countInLine18A ??= new();
      set => countInLine18A = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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

    /// <summary>
    /// A value of MonthStartDte.
    /// </summary>
    [JsonPropertyName("monthStartDte")]
    public DateWorkArea MonthStartDte
    {
      get => monthStartDte ??= new();
      set => monthStartDte = value;
    }

    /// <summary>
    /// A value of MonthEndDate.
    /// </summary>
    [JsonPropertyName("monthEndDate")]
    public DateWorkArea MonthEndDate
    {
      get => monthEndDate ??= new();
      set => monthEndDate = value;
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
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of Line18Curr.
    /// </summary>
    [JsonPropertyName("line18Curr")]
    public Common Line18Curr
    {
      get => line18Curr ??= new();
      set => line18Curr = value;
    }

    /// <summary>
    /// A value of Line18Former.
    /// </summary>
    [JsonPropertyName("line18Former")]
    public Common Line18Former
    {
      get => line18Former ??= new();
      set => line18Former = value;
    }

    /// <summary>
    /// A value of Line18Never.
    /// </summary>
    [JsonPropertyName("line18Never")]
    public Common Line18Never
    {
      get => line18Never ??= new();
      set => line18Never = value;
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
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    /// <summary>
    /// A value of Line18ACurr.
    /// </summary>
    [JsonPropertyName("line18ACurr")]
    public Common Line18ACurr
    {
      get => line18ACurr ??= new();
      set => line18ACurr = value;
    }

    /// <summary>
    /// A value of Line18AFormer.
    /// </summary>
    [JsonPropertyName("line18AFormer")]
    public Common Line18AFormer
    {
      get => line18AFormer ??= new();
      set => line18AFormer = value;
    }

    /// <summary>
    /// A value of Line18ANever.
    /// </summary>
    [JsonPropertyName("line18ANever")]
    public Common Line18ANever
    {
      get => line18ANever ??= new();
      set => line18ANever = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Ocse157Verification null1;
    private Common countInLine18;
    private Common countInLine18A;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Case1 prev;
    private DateWorkArea monthStartDte;
    private DateWorkArea monthEndDate;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common line18Curr;
    private Common line18Former;
    private Common line18Never;
    private Program forVerification;
    private CsePerson suppPersForVerification;
    private Common assistanceProgram;
    private Common line18ACurr;
    private Common line18AFormer;
    private Common line18ANever;
    private Common commitCnt;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of ChOrArCaseRole.
    /// </summary>
    [JsonPropertyName("chOrArCaseRole")]
    public CaseRole ChOrArCaseRole
    {
      get => chOrArCaseRole ??= new();
      set => chOrArCaseRole = value;
    }

    /// <summary>
    /// A value of ChOrArCsePerson.
    /// </summary>
    [JsonPropertyName("chOrArCsePerson")]
    public CsePerson ChOrArCsePerson
    {
      get => chOrArCsePerson ??= new();
      set => chOrArCsePerson = value;
    }

    private Collection adjusted;
    private CsePersonAccount obligor;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Collection collection;
    private ObligationTransaction debt;
    private CsePersonAccount supported;
    private Obligation obligation;
    private ObligationType obligationType;
    private InterstateRequest interstateRequest;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chOrArCaseRole;
    private CsePerson chOrArCsePerson;
  }
#endregion
}
