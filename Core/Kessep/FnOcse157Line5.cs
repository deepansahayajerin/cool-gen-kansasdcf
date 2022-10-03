// Program: FN_OCSE157_LINE_5, ID: 371092704, model: 746.
// Short name: SWE02914
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_5.
/// </summary>
[Serializable]
public partial class FnOcse157Line5: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_5 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line5(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line5.
  /// </summary>
  public FnOcse157Line5(IContext context, Import import, Export export):
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
    // ??/??/??  KDoshi			Initial Development
    // 07/31/01				Only read GC code to determine good cause.
    // 08/02/01				To determine if Good Cause is active, look for active GC 
    // records but
    // 					where there is no CO created aftert the GC record.
    // 10/30/01				Strip line 7 logic. This is done in a seperate CAB now.
    // 					Rename this CAB.
    // 					Add BOW=Y criteria to driving read each instead of
    // 					checking it inside the loop.
    // 04/15/01				Add childs's date of birth per DCL 02-23.
    // 03/09/06  GVandy	WR00230751	Federally mandated changes.
    // 					Add new subline 5a (line 5 from previous FY).
    // 01/19/07  GVandy	PR297812	Change how Line 5a determines the run number 
    // from the previous year.
    // 
    // 10/14/08  GVandy	CQ7393		Include last 4 digits of child SSN in federal
    // audit data.
    // 10/02/13  LSS	        CQ37588		Remove last 4 digits of child SSN from 
    // federal audit data.
    //                                         
    // SSN is not included in the federal
    // requirements for data submission.
    // 05/20/14  GVandy	CQ36717		Correct restart issue causing Line 05 to be 
    // skipped.
    // -------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) != 'Y' || AsChar
      (import.ProgramCheckpointRestart.RestartInd) == 'Y' && Lt
      (Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 3), "05A"))
      
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "05 "))
      {
        local.Restart.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

        // --------------------------------------------------------------
        // Initialize counters
        // --------------------------------------------------------------
        local.Line5.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      }

      // -------------------------------------------------------------------
      // Read CH case roles that are active at some point during the FY.
      // Driving READ EACH construct is cloned from line 4 cab.
      // -------------------------------------------------------------------
      foreach(var item in ReadCaseRoleCsePersonCase())
      {
        // -------------------------------------
        // Count each Child only once.
        // -------------------------------------
        if (Equal(entities.CsePerson.Number, local.Prev.Number))
        {
          continue;
        }

        MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
          

        // ---------------------------------------------------
        // Was this Child reported in line 4? If not, skip.
        // ---------------------------------------------------
        ReadOcse157Data1();
        ReadOcse157Verification1();

        if (IsEmpty(local.MinimumCsePerson.Number))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "05";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-child not included in line 4.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------------------
        // 6/17/2001
        // If child is active on muliple cases during FY, then count child
        // if there is atleast one case with no active Good Cause as of
        // the end of FY.
        // Read below looks for current Case only. This is okay since
        // we will parse through this logic again for next Case.
        // ----------------------------------------------------------------
        // ---------------------------------------------------------
        // 07/31/2001
        // Only read GC code to determine good cause.
        // ---------------------------------------------------------
        // ----------------------------------------------------------------------
        // Possible values for Good Cause Code are.
        // PE-Good Cause Pending
        // GC-Good Cause
        // CO-Cooperating
        // Users 'never' end GC records when establishing CO
        // records. Infact, there are no 'closed' entries on Good Cause
        // table as of 8/2.
        // So, to determine if Good Cause is active, look for active GC
        // records where there is no CO created after the GC record.
        // ----------------------------------------------------------------------
        foreach(var item1 in ReadGoodCauseCaseRole())
        {
          // ---------------------------------------------------------------------
          // Ensure there is no CO record that is created 'after' GC
          // record but before FY end.
          // ---------------------------------------------------------------------
          if (ReadGoodCause())
          {
            continue;
          }
          else
          {
            // ---------------------------------------------------------------------
            // So, GC must be still active.
            // ---------------------------------------------------------------------
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.GoodCauseEffDte =
              entities.GoodCause.EffectiveDate;
            local.ForCreateOcse157Verification.LineNumber = "05";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Good Cause is active.";
            UseFnCreateOcse157Verification();
          }

          goto ReadEach;
        }

        local.Prev.Number = entities.CsePerson.Number;
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "05";
          local.ForError.CaseNumber = entities.Case1.Number;
          local.ForError.SuppPersonNumber = entities.CsePerson.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }

        // -------------------------------------
        // Increment count for Line 5
        // -------------------------------------
        ++local.Line5.Count;
        local.ForCreateOcse157Verification.LineNumber = "05";
        local.ForCreateOcse157Verification.Column = "a";
        local.ForCreateOcse157Verification.SuppPersonNumber =
          entities.CsePerson.Number;
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.DateOfBirth =
          local.CsePersonsWorkSet.Dob;

        // 10/14/08  GVandy  CQ7393  Include last 4 digits of child SSN in 
        // federal audit data.
        // 10/02/13  LSS     CQ37588  Remove last 4 digits of child SSN from 
        // federal audit data.
        UseFnCreateOcse157Verification();
        ++local.CommitCnt.Count;

        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "05 " + entities
            .CsePerson.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (local.Line5.Count, 6, 10);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "05";
            local.ForError.CaseNumber = entities.Case1.Number;
            local.ForError.SuppPersonNumber = entities.CsePerson.Number;
            UseOcse157WriteError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }
        }

