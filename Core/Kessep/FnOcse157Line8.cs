// Program: FN_OCSE157_LINE_8, ID: 371284982, model: 746.
// Short name: SWE02974
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_8.
/// </summary>
[Serializable]
public partial class FnOcse157Line8: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_8 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line8(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line8.
  /// </summary>
  public FnOcse157Line8(IContext context, Import import, Export export):
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
    // 07/11/06  GVandy	WR00230751	Initial Development.
    // 11/03/06  GVandy	PR294769	Skip child if
    // 					1. paternity established <> Y
    // 					2. paternity established date is not duruing last calendar year
    // 					3. no J class legal action with EP legal detail established by CS or
    // CT
    // 01/19/07  GVandy	PR297812	Change how Line 8a determines the run number 
    // from the previous year.
    // 10/02/13  LSS	        CQ37588		Remove SSN from federal audit data -
    //                                         
    // SSN is not included in the federal
    // requirements for data submission.
    //                                         
    // Include Child Name in audit data per
    // federal data submission requirments
    //                                         
    // (FY2013)
    // 05/20/14  GVandy	CQ36717		Correct restart issue causing Line 08 to be 
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
      (Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 3), "08A"))
      
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "08 "))
      {
        local.Restart.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

        // -------------------------------------
        // Initialize counters for line 8
        // -------------------------------------
        local.Line8.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      }

      // ------------------------------------------------------
      // Read all cse_persons that meet our criteria.
      // -------------------------------------------------------
      foreach(var item in ReadCaseRoleCsePersonCase())
      {
        // -------------------------------------
        // Parse each Child only once.
        // -------------------------------------
        if (Equal(entities.CsePerson.Number, local.Prev.Number))
        {
          continue;
        }

        MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
          

        // -----------------------------------------------------
        // Skip if Pat Est Ind is not Y.
        // -----------------------------------------------------
        if (AsChar(entities.CsePerson.PaternityEstablishedIndicator) != 'Y')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Pat Est Ind is not Y.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------
        // Skip if BOW is not Y.
        // -----------------------------------------------------
        if (AsChar(entities.CsePerson.BornOutOfWedlock) != 'Y')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-BOW on CADS is not Y.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------
        // Skip if Birth Place is KS.
        // -----------------------------------------------------
        // -- 7/26/06  Per JRE, if birth_place_state is blank the person should 
        // be included in line 8.
        // The birth_place_state will be blank if the birthplace country is 
        // specified or if no birthplace information has been entered.
        if (Equal(entities.CsePerson.BirthPlaceState, "KS"))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Birth Place on CHDS is KS.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------
        // Skip if Pat Est is not within CY.
        // -----------------------------------------------------
        if (Lt(import.CalendarYearEndDate.Date,
          entities.CsePerson.DatePaternEstab) || Lt
          (entities.CsePerson.DatePaternEstab, import.CalendarYearStartDte.Date))
          
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Pat est date not within CY.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------
        // Skip if Date of Birth is not within reporting calendar year.
        // -----------------------------------------------------
        if (Lt(entities.CsePerson.CreatedTimestamp,
          AddYears(import.CalendarYearStartDte.Timestamp, -1)))
        {
          // To improve performance, skip the child if the created timestamp of 
          // the CSE_Person record is 2 years old or more.
          // (I.E. The child was born well before the reporting calendar year.
          // No need to make the adabas call to check the date of birth. )
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-DOB not within calendar year.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -- Check date of birth in adabas.
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "08";
          local.ForError.CaseNumber = entities.Case1.Number;
          local.ForError.SuppPersonNumber = entities.CsePerson.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }

        if (Lt(local.CsePersonsWorkSet.Dob, import.CalendarYearStartDte.Date) ||
          Lt(import.CalendarYearEndDate.Date, local.CsePersonsWorkSet.Dob))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-DOB not within calendar year.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // -----------------------------------------------------
        // Skip if the case is an incoming interstate case and the child has an 
        // active interstate program at the end of the calendar year.
        // (I.E. the case will be reported to the Feds by the other state)
        // -----------------------------------------------------
        if (ReadInterstateRequest())
        {
          if (ReadPersonProgram())
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.CsePerson.Number;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.LineNumber = "08";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Incoming Interstate w/ Pers Pgm.";
              UseFnCreateOcse157Verification();
            }

            continue;
          }
        }

        // -----------------------------------------------------
        // EP created_date doesn't matter. LDET may have been
        // created  after the end of CY. It's the Paternity_est_date that 
        // matters.
        // Remove references to legal_action_case_role.
        // Replace with legal_action_person.
        // 7/25 - AP must also be tied to ldet.
        // -----------------------------------------------------
        local.EpLdetFound.Flag = "N";

        foreach(var item1 in ReadLegalActionDetailLegalAction())
        {
          if (!Equal(entities.LegalAction.EstablishmentCode, "CS") && !
            Equal(entities.LegalAction.EstablishmentCode, "CT"))
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.CsePerson.Number;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;
              local.ForCreateOcse157Verification.LineNumber = "08";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-EP ldet estab by non IV-D.";
              UseFnCreateOcse157Verification();
            }

            continue;
          }

          local.EpLdetFound.Flag = "Y";

          break;
        }

        // ----------------------------------------------------
        // If EP ldet is nf. Don't count current record.
        // ---------------------------------------------------
        if (AsChar(local.EpLdetFound.Flag) == 'N')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.CsePerson.Number;
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "08";
            local.ForCreateOcse157Verification.Comment =
              "Error-Paternity est. EP ldet nf.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        UseFnB716FormatName();
        local.Prev.Number = entities.CsePerson.Number;
        ++local.Line8.Count;
        local.ForCreateOcse157Verification.LineNumber = "08";
        local.ForCreateOcse157Verification.Column = "a";
        local.ForCreateOcse157Verification.SuppPersonNumber =
          entities.CsePerson.Number;
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.DateOfBirth =
          local.CsePersonsWorkSet.Dob;
        local.ForCreateOcse157Verification.PlaceOfBirth =
          entities.CsePerson.BirthPlaceState;
        local.ForCreateOcse157Verification.ChildName =
          local.CsePersonsWorkSet.FormattedName;

        // ------------------------------------------------------------
        // If SSN is not retrieved from ADABAS or is not valid, it will be
        // defaulted to SPACES.
        // ------------------------------------------------------------
        // ------------------------------------------------------------------------------
        // CQ37588 SSN is not included in the federal requirements for audit 
        // file data submission
        // ------------------------------------------------------------------------------
        UseFnCreateOcse157Verification();
        ++local.CommitCnt.Count;

        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "08 " + entities
            .CsePerson.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (local.Line8.Count, 6, 10);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "08";
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
      }
    }

    // --  New logic for line 8a...
    local.Restart.Number = "";

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "08A"))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // --------------------------------------------------------------
      // Initialize counters
      // --------------------------------------------------------------
      local.Line8.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line8A.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
    }

    local.PreviousYear.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault() - 1;
    ReadOcse157Data();

    // -- Log the run number used for the prior year Line 8 to the control 
    // report.
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      Substring(NumberToString(
        local.PreviousYear.FiscalYear.GetValueOrDefault(), 12, 4) +
      " Line 8 Run Number.....................................................",
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

    foreach(var item in ReadOcse157Verification())
    {
      // -------------------------------------
      // Increment count for Line 8a
      // -------------------------------------
      ++local.Line8A.Count;
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
      local.ForCreateOcse157Verification.LineNumber = "08a";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.SuppPersonNumber =
        entities.Ocse157Verification.SuppPersonNumber;
      local.ForCreateOcse157Verification.CaseNumber =
        entities.Ocse157Verification.CaseNumber;
      local.ForCreateOcse157Verification.DateOfBirth =
        entities.Ocse157Verification.DateOfBirth;
      local.ForCreateOcse157Verification.PlaceOfBirth =
        entities.Ocse157Verification.PlaceOfBirth;
      local.ForCreateOcse157Verification.ChildName =
        entities.Ocse157Verification.ChildName;
      local.ForCreateOcse157Verification.ChildName =
        local.CsePersonsWorkSet.FormattedName;
      UseFnCreateOcse157Verification();
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "08A" + entities
          .Ocse157Verification.SuppPersonNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line8.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line8A.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "08a";
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
    // Create ocse157_data records and take final checkpoint.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "08";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line8.Count + import
      .Line8B.Number.GetValueOrDefault();
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "08a";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line8A.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "08b";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number =
      import.Line8B.Number.GetValueOrDefault();
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "09 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "08";
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
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
    target.ChildName = source.ChildName;
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
    target.DateOfBirth = source.DateOfBirth;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.GoodCauseEffDte = source.GoodCauseEffDte;
    target.Comment = source.Comment;
    target.ChildName = source.ChildName;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

  private void UseFnB716FormatName()
  {
    var useImport = new FnB716FormatName.Import();
    var useExport = new FnB716FormatName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(FnB716FormatName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
        db.SetString(command, "numb", local.Restart.Number);
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 7);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 9);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 10);
        entities.CsePerson.DatePaternEstab = db.GetDate(reader, 11);
        entities.CsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 12);
        entities.CsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 13);
        entities.CsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 14);
        entities.CsePerson.HospitalPatEstInd = db.GetNullableString(reader, 15);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 16);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
        db.SetDateTime(
          command, "createdTimestamp",
          import.CalendarYearEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "othStateClsDte",
          import.CalendarYearEndDate.Date.GetValueOrDefault());
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
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 2);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 3);
        entities.LegalAction.Classification = db.GetString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadOcse157Data()
  {
    local.PreviousYear.Populated = false;

    return Read("ReadOcse157Data",
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

  private IEnumerable<bool> ReadOcse157Verification()
  {
    entities.Ocse157Verification.Populated = false;

    return ReadEach("ReadOcse157Verification",
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
        entities.Ocse157Verification.PlaceOfBirth =
          db.GetNullableString(reader, 8);
        entities.Ocse157Verification.SocialSecurityNumber =
          db.GetNullableInt32(reader, 9);
        entities.Ocse157Verification.ChildName =
          db.GetNullableString(reader, 10);
        entities.Ocse157Verification.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.CalendarYearEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of CalendarYearEndDate.
    /// </summary>
    [JsonPropertyName("calendarYearEndDate")]
    public DateWorkArea CalendarYearEndDate
    {
      get => calendarYearEndDate ??= new();
      set => calendarYearEndDate = value;
    }

    /// <summary>
    /// A value of CalendarYearStartDte.
    /// </summary>
    [JsonPropertyName("calendarYearStartDte")]
    public DateWorkArea CalendarYearStartDte
    {
      get => calendarYearStartDte ??= new();
      set => calendarYearStartDte = value;
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
    /// A value of Line8B.
    /// </summary>
    [JsonPropertyName("line8B")]
    public Ocse157Data Line8B
    {
      get => line8B ??= new();
      set => line8B = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Common displayInd;
    private DateWorkArea calendarYearEndDate;
    private DateWorkArea calendarYearStartDte;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Ocse157Data line8B;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line8.
    /// </summary>
    [JsonPropertyName("line8")]
    public Common Line8
    {
      get => line8 ??= new();
      set => line8 = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of EpLdetFound.
    /// </summary>
    [JsonPropertyName("epLdetFound")]
    public Common EpLdetFound
    {
      get => epLdetFound ??= new();
      set => epLdetFound = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Line8A.
    /// </summary>
    [JsonPropertyName("line8A")]
    public Common Line8A
    {
      get => line8A ??= new();
      set => line8A = value;
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

    private Ocse157Verification previousYear;
    private AbendData abendData;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private Common line8;
    private CsePerson prev;
    private Ocse157Verification null1;
    private DateWorkArea nullDate;
    private Common epLdetFound;
    private Ocse157Verification forError;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common commitCnt;
    private Common line8A;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ApLegalActionPerson.
    /// </summary>
    [JsonPropertyName("apLegalActionPerson")]
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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

    private Ocse157Data ocse157Data;
    private CsePerson csePerson;
    private Case1 case1;
    private PersonProgram personProgram;
    private Program program;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
    private Ocse157Verification ocse157Verification;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson apLegalActionPerson;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
