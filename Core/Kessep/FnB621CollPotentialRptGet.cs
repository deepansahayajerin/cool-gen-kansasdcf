// Program: FN_B621_COLL_POTENTIAL_RPT_GET, ID: 373454665, model: 746.
// Short name: SWEF621B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B621_COLL_POTENTIAL_RPT_GET.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB621CollPotentialRptGet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B621_COLL_POTENTIAL_RPT_GET program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB621CollPotentialRptGet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB621CollPotentialRptGet.
  /// </summary>
  public FnB621CollPotentialRptGet(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // 09-01-2001  SWSRGMB            Initial Development
    // 02-21-2003  SWSRGMF            Changed code to not abend on a Office 
    // Service Provider NF msg.  Also, changed code to print out the current
    // Obligation being processed when a NF happens.
    // 12-13-2007  SWCOAMX - Arun     Changed the code to process the specified 
    // county only, if passed through the parameter list ( CQ#619 )
    // 12-26-2007  SWCOAMX - Arun     Changed the process as to how collection 
    // gets added back to the debts ( CQ#619 )
    // 01-02-2008  SWCOAMX - Arun     Changed the read properties ( CQ#619 )
    // 01-14-2008  SWCOAMX - Arun     Added the Debt Adjustments Logic ( CQ#619 
    // )
    // 12-22-2008  SWSRGAV - George     Modified program derivation logic for 
    // Distrihbution 2009 ( CQ#4387 )
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeKansas.State = 20;

    // : Set hardcoded values for Program.
    local.Hardcoded.HardcodedAf.SystemGeneratedIdentifier = 2;
    local.Hardcoded.HardcodedAf.Code = "AF";
    local.Hardcoded.HardcodedAfi.SystemGeneratedIdentifier = 14;
    local.Hardcoded.HardcodedAfi.Code = "AFI";
    local.Hardcoded.HardcodedFc.SystemGeneratedIdentifier = 15;
    local.Hardcoded.HardcodedFc.Code = "FC";
    local.Hardcoded.HardcodedFci.SystemGeneratedIdentifier = 16;
    local.Hardcoded.HardcodedFci.Code = "FCI";
    local.Hardcoded.HardcodedNa.SystemGeneratedIdentifier = 12;
    local.Hardcoded.HardcodedNa.Code = "NA";
    local.Hardcoded.HardcodedNai.SystemGeneratedIdentifier = 18;
    local.Hardcoded.HardcodedNai.Code = "NAI";
    local.Hardcoded.HardcodedNc.SystemGeneratedIdentifier = 13;
    local.Hardcoded.HardcodedNc.Code = "NC";
    local.Hardcoded.HardcodedNf.SystemGeneratedIdentifier = 3;
    local.Hardcoded.HardcodedNf.Code = "NF";
    local.Hardcoded.HardcodedMai.SystemGeneratedIdentifier = 17;
    local.Hardcoded.HardcodedMai.Code = "MAI";
    local.Hardcoded.HardcodeCa.ProgramState = "CA";
    local.Hardcoded.HardcodePa.ProgramState = "PA";
    local.Hardcoded.HardcodeNa.ProgramState = "NA";
    local.Hardcoded.HardcodeTa.ProgramState = "TA";

    // : Set hardcode collection types
    local.Hardcoded.HardcodeRegular.SequentialIdentifier = 1;
    local.Hardcoded.HardcodeIncomeWithholdin.SequentialIdentifier = 6;
    local.Hardcoded.HardcodeDirPmtAp.SequentialIdentifier = 14;
    local.Hardcoded.HardcodeDirPmtCt.SequentialIdentifier = 20;
    local.Hardcoded.HardcodeDirPmtCru.SequentialIdentifier = 21;
    local.Hardcoded.Hardcoded718B.SystemGeneratedIdentifier = 18;
    local.Hardcoded.HardcodeAccruing.Classification = "A";
    local.Hardcoded.HardcodeSecondary.PrimarySecondaryCode = "S";
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      local.VerifyThisRun.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

      // CR#619 Changed on Dec 13, 2007 to get County parameter from the 
      // parameter list
      local.CountyParameter.CountyAbbreviation =
        Substring(local.ProgramProcessingInfo.ParameterList, 2, 2);
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        local.Restart.StandardNumber =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 20);
      }
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    // *****************************************************************
    // * Setup of batch error handling
    // 
    // *
    // *****************************************************************
    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // * End of Batch error handling setup                             *
    // *****************************************************************
    // : Open the output file
    local.EabFileHandling.Action = "OPEN";
    UseFnEabSwexfw17CollPotWrite2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // <=========== GET REPORTING DATES ========>
    // The following logic sets the local current date work area date to either:
    // 1. If the processing info date is blank, set the BOM (Begining of Month) 
    // DATE
    // to the system current date month - 1 month and set day to the 1st, set 
    // EOM to last
    // day of the reporting month.
    // 2. If the processing info date is max date (2099-12-31), same as above
    // 3. Otherwise, use the program processing info date to calculate the BOM 
    // and EOM.
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Initialized.Date))
    {
      local.Current.Date = Now().Date;
    }
    else if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Max.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    }

    UseCabDetermineReportingDates();

    // : The following is setting up information for when the 'verify' option is
    // selected.
    //   The verify_this_run flag is set via input parm position 1.  If it is '
    // Y',
    //   we will populate the stats verifi table during processing with details 
    // of
    //   what was retrieved and included in the report.
    //   This flag should not be set to 'Y' in prod.
    if (AsChar(local.VerifyThisRun.Flag) == 'Y')
    {
      local.DebtType.FirstRunNumber = 99;
      local.DebtType.ParentId = 621;
      local.DebtType.SuppPersonNumber = "SWEFB621";
      local.DebtType.LineNumber = 2;
      UseCabGetYearMonthFromDate();
      local.DebtType.YearMonth = local.Bom.YearMonth;
      local.CollectionType.Assign(local.DebtType);
      local.CollectionType.LineNumber = 3;
    }

    // : Create a table of programs.
    // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
    // Uncommitted Browse **
    local.OfPgms.Index = 0;
    local.OfPgms.Clear();

    foreach(var item in ReadProgram())
    {
      local.OfPgms.Update.OfPgms1.Assign(entities.Program);
      local.OfPgms.Next();
    }

    // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
    // Uncommitted Browse **
    foreach(var item in ReadLegalAction())
    {
      if (!Equal(entities.LegalAction.StandardNumber, local.Save.StandardNumber))
        
      {
        // : Check for an active case unit with an locate flag not 'Y' (ap not 
        // located).
        local.LnumberOfOrdersInLocate.Count = 0;

        // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
        // Uncommitted Browse **
        // ** Also, Added Type = "AP" in the read statement  and moved it after 
        // the Tribunal Read **
        // ** Added by SWCOAMX to display the standard nbr to know how many are 
        // processed. This is temporary so error report is used **
        // **Change Begins Here **
        // **Change Ends   Here **
        MoveLegalAction(entities.LegalAction, local.Save);

        // <=========== GET COUNTY OF LEGAL ACTION ========>
        // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
        // Uncommitted Browse **
        if (ReadTribunal())
        {
          // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify 
          // to Uncommitted Browse **
          if (ReadFips())
          {
            // CQ#619 Changes Begin Here
            if (!IsEmpty(local.CountyParameter.CountyAbbreviation))
            {
              // : Specific County passed thru the Parameter list
              if (entities.Fips.State != local.HardcodeKansas.State)
              {
                // : It is not in Kansas Sate
                continue;
              }
              else
              {
                // : It is in Kansas Sate
                if (!Equal(entities.Fips.CountyAbbreviation,
                  local.CountyParameter.CountyAbbreviation))
                {
                  // : It is not the County specified in the parameter list
                  continue;
                }

                // : : It is the County specified in the parameter list
              }
            }

            // CQ#619 Changes End   Here
            if (entities.Fips.State != local.HardcodeKansas.State)
            {
              // : Interstate.
              local.Fips.StateAbbreviation = "XX";
              local.Fips.CountyAbbreviation = "XX";
              local.Tribunal.JudicialDistrict = "XX";
            }
            else if (entities.Fips.County == 0)
            {
              if (entities.Fips.Location >= 20 && entities.Fips.Location <= 99)
              {
                // : This is a tribe.
                local.Fips.StateAbbreviation = "YY";
                local.Fips.CountyAbbreviation = "YY";
                local.Tribunal.JudicialDistrict = "YY";
              }
              else if (entities.Fips.Location == 3 && entities.Fips.State == 20)
              {
                // : Kansas Pay Center.
              }
              else
              {
                // : Interstate.
                local.Fips.StateAbbreviation = "XX";
                local.Fips.CountyAbbreviation = "XX";
                local.Tribunal.JudicialDistrict = "XX";
              }
            }
            else
            {
              local.Fips.Assign(entities.Fips);
              local.Tribunal.JudicialDistrict =
                entities.Tribunal.JudicialDistrict;

              // : Following is done so that data is sorted properly.  Need a JD
              //   less than 10 to be ' 9', not '9 '.
              if (IsEmpty(Substring(local.Tribunal.JudicialDistrict, 2, 1)))
              {
                local.Tribunal.JudicialDistrict = " " + Substring
                  (local.Tribunal.JudicialDistrict,
                  Tribunal.JudicialDistrict_MaxLength, 1, 4);
              }
            }
          }
          else
          {
            // CQ#619 Changes Begin Here
            if (!IsEmpty(local.CountyParameter.CountyAbbreviation))
            {
              // : Skip the record if a specific county is passed thru the 
              // parameter list
              continue;
            }

            // CQ#619 Changes End   Here
            // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify
            // to Uncommitted Browse **
            if (ReadFipsTribAddress())
            {
              // : Out of Country.
              local.Fips.StateAbbreviation = "ZZ";
              local.Fips.CountyAbbreviation = "ZZ";
              local.Tribunal.JudicialDistrict = "ZZ";
            }
            else
            {
              // <===========Write error message to error report ========>
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail = "FIPS NF: " + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport2();

              continue;
            }
          }
        }
        else
        {
          // <===========Write error message to error report ========>
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Tribunal and/or FIPS NF: " + entities
            .LegalAction.StandardNumber;
          UseCabErrorReport2();

          continue;
        }

        // ** SWCOAMX moved from above
        foreach(var item1 in ReadCaseUnit())
        {
          if (CharAt(entities.CaseUnit.State, 3) == 'N')
          {
            local.LnumberOfOrdersInLocate.Count = 1;

            break;
          }
        }

        if (AsChar(local.VerifyThisRun.Flag) == 'Y')
        {
          // : Had to use different fields to be able to give the needed info 
          // using
          //   stats verifi table for testing.
          local.DebtType.PersonProgCode = local.Fips.StateAbbreviation;
          local.CollectionType.PersonProgCode = local.Fips.StateAbbreviation;
          local.DebtType.CaseNumber = local.Tribunal.JudicialDistrict + " " + (
            local.Fips.CountyAbbreviation ?? "");
          local.CollectionType.CaseNumber = local.Tribunal.JudicialDistrict + " " +
            (local.Fips.CountyAbbreviation ?? "");
          UseFnB621GetReferral();
          local.DebtType.CourtOrderNumber = entities.LegalAction.StandardNumber;
          local.DebtType.OfficeId = local.Office.SystemGeneratedId;
          local.DebtType.ServicePrvdrId =
            local.ServiceProvider.SystemGeneratedId;
          local.CollectionType.CourtOrderNumber =
            entities.LegalAction.StandardNumber;
          local.CollectionType.OfficeId = local.Office.SystemGeneratedId;
          local.CollectionType.ServicePrvdrId =
            local.ServiceProvider.SystemGeneratedId;
        }

        // : Call cab to build a table of all supported persons and their 
        // programs related to the court order.
        UseFnB621BuildPgmHstForCo();

        // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify to 
        // Uncommitted Browse **
        foreach(var item1 in ReadObligationObligationType())
        {
          if (AsChar(local.VerifyThisRun.Flag) == 'Y')
          {
            // : Only get the AP if in test/verify mode.
            // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify
            // to Uncommitted Browse **
            if (ReadCsePerson())
            {
              local.DebtType.ObligorPersonNbr = entities.Ap.Number;
              local.CollectionType.ObligorPersonNbr = entities.Ap.Number;
            }
            else
            {
              // <===========Write error message to error report ========>
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail = "CSE PERSON NOT FOUND!! " + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport2();
            }
          }

          local.JsSecondObligation.Flag = "";

          switch(AsChar(entities.Obligation.PrimarySecondaryCode))
          {
            case ' ':
              break;
            case 'S':
              // : Secondary obligation - we use the primary only.
              continue;
            case 'J':
              // : Only process one of the obligations in a j/s situation.
              //   To do this, we only use the obligations that are tied to the 
              // obligation
              //   rln as the 'first obligor'.
              //   Set a flag to indicate this situation, as we will still need 
              // to read the
              //   collections for this obligation.
              // ** SWCOAMX Changed the properties on 01-02-08 from Do not 
              // Specify to Uncommitted Browse **
              if (!ReadObligationRln())
              {
                // : This is the second obligation of a j/s, so set a flag.  We 
                // won't add to debt totals for this one.
                //   We do need to read the debts for the obligation in order to
                // get non-concurrent collections
                //   that may have been applied.
                local.JsSecondObligation.Flag = "Y";
                local.DebtDetail.BalanceDueAmt = 0;
              }

              break;
            default:
              break;
          }

          // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify 
          // to Uncommitted Browse **
          foreach(var item2 in ReadDebtDetailDebtCsePerson())
          {
            // : Move the programs for the current supported person to a group 
            // view.
            for(local.PgmHist.Index = 0; local.PgmHist.Index < local
              .PgmHist.Count; ++local.PgmHist.Index)
            {
              if (Equal(local.PgmHist.Item.PgmHistSuppPrsn.Number,
                entities.Child.Number))
              {
                local.PgmsForSp.Index = 0;
                local.PgmsForSp.Clear();

                for(local.PgmHist.Item.PgmHistDtl.Index = 0; local
                  .PgmHist.Item.PgmHistDtl.Index < local
                  .PgmHist.Item.PgmHistDtl.Count; ++
                  local.PgmHist.Item.PgmHistDtl.Index)
                {
                  if (local.PgmsForSp.IsFull)
                  {
                    break;
                  }

                  // -- We only want to pass programs that started before the 
                  // end of the month to the program determination cab.
                  // It will then only process the program derivation matrix up 
                  // to that point in time.
                  if (!Lt(local.Eom.Date,
                    local.PgmHist.Item.PgmHistDtl.Item.PgmHistDtlPersonProgram.
                      EffectiveDate))
                  {
                    MovePersonProgram(local.PgmHist.Item.PgmHistDtl.Item.
                      PgmHistDtlPersonProgram,
                      local.PgmsForSp.Update.PgmHistDtlSpPersonProgram);
                    local.PgmsForSp.Update.PgmHistDtlSpProgram.Assign(
                      local.PgmHist.Item.PgmHistDtl.Item.PgmHistDtlProgram);
                  }

                  local.PgmsForSp.Next();
                }

                break;
              }
            }

            // : Get the program for the current supported person.
            // : pass in date that is the last date of the reporting month.
            UseFnB621DetermineProgramForSp();
            local.DebtDetail.BalanceDueAmt = 0;

            // : Determine if the debt is current or arrears.
            if (entities.DebtDetail.BalanceDueAmt == 0)
            {
              // :  If the debt bal is zero, it may have been arrears and 
              // retired during the report month,
              //    so the indicator should be set to arrears.
              if (!Lt(entities.DebtDetail.RetiredDt, local.Bom.Date))
              {
                if (AsChar(entities.ObligationType.Classification) == 'A')
                {
                  if (Lt(entities.DebtDetail.DueDt, local.Bom.Date))
                  {
                    local.CurrArrInd.Flag = "A";
                  }
                  else
                  {
                    local.CurrArrInd.Flag = "C";
                  }
                }
                else
                {
                  local.CurrArrInd.Flag = "A";
                }
              }
              else
              {
                local.CurrArrInd.Flag = "C";
              }
            }
            else if (AsChar(entities.ObligationType.Classification) == 'A')
            {
              if (Lt(entities.DebtDetail.DueDt, local.Bom.Date))
              {
                local.CurrArrInd.Flag = "A";
              }
              else
              {
                local.CurrArrInd.Flag = "C";
              }
            }
            else
            {
              local.CurrArrInd.Flag = "A";
            }

            // : We need to read debts with zero bal because we need to retrieve
            // collections for those debts later.
            //   But we do not need to process a zero balance debt for the 
            // report.
            // ** CQ#619 Commented the below code **
            // ** CQ#619 Changes Begin Here **
            if (IsEmpty(local.JsSecondObligation.Flag))
            {
              local.DebtDetail.BalanceDueAmt =
                entities.DebtDetail.BalanceDueAmt;
              local.TotalNonDebt.Amount = 0;

              // ** Read Debt Adjustments for the debt
              foreach(var item3 in ReadObligationTransactionRln())
              {
                if (ReadObligationTransaction())
                {
                  if (Equal(entities.NonDebt.Type1, "DA"))
                  {
                    if (AsChar(entities.NonDebt.DebtAdjustmentType) == 'I')
                    {
                      local.TotalNonDebt.Amount += entities.NonDebt.Amount;
                    }
                    else
                    {
                      local.TotalNonDebt.Amount -= entities.NonDebt.Amount;
                    }
                  }
                }
                else
                {
                  // Continue
                }
              }

              if (local.TotalNonDebt.Amount != 0)
              {
                // Subtract the total debt adjustments from the debt and this 
                // should take care of Increase OR Decrease
                local.DebtDetail.BalanceDueAmt -= local.TotalNonDebt.Amount;

                if (AsChar(local.VerifyThisRun.Flag) == 'Y')
                {
                  local.DebtType.Comment = "Lines 4, 5";
                  local.DebtType.ObligationType = entities.ObligationType.Code;
                  local.DebtType.CollectionAmount = 0;
                  local.DebtType.CollectionDate = local.Null1.Date;
                  local.DebtType.DebtDetailBaldue = local.TotalNonDebt.Amount;
                  local.DebtType.Dddd = entities.DebtDetail.DueDt;
                  local.DebtType.ProgramType = "";
                  local.DebtType.LineNumber = 4;
                  UseFnB621CreateVerifyRec2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // <===========Write error message to error report ========>
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Legal_Action Standard_Number is: " + entities
                      .LegalAction.StandardNumber;
                    UseCabErrorReport2();

                    goto ReadEach;
                  }
                }
              }

              if (local.DebtDetail.BalanceDueAmt != 0)
              {
                // : Add to Owed total fields.
                local.LowedTotal.TotalCurrency += local.DebtDetail.
                  BalanceDueAmt;

                if (AsChar(local.CurrArrInd.Flag) == 'A')
                {
                  local.LowedArrearsTotal.TotalCurrency += local.DebtDetail.
                    BalanceDueAmt;
                }
                else
                {
                  local.LowedCurrTotal.TotalCurrency += local.DebtDetail.
                    BalanceDueAmt;
                }

                if (Equal(local.Program.Code, "AF") || Equal
                  (local.Program.Code, "FC") || Equal
                  (local.Program.Code, "NF") || Equal
                  (local.Program.Code, "NC"))
                {
                  if (AsChar(local.CurrArrInd.Flag) == 'A')
                  {
                    local.LowedArrearsState.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                  else
                  {
                    local.LowedCurrentState.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                }
                else if (Equal(local.Program.Code, "NA"))
                {
                  if (AsChar(local.CurrArrInd.Flag) == 'A')
                  {
                    local.LowedArrearsFamily.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                  else
                  {
                    local.LowedCurrentFamily.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                }
                else if (Equal(local.Program.Code, "AFI") || Equal
                  (local.Program.Code, "FCI"))
                {
                  if (AsChar(local.CurrArrInd.Flag) == 'A')
                  {
                    local.LowedArrearsIState.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                  else
                  {
                    local.LowedCurrentIState.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                }
                else if (Equal(local.Program.Code, "NAI"))
                {
                  if (AsChar(local.CurrArrInd.Flag) == 'A')
                  {
                    local.LowedArrearsIFamily.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                  else
                  {
                    local.LowedCurrentIFamily.TotalCurrency += local.DebtDetail.
                      BalanceDueAmt;
                  }
                }

                // CQ#619 write verify rec for each debt
                if (AsChar(local.VerifyThisRun.Flag) == 'Y')
                {
                  if (Equal(local.Program.Code, "AF") || Equal
                    (local.Program.Code, "FC") || Equal
                    (local.Program.Code, "NF") || Equal
                    (local.Program.Code, "NC"))
                  {
                    if (AsChar(local.CurrArrInd.Flag) == 'A')
                    {
                      local.DebtType.Comment = "Lines 2, 15, 19, 20";
                    }
                    else
                    {
                      local.DebtType.Comment = "Lines 2, 10, 14, 20";
                    }
                  }
                  else if (Equal(local.Program.Code, "NA"))
                  {
                    if (AsChar(local.CurrArrInd.Flag) == 'A')
                    {
                      local.DebtType.Comment = "Lines 2, 16, 19, 20";
                    }
                    else
                    {
                      local.DebtType.Comment = "Lines 2, 11, 14, 20";
                    }
                  }
                  else if (Equal(local.Program.Code, "AFI") || Equal
                    (local.Program.Code, "FCI"))
                  {
                    if (AsChar(local.CurrArrInd.Flag) == 'A')
                    {
                      local.DebtType.Comment = "Lines 2, 17, 19, 20";
                    }
                    else
                    {
                      local.DebtType.Comment = "Lines 2, 12, 14, 20";
                    }
                  }
                  else if (Equal(local.Program.Code, "NAI"))
                  {
                    if (AsChar(local.CurrArrInd.Flag) == 'A')
                    {
                      local.DebtType.Comment = "Lines 2, 18, 19, 20";
                    }
                    else
                    {
                      local.DebtType.Comment = "Lines 2, 13, 14, 20";
                    }
                  }

                  local.DebtType.ObligationType = entities.ObligationType.Code;
                  local.DebtType.CollectionAmount = 0;
                  local.DebtType.CollectionDate = local.Null1.Date;
                  local.DebtType.DebtDetailBaldue =
                    local.DebtDetail.BalanceDueAmt;
                  local.DebtType.Dddd = entities.DebtDetail.DueDt;
                  local.DebtType.ProgramType = local.Program.Code;
                  local.DebtType.LineNumber = 2;
                  UseFnB621CreateVerifyRec2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // <===========Write error message to error report ========>
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Legal_Action Standard_Number is: " + entities
                      .LegalAction.StandardNumber;
                    UseCabErrorReport2();

                    goto ReadEach;
                  }
                }
              }
            }

            // ** CQ#619 Changes End  Here **
            // : Read collections for the debt.
            //   See note below for reason for not qualifying collection date 
            // and collection
            //   adjustment date as less than end of report month.
            // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify
            // to Uncommitted Browse **
            foreach(var item3 in ReadCollectionCollectionType())
            {
              // ** CQ619 Reporting month collection flag will be set depending 
              // on current collection activity **
              local.ReportingMonthCollection.Flag = "N";

              // : If the collection was adjusted, subtract it from the total 
              // fields.
              if (AsChar(entities.Collection.AdjustedInd) == 'Y')
              {
                local.Collection.Amount = -entities.Collection.Amount;
              }
              else
              {
                local.Collection.Amount = entities.Collection.Amount;
              }

              // : The collection types in the following IF statement are the 
              // only collection
              //   types to be considered in calculating the collected fields on
              // the report.
              //   However, we need all collection types later on when the 
              // beginning of
              //   the month debt balances are calculated.  Collections get 
              // added back
              //   Same for checking the collection date against end of report 
              // period.
              //   We only want to look at collections that happened during the 
              // report period
              //   here, but when calculating the debt balance at the beginning 
              // of the report period,
              //   we want to adjust debt balances according to all collection 
              // activity
              //   that happened since the beginning of the report period.
              if ((entities.CollectionType.SequentialIdentifier == local
                .Hardcoded.HardcodeRegular.SequentialIdentifier || entities
                .CollectionType.SequentialIdentifier == local
                .Hardcoded.HardcodeIncomeWithholdin.SequentialIdentifier || entities
                .CollectionType.SequentialIdentifier == local
                .Hardcoded.HardcodeDirPmtAp.SequentialIdentifier || entities
                .CollectionType.SequentialIdentifier == local
                .Hardcoded.HardcodeDirPmtCru.SequentialIdentifier || entities
                .CollectionType.SequentialIdentifier == local
                .Hardcoded.HardcodeDirPmtCt.SequentialIdentifier) && (
                  !Lt(local.Eom.Timestamp, entities.Collection.CreatedTmst) && AsChar
                (entities.Collection.AdjustedInd) == 'N' || !
                Lt(local.Eom.Date, entities.Collection.CollectionAdjustmentDt) &&
                AsChar(entities.Collection.AdjustedInd) == 'Y'))
              {
                // : Reporting month collection
                local.ReportingMonthCollection.Flag = "Y";

                if (AsChar(entities.Collection.AdjustedInd) == 'N')
                {
                  // : Number of paying orders is set to 1 for each order, 
                  // regardless of how many collections exist.
                  local.LnumberOfPayingOrders.Count = 1;
                }

                // : Add to Collection total fields.
                local.LcollectedTotal.TotalCurrency += local.Collection.Amount;

                if (AsChar(entities.Collection.AppliedToCode) == 'A')
                {
                  local.LcollArrearsTotal.TotalCurrency += local.Collection.
                    Amount;
                }
                else
                {
                  local.LcollCurrTotal.TotalCurrency += local.Collection.Amount;
                }

                if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                  (entities.Collection.ProgramAppliedTo, "FC") || Equal
                  (entities.Collection.ProgramAppliedTo, "NF") || Equal
                  (entities.Collection.ProgramAppliedTo, "NC"))
                {
                  if (AsChar(entities.Collection.AppliedToCode) == 'A')
                  {
                    local.LcollArrearsState.TotalCurrency += local.Collection.
                      Amount;
                  }
                  else
                  {
                    local.LcollCurrentState.TotalCurrency += local.Collection.
                      Amount;
                  }
                }
                else if (Equal(entities.Collection.ProgramAppliedTo, "NA"))
                {
                  if (AsChar(entities.Collection.AppliedToCode) == 'A')
                  {
                    local.LcollArrearsFamily.TotalCurrency += local.Collection.
                      Amount;
                  }
                  else
                  {
                    local.LcollCurrentFamily.TotalCurrency += local.Collection.
                      Amount;
                  }
                }
                else if (Equal(entities.Collection.ProgramAppliedTo, "AFI") || Equal
                  (entities.Collection.ProgramAppliedTo, "FCI"))
                {
                  if (AsChar(entities.Collection.AppliedToCode) == 'A')
                  {
                    local.LcollArrearsIState.TotalCurrency += local.Collection.
                      Amount;
                  }
                  else
                  {
                    local.LcollCurrentIState.TotalCurrency += local.Collection.
                      Amount;
                  }
                }
                else if (Equal(entities.Collection.ProgramAppliedTo, "NAI"))
                {
                  if (AsChar(entities.Collection.AppliedToCode) == 'A')
                  {
                    local.LcollArrearsIFamily.TotalCurrency += local.Collection.
                      Amount;
                  }
                  else
                  {
                    local.LcollCurrentIFamily.TotalCurrency += local.Collection.
                      Amount;
                  }
                }

                // : Check to see if this run is to be verified.  Following code
                // is normally
                //   bypassed in production (PPI parm field should be all spaces
                // ).
                if (AsChar(local.VerifyThisRun.Flag) == 'Y')
                {
                  local.CollectionType.CollectionAmount =
                    local.Collection.Amount;
                  local.CollectionType.ProgramType =
                    entities.Collection.ProgramAppliedTo;
                  local.CollectionType.Dddd = entities.DebtDetail.DueDt;
                  local.CollectionType.ObligationType =
                    entities.ObligationType.Code;

                  if (AsChar(entities.Collection.AdjustedInd) == 'Y')
                  {
                    local.CollectionType.CollectionDate =
                      entities.Collection.CollectionAdjustmentDt;
                    local.CollectionType.Comment = "Lines";
                  }
                  else
                  {
                    local.CollectionType.CollectionDate =
                      Date(entities.Collection.CreatedTmst);
                    local.CollectionType.Comment = "Lines 5,";
                  }

                  if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                    (entities.Collection.ProgramAppliedTo, "FC") || Equal
                    (entities.Collection.ProgramAppliedTo, "NF") || Equal
                    (entities.Collection.ProgramAppliedTo, "NC"))
                  {
                    if (AsChar(entities.Collection.AppliedToCode) == 'A')
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 26, 30, 31";
                        
                    }
                    else
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 21, 25, 31";
                        
                    }
                  }
                  else if (Equal(entities.Collection.ProgramAppliedTo, "NA"))
                  {
                    if (AsChar(entities.Collection.AppliedToCode) == 'A')
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 27, 30, 31";
                        
                    }
                    else
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 22, 25, 31";
                        
                    }
                  }
                  else if (Equal(entities.Collection.ProgramAppliedTo, "AFI") ||
                    Equal(entities.Collection.ProgramAppliedTo, "FCI"))
                  {
                    if (AsChar(entities.Collection.AppliedToCode) == 'A')
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 28, 30, 31";
                        
                    }
                    else
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 23, 25, 31";
                        
                    }
                  }
                  else if (Equal(entities.Collection.ProgramAppliedTo, "NAI"))
                  {
                    if (AsChar(entities.Collection.AppliedToCode) == 'A')
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 29, 30, 31";
                        
                    }
                    else
                    {
                      local.CollectionType.Comment =
                        TrimEnd(local.CollectionType.Comment) + "3, 24, 25, 31";
                        
                    }
                  }

                  UseFnB621CreateVerifyRec1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // <===========Write error message to error report ========>
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail =
                      "Legal_Action Standard_Number is: " + entities
                      .LegalAction.StandardNumber;
                    UseCabErrorReport2();

                    goto ReadEach;
                  }
                }
              }

              // : Now must add collection back to the total owed fields, so 
              // that these fields are not understated.
              // ** CQ#619 Below line commented on 01-14-08
              // : Add to Owed total fields.
              local.LowedTotal.TotalCurrency += local.Collection.Amount;

              // : CQ#619 Commented the below code and changed it to program 
              // applied to and applied to code from collection
              // CQ#619 Check program applied to and applied to code from 
              // collection, also write verify rec for each collection added
              // back as debt
              // ****** Changes Begin Here ******
              if (AsChar(local.ReportingMonthCollection.Flag) == 'Y')
              {
                local.Decided.ProgramAppliedTo =
                  entities.Collection.ProgramAppliedTo;
                local.Decided.AppliedToCode = entities.Collection.AppliedToCode;
              }
              else
              {
                local.Decided.ProgramAppliedTo = local.Program.Code;
                local.Decided.AppliedToCode = local.CurrArrInd.Flag;
              }

              if (AsChar(local.Decided.AppliedToCode) == 'A')
              {
                local.LowedArrearsTotal.TotalCurrency += local.Collection.
                  Amount;
              }
              else
              {
                local.LowedCurrTotal.TotalCurrency += local.Collection.Amount;
              }

              if (Equal(local.Decided.ProgramAppliedTo, "AF") || Equal
                (local.Decided.ProgramAppliedTo, "FC") || Equal
                (local.Decided.ProgramAppliedTo, "NF") || Equal
                (local.Decided.ProgramAppliedTo, "NC"))
              {
                if (AsChar(local.Decided.AppliedToCode) == 'A')
                {
                  local.LowedArrearsState.TotalCurrency += local.Collection.
                    Amount;
                }
                else
                {
                  local.LowedCurrentState.TotalCurrency += local.Collection.
                    Amount;
                }
              }
              else if (Equal(local.Decided.ProgramAppliedTo, "NA"))
              {
                if (AsChar(local.Decided.AppliedToCode) == 'A')
                {
                  local.LowedArrearsFamily.TotalCurrency += local.Collection.
                    Amount;
                }
                else
                {
                  local.LowedCurrentFamily.TotalCurrency += local.Collection.
                    Amount;
                }
              }
              else if (Equal(local.Decided.ProgramAppliedTo, "AFI") || Equal
                (local.Decided.ProgramAppliedTo, "FCI"))
              {
                if (AsChar(local.Decided.AppliedToCode) == 'A')
                {
                  local.LowedArrearsIState.TotalCurrency += local.Collection.
                    Amount;
                }
                else
                {
                  local.LowedCurrentIState.TotalCurrency += local.Collection.
                    Amount;
                }
              }
              else if (Equal(local.Decided.ProgramAppliedTo, "NAI"))
              {
                if (AsChar(local.Decided.AppliedToCode) == 'A')
                {
                  local.LowedArrearsIFamily.TotalCurrency += local.Collection.
                    Amount;
                }
                else
                {
                  local.LowedCurrentIFamily.TotalCurrency += local.Collection.
                    Amount;
                }
              }

              // : Write verify rec
              if (AsChar(local.VerifyThisRun.Flag) == 'Y')
              {
                if (Equal(local.Decided.ProgramAppliedTo, "AF") || Equal
                  (local.Decided.ProgramAppliedTo, "FC") || Equal
                  (local.Decided.ProgramAppliedTo, "NF") || Equal
                  (local.Decided.ProgramAppliedTo, "NC"))
                {
                  if (AsChar(local.Decided.AppliedToCode) == 'A')
                  {
                    local.DebtType.Comment = "Lines 2, 15, 19, 20";
                  }
                  else
                  {
                    local.DebtType.Comment = "Lines 2, 10, 14, 20";
                  }
                }
                else if (Equal(local.Decided.ProgramAppliedTo, "NA"))
                {
                  if (AsChar(local.Decided.AppliedToCode) == 'A')
                  {
                    local.DebtType.Comment = "Lines 2, 16, 19, 20";
                  }
                  else
                  {
                    local.DebtType.Comment = "Lines 2, 11, 14, 20";
                  }
                }
                else if (Equal(local.Decided.ProgramAppliedTo, "AFI") || Equal
                  (local.Decided.ProgramAppliedTo, "FCI"))
                {
                  if (AsChar(local.Decided.AppliedToCode) == 'A')
                  {
                    local.DebtType.Comment = "Lines 2, 17, 19, 20";
                  }
                  else
                  {
                    local.DebtType.Comment = "Lines 2, 12, 14, 20";
                  }
                }
                else if (Equal(local.Decided.ProgramAppliedTo, "NAI"))
                {
                  if (AsChar(local.Decided.AppliedToCode) == 'A')
                  {
                    local.DebtType.Comment = "Lines 2, 18, 19, 20";
                  }
                  else
                  {
                    local.DebtType.Comment = "Lines 2, 13, 14, 20";
                  }
                }

                local.DebtType.ObligationType = entities.ObligationType.Code;
                local.DebtType.CollectionAmount = 0;
                local.DebtType.CollectionDate = local.Null1.Date;
                local.DebtType.DebtDetailBaldue = local.Collection.Amount;
                local.DebtType.Dddd = entities.DebtDetail.DueDt;
                local.DebtType.ProgramType = local.Decided.ProgramAppliedTo;
                local.DebtType.LineNumber = 3;

                if (AsChar(local.JsSecondObligation.Flag) == 'Y')
                {
                  local.DebtType.Comment = TrimEnd(local.DebtType.Comment) + " " +
                    "Second ob. of a J/S obligation.";
                }

                UseFnB621CreateVerifyRec2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // <===========Write error message to error report ========>
                  local.EabFileHandling.Action = "WRITE";
                  local.NeededToWrite.RptDetail =
                    "Legal_Action Standard_Number is: " + entities
                    .LegalAction.StandardNumber;
                  UseCabErrorReport2();

                  goto ReadEach;
                }
              }

              // ****** Changes End   Here ******
            }

            // : CQ#619 Changed on Dec 13, 2007 the below balance due amt 
            // condition from "Greater than" to "Not Equal to"
            // : CQ#619 Commented the below code and this code is copied in the 
            // debt and collection section
          }
        }

        // : If the total fields checked here are zero, and there are no active 
        // accrual instructions on the court order, no need to go further with
        // this Legal Action.
        if (local.LcollectedTotal.TotalCurrency == 0 && local
          .LowedTotal.TotalCurrency == 0 && IsEmpty
          (local.JsSecondObligation.Flag))
        {
          // ** SWCOAMX Changed the properties on 01-02-08 from Do not Specify 
          // to Uncommitted Browse **
          if (ReadAccrualInstructions())
          {
            // : write verify rec for line 1 in this situation, as nothing else 
            // will be written.
            //  (no collections or debts were found).
            if (AsChar(local.VerifyThisRun.Flag) == 'Y')
            {
              local.DebtType.CollectionAmount = 0;
              local.DebtType.CollectionDate = local.Null1.Date;
              local.DebtType.DebtDetailBaldue = 0;
              local.DebtType.Dddd = local.Null1.Date;
              local.DebtType.ProgramType = "";
              local.DebtType.Comment = "Line 1, 5";
              local.DebtType.LineNumber = 1;
              UseFnB621CreateVerifyRec2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // <===========Write error message to error report ========>
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Legal_Action Standard_Number is: " + entities
                  .LegalAction.StandardNumber;
                UseCabErrorReport2();

                continue;
              }
            }
          }
          else
          {
            // : There are no active accrual instructions on the court order, so
            // no need to go
            //   further with this Legal Action.
            // : CQ#619 write verify rec
            // ****** Changes Begin Here ******
            if (AsChar(local.VerifyThisRun.Flag) == 'Y')
            {
              local.DebtType.CollectionAmount = 0;
              local.DebtType.CollectionDate = local.Null1.Date;
              local.DebtType.DebtDetailBaldue = 0;
              local.DebtType.Dddd = local.Null1.Date;
              local.DebtType.ProgramType = "";
              local.DebtType.Comment = "Line 1, 10";
              local.DebtType.LineNumber = 1;
              UseFnB621CreateVerifyRec2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // <===========Write error message to error report ========>
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Legal_Action Standard_Number is: " + entities
                  .LegalAction.StandardNumber;
                UseCabErrorReport2();

                continue;
              }
            }

            // ****** Changes End   Here ******
            continue;
          }
        }

        if (AsChar(local.VerifyThisRun.Flag) == 'Y')
        {
        }
        else
        {
          // : If we are writing records to the verify table for this run, the 
          // referral was
          //   already retrieved. But it is more efficient to retrieve it at 
          // this point, so if the
          //  verify flag is not Y (as it will be in prod most of the time), get
          // the referral here.
          UseFnB621GetReferral();
        }

        if (AsChar(local.VerifyThisRun.Flag) == 'Y')
        {
          // : Write a verify rec on legal action level if legal action has an 
          // unlocated case unit.
          if (local.LnumberOfOrdersInLocate.Count > 0)
          {
            local.DebtType.CollectionAmount = 0;
            local.DebtType.CollectionDate = local.Null1.Date;
            local.DebtType.DebtDetailBaldue = 0;
            local.DebtType.Dddd = local.Null1.Date;
            local.DebtType.ProgramType = "";
            local.DebtType.Comment = "Line 8";
            local.DebtType.LineNumber = 8;
            UseFnB621CreateVerifyRec2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // <===========Write error message to error report ========>
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Legal_Action Standard_Number is: " + entities
                .LegalAction.StandardNumber;
              UseCabErrorReport2();

              continue;
            }
          }
        }

        // : Write to sequential file.
        local.LtotalOrdersReferred.Count = 1;
        local.EabFileHandling.Action = "WRITE";
        UseFnEabSwexfw17CollPotWrite1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.LtotalOrdersReferred.Count = 0;
        local.LnumberOfOrdersInLocate.Count = 0;
        local.LnumberOfPayingOrders.Count = 0;
        local.LcollArrearsFamily.TotalCurrency = 0;
        local.LcollArrearsIFamily.TotalCurrency = 0;
        local.LcollArrearsIState.TotalCurrency = 0;
        local.LcollArrearsState.TotalCurrency = 0;
        local.LcollArrearsTotal.TotalCurrency = 0;
        local.LcollCurrTotal.TotalCurrency = 0;
        local.LcollCurrentFamily.TotalCurrency = 0;
        local.LcollCurrentIFamily.TotalCurrency = 0;
        local.LcollCurrentIState.TotalCurrency = 0;
        local.LcollCurrentState.TotalCurrency = 0;
        local.LcollectedTotal.TotalCurrency = 0;
        local.LowedArrearsFamily.TotalCurrency = 0;
        local.LowedArrearsIFamily.TotalCurrency = 0;
        local.LowedArrearsIState.TotalCurrency = 0;
        local.LowedArrearsState.TotalCurrency = 0;
        local.LowedArrearsTotal.TotalCurrency = 0;
        local.LowedCurrTotal.TotalCurrency = 0;
        local.LowedCurrentFamily.TotalCurrency = 0;
        local.LowedCurrentIFamily.TotalCurrency = 0;
        local.LowedCurrentIState.TotalCurrency = 0;
        local.LowedCurrentState.TotalCurrency = 0;
        local.LowedTotal.TotalCurrency = 0;

        // *** Commit if it's time
        if (local.ProcessCountToCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.ProgramName = global.UserId;
          local.ProgramCheckpointRestart.RestartInfo =
            entities.LegalAction.StandardNumber;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail =
              "Error in update checkpoint restart.  Exitstate msg is: " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.NeededToWrite.RptDetail =
              "Error in External to do a commit for: " + local
              .NeededToWrite.RptDetail;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ProcessCountToCommit.Count = 0;
        }

        // This is the end of the legal action control break code.
      }

      ++local.ProcessCountToCommit.Count;

      // This is the Legal Action Read Each loop
