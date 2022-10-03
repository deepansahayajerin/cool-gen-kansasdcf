// Program: FN_OCSE157_LINE_21_22_23, ID: 371092717, model: 746.
// Short name: SWE02926
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_21_22_23.
/// </summary>
[Serializable]
public partial class FnOcse157Line212223: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_21_22_23 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line212223(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line212223.
  /// </summary>
  public FnOcse157Line212223(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 05/??/01  KDoshi			Initial Development
    // 07/24/01				Health Insurance Policy need  'not'  be verified.
    // 07/30/01				Fix logic to create ocse157_data records.
    // 08/07/01				Delete attribute debt_detail preconversion_program_state.
    // 08/13/01				Include cases where AR is ordered to provide med support.
    // 10/18/01				For line 23, exclude coverage provided by someone other than 
    // AP/AR..
    // 10/30/01				For non-fin ldets, read LROL instead of LOPS.
    // 03/09/06  GVandy	WR00230751	1) Include "O" class legal actions.
    // 					2) Exclude arrears only cases.
    // 					3) Legal action must have a filed date.
    // 					4) New line 21a.
    // 					5) Line 22 - exclude cases where Health Insurance is not viable.
    // 10/27/06  GVandy	PR294186	Performance change
    // 01/19/07  GVandy	PR297812	Always create Line 21 & 21a audit data.
    // -------------------------------------------------------------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    MoveOcse157Verification2(local.ForCreateOcse157Verification,
      local.Line21AOcse157Verification);
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "21 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line21.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line21ACommon.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line22Common.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
      local.Line23.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 10));
    }

    ReadOcse157Data();

    // --------------------------------------------------------------------------------
    // Read all Cases.
    // Skip if Case is not included in Line 1.
    // Read AP/CH or AR/CH combos - active during FY.
    // Read 'J' class LDETs using AP/CH and AR/CH.
    // Look for HIC first. If active, include Case in lines 21 and 22.
    // If Ins Policy is active and HIC is active, also count in 23.
    // If active HIC is nf, look for UM.
    // If UM is active, then include in line 21.
    // Now look for MC or MS.
    // If MC or MS has outstanding balance or open accrual
    // instructions, then count case in line 21.
    // ---------------------------------------------------------------------------------
    foreach(var item in ReadCase())
    {
      MoveOcse157Verification3(local.NullOcse157Verification,
        local.ForCreateOcse157Verification);

      // ----------------------------------------------
      // Was this Case reported in line 1? If not, skip.
      // ----------------------------------------------
      ReadOcse157Verification();

      if (IsEmpty(local.Minimum.Number))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "21";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case not included in line 1.";
          UseFnCreateOcse157Verification2();
        }

        continue;
      }

      // ---------------------------------------------------------------
      // 08/06/2006  Skip if case is arrears only.
      // ---------------------------------------------------------------
      local.CaseIsArrearsOnly.Flag = "";

      if (ReadAccrualInstructions1())
      {
        // -- One or more active accrual instruction exists.  The case is not 
        // arrears only.
        local.CaseIsArrearsOnly.Flag = "N";
      }

      if (IsEmpty(local.CaseIsArrearsOnly.Flag))
      {
        foreach(var item1 in ReadDebtDetail())
        {
          // -- There are no active accrual instructions and there is one or 
          // more debt with a balance due.  The case is arrears only.
          local.CaseIsArrearsOnly.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.LineNumber = "21";
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Case is Arrears only.";
            UseFnCreateOcse157Verification2();
          }

          goto ReadEach3;
        }
      }

      local.CountIn21.Flag = "N";
      local.CountIn22.Flag = "N";
      local.CountIn23.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH or AR/CH combos -
      // active at some point during the FY.
      // Date checks are to ensure we read overlapping AP/CH or
      // AR/CH roles only.
      // -----------------------------------------------------------------------
      // ----------------------------------------------------------
      // 8/13/2001
      // Include cases where AR is ordered to provide med support.
      // ---------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // ----------------------------------------------------------------------
        // Read 'active' HIC LDETs using AP/CH or AR/CH.
        // Read for J Class legal actions only.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        // --  08/06/2006  Added "O" classification and filed_date to read each 
        // below.
        foreach(var item2 in ReadLegalActionDetailLegalAction2())
        {
          local.CountIn21.Flag = "Y";
          local.CountIn22.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChCsePerson.Number;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApOrArCsePerson.Number;
            local.ForCreateOcse157Verification.CourtOrderNumber =
              entities.LegalAction.StandardNumber;
            local.ForCreateOcse157Verification.ObTypeSgi = 999;
            local.ForCreateOcse157Verification.Comment =
              "Health insurance ordered.";
          }

          // @@@
          // ---------------------------------------------------------------
          // 08/06/2006  Do not include the case in Lines 22 and 23 if
          // Health Insurance is not viable for any AP/CH or AR/CH combo
          // on the HIC legal detail.
          // ---------------------------------------------------------------
          if (ReadHealthInsuranceViability())
          {
            if (AsChar(entities.HealthInsuranceViability.HinsViableInd) == 'N')
            {
              local.CountIn22.Flag = "N";
              local.CountIn23.Flag = "N";

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.Line22Ocse157Verification.Assign(
                  local.ForCreateOcse157Verification);
                local.Line22Ocse157Verification.LineNumber = "22";
                local.Line22Ocse157Verification.CaseNumber =
                  entities.Case1.Number;
                local.Line22Ocse157Verification.Comment =
                  "Skipped-Health Insurance Not Viable.";
                UseFnCreateOcse157Verification1();
              }

              goto ReadEach2;
            }
          }

          // -------------------------------------------------------
          // AP/AR must be the policy holder.
          // CH must be covered on this policy.
          // Coverage must be active.
          // -------------------------------------------------------
          foreach(var item3 in ReadHealthInsuranceCoveragePersonalHealthInsurance())
            
          {
            if (ReadContact())
            {
              // ---------------------------------------------------------------
              // Skip since Coverage is provided by someone other than AP/AR.
              // ---------------------------------------------------------------
              continue;
            }

            local.CountIn23.Flag = "Y";

            // -- 08/06/2006 Don't escape out.  We still need to check if Health
            //    Insurance is not viable for any other AP/CH combos on the HIC 
            // legal detail.
            break;
          }

          // -- Get next AP/CH combo...
          goto ReadEach1;
        }

