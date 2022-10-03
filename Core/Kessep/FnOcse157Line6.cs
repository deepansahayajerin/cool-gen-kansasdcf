// Program: FN_OCSE157_LINE_6, ID: 371092712, model: 746.
// Short name: SWE02930
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_6.
/// </summary>
[Serializable]
public partial class FnOcse157Line6: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_6 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line6(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line6.
  /// </summary>
  public FnOcse157Line6(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // 7/29/2001
    // Reset local_for_create view after each parse.
    // ---------------------------------------------
    // ----------------------------------------------------
    // 07/31/2001
    // Only read GC code to determine good cause.
    // ----------------------------------------------------
    // ---------------------------------------------------------------------
    // 8/2/2001.
    // To determine if Good Cause is active, look for active GC
    // records but where there is no CO created after the GC record.
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // 8/16/2001.
    // Create verification record for audit (even if display ind <> Y)
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // 10/12/2001.
    // Count only if BOW=Y and Pat Est Ind =Y
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // 10/30/2001.
    // At the end of line 6 processing, set restart line # to 07.
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // 04/15/2003.
    // Add paternity established date per DCL 02-23.
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // 10/14/2008  GVandy  CQ7393
    // Include last 4 digits of child SSN in federal audit data.
    // ---------------------------------------------------------------------
    // 10/02/13 LSS     CQ37588
    // Remove last 4 digits of child SSN from federal audit data.
    // SSN is not included in the federal requirements for data submission.
    // ---------------------------------------------------------------------
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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "06 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // -------------------------------------
      // Initialize counters for line 6
      // -------------------------------------
      local.Line6.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    // -------------------------------------------------------------------
    // Read CH case roles that are active at some point during FY.
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

      // -----------------------------------------------------
      // Skip child if emancipation date is set and is before the start of FY.
      // -----------------------------------------------------
      if (Lt(local.NullDate.Date, entities.CaseRole.DateOfEmancipation) && Lt
        (entities.CaseRole.DateOfEmancipation, import.ReportStartDate.Date))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.LineNumber = "06";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-emancipated child.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // ----------------------------------------------------
      // Skip if case is not open at some point during the FY.
      // ----------------------------------------------------
      ReadCaseAssignment();

      if (!entities.CaseAssignment.Populated)
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "06";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case assignment is not open.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // ---------------------------------------------------------
      // Exclude children where Good Cause is active for AR as of end of FY.
      // --------------------------------------------------------
      // ---------------------------------------------------------
      // 6/17/2001
      // If child is active on muliple cases during FY, then count child
      // if there is atleast one case with no active Good Cause as of
      // the end of FY.
      // Read below looks for current Case only. This is okay since
      // we will parse through this logic again for next Case.
      // --------------------------------------------------------
      // ---------------------------------------------------------
      // 07/31/2001
      // Only read GC code to determine good cause.
      // --------------------------------------------------------
      // ---------------------------------------------------------------------------
      // Possible values for Good Cause Code are.
      // PE-Good Cause Pending
      // GC-Good Cause
      // CO-Cooperating
      // Users 'never' end GC records when establishing CO
      // records. Infact, there are no 'closed' entries on Good Cause
      // table as of 8/2.
      // So, to determine if Good Cause is active, look for active GC
      // records where there is no CO created after the GC record.
      // --------------------------------------------------------------------------
      foreach(var item1 in ReadGoodCauseCaseRole())
      {
        // ---------------------------------------------------------------------
        // Ensure there is no CO record that is created after the GC
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
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "06";
          local.ForCreateOcse157Verification.CaseAsinEffDte =
            entities.CaseAssignment.EffectiveDate;
          local.ForCreateOcse157Verification.CaseAsinEndDte =
            entities.CaseAssignment.DiscontinueDate;
          local.ForCreateOcse157Verification.GoodCauseEffDte =
            entities.GoodCause.EffectiveDate;
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Good Cause active.";
          UseFnCreateOcse157Verification();
        }

        goto ReadEach;
      }

      local.Prev.Number = entities.CsePerson.Number;

      // 10/14/08  GVandy  CQ7393  Include last 4 digits of child SSN in federal
      // audit data.  Retrieve the SSN from Adabas.
      // 10/02/13  LSS     CQ37588  Remove last 4 digits of child SSN from 
      // federal audit data.
      // -----------------------------------------------------------
      // All conditions are satisifed. Count Child.
      // -----------------------------------------------------------
      ++local.Line6.Count;
      local.ForCreateOcse157Verification.SuppPersonNumber =
        entities.CsePerson.Number;
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
      local.ForCreateOcse157Verification.LineNumber = "06";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.DtePaternityEst =
        entities.CsePerson.DatePaternEstab;

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.CaseAsinEffDte =
          entities.CaseAssignment.EffectiveDate;
        local.ForCreateOcse157Verification.CaseAsinEndDte =
          entities.CaseAssignment.DiscontinueDate;
      }

      UseFnCreateOcse157Verification();

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "06 " + entities
          .CsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line6.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "06";
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

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "06";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line6.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "07 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "06";
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
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
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
    target.DtePaternityEst = source.DtePaternityEst;
    target.CourtOrderNumber = source.CourtOrderNumber;
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

  private IEnumerable<bool> ReadCaseRoleCsePersonCase()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

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
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 10);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 11);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Common displayInd;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Line6.
    /// </summary>
    [JsonPropertyName("line6")]
    public Common Line6
    {
      get => line6 ??= new();
      set => line6 = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Ocse157Verification null1;
    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private DateWorkArea nullDate;
    private CsePerson prev;
    private Common line6;
    private Ocse157Verification forCreateOcse157Verification;
    private Common commitCnt;
    private Ocse157Data forCreateOcse157Data;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Case1 case1;
    private GoodCause goodCause;
    private CaseRole ar;
    private GoodCause next;
  }
#endregion
}
