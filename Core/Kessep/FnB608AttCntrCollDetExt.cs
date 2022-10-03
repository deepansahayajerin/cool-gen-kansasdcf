// Program: FN_B608_ATT_CNTR_COLL_DET_EXT, ID: 372956309, model: 746.
// Short name: SWEF608B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B608_ATT_CNTR_COLL_DET_EXT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB608AttCntrCollDetExt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B608_ATT_CNTR_COLL_DET_EXT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB608AttCntrCollDetExt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB608AttCntrCollDetExt.
  /// </summary>
  public FnB608AttCntrCollDetExt(IContext context, Import import, Export export):
    
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
    // 08-25-1999  SWSRKEH             Initial Development
    // 11-25-1999  SWSRMFB             Changed the program entirely
    // 04-05-2000  SWSRMFB             Removed read and edits of FIPS and 
    // TRIBUNAL.  These are not required for this report, but an edit for blank
    // judicial district (on tribunal table) were causing collections to be
    // bypassed.
    // NOTE: There is another report that is not currently in use, called 
    // SRRUN159 Attorney Contractor Collections by Judicial District.  This
    // report needs Tribunal, FIPS and judicial district.  It uses a file of
    // collections that this program will generate.  The code for all of this is
    // currently commented out (see notes above commented code).  But when we
    // return to SRRUN159, we must resolve the blank judicial district field on
    // the Tribunal table.
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    // : Set hardcode values.
    local.Hardcoded.HardcodeSpousal.SystemGeneratedIdentifier = 2;
    local.Hardcoded.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier = 17;
    local.Hardcoded.HardcodeVoluntary.SystemGeneratedIdentifier = 16;
    local.Hardcoded.HardcodeVoluntary.Classification = "V";
    local.Hardcoded.HardcodeRecovery.Classification = "R";

    // : Set hardcode collection types
    local.Hardcoded.HardcodeRegular.SequentialIdentifier = 1;
    local.Hardcoded.HardcodedUnemploymentOffset.SequentialIdentifier = 5;
    local.Hardcoded.HardcodeIncomeWithholdin.SequentialIdentifier = 6;
    local.Hardcoded.HardcodeFeePmt.SequentialIdentifier = 9;
    local.Hardcoded.HardcodeDirPmtAp.SequentialIdentifier = 14;
    local.Hardcoded.HardcodeCollAgency.SequentialIdentifier = 15;
    local.Hardcoded.HardcodeDirPmtCt.SequentialIdentifier = 20;
    local.Hardcoded.HardcodeDirPmtCru.SequentialIdentifier = 21;

    // : Set hardcode cash receipt types
    local.Hardcoded.HardcodeFcrtRec.SystemGeneratedIdentifier = 2;
    local.Hardcoded.HardcodeFdirPmt.SystemGeneratedIdentifier = 7;
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
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
    // **** OPEN EXTRACT DSN (DDNAME = EXTRT162) ****
    local.Send.Parm1 = "OF";
    local.Send.Parm2 = "";
    UseEabB608FileWriter2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the 608 output file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.External.FileInstruction = "OPEN";
    UseFnEabB608WriteError5();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the 608 error output file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Disabled this March 5, 2000.  This file is generated for SRRUN159, 
    // programs SWEFB606/7.  That report has been put on hold indefinately.
    // This code will be re-instated if and when that report is required.
    // - Maureen Brown
    // **** END OF REPORT DSN OPEN PROCESS ****
    // <=============================================>
    // <======= Beginning Main Program Logic ========>
    // <=============================================>
    // <=========== GET REPORTING DATES ========>
    // To facilitate testing, the following logic sets the local current date 
    // work area date to either:
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

    // <=========== end of calculating REPORTING DATES ========>
    // <=========== Set Report Params ========>
    local.Send.Parm1 = "GR";
    local.Send.Parm2 = "";

    // <=========== Main program logic ========>
    foreach(var item in ReadCollectionCsePersonDebtObligation())
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // : Reset the adjustment indicator to 'N' if the collection was created 
      // in the report month, and adjusted after the report month.  We want to
      // still report it as an actual collection.  The adjustment will show up
      // on the next month's report.
      MoveCollection(entities.Collection, local.Collection);

      if (AsChar(entities.Collection.AdjustedInd) == 'Y' && Lt
        (local.Eom.Date, entities.Collection.CollectionAdjustmentDt))
      {
        local.Collection.AdjustedInd = "N";
      }

      // <=== Check for number of records read - if = 500 do a      ===>
      // <=== commit to eliminate the -911/-904 resource contention ===>
      // <=== or resource not available error                  ========>
      ++local.RecCount.Count;

      if (local.RecCount.Count == 500)
      {
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.RecCount.Count = 1;
      }

      if (ReadObligationType())
      {
        if (AsChar(entities.ObligationType.Classification) == AsChar
          (local.Hardcoded.HardcodeRecovery.Classification))
        {
          continue;
        }

        if (AsChar(entities.ObligationType.SupportedPersonReqInd) != 'Y')
        {
          continue;
        }
      }
      else
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "Debt not found for collection id:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        UseFnEabB608WriteError4();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. obligation type " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // <=========== Initilize local extract attributes ========>
      local.ExtCase.Number = local.InitCase.Number;
      local.ExtCollection.Assign(local.InitCollection);
      local.ExtCollectionType.Code = local.InitCollectionType.Code;
      local.ExtLegalAction.Assign(local.InitLegalAction);
      local.ExtLegalReferralAssignment.EffectiveDate =
        local.InitLegalReferralAssignment.EffectiveDate;
      MoveServiceProvider2(local.InitServiceProvider, local.ExtServiceProvider);
      local.ExtKOrO.Text1 = local.InitKOrO.Text1;
      local.ExtProgGrp.Text1 = local.InitProgGrp.Text1;
      local.ExtTribunal.JudicialDistrict = local.InitTribunal.JudicialDistrict;
      local.ExtFips.CountyDescription = local.InitFips.CountyDescription;
      MoveOffice(local.InitOffice, local.Office);
      MoveCseOrganization(local.InitRegion, local.Region);

      // <=========== end of Initilizing local extract attributes ========>
      if (ReadCashReceiptDetailCollectionType())
      {
        if (entities.CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeCollAgency.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeDirPmtAp.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeDirPmtCru.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeDirPmtCt.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeFeePmt.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeIncomeWithholdin.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodeRegular.SequentialIdentifier || entities
          .CollectionType.SequentialIdentifier == local
          .Hardcoded.HardcodedUnemploymentOffset.SequentialIdentifier)
        {
          // added umemployement off set as part of the tribal project  11/2012
        }
        else
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      if (ReadCashReceiptType())
      {
        if (entities.CashReceiptType.SystemGeneratedIdentifier == local
          .Hardcoded.HardcodeFcrtRec.SystemGeneratedIdentifier || entities
          .CashReceiptType.SystemGeneratedIdentifier == local
          .Hardcoded.HardcodeFdirPmt.SystemGeneratedIdentifier)
        {
          if (ReadCashReceiptSourceType())
          {
            if (Equal(entities.CashReceiptSourceType.Code, "TRIBAL"))
            {
              goto Read;
            }
          }

          continue;
        }
      }
      else
      {
        continue;
      }

Read:

      local.ExtCollection.Assign(local.Collection);
      local.ExtCollectionType.Code = entities.CollectionType.Code;

      // <=========== Set Date Received ========>
      local.Rec2Write.Count = 1;

      if (AsChar(local.Collection.AdjustedInd) == 'Y')
      {
        local.ExtCollection.CollectionDt =
          local.Collection.CollectionAdjustmentDt;

        if (!Lt(local.Collection.CreatedTmst, local.Bom.Timestamp) && !
          Lt(local.Eom.Timestamp, local.Collection.CreatedTmst))
        {
          local.Rec2Write.Count = 2;
        }
      }
      else
      {
        local.ExtCollection.CollectionDt = Date(local.Collection.CreatedTmst);
      }

      if (!Equal(entities.Ap.Number, local.PreviousAp.Number))
      {
        local.PreviousAp.Number = entities.Ap.Number;
        local.ExtAp.Number = entities.Ap.Number;
        local.ExtAp.LastName = entities.CashReceiptDetail.ObligorLastName ?? Spaces
          (17);
        local.ExtAp.FirstName = entities.CashReceiptDetail.ObligorFirstName ?? Spaces
          (12);
        local.ExtAp.MiddleInitial =
          entities.CashReceiptDetail.ObligorMiddleName ?? Spaces(1);
        UseSiFormatCsePersonName();
      }

      // <=========== get Suported Person ========>
      if (!ReadCsePerson())
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "SP not found for collection id:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        UseFnEabB608WriteError4();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. supported person " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (!ReadDebtDetail())
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "Debt Dtl NF for collection ID:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        UseFnEabB608WriteError4();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. debt detail " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      UseFnDetermineCaseAndAr2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // : Want to keep calls to the adabas cab to a minimum.  If  AR has not 
        // changed,
        //   do not call the cab.
        if (!Equal(local.Ar.Number, local.ExtAr.Number))
        {
          local.ExtAr.Number = local.Ar.Number;
          UseSiReadCsePersonBatch();
        }
      }
      else
      {
        // : July, 2000, mfb - added totals for different kinds of bypasses.
        if (AsChar(local.Collection.AdjustedInd) == 'Y')
        {
          local.Common.TotalCurrency = -local.Collection.Amount;
        }
        else
        {
          local.Common.TotalCurrency = local.Collection.Amount;
        }

        switch(TrimEnd(local.Condition.Text8))
        {
          case "VOL NF":
            ++local.BypassTotals.VolNf.Count;
            local.BypassTotals.VolNf.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          case "SPOUS NF":
            ++local.BypassTotals.SpousNf.Count;
            local.BypassTotals.SpousNf.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          case "NO AR":
            ++local.BypassTotals.ObligeeNf.Count;
            local.BypassTotals.ObligeeNf.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          case "CS NF":
            ++local.BypassTotals.CsNf.Count;
            local.BypassTotals.CsNf.TotalCurrency += local.Common.TotalCurrency;

            break;
          default:
            break;
        }

        if (IsEmpty(local.NeededToWrite.RptDetail))
        {
          UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = local.ExitState.Message;
        }

        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.Case1.Number = local.ExtCase.Number;
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " for S.P.:" + entities
          .Supported1.Number + " with AP:" + local.ExtAp.Number;
        UseFnEabB608WriteError3();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. case and ap " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // <===== Get Attorney/Contractor Name that the case was referred ====>
      // : Use the collection date to find the contractor, rather than the 
      // create date.  This is because we may have retroactive collections, and
      // there could have been a different (or no) contractor at that time.
      local.ReadSearchDate.Date = local.Collection.CollectionDt;
      local.NeededToWrite.RptDetail = "Collection id: " + NumberToString
        (entities.Collection.SystemGeneratedIdentifier, 15);

      // <=========== Get Standard Court Order Number ========>
      if (ReadLegalAction())
      {
        local.ExtLegalAction.Assign(entities.LegalAction);
      }
      else
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "LA Std CO# NF for Coll ID:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + "Case:" + local
          .ExtCase.Number + "Obligor:" + local.ExtAp.Number;
        UseFnEabB608WriteError3();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. legal action nf " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        // : Add to total and count for this error.
        ++local.BypassTotals.LegalActionNf.Count;

        if (AsChar(local.Collection.AdjustedInd) == 'Y')
        {
          local.BypassTotals.LegalActionNf.TotalCurrency =
            -local.Collection.Amount;
        }
        else
        {
          local.BypassTotals.LegalActionNf.TotalCurrency =
            local.Collection.Amount;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      UseFnRetrieveLegalRefAndSp2();

      if (!IsEmpty(local.External.TextReturnCode) && !
        Equal(local.External.TextReturnCode, "OK"))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the 608 error output file. legal ref and sp " +
          local.External.TextReturnCode;
        UseCabErrorReport2();
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      if (!IsEmpty(local.NeededToWrite.RptDetail))
      {
        // : July, 2000, mfb - added totals for different kinds of bypasses.
        if (AsChar(local.Collection.AdjustedInd) == 'Y')
        {
          local.Common.TotalCurrency = -local.Collection.Amount;
        }
        else
        {
          local.Common.TotalCurrency = local.Collection.Amount;
        }

        switch(TrimEnd(local.Condition.Text8))
        {
          case "NO REF":
            ++local.BypassTotals.NoRef.Count;
            local.BypassTotals.NoRef.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          case "OSP EXP":
            ++local.BypassTotals.OspExp.Count;
            local.BypassTotals.OspExp.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          case "OSP NF":
            ++local.BypassTotals.OspNf.Count;
            local.BypassTotals.OspNf.TotalCurrency += local.Common.
              TotalCurrency;

            break;
          default:
            break;
        }

        // : Error message was written out in the cab.  Go to the next 
        // collection.
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (IsEmpty(local.ExtServiceProvider.UserId))
      {
        // : No Referral found - go to next collection.
        continue;
      }

      // : April 5, 2000, MFB - commented out this code.  These fields are no 
      // longer on the report.
      // <=========== GET COUNTY OF LEGAL ACTION ========>
      if (ReadTribunalFips())
      {
        local.ExtFips.CountyDescription = entities.Fips.CountyDescription;
        local.ExtTribunal.JudicialDistrict = entities.Tribunal.JudicialDistrict;
        local.Region.Name = entities.Fips.CountyDescription ?? Spaces(20);
      }
      else
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "Tribunal and/or FIPS NF for Coll ID:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + "Case:" + local
          .ExtCase.Number + "Obligor:" + local.ExtAp.Number;
        UseFnEabB608WriteError1();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. read fips " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (IsEmpty(local.ExtFips.CountyDescription))
      {
        // <===========Write error message to error report ========>
        local.External.FileInstruction = "WRITE";
        local.NeededToWrite.RptDetail = "FIPS NF for Coll ID:";
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + "Case:" + local
          .ExtCase.Number + "Obligor:" + local.ExtAp.Number;
        UseFnEabB608WriteError2();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the 608 error output file. county description " +
            local.External.TextReturnCode;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // <=== Determine Collection Program Grouping - TAF/NonTAF or State Only 
      // ====>
      // : Assign letters for sort order.
      //  Local ext prog grp text_1: TAF = A, NTAF = B, and STATE ONLY = C.
      //  Local ext k or o text_1 is K for Kansas, and O for interstate.
      switch(TrimEnd(entities.Collection.ProgramAppliedTo))
      {
        case "AF":
          local.ExtKOrO.Text1 = "K";
          local.ExtProgGrp.Text1 = "A";

          break;
        case "AFI":
          local.ExtKOrO.Text1 = "O";
          local.ExtProgGrp.Text1 = "A";

          break;
        case "FC":
          local.ExtKOrO.Text1 = "K";
          local.ExtProgGrp.Text1 = "A";

          break;
        case "FCI":
          local.ExtKOrO.Text1 = "O";
          local.ExtProgGrp.Text1 = "A";

          break;
        case "NA":
          local.ExtKOrO.Text1 = "K";
          local.ExtProgGrp.Text1 = "B";

          break;
        case "NAI":
          local.ExtKOrO.Text1 = "O";
          local.ExtProgGrp.Text1 = "B";

          break;
        case "NF":
          local.ExtKOrO.Text1 = "K";
          local.ExtProgGrp.Text1 = "C";

          break;
        case "NC":
          local.ExtKOrO.Text1 = "K";
          local.ExtProgGrp.Text1 = "C";

          break;
        default:
          // <===========Write error message to error report ========>
          local.External.FileInstruction = "WRITE";
          local.NeededToWrite.RptDetail = "Program Grouping error - coll ID :";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
            (entities.Collection.SystemGeneratedIdentifier, 15);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + "Case:" + local
            .ExtCase.Number + " with AR:" + local.ExtAr.Number;
          UseFnEabB608WriteError2();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            // *****************************************************************
            // * Write a line to the ERROR RPT.
            // 
            // *
            // *****************************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered writing the 608 error output file. Program group " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_4_BATCH";

            return;
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
      }

      // <====================================================>
      // <=========== Write extracts out to flat file ========>
      // <====================================================>
      // <========================================================>
      // <== If a adjusted collection and the adjusted and ==>
      // <== collection dates are within the reporting month ==>
      // <==  then create two records =========================>
      // <====================================================>
      UseEabB608FileWriter1();

      if (!IsEmpty(local.Return1.Parm1))
      {
        // *****************************************************************
        // * Write a line to the ERROR RPT.
        // 
        // *
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing to the Attorney/Contractor collection detail extract file." +
          local.Return1.Parm1;
        UseCabErrorReport2();
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      if (local.Rec2Write.Count > 1)
      {
        local.ExtCollection.AdjustedInd = "N";
        local.ExtCollection.CollectionDt =
          Date(entities.Collection.CreatedTmst);
        UseEabB608FileWriter1();

        if (!IsEmpty(local.Return1.Parm1))
        {
          // *****************************************************************
          // * Write a line to the ERROR RPT.
          // 
          // *
          // *****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing to the Attorney/Contractor collection detail extract file." +
            local.Return1.Parm1;
          UseCabErrorReport2();
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }

      // : Disabled this March 5, 2000.  This file is generated for SRRUN159, 
      // programs SWEFB606/7.  That report has been put on hold indefinately.
      // This code will be re-instated if and when that report is required.
      // - Maureen Brown
    }

    // <=============================================>
    // <========= End of Main Program Logic =========>
    // <=============================================>
    // *****************************************************************
    // * Write out bypass totals
    // *****************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport2();
    local.NeededToWrite.RptDetail =
      "******************   BYPASS TOTALS **********************";
    UseCabErrorReport2();
    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport2();
    local.NeededToWrite.RptDetail =
      "ERROR CONDITION        COLLECTION AMOUNT    NUMBER OF COLLECTIONS";
    UseCabErrorReport2();
    local.NeededToWrite.RptDetail =
      "---------------        -----------------    ---------------------";
    UseCabErrorReport2();

    // : Voluntary
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.VolNf.TotalCurrency * 100), 15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.VolNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "Voluntary Not Found      " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.VolNf.Count, 15);
    UseCabErrorReport2();

    // : Spousal
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.SpousNf.TotalCurrency * 100), 15);
      
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.SpousNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "Spousal Not Found        " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.SpousNf.Count, 15);
    UseCabErrorReport2();

    // : Child Support
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.CsNf.TotalCurrency * 100), 15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.CsNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "CS Not Found             " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.CsNf.Count, 15);
    UseCabErrorReport2();

    // : No Obligee Found
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.ObligeeNf.TotalCurrency * 100),
      15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.ObligeeNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "Obligee Not Found        " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.ObligeeNf.Count, 15);
    UseCabErrorReport2();

    // : No Legal Referral Found
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.NoRef.TotalCurrency * 100), 15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.NoRef.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "Leg Ref Not Found        " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.NoRef.Count, 15);
    UseCabErrorReport2();

    // : OSP Not Active
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.OspExp.TotalCurrency * 100), 15);
      
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.OspExp.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "OSP not active           " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.OspExp.Count, 15);
    UseCabErrorReport2();

    // : OSP Not Found
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.OspNf.TotalCurrency * 100), 15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.OspNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "OSP Not Found            " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.OspNf.Count, 15);
    UseCabErrorReport2();

    // : Legal Action Not Found
    local.TextAmount.Text15 =
      NumberToString((long)(local.BypassTotals.LegalActionNf.TotalCurrency * 100)
      , 15);
    local.TextAmount.Text15 =
      Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.TextAmount.Text15, WorkArea.Text15_MaxLength, 14, 2) + " ";
      

    if (local.BypassTotals.LegalActionNf.TotalCurrency < 0)
    {
      local.TextAmount.Text15 = "-" + Substring
        (local.TextAmount.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }

    local.NeededToWrite.RptDetail = "Legal Action Not Found   " + local
      .TextAmount.Text15 + "          " + NumberToString
      (local.BypassTotals.LegalActionNf.Count, 15);
    UseCabErrorReport2();

    // **** START OF REPORT DSN CLOSE PROCESS ****
    local.Send.Parm1 = "CF";
    local.Send.Parm2 = "";
    UseEabB608FileWriter2();

    if (!IsEmpty(local.Return1.Parm1))
    {
      // *****************************************************************
      // * Write a line to the ERROR RPT.
      // 
      // *
      // *****************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the 608 file writer.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Disabled this March 5, 2000.  This file is generated for SRRUN159, 
    // programs SWEFB606/7.  That report has been put on hold indefinately.
    // This code will be re-instated if and when that report is required.
    // - Maureen Brown
    // **** END OF REPORT DSN CLOSE PROCESS ****
    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.DisbursementProcessingNeedInd = source.DisbursementProcessingNeedInd;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private static void MoveServiceProvider1(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveServiceProvider2(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private void UseCabDetermineReportingDates()
  {
    var useImport = new CabDetermineReportingDates.Import();
    var useExport = new CabDetermineReportingDates.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabDetermineReportingDates.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Eom, local.Eom);
    MoveDateWorkArea(useExport.Bom, local.Bom);
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabB608FileWriter1()
  {
    var useImport = new EabB608FileWriter.Import();
    var useExport = new EabB608FileWriter.Export();

    useImport.ReportParms.Assign(local.Send);
    useImport.Bom.Date = local.Bom.Date;
    MoveServiceProvider1(local.ExtServiceProvider, useImport.ServiceProvider);
    useImport.Ko.Text1 = local.ExtKOrO.Text1;
    useImport.Collection.Assign(local.ExtCollection);
    useImport.ProgGrp.Text1 = local.ExtProgGrp.Text1;
    useImport.Ap.Assign(local.ExtAp);
    useImport.Ar.Assign(local.ExtAr);
    useImport.LegalAction.Assign(local.ExtLegalAction);
    useImport.Case1.Number = local.ExtCase.Number;
    useImport.CollectionType.Code = local.ExtCollectionType.Code;
    useImport.LegalReferralAssignment.EffectiveDate =
      local.ExtLegalReferralAssignment.EffectiveDate;
    useImport.Tribunal.JudicialDistrict = local.ExtTribunal.JudicialDistrict;
    useImport.Fips.CountyDescription = local.ExtFips.CountyDescription;
    useImport.Office.Name = local.Office.Name;
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB608FileWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabB608FileWriter2()
  {
    var useImport = new EabB608FileWriter.Import();
    var useExport = new EabB608FileWriter.Export();

    useImport.ReportParms.Assign(local.Send);
    MoveReportParms(local.Return1, useExport.ReportParms);

    Call(EabB608FileWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.Return1);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitState.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitState.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnDetermineCaseAndAr2()
  {
    var useImport = new FnDetermineCaseAndAr2.Import();
    var useExport = new FnDetermineCaseAndAr2.Export();

    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.PerSupported.Assign(entities.Supported1);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.Ap.Number;
    MoveDateWorkArea(local.Bom, useImport.Bom);
    MoveDateWorkArea(local.Eom, useImport.Eom);
    useImport.Hardcode.HardcodeVoluntary.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeSpousal.SystemGeneratedIdentifier;
    useImport.Hardcode.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier;

    Call(FnDetermineCaseAndAr2.Execute, useImport, useExport);

    entities.Supported1.Number = useImport.PerSupported.Number;
    local.NeededToWrite.RptDetail = useExport.EabReportSend.RptDetail;
    local.Ar.Number = useExport.Obligee.Number;
    local.ExtCase.Number = useExport.Case1.Number;
    local.Condition.Text8 = useExport.Condition.Text8;
  }

  private void UseFnEabB608WriteError1()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.Case1.Number = local.ExtCase.Number;
    useImport.ServiceProvider.Assign(local.ExtServiceProvider);
    MoveOffice(local.Office, useImport.Office);
    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useExport.External.Assign(local.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError2()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.Case1.Number = local.ExtCase.Number;
    useImport.ServiceProvider.Assign(local.ExtServiceProvider);
    MoveOffice(local.Office, useImport.Office);
    useImport.Region.Name = local.Region.Name;
    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useExport.External.Assign(local.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError3()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.Case1.Number = local.ExtCase.Number;
    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useExport.External.Assign(local.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError4()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useExport.External.Assign(local.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnEabB608WriteError5()
  {
    var useImport = new FnEabB608WriteError.Import();
    var useExport = new FnEabB608WriteError.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.External.Assign(local.External);

    Call(FnEabB608WriteError.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnRetrieveLegalRefAndSp2()
  {
    var useImport = new FnRetrieveLegalRefAndSp2.Import();
    var useExport = new FnRetrieveLegalRefAndSp2.Export();

    useImport.PersistentSupported.Assign(entities.Supported1);
    useImport.Ar.Number = local.Ar.Number;
    useImport.Case1.Number = local.ExtCase.Number;
    MoveDateWorkArea(local.ReadSearchDate, useImport.ReadSearchDate);
    useImport.IdentifierForErrMsg.RptDetail = local.NeededToWrite.RptDetail;
    useImport.Collection.CourtOrderAppliedTo =
      entities.Collection.CourtOrderAppliedTo;
    useImport.LegalAction.Assign(local.ExtLegalAction);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Hardcode.HardcodeVoluntary.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeSpousal.SystemGeneratedIdentifier;
    useImport.Hardcode.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier =
      local.Hardcoded.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier;

    Call(FnRetrieveLegalRefAndSp2.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
    local.Condition.Text8 = useExport.Condition.Text8;
    local.NeededToWrite.RptDetail = useExport.ErrorMessage.RptDetail;
    local.ExtLegalReferralAssignment.EffectiveDate =
      useExport.LegalReferralAssignment.EffectiveDate;
    local.ExtServiceProvider.Assign(useExport.ServiceProvider);
    MoveOffice(useExport.Office, local.Office);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.ExtAp);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.ExtAp.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ExtAr.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Ar.Number = useExport.CsePerson.Number;
    local.ExtAr.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetailCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 7);
        entities.CollectionType.Code = db.GetString(reader, 8);
        entities.CollectionType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtypeId",
          entities.CashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonDebtObligation()
  {
    entities.Ap.Populated = false;
    entities.Obligation.Populated = false;
    entities.Debt.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePersonDebtObligation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.Bom.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.Eom.Timestamp.GetValueOrDefault());
        db.SetDate(command, "date1", local.Bom.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.Eom.Date.GetValueOrDefault());
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
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Obligation.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.Collection.Amount = db.GetDecimal(reader, 19);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 20);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 21);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 22);
        entities.Ap.Number = db.GetString(reader, 23);
        entities.Debt.CspNumber = db.GetString(reader, 23);
        entities.Obligation.CspNumber = db.GetString(reader, 23);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 24);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 25);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 26);
        entities.Ap.Populated = true;
        entities.Obligation.Populated = true;
        entities.Debt.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Debt.OtyType);
        db.SetInt32(command, "obId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Debt.OtyType);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 1);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 4);
        entities.Fips.County = db.GetInt32(reader, 4);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 5);
        entities.Fips.State = db.GetInt32(reader, 5);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 6);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
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
    /// <summary>A HardcodedGroup group.</summary>
    [Serializable]
    public class HardcodedGroup
    {
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
      /// A value of HardcodeCsenetIrsTaxIntercpt.
      /// </summary>
      [JsonPropertyName("hardcodeCsenetIrsTaxIntercpt")]
      public CollectionType HardcodeCsenetIrsTaxIntercpt
      {
        get => hardcodeCsenetIrsTaxIntercpt ??= new();
        set => hardcodeCsenetIrsTaxIntercpt = value;
      }

      /// <summary>
      /// A value of HardcodeCsenetStTaxIntercpt.
      /// </summary>
      [JsonPropertyName("hardcodeCsenetStTaxIntercpt")]
      public CollectionType HardcodeCsenetStTaxIntercpt
      {
        get => hardcodeCsenetStTaxIntercpt ??= new();
        set => hardcodeCsenetStTaxIntercpt = value;
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

      /// <summary>
      /// A value of HardcodedUnemploymentOffset.
      /// </summary>
      [JsonPropertyName("hardcodedUnemploymentOffset")]
      public CollectionType HardcodedUnemploymentOffset
      {
        get => hardcodedUnemploymentOffset ??= new();
        set => hardcodedUnemploymentOffset = value;
      }

      private CashReceiptType hardcodeFdirPmt;
      private CashReceiptType hardcodeFcrtRec;
      private CollectionType hardcodeRegular;
      private CollectionType hardcodeIncomeWithholdin;
      private CollectionType hardcodeCsenetIrsTaxIntercpt;
      private CollectionType hardcodeCsenetStTaxIntercpt;
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
      private CollectionType hardcodedUnemploymentOffset;
    }

    /// <summary>A BypassTotalsGroup group.</summary>
    [Serializable]
    public class BypassTotalsGroup
    {
      /// <summary>
      /// A value of LegalActionNf.
      /// </summary>
      [JsonPropertyName("legalActionNf")]
      public Common LegalActionNf
      {
        get => legalActionNf ??= new();
        set => legalActionNf = value;
      }

      /// <summary>
      /// A value of SpousNf.
      /// </summary>
      [JsonPropertyName("spousNf")]
      public Common SpousNf
      {
        get => spousNf ??= new();
        set => spousNf = value;
      }

      /// <summary>
      /// A value of ObligeeNf.
      /// </summary>
      [JsonPropertyName("obligeeNf")]
      public Common ObligeeNf
      {
        get => obligeeNf ??= new();
        set => obligeeNf = value;
      }

      /// <summary>
      /// A value of CsNf.
      /// </summary>
      [JsonPropertyName("csNf")]
      public Common CsNf
      {
        get => csNf ??= new();
        set => csNf = value;
      }

      /// <summary>
      /// A value of VolNf.
      /// </summary>
      [JsonPropertyName("volNf")]
      public Common VolNf
      {
        get => volNf ??= new();
        set => volNf = value;
      }

      /// <summary>
      /// A value of OspNf.
      /// </summary>
      [JsonPropertyName("ospNf")]
      public Common OspNf
      {
        get => ospNf ??= new();
        set => ospNf = value;
      }

      /// <summary>
      /// A value of OspExp.
      /// </summary>
      [JsonPropertyName("ospExp")]
      public Common OspExp
      {
        get => ospExp ??= new();
        set => ospExp = value;
      }

      /// <summary>
      /// A value of NoRef.
      /// </summary>
      [JsonPropertyName("noRef")]
      public Common NoRef
      {
        get => noRef ??= new();
        set => noRef = value;
      }

      private Common legalActionNf;
      private Common spousNf;
      private Common obligeeNf;
      private Common csNf;
      private Common volNf;
      private Common ospNf;
      private Common ospExp;
      private Common noRef;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Region.
    /// </summary>
    [JsonPropertyName("region")]
    public CseOrganization Region
    {
      get => region ??= new();
      set => region = value;
    }

    /// <summary>
    /// A value of TextAmount.
    /// </summary>
    [JsonPropertyName("textAmount")]
    public WorkArea TextAmount
    {
      get => textAmount ??= new();
      set => textAmount = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public TextWorkArea Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of Condition.
    /// </summary>
    [JsonPropertyName("condition")]
    public TextWorkArea Condition
    {
      get => condition ??= new();
      set => condition = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of PreviousAp.
    /// </summary>
    [JsonPropertyName("previousAp")]
    public CsePerson PreviousAp
    {
      get => previousAp ??= new();
      set => previousAp = value;
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
    /// A value of LrefCrolesExist.
    /// </summary>
    [JsonPropertyName("lrefCrolesExist")]
    public Common LrefCrolesExist
    {
      get => lrefCrolesExist ??= new();
      set => lrefCrolesExist = value;
    }

    /// <summary>
    /// A value of ExitState.
    /// </summary>
    [JsonPropertyName("exitState")]
    public ExitStateWorkArea ExitState
    {
      get => exitState ??= new();
      set => exitState = value;
    }

    /// <summary>
    /// A value of ReadSearchDate.
    /// </summary>
    [JsonPropertyName("readSearchDate")]
    public DateWorkArea ReadSearchDate
    {
      get => readSearchDate ??= new();
      set => readSearchDate = value;
    }

    /// <summary>
    /// A value of RecCount.
    /// </summary>
    [JsonPropertyName("recCount")]
    public Common RecCount
    {
      get => recCount ??= new();
      set => recCount = value;
    }

    /// <summary>
    /// A value of Rec2Write.
    /// </summary>
    [JsonPropertyName("rec2Write")]
    public Common Rec2Write
    {
      get => rec2Write ??= new();
      set => rec2Write = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of InitServiceProvider.
    /// </summary>
    [JsonPropertyName("initServiceProvider")]
    public ServiceProvider InitServiceProvider
    {
      get => initServiceProvider ??= new();
      set => initServiceProvider = value;
    }

    /// <summary>
    /// A value of InitOffice.
    /// </summary>
    [JsonPropertyName("initOffice")]
    public Office InitOffice
    {
      get => initOffice ??= new();
      set => initOffice = value;
    }

    /// <summary>
    /// A value of InitRegion.
    /// </summary>
    [JsonPropertyName("initRegion")]
    public CseOrganization InitRegion
    {
      get => initRegion ??= new();
      set => initRegion = value;
    }

    /// <summary>
    /// A value of InitCollection.
    /// </summary>
    [JsonPropertyName("initCollection")]
    public Collection InitCollection
    {
      get => initCollection ??= new();
      set => initCollection = value;
    }

    /// <summary>
    /// A value of InitFips.
    /// </summary>
    [JsonPropertyName("initFips")]
    public Fips InitFips
    {
      get => initFips ??= new();
      set => initFips = value;
    }

    /// <summary>
    /// A value of InitTribunal.
    /// </summary>
    [JsonPropertyName("initTribunal")]
    public Tribunal InitTribunal
    {
      get => initTribunal ??= new();
      set => initTribunal = value;
    }

    /// <summary>
    /// A value of InitKOrO.
    /// </summary>
    [JsonPropertyName("initKOrO")]
    public TextWorkArea InitKOrO
    {
      get => initKOrO ??= new();
      set => initKOrO = value;
    }

    /// <summary>
    /// A value of InitProgGrp.
    /// </summary>
    [JsonPropertyName("initProgGrp")]
    public TextWorkArea InitProgGrp
    {
      get => initProgGrp ??= new();
      set => initProgGrp = value;
    }

    /// <summary>
    /// A value of InitLegalAction.
    /// </summary>
    [JsonPropertyName("initLegalAction")]
    public LegalAction InitLegalAction
    {
      get => initLegalAction ??= new();
      set => initLegalAction = value;
    }

    /// <summary>
    /// A value of InitCase.
    /// </summary>
    [JsonPropertyName("initCase")]
    public Case1 InitCase
    {
      get => initCase ??= new();
      set => initCase = value;
    }

    /// <summary>
    /// A value of InitCollectionType.
    /// </summary>
    [JsonPropertyName("initCollectionType")]
    public CollectionType InitCollectionType
    {
      get => initCollectionType ??= new();
      set => initCollectionType = value;
    }

    /// <summary>
    /// A value of InitLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("initLegalReferralAssignment")]
    public LegalReferralAssignment InitLegalReferralAssignment
    {
      get => initLegalReferralAssignment ??= new();
      set => initLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of ExtAp.
    /// </summary>
    [JsonPropertyName("extAp")]
    public CsePersonsWorkSet ExtAp
    {
      get => extAp ??= new();
      set => extAp = value;
    }

    /// <summary>
    /// A value of ExtAr.
    /// </summary>
    [JsonPropertyName("extAr")]
    public CsePersonsWorkSet ExtAr
    {
      get => extAr ??= new();
      set => extAr = value;
    }

    /// <summary>
    /// A value of ExtServiceProvider.
    /// </summary>
    [JsonPropertyName("extServiceProvider")]
    public ServiceProvider ExtServiceProvider
    {
      get => extServiceProvider ??= new();
      set => extServiceProvider = value;
    }

    /// <summary>
    /// A value of ExtCollection.
    /// </summary>
    [JsonPropertyName("extCollection")]
    public Collection ExtCollection
    {
      get => extCollection ??= new();
      set => extCollection = value;
    }

    /// <summary>
    /// A value of ExtLegalAction.
    /// </summary>
    [JsonPropertyName("extLegalAction")]
    public LegalAction ExtLegalAction
    {
      get => extLegalAction ??= new();
      set => extLegalAction = value;
    }

    /// <summary>
    /// A value of ExtCase.
    /// </summary>
    [JsonPropertyName("extCase")]
    public Case1 ExtCase
    {
      get => extCase ??= new();
      set => extCase = value;
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
    /// A value of ExtTribunal.
    /// </summary>
    [JsonPropertyName("extTribunal")]
    public Tribunal ExtTribunal
    {
      get => extTribunal ??= new();
      set => extTribunal = value;
    }

    /// <summary>
    /// A value of ExtFips.
    /// </summary>
    [JsonPropertyName("extFips")]
    public Fips ExtFips
    {
      get => extFips ??= new();
      set => extFips = value;
    }

    /// <summary>
    /// A value of ExtKOrO.
    /// </summary>
    [JsonPropertyName("extKOrO")]
    public TextWorkArea ExtKOrO
    {
      get => extKOrO ??= new();
      set => extKOrO = value;
    }

    /// <summary>
    /// A value of ExtProgGrp.
    /// </summary>
    [JsonPropertyName("extProgGrp")]
    public TextWorkArea ExtProgGrp
    {
      get => extProgGrp ??= new();
      set => extProgGrp = value;
    }

    /// <summary>
    /// A value of ExtCollectionType.
    /// </summary>
    [JsonPropertyName("extCollectionType")]
    public CollectionType ExtCollectionType
    {
      get => extCollectionType ??= new();
      set => extCollectionType = value;
    }

    /// <summary>
    /// A value of ExtLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("extLegalReferralAssignment")]
    public LegalReferralAssignment ExtLegalReferralAssignment
    {
      get => extLegalReferralAssignment ??= new();
      set => extLegalReferralAssignment = value;
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
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public ReportParms Send
    {
      get => send ??= new();
      set => send = value;
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
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public ReportParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of Ext606ProgGrp.
    /// </summary>
    [JsonPropertyName("ext606ProgGrp")]
    public TextWorkArea Ext606ProgGrp
    {
      get => ext606ProgGrp ??= new();
      set => ext606ProgGrp = value;
    }

    /// <summary>
    /// A value of Ext606ArrearsOrCurrent.
    /// </summary>
    [JsonPropertyName("ext606ArrearsOrCurrent")]
    public Common Ext606ArrearsOrCurrent
    {
      get => ext606ArrearsOrCurrent ??= new();
      set => ext606ArrearsOrCurrent = value;
    }

    /// <summary>
    /// A value of Ext606CollOrDebt.
    /// </summary>
    [JsonPropertyName("ext606CollOrDebt")]
    public Common Ext606CollOrDebt
    {
      get => ext606CollOrDebt ??= new();
      set => ext606CollOrDebt = value;
    }

    /// <summary>
    /// A value of Ext606CollOrDebtType.
    /// </summary>
    [JsonPropertyName("ext606CollOrDebtType")]
    public Common Ext606CollOrDebtType
    {
      get => ext606CollOrDebtType ??= new();
      set => ext606CollOrDebtType = value;
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
    /// Gets a value of BypassTotals.
    /// </summary>
    [JsonPropertyName("bypassTotals")]
    public BypassTotalsGroup BypassTotals
    {
      get => bypassTotals ?? (bypassTotals = new());
      set => bypassTotals = value;
    }

    private External external;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private Office office;
    private CseOrganization region;
    private WorkArea textAmount;
    private TextWorkArea amount;
    private TextWorkArea condition;
    private CsePerson ar;
    private CsePerson previousAp;
    private Collection collection;
    private Common lrefCrolesExist;
    private ExitStateWorkArea exitState;
    private DateWorkArea readSearchDate;
    private Common recCount;
    private Common rec2Write;
    private Common errorFound;
    private Common common;
    private ServiceProvider initServiceProvider;
    private Office initOffice;
    private CseOrganization initRegion;
    private Collection initCollection;
    private Fips initFips;
    private Tribunal initTribunal;
    private TextWorkArea initKOrO;
    private TextWorkArea initProgGrp;
    private LegalAction initLegalAction;
    private Case1 initCase;
    private CollectionType initCollectionType;
    private LegalReferralAssignment initLegalReferralAssignment;
    private CsePersonsWorkSet extAp;
    private CsePersonsWorkSet extAr;
    private ServiceProvider extServiceProvider;
    private Collection extCollection;
    private LegalAction extLegalAction;
    private Case1 extCase;
    private Program program;
    private Tribunal extTribunal;
    private Fips extFips;
    private TextWorkArea extKOrO;
    private TextWorkArea extProgGrp;
    private CollectionType extCollectionType;
    private LegalReferralAssignment extLegalReferralAssignment;
    private ProgramControlTotal programControlTotal;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private ReportParms send;
    private EabReportSend neededToWrite;
    private ReportParms return1;
    private TextWorkArea ext606ProgGrp;
    private Common ext606ArrearsOrCurrent;
    private Common ext606CollOrDebt;
    private Common ext606CollOrDebtType;
    private HardcodedGroup hardcoded;
    private BypassTotalsGroup bypassTotals;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CsePerson ap;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private CsePersonAccount obligor;
    private Tribunal tribunal;
    private Fips fips;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
  }
#endregion
}