ReadEach1:
        ;
      }

ReadEach2:

      if (AsChar(local.CountIn21.Flag) == 'N')
      {
        // ----------------------------------------------------------------------
        // We got here because HIC is not 'active'.
        // ----------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // For current case, read all valid AP/CH and AR/CH combos -
        // active at some point during the FY.
        // Date checks are to ensure we read overlapping AP/CH
        // or AR/CH roles only.
        // -----------------------------------------------------------------------
        foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // -----------------------------------------------------------
          // Read 'active' UM LDETs using AP/CH and AR/CH.
          // -----------------------------------------------------------
          // --  08/06/2006  Added "O" classification and filed_date to read 
          // each below.
          if (ReadLegalActionDetailLegalAction1())
          {
            local.CountIn21.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;
              local.ForCreateOcse157Verification.ObTypeSgi = 999;
              local.ForCreateOcse157Verification.Comment =
                "Un-reimbursed medical ordered.";
            }

            goto Test;
          }

          // ----------------------------------------------------------------------
          // We got here because HIC and UM are not active.
          // Read MC or MS fin LDETs using AP/CH and AR/CH.
          // ----------------------------------------------------------------------
          // --  08/06/2006  Added "O" classification and filed_date to read 
          // each below.
          // Obligation Type Sys Gen Ids... MS=3,  MC=19
          foreach(var item2 in ReadAccrualInstructions2())
          {
            if (!ReadDebt())
            {
              // -- Obligation is not for the current child.
              continue;
            }

            if (!ReadObligationTypeLegalAction())
            {
              continue;
            }

            local.CountIn21.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.Comment =
                "MS or MC obligation accruing.";
            }

            goto Test;
          }
        }
      }

Test:

      if (AsChar(local.CountIn21.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.LineNumber = "21";
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Comment = "Skipped-LDET nf.";
          UseFnCreateOcse157Verification2();
        }

        continue;
      }

      // -----------------------------------------------------------
      // All conditions are satisifed for line 21. Count Case.
      // -----------------------------------------------------------
      ++local.Line21.Count;

      // -- 01/19/07 GVandy PR297812  Always create Line 21 audit data.
      local.ForCreateOcse157Verification.LineNumber = "21";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
      UseFnCreateOcse157Verification2();

      // -----------------------------------------------------------
      // Check if Case needs to be included in line 21a.
      // -----------------------------------------------------------
      UseFnOcse157Line21A();

      if (AsChar(local.CountIn21A.Flag) == 'Y')
      {
        ++local.Line21ACommon.Count;

        // -- 01/19/07 GVandy PR297812  Always create Line 21a audit data.
        local.Line21AOcse157Verification.LineNumber = "21a";
        local.Line21AOcse157Verification.Column = "a";
        local.Line21AOcse157Verification.CaseNumber = entities.Case1.Number;
        UseFnCreateOcse157Verification3();
      }

      // -----------------------------------------------------------
      // Check if Case needs to be included in lines 22 and 23.
      // -----------------------------------------------------------
      if (AsChar(local.CountIn22.Flag) == 'Y')
      {
        ++local.Line22Common.Count;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.LineNumber = "22";
          UseFnCreateOcse157Verification2();
        }
      }

      if (AsChar(local.CountIn23.Flag) == 'Y')
      {
        ++local.Line23.Count;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.LineNumber = "23";
          local.ForCreateOcse157Verification.HlthInsCovrgId =
            entities.HealthInsuranceCoverage.Identifier;
          local.ForCreateOcse157Verification.Comment =
            "Health insurance provided as ordered.";
          UseFnCreateOcse157Verification2();
        }
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "21 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line21.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line21ACommon.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line22Common.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line23.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "21";
          local.ForError.CaseNumber = entities.Case1.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

