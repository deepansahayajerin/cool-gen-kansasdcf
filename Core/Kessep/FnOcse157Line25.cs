// Program: FN_OCSE157_LINE_25, ID: 371114585, model: 746.
// Short name: SWE02964
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_25.
/// </summary>
[Serializable]
public partial class FnOcse157Line25: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_25 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line25(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line25.
  /// </summary>
  public FnOcse157Line25(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 08/23/01 				Initial version - Cloned from fn_ocse157_line_25_old.
    // 					This CAB writes verification record for each
    // 					debt/collection.
    // 08/11/01				Use correct flag to determine assistance.
    // 08/30/01				Include CSENet collections.
    // 09/06/2001				Add AP number to verification data.
    // 04/14/08  GVandy	CQ#2461		Per federal data reliability audit, provide 
    // case number
    // 					in audit data.  Use the collection created date to
    // 					find the appropriate case.
    // 08/28/13  LSS           CQ39887         OCSE157 Report modifications per 
    // the 2012 DRA (Data
    // 					Reliability Audit) requirements / findings
    //                                         
    // Line 25 -
    //                                            
    // 1) Do not count negative collection where
    // adjustment
    // 					      occurred in report period to a collection
    // 					      created in a prior report period
    // 					      (DRA Audit Sample #036)
    //                                            
    // 2) Include collection created in Sept of
    // previous
    // 					      fiscal year and posted in October of the current
    // 					      fiscal year (DRA Audit Sample #006)
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, include only amounts that
    // are
    // 					both distributed and disbursed.
    // ---------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "25";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.PreviousMonthStart.Timestamp =
      AddMonths(import.ReportStartDate.Timestamp, -1);

    if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() < import
      .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "25 "))
      {
        local.RestartCsePerson.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

        // --------------------------------------------------------------
        // Initialize counters on restart
        // ---------------------------------------------------------------
        local.Line25Curr.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 14, 15)) / (
            decimal)100;
        local.Line25Former.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 29, 15)) / (
            decimal)100;
        local.Line25Never.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 44, 15)) / (
            decimal)100;
      }

      // -------------------------------------------------------------------
      // Read Each is sorted in Asc order of Supp Person #.
      // Maintain a running total for each Supp person and then
      // process a break in person #. This is so we only determine
      // Assistance type once per Supp person (as opposed to
      // once per Collection)
      // -------------------------------------------------------------------
      foreach(var item in ReadCsePersonSupported())
      {
        if (Equal(entities.Supp.Number, local.Prev.Number))
        {
          continue;
        }

        local.Prev.Number = entities.Supp.Number;
        ++local.CommitCnt.Count;

        // -------------------------------------------------------------------
        // Clear local views
        // -------------------------------------------------------------------
        local.CurrSupp.Currency152 = 0;
        MoveOcse157Verification3(local.Null1, local.ForCreateOcse157Verification);
          
        UseFnOcse157ClearGroupView();

        // -------------------------------------------------------------------
        // -Read Gift and Curr collections
        // -Read colls 'created during' FY and un-adj at the end of FY
        // -Read colls 'adjusted during' FY but created in prev FYs
        // -Skip Concurrent colls
        // -Skip direct payments. (CRT= 2 or 7)
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Comments on READ EACH.
        // -Generates 3 table join on collection, ob_trn and crd
        // -Redundant created_tmst qualification is to aid performance
        // -------------------------------------------------------------------
        // CQ39887 - disable READ and add new READ
        //           Do not count negative adjustments from previous FY (2012 
        // DRA Audit Sample #036)
        //           Include collection created in previous FY and posted in 
        // current FY (2012 DRA Audit Sample #006)
        // CQ39887 - added new READ
        local.Group.Index = 0;
        local.Group.Clear();

        foreach(var item1 in ReadCollectionCsePerson2())
        {
          local.ForCreateOcse157Verification.CollectionAmount =
            entities.Collection.Amount;
          local.CurrSupp.Currency152 += entities.Collection.Amount;

          // ----------------------------------------------------------------------------------------------------------------------------
          // 04/14/08  GVandy  CQ#2461  Per federal data reliability audit, 
          // provide case number in audit data.  Use the collection
          // created date to find the appropriate case.
          // ----------------------------------------------------------------------------------------------------------------------------
          local.Collection.Date = Date(entities.Collection.CreatedTmst);
          local.ForCreateOcse157Verification.CaseNumber =
            UseFnOcse157FindCaseForAudit();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              entities.Collection.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionDte =
              entities.Collection.CollectionDt;
          }

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

          // --------------------------------------------
          // Build an array for verification data.
          // --------------------------------------------
          local.Group.Update.Ocse157Verification.Assign(
            local.ForCreateOcse157Verification);

          // --------------------------------------------
          // End of MAIN READ EACH.
          // --------------------------------------------
          local.Group.Next();
        }

        // CQ39887 - END  (disabled READ and added new READ)
        // -------------------------------------------------------------
        // *** Finished processing for this person.
        // -------------------------------------------------------------
        UseFnCreateVerificationForGroup();

        switch(AsChar(local.Assistance.Flag))
        {
          case 'C':
            local.Line25Curr.Currency152 += local.CurrSupp.Currency152;

            break;
          case 'F':
            local.Line25Former.Currency152 += local.CurrSupp.Currency152;

            break;
          default:
            local.Line25Never.Currency152 += local.CurrSupp.Currency152;

            break;
        }

        // --------------------------------------------
        // Check commit counts.
        // --------------------------------------------
        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "25 " + entities
            .Supp.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Curr.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Former.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Never.Currency152 * 100), 15);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.SuppPersonNumber = entities.Supp.Number;
            local.ForError.LineNumber = "25";
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
    else
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "25 "))
      {
        if (CharAt(local.ProgramCheckpointRestart.RestartInfo, 4) == 'B')
        {
          local.RestartCsePerson.Number = "9999999999";
          local.RestartPaymentRequest.SystemGeneratedIdentifier =
            (int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 5, 9));
        }
        else
        {
          local.RestartCsePerson.Number =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
          local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
        }

        // --------------------------------------------------------------
        // Initialize counters on restart
        // ---------------------------------------------------------------
        local.Line25Curr.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 14, 15)) / (
            decimal)100;
        local.Line25Former.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 29, 15)) / (
            decimal)100;
        local.Line25Never.Currency152 =
          StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 44, 15)) / (
            decimal)100;
      }

      // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
      // that are both
      // distributed and disbursed.
      // -------------------------------------------------------------------
      // Read Each is sorted in Asc order of Supp Person #.
      // Maintain a running total for each Supp person and then
      // process a break in person #. This is so we only determine
      // Assistance type once per Supp person (as opposed to
      // once per Collection)
      // -------------------------------------------------------------------
      foreach(var item in ReadCsePersonSupported())
      {
        if (Equal(entities.Supp.Number, local.Prev.Number))
        {
          continue;
        }

        local.Prev.Number = entities.Supp.Number;
        ++local.CommitCnt.Count;

        // -------------------------------------------------------------------
        // Clear local views
        // -------------------------------------------------------------------
        local.CurrSupp.Currency152 = 0;
        MoveOcse157Verification3(local.Null1, local.ForCreateOcse157Verification);
          
        UseFnOcse157ClearGroupView();

        // 02/04/20  GVandy  CQ66220  Beginning in FY 2022, include only amounts
        // that are both
        // distributed and disbursed.  First we gather the current support 
        // collections that are
        // retained by the state.  These collection will have applied as AF, FC,
        // NF, or NC.
        // --Count all AF, FC, NF, and NC collections created during the 
        // reporting period that
        // are either non adjusted or adjusted after the end of the reporting 
        // period.  These
        // collections do not have disbursement records.
        local.Group.Index = 0;
        local.Group.Clear();

        foreach(var item1 in ReadCollectionCsePerson1())
        {
          local.ForCreateOcse157Verification.CollectionAmount =
            entities.Collection.Amount;
          local.CurrSupp.Currency152 += entities.Collection.Amount;

          // ----------------------------------------------------------------------------------------------------------------------------
          // 04/14/08  GVandy  CQ#2461  Per federal data reliability audit, 
          // provide case number in audit data.  Use the collection
          // created date to find the appropriate case.
          // ----------------------------------------------------------------------------------------------------------------------------
          local.Collection.Date = Date(entities.Collection.CreatedTmst);
          local.ForCreateOcse157Verification.CaseNumber =
            UseFnOcse157FindCaseForAudit();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              entities.Collection.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionDte =
              entities.Collection.CollectionDt;
          }

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

          // --------------------------------------------
          // Build an array for verification data.
          // --------------------------------------------
          local.Group.Update.Ocse157Verification.Assign(
            local.ForCreateOcse157Verification);

          // --------------------------------------------
          // End of MAIN READ EACH.
          // --------------------------------------------
          local.Group.Next();
        }

        // -------------------------------------------------------------
        // *** Finished processing for this person.
        // -------------------------------------------------------------
        UseFnCreateVerificationForGroup();

        switch(AsChar(local.Assistance.Flag))
        {
          case 'C':
            local.Line25Curr.Currency152 += local.CurrSupp.Currency152;

            break;
          case 'F':
            local.Line25Former.Currency152 += local.CurrSupp.Currency152;

            break;
          default:
            local.Line25Never.Currency152 += local.CurrSupp.Currency152;

            break;
        }

        // --------------------------------------------
        // Check commit counts.
        // --------------------------------------------
        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "25 " + entities
            .Supp.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Curr.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Former.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Never.Currency152 * 100), 15);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.SuppPersonNumber = entities.Supp.Number;
            local.ForError.LineNumber = "25";
            UseOcse157WriteError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
      }

      // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
      // that are both
      // distributed and disbursed.  We are now gathering the current support 
      // collections
      // that are disbursed to a NCP or their designated payee.
      foreach(var item in ReadPaymentRequest())
      {
        ++local.CommitCnt.Count;
        local.PrevSupp.Number = "";

        // --Count all non AF, FC, NF, and NC current support collections in 
        // this payment
        // request that are either non adjusted or adjusted after the end of the
        // reporting
        // period.
        // --the properties on this read each are set to read DISTINCT 
        // collections.
        foreach(var item1 in ReadCollectionCsePersonCsePerson())
        {
          local.ForCreateOcse157Verification.CollectionAmount =
            entities.Collection.Amount;

          // ----------------------------------------------------------------------------------------------------------------------------
          // 04/14/08  GVandy  CQ#2461  Per federal data reliability audit, 
          // provide case number in audit data.  Use the collection
          // created date to find the appropriate case.
          // ----------------------------------------------------------------------------------------------------------------------------
          local.Collection.Date = Date(entities.Collection.CreatedTmst);
          local.ForCreateOcse157Verification.CaseNumber =
            UseFnOcse157FindCaseForAudit();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              entities.Collection.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionDte =
              entities.Collection.CollectionDt;
          }

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

          if (!Equal(entities.Supp.Number, local.PrevSupp.Number))
          {
            UseFn157GetAssistanceForPerson();

            switch(AsChar(local.Assistance.Flag))
            {
              case 'C':
                local.ForCreateOcse157Verification.Column = "b";

                break;
              case 'F':
                local.ForCreateOcse157Verification.Column = "c";

                break;
              default:
                local.ForCreateOcse157Verification.Column = "d";

                break;
            }

            local.PrevSupp.Number = entities.Supp.Number;
          }

          UseFnCreateOcse157Verification();

          switch(AsChar(local.Assistance.Flag))
          {
            case 'C':
              local.Line25Curr.Currency152 += entities.Collection.Amount;

              break;
            case 'F':
              local.Line25Former.Currency152 += entities.Collection.Amount;

              break;
            default:
              local.Line25Never.Currency152 += entities.Collection.Amount;

              break;
          }
        }

        // --------------------------------------------
        // Check commit counts.
        // --------------------------------------------
        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "25 B" + NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Curr.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Former.Currency152 * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            ((long)(local.Line25Never.Currency152 * 100), 15);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.SuppPersonNumber = entities.Supp.Number;
            local.ForError.LineNumber = "25";
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

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "25";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line25Curr.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line25Former.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line25Never.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "26 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "25";
      local.ForError.SuppPersonNumber = "";
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

  private static void MoveGroup1(FnCreateVerificationForGroup.Import.
    GroupGroup source, Local.GroupGroup target)
  {
    target.Ocse157Verification.Assign(source.Ocse157Verification);
  }

  private static void MoveGroup2(Local.GroupGroup source,
    FnCreateVerificationForGroup.Import.GroupGroup target)
  {
    target.Ocse157Verification.Assign(source.Ocse157Verification);
  }

  private static void MoveNull1ToGroup(FnOcse157ClearGroupView.Export.
    NullGroup source, Local.GroupGroup target)
  {
    target.Ocse157Verification.Assign(source.Ocse157Verification);
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

  private void UseFnCreateVerificationForGroup()
  {
    var useImport = new FnCreateVerificationForGroup.Import();
    var useExport = new FnCreateVerificationForGroup.Export();

    useImport.Supp.Number = entities.Supp.Number;
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    local.Group.CopyTo(useImport.Group, MoveGroup2);

    Call(FnCreateVerificationForGroup.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup1);
    local.Assistance.Flag = useExport.Assistance.Flag;
  }

  private void UseFnOcse157ClearGroupView()
  {
    var useImport = new FnOcse157ClearGroupView.Import();
    var useExport = new FnOcse157ClearGroupView.Export();

    Call(FnOcse157ClearGroupView.Execute, useImport, useExport);

    useExport.Null1.CopyTo(local.Group, MoveNull1ToGroup);
  }

  private string UseFnOcse157FindCaseForAudit()
  {
    var useImport = new FnOcse157FindCaseForAudit.Import();
    var useExport = new FnOcse157FindCaseForAudit.Export();

    useImport.Supported.Number = entities.Supp.Number;
    useImport.Ap.Number = entities.Ap.Number;
    useImport.DebtOrCollection.Date = local.Collection.Date;

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

  private IEnumerable<bool> ReadCollectionCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return ReadEach("ReadCollectionCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PreviousMonthStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

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
        entities.Collection.Populated = true;
        entities.Ap.Populated = true;
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

  private IEnumerable<bool> ReadCollectionCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return ReadEach("ReadCollectionCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PreviousMonthStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

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
        entities.Collection.Populated = true;
        entities.Ap.Populated = true;
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

  private IEnumerable<bool> ReadCollectionCsePersonCsePerson()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCollectionCsePersonCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
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
        entities.Supp.Number = db.GetString(reader, 20);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.Ap.Populated = true;
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
        db.SetString(command, "cspNumber", local.RestartCsePerson.Number);
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

  private IEnumerable<bool> ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "paymentRequestId",
          local.RestartPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaymentRequest.Classification = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 7);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

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
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private Common displayInd;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private Ocse157Verification ocse157Verification;
    }

    /// <summary>
    /// A value of PrevSupp.
    /// </summary>
    [JsonPropertyName("prevSupp")]
    public CsePerson PrevSupp
    {
      get => prevSupp ??= new();
      set => prevSupp = value;
    }

    /// <summary>
    /// A value of PreviousMonthStart.
    /// </summary>
    [JsonPropertyName("previousMonthStart")]
    public DateWorkArea PreviousMonthStart
    {
      get => previousMonthStart ??= new();
      set => previousMonthStart = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of Line25Curr.
    /// </summary>
    [JsonPropertyName("line25Curr")]
    public ReportTotals Line25Curr
    {
      get => line25Curr ??= new();
      set => line25Curr = value;
    }

    /// <summary>
    /// A value of Line25Former.
    /// </summary>
    [JsonPropertyName("line25Former")]
    public ReportTotals Line25Former
    {
      get => line25Former ??= new();
      set => line25Former = value;
    }

    /// <summary>
    /// A value of Line25Never.
    /// </summary>
    [JsonPropertyName("line25Never")]
    public ReportTotals Line25Never
    {
      get => line25Never ??= new();
      set => line25Never = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of CurrSupp.
    /// </summary>
    [JsonPropertyName("currSupp")]
    public ReportTotals CurrSupp
    {
      get => currSupp ??= new();
      set => currSupp = value;
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
    /// A value of RestartPaymentRequest.
    /// </summary>
    [JsonPropertyName("restartPaymentRequest")]
    public PaymentRequest RestartPaymentRequest
    {
      get => restartPaymentRequest ??= new();
      set => restartPaymentRequest = value;
    }

    private CsePerson prevSupp;
    private DateWorkArea previousMonthStart;
    private DateWorkArea collection;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restartCsePerson;
    private ReportTotals line25Curr;
    private ReportTotals line25Former;
    private ReportTotals line25Never;
    private CsePerson prev;
    private Common commitCnt;
    private Ocse157Verification forError;
    private Ocse157Verification null1;
    private Array<GroupGroup> group;
    private ReportTotals currSupp;
    private Common assistance;
    private PaymentRequest restartPaymentRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction disbCollection;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private CsePerson supp;
    private CsePersonAccount supported;
    private Collection collection;
    private ObligationTransaction debt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CsePerson ap;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private PaymentRequest paymentRequest;
  }
#endregion
}