ReadEach:
      ;
    }

    // END OF PROCESSING
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Successful end of job, so update checkpoint restart.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
        local.ExitStateWorkArea.Message;
      UseCabErrorReport2();
    }

    // **** START OF REPORT DSN CLOSE PROCESS ****
    // : Close the output file
    local.EabFileHandling.Action = "CLOSE";
    UseFnEabSwexfw17CollPotWrite3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Collection Potential file was unsuccessfu.";
      UseCabErrorReport2();
    }

    // **** END OF REPORT DSN CLOSE PROCESS ****
    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveOfPgms(Local.OfPgmsGroup source,
    FnB621BuildPgmHstForCo.Import.OfPgmsGroup target)
  {
    target.OfPgms1.Assign(source.OfPgms1);
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePgmHist(FnB621BuildPgmHstForCo.Export.
    PgmHistGroup source, Local.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl);
  }

  private static void MovePgmHistDtl(FnB621BuildPgmHstForCo.Export.
    PgmHistDtlGroup source, Local.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmsForSpToPgmHistDtl(Local.PgmsForSpGroup source,
    FnB621DetermineProgramForSp.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlSpProgram);
    MovePersonProgram(source.PgmHistDtlSpPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private static void MoveStatsVerifi(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.ParentId = source.ParentId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.Dddd = source.Dddd;
    target.DebtDetailBaldue = source.DebtDetailBaldue;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseCabDetermineReportingDates()
  {
    var useImport = new CabDetermineReportingDates.Import();
    var useExport = new CabDetermineReportingDates.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabDetermineReportingDates.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Bom, local.Bom);
    MoveDateWorkArea(useExport.Eom, local.Eom);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Bom.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.Bom.YearMonth = useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB621BuildPgmHstForCo()
  {
    var useImport = new FnB621BuildPgmHstForCo.Import();
    var useExport = new FnB621BuildPgmHstForCo.Export();

    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.MaximumDiscontinue.Date = local.Max.Date;
    local.OfPgms.CopyTo(useImport.OfPgms, MoveOfPgms);

    Call(FnB621BuildPgmHstForCo.Execute, useImport, useExport);

    useExport.PgmHist.CopyTo(local.PgmHist, MovePgmHist);
  }

  private void UseFnB621CreateVerifyRec1()
  {
    var useImport = new FnB621CreateVerifyRec.Import();
    var useExport = new FnB621CreateVerifyRec.Export();

    MoveStatsVerifi(local.CollectionType, useImport.StatsVerifi);

    Call(FnB621CreateVerifyRec.Execute, useImport, useExport);
  }

  private void UseFnB621CreateVerifyRec2()
  {
    var useImport = new FnB621CreateVerifyRec.Import();
    var useExport = new FnB621CreateVerifyRec.Export();

    MoveStatsVerifi(local.DebtType, useImport.StatsVerifi);

    Call(FnB621CreateVerifyRec.Execute, useImport, useExport);
  }

  private void UseFnB621DetermineProgramForSp()
  {
    var useImport = new FnB621DetermineProgramForSp.Import();
    var useExport = new FnB621DetermineProgramForSp.Export();

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    MoveObligation(entities.Obligation, useImport.Obligation);
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Collection.Date = local.Eom.Date;
    local.PgmsForSp.CopyTo(useImport.PgmHistDtl, MovePgmsForSpToPgmHistDtl);
    useImport.HardcodedAccruingClass.Classification =
      local.Hardcoded.HardcodeAccruing.Classification;
    MoveProgram(local.Hardcoded.HardcodedAf, useImport.HardcodedAf);
    MoveProgram(local.Hardcoded.HardcodedAfi, useImport.HardcodedAfi);
    MoveProgram(local.Hardcoded.HardcodedFci, useImport.HardcodedFci);
    MoveProgram(local.Hardcoded.HardcodedNa, useImport.HardcodedNaProgram);
    MoveProgram(local.Hardcoded.HardcodedNai, useImport.HardcodedNai);
    MoveProgram(local.Hardcoded.HardcodedNc, useImport.HardcodedNc);
    MoveProgram(local.Hardcoded.HardcodedNf, useImport.HardcodedNf);
    MoveProgram(local.Hardcoded.HardcodedMai, useImport.HardcodedMai);
    useImport.HardcodedFc.Assign(local.Hardcoded.HardcodedFc);
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedCa.ProgramState =
      local.Hardcoded.HardcodeCa.ProgramState;
    useImport.HardcodedNaDprProgram.ProgramState =
      local.Hardcoded.HardcodeNa.ProgramState;
    useImport.HardcodedTa.ProgramState =
      local.Hardcoded.HardcodeTa.ProgramState;
    useImport.HardcodedPa.ProgramState =
      local.Hardcoded.HardcodePa.ProgramState;

    Call(FnB621DetermineProgramForSp.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnB621GetReferral()
  {
    var useImport = new FnB621GetReferral.Import();
    var useExport = new FnB621GetReferral.Export();

    useImport.Tribunal.Identifier = entities.Tribunal.Identifier;
    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.Bom.Assign(local.Bom);
    MoveDateWorkArea(local.Eom, useImport.Eom);

    Call(FnB621GetReferral.Execute, useImport, useExport);

    local.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    local.OfficeServiceProvider.RoleCode =
      useExport.OfficeServiceProvider.RoleCode;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider);
    local.ReferralType.Text2 = useExport.ReferralType.Text2;
  }

  private void UseFnEabSwexfw17CollPotWrite1()
  {
    var useImport = new FnEabSwexfw17CollPotWrite.Import();
    var useExport = new FnEabSwexfw17CollPotWrite.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.TotalOrdersReferred.Count = local.LtotalOrdersReferred.Count;
    useImport.NumberOfPayingOrders.Count = local.LnumberOfPayingOrders.Count;
    useImport.OrdersInLocate.Count = local.LnumberOfOrdersInLocate.Count;
    useImport.Fips.Assign(local.Fips);
    useImport.Tribunal.JudicialDistrict = local.Tribunal.JudicialDistrict;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.ReferralType.Text2 = local.ReferralType.Text2;
    useImport.CollCurrentState.TotalCurrency =
      local.LcollCurrentState.TotalCurrency;
    useImport.CollCurrentFamily.TotalCurrency =
      local.LcollCurrentFamily.TotalCurrency;
    useImport.CollCurrentIState.TotalCurrency =
      local.LcollCurrentIState.TotalCurrency;
    useImport.CollCurrentIFamily.TotalCurrency =
      local.LcollCurrentIFamily.TotalCurrency;
    useImport.CollArrearsState.TotalCurrency =
      local.LcollArrearsState.TotalCurrency;
    useImport.CollArrearsFamily.TotalCurrency =
      local.LcollArrearsFamily.TotalCurrency;
    useImport.CollArrearsIState.TotalCurrency =
      local.LcollArrearsIState.TotalCurrency;
    useImport.CollArrearsIFamily.TotalCurrency =
      local.LcollArrearsIFamily.TotalCurrency;
    useImport.OwedCurrentState.TotalCurrency =
      local.LowedCurrentState.TotalCurrency;
    useImport.OwedCurrentFamily.TotalCurrency =
      local.LowedCurrentFamily.TotalCurrency;
    useImport.OwedCurrentIState.TotalCurrency =
      local.LowedCurrentIState.TotalCurrency;
    useImport.OwedCurrentIFamily.TotalCurrency =
      local.LowedCurrentIFamily.TotalCurrency;
    useImport.OwedArrearsState.TotalCurrency =
      local.LowedArrearsState.TotalCurrency;
    useImport.OwedArrearsFamily.TotalCurrency =
      local.LowedArrearsFamily.TotalCurrency;
    useImport.OwedArrearsIState.TotalCurrency =
      local.LowedArrearsIState.TotalCurrency;
    useImport.OwedArrearsIFamily.TotalCurrency =
      local.LowedArrearsIFamily.TotalCurrency;
    useImport.OwedCurrTotal.TotalCurrency = local.LowedCurrTotal.TotalCurrency;
    useImport.OwedArrearsTotal.TotalCurrency =
      local.LowedArrearsTotal.TotalCurrency;
    useImport.OwedTotal.TotalCurrency = local.LowedTotal.TotalCurrency;
    useImport.CollCurrTotal.TotalCurrency = local.LcollCurrTotal.TotalCurrency;
    useImport.CollArrearsTotal.TotalCurrency =
      local.LcollArrearsTotal.TotalCurrency;
    useImport.CollectedTotal.TotalCurrency =
      local.LcollectedTotal.TotalCurrency;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabSwexfw17CollPotWrite.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnEabSwexfw17CollPotWrite2()
  {
    var useImport = new FnEabSwexfw17CollPotWrite.Import();
    var useExport = new FnEabSwexfw17CollPotWrite.Export();

    useImport.LegalAction.StandardNumber = local.Restart.StandardNumber;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(FnEabSwexfw17CollPotWrite.Execute, useImport, useExport);
  }

  private void UseFnEabSwexfw17CollPotWrite3()
  {
    var useImport = new FnEabSwexfw17CollPotWrite.Import();
    var useExport = new FnEabSwexfw17CollPotWrite.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(FnEabSwexfw17CollPotWrite.Execute, useImport, useExport);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", local.Bom.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Bom.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnly.Populated);
    entities.CollectionType.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.KeyOnly.OtyType);
        db.SetString(command, "otrType", entities.KeyOnly.Type1);
        db.
          SetInt32(command, "otrId", entities.KeyOnly.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", entities.KeyOnly.CpaType);
        db.SetString(command, "cspNumber", entities.KeyOnly.CspNumber);
        db.SetInt32(command, "obgId", entities.KeyOnly.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst", local.Bom.Timestamp.GetValueOrDefault());
        db.SetDate(command, "collAdjDt", local.Bom.Date.GetValueOrDefault());
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
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 19);
        entities.CollectionType.Populated = true;
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

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Ap.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebtCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.KeyOnly.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadDebtDetailDebtCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(command, "dueDt", local.Eom.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.KeyOnly.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.KeyOnly.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.KeyOnly.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.KeyOnly.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 11);
        entities.KeyOnly.CspSupNumber = db.GetNullableString(reader, 12);
        entities.Child.Number = db.GetString(reader, 12);
        entities.KeyOnly.CpaSupType = db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        entities.KeyOnly.Populated = true;
        entities.Child.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.KeyOnly.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.KeyOnly.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.KeyOnly.CpaSupType);

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 1);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.Restart.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Classification = db.GetString(reader, 8);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 9);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.NonDebt.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationTransactionRln.OtyTypeSecondary);
        db.SetString(
          command, "obTrnTyp", entities.ObligationTransactionRln.OtrType);
        db.SetInt32(
          command, "obTrnId", entities.ObligationTransactionRln.OtrGeneratedId);
          
        db.SetString(
          command, "cpaType", entities.ObligationTransactionRln.CpaType);
        db.SetString(
          command, "cspNumber", entities.ObligationTransactionRln.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransactionRln.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.NonDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.NonDebt.CspNumber = db.GetString(reader, 1);
        entities.NonDebt.CpaType = db.GetString(reader, 2);
        entities.NonDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.NonDebt.Type1 = db.GetString(reader, 4);
        entities.NonDebt.Amount = db.GetDecimal(reader, 5);
        entities.NonDebt.DebtAdjustmentType = db.GetString(reader, 6);
        entities.NonDebt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.NonDebt.CpaSupType = db.GetNullableString(reader, 8);
        entities.NonDebt.OtyType = db.GetInt32(reader, 9);
        entities.NonDebt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.NonDebt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.NonDebt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.NonDebt.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.NonDebt.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnly.Populated);
    entities.ObligationTransactionRln.Populated = false;

    return ReadEach("ReadObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.KeyOnly.OtyType);
        db.SetString(command, "otrPType", entities.KeyOnly.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.KeyOnly.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.KeyOnly.CpaType);
        db.SetString(command, "cspPNumber", entities.KeyOnly.CspNumber);
        db.
          SetInt32(command, "obgPGeneratedId", entities.KeyOnly.ObgGeneratedId);
          
        db.SetDateTime(
          command, "createdTmst", local.Eom.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.CreatedTmst =
          db.GetDateTime(reader, 12);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 14);
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);

        return true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      null,
      (db, reader) =>
      {
        if (local.OfPgms.IsFull)
        {
          return false;
        }

        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.InterstateIndicator = db.GetString(reader, 2);
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PgmsForSpGroup group.</summary>
    [Serializable]
    public class PgmsForSpGroup
    {
      /// <summary>
      /// A value of PgmHistDtlSpProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlSpProgram")]
      public Program PgmHistDtlSpProgram
      {
        get => pgmHistDtlSpProgram ??= new();
        set => pgmHistDtlSpProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlSpPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlSpPersonProgram")]
      public PersonProgram PgmHistDtlSpPersonProgram
      {
        get => pgmHistDtlSpPersonProgram ??= new();
        set => pgmHistDtlSpPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlSpProgram;
      private PersonProgram pgmHistDtlSpPersonProgram;
    }

    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A OfPgmsGroup group.</summary>
    [Serializable]
    public class OfPgmsGroup
    {
      /// <summary>
      /// A value of OfPgms1.
      /// </summary>
      [JsonPropertyName("ofPgms1")]
      public Program OfPgms1
      {
        get => ofPgms1 ??= new();
        set => ofPgms1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgms1;
    }

    /// <summary>A HardcodedGroup group.</summary>
    [Serializable]
    public class HardcodedGroup
    {
      /// <summary>
      /// A value of HardcodeAccruing.
      /// </summary>
      [JsonPropertyName("hardcodeAccruing")]
      public ObligationType HardcodeAccruing
      {
        get => hardcodeAccruing ??= new();
        set => hardcodeAccruing = value;
      }

      /// <summary>
      /// A value of HardcodedAf.
      /// </summary>
      [JsonPropertyName("hardcodedAf")]
      public Program HardcodedAf
      {
        get => hardcodedAf ??= new();
        set => hardcodedAf = value;
      }

      /// <summary>
      /// A value of HardcodedAfi.
      /// </summary>
      [JsonPropertyName("hardcodedAfi")]
      public Program HardcodedAfi
      {
        get => hardcodedAfi ??= new();
        set => hardcodedAfi = value;
      }

      /// <summary>
      /// A value of HardcodedFci.
      /// </summary>
      [JsonPropertyName("hardcodedFci")]
      public Program HardcodedFci
      {
        get => hardcodedFci ??= new();
        set => hardcodedFci = value;
      }

      /// <summary>
      /// A value of HardcodedNa.
      /// </summary>
      [JsonPropertyName("hardcodedNa")]
      public Program HardcodedNa
      {
        get => hardcodedNa ??= new();
        set => hardcodedNa = value;
      }

      /// <summary>
      /// A value of HardcodedNai.
      /// </summary>
      [JsonPropertyName("hardcodedNai")]
      public Program HardcodedNai
      {
        get => hardcodedNai ??= new();
        set => hardcodedNai = value;
      }

      /// <summary>
      /// A value of HardcodedNc.
      /// </summary>
      [JsonPropertyName("hardcodedNc")]
      public Program HardcodedNc
      {
        get => hardcodedNc ??= new();
        set => hardcodedNc = value;
      }

      /// <summary>
      /// A value of HardcodedNf.
      /// </summary>
      [JsonPropertyName("hardcodedNf")]
      public Program HardcodedNf
      {
        get => hardcodedNf ??= new();
        set => hardcodedNf = value;
      }

      /// <summary>
      /// A value of HardcodedMai.
      /// </summary>
      [JsonPropertyName("hardcodedMai")]
      public Program HardcodedMai
      {
        get => hardcodedMai ??= new();
        set => hardcodedMai = value;
      }

      /// <summary>
      /// A value of HardcodedFc.
      /// </summary>
      [JsonPropertyName("hardcodedFc")]
      public Program HardcodedFc
      {
        get => hardcodedFc ??= new();
        set => hardcodedFc = value;
      }

      /// <summary>
      /// A value of Hardcoded718B.
      /// </summary>
      [JsonPropertyName("hardcoded718B")]
      public ObligationType Hardcoded718B
      {
        get => hardcoded718B ??= new();
        set => hardcoded718B = value;
      }

      /// <summary>
      /// A value of HardcodeSecondary.
      /// </summary>
      [JsonPropertyName("hardcodeSecondary")]
      public Obligation HardcodeSecondary
      {
        get => hardcodeSecondary ??= new();
        set => hardcodeSecondary = value;
      }

      /// <summary>
      /// A value of HardcodeCa.
      /// </summary>
      [JsonPropertyName("hardcodeCa")]
      public DprProgram HardcodeCa
      {
        get => hardcodeCa ??= new();
        set => hardcodeCa = value;
      }

      /// <summary>
      /// A value of HardcodeNa.
      /// </summary>
      [JsonPropertyName("hardcodeNa")]
      public DprProgram HardcodeNa
      {
        get => hardcodeNa ??= new();
        set => hardcodeNa = value;
      }

      /// <summary>
      /// A value of HardcodeTa.
      /// </summary>
      [JsonPropertyName("hardcodeTa")]
      public DprProgram HardcodeTa
      {
        get => hardcodeTa ??= new();
        set => hardcodeTa = value;
      }

      /// <summary>
      /// A value of HardcodePa.
      /// </summary>
      [JsonPropertyName("hardcodePa")]
      public DprProgram HardcodePa
      {
        get => hardcodePa ??= new();
        set => hardcodePa = value;
      }

      /// <summary>
      /// A value of HardcodeFdirPmt.
      /// </summary>
      [JsonPropertyName("hardcodeFdirPmt")]
      public CashReceiptType HardcodeFdirPmt
      {
        get => hardcodeFdirPmt ??= new();
        set => hardcodeFdirPmt = value;
      }

      /// <summary>
      /// A value of HardcodeFcrtRec.
      /// </summary>
      [JsonPropertyName("hardcodeFcrtRec")]
      public CashReceiptType HardcodeFcrtRec
      {
        get => hardcodeFcrtRec ??= new();
        set => hardcodeFcrtRec = value;
      }

      /// <summary>
      /// A value of HardcodeRegular.
      /// </summary>
      [JsonPropertyName("hardcodeRegular")]
      public CollectionType HardcodeRegular
      {
        get => hardcodeRegular ??= new();
        set => hardcodeRegular = value;
      }

      /// <summary>
      /// A value of HardcodeIncomeWithholdin.
      /// </summary>
      [JsonPropertyName("hardcodeIncomeWithholdin")]
      public CollectionType HardcodeIncomeWithholdin
      {
        get => hardcodeIncomeWithholdin ??= new();
        set => hardcodeIncomeWithholdin = value;
      }

      /// <summary>
      /// A value of HardcodeFeePmt.
      /// </summary>
      [JsonPropertyName("hardcodeFeePmt")]
      public CollectionType HardcodeFeePmt
      {
        get => hardcodeFeePmt ??= new();
        set => hardcodeFeePmt = value;
      }

      /// <summary>
      /// A value of HardcodeDirPmtAp.
      /// </summary>
      [JsonPropertyName("hardcodeDirPmtAp")]
      public CollectionType HardcodeDirPmtAp
      {
        get => hardcodeDirPmtAp ??= new();
        set => hardcodeDirPmtAp = value;
      }

      /// <summary>
      /// A value of HardcodeCollAgency.
      /// </summary>
      [JsonPropertyName("hardcodeCollAgency")]
      public CollectionType HardcodeCollAgency
      {
        get => hardcodeCollAgency ??= new();
        set => hardcodeCollAgency = value;
      }

      /// <summary>
      /// A value of HardcodeDirPmtCt.
      /// </summary>
      [JsonPropertyName("hardcodeDirPmtCt")]
      public CollectionType HardcodeDirPmtCt
      {
        get => hardcodeDirPmtCt ??= new();
        set => hardcodeDirPmtCt = value;
      }

      /// <summary>
      /// A value of HardcodeDirPmtCru.
      /// </summary>
      [JsonPropertyName("hardcodeDirPmtCru")]
      public CollectionType HardcodeDirPmtCru
      {
        get => hardcodeDirPmtCru ??= new();
        set => hardcodeDirPmtCru = value;
      }

      /// <summary>
      /// A value of HardcodeCsenetUiTaxIntercpt.
      /// </summary>
      [JsonPropertyName("hardcodeCsenetUiTaxIntercpt")]
      public CollectionType HardcodeCsenetUiTaxIntercpt
      {
        get => hardcodeCsenetUiTaxIntercpt ??= new();
        set => hardcodeCsenetUiTaxIntercpt = value;
      }

      /// <summary>
      /// A value of HardcodeRecovery.
      /// </summary>
      [JsonPropertyName("hardcodeRecovery")]
      public ObligationType HardcodeRecovery
      {
        get => hardcodeRecovery ??= new();
        set => hardcodeRecovery = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgmt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgmt")]
      public ObligationType HardcodeSpArrearsJudgmt
      {
        get => hardcodeSpArrearsJudgmt ??= new();
        set => hardcodeSpArrearsJudgmt = value;
      }

      /// <summary>
      /// A value of HardcodeSpousal.
      /// </summary>
      [JsonPropertyName("hardcodeSpousal")]
      public ObligationType HardcodeSpousal
      {
        get => hardcodeSpousal ??= new();
        set => hardcodeSpousal = value;
      }

      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      private ObligationType hardcodeAccruing;
      private Program hardcodedAf;
      private Program hardcodedAfi;
      private Program hardcodedFci;
      private Program hardcodedNa;
      private Program hardcodedNai;
      private Program hardcodedNc;
      private Program hardcodedNf;
      private Program hardcodedMai;
      private Program hardcodedFc;
      private ObligationType hardcoded718B;
      private Obligation hardcodeSecondary;
      private DprProgram hardcodeCa;
      private DprProgram hardcodeNa;
      private DprProgram hardcodeTa;
      private DprProgram hardcodePa;
      private CashReceiptType hardcodeFdirPmt;
      private CashReceiptType hardcodeFcrtRec;
      private CollectionType hardcodeRegular;
      private CollectionType hardcodeIncomeWithholdin;
      private CollectionType hardcodeFeePmt;
      private CollectionType hardcodeDirPmtAp;
      private CollectionType hardcodeCollAgency;
      private CollectionType hardcodeDirPmtCt;
      private CollectionType hardcodeDirPmtCru;
      private CollectionType hardcodeCsenetUiTaxIntercpt;
      private ObligationType hardcodeRecovery;
      private ObligationType hardcodeSpArrearsJudgmt;
      private ObligationType hardcodeSpousal;
      private ObligationType hardcodeVoluntary;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public LegalAction Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of JsSecondObligation.
    /// </summary>
    [JsonPropertyName("jsSecondObligation")]
    public Common JsSecondObligation
    {
      get => jsSecondObligation ??= new();
      set => jsSecondObligation = value;
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
    /// A value of HardcodeKansas.
    /// </summary>
    [JsonPropertyName("hardcodeKansas")]
    public Fips HardcodeKansas
    {
      get => hardcodeKansas ??= new();
      set => hardcodeKansas = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of VerifyThisRun.
    /// </summary>
    [JsonPropertyName("verifyThisRun")]
    public Common VerifyThisRun
    {
      get => verifyThisRun ??= new();
      set => verifyThisRun = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LcollectedTotal.
    /// </summary>
    [JsonPropertyName("lcollectedTotal")]
    public Common LcollectedTotal
    {
      get => lcollectedTotal ??= new();
      set => lcollectedTotal = value;
    }

    /// <summary>
    /// A value of LcollArrearsTotal.
    /// </summary>
    [JsonPropertyName("lcollArrearsTotal")]
    public Common LcollArrearsTotal
    {
      get => lcollArrearsTotal ??= new();
      set => lcollArrearsTotal = value;
    }

    /// <summary>
    /// A value of LcollCurrTotal.
    /// </summary>
    [JsonPropertyName("lcollCurrTotal")]
    public Common LcollCurrTotal
    {
      get => lcollCurrTotal ??= new();
      set => lcollCurrTotal = value;
    }

    /// <summary>
    /// A value of LowedTotal.
    /// </summary>
    [JsonPropertyName("lowedTotal")]
    public Common LowedTotal
    {
      get => lowedTotal ??= new();
      set => lowedTotal = value;
    }

    /// <summary>
    /// A value of LowedArrearsTotal.
    /// </summary>
    [JsonPropertyName("lowedArrearsTotal")]
    public Common LowedArrearsTotal
    {
      get => lowedArrearsTotal ??= new();
      set => lowedArrearsTotal = value;
    }

    /// <summary>
    /// A value of LowedCurrTotal.
    /// </summary>
    [JsonPropertyName("lowedCurrTotal")]
    public Common LowedCurrTotal
    {
      get => lowedCurrTotal ??= new();
      set => lowedCurrTotal = value;
    }

    /// <summary>
    /// A value of LnumberOfOrdersInLocate.
    /// </summary>
    [JsonPropertyName("lnumberOfOrdersInLocate")]
    public Common LnumberOfOrdersInLocate
    {
      get => lnumberOfOrdersInLocate ??= new();
      set => lnumberOfOrdersInLocate = value;
    }

    /// <summary>
    /// A value of LnumberOfPayingOrders.
    /// </summary>
    [JsonPropertyName("lnumberOfPayingOrders")]
    public Common LnumberOfPayingOrders
    {
      get => lnumberOfPayingOrders ??= new();
      set => lnumberOfPayingOrders = value;
    }

    /// <summary>
    /// A value of LtotalOrdersReferred.
    /// </summary>
    [JsonPropertyName("ltotalOrdersReferred")]
    public Common LtotalOrdersReferred
    {
      get => ltotalOrdersReferred ??= new();
      set => ltotalOrdersReferred = value;
    }

    /// <summary>
    /// A value of CurrArrInd.
    /// </summary>
    [JsonPropertyName("currArrInd")]
    public Common CurrArrInd
    {
      get => currArrInd ??= new();
      set => currArrInd = value;
    }

    /// <summary>
    /// A value of ReferralType.
    /// </summary>
    [JsonPropertyName("referralType")]
    public TextWorkArea ReferralType
    {
      get => referralType ??= new();
      set => referralType = value;
    }

    /// <summary>
    /// A value of LcollCurrentState.
    /// </summary>
    [JsonPropertyName("lcollCurrentState")]
    public Common LcollCurrentState
    {
      get => lcollCurrentState ??= new();
      set => lcollCurrentState = value;
    }

    /// <summary>
    /// A value of LcollCurrentFamily.
    /// </summary>
    [JsonPropertyName("lcollCurrentFamily")]
    public Common LcollCurrentFamily
    {
      get => lcollCurrentFamily ??= new();
      set => lcollCurrentFamily = value;
    }

    /// <summary>
    /// A value of LcollCurrentIState.
    /// </summary>
    [JsonPropertyName("lcollCurrentIState")]
    public Common LcollCurrentIState
    {
      get => lcollCurrentIState ??= new();
      set => lcollCurrentIState = value;
    }

    /// <summary>
    /// A value of LcollCurrentIFamily.
    /// </summary>
    [JsonPropertyName("lcollCurrentIFamily")]
    public Common LcollCurrentIFamily
    {
      get => lcollCurrentIFamily ??= new();
      set => lcollCurrentIFamily = value;
    }

    /// <summary>
    /// A value of LcollArrearsState.
    /// </summary>
    [JsonPropertyName("lcollArrearsState")]
    public Common LcollArrearsState
    {
      get => lcollArrearsState ??= new();
      set => lcollArrearsState = value;
    }

    /// <summary>
    /// A value of LcollArrearsFamily.
    /// </summary>
    [JsonPropertyName("lcollArrearsFamily")]
    public Common LcollArrearsFamily
    {
      get => lcollArrearsFamily ??= new();
      set => lcollArrearsFamily = value;
    }

    /// <summary>
    /// A value of LcollArrearsIState.
    /// </summary>
    [JsonPropertyName("lcollArrearsIState")]
    public Common LcollArrearsIState
    {
      get => lcollArrearsIState ??= new();
      set => lcollArrearsIState = value;
    }

    /// <summary>
    /// A value of LcollArrearsIFamily.
    /// </summary>
    [JsonPropertyName("lcollArrearsIFamily")]
    public Common LcollArrearsIFamily
    {
      get => lcollArrearsIFamily ??= new();
      set => lcollArrearsIFamily = value;
    }

    /// <summary>
    /// A value of LowedCurrentState.
    /// </summary>
    [JsonPropertyName("lowedCurrentState")]
    public Common LowedCurrentState
    {
      get => lowedCurrentState ??= new();
      set => lowedCurrentState = value;
    }

    /// <summary>
    /// A value of LowedCurrentFamily.
    /// </summary>
    [JsonPropertyName("lowedCurrentFamily")]
    public Common LowedCurrentFamily
    {
      get => lowedCurrentFamily ??= new();
      set => lowedCurrentFamily = value;
    }

    /// <summary>
    /// A value of LowedCurrentIState.
    /// </summary>
    [JsonPropertyName("lowedCurrentIState")]
    public Common LowedCurrentIState
    {
      get => lowedCurrentIState ??= new();
      set => lowedCurrentIState = value;
    }

    /// <summary>
    /// A value of LowedCurrentIFamily.
    /// </summary>
    [JsonPropertyName("lowedCurrentIFamily")]
    public Common LowedCurrentIFamily
    {
      get => lowedCurrentIFamily ??= new();
      set => lowedCurrentIFamily = value;
    }

    /// <summary>
    /// A value of LowedArrearsState.
    /// </summary>
    [JsonPropertyName("lowedArrearsState")]
    public Common LowedArrearsState
    {
      get => lowedArrearsState ??= new();
      set => lowedArrearsState = value;
    }

    /// <summary>
    /// A value of LowedArrearsFamily.
    /// </summary>
    [JsonPropertyName("lowedArrearsFamily")]
    public Common LowedArrearsFamily
    {
      get => lowedArrearsFamily ??= new();
      set => lowedArrearsFamily = value;
    }

    /// <summary>
    /// A value of LowedArrearsIState.
    /// </summary>
    [JsonPropertyName("lowedArrearsIState")]
    public Common LowedArrearsIState
    {
      get => lowedArrearsIState ??= new();
      set => lowedArrearsIState = value;
    }

    /// <summary>
    /// A value of LowedArrearsIFamily.
    /// </summary>
    [JsonPropertyName("lowedArrearsIFamily")]
    public Common LowedArrearsIFamily
    {
      get => lowedArrearsIFamily ??= new();
      set => lowedArrearsIFamily = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public LegalAction Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// Gets a value of PgmsForSp.
    /// </summary>
    [JsonIgnore]
    public Array<PgmsForSpGroup> PgmsForSp => pgmsForSp ??= new(
      PgmsForSpGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmsForSp for json serialization.
    /// </summary>
    [JsonPropertyName("pgmsForSp")]
    [Computed]
    public IList<PgmsForSpGroup> PgmsForSp_Json
    {
      get => pgmsForSp;
      set => PgmsForSp.Assign(value);
    }

    /// <summary>
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    /// <summary>
    /// Gets a value of OfPgms.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmsGroup> OfPgms => ofPgms ??= new(OfPgmsGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgms for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgms")]
    [Computed]
    public IList<OfPgmsGroup> OfPgms_Json
    {
      get => ofPgms;
      set => OfPgms.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hardcoded.
    /// </summary>
    [JsonPropertyName("hardcoded")]
    public HardcodedGroup Hardcoded
    {
      get => hardcoded ?? (hardcoded = new());
      set => hardcoded = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of Abend.
    /// </summary>
    [JsonPropertyName("abend")]
    public Common Abend
    {
      get => abend ??= new();
      set => abend = value;
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
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public StatsVerifi CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DebtType.
    /// </summary>
    [JsonPropertyName("debtType")]
    public StatsVerifi DebtType
    {
      get => debtType ??= new();
      set => debtType = value;
    }

    /// <summary>
    /// A value of CountyParameter.
    /// </summary>
    [JsonPropertyName("countyParameter")]
    public Fips CountyParameter
    {
      get => countyParameter ??= new();
      set => countyParameter = value;
    }

    /// <summary>
    /// A value of TotalNonDebt.
    /// </summary>
    [JsonPropertyName("totalNonDebt")]
    public ObligationTransaction TotalNonDebt
    {
      get => totalNonDebt ??= new();
      set => totalNonDebt = value;
    }

    /// <summary>
    /// A value of Decided.
    /// </summary>
    [JsonPropertyName("decided")]
    public Collection Decided
    {
      get => decided ??= new();
      set => decided = value;
    }

    /// <summary>
    /// A value of ReportingMonthCollection.
    /// </summary>
    [JsonPropertyName("reportingMonthCollection")]
    public Common ReportingMonthCollection
    {
      get => reportingMonthCollection ??= new();
      set => reportingMonthCollection = value;
    }

    private LegalAction save;
    private Common jsSecondObligation;
    private DebtDetail debtDetail;
    private Fips hardcodeKansas;
    private DateWorkArea null1;
    private Common verifyThisRun;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ExitStateWorkArea exitStateWorkArea;
    private Collection collection;
    private LegalActionPerson legalActionPerson;
    private Common lcollectedTotal;
    private Common lcollArrearsTotal;
    private Common lcollCurrTotal;
    private Common lowedTotal;
    private Common lowedArrearsTotal;
    private Common lowedCurrTotal;
    private Common lnumberOfOrdersInLocate;
    private Common lnumberOfPayingOrders;
    private Common ltotalOrdersReferred;
    private Common currArrInd;
    private TextWorkArea referralType;
    private Common lcollCurrentState;
    private Common lcollCurrentFamily;
    private Common lcollCurrentIState;
    private Common lcollCurrentIFamily;
    private Common lcollArrearsState;
    private Common lcollArrearsFamily;
    private Common lcollArrearsIState;
    private Common lcollArrearsIFamily;
    private Common lowedCurrentState;
    private Common lowedCurrentFamily;
    private Common lowedCurrentIState;
    private Common lowedCurrentIFamily;
    private Common lowedArrearsState;
    private Common lowedArrearsFamily;
    private Common lowedArrearsIState;
    private Common lowedArrearsIFamily;
    private LegalAction restart;
    private Program program;
    private Tribunal tribunal;
    private Fips fips;
    private ProgramControlTotal programControlTotal;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private Array<PgmsForSpGroup> pgmsForSp;
    private Array<PgmHistGroup> pgmHist;
    private Array<OfPgmsGroup> ofPgms;
    private HardcodedGroup hardcoded;
    private Common processCountToCommit;
    private External passArea;
    private Common abend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common found;
    private StatsVerifi collectionType;
    private StatsVerifi debtType;
    private Fips countyParameter;
    private ObligationTransaction totalNonDebt;
    private Collection decided;
    private Common reportingMonthCollection;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LaDetFinancial.
    /// </summary>
    [JsonPropertyName("laDetFinancial")]
    public LegalActionDetail LaDetFinancial
    {
      get => laDetFinancial ??= new();
      set => laDetFinancial = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of StatsVerifi.
    /// </summary>
    [JsonPropertyName("statsVerifi")]
    public StatsVerifi StatsVerifi
    {
      get => statsVerifi ??= new();
      set => statsVerifi = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public LegalAction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public ObligationTransaction KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of NonDebt.
    /// </summary>
    [JsonPropertyName("nonDebt")]
    public ObligationTransaction NonDebt
    {
      get => nonDebt ??= new();
      set => nonDebt = value;
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

    private LegalActionPerson legalActionPerson;
    private LegalActionDetail laDetFinancial;
    private ObligationRln obligationRln;
    private AccrualInstructions accrualInstructions;
    private StatsVerifi statsVerifi;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CsePerson ap;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CaseRole absentParent;
    private LegalActionCaseRole legalActionCaseRole;
    private Collection collection;
    private DebtDetail debtDetail;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalAction other;
    private ObligationTransaction keyOnly;
    private CsePersonAccount supported;
    private CsePerson child;
    private CsePersonAccount csePersonAccount;
    private Program program;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction nonDebt;
    private CaseRole caseRole;
  }
#endregion
}
