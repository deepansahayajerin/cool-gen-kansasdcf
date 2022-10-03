// Program: FN_OCSE157_LINE_14, ID: 371092713, model: 746.
// Short name: SWE02919
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_14.
/// </summary>
[Serializable]
public partial class FnOcse157Line14: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_14 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line14(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line14.
  /// </summary>
  public FnOcse157Line14(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------
    // Initial Version - 7/2001
    // -------------------------------------
    // -----------------------------------------------------
    // 7/23/2001
    // Virtual re-write of this CAB.
    // -----------------------------------------------------
    // ---------------------------------------------------------
    // 7/24/2001
    // Count 'once' if AF is discontinued on multiple CH (or AR)
    // on a Case on the same day.
    // Fix checkpoint logic.
    // ---------------------------------------------------------
    // -------------------------------------------------------
    // 8/12/2001.
    // Fix invalid date problem.
    // -------------------------------------------------------
    // -------------------------------------------------------
    // 8/12/2001.
    // Include Collections adjusted after the FY end.
    // -------------------------------------------------------
    // --------------------------------------------------------
    // 8/30/2001.
    // Exclude direct payments through REIP. i.e. CRT 2, 7.
    // --------------------------------------------------------
    // ---------------------------------------------------------------
    // 9/4/2001.  PR# 127262
    // Significant changes to cater for multiple-case involvement for
    // a supp person.
    // ---------------------------------------------------------------
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "14 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line14.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    // ----------------------------------------------
    // Read AF and AFI closures during FY.
    // ----------------------------------------------
    foreach(var item in ReadPersonProgramCsePerson1())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
      ++local.CommitCnt.Count;

      // --------------------------------------------------
      // Take checkpoint if person number has changed and
      // checkpoint count has been reached..
      // --------------------------------------------------
      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && !
        Equal(entities.CsePerson.Number, local.Prev.Number))
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "14 " + local.Prev.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line14.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "14";
          local.ForError.SuppPersonNumber = local.Prev.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      local.Prev.Number = entities.CsePerson.Number;

      // ------------------------------------------------------------------------
      // Get first and last day of Calendar month during which AF was closed.
      // ------------------------------------------------------------------------
      local.AfDiscontinueDt.Date = entities.PersonProgram.DiscontinueDate;
      UseCabGetYearMonthFromDate();
      UseOeGetMonthStartAndEndDate();

      if (AsChar(local.InvalidDate.Flag) == 'Y')
      {
        // ----------------------------------------------
        // Should never happen. Just a safety net!
        // ----------------------------------------------
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.LineNumber = "14";
          local.ForCreateOcse157Verification.Comment =
            "Skipped- Invalid Pers Program date!";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        continue;
      }

      local.NextMonth.Date =
        AddMonths(entities.PersonProgram.DiscontinueDate, 1);

      // ---------------------------------------------------------------------
      // Get Case as of PEPR discontinue_date.
      // If supp person is active on multiple cases,
      // count each case assuming AP makes payment and all other
      // conditions are met.
      // ---------------------------------------------------------------------
      local.ChOrArCaseRoleFound.Flag = "N";

      foreach(var item1 in ReadCaseRoleCase())
      {
        local.ChOrArCaseRoleFound.Flag = "Y";

        // ---------------------------------------
        // Skip emancipated children.
        // ---------------------------------------
        if (Lt(local.NullDate.Date, entities.CaseRole.DateOfEmancipation) && Lt
          (entities.CaseRole.DateOfEmancipation, import.ReportStartDate.Date) &&
          Equal(entities.CaseRole.Type1, "CH"))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "14";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Emancipated CHild.";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);
          }

          continue;
        }

        // --------------------------------------------------------
        // Case must be open during the month AF or AFI ended.
        // -------------------------------------------------------
        if (!ReadCaseAssignment())
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "14";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Case not Open.";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);
          }

          continue;
        }

        // ---------------------------------------------------------------
        // If AF, AFI program is active for any CH or AR on current case,
        // during the next month then ignore this closure.
        // Read programs during the time frame when the person is on
        // the case.  We want to ignore instances where CH goes on
        // AF only after he leaves current case.
        // ---------------------------------------------------------------
        foreach(var item2 in ReadPersonProgramCsePerson2())
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "14";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-AF active on " + entities.SubsequentCsePerson.Number;
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);
          }

          goto ReadEach;
        }

        // ---------------------------------------------------------------
        // If a Collection was received (Collection_date) during the
        // month AF was closed, then count, else skip.
        // Appl_to_code must be 'C' for Current.
        // Skip Collections created after the end of FY.
        // Skip adjusted Collections.
        // Skip concurrent Collections.
        // Skip REIP payments - CRT = 2 or 7.
        // Read all ob types.
        // ---------------------------------------------------------------
        local.CollectionFound.Flag = "N";

        foreach(var item2 in ReadCollectionCsePerson1())
        {
          // ---------------------------------------------------------------
          // So, we received a current collection for supp person.
          // Was the collection received from an AP on current case?
          // ---------------------------------------------------------------
          if (!ReadCaseRole1())
          {
            continue;
          }

          local.CollectionFound.Flag = "Y";

          // ---------------------------------------------------------------
          // Save to create verification later
          // ---------------------------------------------------------------
          MoveCollection(entities.Collection, local.Collection);
          local.Ap.Number = entities.ApCsePerson.Number;

          break;
        }

        if (AsChar(local.CollectionFound.Flag) == 'N')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "14";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-No Coll during AF close mnth.";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);
          }

          continue;
        }

        // -----------------------------------------------------------
        // If AF is discontinued on the same day for multiple CH or AR
        // on the same case and a Collection is received for each, then
        // count this as one closure.
        // We are processing in Ascending order of cse_person
        // number. If we find another CH/AR on current case that also
        // has AF discontinued on same day, and that CH/AR person
        // number is less than current person number, then we know we
        // have already reported this closure. So skip.
        // -----------------------------------------------------------
        foreach(var item2 in ReadPersonProgramCsePersonCaseRole())
        {
          // ---------------------------------------------------------------
          // Skip emancipated CHildren.
          // ---------------------------------------------------------------
          if (Lt(local.NullDate.Date,
            entities.SubsequentCaseRole.DateOfEmancipation) && Lt
            (entities.SubsequentCaseRole.DateOfEmancipation,
            import.ReportStartDate.Date) && Equal
            (entities.SubsequentCaseRole.Type1, "CH"))
          {
            continue;
          }

          // ---------------------------------------------------------------
          // Ok, so we know there was another CH on this case that also
          // had AF discontinued on the same day. But, did we receive a
          // Collection that would make us count that CH??
          // ---------------------------------------------------------------
          foreach(var item3 in ReadCollectionCsePerson2())
          {
            // ---------------------------------------------------------------
            // So, we received a current collection for subsequent person.
            // Was the collection received from an AP on current case?
            // ---------------------------------------------------------------
            if (!ReadCaseRole2())
            {
              continue;
            }

            // ---------------------------------------------------------------
            // We would have already counted this closure. Skip.
            // ---------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.CsePerson.Number;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.LineNumber = "14";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Already counted " + entities
                .SubsequentCsePerson.Number;
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            goto ReadEach;
          }
        }

        // -----------------------------------------------------------
        // All conditions are satisfied. Increment counter by 1.
        // -----------------------------------------------------------
        ++local.Line14.Count;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.ObligorPersonNbr = local.Ap.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "14";
          local.ForCreateOcse157Verification.Column = "a";
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.CaseAssignment.EffectiveDate;
          local.ForCreateOcse157Verification.CaseAsinEndDte =
            entities.CaseAssignment.DiscontinueDate;
          local.ForCreateOcse157Verification.CollectionDte =
            local.Collection.CollectionDt;
          local.ForCreateOcse157Verification.CollCreatedDte =
            Date(local.Collection.CreatedTmst);
          local.ForCreateOcse157Verification.CollectionAmount =
            local.Collection.Amount;
          local.ForCreateOcse157Verification.CollectionSgi =
            local.Collection.SystemGeneratedIdentifier;
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        // --------------------------------------------------
        // End of Case READ EACH.
        // --------------------------------------------------
