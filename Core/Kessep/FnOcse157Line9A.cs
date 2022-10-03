// Program: FN_OCSE157_LINE_9A, ID: 371092721, model: 746.
// Short name: SWE02931
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_9A.
/// </summary>
[Serializable]
public partial class FnOcse157Line9A: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_9A program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line9A(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line9A.
  /// </summary>
  public FnOcse157Line9A(IContext context, Import import, Export export):
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
    // 07/20/01  ?????				EP created_date doesn't matter. LDET may have been 
    // setup
    // 					after the end of CY. What matters is Paternity_est_date.
    // 07/21/01  ?????				Remove legal_action_case_role references.
    // 					Replace with legal_action_person.
    // 					Add call to ADABAS cab to retrieve DOB and SSN.
    // 07/25/01  ?????				AP must also be tied to EP ldet.
    // 07/31/01  ?????				Add code to exclude cases where good cause is active.
    // 					Only read GC code to determine good cause.
    // 08/02/01  ?????				To determine if Good Cause is active, look for active 
    // GC
    // 					records but where there is no CO created after the GC record.
    // 04/15/02  ?????		PR141288	Exclude children where BC signed flag = Y.
    // 08/14/02  ?????		PR153845	Exclude children where BC signed flag = SPACES 
    // and father's name
    // 					has been entered on BC..
    // 09/04/03  ?????		 PR186578	Commented out code that was writting a row to 
    // the verification table
    // 					with a comment of 'Error - Paternity est. EP ldet nf' as this should
    // 					not have been written to the table just to the error report which it
    // was
    // 					also doing.
    // 10/03/03  ?????		PR189430	Added code to populate the date paternity 
    // established in the table
    // 07/11/06  GVandy	WR00230751	Federally mandated changes.
    // 					1) Skip child if the Hospital Paternity Acknowledged Indicator = Y.
    // 					2) Skip child if the case is incoming interstate and the child
    // 					   has an active interstate program.
    // 					3) Remove restriction that child must be born in Kansas.
    // 					4) Don't count if EP legal detail established by other state (OS).
    // 11/01/06  GVandy	PR294583	Look at each case a child is associated with 
    // when reading for EP legal details.
    // 10/14/08  GVandy	CQ7393		Include last 4 digits of child SSN in federal 
    // audit data
    // 10/02/13  LSS	        CQ37588		Remove SSN from federal audit data -
    //                                         
    // SSN is not included in the federal
    // requirements for data submission.
    //                                         
    // Include Child Name in audit data per
    // federal data submission requirments
    //                                         
    // (FY2013)
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "09 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // -------------------------------------
      // Initialize counters for line 9a
      // -------------------------------------
      local.Line9A.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    // ------------------------------------------------------
    // Read all cse_persons that meet our criteria for paternity.
    // Case Role need not be open during CY.
    // Case need not be Open during CY.
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
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
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
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-BOW on CADS is not Y.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // -----------------------------------------------------
      // Skip if Hospital Paternity Establishment = 'Y'.
      // -----------------------------------------------------
      if (AsChar(entities.CsePerson.HospitalPatEstInd) == 'Y')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Hospital Paternity Estab is Y.";
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
            local.ForCreateOcse157Verification.LineNumber = "09a";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-Incoming Interstate w/ Pers Pgm.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }
      }

      // -----------------------------------------------------
      // Skip if Pat Est is not within CY.
      // -----------------------------------------------------
      if (Lt(import.CalendarYearEndDate.Date, entities.CsePerson.DatePaternEstab)
        || Lt
        (entities.CsePerson.DatePaternEstab, import.CalendarYearStartDte.Date))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Pat est date not within CY.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // -----------------------------------------------------
      // Skip child if emancipation date is set and is before the start of FY.
      // -----------------------------------------------------
      // @@@  Per e-mail from Brian, children who emancipated during the CY 
      // should be counted.
      // The IF statement below needs to be changed to check if the date of 
      // emancipation is less
      // than import_calendar_year_start_date not ...end_date.
      if (Lt(local.NullDate.Date, entities.CaseRole.DateOfEmancipation) && Lt
        (entities.CaseRole.DateOfEmancipation, import.CalendarYearStartDte.Date))
        
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-emancipated child.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // -----------------------------------------------------
      // 4/14/2002 - Skip if BC is signed.
      // -----------------------------------------------------
      if (AsChar(entities.CsePerson.BirthCertificateSignature) == 'Y')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Birth certificate is signed.";
          UseFnCreateOcse157Verification();
        }

        // ----------------------------------------------------------------
        // 8/14/2002 - Skip if BC signed flag=SPACES and father's
        // name is entered on BC.
        // ----------------------------------------------------------------
        continue;
      }
      else if (IsEmpty(entities.CsePerson.BirthCertificateSignature) && (
        !IsEmpty(entities.CsePerson.BirthCertFathersLastName) || !
        IsEmpty(entities.CsePerson.BirthCertFathersFirstName)))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.CsePerson.Number;
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Father's name on Birth Cert.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // -----------------------------------------------------
      // EP created_date doesn't matter. LDET may have been
      // created  after the end of CY. It's the Paternity_est_date that matters.
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
            local.ForCreateOcse157Verification.LineNumber = "09a";
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
      // If EP ldet is nf, write error. Don't count current record.
      // ---------------------------------------------------
      if (AsChar(local.EpLdetFound.Flag) == 'N')
      {
        ExitState = "FN_PATERNITY_EST_BUT_EP_LDET_NF";
        local.ForError.LineNumber = "09a";
        local.ForError.CaseNumber = entities.Case1.Number;
        local.ForError.SuppPersonNumber = entities.CsePerson.Number;
        UseOcse157WriteError();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Abort.Flag = "Y";

          return;
        }

        continue;
      }

      // ---------------------------------------------------------
      // 07/31/2001
      // Add code to exclude cases where good cause is active.
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
        // Ensure there is no CO record that is created 'after' GC
        // record but before the CY end.
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
          local.ForCreateOcse157Verification.GoodCauseEffDte =
            entities.GoodCause.EffectiveDate;
          local.ForCreateOcse157Verification.LineNumber = "09a";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Good Cause is active.";
          UseFnCreateOcse157Verification();
        }

        goto ReadEach;
      }

      // -- 11/01/06  GVandy  PR294583  Look at each case a child is associated 
      // with when reading for EP legal details.
      local.Prev.Number = entities.CsePerson.Number;

      // ----------------------------------------------------
      // Get SSN and DOB from ADABAS.
      // NB - Birth Place State is read from Cse_person table.
      // ---------------------------------------------------
      // ---------------------------------------------------------------------------
      // CQ37588 SSN is not included in the required data for audit file 
      // submission.
      // ---------------------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseCabReadAdabasPersonBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ForError.LineNumber = "09a";
        local.ForError.CaseNumber = entities.Case1.Number;
        local.ForError.SuppPersonNumber = entities.CsePerson.Number;
        UseOcse157WriteError();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Abort.Flag = "Y";

          return;
        }
      }

      UseFnB716FormatName();
      ++local.Line9A.Count;
      local.ForCreateOcse157Verification.LineNumber = "09a";
      local.ForCreateOcse157Verification.Column = "a";
      local.ForCreateOcse157Verification.SuppPersonNumber =
        entities.CsePerson.Number;
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
      local.ForCreateOcse157Verification.CourtOrderNumber =
        entities.LegalAction.StandardNumber;
      local.ForCreateOcse157Verification.DateOfBirth =
        local.CsePersonsWorkSet.Dob;
      local.ForCreateOcse157Verification.PlaceOfBirth =
        entities.CsePerson.BirthPlaceState;
      local.ForCreateOcse157Verification.ChildName =
        local.CsePersonsWorkSet.FormattedName;

      // ------------------------------------------------------------------------------
      // CQ37588 SSN is not included in the federal requirements for audit file 
      // data submission
      // ------------------------------------------------------------------------------
      // -----------------------------------------------------------------------------------------------------------
      // 10/03/2003. PR189430
      // 
      // Added code to populate the date paternity established in the table.
      // 
      // ----------------------------------------------------------------------------------------------
      local.ForCreateOcse157Verification.DtePaternityEst =
        entities.CsePerson.DatePaternEstab;

      // ------------------------------------------------------------
      // If SSN is not retrieved from ADABAS or is not valid, it will be
      // defaulted to SPACES.
      // ------------------------------------------------------------
      // ------------------------------------------------------------------------------
      // CQ37588 SSN is not included in the federal requirements for audit file 
      // data submission
      // ------------------------------------------------------------------------------
      UseFnCreateOcse157Verification();
      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "09 " + entities
          .CsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line9A.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "09a";
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
    local.ForCreateOcse157Data.LineNumber = "09a";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line9A.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "12 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "09a";
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
    target.Child4DigitSsn = source.Child4DigitSsn;
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
    target.Child4DigitSsn = source.Child4DigitSsn;
    target.ChildName = source.ChildName;
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
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

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
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
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
        entities.CaseRole.Populated = true;
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
          import.CalendarYearEndDate.Timestamp.GetValueOrDefault());
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
          import.CalendarYearEndDate.Date.GetValueOrDefault());
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
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

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
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

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
    /// A value of CalendarYearStartDte.
    /// </summary>
    [JsonPropertyName("calendarYearStartDte")]
    public DateWorkArea CalendarYearStartDte
    {
      get => calendarYearStartDte ??= new();
      set => calendarYearStartDte = value;
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

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private DateWorkArea calendarYearStartDte;
    private DateWorkArea calendarYearEndDate;
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
    /// A value of EpLdetFound.
    /// </summary>
    [JsonPropertyName("epLdetFound")]
    public Common EpLdetFound
    {
      get => epLdetFound ??= new();
      set => epLdetFound = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Line9A.
    /// </summary>
    [JsonPropertyName("line9A")]
    public Common Line9A
    {
      get => line9A ??= new();
      set => line9A = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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

    private Common epLdetFound;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Ocse157Verification null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private Common line9A;
    private CsePerson prev;
    private DateWorkArea nullDate;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of ApLegalActionPerson.
    /// </summary>
    [JsonPropertyName("apLegalActionPerson")]
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
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

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private Program program;
    private PersonProgram personProgram;
    private InterstateRequest interstateRequest;
    private CsePerson apCsePerson;
    private LegalActionPerson apLegalActionPerson;
    private CaseRole apCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseRole ch;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private GoodCause goodCause;
    private CaseRole ar;
    private GoodCause next;
  }
#endregion
}
