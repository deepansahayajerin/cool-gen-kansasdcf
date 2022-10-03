// Program: FN_OCSE157_LINE_24, ID: 371113624, model: 746.
// Short name: SWE02963
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_24.
/// </summary>
[Serializable]
public partial class FnOcse157Line24: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_24 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line24(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line24.
  /// </summary>
  public FnOcse157Line24(IContext context, Import import, Export export):
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
    // 08/23/01  KDoshi			Initial version
    // 					Cloned from fn_ocse157_line_24_old.
    // 					This CAB writes verification record for each debt/collection.
    // 08/07/01				Skip debts before earliest case role date (as opposed to
    // 					earliest case assignment date)
    // 					Skip debts created after FY end.
    // 08/10/01				Fix Collection READ to add Ob_type qualification.
    // 08/11/01				Use correct flag to determine assistance.
    // 					Correct checkpoint logic to use supp cse_person (instead of prev).
    // 08/30/01				Include CSENet collections.
    // 09/06/01				Add AP number to verification data.
    // 10/02/01				Report correct court order #
    // 08/16/02				Skip Debt Adj that were made to close a case.
    // 07/31/06  GVandy	WR00230751	Include collection amounts created during the
    // reporting
    // 					period for debts due in the month following the end
    // 					of the reporting period.
    // 04/14/08  GVandy	CQ#2461		Per federal data reliability audit, provide 
    // case
    // 					number in audit data.  Use the debt detail due
    // 					date or collection created date to find the
    // 					appropriate case.
    // 08/28/13  LSS           CQ39887         OCSE157 Report modifications per 
    // the 2012 DRA (Data
    // 					Reliability Audit) requirements / findings
    //                                         
    // Line 24 -
    //                                            
    // 1) Do not include furture collection amount
    // when
    // 					      the collection created date is within September
    // 					      of the current reporting period but the debt
    // 					      detail due date is October in the next reporting
    // 					      period (DRA Audit Sample #006)
    // 09/16/2014  GVandy	CQ45651		Include closed case (CLOSECA) adjustments if 
    // they
    // 					are not the final adjustment in the fiscal year.
    // -------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "24";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "24 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // --------------------------------------------------------------
      // Initialize counters
      // ---------------------------------------------------------------
      local.Line24Curr.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 15)) / (
          decimal)100;
      local.Line24Former.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 29, 15)) / (
          decimal)100;
      local.Line24Never.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 15)) / (
          decimal)100;
    }

    // -------------------------------------------------------------------
    // Read Each is sorted in Asc order of Supp Person #.
    // Maintain a running total for each Supp person and then
    // process a break in person #. This is necessary so we only
    // read Assistance type once per Supp person (as opposed to
    // once per Debt or Collection)
    // -------------------------------------------------------------------
    foreach(var item in ReadCsePersonSupported())
    {
      if (Equal(entities.Supp.Number, local.PrevCsePerson.Number))
      {
        continue;
      }

      local.PrevCsePerson.Number = entities.Supp.Number;
      ++local.CommitCnt.Count;
      UseFnGetEarliestCaseRole4Pers();

      // -------------------------------------------------------------------
      // Skip if supp person is not setup as CH/AR on a case.
      // -------------------------------------------------------------------
      if (Equal(local.Earliest.StartDate, local.NullDateWorkArea.Date))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.SuppPersonNumber =
            entities.Supp.Number;
          local.ForCreateOcse157Verification.Comment =
            "Skipped-No AR or CH role.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      // ----------------------------
      // Clear local views
      // ----------------------------
      MoveOcse157Verification3(local.NullOcse157Verification,
        local.ForCreateOcse157Verification);

      // -------------------------------------------------------------------
      // Determine Current, Former or Never
      // -------------------------------------------------------------------
      UseFn157GetAssistanceForPerson();

      // -------------------------------------------------------------------
      // -Read Accruing debts that are 'due during' FY
      // -Skip debts due before supp person was assigned to Case
      // -Skip debts created after FY end
      // -------------------------------------------------------------------
      foreach(var item1 in ReadDebtObligationObligationTypeDebtDetailCsePerson())
        
      {
        // -------------------------------------------------------------------
        // Include first obligation in a J/S situation. Skip others.
        // -------------------------------------------------------------------
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
        {
          if (!ReadObligationRln())
          {
            continue;
          }
        }

        // -------------------------------------------------------------
        // NB- There are 2 relationships between Obligation and LA.
        // One is direct, second reln is via LAD. Both relationships are
        // maintained for Accruing Obligations. For faster access we will
        // use the direct relationship.
        // ------------------------------------------------------------
        if (!ReadLegalAction())
        {
          // ------------------------------------------------------------
          // We should always find a legal action on Accruing
          // obligations. However, reationship is defined as optional.
          // Set SPACES for court order if LA is nf.
          // ------------------------------------------------------------
        }

        // ----------------------------------------------------------------------------------------------------------------------------
        // 04/14/08  GVandy  CQ#2461  Per federal data reliability audit, 
        // provide case number in audit data.  Use the debt detail
        // due date to find the appropriate case.
        // ----------------------------------------------------------------------------------------------------------------------------
        local.DateWorkArea.Date = entities.DebtDetail.DueDt;
        local.ForCreateOcse157Verification.CaseNumber =
          UseFnOcse157FindCaseForAudit();
        local.ForCreateOcse157Verification.DebtDetailDueDt =
          entities.DebtDetail.DueDt;
        local.ForCreateOcse157Verification.ObTranAmount = entities.Debt.Amount;
        local.ForCreateOcse157Verification.SuppPersonNumber =
          entities.Supp.Number;
        local.ForCreateOcse157Verification.ObligorPersonNbr =
          entities.Ap.Number;
        local.ForCreateOcse157Verification.CourtOrderNumber =
          entities.LegalAction.StandardNumber;

        switch(AsChar(local.Assistance.Flag))
        {
          case 'C':
            local.Line24Curr.Currency152 += entities.Debt.Amount;
            local.ForCreateOcse157Verification.Column = "b";

            break;
          case 'F':
            local.Line24Former.Currency152 += entities.Debt.Amount;
            local.ForCreateOcse157Verification.Column = "c";

            break;
          default:
            local.Line24Never.Currency152 += entities.Debt.Amount;
            local.ForCreateOcse157Verification.Column = "d";

            break;
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.ObTranSgi =
            entities.Debt.SystemGeneratedIdentifier;
          local.ForCreateOcse157Verification.ObligationSgi =
            entities.Obligation.SystemGeneratedIdentifier;
          local.ForCreateOcse157Verification.ObTypeSgi =
            entities.ObligationType.SystemGeneratedIdentifier;
        }

        UseFnCreateOcse157Verification();

        // -- 09/16/2014  GVandy  CQ45651  Include closed case (CLOSECA, 
        // system_generated_id=9) adjustments
        //    if they are not the final adjustment in the fiscal year.
        //    Restructured the READ EACH below to read all the adjustments 
        // sorted descending by the
        //    adjustment date.  If the first adjustment found is CLOSECA then 
        // don't include the amount
        //    in line 24.
        //    Original logic is commented out below.
        local.MostRecentAdjustment.Flag = "Y";

        foreach(var item2 in ReadDebtAdjustmentObligationTransactionRlnRsn())
        {
          if (AsChar(local.MostRecentAdjustment.Flag) == 'Y')
          {
            local.MostRecentAdjustment.Flag = "N";

            if (entities.ObligationTransactionRlnRsn.
              SystemGeneratedIdentifier == 9)
            {
              // -- 09/16/2014  GVandy  CQ45651  Include closed case (CLOSECA, 
              // system_generated_id=9) adjustments
              //    if they are not the final adjustment in the fiscal year.
              continue;
            }
          }

          // ------------------------------------------------------------------
          // I type adj will increase original debt amt. D type decreases it.
          // ------------------------------------------------------------------
          if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
          {
            local.ForCreateOcse157Verification.ObTranAmount =
              entities.DebtAdjustment.Amount;
          }
          else
          {
            local.ForCreateOcse157Verification.ObTranAmount =
              -entities.DebtAdjustment.Amount;
          }

          local.ForCreateOcse157Verification.DebtAdjType =
            entities.DebtAdjustment.DebtAdjustmentType;

          switch(AsChar(local.Assistance.Flag))
          {
            case 'C':
              local.Line24Curr.Currency152 += local.
                ForCreateOcse157Verification.ObTranAmount.GetValueOrDefault();

              break;
            case 'F':
              local.Line24Former.Currency152 += local.
                ForCreateOcse157Verification.ObTranAmount.GetValueOrDefault();

              break;
            default:
              local.Line24Never.Currency152 += local.
                ForCreateOcse157Verification.ObTranAmount.GetValueOrDefault();

              break;
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.ObTranSgi =
              entities.DebtAdjustment.SystemGeneratedIdentifier;
          }

          UseFnCreateOcse157Verification();

          // ---------------------------------------------
          // End of Debt Adjustments READ EACH.
          // --------------------------------------------
        }

        MoveOcse157Verification3(local.NullOcse157Verification,
          local.ForCreateOcse157Verification);

        // ------------------------------------------
        // End of Debt READ EACH.
        // ------------------------------------------
      }

      // --  Include collections created during the reporting period for debts 
      // due in the month following the report period.
      //     (i.e. collections created during September for debts due in October
      // ).
      // CQ39887 - disabled READ - no longer include collections for future 
      // debts - per DRA findings for 2012 (DRA Audit Sample #006)
      // CQ39887 - END of disabled READ - no longer include collections for 
      // future debts
      // -------------------------------------------------------------------
      // We finished processing all Accruing debts for Supp Person.
      // Now, read Collections applied to Vol and Gifts, since these
      // debts don't show an amount due at Debt level.
      // -------------------------------------------------------------------
      // -------------------------------------------------------------------
      // -Read Gift and VOL collections
      // -Read colls 'created during' FY and un-adj at the end of FY
      // -Skip Concurrent colls
      // -Skip direct payments. (CRT= 2 or 7)
      // -------------------------------------------------------------------
      // -------------------------------------------------------------------
      // Comments on READ EACH.
      // -Generates 2 table join on collection and ob_trn
      // -------------------------------------------------------------------
      foreach(var item1 in ReadCollectionCsePerson())
      {
        // -------------------------------------------------------------------
        // -Skip colls before person is assigned to case.
        // -------------------------------------------------------------------
        if (Lt(Date(entities.Collection.CreatedTmst), local.Earliest.StartDate))
        {
          continue;
        }

        // ----------------------------------------------------------------------------------------------------------------------------
        // 04/14/08  GVandy  CQ#2461  Per federal data reliability audit, 
        // provide case number in audit data.  Use the collection
        // created date to find the appropriate case.
        // ----------------------------------------------------------------------------------------------------------------------------
        local.DateWorkArea.Date = Date(entities.Collection.CreatedTmst);
        local.ForCreateOcse157Verification.CaseNumber =
          UseFnOcse157FindCaseForAudit();

        switch(AsChar(local.Assistance.Flag))
        {
          case 'C':
            local.Line24Curr.Currency152 += entities.Collection.Amount;
            local.ForCreateOcse157Verification.Column = "b";

            break;
          case 'F':
            local.Line24Former.Currency152 += entities.Collection.Amount;
            local.ForCreateOcse157Verification.Column = "c";

            break;
          default:
            local.Line24Never.Currency152 += entities.Collection.Amount;
            local.ForCreateOcse157Verification.Column = "d";

            break;
        }

        local.ForCreateOcse157Verification.CollectionAmount =
          entities.Collection.Amount;
        local.ForCreateOcse157Verification.CollCreatedDte =
          Date(entities.Collection.CreatedTmst);
        local.ForCreateOcse157Verification.CollApplToCode =
          entities.Collection.AppliedToCode;
        local.ForCreateOcse157Verification.SuppPersonNumber =
          entities.Supp.Number;
        local.ForCreateOcse157Verification.ObligorPersonNbr =
          entities.Ap.Number;
        local.ForCreateOcse157Verification.CourtOrderNumber =
          entities.Collection.CourtOrderAppliedTo;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CollectionSgi =
            entities.Collection.SystemGeneratedIdentifier;
          local.ForCreateOcse157Verification.CollectionDte =
            entities.Collection.CollectionDt;
        }

        UseFnCreateOcse157Verification();

        // --------------------------------------------
        // End of Collection READ EACH.
        // --------------------------------------------
      }

      // -------------------------------------------------------------
      // *** Finished processing all Debts and Colls for this person.
      // -------------------------------------------------------------
      // --------------------------------------------
      // Check commit counts.
      // --------------------------------------------
      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "24 " + entities
          .Supp.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line24Curr.Currency152 * 100), 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line24Former.Currency152 * 100), 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line24Never.Currency152 * 100), 15);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "24";
          local.ForError.SuppPersonNumber = entities.Supp.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      // --------------------------------------------
      // End of main READ EACH.
      // --------------------------------------------
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "24";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line24Curr.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line24Former.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line24Never.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "25 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "24";
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
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
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseFn157GetAssistanceForPerson()
  {
    var useImport = new Fn157GetAssistanceForPerson.Import();
    var useExport = new Fn157GetAssistanceForPerson.Export();

    useImport.CsePerson.Number = entities.Supp.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(Fn157GetAssistanceForPerson.Execute, useImport, useExport);

    local.Assistance.Flag = useExport.AssistanceProgram.Flag;
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

  private void UseFnGetEarliestCaseRole4Pers()
  {
    var useImport = new FnGetEarliestCaseRole4Pers.Import();
    var useExport = new FnGetEarliestCaseRole4Pers.Export();

    useImport.CsePerson.Number = entities.Supp.Number;

    Call(FnGetEarliestCaseRole4Pers.Execute, useImport, useExport);

    local.Earliest.StartDate = useExport.Earliest.StartDate;
  }

  private string UseFnOcse157FindCaseForAudit()
  {
    var useImport = new FnOcse157FindCaseForAudit.Import();
    var useExport = new FnOcse157FindCaseForAudit.Export();

    useImport.Ap.Number = entities.Ap.Number;
    useImport.Supported.Number = entities.Supp.Number;
    useImport.DebtOrCollection.Date = local.DateWorkArea.Date;

    Call(FnOcse157FindCaseForAudit.Execute, useImport, useExport);

    return useExport.Ocse157Verification.CaseNumber ?? "";
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification2(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCollectionCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Ap.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
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
        entities.Ap.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Ap.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupported()
  {
    entities.Supp.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Restart.Number);
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supp.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustmentObligationTransactionRlnRsn()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustmentObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDebtObligationObligationTypeDebtDetailCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Ap.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetailCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.Earliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 10);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 11);
        entities.ObligationType.Classification = db.GetString(reader, 12);
        entities.DebtDetail.DueDt = db.GetDate(reader, 13);
        entities.Ap.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 7);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 8);
        entities.ObligationRln.Description = db.GetString(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
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
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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

    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private DateWorkArea reportEndDate;
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
    /// A value of MostRecentAdjustment.
    /// </summary>
    [JsonPropertyName("mostRecentAdjustment")]
    public Common MostRecentAdjustment
    {
      get => mostRecentAdjustment ??= new();
      set => mostRecentAdjustment = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of PrevObligation.
    /// </summary>
    [JsonPropertyName("prevObligation")]
    public Obligation PrevObligation
    {
      get => prevObligation ??= new();
      set => prevObligation = value;
    }

    /// <summary>
    /// A value of Earliest.
    /// </summary>
    [JsonPropertyName("earliest")]
    public CaseRole Earliest
    {
      get => earliest ??= new();
      set => earliest = value;
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
    /// A value of Line24Curr.
    /// </summary>
    [JsonPropertyName("line24Curr")]
    public ReportTotals Line24Curr
    {
      get => line24Curr ??= new();
      set => line24Curr = value;
    }

    /// <summary>
    /// A value of Line24Former.
    /// </summary>
    [JsonPropertyName("line24Former")]
    public ReportTotals Line24Former
    {
      get => line24Former ??= new();
      set => line24Former = value;
    }

    /// <summary>
    /// A value of Line24Never.
    /// </summary>
    [JsonPropertyName("line24Never")]
    public ReportTotals Line24Never
    {
      get => line24Never ??= new();
      set => line24Never = value;
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
    /// A value of Assistance.
    /// </summary>
    [JsonPropertyName("assistance")]
    public Common Assistance
    {
      get => assistance ??= new();
      set => assistance = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private Common mostRecentAdjustment;
    private DateWorkArea dateWorkArea;
    private CsePerson prevCsePerson;
    private Obligation prevObligation;
    private CaseRole earliest;
    private DateWorkArea nullDateWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private ReportTotals line24Curr;
    private ReportTotals line24Former;
    private ReportTotals line24Never;
    private Ocse157Verification nullOcse157Verification;
    private Common assistance;
    private Common commitCnt;
    private Ocse157Verification forError;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private CsePerson ap;
    private CsePersonAccount obligor;
    private LegalAction legalAction;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debtAdjustment;
    private ObligationRln obligationRln;
    private DebtDetail debtDetail;
    private Collection collection;
    private CsePerson supp;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
  }
#endregion
}