ReadEach:
        ;
      }

      if (AsChar(local.ChOrArCaseRoleFound.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.LineNumber = "14";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Pers does not have AR/CH role.";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }
      }

      // --------------------------------------------------
      // End of Person Program READ EACH.
      // --------------------------------------------------
    }

    // --------------------------------------------------
    // Processing complete for this line
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "14";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line14.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "16 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "14";
      local.ForError.CaseNumber = "";
      local.ForError.SuppPersonNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
    target.CreatedTmst = source.CreatedTmst;
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
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
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
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.AfDiscontinueDt.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.AfDiscontinueDt.YearMonth = useExport.DateWorkArea.YearMonth;
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

  private void UseOeGetMonthStartAndEndDate()
  {
    var useImport = new OeGetMonthStartAndEndDate.Import();
    var useExport = new OeGetMonthStartAndEndDate.Export();

    useImport.DateWorkArea.YearMonth = local.AfDiscontinueDt.YearMonth;

    Call(OeGetMonthStartAndEndDate.Execute, useImport, useExport);

    local.InvalidDate.Flag = useExport.InvalidMonth.Flag;
    local.MonthEndDate.Date = useExport.MonthEndDate.Date;
    local.MonthStartDte.Date = useExport.MonthStartDate.Date;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
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
          local.MonthEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.MonthStartDte.Date.GetValueOrDefault());
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

  private bool ReadCaseRole1()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.CaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", entities.CaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          entities.SubsequentCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          entities.SubsequentCaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          entities.PersonProgram.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 8);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 9);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePerson1()
  {
    entities.ApCsePerson.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePerson1",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.
          SetNullableString(command, "cspSupNumber", entities.CsePerson.Number);
          
        db.SetDate(
          command, "date1", local.MonthStartDte.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", local.MonthEndDate.Date.GetValueOrDefault());
          
        db.SetDateTime(
          command, "createdTmst",
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 19);
        entities.ApCsePerson.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePerson2()
  {
    entities.ApCsePerson.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePerson2",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.SubsequentCsePerson.Number);
        db.SetDate(
          command, "date1", local.MonthStartDte.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", local.MonthEndDate.Date.GetValueOrDefault());
          
        db.SetDateTime(
          command, "createdTmst",
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 19);
        entities.ApCsePerson.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramCsePerson1()
  {
    entities.PersonProgram.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadPersonProgramCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Restart.Number);
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramCsePerson2()
  {
    entities.SubsequentCsePerson.Populated = false;
    entities.SubsequentPersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          entities.PersonProgram.DiscontinueDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.NextMonth.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SubsequentPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.SubsequentCsePerson.Number = db.GetString(reader, 0);
        entities.SubsequentPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.SubsequentPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.SubsequentPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.SubsequentPersonProgram.PrgGeneratedId =
          db.GetInt32(reader, 4);
        entities.SubsequentCsePerson.Populated = true;
        entities.SubsequentPersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramCsePersonCaseRole()
  {
    entities.SubsequentCsePerson.Populated = false;
    entities.SubsequentCaseRole.Populated = false;
    entities.SubsequentPersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          entities.PersonProgram.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SubsequentPersonProgram.CspNumber = db.GetString(reader, 0);
        entities.SubsequentCsePerson.Number = db.GetString(reader, 0);
        entities.SubsequentPersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.SubsequentPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.SubsequentPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.SubsequentPersonProgram.PrgGeneratedId =
          db.GetInt32(reader, 4);
        entities.SubsequentCaseRole.CasNumber = db.GetString(reader, 5);
        entities.SubsequentCaseRole.CspNumber = db.GetString(reader, 6);
        entities.SubsequentCaseRole.Type1 = db.GetString(reader, 7);
        entities.SubsequentCaseRole.Identifier = db.GetInt32(reader, 8);
        entities.SubsequentCaseRole.StartDate = db.GetNullableDate(reader, 9);
        entities.SubsequentCaseRole.EndDate = db.GetNullableDate(reader, 10);
        entities.SubsequentCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 11);
        entities.SubsequentCsePerson.Populated = true;
        entities.SubsequentCaseRole.Populated = true;
        entities.SubsequentPersonProgram.Populated = true;

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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CollectionFound.
    /// </summary>
    [JsonPropertyName("collectionFound")]
    public Common CollectionFound
    {
      get => collectionFound ??= new();
      set => collectionFound = value;
    }

    /// <summary>
    /// A value of ChOrArCaseRoleFound.
    /// </summary>
    [JsonPropertyName("chOrArCaseRoleFound")]
    public Common ChOrArCaseRoleFound
    {
      get => chOrArCaseRoleFound ??= new();
      set => chOrArCaseRoleFound = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of InvalidDate.
    /// </summary>
    [JsonPropertyName("invalidDate")]
    public Common InvalidDate
    {
      get => invalidDate ??= new();
      set => invalidDate = value;
    }

    /// <summary>
    /// A value of AfDiscontinueDt.
    /// </summary>
    [JsonPropertyName("afDiscontinueDt")]
    public DateWorkArea AfDiscontinueDt
    {
      get => afDiscontinueDt ??= new();
      set => afDiscontinueDt = value;
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
    /// A value of MonthStartDte.
    /// </summary>
    [JsonPropertyName("monthStartDte")]
    public DateWorkArea MonthStartDte
    {
      get => monthStartDte ??= new();
      set => monthStartDte = value;
    }

    /// <summary>
    /// A value of NextMonth.
    /// </summary>
    [JsonPropertyName("nextMonth")]
    public DateWorkArea NextMonth
    {
      get => nextMonth ??= new();
      set => nextMonth = value;
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
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Line14.
    /// </summary>
    [JsonPropertyName("line14")]
    public Common Line14
    {
      get => line14 ??= new();
      set => line14 = value;
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

    private CsePerson ap;
    private Common collectionFound;
    private Common chOrArCaseRoleFound;
    private Collection collection;
    private Ocse157Verification null1;
    private Common invalidDate;
    private DateWorkArea afDiscontinueDt;
    private DateWorkArea monthEndDate;
    private DateWorkArea monthStartDte;
    private DateWorkArea nextMonth;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private DateWorkArea nullDate;
    private CsePerson prev;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common line14;
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
    /// A value of AnotherProgram.
    /// </summary>
    [JsonPropertyName("anotherProgram")]
    public Program AnotherProgram
    {
      get => anotherProgram ??= new();
      set => anotherProgram = value;
    }

    /// <summary>
    /// A value of AnotherCaseRole.
    /// </summary>
    [JsonPropertyName("anotherCaseRole")]
    public CaseRole AnotherCaseRole
    {
      get => anotherCaseRole ??= new();
      set => anotherCaseRole = value;
    }

    /// <summary>
    /// A value of AnotherCsePerson.
    /// </summary>
    [JsonPropertyName("anotherCsePerson")]
    public CsePerson AnotherCsePerson
    {
      get => anotherCsePerson ??= new();
      set => anotherCsePerson = value;
    }

    /// <summary>
    /// A value of SubsequentProgram.
    /// </summary>
    [JsonPropertyName("subsequentProgram")]
    public Program SubsequentProgram
    {
      get => subsequentProgram ??= new();
      set => subsequentProgram = value;
    }

    /// <summary>
    /// A value of SubsequentCsePerson.
    /// </summary>
    [JsonPropertyName("subsequentCsePerson")]
    public CsePerson SubsequentCsePerson
    {
      get => subsequentCsePerson ??= new();
      set => subsequentCsePerson = value;
    }

    /// <summary>
    /// A value of SubsequentCaseRole.
    /// </summary>
    [JsonPropertyName("subsequentCaseRole")]
    public CaseRole SubsequentCaseRole
    {
      get => subsequentCaseRole ??= new();
      set => subsequentCaseRole = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of SubsequentPersonProgram.
    /// </summary>
    [JsonPropertyName("subsequentPersonProgram")]
    public PersonProgram SubsequentPersonProgram
    {
      get => subsequentPersonProgram ??= new();
      set => subsequentPersonProgram = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CsePersonAccount obligor;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private Program anotherProgram;
    private CaseRole anotherCaseRole;
    private CsePerson anotherCsePerson;
    private Program subsequentProgram;
    private CsePerson subsequentCsePerson;
    private CaseRole subsequentCaseRole;
    private CsePersonAccount supported;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private Collection collection;
    private PersonProgram subsequentPersonProgram;
    private Program program;
    private PersonProgram personProgram;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseAssignment caseAssignment;
  }
#endregion
}