ReadEach:
        ;
      }
    }

    // --  New logic for line 5a...
    local.Restart.Number = "";

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "05A"))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // --------------------------------------------------------------
      // Initialize counters
      // --------------------------------------------------------------
      local.Line5.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line5A.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
    }

    local.PreviousYear.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault() - 1;
    ReadOcse157Data2();

    // -- Log the run number used for the prior year Line 5 to the control 
    // report.
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      Substring(NumberToString(
        local.PreviousYear.FiscalYear.GetValueOrDefault(), 12, 4) +
      " Line 5 Run Number.....................................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (local.PreviousYear.RunNumber.GetValueOrDefault(), 14, 2);
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      export.Abort.Flag = "Y";
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    foreach(var item in ReadOcse157Verification2())
    {
      // -------------------------------------
      // Increment count for Line 5a
      // -------------------------------------
      ++local.Line5A.Count;
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
      local.ForCreateOcse157Verification.LineNumber = "05a";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.SuppPersonNumber =
        entities.Ocse157Verification.SuppPersonNumber;
      local.ForCreateOcse157Verification.CaseNumber =
        entities.Ocse157Verification.CaseNumber;
      local.ForCreateOcse157Verification.DateOfBirth =
        entities.Ocse157Verification.DateOfBirth;
      UseFnCreateOcse157Verification();
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "05A" + entities
          .Ocse157Verification.SuppPersonNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line5.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line5A.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "05a";
          local.ForError.CaseNumber = entities.Ocse157Verification.CaseNumber;
          local.ForError.SuppPersonNumber =
            entities.Ocse157Verification.SuppPersonNumber;
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
    local.ForCreateOcse157Data.LineNumber = "05";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line5.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "05a";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line5A.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "06 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "05";
      local.ForError.CaseNumber = "";
      local.ForError.SuppPersonNumber = "";
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
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
    target.Child4DigitSsn = source.Child4DigitSsn;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
    target.Child4DigitSsn = source.Child4DigitSsn;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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

  private IEnumerable<bool> ReadCaseRoleCsePersonCase()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.Restart.Number);
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 8);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 9);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 10);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.Ar.Populated);
    entities.Next.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Ar.CasNumber);
        db.SetString(command, "cspNumber", entities.Ar.CspNumber);
        db.SetString(command, "croType", entities.Ar.Type1);
        db.SetInt32(command, "croIdentifier", entities.Ar.Identifier);
        db.SetDateTime(
          command, "createdTimestamp1",
          entities.GoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Next.Code = db.GetNullableString(reader, 0);
        entities.Next.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.Next.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Next.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Next.CasNumber = db.GetString(reader, 4);
        entities.Next.CspNumber = db.GetString(reader, 5);
        entities.Next.CroType = db.GetString(reader, 6);
        entities.Next.CroIdentifier = db.GetInt32(reader, 7);
        entities.Next.Populated = true;
        CheckValid<GoodCause>("CroType", entities.Next.CroType);
      });
  }

  private IEnumerable<bool> ReadGoodCauseCaseRole()
  {
    entities.GoodCause.Populated = false;
    entities.Ar.Populated = false;

    return ReadEach("ReadGoodCauseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.Ar.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.Ar.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.Ar.Type1 = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.Ar.Identifier = db.GetInt32(reader, 7);
        entities.Ar.StartDate = db.GetNullableDate(reader, 8);
        entities.Ar.EndDate = db.GetNullableDate(reader, 9);
        entities.GoodCause.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);

        return true;
      });
  }

  private bool ReadOcse157Data1()
  {
    local.Max.Populated = false;

    return Read("ReadOcse157Data1",
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

  private bool ReadOcse157Data2()
  {
    local.PreviousYear.Populated = false;

    return Read("ReadOcse157Data2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          local.PreviousYear.FiscalYear.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.PreviousYear.RunNumber = db.GetNullableInt32(reader, 0);
        local.PreviousYear.Populated = true;
      });
  }

  private bool ReadOcse157Verification1()
  {
    local.MinimumCsePerson.Populated = false;

    return Read("ReadOcse157Verification1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber", local.Max.RunNumber.GetValueOrDefault());
        db.SetNullableString(
          command, "suppPersonNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.MinimumCsePerson.Number = db.GetString(reader, 0);
        local.MinimumCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOcse157Verification2()
  {
    entities.Ocse157Verification.Populated = false;

    return ReadEach("ReadOcse157Verification2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          local.PreviousYear.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber",
          local.PreviousYear.RunNumber.GetValueOrDefault());
        db.
          SetNullableString(command, "suppPersonNumber1", local.Restart.Number);
          
        db.SetNullableString(
          command, "suppPersonNumber2", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber3", import.To.SuppPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Ocse157Verification.FiscalYear =
          db.GetNullableInt32(reader, 0);
        entities.Ocse157Verification.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Verification.LineNumber =
          db.GetNullableString(reader, 2);
        entities.Ocse157Verification.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Verification.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.Ocse157Verification.CaseNumber =
          db.GetNullableString(reader, 5);
        entities.Ocse157Verification.SuppPersonNumber =
          db.GetNullableString(reader, 6);
        entities.Ocse157Verification.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.Ocse157Verification.Child4DigitSsn =
          db.GetNullableString(reader, 8);
        entities.Ocse157Verification.Populated = true;

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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
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
    /// A value of PreviousYear.
    /// </summary>
    [JsonPropertyName("previousYear")]
    public Ocse157Verification PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of Line5A.
    /// </summary>
    [JsonPropertyName("line5A")]
    public Common Line5A
    {
      get => line5A ??= new();
      set => line5A = value;
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
    /// A value of MinimumCsePerson.
    /// </summary>
    [JsonPropertyName("minimumCsePerson")]
    public CsePerson MinimumCsePerson
    {
      get => minimumCsePerson ??= new();
      set => minimumCsePerson = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line5.
    /// </summary>
    [JsonPropertyName("line5")]
    public Common Line5
    {
      get => line5 ??= new();
      set => line5 = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of MinimumCase.
    /// </summary>
    [JsonPropertyName("minimumCase")]
    public Case1 MinimumCase
    {
      get => minimumCase ??= new();
      set => minimumCase = value;
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
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private Ocse157Verification previousYear;
    private Common line5A;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson minimumCsePerson;
    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private Common line5;
    private CsePerson prev;
    private Ocse157Verification null1;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data max;
    private Case1 minimumCase;
    private Common commitCnt;
    private Ocse157Data forCreateOcse157Data;
    private Ocse157Verification forError;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public GoodCause Next
    {
      get => next ??= new();
      set => next = value;
    }

    private CsePerson csePerson;
    private Ocse157Data ocse157Data;
    private Ocse157Verification ocse157Verification;
    private Case1 case1;
    private CaseRole caseRole;
    private GoodCause goodCause;
    private CaseRole ar;
    private GoodCause next;
  }
#endregion
}