ReadEach3:
      ;
    }

    // --------------------------------------------------
    // Processing complete.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "21";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line21.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "21a";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line21ACommon.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "22";
    local.ForCreateOcse157Data.Number = local.Line22Common.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "23";
    local.ForCreateOcse157Data.Number = local.Line23.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "24 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "21";
      local.ForError.CaseNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.HlthInsCovrgId = source.HlthInsCovrgId;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
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
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.HlthInsCovrgId = source.HlthInsCovrgId;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification4(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.Comment = source.Comment;
  }

  private void UseFnCreateOcse157Data()
  {
    var useImport = new FnCreateOcse157Data.Import();
    var useExport = new FnCreateOcse157Data.Export();

    useImport.Ocse157Data.Assign(local.ForCreateOcse157Data);

    Call(FnCreateOcse157Data.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification1()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.Line22Ocse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification2()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification3()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification2(local.Line21AOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseFnOcse157Line21A()
  {
    var useImport = new FnOcse157Line21A.Import();
    var useExport = new FnOcse157Line21A.Export();

    useImport.Persistent.Assign(entities.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);

    Call(FnOcse157Line21A.Execute, useImport, useExport);

    MoveOcse157Verification4(useExport.Ocse157Verification,
      local.Line21AOcse157Verification);
    local.CountIn21A.Flag = useExport.CountInLine21A.Flag;
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

  private bool ReadAccrualInstructions1()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadAccrualInstructions2()
  {
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApOrArCsePerson.Number);
        db.SetDate(
          command, "asOfDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
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
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;
    entities.ApOrArCsePerson.Populated = false;
    entities.ApOrArCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApOrArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApOrArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApOrArCsePerson.Number = db.GetString(reader, 1);
        entities.ApOrArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApOrArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApOrArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApOrArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 6);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 7);
        entities.ChCsePerson.Number = db.GetString(reader, 7);
        entities.ChCaseRole.Type1 = db.GetString(reader, 8);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.ChCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ApOrArCsePerson.Populated = true;
        entities.ApOrArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApOrArCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

        return true;
      });
  }

  private bool ReadContact()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          entities.HealthInsuranceCoverage.ConHNumber.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.HealthInsuranceCoverage.CspHNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadDebt()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyType", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obTrnId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    entities.HealthInsuranceCoverage.Populated = false;
    entities.PersonalHealthInsurance.Populated = false;

    return ReadEach("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ChCsePerson.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber2", entities.ApOrArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 5);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 6);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 7);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 8);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;

        return true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType", entities.ChCaseRole.Type1);
        db.SetString(command, "casNumber", entities.ChCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ChCaseRole.CspNumber);
        db.
          SetNullableString(command, "cspNum", entities.ApOrArCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadLegalActionDetailLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ApOrArCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "croIdentifier1", entities.ApOrArCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApOrArCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApOrArCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApOrArCaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt1", local.NullDateWorkArea.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "filedDt2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ApOrArCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "croIdentifier1", entities.ApOrArCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApOrArCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApOrArCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApOrArCaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt1", local.NullDateWorkArea.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "filedDt2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadObligationTypeLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.LegalAction.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationTypeLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Debt.OtyType);
        db.SetInt32(command, "obId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.LegalAction.Identifier = db.GetInt32(reader, 3);
        entities.LegalAction.Classification = db.GetString(reader, 4);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadOcse157Data()
  {
    local.Max.Populated = false;

    return Read("ReadOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.Max.RunNumber = db.GetNullableInt32(reader, 0);
        local.Max.Populated = true;
      });
  }

  private bool ReadOcse157Verification()
  {
    local.Minimum.Populated = false;

    return Read("ReadOcse157Verification",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber", local.Max.RunNumber.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.Minimum.Number = db.GetString(reader, 0);
        local.Minimum.Populated = true;
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
    /// A value of Line22Ocse157Verification.
    /// </summary>
    [JsonPropertyName("line22Ocse157Verification")]
    public Ocse157Verification Line22Ocse157Verification
    {
      get => line22Ocse157Verification ??= new();
      set => line22Ocse157Verification = value;
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
    /// A value of Line21AOcse157Verification.
    /// </summary>
    [JsonPropertyName("line21AOcse157Verification")]
    public Ocse157Verification Line21AOcse157Verification
    {
      get => line21AOcse157Verification ??= new();
      set => line21AOcse157Verification = value;
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
    /// A value of Line21.
    /// </summary>
    [JsonPropertyName("line21")]
    public Common Line21
    {
      get => line21 ??= new();
      set => line21 = value;
    }

    /// <summary>
    /// A value of Line21ACommon.
    /// </summary>
    [JsonPropertyName("line21ACommon")]
    public Common Line21ACommon
    {
      get => line21ACommon ??= new();
      set => line21ACommon = value;
    }

    /// <summary>
    /// A value of Line22Common.
    /// </summary>
    [JsonPropertyName("line22Common")]
    public Common Line22Common
    {
      get => line22Common ??= new();
      set => line22Common = value;
    }

    /// <summary>
    /// A value of Line23.
    /// </summary>
    [JsonPropertyName("line23")]
    public Common Line23
    {
      get => line23 ??= new();
      set => line23 = value;
    }

    /// <summary>
    /// A value of NullOcse157Verification.
    /// </summary>
    [JsonPropertyName("nullOcse157Verification")]
    public Ocse157Verification NullOcse157Verification
    {
      get => nullOcse157Verification ??= new();
      set => nullOcse157Verification = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Minimum.
    /// </summary>
    [JsonPropertyName("minimum")]
    public Case1 Minimum
    {
      get => minimum ??= new();
      set => minimum = value;
    }

    /// <summary>
    /// A value of CaseIsArrearsOnly.
    /// </summary>
    [JsonPropertyName("caseIsArrearsOnly")]
    public Common CaseIsArrearsOnly
    {
      get => caseIsArrearsOnly ??= new();
      set => caseIsArrearsOnly = value;
    }

    /// <summary>
    /// A value of CountIn21.
    /// </summary>
    [JsonPropertyName("countIn21")]
    public Common CountIn21
    {
      get => countIn21 ??= new();
      set => countIn21 = value;
    }

    /// <summary>
    /// A value of CountIn22.
    /// </summary>
    [JsonPropertyName("countIn22")]
    public Common CountIn22
    {
      get => countIn22 ??= new();
      set => countIn22 = value;
    }

    /// <summary>
    /// A value of CountIn23.
    /// </summary>
    [JsonPropertyName("countIn23")]
    public Common CountIn23
    {
      get => countIn23 ??= new();
      set => countIn23 = value;
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
    /// A value of CountIn21A.
    /// </summary>
    [JsonPropertyName("countIn21A")]
    public Common CountIn21A
    {
      get => countIn21A ??= new();
      set => countIn21A = value;
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

    private Ocse157Verification line22Ocse157Verification;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Ocse157Verification line21AOcse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Common line21;
    private Common line21ACommon;
    private Common line22Common;
    private Common line23;
    private Ocse157Verification nullOcse157Verification;
    private Ocse157Data max;
    private Case1 minimum;
    private Common caseIsArrearsOnly;
    private Common countIn21;
    private Common countIn22;
    private Common countIn23;
    private DateWorkArea nullDateWorkArea;
    private Common countIn21A;
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
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of ApOrArCsePerson.
    /// </summary>
    [JsonPropertyName("apOrArCsePerson")]
    public CsePerson ApOrArCsePerson
    {
      get => apOrArCsePerson ??= new();
      set => apOrArCsePerson = value;
    }

    /// <summary>
    /// A value of ApOrArCaseRole.
    /// </summary>
    [JsonPropertyName("apOrArCaseRole")]
    public CaseRole ApOrArCaseRole
    {
      get => apOrArCaseRole ??= new();
      set => apOrArCaseRole = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public LegalActionCaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public LegalActionPerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    private Ocse157Data ocse157Data;
    private Ocse157Verification ocse157Verification;
    private Case1 case1;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private CsePersonAccount supported;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Obligation obligation;
    private CsePersonAccount obligor1;
    private CsePerson apOrArCsePerson;
    private CaseRole apOrArCaseRole;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private HealthInsuranceViability healthInsuranceViability;
    private Contact contact;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private LegalActionDetail legalActionDetail;
    private LegalActionCaseRole ap;
    private LegalActionCaseRole chLegalActionCaseRole;
    private ObligationType obligationType;
    private LegalActionPerson obligor2;
    private LegalActionPerson supp;
  }
#endregion
}
